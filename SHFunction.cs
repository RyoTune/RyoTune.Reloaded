using Reloaded.Hooks.Definitions;

namespace RyoTune.Reloaded;

/// <summary>
/// Creates a hook or wrapper from a <see cref="ScanHooks"/> result.
/// </summary>
/// <typeparam name="TFunction">Function.</typeparam>
public class SHFunction<TFunction>
{
    private IHook<TFunction>? _hook;
    private TFunction? _wrapper;
    private nint _wrapperAddress;
    private readonly string _name;

    /// <summary>
    /// <see cref="ScanHooks"/> function hook.
    /// </summary>
    /// <param name="function">Hook function.</param>
    /// <param name="pattern">Function pattern.</param>
    public SHFunction(TFunction function, string pattern)
    {
        _name = typeof(TFunction).Name;
        ScanHooks.Add(_name, pattern, (hooks, result) => _hook = hooks.CreateHook(function, result).Activate());
    }

    /// <summary>
    /// <see cref="ScanHooks"/> function wrapper.
    /// </summary>
    /// <param name="pattern">Function pattern.</param>
    public SHFunction(string pattern)
    {
        _name = typeof(TFunction).Name;
        ScanHooks.Add(_name, pattern, (hooks, result) => _wrapper = hooks.CreateWrapper<TFunction>(result, out _));
    }

    /// <summary>
    /// Original function wrapper.
    /// </summary>
    public TFunction OriginalFunction => _wrapper ?? _hook!.OriginalFunction;

    /// <summary>
    /// Disable function hook, if exists.
    /// </summary>
    public void Disable()
    {
        if (_hook != null)
        {
            _hook?.Disable();
        }
        else
        {
            Log.Warning($"{nameof(SHFunction<TFunction>)}<{_name}> is a wrapper and can not be disabled.");
        }
    }

    /// <summary>
    /// Enable function hook, if exists.
    /// </summary>
    public void Enable()
    {
        if (_hook != null)
        {
            _hook?.Enable();
        }
        else
        {
            Log.Warning($"{nameof(SHFunction<TFunction>)}<{_name}> is a wrapper and can not be enabled.");
        }
    }
}
