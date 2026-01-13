using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace MacroStudio.Core;

public partial class Mouse
{
	[StructLayout(LayoutKind.Sequential)]
	private struct POINT
	{
		public int X;
		public int Y;
	}

	[LibraryImport("user32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static partial bool GetCursorPos(out POINT lpPoint);

	public static Point GetCursorPosition()
	{
		if (GetCursorPos(out POINT lpPoint))
		{
			return new Point(lpPoint.X, lpPoint.Y);
		}

		return Point.Empty;
	}

	[LibraryImport("user32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static partial bool SetCursorPos(int x, int y);

	[LibraryImport("user32.dll")]
	private static partial void mouse_event(uint dwFlags, int dx, int dy, uint dwData, UIntPtr dwExtraInfo);

	public static void LeftDown() => mouse_event((uint)MouseEventFlags.LEFTDOWN, 0, 0, 0, new UIntPtr(0));
	public static void LeftUp() => mouse_event((uint)MouseEventFlags.LEFTUP, 0, 0, 0, new UIntPtr(0));

	public static void MiddleDown() => mouse_event((uint)MouseEventFlags.MIDDLEDOWN, 0, 0, 0, new UIntPtr(0));
	public static void MiddleUp() => mouse_event((uint)MouseEventFlags.MIDDLEUP, 0, 0, 0, new UIntPtr(0));

	public static void RightDown() => mouse_event((uint)MouseEventFlags.RIGHTDOWN, 0, 0, 0, new UIntPtr(0));
	public static void RightUp() => mouse_event((uint)MouseEventFlags.RIGHTUP, 0, 0, 0, new UIntPtr(0));

	public static void SetCursorPosition(int x, int y) => SetCursorPos(x, y);
	public static void SetCursorPosition(Point point) => SetCursorPosition(point.X, point.Y);
}