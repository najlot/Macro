using System.Windows.Controls;

namespace MacroStudio.Views;

public partial class RecordView : UserControl
{
	public RecordView()
	{
		InitializeComponent();
		TextEditorUtils.ApplyDarkTheme(Code);
	}
}
