using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Text;

namespace RyoTune.Reloaded;

/// <summary>
/// String extension methods for easily converting to pointers for use in native functions.
/// Converted strings are cached and pointers are safe for reuse.
/// </summary>
public static class StringExtensions
{
    private static readonly Dictionary<StringCacheKey, nint> StringCache = [];
    private static readonly ConcurrentDictionary<StringCacheKey, nint> StringCacheSafe = [];
    
    /// <summary>
    /// Returns a pointer to this string in ANSI.
    /// </summary>
    /// <param name="str"/>
    /// <param name="useThreadSafe">Whether to use thread-safe caching.</param>
    public static nint AsPointerAnsi(this string str, bool useThreadSafe = false)
    {
        IDictionary<StringCacheKey, nint> cache = useThreadSafe ? StringCacheSafe : StringCache;

        var key = new StringCacheKey(str, Encoding.ASCII.CodePage);
        if (cache.TryGetValue(key, out var pointer)) return pointer;
        
        cache[key] = Marshal.StringToHGlobalAnsi(str);
        return cache[key];
    }

    /// <summary>
    /// Returns a pointer to this string in Unicode.
    /// </summary>
    /// <param name="str"/>
    /// <param name="useThreadSafe">Whether to use thread-safe caching.</param>
    public static nint AsPointerUni(this string str, bool useThreadSafe = false)
    {
        IDictionary<StringCacheKey, nint> cache = useThreadSafe ? StringCacheSafe : StringCache;

        var key = new StringCacheKey(str, Encoding.Unicode.CodePage);
        if (cache.TryGetValue(key, out var pointer)) return pointer;
        
        cache[key] = Marshal.StringToHGlobalUni(str);
        return cache[key];
    }
    
    /// <summary>
    /// Returns a pointer to this string in the specified encoding.
    /// </summary>
    /// <param name="str"/>
    /// <param name="encoding">Encoding of the string pointer.</param>
    /// <param name="useThreadSafe">Whether to use thread-safe caching.</param>
    public static nint AsPointer(this string str, Encoding encoding, bool useThreadSafe = false)
    {
        IDictionary<StringCacheKey, nint> cache = useThreadSafe ? StringCacheSafe : StringCache;

        var key = new StringCacheKey(str, encoding.CodePage);
        if (cache.TryGetValue(key, out var pointer)) return pointer;
        
        var bytes = encoding.GetBytes(str + '\0');
        cache[key] = Marshal.AllocHGlobal(bytes.Length);
        Marshal.Copy(bytes, 0, cache[key], bytes.Length);
        
        return cache[key];
    }

    // ReSharper disable NotAccessedPositionalProperty.Local
    private record struct StringCacheKey(string Content, int CodePage);
}