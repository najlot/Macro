using MacroStudio.App.Services;
using MacroStudio.Backend.Linux.Execution;

namespace MacroStudio.Backend.Linux.Services;

public sealed class LinuxMacroExecutionService : IMacroExecutionService
{
	public bool IsSupported => X11AutomationContext.TryGetSupportState(requireXTest: true, out _);

	public string UnsupportedReason
		=> X11AutomationContext.TryGetSupportState(requireXTest: true, out var reason)
			? string.Empty
			: reason;

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

		using var automation = X11AutomationContext.Open(requireXTest: true);
		var globals = new ScriptGlobals(automation, resources);
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