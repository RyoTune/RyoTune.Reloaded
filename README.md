
# RyoTune.Reloaded

[![NuGet](https://img.shields.io/nuget/v/RyoTune.Reloaded)](https://img.shields.io/nuget/v/RyoTune.Reloaded)
[![GitHub last commit](https://img.shields.io/github/license/RyoTune/RyoTune.Reloaded)](https://img.shields.io/github/license/RyoTune/RyoTune.Reloaded)

My persona modding library for making Reloaded mods. 

Contains general-purpose functionality useful in any Reloaded mod, such as logging with levels, simplified sig scanning and hooking, and more. 

# Installation

1. Install `RyoTune.Reloaded` using `NuGet`.
2. Create or add to a `GlobalUsings.cs` file the following: `global using RyoTune.Reloaded;`
3. In your `Mod.cs` constructor, initialize the library with: `Project.Initialize(IModConfig modConfig, IModLoader modLoader, ILogger log, bool useAsyncLog = false)`

Optionally, you can manually set the color for `Information` log messages with: 
````
Project.Initialize(IModConfig modConfig, IModLoader modLoader, ILogger log, Color color, bool useAsyncLog = false)
````

# Features
## SHFunction
`SHFunction` simplifies the creation of function hooks and/or wrappers, only requiring a function delegate and sig pattern to set up.

A hook can be easily added and enabled by supplying a function implementation in the constructor or separately with `SetHook`.

`SHFunction` uses a `ScanHook`, hence the `SH` in the name, and supports pattern configuration (see below).

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

## INI Configs
Mods have access to an easy to use, but versatile, INI configuration system (`Project.Inis`).

INI settings can have a defined default value, as well as a game-specific value. A mod's INI settings can also be provided or overwritten by an external mod in the same way.

### Usage
Mod INIs should be placed at: `MOD_FOLDER/Project/MOD_ID/`

**Example:** `MOD_FOLDER/Project/UE.Toolkit.Reloaded/scans.ini`

Game-specific mod INIs should be placed at: `MOD_FOLDER/Project/MOD_ID/APP_ID`

**Example:** `MOD_FOLDER/Project/UE.Toolkit.Reloaded/p3r.exe/scans.ini`

---

There are 3 pieces needed to retrieve a setting:
- **INI ID** - Essentially equivalent to the INI file name.
- **Setting Name** - Name of setting to retrieve.
- **Setting Section** - INI section to retrieve setting from. For **global settings** (not within a section), use `null`.

**Code**
```csharp
var setting = GetSetting("config", "Name", "Player"); // setting = Player 1
```

**INI** 
```config.ini
[Player]
Name=Player 1
```

### External Mod Support
To support loading settings from external mods it's required to wrap the code using the setting within a callback, since the value may update at any point as mods load.

**Code**
```csharp
private string? _name; // Current value.

// Set current value to newest value.
UsingSetting("config", "Name", "Player", newValue => _name = newValue); 
```

## Scans
The scanning service (`Project.Scans`) provides simplified methods for adding sigscans which can also be fully (re)configured through the INI system.

### Usage
All scans include the following:
- **Scan ID** - ID of the scan whose result to receive. Can be reused as needed.
- **Success Callback** - Callback given the scan result if the scan was successful.
- **Failure Callback (Optional)** - Callback run if the scan failed. If not provided, the failed scan will be logged as an error.

#### Scan Types
##### Scan with Pattern
The most common type of scan, which includes a pattern in code.

```csharp
// AddScan(string id, string? pattern, Action<nint> onSuccess, Action? onFail = null);
Project.Scans.AddScan("GUObjectArray", "48 8B 05 ?? ?? ?? ?? 48 8B 0C", result => { });
```

##### Scan without Pattern
If a pattern will always be provided through the **Scan INI**, you can add a scan without one.

```csharp
// AddScan(string id, Action<nint> onSuccess, Action? onFail = null);
Project.Scans.AddScan("GUObjectArray", result => { });
```

##### Scan with Default Result
During mod development, it can be tedious to generate sig patterns constantly.
This method allows for using hardcoded addresses which can later be overwritten with a normal sigscan through the **Scan INI**.

```csharp
// AddScan(string id, nint defaultResult, Action<nint> onSuccess, Action? onFail = null);
Project.Scans.AddScan("GUObjectArray", 0x14000000, result => { });
```

##### Scan Hooks
Each of the above methods include an alternative which provides both the **result** and **Reloaded Hooks** to the success callback.
```csharp
// AddScanHook(string id, string? pattern, Action<nint, IReloadedHooks> onSuccess, Action? onFail = null);
Project.Scans.AddScanHook("GUObjectArray", "48 8B 05 ?? ?? ?? ?? 48 8B 0C", (result, hooks) => { });
```

### Scan INI
Every scan be configured through an INI config. This include the full functionality of the INI system, such as per-game settings and external mods.

The default **Scan INI** can be found (or created) at: `MOD_FOLDER/Project/MOD_ID/scans.ini`

All settings should be in a `Scans` section, as shown.

```scans.ini
[Scans]
ExampleSetting=0
```

#### Set a Scan Pattern
To set a scan's pattern, add setting of the **Scan ID** equal to the pattern.

```scans.ini
[Scans]
GUObjectArray=48 8B 05 ?? ?? ?? ?? 48 8B 0C ?? 48 8D 04 ?? 48 85 C0 74 ?? 44 39 40 ?? 75 ?? F7 40 ?? 00 00 00 30 75 ?? 48 8B 00
````

#### Adjust Scan Result Before Use
Sometimes the scan result is not the final value you want to use. While you can adjust it in code, doing so restricts patterns to ones that account for those changes.

To ensure scans can be fully reconfigured, all scans can have a **result expression** for the initial result to go through first.

A scan's **result expression** setting is the **Scan ID** appended with `_RESULT`.

```scans.ini
[Scans]
GUObjectArray=48 8B 05 ?? ?? ?? ?? 48 8B 0C ?? 48 8D 04 ?? 48 85 C0 74 ?? 44 39 40 ?? 75 ?? F7 40 ?? 00 00 00 30 75 ?? 48 8B 00

GUObjectArray_RESULT=GetGlobalAddress(result + 3) - 0x10
````

**Variables**

**result** - The initial result.

**Functions**

**GetGlobalAddress** - Returns the absolute address from a pointer to a relative address.

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

## Mod Dependency Requirement
**tl;dr:** Use `Project.IsModDependent(IModConfigV1 mod)` to check if a mod has a dependency on your mod, including as a sub-dependency.

### Reasoning
If your mod is meant to be used for creating other mods, it's highly recommended to require those mods to add a **Mod Dependency** on yours. This makes sure that Reloaded always loads the mods in the correct order, and can automatically download your mod from those other mods.

However, this can cause some confusion when mods depend on those other mods but can't use the functionality from your mod, since it lacks a direct dependency.

### Problem
**Mod A** adds support for audio replacement, with **Mod B** using it to replace voice lines.

Someone wanting to replace some of **Mod B's** voice lines might try copying the file and folder structure in their own **Mod C**, using their files, and with a **Mod Dependency** on **Mod B**. 

Unfortunately, if **Mod A** only checks for a direct dependency, then **Mod C's** files will be ignored. While **Mod C** could just add another dependency on **Mod A**, this can be a bit non-obvious without help.

### Solution
**Mod A** should require a **Mod Dependency**, but also include mods which have it as a sub-dependency. This can be checked for with: `Project.IsModDependent(IModConfigV1 mod)`

In the example above, since **Mod C** depends on **Mod B**, and **Mod B** depends on **Mod A**, then **Mod C** also counts as depending on **Mod A**. Its files would then be loaded as initially expected with a single **Mod Dependency**.
