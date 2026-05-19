using MacroStudio.MVVM;
using MacroStudio.MVVM.ViewModel;
using System.Windows.Input;
using System.Windows;
using MacroStudio.Execution;
using System.IO;
using System.Collections.ObjectModel;

namespace MacroStudio.ViewModels;

public class ResourceViewModel : AbstractViewModel
{
    public string Name { get => field; set => Set(ref field, value); } = string.Empty;
    public byte[] Value { get; set; } = [];
}

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

	public ObservableCollection<ResourceViewModel> Resources { get; } = [];

    public ICommand RunCommand { get; }
	public ICommand SaveCommand { get; }
	public ICommand LoadCommand { get; }

    public ICommand AddResourceCommand { get; }
    public ICommand RemoveResourceCommand { get; }

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

        AddResourceCommand = new RelayCommand(AddResource);
        RemoveResourceCommand = new RelayCommand<ResourceViewModel>(RemoveResource);

    }

	private Task ShowErrorAsync(Exception? ex)
	{
		MessageBox.Show(ex?.ToString() ?? "Unknown error.");
		return Task.CompletedTask;
	}

	private void AddResource()
	{
		var openFileDialog = new Microsoft.Win32.OpenFileDialog()
		{
            DefaultExt = ".bmp",
            Filter = "Images (*.bmp)|*.bmp",
            FilterIndex = 1,
            RestoreDirectory = true,
		};

        if (openFileDialog.ShowDialog() ?? false)
		{
			try
			{
				var filePath = openFileDialog.FileName;
				var name = Path.GetFileNameWithoutExtension(filePath);
				var value = File.ReadAllBytes(filePath);

				int count = 1;
				var newName = name;

				while (Resources.Any(r => r.Name.Equals(newName, StringComparison.OrdinalIgnoreCase)))
				{
					count++;
                    newName = name + " " + count;
                }

				Resources.Add(new ResourceViewModel { Name = newName, Value = value });
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
            }
        }
    }

    private void RemoveResource(ResourceViewModel vm)
    {
        Resources.Remove(vm);
    }

    private async Task SaveAsync()
	{
		var saveFileDialog = new Microsoft.Win32.SaveFileDialog
		{
			DefaultExt = ".macro",
			FileName = "new.macro",
			Filter = "Macros (*.macro)|*.macro",
			FilterIndex = 1,
			RestoreDirectory = true
		};

		if (saveFileDialog.ShowDialog() ?? false)
		{
			using var zipStream = new FileStream(saveFileDialog.FileName, FileMode.Create);
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
				await versionWriter.WriteAsync("2");
			}

			{
				var executionsEntry = archive.CreateEntry("executions", System.IO.Compression.CompressionLevel.NoCompression);
				using var executionsStream = executionsEntry.Open();
				using var executionsWriter = new StreamWriter(executionsStream);
				await executionsWriter.WriteAsync(Executions.ToString());
			}

			foreach (var resource in Resources)
			{
				var resourceEntry = archive.CreateEntry("resources/" + resource.Name, System.IO.Compression.CompressionLevel.Optimal);
				using var resourceStream = resourceEntry.Open();
				await resourceStream.WriteAsync(resource.Value, 0, resource.Value.Length);
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
			var (code, executions, resources) = await MacroFile.ReadAsync(filePath);
			Code = code;
			Executions = executions;

			foreach (var resource in resources)
			{
				Resources.Add(resource);
            }
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
				
				foreach (var resource in Resources)
				{
					globals.Resources[resource.Name] = resource.Value;
                }

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
