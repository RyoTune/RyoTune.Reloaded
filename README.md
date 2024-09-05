# RyoTune.Reloaded

Common functionality for Reloaded mods.

1. Install `RyoTune.Reloaded` using `Nuget`.
2. Create or add to a `GlobalUsings.cs` file the following: `global using RyoTune.Reloaded`
3. In your `Mod.cs` constructor, initialize functionality with: `Project.Init(IModConfig, IModLoader, ILogger)`

Optionally, you can also manually set the color for `INFO` log messages and whether to use async logging.

## ScanHooks
`ScanHooks` simplify adding signature scans with the ability to create a hook if found.
Use the static method `ScanHooks.Add(string name, string? pattern, Action<IReloadedHooks, nint> success)`
- `name` is the name of the pattern for logging and listeners.
- `pattern` is the sig pattern to search for.
- `success` is the callback to run, once found. Callbacks are given `IReloadedHooks` and the search result `nint`.

## Logging
Adds static methods for logging messages with specific log levels, as well as exception messages.
- `Log.Verbose`
- `Log.Debug`
- `Log.Information`
- `Log.Warning`
- `Log.Error`

The log level can be changed at any time through the `Log.LogLevel` property.
