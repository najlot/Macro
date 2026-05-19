using MacroStudio.Core;
using MacroStudio.Execution;
using MacroStudio.MVVM;
using MacroStudio.MVVM.ViewModel;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Windows.Threading;

namespace MacroStudio.ViewModels;

public class RecordViewModel : AbstractViewModel, ITabItem
{
	private readonly Thread[] threads = new Thread[256];
	private readonly ConcurrentStack<TimedAction> _actions = [];
	private readonly Stopwatch _stopwatch = Stopwatch.StartNew();
	private volatile bool _shouldListen = false;
	
	public bool IsSelected { get; set; }
	public bool Verbose { get; set; } = true;

	public Action<bool> ShowMainWindow { get; }

	public RecordViewModel(Action<bool> showMainWindow)
	{
		ShowMainWindow = showMainWindow;

		StartRecordingCommand = new RelayCommand(async () =>
		{
			IsRecordEnabled = false;
			await Task.Run(OnStartRecording);
		});

        SaveScreenshotCommand = new RelayCommand(SaveScreenshot);
    }

	private void SaveScreenshot()
	{
		var saveFileDialog = new Microsoft.Win32.SaveFileDialog
		{
            DefaultExt = ".bmp",
            Filter = "Images (*.bmp)|*.bmp",
            FilterIndex = 1,
            RestoreDirectory = true,
        };

		if (saveFileDialog.ShowDialog() == true)
		{
			var filePath = saveFileDialog.FileName;
            using var screenshot = DesktopWindow.GetScreenshot();
			screenshot.Save(filePath);

            // Open in MS-Paint for editing
            var process = new Process
            {
                StartInfo = new ProcessStartInfo("mspaint.exe")
                {
                    UseShellExecute = true,
                }
            };

            process.StartInfo.ArgumentList.Add(filePath);
            process.Start();
        }
    }

    public string Code
	{
		get => field;
		set => Set(ref field, value);
	} = string.Empty;

	public bool IsRecordEnabled
	{
		get => field;
		set => Set(ref field, value);
	} = true;

	public RelayCommand StartRecordingCommand { get; }
	public RelayCommand SaveScreenshotCommand { get; }

	private void ListenForStopAction(object? _)
	{
		while (_shouldListen)
		{
			if (Keyboard.IsKeyDown(27))
			{
				OnStopRecording();
			}

			Thread.Sleep(5);
		}
	}

	private void ListenAction(object? state)
	{
		if (state is not int key)
		{
			return;
		}

		while (_shouldListen)
		{
			if (Keyboard.IsKeyDown(key))
			{
				var pos = Mouse.GetCursorPosition();

				_actions.Push(new TimedAction()
				{
					Key = key,
					KeyDown = true,
					Miliseconds = _stopwatch.ElapsedMilliseconds,
					X = pos.X,
					Y = pos.Y
				});

				_stopwatch.Restart();

				while (Keyboard.IsKeyDown(key))
				{
					Thread.Sleep(5);
				}

				pos = Mouse.GetCursorPosition();

				_actions.Push(new TimedAction()
				{
					Key = key,
					KeyDown = false,
					Miliseconds = _stopwatch.ElapsedMilliseconds,
					X = pos.X,
					Y = pos.Y
				});

				_stopwatch.Restart();
			}

			Thread.Sleep(5);
		}
	}

	private void OnStopRecording()
	{
		_shouldListen = false;
		ShowMainWindow(true);

		var actionsList = _actions.ToList();
		actionsList.Reverse();

		string outputCode = string.Join("\r\n", actionsList.Select(a =>
		{
			var down = a.KeyDown.ToString().ToLower();
			
			if (Verbose && a.Key == 1)
			{
				var command = $"MoveCursorTo({a.X}, {a.Y}, {a.Miliseconds});\r\n";

				if (a.KeyDown)
				{
					command += "MouseLeftDown();";
				}
				else
				{
					command += "MouseLeftUp();";
				}

				return command;
			}
			else if (Verbose && a.Key == 2)
			{
				var command = $"MoveCursorTo({a.X}, {a.Y}, {a.Miliseconds});\r\n";

				if (a.KeyDown)
				{
					command += "MouseRightDown();";
				}
				else
				{
					command += "MouseRightUp();";
				}

				return command;
			}
			else if (Verbose && a.Key == 4)
			{
				var command = $"MoveCursorTo({a.X}, {a.Y}, {a.Miliseconds});\r\n";

				if (a.KeyDown)
				{
					command += "MouseMiddleDown();";
				}
				else
				{
					command += "MouseMiddleUp();";
				}

				return command;
			}
			else if (Verbose && a.Key >= 32 && a.Key <= 125)
			{
				var command = $"MoveCursorTo({a.X}, {a.Y}, {a.Miliseconds});\r\n";

				if (a.KeyDown)
				{
					command += $"PressKeyboardKey({a.Key} /*{(char)a.Key}*/);";
				}
				else
				{
					command += $"ReleaseKeyboardKey({a.Key} /*{(char)a.Key}*/);";
				}

				return command;
			}

			return $"Simulate({a.Key}, {a.X}, {a.Y}, {down}, {a.Miliseconds});";
		}));

		Dispatcher.CurrentDispatcher.Invoke(() =>
		{
			Code = outputCode;
			IsRecordEnabled = true;
		});
	}

	private void OnStartRecording()
	{
		for (int i = 1; i < 256; i++)
		{
			if (i == 27)
			{
				threads[i] = new Thread((x) => ListenForStopAction(x));
			}
			else
			{
				threads[i] = new Thread((x) => ListenAction(x));
			}
			
		}

		_actions.Clear();

		_shouldListen = true;

		SpinWait.SpinUntil(() => !Keyboard.IsKeyDown(27));

		_stopwatch.Restart();

		for (int i = 1; i < 256; i++)
		{
			threads[i].Start(i);
		}

		_stopwatch.Restart();
		ShowMainWindow(false);
	}
}
