using MacroStudio.App.Services;
using MacroStudio.Core;

namespace MacroStudio.Backend.Windows.Services;

public sealed class WindowsCursorInspectionService : ICursorInspectionService
{
	public bool IsSupported => true;
	public string UnsupportedReason => string.Empty;

	public (int X, int Y) GetCursorPosition()
	{
		var position = Mouse.GetCursorPosition();
		return (position.X, position.Y);
	}

	public void SetCursorPosition(int x, int y)
	{
		Mouse.SetCursorPosition(x, y);
	}
}