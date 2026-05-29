using System.Collections.Concurrent;
using System.Diagnostics;
using MacroStudio.App.Services;
using MacroStudio.Backend.Windows.Execution;
using MacroStudio.Core;

namespace MacroStudio.Backend.Windows.Services;

public sealed class WindowsMacroRecordingService : IMacroRecordingService
{
	private static readonly FileDialogFilter BitmapFilter = new("Images", "*.bmp");
	private readonly IFileDialogService _fileDialogService;

	public WindowsMacroRecordingService(IFileDialogService fileDialogService)
	{
		_fileDialogService = fileDialogService;
	}

	public bool IsSupported => OperatingSystem.IsWindows();

	public string UnsupportedReason => IsSupported
		? string.Empty
		: "Macro recording is currently implemented only for Windows. Linux support needs platform-specific input hooks and screen capture APIs.";

	public Task<string?> RecordAsync(bool verbose, CancellationToken cancellationToken)
	{
		if (!IsSupported)
		{
			throw new PlatformNotSupportedException(UnsupportedReason);
		}

		return Task.Run(() => (string?)RecordInternal(verbose, cancellationToken), cancellationToken);
	}

	public async Task SaveScreenshotAsync(CancellationToken cancellationToken)
	{
		if (!IsSupported)
		{
			throw new PlatformNotSupportedException(UnsupportedReason);
		}

		var filePath = await _fileDialogService.SaveFileAsync("Save Screenshot", "screenshot.bmp", BitmapFilter);
		if (string.IsNullOrWhiteSpace(filePath))
		{
			return;
		}

		using var screenshot = DesktopWindow.GetScreenshot();
		screenshot.Save(filePath);

		var process = new Process
		{
			StartInfo = new ProcessStartInfo(filePath)
			{
				UseShellExecute = true
			}
		};

		process.Start();
	}

	private static string RecordInternal(bool verbose, CancellationToken cancellationToken)
	{
		var session = new RecordingSession(verbose);

		for (var key = 1; key < 256; key++)
		{
			session.Threads[key] = key == 27
				? new Thread(ListenForStopAction)
				: new Thread(ListenAction);
		}

		session.Actions.Clear();
		session.ShouldListen = true;

		SpinWait.SpinUntil(() => !Keyboard.IsKeyDown(27));
		session.Stopwatch.Restart();

		for (var key = 1; key < 256; key++)
		{
			session.Threads[key].Start(new ListenerState(session, key, cancellationToken));
		}

		while (session.ShouldListen)
		{
			if (cancellationToken.IsCancellationRequested)
			{
				session.ShouldListen = false;
				break;
			}

			Thread.Sleep(25);
		}

		foreach (var thread in session.Threads.Where(thread => thread is not null))
		{
			thread.Join();
		}

		cancellationToken.ThrowIfCancellationRequested();
		return BuildRecordedCode(session.Actions, session.Verbose);
	}

	private static void ListenForStopAction(object? state)
	{
		if (state is not ListenerState listenerState)
		{
			return;
		}

		while (listenerState.Session.ShouldListen)
		{
			if (listenerState.CancellationToken.IsCancellationRequested)
			{
				listenerState.Session.ShouldListen = false;
				return;
			}

			if (Keyboard.IsKeyDown(27))
			{
				listenerState.Session.ShouldListen = false;
				return;
			}

			Thread.Sleep(5);
		}
	}

	private static void ListenAction(object? state)
	{
		if (state is not ListenerState listenerState)
		{
			return;
		}

		while (listenerState.Session.ShouldListen)
		{
			if (listenerState.CancellationToken.IsCancellationRequested)
			{
				listenerState.Session.ShouldListen = false;
				return;
			}

			if (Keyboard.IsKeyDown(listenerState.Key))
			{
				var position = Mouse.GetCursorPosition();

				listenerState.Session.Actions.Push(new TimedAction
				{
					Key = listenerState.Key,
					KeyDown = true,
					Miliseconds = listenerState.Session.Stopwatch.ElapsedMilliseconds,
					X = position.X,
					Y = position.Y
				});

				listenerState.Session.Stopwatch.Restart();

				while (listenerState.Session.ShouldListen && Keyboard.IsKeyDown(listenerState.Key))
				{
					if (listenerState.CancellationToken.IsCancellationRequested)
					{
						listenerState.Session.ShouldListen = false;
						return;
					}

					Thread.Sleep(5);
				}

				position = Mouse.GetCursorPosition();

				listenerState.Session.Actions.Push(new TimedAction
				{
					Key = listenerState.Key,
					KeyDown = false,
					Miliseconds = listenerState.Session.Stopwatch.ElapsedMilliseconds,
					X = position.X,
					Y = position.Y
				});

				listenerState.Session.Stopwatch.Restart();
			}

			Thread.Sleep(5);
		}
	}

	private static string BuildRecordedCode(ConcurrentStack<TimedAction> actions, bool verbose)
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

	private sealed class RecordingSession
	{
		public RecordingSession(bool verbose)
		{
			Verbose = verbose;
		}

		public Thread[] Threads { get; } = new Thread[256];
		public ConcurrentStack<TimedAction> Actions { get; } = [];
		public Stopwatch Stopwatch { get; } = Stopwatch.StartNew();
		public bool Verbose { get; }
		public volatile bool ShouldListen;
	}

	private sealed record ListenerState(RecordingSession Session, int Key, CancellationToken CancellationToken);
}