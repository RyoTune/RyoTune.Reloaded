﻿using System.Drawing;

namespace RyoTune.Reloaded;

internal static class ColorUtils
{
    public static Color WithMinBrightness(this Color color, double minLum)
    {
        var r = color.R / 255.0;
        var g = color.G / 255.0;
        var b = color.B / 255.0;

        var lum = r * 0.2126 + g * 0.7125 + b * 0.0722;

        if (lum >= minLum)
        {
            return color;
        }

        var maxLum = 0.95;
        var diff = minLum / lum;
        var newR = Math.Min(r * diff * 255, 255 * maxLum);
        var newG = Math.Min(g * diff * 255, 255 * maxLum);
        var newB = Math.Min(b * diff * 255, 255 * maxLum);
        return Color.FromArgb(0xFF, (byte)newR, (byte)newG, (byte)newB);
    }
}