using MacroStudio.App.Services;

namespace MacroStudio.Avalonia.Services;

internal sealed class UnsupportedMacroRecordingService : IMacroRecordingService
{
	public UnsupportedMacroRecordingService(string unsupportedReason)
	{
		UnsupportedReason = unsupportedReason;
	}

	public bool IsSupported => false;
	public string UnsupportedReason { get; }

	public Task<string?> RecordAsync(bool verbose, CancellationToken cancellationToken)
	{
		throw new PlatformNotSupportedException(UnsupportedReason);
	}

	public Task SaveScreenshotAsync(CancellationToken cancellationToken)
	{
		throw new PlatformNotSupportedException(UnsupportedReason);
	}
}