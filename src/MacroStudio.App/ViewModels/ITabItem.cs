namespace MacroStudio.App.ViewModels;

public interface ITabItem
{
	string Title { get; }
	bool IsSelected { get; set; }
}