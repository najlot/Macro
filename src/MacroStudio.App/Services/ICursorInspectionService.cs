namespace MacroStudio.App.Services;

public interface ICursorInspectionService
{
	bool IsSupported { get; }
	string UnsupportedReason { get; }
	(int X, int Y) GetCursorPosition();
	void SetCursorPosition(int x, int y);
}