using System.Drawing;
using MacroStudio.Core;

namespace MacroStudio.Backend.Windows.Execution;

public sealed class ScriptGlobals
{
	public ScriptGlobals()
	{
	}

	public ScriptGlobals(IReadOnlyDictionary<string, byte[]> resources)
	{
		foreach (var resource in resources)
		{
			Resources[resource.Key] = resource.Value;
		}
	}

	internal Dictionary<string, byte[]> Resources { get; } = [];

	public string GetClipboardText() => ClipboardUtils.GetClipboardText();
	public void SetClipboardText(string text) => ClipboardUtils.SetClipboardText(text);

	public void Wait(int milliseconds) => WaitMiliseconds(milliseconds);
	public void WaitMiliseconds(int milliseconds) => WaitUtils.Wait(TimeSpan.FromMilliseconds(milliseconds));
	public void WaitSeconds(int seconds) => WaitUtils.Wait(TimeSpan.FromSeconds(seconds));
	public void WaitMinutes(int minutes) => WaitUtils.Wait(TimeSpan.FromMinutes(minutes));
	public void WaitHours(int hours) => WaitUtils.Wait(TimeSpan.FromHours(hours));

	public void Simulate(int key, int x, int y, bool down, int waitTime) => SimulationUtils.Simulate(key, x, y, down, waitTime);
	public void Simulate(int key, int x, int y, int waitTime) => SimulationUtils.Simulate(key, x, y, waitTime);

	public Bitmap GetScreenshot() => DesktopWindow.GetScreenshot();

	public Bitmap GetResourceBitmap(string name) => BitmapUtils.GetBitmap(Resources[name]);
	public Bitmap GetBitmap(string path) => BitmapUtils.GetBitmap(path);

	public Rectangle SearchBitmap(Bitmap smallBmp, Bitmap bigBmp, double tolerance, int startX = 0, int startY = 0)
		=> BitmapFinder.SearchBitmap(smallBmp, bigBmp, tolerance, startX, startY);

	public bool HasBitmap(Bitmap smallBmp, Bitmap bigBmp, double tolerance, int startX = 0, int startY = 0)
		=> BitmapFinder.SearchBitmap(smallBmp, bigBmp, tolerance, startX, startY) != Rectangle.Empty;

	public Rectangle SearchBitmap(Bitmap smallBmp, Bitmap bigBmp, int startX = 0, int startY = 0)
		=> BitmapFinder.SearchBitmap(smallBmp, bigBmp, 0, startX, startY);

	public bool HasBitmap(Bitmap smallBmp, Bitmap bigBmp, int startX = 0, int startY = 0)
		=> BitmapFinder.SearchBitmap(smallBmp, bigBmp, 0, startX, startY) != Rectangle.Empty;

	public void SaveBitmap(string path, Bitmap bitmap) => bitmap.Save(path);

	public void SetCursorPosition(int x, int y) => Mouse.SetCursorPosition(x, y);
	public void MoveCursorTo(int x, int y, int ms) => SimulationUtils.MoveCursorTo(x, y, ms);

	public void MouseLeftDown() => Mouse.LeftDown();
	public void MouseLeftUp() => Mouse.LeftUp();
	public void MouseMiddleDown() => Mouse.MiddleDown();
	public void MouseMiddleUp() => Mouse.MiddleUp();
	public void MouseRightDown() => Mouse.RightDown();
	public void MouseRightUp() => Mouse.RightUp();

	public void PressKeyboardKey(byte key) => Keyboard.PressKey(key);
	public void ReleaseKeyboardKey(byte key) => Keyboard.ReleaseKey(key);
}