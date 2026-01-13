using MacroStudio.Core;

namespace MacroStudio.Execution;

public static class WaitUtils
{
	public static void Wait(TimeSpan waitTime)
	{
		var second = TimeSpan.FromSeconds(1);

		while (waitTime.TotalSeconds >= 1)
		{
			if (Keyboard.IsKeyDown(27))
			{
				throw new TaskCanceledException();
			}

			Thread.Sleep(second);
			waitTime -= second;
		}

		Thread.Sleep(waitTime);

		if (Keyboard.IsKeyDown(27))
		{
			throw new TaskCanceledException();
		}
	}
}
