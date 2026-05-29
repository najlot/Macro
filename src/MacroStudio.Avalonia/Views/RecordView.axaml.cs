using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace MacroStudio.Avalonia.Views;

public partial class RecordView : UserControl
{
	public RecordView()
	{
		InitializeComponent();
	}

	private void InitializeComponent()
	{
		AvaloniaXamlLoader.Load(this);
	}
}