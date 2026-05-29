using MacroStudio.MVVM.ViewModel;

namespace MacroStudio.App.ViewModels;

public class MainViewModel : AbstractViewModel
{
	public RecordViewModel RecordViewModel { get; }
	public ExecuteViewModel ExecuteViewModel { get; }
	public InspectViewModel InspectViewModel { get; }
	public GlobalMethodsViewModel GlobalMethodsViewModel { get; }
	public ITabItem[] Tabs { get; }

	public int SelectedTabIndex
	{
		get => field;
		set
		{
			if (Set(ref field, value))
			{
				UpdateSelection();
			}
		}
	}

	public MainViewModel(
		RecordViewModel recordViewModel,
		ExecuteViewModel executeViewModel,
		InspectViewModel inspectViewModel,
		GlobalMethodsViewModel globalMethodsViewModel)
	{
		RecordViewModel = recordViewModel;
		ExecuteViewModel = executeViewModel;
		InspectViewModel = inspectViewModel;
		GlobalMethodsViewModel = globalMethodsViewModel;
		Tabs = [RecordViewModel, ExecuteViewModel, InspectViewModel, GlobalMethodsViewModel];

		SelectedTabIndex = 0;
	}

	private void UpdateSelection()
	{
		for (var index = 0; index < Tabs.Length; index++)
		{
			Tabs[index].IsSelected = index == SelectedTabIndex;
		}
	}
}