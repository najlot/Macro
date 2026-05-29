using Avalonia.Controls;
using MacroStudio.App.Services;

namespace MacroStudio.Avalonia.Services;

internal sealed class MainWindowService : IMainWindowService
{
	private readonly MainWindowContext _windowContext;

	public MainWindowService(MainWindowContext windowContext)
	{
		_windowContext = windowContext;
	}

	public void SetVisible(bool visible)
	{
		if (_windowContext.MainWindow is not Window window)
		{
			return;
		}

		window.ShowInTaskbar = visible;
		if (visible)
		{
			window.Show();
			window.Activate();
		}
		else
		{
			window.Hide();
		}
	}
}