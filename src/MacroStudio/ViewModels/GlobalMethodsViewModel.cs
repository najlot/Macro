using MacroStudio.MVVM.ViewModel;

namespace MacroStudio.ViewModels;

public class GlobalMethodsViewModel : AbstractViewModel, ITabItem
{
	public bool IsSelected { get; set; }
	public string Code { get; set; } = @"/// <summary>
/// Gets the current text content from the clipboard.
/// </summary>
/// <returns>The clipboard text.</returns>
string GetClipboardText();

/// <summary>
/// Sets the specified text to the clipboard.
/// </summary>
/// <param name=""text"">The text to set on the clipboard.</param>
void SetClipboardText(string text);

/// <summary>
/// Captures a screenshot of the desktop.
/// </summary>
/// <returns>A <see cref=""Bitmap""/> containing the screenshot.</returns>
Bitmap GetScreenshot();

/// <summary>
/// Loads a bitmap image from the specified file path.
/// </summary>
/// <param name=""path"">The file path of the bitmap.</param>
/// <returns>The loaded <see cref=""Bitmap""/>.</returns>
Bitmap GetBitmap(string path);

/// <summary>
/// Loads a bitmap image of the specified resource.
/// </summary>
/// <param name=""path"">The name of the resourcep.</param>
/// <returns>The loaded <see cref=""Bitmap""/>.</returns>
Bitmap GetResourceBitmap(string name);

/// <summary>
/// Saves a bitmap image to the specified file path.
/// </summary>
/// <param name=""path"">The destination file path.</param>
/// <param name=""bitmap"">The <see cref=""Bitmap""/> to save.</param>
void SaveBitmap(string path, Bitmap bitmap);

/// <summary>
/// Determines if the specified small bitmap exists within the larger bitmap.
/// </summary>
/// <param name=""smallBmp"">The bitmap to search for.</param>
/// <param name=""bigBmp"">The bitmap to search within.</param>
/// <param name=""startX"">The X coordinate to start searching from.</param>
/// <param name=""startY"">The Y coordinate to start searching from.</param>
/// <returns>True if the small bitmap is found; otherwise, false.</returns>
bool HasBitmap(Bitmap smallBmp, Bitmap bigBmp, int startX = 0, int startY = 0);

/// <summary>
/// Determines if the specified small bitmap exists within the larger bitmap, using a tolerance value.
/// </summary>
/// <param name=""smallBmp"">The bitmap to search for.</param>
/// <param name=""bigBmp"">The bitmap to search within.</param>
/// <param name=""tolerance"">The allowed color difference tolerance (0 = exact match, 0.1 = 10%, 1 = 100%).</param>
/// <param name=""startX"">The X coordinate to start searching from.</param>
/// <param name=""startY"">The Y coordinate to start searching from.</param>
/// <returns>True if the small bitmap is found; otherwise, false.</returns>
bool HasBitmap(Bitmap smallBmp, Bitmap bigBmp, double tolerance, int startX = 0, int startY = 0);

/// <summary>
/// Searches for the specified small bitmap within the larger bitmap.
/// </summary>
/// <param name=""smallBmp"">The bitmap to search for.</param>
/// <param name=""bigBmp"">The bitmap to search within.</param>
/// <param name=""startX"">The X coordinate to start searching from.</param>
/// <param name=""startY"">The Y coordinate to start searching from.</param>
/// <returns>
/// A <see cref=""Rectangle""/> representing the location of the found bitmap,
/// or <see cref=""Rectangle.Empty""/> if not found.
/// </returns>
Rectangle SearchBitmap(Bitmap smallBmp, Bitmap bigBmp, int startX = 0, int startY = 0);

/// <summary>
/// Searches for the specified small bitmap within the larger bitmap, using a tolerance value.
/// </summary>
/// <param name=""smallBmp"">The bitmap to search for.</param>
/// <param name=""bigBmp"">The bitmap to search within.</param>
/// <param name=""tolerance"">The allowed color difference tolerance (0 = exact match, 0.1 = 10%, 1 = 100%).</param>
/// <param name=""startX"">The X coordinate to start searching from.</param>
/// <param name=""startY"">The Y coordinate to start searching from.</param>
/// <returns>
/// A <see cref=""Rectangle""/> representing the location of the found bitmap,
/// or <see cref=""Rectangle.Empty""/> if not found.
/// </returns>
Rectangle SearchBitmap(Bitmap smallBmp, Bitmap bigBmp, double tolerance, int startX = 0, int startY = 0);

/// <summary>
/// Simulates a keyboard or mouse input event with the specified parameters.
/// </summary>
/// <param name=""key"">The key code or mouse button to simulate.</param>
/// <param name=""x"">The X coordinate for the event.</param>
/// <param name=""y"">The Y coordinate for the event.</param>
/// <param name=""waitTime"">The time to wait before the event, in milliseconds.</param>
void Simulate(int key, int x, int y, int waitTime);

/// <summary>
/// Simulates a keyboard or mouse input event with the specified parameters.
/// </summary>
/// <param name=""key"">The key code or mouse button to simulate.</param>
/// <param name=""x"">The X coordinate for the event.</param>
/// <param name=""y"">The Y coordinate for the event.</param>
/// <param name=""down"">True for key/button down, false for up.</param>
/// <param name=""waitTime"">The time to wait before the event, in milliseconds.</param>
void Simulate(int key, int x, int y, bool down, int waitTime);

/// <summary>
/// Waits for the specified number of milliseconds.
/// </summary>
/// <param name=""milliseconds"">The number of milliseconds to wait.</param>
void Wait(int milliseconds);

/// <summary>
/// Waits for the specified number of milliseconds.
/// </summary>
/// <param name=""milliseconds"">The number of milliseconds to wait.</param>
void WaitMiliseconds(int milliseconds);

/// <summary>
/// Waits for the specified number of seconds.
/// </summary>
/// <param name=""seconds"">The number of seconds to wait.</param>
void WaitSeconds(int seconds);

/// <summary>
/// Waits for the specified number of minutes.
/// </summary>
/// <param name=""minutes"">The number of minutes to wait.</param>
void WaitMinutes(int minutes);

/// <summary>
/// Waits for the specified number of hours.
/// </summary>
/// <param name=""hours"">The number of hours to wait.</param>
void WaitHours(int hours);

/// <summary>
/// Sets the cursor position to the specified screen coordinates.
/// </summary>
/// <param name=""x"">The X coordinate to set the cursor to.</param>
/// <param name=""y"">The Y coordinate to set the cursor to.</param>
void SetCursorPosition(int x, int y);

/// <summary>
/// Smoothly moves the cursor to the specified screen coordinates over a given duration.
/// </summary>
/// <param name=""x"">The target X coordinate.</param>
/// <param name=""y"" > The target Y coordinate.</param>
/// <param name=""ms"">The duration of the movement in milliseconds.</param>
void MoveCursorTo(int x, int y, int ms);

/// <summary>
/// Simulates pressing down the left mouse button.
/// </summary>
void MouseLeftDown();

/// <summary>
/// Simulates releasing the left mouse button.
/// </summary>
void MouseLeftUp();

/// <summary>
/// Simulates pressing down the middle mouse button.
/// </summary>
void MouseMiddleDown();

/// <summary>
/// Simulates releasing the middle mouse button.
/// </summary>
void MouseMiddleUp();

/// <summary>
/// Simulates pressing down the right mouse button.
/// </summary>
void MouseRightDown();

/// <summary>
/// Simulates releasing the right mouse button.
/// </summary>
void MouseRightUp();

/// <summary>
/// Simulates pressing a keyboard key.
/// </summary>
/// <param name=""key"">The key code of the key to press.</param>
void PressKeyboardKey(byte key);

/// <summary>
/// Simulates releasing a keyboard key.
/// </summary>
/// <param name=""key"">The key code of the key to release.</param>
void ReleaseKeyboardKey(byte key);";
}
