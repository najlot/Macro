using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using MacroStudio.Core.Completion;
using MacroStudio.Execution;
using Microsoft.CodeAnalysis;
using Microsoft.Xaml.Behaviors;
using System.Collections.Immutable;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace MacroStudio.Behaviors;

public sealed class TextEditorCompletionBehavior : Behavior<TextEditor>
{
	private CompletionWindow? _completionWindow;
	private CSharpCompletionService? _completionService;
	private CancellationTokenSource? _cts;

	public static readonly DependencyProperty IsEnabledProperty =
		DependencyProperty.Register(nameof(IsEnabled), typeof(bool), typeof(TextEditorCompletionBehavior), new PropertyMetadata(true));

	public bool IsEnabled
	{
		get => (bool)GetValue(IsEnabledProperty);
		set => SetValue(IsEnabledProperty, value);
	}

	protected override void OnAttached()
	{
		base.OnAttached();

		if (AssociatedObject is null)
		{
			return;
		}

		AssociatedObject.TextArea.TextEntered += TextAreaOnTextEntered;
		AssociatedObject.TextArea.TextEntering += TextAreaOnTextEntering;
		AssociatedObject.TextArea.PreviewKeyDown += TextAreaOnPreviewKeyDown;
	}

	protected override void OnDetaching()
	{
		base.OnDetaching();

		if (AssociatedObject is null)
		{
			return;
		}

		AssociatedObject.TextArea.TextEntered -= TextAreaOnTextEntered;
		AssociatedObject.TextArea.TextEntering -= TextAreaOnTextEntering;
		AssociatedObject.TextArea.PreviewKeyDown -= TextAreaOnPreviewKeyDown;

		_cts?.Cancel();
		_cts?.Dispose();
		_cts = null;
	}

	private void EnsureCompletionService()
	{
		if (_completionService is not null)
		{
			return;
		}

		var references = ExecutionUtils.References
			.Select(a => (MetadataReference)MetadataReference.CreateFromFile(a.Location))
			.ToArray();

		_completionService = new CSharpCompletionService(ExecutionUtils.Imports, references);
	}

	private void TextAreaOnPreviewKeyDown(object sender, KeyEventArgs e)
	{
		if (!IsEnabled || AssociatedObject is null)
		{
			return;
		}

		if (e.Key == Key.Space && Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
		{
			e.Handled = true;
			_ = ShowCompletionAsync();
		}
	}

	private void TextAreaOnTextEntered(object sender, TextCompositionEventArgs e)
	{
		if (!IsEnabled)
		{
			return;
		}

		if (e.Text == "." || (e.Text.Length == 1 && char.IsLetterOrDigit(e.Text[0])) || e.Text == "_" || e.Text == " ")
		{
			_ = ShowCompletionAsync();
		}
	}

	private void TextAreaOnTextEntering(object sender, TextCompositionEventArgs e)
	{
		if (_completionWindow is null)
		{
			return;
		}

		if (e.Text.Length > 0 && !char.IsLetterOrDigit(e.Text[0]) && e.Text[0] != '_')
		{
			_completionWindow.CompletionList.RequestInsertion(e);
		}
	}

	private async Task ShowCompletionAsync()
	{
		if (!IsEnabled || AssociatedObject?.Document is null)
		{
			return;
		}

		EnsureCompletionService();
		if (_completionService is null)
		{
			return;
		}

		var triggerOffset = AssociatedObject.CaretOffset;
		var wordStartOffset = GetWordStartOffset(AssociatedObject.Document, triggerOffset);

		_cts?.Cancel();
		_cts?.Dispose();
		_cts = new CancellationTokenSource();
		var ct = _cts.Token;

		// Small debounce to avoid firing on every keystroke burst.
		try
		{
			await Task.Delay(150, ct).ConfigureAwait(true);
		}
		catch (OperationCanceledException)
		{
			return;
		}

		var code = AssociatedObject.Document.Text;
		var offset = AssociatedObject.CaretOffset;

		var items = await _completionService.GetCompletionItemsAsync(code, offset, ct).ConfigureAwait(true);
		if (ct.IsCancellationRequested || items.Count == 0)
		{
			return;
		}

        _completionWindow?.Close();
        _completionWindow = new CompletionWindow(AssociatedObject.TextArea)
        {
            StartOffset = wordStartOffset,
            EndOffset = triggerOffset,
			Foreground = new SolidColorBrush(Colors.WhiteSmoke),
			Background = new SolidColorBrush(Color.FromRgb(61, 61, 76))
        };

        var word = AssociatedObject.Document.GetText(wordStartOffset, triggerOffset - wordStartOffset);
        var data = _completionWindow.CompletionList.CompletionData;

		var stringItems = items
			.Select(item => item.DisplayText)
            .Where(s => s.Contains(word, StringComparison.OrdinalIgnoreCase))
            .Distinct()
            .ToArray()
			.OrderBy(item => !item.StartsWith(word, StringComparison.OrdinalIgnoreCase));

        foreach (var item in stringItems)
		{
            data.Add(new RoslynCompletionData(item));
        }

		_completionWindow.Closed += (_, _) => _completionWindow = null;
		_completionWindow.Show();
	}

	private static int GetWordStartOffset(ICSharpCode.AvalonEdit.Document.TextDocument document, int caretOffset)
	{
		var offset = Math.Clamp(caretOffset, 0, document.TextLength);
		while (offset > 0)
		{
			var c = document.GetCharAt(offset - 1);
			if (!char.IsLetterOrDigit(c) && c != '_')
			{
				break;
			}

			offset--;
		}

		return offset;
	}

	private sealed class RoslynCompletionData(string text) : ICompletionData
	{
        public System.Windows.Media.ImageSource? Image => null;
        public string Text { get; } = text;
        public object Content => Text;
		public object? Description => null;
		public double Priority => 0;

		public void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
		{
			textArea.Document.Replace(completionSegment, Text);
		}
	}
}
