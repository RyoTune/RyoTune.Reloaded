using Reloaded.Hooks.Definitions;

namespace RyoTune.Reloaded;

/// <summary>
/// Create a hook and/or wrapper from a <see cref="ScanHooks"/> result.
/// </summary>
/// <typeparam name="TFunction">Function.</typeparam>
public class SHFunction<TFunction>
{
    private readonly string _name;
    private IFunction<TFunction>? _function;
    private TFunction? _hookFunction;
    
    /// <summary>
    /// Creates a <see cref="SHFunction{TFunction}"/> with only a function wrapper,
    /// and the option to set a hook separately with <see cref="SetHook"/>.
    /// </summary>
    /// <param name="pattern">Sig pattern of function.</param>
    public SHFunction(string pattern)
    {
        _name = typeof(TFunction).Name;
        
        ScanHooks.Add(_name, pattern, (hooks, result) =>
        {
            _function = hooks.CreateFunction<TFunction>(result);
            if (_hookFunction != null) Hook = _function.Hook(_hookFunction).Activate();
        });
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
    /// <see cref="IReloadedHooks"/> instance, if a hook function was set.
    /// </summary>
    public IHook<TFunction>? Hook { get; private set; }

    /// <summary>
    /// Function wrapper for calling the native function.
    /// </summary>
    public TFunction Wrapper => _function!.GetWrapper();
    
    /// <summary>
    /// Set a function to create a <see cref="IReloadedHooks"/> hook with.
    /// Must be done before scanning has started, during normal mod initialization.
    /// </summary>
    /// <param name="hookFunction">The hook function. If <c>null</c>, no hook will be created.</param>
    public void SetHook(TFunction? hookFunction) => _hookFunction = hookFunction;
}
