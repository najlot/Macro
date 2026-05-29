using MacroStudio.App.Services;
using MacroStudio.Avalonia.Services;

#if WINDOWS
using MacroStudio.Backend.Windows.Services;
#else
using MacroStudio.Backend.Linux.Services;
#endif

namespace MacroStudio.Avalonia.Platform;

internal static class PlatformServiceFactory
{
	public static PlatformServices Create(IFileDialogService fileDialogService)
	{
#if WINDOWS
		return new PlatformServices(
			new WindowsMacroExecutionService(),
			new WindowsMacroRecordingService(fileDialogService),
			new WindowsCursorInspectionService());
#else
		return new PlatformServices(
			new LinuxMacroExecutionService(),
			new LinuxMacroRecordingService(fileDialogService),
			new LinuxCursorInspectionService());
#endif
	}
}

internal sealed record PlatformServices(
	IMacroExecutionService ExecutionService,
	IMacroRecordingService RecordingService,
	ICursorInspectionService CursorInspectionService);