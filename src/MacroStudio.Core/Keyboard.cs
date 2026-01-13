using System;
using System.Runtime.InteropServices;

namespace MacroStudio.Core;

public partial class Keyboard
{
	[LibraryImport("User32.dll")]
	private static partial short GetAsyncKeyState(int vKey);

	[LibraryImport("user32.dll")]
	private static partial void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

	public static bool IsKeyDown(int key) => (GetAsyncKeyState(key) & 0x8000) > 0;

	public static void PressKey(byte key) => keybd_event(key, key, 0, new UIntPtr(0));
	public static void ReleaseKey(byte key) => keybd_event(key, key, 2, new UIntPtr(0));
}
