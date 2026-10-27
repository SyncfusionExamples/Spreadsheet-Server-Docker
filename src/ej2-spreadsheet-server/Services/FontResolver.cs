using System;
using System.Collections.Generic;
using SkiaSharp;

namespace ej2_spreadsheet_server.Services
{
    /// <summary>
    /// Provides font resolution for SkiaSharp with fallback support.
    /// Handles cases where system fonts are not available (e.g., Windows nanoserver containers).
    /// </summary>
    public class FontResolver
    {
        private static readonly Dictionary<string, string> FontFamilyMap = new(StringComparer.OrdinalIgnoreCase)
        {
            // Standard font mappings for common requests
            { "Arial", "Segoe UI" },
            { "Calibri", "Segoe UI" },
            { "Times New Roman", "Segoe UI" },
            { "Courier New", "Segoe UI" },
            { "Georgia", "Segoe UI" },
            { "Verdana", "Segoe UI" },
            { "Tahoma", "Segoe UI" },
            { "Consolas", "Segoe UI" },
            { "Trebuchet MS", "Segoe UI" },
        };

        private static readonly List<string> FallbackFonts = new()
        {
            "Segoe UI",
            "Arial",
            "Helvetica",
            "sans-serif"
        };

        /// <summary>
        /// Resolves a font family name to an available SKTypeface, with fallback support.
        /// </summary>
        /// <param name="fontFamily">The requested font family name</param>
        /// <returns>SKTypeface if found, otherwise SKTypeface.Default</returns>
        public static SKTypeface ResolveFont(string fontFamily)
        {
            if (string.IsNullOrWhiteSpace(fontFamily))
            {
                return SKTypeface.Default;
            }

            // Try the requested font first
            var typeface = SKTypeface.FromFamilyName(fontFamily);
            if (typeface != null)
            {
                return typeface;
            }

            // Try mapped alternative
            if (FontFamilyMap.TryGetValue(fontFamily, out var mappedFont))
            {
                typeface = SKTypeface.FromFamilyName(mappedFont);
                if (typeface != null)
                {
                    return typeface;
                }
            }

            // Try fallback fonts
            foreach (var fallbackFont in FallbackFonts)
            {
                typeface = SKTypeface.FromFamilyName(fallbackFont);
                if (typeface != null)
                {
                    return typeface;
                }
            }

            // Return default as last resort
            return SKTypeface.Default;
        }

        /// <summary>
        /// Resolves a font with specific styling.
        /// </summary>
        /// <param name="fontFamily">The requested font family name</param>
        /// <param name="fontStyle">The requested font style</param>
        /// <returns>SKTypeface if found, otherwise SKTypeface.Default</returns>
        public static SKTypeface ResolveFontWithStyle(string fontFamily, SKFontStyle fontStyle)
        {
            if (string.IsNullOrWhiteSpace(fontFamily))
            {
                return SKTypeface.Default;
            }

            // Try the requested font first
            var typeface = SKTypeface.FromFamilyName(fontFamily, fontStyle);
            if (typeface != null)
            {
                return typeface;
            }

            // Try mapped alternative
            if (FontFamilyMap.TryGetValue(fontFamily, out var mappedFont))
            {
                typeface = SKTypeface.FromFamilyName(mappedFont, fontStyle);
                if (typeface != null)
                {
                    return typeface;
                }
            }

            // Try fallback fonts
            foreach (var fallbackFont in FallbackFonts)
            {
                typeface = SKTypeface.FromFamilyName(fallbackFont, fontStyle);
                if (typeface != null)
                {
                    return typeface;
                }
            }

            // Return default as last resort
            return SKTypeface.Default;
        }

        /// <summary>
        /// Initializes the font resolver and registers system fonts.
        /// Call this during application startup.
        /// </summary>
        public static void Initialize()
        {
            try
            {
                // Attempt to force load system fonts
                // This helps in container environments to register available fonts
                var customFontsPath = Environment.GetEnvironmentVariable("CUSTOM_FONTS_PATH");
                if (!string.IsNullOrEmpty(customFontsPath) && System.IO.Directory.Exists(customFontsPath))
                {
                    foreach (var fontFile in System.IO.Directory.GetFiles(customFontsPath, "*.ttf"))
                    {
                        try
                        {
                            SKTypeface.FromFile(fontFile);
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Failed to load font file {fontFile}: {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Font initialization error: {ex.Message}");
            }
        }
    }
}
