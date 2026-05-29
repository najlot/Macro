namespace MacroStudio.Backend.Linux.Execution;

public readonly record struct Rectangle(int X, int Y, int Width, int Height)
{
	public static Rectangle Empty { get; } = new();

	public bool IsEmpty => this == Empty;
}