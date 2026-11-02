using Reloaded.Hooks.Definitions;
using RyoTune.Reloaded.Scans;

namespace RyoTune.Reloaded;

/// <summary>
/// Create a function wrapper and optionally a hook through <see cref="IScans"/>.
/// </summary>
/// <typeparam name="TFunction">Function.</typeparam>
public class SHFunction<TFunction>
{
    private readonly string _name;
    private bool _hasInitialized;
    private IFunction<TFunction>? _function;
    private TFunction? _hookFunction;

    /// <summary>
    /// Creates a <see cref="SHFunction{TFunction}"/> with only a function wrapper,
    /// and the option to set a hook separately with <see cref="SetQueuedHook"/>.
    /// </summary>
    /// <param name="address">Address of function.</param>
    public SHFunction(nint address)
    {
        _name = typeof(TFunction).Name;
        Project.Scans.AddScanHook(_name, address, Initialize);
    }
    
    /// <summary>
    /// Creates a <see cref="SHFunction{TFunction}"/> with both a function wrapper
    /// and function hook.
    /// </summary>
    /// <param name="hookFunction">Hook function.</param>
    /// <param name="address">Address of function.</param>
    public SHFunction(TFunction hookFunction, nint address) : this(address)
        => _hookFunction = hookFunction;
    
    /// <summary>
    /// Creates a <see cref="SHFunction{TFunction}"/> with only a function wrapper,
    /// and the option to set a hook separately with <see cref="SetQueuedHook"/>.
    /// </summary>
    /// <param name="pattern">Sig pattern of function.</param>
    public SHFunction(string pattern)
    {
        _name = typeof(TFunction).Name;
        Project.Scans.AddScanHook(_name, pattern, Initialize);
    }
    
    /// <summary>
    /// Creates a <see cref="SHFunction{TFunction}"/> with both a function wrapper
    /// and function hook.
    /// </summary>
    /// <param name="hookFunction">Hook function.</param>
    /// <param name="pattern">Sig pattern of function.</param>
    public SHFunction(TFunction hookFunction, string pattern) : this(pattern)
        => _hookFunction = hookFunction;
    
    /// <summary>
    /// Creates a <see cref="SHFunction{TFunction}"/> with only a function wrapper,
    /// and the option to set a hook separately with <see cref="SetQueuedHook"/>.
    /// </summary>
    public SHFunction()
    {
        _name = typeof(TFunction).Name;
        Project.Scans.AddScanHook(_name, Initialize);
    }

    /// <summary>
    /// <see cref="IReloadedHooks"/> instance, if a hook function was set.
    /// </summary>
    public IHook<TFunction> Hook { get; private set; } = null!;

    /// <summary>
    /// Function wrapper for calling the native function.
    /// </summary>
    public TFunction Wrapper => _function!.GetWrapper();
    
    /// <summary>
    /// Set a function to create a <see cref="IReloadedHooks"/> hook with.<br/>
    /// Must be done before scanning has started, during mod initialization.
    /// </summary>
    /// <param name="hookFunction">The hook function.</param>
    public void SetQueuedHook(TFunction hookFunction)
    {
        if (_hasInitialized)
        {
            Log.Error("Cannot queue hook after initialization.");
            return;
        }
        
        _hookFunction = hookFunction;
    }

    /// <summary>
    /// Remove the queued hook function.
    /// </summary>
    public void RemoveQueuedHook()
    {
        if (_hasInitialized)
        {
            Log.Error("Cannot remove queued hook after initialization. To disable a hook, use the hook's Disable() method.");
            return;
        }
        
        _hookFunction = default;
    }

    private void Initialize(nint result, IReloadedHooks hooks)
    {
        _function = hooks.CreateFunction<TFunction>(result);
        if (_hookFunction != null) Hook = _function.Hook(_hookFunction).Activate();
        _hasInitialized = true;
    }
}
