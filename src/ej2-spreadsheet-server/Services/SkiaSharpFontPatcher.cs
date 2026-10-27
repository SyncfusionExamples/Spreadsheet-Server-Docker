using System;
using System.Reflection;
using SkiaSharp;

namespace ej2_spreadsheet_server.Services
{
    /// <summary>
    /// Patches SkiaSharp font resolution to use the FontResolver with fallback support.
    /// This helps prevent NullReferenceException in environments with limited font support (e.g., Windows nanoserver).
    /// </summary>
    public class SkiaSharpFontPatcher
    {
        /// <summary>
        /// Initializes the font patcher by replacing SkiaSharp's font resolution with fallback-aware version.
        /// </summary>
        public static void Initialize()
        {
            try
            {
                FontResolver.Initialize();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SkiaSharp font patcher initialization error: {ex.Message}");
            }
        }

        /// <summary>
        /// Creates a safe typeface that won't return null, using FontResolver.
        /// </summary>
        public static SKTypeface CreateSafeTypeface(string fontFamily)
        {
            return FontResolver.ResolveFont(fontFamily) ?? SKTypeface.Default;
        }

        /// <summary>
        /// Creates a safe typeface with specific style that won't return null.
        /// </summary>
        public static SKTypeface CreateSafeTypeface(string fontFamily, SKFontStyle fontStyle)
        {
            return FontResolver.ResolveFontWithStyle(fontFamily, fontStyle) ?? SKTypeface.Default;
        }
    }
}
