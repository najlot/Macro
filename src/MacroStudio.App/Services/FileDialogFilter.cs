namespace MacroStudio.App.Services;

public sealed class FileDialogFilter
{
	public string Name { get; }
	public IReadOnlyList<string> Patterns { get; }

	public FileDialogFilter(string name, params string[] patterns)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(name);
		ArgumentNullException.ThrowIfNull(patterns);

		Name = name;
		Patterns = patterns;
	}
}