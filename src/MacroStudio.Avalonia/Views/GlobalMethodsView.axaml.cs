using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace MacroStudio.Avalonia.Views;

public partial class GlobalMethodsView : UserControl
{
	public GlobalMethodsView()
	{
		InitializeComponent();
	}

	private void InitializeComponent()
	{
		AvaloniaXamlLoader.Load(this);
	}
}