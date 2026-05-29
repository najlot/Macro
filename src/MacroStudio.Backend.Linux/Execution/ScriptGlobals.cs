using Avalonia.Media.Imaging;

namespace MacroStudio.Backend.Linux.Execution;

public sealed class ScriptGlobals
{
	private readonly X11AutomationContext? _automation;

	public ScriptGlobals()
	{
	}

	internal ScriptGlobals(X11AutomationContext automation, IReadOnlyDictionary<string, byte[]>? resources = null)
	{
		_automation = automation;

		if (resources is null)
		{
			return;
		}

		foreach (var resource in resources)
		{
			Resources[resource.Key] = resource.Value;
		}
	}

	internal Dictionary<string, byte[]> Resources { get; } = [];

	private X11AutomationContext Automation => _automation ?? throw new InvalidOperationException("Linux automation context is not initialized.");

	public string GetClipboardText() => ClipboardUtils.GetClipboardText();
	public void SetClipboardText(string text) => ClipboardUtils.SetClipboardText(text);

	public void Wait(int milliseconds) => WaitMiliseconds(milliseconds);
	public void WaitMiliseconds(int milliseconds) => WaitUtils.Wait(Automation, TimeSpan.FromMilliseconds(milliseconds));
	public void WaitSeconds(int seconds) => WaitUtils.Wait(Automation, TimeSpan.FromSeconds(seconds));
	public void WaitMinutes(int minutes) => WaitUtils.Wait(Automation, TimeSpan.FromMinutes(minutes));
	public void WaitHours(int hours) => WaitUtils.Wait(Automation, TimeSpan.FromHours(hours));

	public void Simulate(int key, int x, int y, bool down, int waitTime) => SimulationUtils.Simulate(Automation, key, x, y, down, waitTime);
	public void Simulate(int key, int x, int y, int waitTime) => SimulationUtils.Simulate(Automation, key, x, y, waitTime);

	public Bitmap GetScreenshot() => Automation.CaptureScreenshot();

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

	public void SetCursorPosition(int x, int y) => Automation.SetCursorPosition(x, y);
	public void MoveCursorTo(int x, int y, int ms) => SimulationUtils.MoveCursorTo(Automation, x, y, ms);

	public void MouseLeftDown() => Automation.MouseLeftDown();
	public void MouseLeftUp() => Automation.MouseLeftUp();
	public void MouseMiddleDown() => Automation.MouseMiddleDown();
	public void MouseMiddleUp() => Automation.MouseMiddleUp();
	public void MouseRightDown() => Automation.MouseRightDown();
	public void MouseRightUp() => Automation.MouseRightUp();

	public void PressKeyboardKey(byte key) => Automation.PressKey(key);
	public void ReleaseKeyboardKey(byte key) => Automation.ReleaseKey(key);
}