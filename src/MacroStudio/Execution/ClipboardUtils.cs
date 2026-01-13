using System.Diagnostics;
using System.Windows;

namespace MacroStudio.Execution;

public static class ClipboardUtils
{
	public static string GetClipboardText()
	{
		var text = string.Empty;

		var thread = new Thread(() =>
		{
			try
			{
                text = Clipboard.GetText();
            }
			catch (Exception ex)
			{
				Debug.WriteLine($"Failed to get clipboard text: {ex}");
                text = string.Empty;
            }
		});

		thread.SetApartmentState(ApartmentState.STA);
		thread.Start();
		thread.Join();

		return text;
	}

	public static void SetClipboardText(string text)
	{
		var thread = new Thread(() =>
		{
			try
			{
                Clipboard.SetText(text);
            }
			catch (Exception ex)
			{
                Debug.WriteLine($"Failed to set clipboard text: {ex}");
            }
		});

		thread.SetApartmentState(ApartmentState.STA);
		thread.Start();
		thread.Join();
	}
}
