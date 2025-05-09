# RyoTune.Reloaded

Common functionality for Reloaded mods.

1. Install `RyoTune.Reloaded` using `NuGet`.
2. Create or add to a `GlobalUsings.cs` file the following: `global using RyoTune.Reloaded`
3. In your `Mod.cs` constructor, initialize functionality with: `Project.Initialize(IModConfig modConfig, IModLoader modLoader, ILogger log, bool useAsyncLog = false)`

Optionally, you can also manually set the color for `Information` log messages: `Project.Initialize(IModConfig modConfig, IModLoader modLoader, ILogger log, Color color, bool useAsyncLog = false)`

## SHFunction
`SHFunction` simplifies the creation of function hooks and/or wrappers, only requiring a function delegate and sig pattern to set up.

A hook can be easily added and enabled by supplying a function implementation in the constructor or separately with `SetHook`.

```csharp
// Basic function wrapper.
_bitGet = new SHFunction<BitGet>("8B C1 99 83 E2 1F 03 C2 8B C8 83 E0 1F 2B C2");
var bitValue = _bitGet.Wrapper(100); // You would use the wrapper *after* the sig scan had found the function, of course.

// Function wrapper and hook.
_bitGet = new SHFunction<BitGet>(BitGetImpl, "8B C1 99 83 E2 1F 03 C2 8B C8 83 E0 1F 2B C2");
var result = _bitGet.Hook!.OriginalFunction(100); // Within BitGetImpl to retrieve the original return value.

// Adding a hook separately, such as based on a condition.
_bitGet = new SHFunction<BitGet>("8B C1 99 83 E2 1F 03 C2 8B C8 83 E0 1F 2B C2");
if (_config.BitGetHookEnabled)
{
    _bitGet.SetHook(BitGetImpl); // Alternatively, you can pass null to disable hooking.
}
```

`SHFunction`'s are expected to be fully configured before scanning has started, while mods are loading, and cannot be configured after.

## ScanHooks
`ScanHooks` streamlines adding sigscans and creating a hooks from a successful result.

`ScanHooks.Add(string id, string? pattern, Action<IReloadedHooks, nint> success)`
- `id` is the id of the scan, similar to an ID. Used for logging, listeners, and pattern configuration.
- `pattern` is the sig pattern to search for.
- `success` is the callback to run, once found. Callbacks are given `IReloadedHooks` and the search result `nint`.

## String Extensions
It is very common to need to convert strings to pointers for use in native functions.
Extension methods have been added to quickly do so, in any encoding, and with caching the result.


```csharp
"Example".AsPointerAnsi();
"Example".AsPointerUni(true); // Uses thread-safe caching, in case of multi-threaded code.
"Example".AsPointer(Encoding.UTF8);
```

## Logging
Add functions for logging messages of various levels. `Information` messages will use the color set or one generated from the mod ID.
- `Log.Verbose`
- `Log.Debug`
- `Log.Information`
- `Log.Warning`
- `Log.Error`

The log level can be changed at any time through the `Log.LogLevel` property.
