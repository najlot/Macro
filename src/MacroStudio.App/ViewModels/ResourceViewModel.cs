using MacroStudio.App.Models;
using MacroStudio.MVVM.ViewModel;

namespace MacroStudio.App.ViewModels;

public class ResourceViewModel : AbstractViewModel
{
	public string Name
	{
		get => field;
		set => Set(ref field, value);
	} = string.Empty;

	public byte[] Value { get; set; } = [];

	public MacroResource ToModel() => new()
	{
		Name = Name,
		Value = Value
	};
}