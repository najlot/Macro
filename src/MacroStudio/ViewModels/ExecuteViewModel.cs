using MacroStudio.MVVM;
using MacroStudio.MVVM.ViewModel;
using System.Windows.Input;
using System.Windows;
using MacroStudio.Execution;
using System.IO;

namespace MacroStudio.ViewModels;

public class ExecuteViewModel : AbstractViewModel, ITabItem
{
	public bool IsSelected
	{
		get => field;
		set
		{
			if (Set(ref field, value))
			{
				if (value)
				{
					ExecutionUtils.Initialize();
				}
			}
		}
	}

	public string Code { get => field; set => Set(ref field, value); } = string.Empty;
	public int Executions { get => field; set => Set(ref field, value); } = 1;
    public bool IsRunButtonEnabled { get => field; set => Set(ref field, value); } = true;

    public ICommand RunCommand { get; }
	public ICommand SaveCommand { get; }
	public ICommand LoadCommand { get; }

	private readonly Action<bool> _showMainWindow;

	public ExecuteViewModel(Action<bool> showMainWindow)
	{
		_showMainWindow = showMainWindow;

		RunCommand = new AsyncCommand(
			RunAsync,
			task => ShowErrorAsync(task.Exception),
			() => IsRunButtonEnabled);

		SaveCommand = new AsyncCommand(
			SaveAsync,
			task => ShowErrorAsync(task.Exception));

		LoadCommand = new AsyncCommand(
			LoadAsync,
			task => ShowErrorAsync(task.Exception));
	}

	private Task ShowErrorAsync(Exception? ex)
	{
		MessageBox.Show(ex?.ToString() ?? "Unknown error.");
		return Task.CompletedTask;
	}

	private async Task SaveAsync()
	{
		var openFileDialog = new Microsoft.Win32.SaveFileDialog
		{
			DefaultExt = ".macro",
			FileName = "new.macro",
			Filter = "Macros (*.macro)|*.macro",
			FilterIndex = 1,
			RestoreDirectory = true
		};

		if (openFileDialog.ShowDialog() ?? false)
		{
			using var zipStream = new FileStream(openFileDialog.FileName, FileMode.Create);
			using var archive = new System.IO.Compression.ZipArchive(zipStream, System.IO.Compression.ZipArchiveMode.Create);

			{
				var codeEntry = archive.CreateEntry("code", System.IO.Compression.CompressionLevel.Optimal);
				using var codeStream = codeEntry.Open();
				using var codeWriter = new StreamWriter(codeStream);
				await codeWriter.WriteAsync(Code);
			}

			{
				var versionEntry = archive.CreateEntry("version", System.IO.Compression.CompressionLevel.NoCompression);
				using var versionStream = versionEntry.Open();
				using var versionWriter = new StreamWriter(versionStream);
				await versionWriter.WriteAsync("1");
			}

			{
				var executionsEntry = archive.CreateEntry("executions", System.IO.Compression.CompressionLevel.NoCompression);
				using var executionsStream = executionsEntry.Open();
				using var executionsWriter = new StreamWriter(executionsStream);
				await executionsWriter.WriteAsync(Executions.ToString());
			}
		}
	}

	private async Task LoadAsync()
	{
		var openFileDialog = new Microsoft.Win32.OpenFileDialog
		{
			Filter = "Macros (*.macro)|*.macro",
			FilterIndex = 1,
			RestoreDirectory = true
		};

		if (openFileDialog.ShowDialog() ?? false)
		{
			await LoadFromFileAsync(openFileDialog.FileName);
		}
	}

	public async Task LoadFromFileAsync(string filePath)
	{
		if (string.IsNullOrWhiteSpace(filePath))
		{
			return;
		}

		try
		{
			var (code, executions) = await MacroFile.ReadAsync(filePath);
			Code = code;
			Executions = executions;
		}
		catch (Exception ex)
		{
			MessageBox.Show(ex.Message);
		}
	}

	private async Task RunAsync()
	{
		await Task.Run(async () =>
		{
			try
			{
				IsRunButtonEnabled = false;

				var globals = new ScriptGlobals();
				var runner = ExecutionUtils.GetRunner(Code);

				if (runner is null)
				{
					MessageBox.Show("Failed to create script runner.");
					return;
				}

				_showMainWindow(false);

				for (int i = 0; i < Executions; i++)
				{
					await runner(globals);
				}
			}
			catch (OperationCanceledException)
			{
				Console.WriteLine("Operation canceled");
			}
			finally
			{
				IsRunButtonEnabled = true;
				_showMainWindow(true);
			}
		});
	}
}
