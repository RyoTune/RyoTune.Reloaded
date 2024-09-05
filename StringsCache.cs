using System.Runtime.InteropServices;

namespace RyoTune.Reloaded;

public static class StringsCache
{
    private static readonly Dictionary<string, nint> cachedStrPtrs = [];
    private static readonly Dictionary<string, nint> cachedStrPtrsUni = [];

    public static nint GetStringPtr(string str)
    {
        if (cachedStrPtrs.TryGetValue(str, out var ptr))
        {
            return ptr;
        }

        var newPtr = Marshal.StringToHGlobalAnsi(str);
        cachedStrPtrs[str] = newPtr;
        return newPtr;
    }

    public static nint GetStringPtrUni(string str)
    {
        if (cachedStrPtrsUni.TryGetValue(str, out var ptr))
        {
            return ptr;
        }

        var newPtr = Marshal.StringToHGlobalUni(str);
        cachedStrPtrsUni[str] = newPtr;
        return newPtr;
    }
}
