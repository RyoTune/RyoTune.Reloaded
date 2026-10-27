namespace RyoTune.Reloaded.Utilities;

/// <summary>
/// Various assembly code snippets.
/// </summary>
public static class AsmSnippets
{
    /// <summary>
    /// ASM for pushing caller registers.
    /// </summary>
    public const string PushCallerRegisters = "push rcx\npush rdx\npush r8\npush r9";

    /// <summary>
    /// ASM for popping caller registers.
    /// </summary>
    public const string PopCallerRegisters = "pop r9\npop r8\npop rdx\npop rcx";
}