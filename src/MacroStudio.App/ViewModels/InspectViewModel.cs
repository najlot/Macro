using System.Threading;
using System.Windows.Input;
using MacroStudio.App.Services;
using MacroStudio.MVVM;
using MacroStudio.MVVM.ViewModel;

namespace MacroStudio.App.ViewModels;

public class InspectViewModel : AbstractViewModel, ITabItem
{
	private readonly ICursorInspectionService _cursorInspectionService;
	private readonly Timer _timer;
	private readonly SynchronizationContext? _synchronizationContext = SynchronizationContext.Current;

	public string Title => "Inspect";

	public bool IsSelected
	{
		get => field;
		set
		{
			if (!Set(ref field, value))
			{
				return;
			}

			if (!_cursorInspectionService.IsSupported)
			{
				CursorPosition = _cursorInspectionService.UnsupportedReason;
				return;
			}

			_timer.Change(value ? 0 : Timeout.Infinite, value ? 50 : Timeout.Infinite);
		}
	}

	public string CursorPosition
	{
		get => field;
		set => Set(ref field, value);
	} = string.Empty;

	public int XPos
	{
		get => field;
		set => Set(ref field, value);
	}

	public int YPos
	{
		get => field;
		set => Set(ref field, value);
	}

	public bool IsCursorControlSupported => _cursorInspectionService.IsSupported;
	public string CursorControlSupportMessage => _cursorInspectionService.UnsupportedReason;

	public ICommand SetCursorPositionCommand { get; }

	public InspectViewModel(ICursorInspectionService cursorInspectionService)
	{
		_cursorInspectionService = cursorInspectionService;
		_timer = new Timer(OnTimerElapsed, null, Timeout.Infinite, Timeout.Infinite);
		SetCursorPositionCommand = new RelayCommand(OnSetCursorPositionClicked, () => _cursorInspectionService.IsSupported);

		if (!_cursorInspectionService.IsSupported)
		{
			CursorPosition = _cursorInspectionService.UnsupportedReason;
		}
	}

	private void OnSetCursorPositionClicked()
	{
		_cursorInspectionService.SetCursorPosition(XPos, YPos);
	}

	private void OnTimerElapsed(object? state)
	{
		var position = _cursorInspectionService.GetCursorPosition();
		var text = $"X: {position.X} Y: {position.Y}";

		if (_synchronizationContext is null)
		{
			CursorPosition = text;
			return;
		}

		_synchronizationContext.Post(_ => CursorPosition = text, null);
	}
}