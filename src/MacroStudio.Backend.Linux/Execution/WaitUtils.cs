namespace MacroStudio.Backend.Linux.Execution;

internal static class WaitUtils
{
	public static void Wait(X11AutomationContext automation, TimeSpan waitTime)
	{
		var second = TimeSpan.FromSeconds(1);

		while (waitTime.TotalSeconds >= 1)
		{
			if (automation.IsMacroKeyDown(27))
			{
				throw new TaskCanceledException();
			}

			Thread.Sleep(second);
			waitTime -= second;
		}

		Thread.Sleep(waitTime);

		if (automation.IsMacroKeyDown(27))
		{
			throw new TaskCanceledException();
		}
	}
}