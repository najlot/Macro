using System.Diagnostics;

namespace MacroStudio.Backend.Linux.Execution;

internal static class SimulationUtils
{
	public static void MoveCursorTo(X11AutomationContext automation, int x, int y, int ms)
	{
		var stopwatch = Stopwatch.StartNew();

		if (automation.IsMacroKeyDown(27))
		{
			throw new TaskCanceledException();
		}

		while (stopwatch.ElapsedMilliseconds < ms)
		{
			Thread.Sleep(5);

			if (automation.IsMacroKeyDown(27))
			{
				throw new TaskCanceledException();
			}

			var percent = stopwatch.ElapsedMilliseconds / (double)ms;
			var cursorPosition = automation.GetCursorPosition();
			var posX = cursorPosition.X + (x - cursorPosition.X) * percent;
			var posY = cursorPosition.Y + (y - cursorPosition.Y) * percent;

			automation.SetCursorPosition((int)posX, (int)posY);
		}

		automation.SetCursorPosition(x, y);
	}

	public static void Simulate(X11AutomationContext automation, int key, int x, int y, bool down, int waitTime)
	{
		MoveCursorTo(automation, x, y, waitTime);

		switch (key)
		{
			case 1:
				if (down)
				{
					automation.MouseLeftDown();
				}
				else
				{
					automation.MouseLeftUp();
				}

				break;

			case 2:
				if (down)
				{
					automation.MouseRightDown();
				}
				else
				{
					automation.MouseRightUp();
				}

				break;

			case 4:
				if (down)
				{
					automation.MouseMiddleDown();
				}
				else
				{
					automation.MouseMiddleUp();
				}

				break;

			default:
				if (down)
				{
					automation.PressKey(key);
				}
				else
				{
					automation.ReleaseKey(key);
				}

				break;
		}
	}

	public static void Simulate(X11AutomationContext automation, int key, int x, int y, int waitTime)
	{
		var stopwatch = Stopwatch.StartNew();

		if (automation.IsMacroKeyDown(27))
		{
			throw new TaskCanceledException();
		}

		while (stopwatch.ElapsedMilliseconds < waitTime)
		{
			Thread.Sleep(5);

			if (automation.IsMacroKeyDown(27))
			{
				throw new TaskCanceledException();
			}
		}

		automation.SetCursorPosition(x, y);

		switch (key)
		{
			case 1:
				automation.MouseLeftDown();
				Thread.Sleep(75);
				automation.MouseLeftUp();
				break;

			case 2:
				automation.MouseRightDown();
				Thread.Sleep(75);
				automation.MouseRightUp();
				break;

			case 4:
				automation.MouseMiddleDown();
				Thread.Sleep(75);
				automation.MouseMiddleUp();
				break;

			default:
				automation.PressKey(key);
				Thread.Sleep(75);
				automation.ReleaseKey(key);
				break;
		}
	}
}