# Macro Studio
Macro Studio is a desktop automation app for recording and replaying input macros. Use it to capture keyboard actions (and related automation steps), review and edit the sequence, then play it back to automate repetitive tasks.

## What this project is
- An Avalonia desktop shell in `src/MacroStudio.Avalonia` for Windows and Linux
- A shared cross-platform application layer in `src/MacroStudio.App`
- A Windows backend in `src/MacroStudio.Backend.Windows` that provides the Windows automation runtime
- A Linux backend in `src/MacroStudio.Backend.Linux` for X11/XWayland automation
- Core automation helpers in `src/MacroStudio.Core`
- MVVM infrastructure and view-model helpers in `src/MacroStudio.MVVM`

## Key features
- Record input actions into a macro
- Edit recorded actions (reorder, remove, adjust)
- Replay macros to automate monotonous workflows

## Repository layout
- `src/MacroStudio.Avalonia` – Avalonia UI application
- `src/MacroStudio.App` – shared app/view-model layer and platform abstractions
- `src/MacroStudio.Backend.Windows` – Windows-specific automation backend for the Avalonia app
- `src/MacroStudio.Backend.Linux` – Linux-specific automation backend for the Avalonia app
- `src/MacroStudio.Core` – core/recording/playback logic
- `src/MacroStudio.MVVM` – MVVM helpers
- `src/MacroStudio.Tests` – automated tests

## Current platform status
- Windows: the Avalonia app can load, save, edit, inspect cursor position, record input, capture screenshots, and execute macros using the new Windows backend.
- Linux: the Avalonia app can now inspect cursor position, record input, capture screenshots, and execute macros through the new Linux backend when running in an X11 or XWayland session with the XTEST extension available.
- Linux note: pure Wayland sessions without an accessible X11/XWayland `DISPLAY` are still unsupported for automation because global cursor and input synthesis are restricted there.

## Build
This repo targets .NET 10.

From the `src/` directory:
- Build the new Avalonia app: `dotnet build .\MacroStudio.Avalonia\MacroStudio.Avalonia.csproj -nologo`
- Run the Avalonia app on Linux: `dotnet run --project .\MacroStudio.Avalonia\MacroStudio.Avalonia.csproj -f net10.0`
- Run the Avalonia app on Windows: `dotnet run --project .\MacroStudio.Avalonia\MacroStudio.Avalonia.csproj -f net10.0-windows`
- Test: `dotnet test`

