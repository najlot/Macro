# Macro Studio
Macro Studio is a desktop automation app for recording and replaying input macros. Use it to capture keyboard actions (and related automation steps), review and edit the sequence, then play it back to automate repetitive tasks.

## What this project is
- A Windows desktop application (UI in `src/MacroStudio`)
- Core automation/recording logic in `src/MacroStudio.Core`
- MVVM infrastructure and view-model helpers in `src/MacroStudio.MVVM`

## Key features
- Record input actions into a macro
- Edit recorded actions (reorder, remove, adjust)
- Replay macros to automate monotonous workflows

## Repository layout
- `src/MacroStudio` – UI application
- `src/MacroStudio.Core` – core/recording/playback logic
- `src/MacroStudio.MVVM` – MVVM helpers
- `src/MacroStudio.Tests` – automated tests

## Build
This repo targets .NET 10.

From the `src/` directory:
- Build: `dotnet build`
- Run: `dotnet run -c Release`
- Test: `dotnet test`

