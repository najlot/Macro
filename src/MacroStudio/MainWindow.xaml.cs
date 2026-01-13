using AdonisUI;
using MacroStudio.ViewModels;
using System.Windows;

namespace MacroStudio;

public partial class MainWindow
{
	public MainViewModel MainViewModel { get; }

	public MainWindow()
	{
		InitializeComponent();

		AdonisUI.ResourceLocator.SetColorScheme(Application.Current.Resources, ResourceLocator.DarkColorScheme);

		MainViewModel = new MainViewModel(show =>
		{
			Dispatcher.Invoke(() =>
			{
				if (show)
				{
					Visibility = Visibility.Visible;
				}
				else
				{
					Visibility = Visibility.Hidden;
				}
			});
		});

		DataContext = MainViewModel;

        TryLoadFromCommandLine();
    }

	private async void TryLoadFromCommandLine()
	{
        var args = Environment.GetCommandLineArgs().Skip(1).ToArray();
        if (args is not { Length: > 0 })
        {
            return;
        }

        string? filePath = null;
        foreach (var arg in args)
        {
            if (!string.IsNullOrWhiteSpace(arg) && System.IO.File.Exists(arg))
            {
                filePath = arg;
                break;
            }
        }

        if (filePath is null)
        {
            return;
        }

        MainViewModel.SelectedTabIndex = 1;
        await MainViewModel.ExecuteViewModel.LoadFromFileAsync(filePath);
    }

	private void TabSelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
	{
		foreach (var item in e.RemovedItems)
		{
			if (item is ITabItem tabItem)
			{
				tabItem.IsSelected = false;
			}
			else if (item is FrameworkElement element && element.DataContext is ITabItem tabItemViewModel)
			{
				tabItemViewModel.IsSelected = false;
			}
		}

		foreach (var item in e.AddedItems)
		{
			if (item is ITabItem tabItem)
			{
				tabItem.IsSelected = true;
			}
			else if (item is FrameworkElement element && element.DataContext is ITabItem tabItemViewModel)
			{
				tabItemViewModel.IsSelected = true;
			}
		}
	}
}