namespace MacroStudio.App.Services;

public interface IFileDialogService
{
	Task<string?> OpenFileAsync(string title, params FileDialogFilter[] filters);
	Task<string?> SaveFileAsync(string title, string suggestedFileName, params FileDialogFilter[] filters);
}