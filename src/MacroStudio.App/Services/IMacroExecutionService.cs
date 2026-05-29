namespace MacroStudio.App.Services;

public interface IMacroExecutionService
{
	bool IsSupported { get; }
	string UnsupportedReason { get; }
	void Initialize();
	Task RunAsync(string code, int executions, IReadOnlyDictionary<string, byte[]> resources, CancellationToken cancellationToken);
}