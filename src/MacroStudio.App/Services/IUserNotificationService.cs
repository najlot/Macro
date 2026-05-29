namespace MacroStudio.App.Services;

public interface IUserNotificationService
{
	Task ShowMessageAsync(string message, string? title = null);
	Task ShowErrorAsync(string message, string? title = null);
}