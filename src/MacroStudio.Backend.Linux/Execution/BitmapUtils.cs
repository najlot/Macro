using Avalonia.Media.Imaging;

namespace MacroStudio.Backend.Linux.Execution;

internal static class BitmapUtils
{
	public static Bitmap GetBitmap(byte[] bytes)
	{
		using var stream = new MemoryStream(bytes, writable: false);
		return new Bitmap(stream);
	}

	public static Bitmap GetBitmap(string path) => new(path);
}