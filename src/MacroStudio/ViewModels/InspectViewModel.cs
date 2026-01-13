using MacroStudio.Core;
using MacroStudio.MVVM;
using MacroStudio.MVVM.ViewModel;

namespace MacroStudio.ViewModels;

public class InspectViewModel : AbstractViewModel, ITabItem
{
	private readonly Timer _timer;

	public bool IsSelected
	{
		get => field;
		set
		{
			if (Set(ref field, value))
			{
				if (value)
				{
					_timer.Change(0, 25);
				}
				else
				{
					_timer.Change(Timeout.Infinite, Timeout.Infinite);
				}
			}
		}
	}

	public string CursorPosition
	{
		get => field;
		set => Set(ref field, value);
	} = string.Empty;

    public InspectViewModel()
	{
		_timer = new Timer(OnTimerElapsed, null, Timeout.Infinite, Timeout.Infinite);
		SetCursorPositionCommand = new RelayCommand(OnSetCursorPositionClicked);
	}

	public int XPos { get; set; }
	public int YPos { get; set; }

	public RelayCommand SetCursorPositionCommand { get; }

	private void OnSetCursorPositionClicked()
	{
		Mouse.SetCursorPosition(XPos, YPos);
	}

	private void OnTimerElapsed(object? state)
	{
		var pos = Mouse.GetCursorPosition();
		CursorPosition = $"X: {pos.X} Y: {pos.Y}";
	}
}
