using Avalonia.Controls;
using Avalonia.Platform.Storage;
using MacroStudio.App.Services;
using AppFileDialogFilter = MacroStudio.App.Services.FileDialogFilter;

namespace MacroStudio.Avalonia.Services;

internal sealed class DesktopFileDialogService : IFileDialogService
{
	private readonly MainWindowContext _windowContext;

	public DesktopFileDialogService(MainWindowContext windowContext)
	{
		_windowContext = windowContext;
	}

	public async Task<string?> OpenFileAsync(string title, params AppFileDialogFilter[] filters)
	{
		var storageProvider = GetStorageProvider();
		if (storageProvider is null)
		{
			return null;
		}

		var files = await storageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
		{
			AllowMultiple = false,
			Title = title,
			FileTypeFilter = filters.Select(ToFilePickerFileType).ToList()
		});

		return ToLocalPath(files.FirstOrDefault());
	}

	public async Task<string?> SaveFileAsync(string title, string suggestedFileName, params AppFileDialogFilter[] filters)
	{
		var storageProvider = GetStorageProvider();
		if (storageProvider is null)
		{
			return null;
		}

		var file = await storageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
		{
			Title = title,
			SuggestedFileName = suggestedFileName,
			DefaultExtension = Path.GetExtension(suggestedFileName),
			FileTypeChoices = filters.Select(ToFilePickerFileType).ToList()
		});

		return ToLocalPath(file);
	}

	private IStorageProvider? GetStorageProvider()
	{
		return _windowContext.MainWindow?.StorageProvider;
	}

	private static FilePickerFileType ToFilePickerFileType(AppFileDialogFilter filter)
	{
		return new FilePickerFileType(filter.Name)
		{
			Patterns = filter.Patterns.ToArray()
		};
	}

	private static string? ToLocalPath(IStorageFile? storageFile)
	{
		if (storageFile is null)
		{
			return null;
		}

		return storageFile.TryGetLocalPath() ?? storageFile.Path.LocalPath;
	}
}