using System.Drawing;
using System.IO;

namespace MacroStudio.Execution;

public static class BitmapUtils
{
	public static Bitmap GetBitmap(string path)
	{
		var bytes = File.ReadAllBytes(path);
		using var str = new MemoryStream(bytes);
		return new Bitmap(Bitmap.FromStream(str));
	}
}
