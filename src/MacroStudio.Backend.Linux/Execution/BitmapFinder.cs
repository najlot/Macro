using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace MacroStudio.Backend.Linux.Execution;

internal static class BitmapFinder
{
	public static Rectangle SearchBitmap(Bitmap smallBmp, Bitmap bigBmp, double tolerance, int startX = 0, int startY = 0)
	{
		Span<byte[]> smallPixels = GetPixelBytes(smallBmp);
		Span<byte[]> bigPixels = GetPixelBytes(bigBmp);

		return SearchBitmap(smallPixels, bigPixels, tolerance, startX, startY);
	}

	private static Rectangle SearchBitmap(Span<byte[]> smallPixels, Span<byte[]> bigPixels, double tolerance, int startX = 0, int startY = 0)
	{
		var margin = Convert.ToInt32(255.0 * tolerance);
		var yMax = bigPixels.Length - smallPixels.Length;
		var xMax = bigPixels[0].Length - smallPixels[0].Length;

		for (var y = startY; y < yMax; y++)
		{
			for (var x = startX * 4; x < xMax; x += 4)
			{
				if (IsEqualWithMargin(bigPixels[y][x], smallPixels[0][0], margin)
					&& IsEqualWithMargin(bigPixels[y][x + 1], smallPixels[0][1], margin)
					&& IsEqualWithMargin(bigPixels[y][x + 2], smallPixels[0][2], margin))
				{
					var found = true;
					Span<byte[]> bigSlice = bigPixels.Slice(y, smallPixels.Length);

					var rowIndex = 0;
					foreach (var big in bigSlice)
					{
						if (!found)
						{
							break;
						}

						Span<byte> bigRowSlice = big.AsSpan().Slice(x, smallPixels[rowIndex].Length);
						Span<byte> smallRow = smallPixels[rowIndex];

						for (var columnIndex = 0; columnIndex < bigRowSlice.Length; columnIndex++)
						{
							if (!IsEqualWithMargin(bigRowSlice[columnIndex], smallRow[columnIndex], margin))
							{
								found = false;
								break;
							}
						}

						rowIndex++;
					}

					if (found)
					{
						return new Rectangle(x / 4, y, smallPixels[0].Length / 4, smallPixels.Length);
					}
				}
			}
		}

		return Rectangle.Empty;
	}

	private static byte[][] GetPixelBytes(Bitmap bitmap)
	{
		using var normalized = new WriteableBitmap(bitmap.PixelSize, bitmap.Dpi, PixelFormat.Bgra8888, AlphaFormat.Opaque);
		using var framebuffer = normalized.Lock();
		bitmap.CopyPixels(framebuffer, AlphaFormat.Opaque);

		var result = new byte[framebuffer.Size.Height][];
		for (var y = 0; y < framebuffer.Size.Height; y++)
		{
			result[y] = new byte[framebuffer.Size.Width * 4];
			Marshal.Copy(IntPtr.Add(framebuffer.Address, y * framebuffer.RowBytes), result[y], 0, result[y].Length);
		}

		return result;
	}

	private static bool IsEqualWithMargin(byte byteBig, byte byteSmall, int margin)
		=> byteBig + margin >= byteSmall && byteBig - margin <= byteSmall;
}