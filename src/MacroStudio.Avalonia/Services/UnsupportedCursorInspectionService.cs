using MacroStudio.App.Services;

namespace MacroStudio.Avalonia.Services;

internal sealed class UnsupportedCursorInspectionService : ICursorInspectionService
{
	public UnsupportedCursorInspectionService(string unsupportedReason)
	{
		UnsupportedReason = unsupportedReason;
	}

	public bool IsSupported => false;
	public string UnsupportedReason { get; }

	public (int X, int Y) GetCursorPosition()
	{
		throw new PlatformNotSupportedException(UnsupportedReason);
	}

	public void SetCursorPosition(int x, int y)
	{
		throw new PlatformNotSupportedException(UnsupportedReason);
	}
}