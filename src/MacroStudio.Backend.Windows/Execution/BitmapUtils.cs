using System.Drawing;
using System.IO;

namespace MacroStudio.Backend.Windows.Execution;

internal static class BitmapUtils
{
	public static Bitmap GetBitmap(byte[] bytes)
	{
		using var stream = new MemoryStream(bytes);
		return new Bitmap(Bitmap.FromStream(stream));
	}

	public static Bitmap GetBitmap(string path) => GetBitmap(File.ReadAllBytes(path));
}