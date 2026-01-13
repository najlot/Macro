using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace MacroStudio.Core;

public class BitmapFinder
{
	public static Rectangle SearchBitmap(Bitmap smallBmp, Bitmap bigBmp, double tolerance, int startX = 0, int startY = 0)
	{
		Span<byte[]> smallPixels = GetPixelBytes(smallBmp);
		Span<byte[]> bigPixels = GetPixelBytes(bigBmp);

		return SearchBitmap(smallPixels, bigPixels, tolerance, startX, startY);
	}

	public static Rectangle SearchBitmap(Span<byte[]> smallPixels, Span<byte[]> bigPixels, double tolerance, int startX = 0, int startY = 0)
	{
		int margin = Convert.ToInt32(255.0 * tolerance);

		int yMax = bigPixels.Length - smallPixels.Length;
		int xMax = bigPixels[0].Length - smallPixels[0].Length;

		for (int y = startY; y < yMax; y++)
		{
			for (int x = startX * 3; x < xMax; x += 3)
			{
				if (IsEqualWithMargin(bigPixels[y][x], smallPixels[0][0], margin)
					&& IsEqualWithMargin(bigPixels[y][x + 1], smallPixels[0][1], margin)
					&& IsEqualWithMargin(bigPixels[y][x + 2], smallPixels[0][2], margin))
				{
					bool found = true;
					Span<byte[]> bigSlice = bigPixels.Slice(y, smallPixels.Length);

					int i = 0;
					foreach (var big in bigSlice)
					{
						if (!found) break;

						Span<byte> rowBig = big;
						Span<byte> bigRowSlice = rowBig.Slice(x, smallPixels[i].Length);
						Span<byte> rowSmal = smallPixels[i];

						int a = 0;
						foreach (var bigByte in bigRowSlice)
						{
							if (!IsEqualWithMargin(bigByte, rowSmal[a], margin))
							{
								found = false;
								break;
							}

							a++;
						}

						i++;
					}

					if (found)
					{
						return new Rectangle
						{
							X = x / 3,
							Y = y,
							Width = smallPixels[0].Length / 3,
							Height = smallPixels.Length,
						};
					}
				}
			}
		}

		return Rectangle.Empty;
	}

	public static byte[][] GetPixelBytes(Bitmap bitmap)
	{
		var result = new byte[bitmap.Height][];
		var bitmapData = bitmap.LockBits(
				new Rectangle(0, 0, bitmap.Width, bitmap.Height),
				ImageLockMode.ReadOnly,
				PixelFormat.Format24bppRgb);

		try
		{
			for (int y = 0; y < bitmap.Height; ++y)
			{
				result[y] = new byte[bitmap.Width * 3];
				Marshal.Copy(bitmapData.Scan0 + y * bitmapData.Stride, result[y], 0, result[y].Length);
			}
		}
		finally
		{
			bitmap.UnlockBits(bitmapData);
		}

		return result;
	}

	private static bool IsEqualWithMargin(byte byteBig, byte byteSmall, int margin)
		=> byteBig + margin >= byteSmall && byteBig - margin <= byteSmall;
}
