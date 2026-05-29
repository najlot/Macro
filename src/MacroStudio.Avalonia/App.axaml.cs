using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using MacroStudio.App.Services;
using MacroStudio.App.ViewModels;
using MacroStudio.Avalonia.Platform;
using MacroStudio.Avalonia.Services;
using MacroStudio.Avalonia.Views;

namespace MacroStudio.Avalonia;

public partial class App : Application
{
	public override void Initialize()
	{
		AvaloniaXamlLoader.Load(this);
	}

	public override void OnFrameworkInitializationCompleted()
	{
		if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
		{
			var windowContext = new MainWindowContext();
			IMainWindowService mainWindowService = new MainWindowService(windowContext);
			IFileDialogService fileDialogService = new DesktopFileDialogService(windowContext);
			IUserNotificationService notificationService = new DesktopNotificationService(windowContext);
			var platformServices = PlatformServiceFactory.Create(fileDialogService);

			var mainViewModel = new MainViewModel(
				new RecordViewModel(platformServices.RecordingService, notificationService, mainWindowService),
				new ExecuteViewModel(new MacroFileService(), fileDialogService, notificationService, mainWindowService, platformServices.ExecutionService),
				new InspectViewModel(platformServices.CursorInspectionService),
				new GlobalMethodsViewModel());

			var mainWindow = new MainWindow
			{
				DataContext = mainViewModel
			};

			windowContext.MainWindow = mainWindow;
			desktop.MainWindow = mainWindow;

			_ = TryLoadFromCommandLineAsync(desktop.Args ?? [], mainViewModel);
		}

		base.OnFrameworkInitializationCompleted();
	}

	private static async Task TryLoadFromCommandLineAsync(string[] args, MainViewModel mainViewModel)
	{
		var filePath = args
			.FirstOrDefault(arg => !string.IsNullOrWhiteSpace(arg) && File.Exists(arg));

		if (filePath is null)
		{
			return;
		}

		mainViewModel.SelectedTabIndex = 1;
		await mainViewModel.ExecuteViewModel.LoadFromFileAsync(filePath);
	}
}