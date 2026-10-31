namespace RyoTune.Reloaded.Utilities;

/// <summary>
/// Utility functions for resolving instruction relative addresses.
/// </summary>
public static class RelativeAddress
{
    /// <summary>
    /// Resolves a relative branch target or supported RIP-relative
    /// memory address in x64 code.
    ///
    /// For indirect calls/jumps, returns the address of the pointer slot.
    /// The instruction pointer must reference valid, readable memory.
    /// </summary>
    public static unsafe nint Resolve(nint instruction)
    {
        if (instruction == 0)
            throw new ArgumentNullException(nameof(instruction));

        byte* p = (byte*)instruction;
        int offset = 0;

        // Optional x64 REX prefix.
        if ((p[offset] & 0xF0) == 0x40)
            offset++;

        byte opcode = p[offset++];

        switch (opcode)
        {
            // CALL rel32 / JMP rel32
            case 0xE8:
            case 0xE9:
                return AddRel32(instruction, p, offset);

            // JMP rel8 / LOOP-family rel8
            case 0xEB:
            case 0xE0:
            case 0xE1:
            case 0xE2:
            case 0xE3:
                return AddRel8(instruction, p, offset);

            // Jcc rel32
            case 0x0F:
            {
                byte secondOpcode = p[offset++];

                if (secondOpcode >= 0x80 && secondOpcode <= 0x8F)
                    return AddRel32(instruction, p, offset);

                break;
            }

            // MOV r8, r/m8 / MOV r, r/m
            // MOV r/m8, r8 / MOV r/m, r
            // LEA r, m
            // CALL/JMP r/m
            case 0x8A:
            case 0x8B:
            case 0x88:
            case 0x89:
            case 0x8D:
            case 0xFF:
            {
                byte modRm = p[offset++];

                // mod = 00, r/m = 101: RIP + signed disp32.
                if ((modRm & 0xC7) != 0x05)
                    break;

                if (opcode == 0xFF)
                {
                    int operation = (modRm >> 3) & 7;

                    // FF /2 = near indirect CALL
                    // FF /4 = near indirect JMP
                    if (operation != 2 && operation != 4)
                        break;
                }

                return AddRel32(instruction, p, offset);
            }

            default:
                // Jcc rel8
                if (opcode >= 0x70 && opcode <= 0x7F)
                    return AddRel8(instruction, p, offset);

                break;
        }

        throw new NotSupportedException(
            "Unsupported instruction or non-relative operand.");
    }

    private static unsafe nint AddRel8(
        nint instruction, byte* bytes, int offset)
    {
        int displacement = unchecked((sbyte)bytes[offset]);
        return unchecked(instruction + offset + 1 + displacement);
    }

    private static unsafe nint AddRel32(
        nint instruction, byte* bytes, int offset)
    {
        byte* p = bytes + offset;

        int displacement = unchecked((int)(
            (uint)p[0] |
            ((uint)p[1] << 8) |
            ((uint)p[2] << 16) |
            ((uint)p[3] << 24)));

        return unchecked(instruction + offset + 4 + displacement);
    }
    
    
    /// <summary>
    /// Gets the absolute address from a relative address.
    /// </summary>
    /// <param name="relAddress">Relative address pointer.</param>
    /// <returns>Global address.</returns>
    [Obsolete("Use Resolve(nint instruction) instead.")]
    public static unsafe nint GetGlobalAddress(nint relAddress) => *(int*)relAddress + relAddress + 4;
}
