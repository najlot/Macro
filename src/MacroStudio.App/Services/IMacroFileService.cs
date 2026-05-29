using MacroStudio.App.Models;

namespace MacroStudio.App.Services;

public interface IMacroFileService
{
	Task<MacroDocument> LoadAsync(string filePath, CancellationToken cancellationToken = default);
	Task SaveAsync(string filePath, MacroDocument document, CancellationToken cancellationToken = default);
}