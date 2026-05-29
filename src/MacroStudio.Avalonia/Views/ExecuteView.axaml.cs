using System.Collections.Immutable;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Input.TextInput;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using AvaloniaEdit;
using AvaloniaEdit.CodeCompletion;
using AvaloniaEdit.Editing;
using AvaloniaEdit.Highlighting;
using MacroStudio.App.ViewModels;
using MacroStudio.Core.Completion;
using Microsoft.CodeAnalysis;

#if WINDOWS
using MacroStudio.Backend.Windows.Execution;
#else
using MacroStudio.Backend.Linux.Execution;
#endif

namespace MacroStudio.Avalonia.Views;

public partial class ExecuteView : UserControl
{
	private CompletionWindow? _completionWindow;
	private CSharpCompletionService? _completionService;
	private CancellationTokenSource? _cts;
	private readonly TextEditor _codeEditor;
	private ExecuteViewModel? _viewModel;
	private bool _isSynchronizingText;

	public ExecuteView()
	{
		InitializeComponent();
		_codeEditor = this.FindControl<TextEditor>(nameof(CodeEditor))
			?? throw new InvalidOperationException("ExecuteView is missing the code editor control.");

		TextEditorUtils.ApplyDarkTheme(_codeEditor);
		_codeEditor.TextChanged += CodeEditorOnTextChanged;
		_codeEditor.TextArea.TextEntered += TextAreaOnTextEntered;
		_codeEditor.TextArea.TextEntering += TextAreaOnTextEntering;
		_codeEditor.TextArea.KeyDown += TextAreaOnKeyDown;
		DataContextChanged += OnDataContextChanged;
		DetachedFromVisualTree += (_, _) => CleanupEditor();

		BindViewModel(DataContext as ExecuteViewModel);
	}

	private void InitializeComponent()
	{
		AvaloniaXamlLoader.Load(this);
	}

	private void CleanupEditor()
	{
		DataContextChanged -= OnDataContextChanged;
		_codeEditor.TextChanged -= CodeEditorOnTextChanged;
		_codeEditor.TextArea.TextEntered -= TextAreaOnTextEntered;
		_codeEditor.TextArea.TextEntering -= TextAreaOnTextEntering;
		_codeEditor.TextArea.KeyDown -= TextAreaOnKeyDown;
		DetachViewModel();

		_cts?.Cancel();
		_cts?.Dispose();
		_cts = null;
		_completionWindow?.Close();
		_completionWindow = null;
	}

	private void OnDataContextChanged(object? sender, EventArgs e)
	{
		BindViewModel(DataContext as ExecuteViewModel);
	}

	private void BindViewModel(ExecuteViewModel? viewModel)
	{
		if (ReferenceEquals(_viewModel, viewModel))
		{
			return;
		}

		DetachViewModel();
		_viewModel = viewModel;

		if (_viewModel is null)
		{
			return;
		}

		_viewModel.PropertyChanged += ViewModelOnPropertyChanged;
		SyncEditorText(_viewModel.Code);
	}

	private void DetachViewModel()
	{
		if (_viewModel is null)
		{
			return;
		}

		_viewModel.PropertyChanged -= ViewModelOnPropertyChanged;
		_viewModel = null;
	}

	private void ViewModelOnPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
	{
		if (e.PropertyName == nameof(ExecuteViewModel.Code) && _viewModel is not null)
		{
			SyncEditorText(_viewModel.Code);
		}
	}

	private void CodeEditorOnTextChanged(object? sender, EventArgs e)
	{
		if (_isSynchronizingText || _viewModel is null)
		{
			return;
		}

		var document = _codeEditor.Document;
		if (document is null)
		{
			return;
		}

		_viewModel.Code = document.Text;
	}

	private void SyncEditorText(string text)
	{
		var document = _codeEditor.Document;
		if (document is null || document.Text == text)
		{
			return;
		}

		_isSynchronizingText = true;
		var caretOffset = _codeEditor.CaretOffset;
		document.Text = text ?? string.Empty;

		try
		{
			_codeEditor.CaretOffset = Math.Min(caretOffset, document.TextLength);
		}
		catch
		{
		}
		finally
		{
			_isSynchronizingText = false;
		}
	}

	private void EnsureCompletionService()
	{
		if (_completionService is not null)
		{
			return;
		}

		var references = GetCompletionReferences()
			.Select(assembly => (MetadataReference)MetadataReference.CreateFromFile(assembly.Location))
			.ToArray();

		_completionService = new CSharpCompletionService(GetCompletionImports(), references);
	}

	private void TextAreaOnKeyDown(object? sender, KeyEventArgs e)
	{
		if (e.Key == Key.Space && e.KeyModifiers.HasFlag(KeyModifiers.Control))
		{
			e.Handled = true;
			_ = ShowCompletionAsync();
		}
	}

	private void TextAreaOnTextEntered(object? sender, TextInputEventArgs e)
	{
		var inputText = e.Text ?? string.Empty;
		if (inputText == "." || (inputText.Length == 1 && char.IsLetterOrDigit(inputText[0])) || inputText == "_" || inputText == " ")
		{
			_ = ShowCompletionAsync();
		}
	}

