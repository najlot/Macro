using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace MacroStudio.Backend.Linux.Execution;

internal sealed class X11AutomationContext : IDisposable
{
	private const int False = 0;
	private const int True = 1;
	private const int ZPixmap = 2;
	private const int LsbFirst = 0;
	private const uint Button1Mask = 1u << 8;
	private const uint Button2Mask = 1u << 9;
	private const uint Button3Mask = 1u << 10;

	private readonly IntPtr _display;
	private readonly int _screenNumber;
	private readonly IntPtr _rootWindow;

	static X11AutomationContext()
	{
		XInitThreads();
	}

	private X11AutomationContext(IntPtr display, int screenNumber, IntPtr rootWindow)
	{
		_display = display;
		_screenNumber = screenNumber;
		_rootWindow = rootWindow;
		MacroKeyMap = LinuxMacroKeyMap.Build(display);
	}

	public IReadOnlyDictionary<int, ushort[]> MacroKeyMap { get; }

	public static bool TryGetSupportState(bool requireXTest, out string reason)
	{
		if (!OperatingSystem.IsLinux())
		{
			reason = "The Linux automation backend can only run on Linux.";
			return false;
		}

		if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("DISPLAY")))
		{
			reason = "Linux automation currently requires an active X11 or XWayland DISPLAY session.";
			return false;
		}

		try
		{
			using var _ = Open(requireXTest);
			reason = string.Empty;
			return true;
		}
		catch (Exception ex)
		{
			reason = ex.Message;
			return false;
		}
	}

	public static X11AutomationContext Open(bool requireXTest)
	{
		if (!OperatingSystem.IsLinux())
		{
			throw new PlatformNotSupportedException("The Linux automation backend can only run on Linux.");
		}

		if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("DISPLAY")))
		{
			throw new PlatformNotSupportedException("Linux automation currently requires an active X11 or XWayland DISPLAY session.");
		}

		var display = XOpenDisplay(null);
		if (display == IntPtr.Zero)
		{
			throw new InvalidOperationException("Failed to open the active X11 display.");
		}

		try
		{
			var screenNumber = XDefaultScreen(display);
			var rootWindow = XRootWindow(display, screenNumber);

			if (rootWindow == IntPtr.Zero)
			{
				throw new InvalidOperationException("Failed to resolve the X11 root window.");
			}

			if (requireXTest && XTestQueryExtension(display, out _, out _, out _, out _) == False)
			{
				throw new PlatformNotSupportedException("The XTEST extension is not available on this X11 display. Macro playback requires XTEST.");
			}

			return new X11AutomationContext(display, screenNumber, rootWindow);
		}
		catch
		{
			XCloseDisplay(display);
			throw;
		}
	}

	public void Dispose()
	{
		if (_display != IntPtr.Zero)
		{
			XCloseDisplay(_display);
		}
	}

	public (int X, int Y) GetCursorPosition()
	{
		var pointer = QueryPointer();
		return (pointer.X, pointer.Y);
	}

	public void SetCursorPosition(int x, int y)
	{
		if (XTestFakeMotionEvent(_display, _screenNumber, x, y, 0) == False)
		{
			throw new InvalidOperationException("Failed to move the cursor through XTEST.");
		}

		Flush();
	}

	public void MouseLeftDown() => SetMouseButtonState(1, true);
	public void MouseLeftUp() => SetMouseButtonState(1, false);
	public void MouseMiddleDown() => SetMouseButtonState(2, true);
	public void MouseMiddleUp() => SetMouseButtonState(2, false);
	public void MouseRightDown() => SetMouseButtonState(3, true);
	public void MouseRightUp() => SetMouseButtonState(3, false);

	public void PressKey(int macroKey) => SetKeyState(macroKey, true);
	public void ReleaseKey(int macroKey) => SetKeyState(macroKey, false);

	public bool IsMacroKeyDown(int macroKey)
	{
		return IsMacroKeyDown(macroKey, QueryKeymap());
	}

	public bool IsMacroKeyDown(int macroKey, byte[] keymap)
	{
		if (!MacroKeyMap.TryGetValue(macroKey, out var keycodes))
		{
			return false;
		}

		foreach (var keycode in keycodes)
		{
			var byteIndex = keycode / 8;
			var bitIndex = keycode % 8;

			if (byteIndex < keymap.Length && (keymap[byteIndex] & (1 << bitIndex)) != 0)
			{
				return true;
			}
		}

		return false;
	}

	public byte[] QueryKeymap()
	{
		var keymap = new byte[32];
		XQueryKeymap(_display, keymap);
		return keymap;
	}

	public PointerState QueryPointer()
	{
		var success = XQueryPointer(
			_display,
			_rootWindow,
			out _,
			out _,
			out var rootX,
			out var rootY,
			out _,
			out _,
			out var mask);

		if (success == False)
		{
			throw new InvalidOperationException("Failed to query the X11 pointer position.");
		}

		return new PointerState(
			rootX,
			rootY,
			(mask & Button1Mask) != 0,
			(mask & Button3Mask) != 0,
			(mask & Button2Mask) != 0);
	}

	public Bitmap CaptureScreenshot()
	{
		var width = XDisplayWidth(_display, _screenNumber);
		var height = XDisplayHeight(_display, _screenNumber);

		var imageHandle = XGetImage(_display, _rootWindow, 0, 0, (uint)width, (uint)height, ulong.MaxValue, ZPixmap);
		if (imageHandle == IntPtr.Zero)
		{
			throw new InvalidOperationException("Failed to capture a screenshot from the X11 root window.");
		}

		try
		{
			var image = Marshal.PtrToStructure<XImage>(imageHandle);
			if (image.data == IntPtr.Zero)
			{
				throw new InvalidOperationException("The X11 screenshot buffer was empty.");
			}

			var rawLength = checked(image.bytes_per_line * image.height);
			var rawPixels = new byte[rawLength];
			Marshal.Copy(image.data, rawPixels, 0, rawLength);

			var bgraPixels = ConvertToBgra(image, rawPixels);
			var pixelSize = new PixelSize(image.width, image.height);
			var handle = GCHandle.Alloc(bgraPixels, GCHandleType.Pinned);

			try
			{
				return new WriteableBitmap(PixelFormat.Bgra8888, AlphaFormat.Opaque, handle.AddrOfPinnedObject(), pixelSize, new Vector(96, 96), image.width * 4);
			}
			finally
			{
				handle.Free();
			}
		}
		finally
		{
			XDestroyImage(imageHandle);
		}
	}

	private void SetKeyState(int macroKey, bool isPressed)
	{
		if (!MacroKeyMap.TryGetValue(macroKey, out var keycodes) || keycodes.Length == 0)
		{
			throw new NotSupportedException($"The Linux backend does not support macro key code {macroKey}. The current implementation targets the existing Windows macro key set for common keys.");
		}

		if (XTestFakeKeyEvent(_display, keycodes[0], isPressed ? True : False, 0) == False)
		{
			throw new InvalidOperationException($"Failed to synthesize macro key code {macroKey} through XTEST.");
		}

		Flush();
	}

	private void SetMouseButtonState(uint button, bool isPressed)
	{
		if (XTestFakeButtonEvent(_display, button, isPressed ? True : False, 0) == False)
		{
			throw new InvalidOperationException($"Failed to synthesize X11 mouse button {button} through XTEST.");
		}

		Flush();
	}

	private void Flush()
	{
		XFlush(_display);
		XSync(_display, False);
	}

	private static byte[] ConvertToBgra(XImage image, byte[] rawPixels)
	{
		var bytesPerPixel = Math.Max(1, (image.bits_per_pixel + 7) / 8);
		var result = new byte[image.width * image.height * 4];

		for (var y = 0; y < image.height; y++)
		{
			for (var x = 0; x < image.width; x++)
			{
				var sourceOffset = checked(y * image.bytes_per_line + x * bytesPerPixel);
				ulong pixelValue = 0;

				if (image.byte_order == LsbFirst)
				{
					for (var i = 0; i < bytesPerPixel; i++)
					{
						pixelValue |= (ulong)rawPixels[sourceOffset + i] << (i * 8);
					}
				}
				else
				{
					for (var i = 0; i < bytesPerPixel; i++)
					{
						pixelValue = (pixelValue << 8) | rawPixels[sourceOffset + i];
					}
				}

				var destinationOffset = checked((y * image.width + x) * 4);
				result[destinationOffset] = ExtractColorComponent(pixelValue, image.blue_mask);
				result[destinationOffset + 1] = ExtractColorComponent(pixelValue, image.green_mask);
				result[destinationOffset + 2] = ExtractColorComponent(pixelValue, image.red_mask);
				result[destinationOffset + 3] = 255;
			}
		}

		return result;
	}

	private static byte ExtractColorComponent(ulong pixelValue, ulong mask)
	{
		if (mask == 0)
		{
			return 0;
		}

		var shift = System.Numerics.BitOperations.TrailingZeroCount(mask);
		var component = (pixelValue & mask) >> shift;
		var max = mask >> shift;

		if (max == 0)
		{
			return 0;
		}

		return (byte)((component * 255 + max / 2) / max);
	}

	[DllImport("libX11")]
	private static extern int XInitThreads();

	[DllImport("libX11")]
	private static extern IntPtr XOpenDisplay(string? displayName);

	[DllImport("libX11")]
	private static extern int XCloseDisplay(IntPtr display);

	[DllImport("libX11")]
	private static extern int XDefaultScreen(IntPtr display);

	[DllImport("libX11")]
	private static extern IntPtr XRootWindow(IntPtr display, int screenNumber);

	[DllImport("libX11")]
	private static extern int XDisplayWidth(IntPtr display, int screenNumber);

	[DllImport("libX11")]
	private static extern int XDisplayHeight(IntPtr display, int screenNumber);

	[DllImport("libX11")]
	private static extern int XQueryPointer(
		IntPtr display,
		IntPtr window,
		out IntPtr rootReturn,
		out IntPtr childReturn,
		out int rootXReturn,
		out int rootYReturn,
		out int winXReturn,
		out int winYReturn,
		out uint maskReturn);

	[DllImport("libX11")]
	private static extern void XQueryKeymap(IntPtr display, byte[] keysReturn);

	[DllImport("libX11")]
	internal static extern byte XKeysymToKeycode(IntPtr display, nuint keySym);

	[DllImport("libX11")]
	private static extern IntPtr XGetImage(IntPtr display, IntPtr drawable, int x, int y, uint width, uint height, ulong planeMask, int format);

	[DllImport("libX11")]
	private static extern int XDestroyImage(IntPtr image);

	[DllImport("libX11")]
	private static extern int XFlush(IntPtr display);

	[DllImport("libX11")]
	private static extern int XSync(IntPtr display, int discard);

	[DllImport("libXtst")]
	private static extern int XTestQueryExtension(IntPtr display, out int eventBase, out int errorBase, out int majorVersion, out int minorVersion);

	[DllImport("libXtst")]
	private static extern int XTestFakeKeyEvent(IntPtr display, uint keycode, int isPress, ulong delay);

	[DllImport("libXtst")]
	private static extern int XTestFakeButtonEvent(IntPtr display, uint button, int isPress, ulong delay);

	[DllImport("libXtst")]
	private static extern int XTestFakeMotionEvent(IntPtr display, int screenNumber, int x, int y, ulong delay);

	[StructLayout(LayoutKind.Sequential)]
	private struct XImage
	{
		public int width;
		public int height;
		public int xoffset;
		public int format;
		public IntPtr data;
		public int byte_order;
		public int bitmap_unit;
		public int bitmap_bit_order;
		public int bitmap_pad;
		public int depth;
		public int bytes_per_line;
		public int bits_per_pixel;
		public ulong red_mask;
		public ulong green_mask;
		public ulong blue_mask;
		public IntPtr obdata;
		public IntPtr funcs;
	}

	public readonly record struct PointerState(int X, int Y, bool IsLeftButtonDown, bool IsRightButtonDown, bool IsMiddleButtonDown);
}