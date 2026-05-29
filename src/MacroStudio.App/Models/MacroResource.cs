namespace MacroStudio.App.Models;

public sealed class MacroResource
{
	public string Name { get; init; } = string.Empty;
	public byte[] Value { get; init; } = [];
}