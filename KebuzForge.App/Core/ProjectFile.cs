using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using KebuzForge.App.Models;
using KebuzForge.Canvas;

namespace KebuzForge.App.Core
{
    internal sealed class ProjectData
    {
        public required CanvasDocument Canvas { get; init; }
        public required AppSettings Settings { get; init; }
        public Color[] Palette { get; init; } = [];
        public byte[]? PaletteIndices { get; init; }
        public bool PaletteActive { get; init; }
        public bool DitherActive { get; init; }
        public bool EffectActive { get; init; }
        public bool FlatActive { get; init; }
        public bool BackgroundRemoved { get; init; }
        public Bitmap? Pixelized { get; init; }
        public Bitmap? Result { get; init; }
        public PixelOverlay? Overlay { get; init; }
        public PixelOverlay? PreOverlay { get; init; }
        public string? SourcePath { get; init; }
    }

    internal static class ProjectFile
    {
        public const string Extension = ".kforge";
        public const int Version = 1;
        private const string ProjectEntry = "project.json";

        public static void Save(string path, ProjectData p)
        {
            string? dir = Path.GetDirectoryName(Path.GetFullPath(path));
            if (dir is not null) Directory.CreateDirectory(dir);
            string tmp = path + ".tmp";
            using (var fs = File.Create(tmp))
            using (var zip = new ZipArchive(fs, ZipArchiveMode.Create))
            {
                var names = new Dictionary<Bitmap, string>();
                var canvas = CanvasJson.Write(p.Canvas, b => names.TryGetValue(b, out var n) ? n : names[b] = $"images/{names.Count + 1}.png");
                foreach (var (bmp, name) in names)
                    WritePng(zip, name, bmp);

                var state = new JsonObject
                {
                    ["paletteActive"] = p.PaletteActive,
                    ["ditherActive"] = p.DitherActive,
                    ["effectActive"] = p.EffectActive,
                    ["flatActive"] = p.FlatActive,
                    ["backgroundRemoved"] = p.BackgroundRemoved,
                    ["pixelized"] = WriteOptionalPng(zip, "state/pixelized.png", p.Pixelized),
                    ["result"] = WriteOptionalPng(zip, "state/result.png", p.Result),
                    ["overlay"] = WriteOverlay(zip, "state/overlay", p.Overlay),
                    ["preOverlay"] = WriteOverlay(zip, "state/preoverlay", p.PreOverlay)
                };
                if (p.PaletteIndices is not null)
                {
                    using var s = zip.CreateEntry("state/indices.bin", CompressionLevel.Optimal).Open();
                    s.Write(p.PaletteIndices);
                    state["indices"] = "state/indices.bin";
                }

                var root = new JsonObject
                {
                    ["format"] = "kforge",
                    ["version"] = Version,
                    ["app"] = Assembly.GetEntryAssembly()?.GetName().Version?.ToString(3) ?? "",
                    ["sourcePath"] = p.SourcePath,
                    ["canvas"] = canvas,
                    ["settings"] = JsonSerializer.SerializeToNode(p.Settings),
                    ["palette"] = new JsonArray(p.Palette.Select(c => (JsonNode?)CanvasJson.ColorText(c)).ToArray()),
                    ["state"] = state
                };
                using var js = zip.CreateEntry(ProjectEntry, CompressionLevel.Optimal).Open();
                using var writer = new Utf8JsonWriter(js, new JsonWriterOptions { Indented = true });
                root.WriteTo(writer);
            }
            File.Move(tmp, path, overwrite: true);
        }

