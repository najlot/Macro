using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace MacroStudio.Avalonia.Views;

public partial class InspectView : UserControl
{
	public InspectView()
	{
		InitializeComponent();
	}

	private void InitializeComponent()
	{
		AvaloniaXamlLoader.Load(this);
	}
}