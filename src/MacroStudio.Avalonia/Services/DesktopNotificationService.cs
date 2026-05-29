using Avalonia.Controls;
using MacroStudio.App.Services;
using MacroStudio.Avalonia.Views;

namespace MacroStudio.Avalonia.Services;

internal sealed class DesktopNotificationService : IUserNotificationService
{
	private readonly MainWindowContext _windowContext;

	public DesktopNotificationService(MainWindowContext windowContext)
	{
		_windowContext = windowContext;
	}

	public Task ShowMessageAsync(string message, string? title = null)
	{
		return ShowDialogAsync(message, title ?? "Macro Studio");
	}

	public Task ShowErrorAsync(string message, string? title = null)
	{
		return ShowDialogAsync(message, title ?? "Macro Studio Error");
	}

	private async Task ShowDialogAsync(string message, string title)
	{
		var dialog = new MessageDialogWindow(title, message);
		if (_windowContext.MainWindow is { } owner)
		{
			await dialog.ShowDialog(owner);
			return;
		}

		dialog.Show();
	}
}