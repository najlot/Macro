namespace MacroStudio.App.Services;

public interface IMacroRecordingService
{
	bool IsSupported { get; }
	string UnsupportedReason { get; }
	Task<string?> RecordAsync(bool verbose, CancellationToken cancellationToken);
	Task SaveScreenshotAsync(CancellationToken cancellationToken);
}