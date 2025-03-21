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
    public readonly static string PushCallerRegisters = "push rcx\npush rdx\npush r8\npush r9";

    /// <summary>
    /// ASM for popping caller registers.
    /// </summary>
    public readonly static string PopCallerRegisters = "pop r9\npop r8\npop rdx\npop rcx";

    /// <summary>
    /// The current process base address.
    /// </summary>
    public readonly static nint BaseAddress = Process.GetCurrentProcess().MainModule?.BaseAddress ?? 0;
}
