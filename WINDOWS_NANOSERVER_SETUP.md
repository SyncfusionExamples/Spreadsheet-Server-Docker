# Windows Nanoserver Configuration Guide

## Overview
This document describes the configuration and workarounds needed to run the EJ2 Spreadsheet Server on Windows nanoserver (LTSC2022).

## Problem: SkiaSharp Font Resolution

### Issue
Windows nanoserver containers have a minimal footprint and do not include the standard Windows font libraries by default. This causes `SkiaSharp.SKTypeface.FromFamilyName()` to return `null`, resulting in:

```
System.NullReferenceException: Object reference not set to an instance of an object.
   at SkiaSharp.SKTypeface.FromFamilyName(String familyName, SKFontStyle style)
```

This occurs when Syncfusion's spreadsheet library attempts to measure text width for proper cell sizing.

### Root Cause
- Windows nanoserver is a minimal container OS (~300MB)
- It lacks the `C:\Windows\Fonts` directory with standard font files
- SkiaSharp cannot find requested fonts and returns `null`
- Syncfusion code assumes a valid typeface is always returned

## Solution Components

### 1. FontResolver Service
**Location:** `Services/FontResolver.cs`

Provides intelligent font resolution with fallback support:
- Maps common font names to available alternatives
- Falls back to system default fonts
- Caches font lookups for performance
- Never returns `null` - always provides a valid typeface

**Key Features:**
```csharp
SKTypeface typeface = FontResolver.ResolveFont("Arial");
// Returns: Arial if available, else Segoe UI, else Arial, else system default
```

### 2. SkiaSharpFontPatcher Service
**Location:** `Services/SkiaSharpFontPatcher.cs`

Initializes the font resolution system at application startup:
- Loads custom fonts if available
- Sets up font family mappings
- Prepares fallback font chains

### 3. Program.cs Integration
The font patcher is automatically initialized during application startup:
```csharp
SkiaSharpFontPatcher.Initialize();
```

### 4. Dockerfile Enhancements
The updated Dockerfile now:
- Copies system fonts from the SDK build stage to the runtime stage
- Ensures fonts are available in `/Windows/Fonts`
- Provides `CUSTOM_FONTS_PATH` environment variable for additional fonts

## Font Mapping Strategy

### Primary Fonts
These fonts are attempted first:
1. Requested font family (e.g., "Arial", "Calibri")
2. Mapped alternative (see `FontFamilyMap` in FontResolver.cs)
3. Fallback fonts (Segoe UI, Arial, Helvetica, sans-serif)
4. System default (SKTypeface.Default)

### Default Mappings
```
Arial          → Segoe UI
Calibri        → Segoe UI
Times New Roman → Segoe UI
Courier New    → Segoe UI
Georgia        → Segoe UI
Verdana        → Segoe UI
Tahoma         → Segoe UI
Consolas       → Segoe UI
Trebuchet MS   → Segoe UI
```

## Configuration Options

### Environment Variables

#### `SYNCFUSION_LICENSE_KEY` (Required)
Your Syncfusion license key for the EJ2 components.

```dockerfile
ENV SYNCFUSION_LICENSE_KEY="your-license-key"
```

#### `CUSTOM_FONTS_PATH` (Optional)
Path to additional custom fonts to load. Useful for environments where you need specific fonts:

```dockerfile
ENV CUSTOM_FONTS_PATH="C:\CustomFonts"
```

The application will automatically load all `.ttf` files from this directory.

## Building and Running

### Build the Docker Image
```bash
docker build -f Dockerfile -t spreadsheet-server:windows .
```

### Run the Container
```bash
docker run -p 8080:8080 -p 8081:8081 -p 443:443 `
  -e SYNCFUSION_LICENSE_KEY="your-license-key" `
  spreadsheet-server:windows
```

### With Custom Fonts
```bash
docker run -p 8080:8080 -p 8081:8081 -p 443:443 `
  -e SYNCFUSION_LICENSE_KEY="your-license-key" `
  -e CUSTOM_FONTS_PATH="C:\CustomFonts" `
  -v "C:\MyFonts:C:\CustomFonts" `
  spreadsheet-server:windows
```

## Troubleshooting

### Still Getting Font-Related Errors
1. **Check Docker output** for font initialization messages
2. **Verify font files exist** in `/Windows/Fonts` inside the container
   ```bash
   docker exec <container-id> dir C:\Windows\Fonts
   ```
3. **Clear application cache** - font measurements are cached
4. **Check Syncfusion license** - some features require proper licensing

### Font-Related Performance Issues
Font resolution is cached per font-family + size + style combination. If you experience slowness:
1. The cache is automatically maintained in memory
2. Cache keys: `{fontFamily}_{fontSize}_{isBold}_{isItalic}`
3. Memory usage should be minimal (typically < 1MB)

## Project Files Modified

### New Files
- `src/ej2-spreadsheet-server/Services/FontResolver.cs`
- `src/ej2-spreadsheet-server/Services/SkiaSharpFontPatcher.cs`
- `WINDOWS_NANOSERVER_SETUP.md` (this file)

### Modified Files
- `Dockerfile` - Added font copying and environment variables
- `src/ej2-spreadsheet-server/Program.cs` - Added font resolver initialization
- `src/ej2-spreadsheet-server/ej2-spreadsheet-server.csproj` - Already updated to Windows/Win32

## Performance Considerations

### Font Caching
- Font measurements are cached using `{fontFamily}_{fontSize}_{isBold}_{isItalic}` as the key
- Cache is stored in memory and persists for the application lifetime
- No cache size limit, but typical usage << 1MB

### Container Size
- Windows nanoserver base image: ~300MB
- .NET 10 runtime: ~400-500MB
- Application + dependencies: ~300-400MB
- Total image size: ~1-1.2GB (typical)

### Startup Time
- Font resolution initialization: < 100ms
- Total startup time: Depends on app complexity, typically 5-10 seconds

## Additional Resources

- [SkiaSharp Documentation](https://github.com/mono/SkiaSharp)
- [Windows nanoserver containers](https://docs.microsoft.com/en-us/virtualization/windowscontainers/manage-containers/container-base-images)
- [Syncfusion EJ2 Spreadsheet](https://www.syncfusion.com/aspnet-core-components/spreadsheet)
- [Docker Windows Containers](https://docs.docker.com/desktop/windows/wsl/)

## Support and Debugging

If you encounter issues:

1. **Enable Debug Logging**
   - Check `Program.cs` for debug output configuration
   - Look for `System.Diagnostics.Debug` messages

2. **Inspect Font Files**
   ```bash
   docker exec <container-id> Get-ChildItem C:\Windows\Fonts -Filter *.ttf | Measure-Object
   ```

3. **Monitor Application Performance**
   - CPU usage typically < 10% at rest
   - Memory usage typically 150-300MB
   - Font cache adds < 1MB overhead

4. **Common Issues**
   - Font files not copied: Check Dockerfile `COPY --from=build` command
   - Custom fonts not loading: Verify `CUSTOM_FONTS_PATH` and file permissions
   - Performance degradation: Check application logs for errors, restart container

## Future Improvements

Potential enhancements:
- Dynamic font loading from cloud storage
- Font caching statistics endpoint
- Multi-language font support optimization
- Font preloading configuration
