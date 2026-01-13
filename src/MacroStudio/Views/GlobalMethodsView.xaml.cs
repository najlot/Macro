using System.Windows.Controls;

namespace MacroStudio.Views;

public partial class GlobalMethodsView : UserControl
{
	public GlobalMethodsView()
	{
		InitializeComponent();
		TextEditorUtils.ApplyDarkTheme(Code);
	}
}
