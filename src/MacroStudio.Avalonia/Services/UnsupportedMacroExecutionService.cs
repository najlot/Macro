using MacroStudio.App.Services;

namespace MacroStudio.Avalonia.Services;

internal sealed class UnsupportedMacroExecutionService : IMacroExecutionService
{
	public UnsupportedMacroExecutionService(string unsupportedReason)
	{
		UnsupportedReason = unsupportedReason;
	}

	public bool IsSupported => false;
	public string UnsupportedReason { get; }

	public void Initialize()
	{
	}

	public Task RunAsync(string code, int executions, IReadOnlyDictionary<string, byte[]> resources, CancellationToken cancellationToken)
	{
		throw new PlatformNotSupportedException(UnsupportedReason);
	}
}