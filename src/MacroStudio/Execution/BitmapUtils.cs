using System.Drawing;
using System.IO;

namespace MacroStudio.Execution;

public static class BitmapUtils
{
	public static Bitmap GetBitmap(byte[] bytes)
	{
		using var str = new MemoryStream(bytes);
		return new Bitmap(Bitmap.FromStream(str));
	}

    public static Bitmap GetBitmap(string path) => GetBitmap(File.ReadAllBytes(path));
}
