using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace MacroStudio.Core;

public static class DesktopWindow
{
	public static Bitmap GetScreenshot()
	{
		var bounds = Screen.PrimaryScreen.Bounds;
		var bitmap = new Bitmap(bounds.Width, bounds.Height, PixelFormat.Format24bppRgb);
		var graphics = Graphics.FromImage(bitmap);
		graphics.CopyFromScreen(bounds.X, bounds.Y, 0, 0, bounds.Size, CopyPixelOperation.SourceCopy);
		return bitmap;
	}
}
