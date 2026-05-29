using System.Collections.Concurrent;

namespace MacroStudio.Backend.Linux.Execution;

internal static class RecordedMacroFormatter
{
	public static string Build(ConcurrentStack<TimedAction> actions, bool verbose)
	{
		var actionList = actions.ToList();
		actionList.Reverse();

		return string.Join("\r\n", actionList.Select(action =>
		{
			var down = action.KeyDown.ToString().ToLowerInvariant();

			if (verbose && action.Key == 1)
			{
				var command = $"MoveCursorTo({action.X}, {action.Y}, {action.Miliseconds});\r\n";
				command += action.KeyDown ? "MouseLeftDown();" : "MouseLeftUp();";
				return command;
			}

			if (verbose && action.Key == 2)
			{
				var command = $"MoveCursorTo({action.X}, {action.Y}, {action.Miliseconds});\r\n";
				command += action.KeyDown ? "MouseRightDown();" : "MouseRightUp();";
				return command;
			}

			if (verbose && action.Key == 4)
			{
				var command = $"MoveCursorTo({action.X}, {action.Y}, {action.Miliseconds});\r\n";
				command += action.KeyDown ? "MouseMiddleDown();" : "MouseMiddleUp();";
				return command;
			}

			if (verbose && action.Key >= 32 && action.Key <= 125)
			{
				var command = $"MoveCursorTo({action.X}, {action.Y}, {action.Miliseconds});\r\n";
				command += action.KeyDown
					? $"PressKeyboardKey({action.Key} /*{(char)action.Key}*/);"
					: $"ReleaseKeyboardKey({action.Key} /*{(char)action.Key}*/);";
				return command;
			}

			return $"Simulate({action.Key}, {action.X}, {action.Y}, {down}, {action.Miliseconds});";
		}));
	}
}