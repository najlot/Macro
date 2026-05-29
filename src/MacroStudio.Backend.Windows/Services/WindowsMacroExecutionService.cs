using MacroStudio.App.Services;
using MacroStudio.Backend.Windows.Execution;

namespace MacroStudio.Backend.Windows.Services;

public sealed class WindowsMacroExecutionService : IMacroExecutionService
{
	public bool IsSupported => OperatingSystem.IsWindows();

	public string UnsupportedReason => IsSupported
		? string.Empty
		: "Macro execution is currently implemented only for Windows. The Avalonia shell still runs on Linux for editing and managing macro files.";

	public void Initialize()
	{
		if (!IsSupported)
		{
			return;
		}

		ExecutionUtils.Initialize();
	}

	public async Task RunAsync(string code, int executions, IReadOnlyDictionary<string, byte[]> resources, CancellationToken cancellationToken)
	{
		if (!IsSupported)
		{
			throw new PlatformNotSupportedException(UnsupportedReason);
		}

		var globals = new ScriptGlobals(resources);
		var runner = ExecutionUtils.GetRunner(code);
		if (runner is null)
		{
			throw new InvalidOperationException("Failed to create script runner.");
		}

		for (var index = 0; index < executions; index++)
		{
			cancellationToken.ThrowIfCancellationRequested();
			await runner(globals);
		}
	}
}