        public static ProjectData Load(string path)
        {
            using var fs = File.OpenRead(path);
            using var zip = new ZipArchive(fs, ZipArchiveMode.Read);
            var entry = zip.GetEntry(ProjectEntry) ?? throw new InvalidDataException("project.json is missing");
            JsonObject root;
            using (var js = entry.Open())
                root = JsonNode.Parse(js) as JsonObject ?? throw new InvalidDataException("project.json is not an object");
            if ((string?)root["format"] != "kforge") throw new InvalidDataException("not a KebuzForge project");
            int version = root["version"]?.GetValue<int>() ?? 0;
            if (version > Version) throw new NotSupportedException($"project version {version}");

            var images = new Dictionary<string, Bitmap>();
            Bitmap Image(string name) =>
                images.TryGetValue(name, out var b) ? b : images[name] = ReadPng(zip, name);

            var canvas = CanvasJson.Read(root["canvas"] as JsonObject ?? throw new InvalidDataException("canvas is missing"), Image);
            var settings = root["settings"]?.Deserialize<AppSettings>() ?? new AppSettings();
            var palette = (root["palette"] as JsonArray ?? [])
                .Select(n => CanvasJson.ParseColor((string?)n))
                .Where(c => c is not null).Select(c => c!.Value).ToArray();
            var state = root["state"] as JsonObject ?? new JsonObject();

            byte[]? indices = null;
            if ((string?)state["indices"] is { } indicesName)
            {
                var e = zip.GetEntry(indicesName) ?? throw new InvalidDataException(indicesName);
                using var s = e.Open();
                using var ms = new MemoryStream();
                s.CopyTo(ms);
                indices = ms.ToArray();
            }

            return new ProjectData
            {
                Canvas = canvas,
                Settings = settings,
                Palette = palette,
                PaletteIndices = indices,
                PaletteActive = Flag(state, "paletteActive"),
                DitherActive = Flag(state, "ditherActive"),
                EffectActive = Flag(state, "effectActive"),
                FlatActive = Flag(state, "flatActive"),
                BackgroundRemoved = Flag(state, "backgroundRemoved"),
                Pixelized = OptionalPng(zip, state, "pixelized"),
                Result = OptionalPng(zip, state, "result"),
                Overlay = ReadOverlay(zip, state, "overlay"),
                PreOverlay = ReadOverlay(zip, state, "preOverlay"),
                SourcePath = (string?)root["sourcePath"]
            };
        }

        private static bool Flag(JsonObject state, string key) =>
            state[key] is JsonValue v && v.GetValue<bool>();

        private static string? WriteOptionalPng(ZipArchive zip, string name, Bitmap? bmp)
        {
            if (bmp is null) return null;
            WritePng(zip, name, bmp);
            return name;
        }

        private static string? WriteOverlay(ZipArchive zip, string baseName, PixelOverlay? overlay)
        {
            if (overlay is null) return null;
            using (var pixels = overlay.ToBitmap())
                WritePng(zip, baseName + ".png", pixels);
            using var s = zip.CreateEntry(baseName + ".mask", CompressionLevel.Optimal).Open();
            s.Write(overlay.MaskBytes());
            return baseName;
        }

        private static PixelOverlay? ReadOverlay(ZipArchive zip, JsonObject state, string key)
        {
            if ((string?)state[key] is not { } baseName) return null;
            using var pixels = ReadPng(zip, baseName + ".png");
            var entry = zip.GetEntry(baseName + ".mask") ?? throw new InvalidDataException($"{baseName}.mask is missing");
            using var s = entry.Open();
            using var ms = new MemoryStream();
            s.CopyTo(ms);
            return PixelOverlay.FromPixels(pixels, ms.ToArray());
        }

        private static Bitmap? OptionalPng(ZipArchive zip, JsonObject state, string key) =>
            (string?)state[key] is { } name ? ReadPng(zip, name) : null;

        private static void WritePng(ZipArchive zip, string name, Bitmap bmp)
        {
            using var ms = new MemoryStream();
            bmp.Save(ms, ImageFormat.Png);
            using var s = zip.CreateEntry(name, CompressionLevel.NoCompression).Open();
            ms.Position = 0;
            ms.CopyTo(s);
        }

        private static Bitmap ReadPng(ZipArchive zip, string name)
        {
            var entry = zip.GetEntry(name) ?? throw new InvalidDataException($"{name} is missing");
            using var s = entry.Open();
            using var ms = new MemoryStream();
            s.CopyTo(ms);
            ms.Position = 0;
            using var tmp = new Bitmap(ms);
            var copy = new Bitmap(tmp.Width, tmp.Height, PixelFormat.Format32bppArgb);
            var rect = new Rectangle(0, 0, tmp.Width, tmp.Height);
            var sd = tmp.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            var td = copy.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            try
            {
                var row = new byte[rect.Width * 4];
                for (int y = 0; y < rect.Height; y++)
                {
                    System.Runtime.InteropServices.Marshal.Copy(sd.Scan0 + y * sd.Stride, row, 0, row.Length);
                    System.Runtime.InteropServices.Marshal.Copy(row, 0, td.Scan0 + y * td.Stride, row.Length);
                }
            }
            finally
            {
                tmp.UnlockBits(sd);
                copy.UnlockBits(td);
            }
            return copy;
        }
    }
}
