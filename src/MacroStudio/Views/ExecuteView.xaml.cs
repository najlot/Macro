using System.Windows.Controls;

namespace MacroStudio.Views;

public partial class ExecuteView : UserControl
{
	public ExecuteView()
	{
		InitializeComponent();
		TextEditorUtils.ApplyDarkTheme(Code);
		Code.Focus();
	}
}
