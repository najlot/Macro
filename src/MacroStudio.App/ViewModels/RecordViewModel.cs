using System.Windows.Input;
using MacroStudio.App.Services;
using MacroStudio.MVVM;
using MacroStudio.MVVM.ViewModel;

namespace MacroStudio.App.ViewModels;

public class RecordViewModel : AbstractViewModel, ITabItem
{
	private readonly IMacroRecordingService _macroRecordingService;
	private readonly IUserNotificationService _notificationService;
	private readonly IMainWindowService _mainWindowService;
	private readonly AsyncCommand _startRecordingCommand;

	public string Title => "Record";

	public bool IsSelected
	{
		get => field;
		set => Set(ref field, value);
	}

	public bool Verbose
	{
		get => field;
		set => Set(ref field, value);
	} = true;

	public string Code
	{
		get => field;
		set => Set(ref field, value);
	} = string.Empty;

	public bool IsRecordEnabled
	{
		get => field;
		set
		{
			if (Set(ref field, value))
			{
				_startRecordingCommand.RaiseCanExecuteChanged();
			}
		}
	} = true;

	public bool IsRecordingSupported => _macroRecordingService.IsSupported;
	public string RecordingSupportMessage => _macroRecordingService.UnsupportedReason;

	public ICommand StartRecordingCommand => _startRecordingCommand;
	public ICommand SaveScreenshotCommand { get; }

	public RecordViewModel(
		IMacroRecordingService macroRecordingService,
		IUserNotificationService notificationService,
		IMainWindowService mainWindowService)
	{
		_macroRecordingService = macroRecordingService;
		_notificationService = notificationService;
		_mainWindowService = mainWindowService;

		_startRecordingCommand = new AsyncCommand(StartRecordingAsync, task => ShowErrorAsync(task.Exception), () => IsRecordEnabled && _macroRecordingService.IsSupported);
		SaveScreenshotCommand = new AsyncCommand(SaveScreenshotAsync, task => ShowErrorAsync(task.Exception));
	}

	private Task ShowErrorAsync(Exception? ex)
	{
		return _notificationService.ShowErrorAsync(ex?.ToString() ?? "Unknown error.");
	}

	private async Task StartRecordingAsync()
	{
		if (!_macroRecordingService.IsSupported)
		{
			await _notificationService.ShowMessageAsync(_macroRecordingService.UnsupportedReason, "Macro Recording Unavailable");
			return;
		}

		try
		{
			IsRecordEnabled = false;
			_mainWindowService.SetVisible(false);

			var recordedCode = await _macroRecordingService.RecordAsync(Verbose, CancellationToken.None);
			if (!string.IsNullOrWhiteSpace(recordedCode))
			{
				Code = recordedCode;
			}
		}
		catch (OperationCanceledException)
		{
		}
		finally
		{
			_mainWindowService.SetVisible(true);
			IsRecordEnabled = true;
		}
	}

	private async Task SaveScreenshotAsync()
	{
		if (!_macroRecordingService.IsSupported)
		{
			await _notificationService.ShowMessageAsync(_macroRecordingService.UnsupportedReason, "Screenshot Capture Unavailable");
			return;
		}

		await _macroRecordingService.SaveScreenshotAsync(CancellationToken.None);
	}
}