	private void TextAreaOnTextEntering(object? sender, TextInputEventArgs e)
	{
		var inputText = e.Text ?? string.Empty;
		var completionWindow = _completionWindow;
		if (completionWindow is null)
		{
			return;
		}

		if (inputText.Length > 0 && !char.IsLetterOrDigit(inputText[0]) && inputText[0] != '_')
		{
			completionWindow.CompletionList.RequestInsertion(e);
		}
	}

	private async Task ShowCompletionAsync()
	{
		var document = _codeEditor.Document;
		if (document is null)
		{
			return;
		}

		EnsureCompletionService();
		if (_completionService is null)
		{
			return;
		}

		var triggerOffset = _codeEditor.CaretOffset;
		var wordStartOffset = GetWordStartOffset(document, triggerOffset);

		_cts?.Cancel();
		_cts?.Dispose();
		_cts = new CancellationTokenSource();
		var cancellationToken = _cts.Token;

		try
		{
			await Task.Delay(150, cancellationToken);
		}
		catch (OperationCanceledException)
		{
			return;
		}

		var code = document.Text;
		var caretOffset = _codeEditor.CaretOffset;
		var items = await _completionService.GetCompletionItemsAsync(code, caretOffset, cancellationToken);
		if (cancellationToken.IsCancellationRequested || items.Count == 0)
		{
			return;
		}

		_completionWindow?.Close();
		_completionWindow = new CompletionWindow(_codeEditor.TextArea)
		{
			StartOffset = wordStartOffset,
			EndOffset = triggerOffset
		};

		var word = document.GetText(wordStartOffset, triggerOffset - wordStartOffset);
		var filteredItems = items
			.Select(item => item.DisplayText)
			.Where(text => text.Contains(word, StringComparison.OrdinalIgnoreCase))
			.Distinct()
			.OrderBy(text => !text.StartsWith(word, StringComparison.OrdinalIgnoreCase))
			.ToArray();

		if (filteredItems.Length == 0)
		{
			_completionWindow.Close();
			_completionWindow = null;
			return;
		}

		foreach (var item in filteredItems)
		{
			_completionWindow.CompletionList.CompletionData.Add(new RoslynCompletionData(item));
		}

		_completionWindow.Closed += CompletionWindowOnClosed;
		_completionWindow.Show();
	}

	private void CompletionWindowOnClosed(object? sender, EventArgs e)
	{
		if (_completionWindow is not null)
		{
			_completionWindow.Closed -= CompletionWindowOnClosed;
		}

		_completionWindow = null;
	}

	private static int GetWordStartOffset(AvaloniaEdit.Document.TextDocument document, int caretOffset)
	{
		var offset = Math.Clamp(caretOffset, 0, document.TextLength);
		while (offset > 0)
		{
			var character = document.GetCharAt(offset - 1);
			if (!char.IsLetterOrDigit(character) && character != '_')
			{
				break;
			}

			offset--;
		}

		return offset;
	}

	private static ImmutableArray<System.Reflection.Assembly> GetCompletionReferences()
	{
#if WINDOWS
		return
		[
			typeof(object).Assembly,
			typeof(System.Drawing.Rectangle).Assembly,
			typeof(FileInfo).Assembly,
			typeof(IQueryable).Assembly,
			typeof(System.Dynamic.DynamicObject).Assembly,
			typeof(System.Text.RegularExpressions.Regex).Assembly,
			typeof(System.Diagnostics.Process).Assembly
		];
#else
		return
		[
			typeof(object).Assembly,
			typeof(FileInfo).Assembly,
			typeof(IQueryable).Assembly,
			typeof(System.Dynamic.DynamicObject).Assembly,
			typeof(System.Text.RegularExpressions.Regex).Assembly,
			typeof(System.Diagnostics.Process).Assembly,
				typeof(global::Avalonia.Media.Imaging.Bitmap).Assembly,
			typeof(Rectangle).Assembly
		];
#endif
	}

	private static string[] GetCompletionImports()
	{
#if WINDOWS
		return
		[
			"System",
			"System.IO",
			"System.Linq",
			"System.Text",
			"System.Drawing",
			"System.Dynamic",
			"System.Diagnostics",
			"System.Collections.Generic",
			"System.Text.RegularExpressions"
		];
#else
		return
		[
			"System",
			"System.IO",
			"System.Linq",
			"System.Text",
			"System.Dynamic",
			"System.Diagnostics",
			"System.Collections.Generic",
			"System.Text.RegularExpressions",
			"Avalonia.Media.Imaging",
			"MacroStudio.Backend.Linux.Execution"
		];
#endif
	}

	private sealed class RoslynCompletionData(string text) : ICompletionData
	{
		public IImage? Image => null;
		public string Text { get; } = text;
		public object Content => Text;
		public object? Description => null;
		public double Priority => 0;

		public void Complete(TextArea textArea, AvaloniaEdit.Document.ISegment completionSegment, EventArgs insertionRequestEventArgs)
		{
			textArea.Document.Replace(completionSegment, Text);
		}
	}
}