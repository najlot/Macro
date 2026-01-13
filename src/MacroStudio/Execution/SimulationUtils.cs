using MacroStudio.Core;
using System.Diagnostics;

namespace MacroStudio.Execution;

public static class SimulationUtils
{
	public static void MoveCursorTo(int x, int y, int ms)
	{
		var sw = Stopwatch.StartNew();

		if (Keyboard.IsKeyDown(27))
		{
			throw new TaskCanceledException();
		}

		while (sw.ElapsedMilliseconds < ms)
		{
			Thread.Sleep(5);

			if (Keyboard.IsKeyDown(27))
			{
				throw new TaskCanceledException();
			}

			var percent = sw.ElapsedMilliseconds / ((double)ms);

			var pCursorPos = Mouse.GetCursorPosition();

			var posX = pCursorPos.X + (x - pCursorPos.X) * percent;
			var posY = pCursorPos.Y + (y - pCursorPos.Y) * percent;

			Mouse.SetCursorPosition((int)posX, (int)posY);
		}

		Mouse.SetCursorPosition(x, y);
	}

	public static void Simulate(int key, int x, int y, bool down, int waitTime)
	{
		MoveCursorTo(x, y, waitTime);

		switch (key)
		{
			case 1: // Left mouse key
				if (down)
				{
					Mouse.LeftDown();
				}
				else
				{
					Mouse.LeftUp();
				}

				break;

			case 2: // Right mouse key
				if (down)
				{
					Mouse.RightDown();
				}
				else
				{
					Mouse.RightUp();
				}

				break;

			case 4: // Middle mouse key
				if (down)
				{
					Mouse.MiddleDown();
				}
				else
				{
					Mouse.MiddleUp();
				}

				break;

			default: // Keyboard
				if (down)
				{
					Keyboard.PressKey((byte)key);
				}
				else
				{
					Keyboard.ReleaseKey((byte)key);
				}
				break;
		}
	}

	public static void Simulate(int key, int x, int y, int waitTime)
	{
		var sw = Stopwatch.StartNew();

		if (Keyboard.IsKeyDown(27))
		{
			throw new TaskCanceledException();
		}

		while (sw.ElapsedMilliseconds < waitTime)
		{
			Thread.Sleep(5);

			if (Keyboard.IsKeyDown(27))
			{
				throw new TaskCanceledException();
			}
		}

		Mouse.SetCursorPosition(x, y);

		switch (key)
		{
			case 1: // Left mouse key
				Mouse.LeftDown();
				Thread.Sleep(75);
				Mouse.LeftUp();
				break;

			case 2: // Right mouse key
				Mouse.RightDown();
				Thread.Sleep(75);
				Mouse.RightUp();
				break;

			case 4: // Middle mouse key
				Mouse.MiddleDown();
				Thread.Sleep(75);
				Mouse.MiddleUp();
				break;

			default: // Keyboard
				Keyboard.PressKey((byte)key);
				Thread.Sleep(75);
				Keyboard.ReleaseKey((byte)key);
				break;
		}
	}
}
