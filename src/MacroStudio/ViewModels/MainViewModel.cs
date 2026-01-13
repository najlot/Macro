using MacroStudio.MVVM.ViewModel;

namespace MacroStudio.ViewModels;

public class MainViewModel : AbstractViewModel
{
	public RecordViewModel RecordViewModel { get; }
	public ExecuteViewModel ExecuteViewModel { get; }
	public InspectViewModel InspectViewModel { get; }
	public GlobalMethodsViewModel GlobalMethodsViewModel { get; }

	public int SelectedTabIndex
	{
		get => field;
		set => Set(ref field, value);
	}

	public MainViewModel(Action<bool> showMainWindow)
	{
		RecordViewModel = new(showMainWindow);
		ExecuteViewModel = new(showMainWindow);
		InspectViewModel = new();
		GlobalMethodsViewModel = new();
	}
}
