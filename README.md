[English](README.md) | [Русский](README_RU.md)

# KebuzForge

A tool for creating pixel art and icons from regular images (Windows, WinForms, .NET 8).

## Features

- **Pixelization** - block averaging or center sampling, aspect ratio lock
- **Palettes** - Median Cut quantization, retro palettes, per-color recoloring without re-quantization, import/export (JASC PAL, Photoshop ACT, GIMP GPL)
- **Dithering** - Floyd–Steinberg, Bayer 4×4 / 8×8 with adjustable intensity, applied over effects
- **3D effects** - sphere, cylinder, rounded rectangle with lighting (ambient/diffuse, light direction)
- **Pixel editor** - pencil, line, rectangle, ellipse (Shift - regular shape, Ctrl - from center), fill, eyedropper, eraser, secondary color on right click, zoom up to 16×
- **Animation** - frame-by-frame GIF processing through the full pipeline, animated GIF export
- **Export** - PNG, Indexed PNG (8 bpp), multi-size ICO, batch processing
- **Pipeline auto-cascade** - re-pixelizing recomputes palette, dithering and effects automatically (can be turned off)
- **Background removal**, outlines, detachable panels, dark/light/hybrid themes, presets, double-click a slider label to type an exact value
- **Plugins** - extend functionality via DLLs

## Download

Ready-to-use builds are in the [Releases](../../releases) section:

| File | Description |
|---|---|
| `KebuzForge-Setup-…-win-x64.exe` | Installer: shortcuts, uninstaller, downloads .NET 8 Runtime automatically |
| `KebuzForge-win-x64-portable.exe` | Single file, no installation needed (~68 MB) |
| `KebuzForge-win-x64-lite.exe` | Single file ~0.4 MB, requires [.NET 8 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/8.0) |
| `KebuzForge-win-arm64-…` / `win-x86-…` | Same for Windows ARM64 and 32-bit systems |
| `FaviconForgePlugin-….zip` | "Favicon Generator" plugin |

## Plugins

Plugins are DLLs placed in `%LocalAppData%\Kebuz\KebuzForge\Plugins`
(menu **Plugins → Open plugins folder...**) or in the `Plugins` folder next to the exe (portable variant).

### Favicon Generator

Generates a complete favicon package for a website (similar to realfavicongenerator.net):
favicon.ico + PNG (16/32/48), apple-touch-icon, android-chrome 192/512, PWA maskable,
Windows Tiles + browserconfig.xml, safari-pinned-tab.svg, site.webmanifest and ready-to-use HTML code.
Background, padding, rounding and color settings per platform, live previews, export to ZIP or folder.

### Your own plugin

A plugin is a net8.0-windows assembly with a public static class `PluginEntry`:

```csharp
public static class PluginEntry
{
    public static string PluginName => "Plugin name";
    public static string PluginDescription => "Description";
    public static string PluginVersion => "1.0.0";
    public static Form CreateMainForm() => new MyPluginForm();
}
```

## Building from source

```
dotnet build KebuzForge.sln -c Release
```

Single-file publish:

```
dotnet publish KebuzForge.App -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
dotnet publish KebuzForge.App -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true -p:EnableCompressionInSingleFile=false
```

Installer (requires [Inno Setup 6](https://jrsoftware.org/isinfo.php)):

```
iscc installer\setup.iss
```

## License

[MIT](LICENSE)
