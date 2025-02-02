using Reloaded.Hooks.Definitions;

namespace RyoTune.Reloaded;

/// <summary>
/// <see cref="IHook"/> instance that eventually creates the actual hook from a <see cref="ScanHooks"/> result.
/// </summary>
/// <typeparam name="TFunction">Function.</typeparam>
public class SHFunction<TFunction> : IHook<TFunction>
{
    private IHook<TFunction>? _hook;

    /// <summary>
    /// <see cref="ScanHooks"/> function hook.
    /// </summary>
    /// <param name="function">Hook function.</param>
    /// <param name="pattern">Function pattern.</param>
    public SHFunction(TFunction function, string pattern)
        => ScanHooks.Add(typeof(TFunction).Name, pattern, (hooks, result) => _hook = hooks.CreateHook(function, result).Activate());

    /// <inheritdoc/>
    public TFunction OriginalFunction => _hook!.OriginalFunction;

    /// <inheritdoc/>
    public IReverseWrapper<TFunction> ReverseWrapper => _hook!.ReverseWrapper;

    /// <inheritdoc/>
    public bool IsHookEnabled => _hook!.IsHookEnabled;

    /// <inheritdoc/>
    public bool IsHookActivated => _hook!.IsHookActivated;

    /// <inheritdoc/>
    public nint OriginalFunctionAddress => _hook!.OriginalFunctionAddress;

    /// <inheritdoc/>
    public nint OriginalFunctionWrapperAddress => _hook!.OriginalFunctionWrapperAddress;

    /// <inheritdoc/>
    public IHook<TFunction> Activate() => throw new NotImplementedException("Hook is activated once ready, do not call manually.");

    /// <inheritdoc/>
    public void Disable() => _hook!.Disable();

    /// <inheritdoc/>
    public void Enable() => _hook!.Enable();
}
