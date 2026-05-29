using System.Collections.ObjectModel;
using System.Windows.Input;
using MacroStudio.App.Models;
using MacroStudio.App.Services;
using MacroStudio.MVVM;
using MacroStudio.MVVM.ViewModel;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Scripting;

namespace MacroStudio.App.ViewModels;

public class ExecuteViewModel : AbstractViewModel, ITabItem
{
	private const string EmptyCodeStatusMessage = "Enter macro code to run it.";
	private const string ReadyStatusMessage = "Ready to run the current macro.";

	private static readonly FileDialogFilter MacroFilter = new("Macros", "*.macro");
	private static readonly FileDialogFilter ImageFilter = new("Images", "*.bmp", "*.png", "*.jpg", "*.jpeg", "*.webp");

	private readonly IMacroFileService _macroFileService;
	private readonly IFileDialogService _fileDialogService;
	private readonly IUserNotificationService _notificationService;
	private readonly IMainWindowService _mainWindowService;
	private readonly IMacroExecutionService _macroExecutionService;
	private readonly AsyncCommand _runCommand;
	private readonly AsyncCommand _saveCommand;
	private readonly AsyncCommand _loadCommand;
	private readonly AsyncCommand _addResourceCommand;

	public string Title => "Execute";

	public bool IsSelected
	{
		get => field;
		set
		{
			if (Set(ref field, value) && value && _macroExecutionService.IsSupported)
			{
				_macroExecutionService.Initialize();
			}
		}
	}

	public string Code
	{
		get => field;
		set
		{
			if (Set(ref field, value) && IsRunButtonEnabled && IsRunSupported)
			{
				RunStatusMessage = GetReadyStatusMessage();
			}
		}
	} = string.Empty;

	public int Executions
	{
		get => field;
		set => Set(ref field, value <= 0 ? 1 : value);
	} = 1;

	public bool IsRunButtonEnabled
	{
		get => field;
		set
		{
			if (Set(ref field, value))
			{
				_runCommand.RaiseCanExecuteChanged();
				RaisePropertyChanged(nameof(CanRun));
				RaisePropertyChanged(nameof(RunButtonText));
			}
		}
	} = true;

	public bool IsRunSupported => _macroExecutionService.IsSupported;
	public bool CanRun => IsRunSupported && IsRunButtonEnabled;
	public string RunButtonText => IsRunButtonEnabled ? "Run" : "Running...";
	public string RunSupportMessage => _macroExecutionService.UnsupportedReason;

	public string RunStatusMessage
	{
		get => field;
		private set => Set(ref field, value);
	} = EmptyCodeStatusMessage;

	public ObservableCollection<ResourceViewModel> Resources { get; } = [];

	public ICommand RunCommand => _runCommand;
	public ICommand SaveCommand => _saveCommand;
	public ICommand LoadCommand => _loadCommand;
	public ICommand AddResourceCommand => _addResourceCommand;
	public ICommand RemoveResourceCommand { get; }

	public ExecuteViewModel(
		IMacroFileService macroFileService,
		IFileDialogService fileDialogService,
		IUserNotificationService notificationService,
		IMainWindowService mainWindowService,
		IMacroExecutionService macroExecutionService)
	{
		_macroFileService = macroFileService;
		_fileDialogService = fileDialogService;
		_notificationService = notificationService;
		_mainWindowService = mainWindowService;
		_macroExecutionService = macroExecutionService;

		_runCommand = new AsyncCommand(RunAsync, task => ShowErrorAsync(task.Exception), () => IsRunButtonEnabled && _macroExecutionService.IsSupported);
		_saveCommand = new AsyncCommand(SaveAsync, task => ShowErrorAsync(task.Exception));
		_loadCommand = new AsyncCommand(LoadAsync, task => ShowErrorAsync(task.Exception));
		_addResourceCommand = new AsyncCommand(AddResourceAsync, task => ShowErrorAsync(task.Exception));
		RemoveResourceCommand = new RelayCommand<ResourceViewModel>(RemoveResource);

		if (!_macroExecutionService.IsSupported)
		{
			RunStatusMessage = _macroExecutionService.UnsupportedReason;
			return;
		}

		RunStatusMessage = GetReadyStatusMessage();
	}

	private Task ShowErrorAsync(Exception? ex)
	{
		return _notificationService.ShowErrorAsync(ex?.ToString() ?? "Unknown error.");
	}

