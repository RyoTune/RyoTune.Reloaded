# RyoTune.Reloaded

Common functionality for Reloaded mods.

1. Install `RyoTune.Reloaded` using `Nuget`.
2. Create or add to a `GlobalUsings.cs` file the following: `global using RyoTune.Reloaded`
3. In your `Mod.cs` constructor, initialize functionality with: `Project.Init(IModConfig, IModLoader, ILogger)`

Optionally, you can also manually set the color for `Information` log messages and whether to use async logging.

## ScanHooks
`ScanHooks` streamlines adding sigscans and creating a hook if found.

`ScanHooks.Add(string name, string? pattern, Action<IReloadedHooks, nint> success)`
- `name` is the name of the scan, similar to an ID. Used for logging and by listeners.
- `pattern` is the sig pattern to search for.
- `success` is the callback to run, once found. Callbacks are given `IReloadedHooks` and the search result `nint`.

## Logging
Add functions for logging messages of various levels. `Information` messages will use the color set or one generated from the mod ID.
- `Log.Verbose`
- `Log.Debug`
- `Log.Information`
- `Log.Warning`
- `Log.Error`

The log level can be changed at any time through the `Log.LogLevel` property.
