using System.Diagnostics;

namespace RyoTune.Reloaded;

/// <summary>
/// Utility functionality.
/// </summary>
public static class Utilities
{
    /// <summary>
    /// ASM for pushing caller registers.
    /// </summary>
    public const string PushCallerRegisters = "push rcx\npush rdx\npush r8\npush r9";

    /// <summary>
    /// ASM for popping caller registers.
    /// </summary>
    public const string PopCallerRegisters = "pop r9\npop r8\npop rdx\npop rcx";

    /// <summary>
    /// The current process base address.
    /// </summary>
    public static readonly nint BaseAddress = Process.GetCurrentProcess().MainModule?.BaseAddress ?? 0;
    
    /// <summary>
    /// Gets the absolute address from a relative address.
    /// </summary>
    /// <param name="relAddress">Relative address pointer.</param>
    /// <returns>Global address.</returns>
    public static unsafe nint GetGlobalAddress(int* relAddress) => *relAddress + (nint)relAddress + 4;
}
