using MacroStudio.App.Services;
using MacroStudio.Backend.Linux.Execution;

namespace MacroStudio.Backend.Linux.Services;

public sealed class LinuxCursorInspectionService : ICursorInspectionService
{
	public bool IsSupported => X11AutomationContext.TryGetSupportState(requireXTest: true, out _);

	public string UnsupportedReason
		=> X11AutomationContext.TryGetSupportState(requireXTest: true, out var reason)
			? string.Empty
			: reason;

	public (int X, int Y) GetCursorPosition()
	{
		using var automation = X11AutomationContext.Open(requireXTest: true);
		return automation.GetCursorPosition();
	}

	public void SetCursorPosition(int x, int y)
	{
		using var automation = X11AutomationContext.Open(requireXTest: true);
		automation.SetCursorPosition(x, y);
	}
}