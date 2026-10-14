Run the regression checks with Godot 4.7.2 .NET after building the C# project:

```powershell
dotnet build 'Donnas Adventure.csproj'
godot --headless --path . --script res://Tests/optimization_checks.gd
```

Replace `godot` with the path to your Godot .NET executable if it is not on PATH.
For isolated save data, set `APPDATA` to a temporary directory for this command.

The checks cover particle cleanup, offscreen effect lifetime, 20 platform cycles,
startup velocity, HUD timer boundaries and resets, cached player controls and
materials, special-treat rewards and duplicate entry, menu music and awaited
sound playback, and scene teardown during pending tutorial and blink callbacks.
The process returns a nonzero exit code if a check fails.

Headless Godot does not simulate GPU particles, so the suite drives their native
completion signals to test cleanup and separately checks the timeout fallback.
Rendered particle timing and how it feels to ride moving platforms still need
an interactive playthrough.

See [GRAPHICS.md](GRAPHICS.md) for the rendered GPU benchmark and graphics changes.
