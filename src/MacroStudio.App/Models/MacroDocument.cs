using System.Collections.ObjectModel;

namespace MacroStudio.App.Models;

public sealed class MacroDocument
{
	public string Code { get; init; } = string.Empty;
	public int Executions { get; init; } = 1;
	public Collection<MacroResource> Resources { get; init; } = [];
}