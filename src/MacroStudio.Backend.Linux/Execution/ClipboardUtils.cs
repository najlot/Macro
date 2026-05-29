using System.Diagnostics;
using TextCopy;

namespace MacroStudio.Backend.Linux.Execution;

internal static class ClipboardUtils
{
	public static string GetClipboardText()
	{
		try
		{
			return ClipboardService.GetText() ?? string.Empty;
		}
		catch (Exception ex)
		{
			Debug.WriteLine($"Failed to get clipboard text: {ex}");
			return string.Empty;
		}
	}

	public static void SetClipboardText(string text)
	{
		try
		{
			ClipboardService.SetText(text);
		}
		catch (Exception ex)
		{
			Debug.WriteLine($"Failed to set clipboard text: {ex}");
		}
	}
}