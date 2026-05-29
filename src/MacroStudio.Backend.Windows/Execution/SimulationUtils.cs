using System.Diagnostics;
using MacroStudio.Core;

namespace MacroStudio.Backend.Windows.Execution;

internal static class SimulationUtils
{
	public static void MoveCursorTo(int x, int y, int ms)
	{
		var stopwatch = Stopwatch.StartNew();

		if (Keyboard.IsKeyDown(27))
		{
			throw new TaskCanceledException();
		}

		while (stopwatch.ElapsedMilliseconds < ms)
		{
			Thread.Sleep(5);

			if (Keyboard.IsKeyDown(27))
			{
				throw new TaskCanceledException();
			}

			var percent = stopwatch.ElapsedMilliseconds / (double)ms;
			var cursorPosition = Mouse.GetCursorPosition();
			var posX = cursorPosition.X + (x - cursorPosition.X) * percent;
			var posY = cursorPosition.Y + (y - cursorPosition.Y) * percent;

			Mouse.SetCursorPosition((int)posX, (int)posY);
		}

		Mouse.SetCursorPosition(x, y);
	}

	public static void Simulate(int key, int x, int y, bool down, int waitTime)
	{
		MoveCursorTo(x, y, waitTime);

		switch (key)
		{
			case 1:
				if (down)
				{
					Mouse.LeftDown();
				}
				else
				{
					Mouse.LeftUp();
				}

				break;

			case 2:
				if (down)
				{
					Mouse.RightDown();
				}
				else
				{
					Mouse.RightUp();
				}

				break;

			case 4:
				if (down)
				{
					Mouse.MiddleDown();
				}
				else
				{
					Mouse.MiddleUp();
				}

				break;

			default:
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
		var stopwatch = Stopwatch.StartNew();

		if (Keyboard.IsKeyDown(27))
		{
			throw new TaskCanceledException();
		}

		while (stopwatch.ElapsedMilliseconds < waitTime)
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
			case 1:
				Mouse.LeftDown();
				Thread.Sleep(75);
				Mouse.LeftUp();
				break;

			case 2:
				Mouse.RightDown();
				Thread.Sleep(75);
				Mouse.RightUp();
				break;

			case 4:
				Mouse.MiddleDown();
				Thread.Sleep(75);
				Mouse.MiddleUp();
				break;

			default:
				Keyboard.PressKey((byte)key);
				Thread.Sleep(75);
				Keyboard.ReleaseKey((byte)key);
				break;
		}
	}
}