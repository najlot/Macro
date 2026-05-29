using System.Collections.Concurrent;
using System.Diagnostics;
using MacroStudio.App.Services;
using MacroStudio.Backend.Linux.Execution;

namespace MacroStudio.Backend.Linux.Services;

public sealed class LinuxMacroRecordingService : IMacroRecordingService
{
	private static readonly FileDialogFilter ScreenshotFilter = new("Images", "*.png");
	private static readonly int[] SupportedKeys = LinuxMacroKeyMap.SupportedMacroKeys.Where(key => key != 27).ToArray();
	private readonly IFileDialogService _fileDialogService;

	public LinuxMacroRecordingService(IFileDialogService fileDialogService)
	{
		_fileDialogService = fileDialogService;
	}

	public bool IsSupported => X11AutomationContext.TryGetSupportState(requireXTest: true, out _);

	public string UnsupportedReason
		=> X11AutomationContext.TryGetSupportState(requireXTest: true, out var reason)
			? string.Empty
			: reason;

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

		var filePath = await _fileDialogService.SaveFileAsync("Save Screenshot", "screenshot.png", ScreenshotFilter);
		if (string.IsNullOrWhiteSpace(filePath))
		{
			return;
		}

		using var automation = X11AutomationContext.Open(requireXTest: true);
		using var screenshot = automation.CaptureScreenshot();
		screenshot.Save(filePath);

		TryOpenFile(filePath);
	}

	private static string RecordInternal(bool verbose, CancellationToken cancellationToken)
	{
		using var automation = X11AutomationContext.Open(requireXTest: true);
		var actions = new ConcurrentStack<TimedAction>();
		var previousStates = SupportedKeys.ToDictionary(key => key, _ => false);
		var previousLeft = false;
		var previousRight = false;
		var previousMiddle = false;
		var stopwatch = Stopwatch.StartNew();

		SpinWait.SpinUntil(() => !automation.IsMacroKeyDown(27));
		stopwatch.Restart();

		while (true)
		{
			cancellationToken.ThrowIfCancellationRequested();

			var keymap = automation.QueryKeymap();
			if (automation.IsMacroKeyDown(27, keymap))
			{
				break;
			}

			var pointer = automation.QueryPointer();
			RecordTransition(actions, stopwatch, 1, pointer.IsLeftButtonDown, ref previousLeft, pointer.X, pointer.Y);
			RecordTransition(actions, stopwatch, 2, pointer.IsRightButtonDown, ref previousRight, pointer.X, pointer.Y);
			RecordTransition(actions, stopwatch, 4, pointer.IsMiddleButtonDown, ref previousMiddle, pointer.X, pointer.Y);

			foreach (var key in SupportedKeys)
			{
				var currentState = automation.IsMacroKeyDown(key, keymap);
				var previousState = previousStates[key];

				if (currentState == previousState)
				{
					continue;
				}

				actions.Push(new TimedAction
				{
					Key = key,
					KeyDown = currentState,
					Miliseconds = stopwatch.ElapsedMilliseconds,
					X = pointer.X,
					Y = pointer.Y
				});

				stopwatch.Restart();
				previousStates[key] = currentState;
			}

			Thread.Sleep(5);
		}

		return RecordedMacroFormatter.Build(actions, verbose);
	}

	private static void RecordTransition(ConcurrentStack<TimedAction> actions, Stopwatch stopwatch, int key, bool currentState, ref bool previousState, int x, int y)
	{
		if (currentState == previousState)
		{
			return;
		}

		actions.Push(new TimedAction
		{
			Key = key,
			KeyDown = currentState,
			Miliseconds = stopwatch.ElapsedMilliseconds,
			X = x,
			Y = y
		});

		stopwatch.Restart();
		previousState = currentState;
	}

	private static void TryOpenFile(string filePath)
	{
		try
		{
			Process.Start(new ProcessStartInfo("xdg-open", filePath)
			{
				UseShellExecute = false,
				CreateNoWindow = true
			});
		}
		catch
		{
		}
	}
}