	private async Task AddResourceAsync()
	{
		var filePath = await _fileDialogService.OpenFileAsync("Add Resource", ImageFilter);
		if (string.IsNullOrWhiteSpace(filePath))
		{
			return;
		}

		var name = Path.GetFileNameWithoutExtension(filePath);
		var value = await File.ReadAllBytesAsync(filePath);

		var newName = name;
		var count = 1;
		while (Resources.Any(resource => resource.Name.Equals(newName, StringComparison.OrdinalIgnoreCase)))
		{
			count++;
			newName = $"{name} {count}";
		}

		Resources.Add(new ResourceViewModel
		{
			Name = newName,
			Value = value
		});
	}

	private void RemoveResource(ResourceViewModel resource)
	{
		Resources.Remove(resource);
	}

	private async Task SaveAsync()
	{
		var filePath = await _fileDialogService.SaveFileAsync("Save Macro", "new.macro", MacroFilter);
		if (string.IsNullOrWhiteSpace(filePath))
		{
			return;
		}

		var document = new MacroDocument
		{
			Code = Code,
			Executions = Executions,
			Resources = new Collection<MacroResource>(Resources.Select(resource => resource.ToModel()).ToList())
		};

		await _macroFileService.SaveAsync(filePath, document);
	}

	private async Task LoadAsync()
	{
		var filePath = await _fileDialogService.OpenFileAsync("Load Macro", MacroFilter);
		if (string.IsNullOrWhiteSpace(filePath))
		{
			return;
		}

		await LoadFromFileAsync(filePath);
	}

	public async Task LoadFromFileAsync(string filePath)
	{
		if (string.IsNullOrWhiteSpace(filePath))
		{
			return;
		}

		var document = await _macroFileService.LoadAsync(filePath);
		Code = document.Code;
		Executions = document.Executions;

		Resources.Clear();
		foreach (var resource in document.Resources)
		{
			Resources.Add(new ResourceViewModel
			{
				Name = resource.Name,
				Value = resource.Value
			});
		}

		RunStatusMessage = GetReadyStatusMessage();
	}

	private async Task RunAsync()
	{
		if (!_macroExecutionService.IsSupported)
		{
			await _notificationService.ShowMessageAsync(_macroExecutionService.UnsupportedReason, "Macro Execution Unavailable");
			return;
		}

		if (string.IsNullOrWhiteSpace(Code))
		{
			RunStatusMessage = EmptyCodeStatusMessage;
			await _notificationService.ShowMessageAsync("Enter some macro code before running it.", "Nothing To Run");
			return;
		}

		try
		{
			IsRunButtonEnabled = false;
			RunStatusMessage = "Macro is running. The window will hide until execution completes.";
			_mainWindowService.SetVisible(false);

			await _macroExecutionService.RunAsync(
				Code,
				Executions,
				Resources.ToDictionary(resource => resource.Name, resource => resource.Value),
				CancellationToken.None);

			RunStatusMessage = $"Macro finished at {DateTime.Now:t}.";
		}
		catch (CompilationErrorException ex)
		{
			RunStatusMessage = FormatCompilationErrors(ex);
		}
		catch (OperationCanceledException)
		{
			RunStatusMessage = "Macro run was canceled.";
		}
		catch (Exception ex)
		{
			RunStatusMessage = $"Macro run failed: {ex.Message}";
			throw;
		}
		finally
		{
			_mainWindowService.SetVisible(true);
			IsRunButtonEnabled = true;
		}
	}

	private string GetReadyStatusMessage()
	{
		return string.IsNullOrWhiteSpace(Code)
			? EmptyCodeStatusMessage
			: ReadyStatusMessage;
	}

	private static string FormatCompilationErrors(CompilationErrorException ex)
	{
		var diagnostics = ex.Diagnostics
			.Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
			.ToArray();

		if (diagnostics.Length == 0)
		{
			return $"Compile error: {ex.Message}";
		}

		var displayedDiagnostics = diagnostics
			.Take(3)
			.Select(FormatDiagnostic)
			.ToArray();

		var remainingCount = diagnostics.Length - displayedDiagnostics.Length;
		var remainingSuffix = remainingCount > 0 ? $" (+{remainingCount} more)" : string.Empty;
		return $"Compile error: {string.Join(" | ", displayedDiagnostics)}{remainingSuffix}";
	}

	private static string FormatDiagnostic(Diagnostic diagnostic)
	{
		var lineSpan = diagnostic.Location.GetLineSpan();
		if (!lineSpan.IsValid)
		{
			return diagnostic.GetMessage();
		}

		var line = lineSpan.StartLinePosition.Line + 1;
		var column = lineSpan.StartLinePosition.Character + 1;
		return $"L{line}:C{column} {diagnostic.GetMessage()}";
	}
}