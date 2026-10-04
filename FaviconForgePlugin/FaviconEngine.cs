using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace FaviconForge
{
    public sealed class FaviconOptions
    {
        public string AppName { get; set; } = "My Application";
        public string ShortName { get; set; } = "App";
        public string AppleTitle { get; set; } = "My App";
        public string PathPrefix { get; set; } = "/";
        public Color ThemeColor { get; set; } = Color.White;
        public bool PixelArt { get; set; }

        public bool FaviconBackground { get; set; }
        public Color FaviconBackColor { get; set; } = Color.White;
        public int FaviconMargin { get; set; }
        public int FaviconRadius { get; set; }

        public bool IosBackground { get; set; } = true;
        public Color IosBackColor { get; set; } = Color.White;
        public int IosMargin { get; set; } = 10;

        public bool AndroidBackground { get; set; }
        public Color AndroidBackColor { get; set; } = Color.White;
        public int AndroidMargin { get; set; }
        public bool AndroidCircle { get; set; }

        public Color TileColor { get; set; } = Color.FromArgb(0x2D, 0x89, 0xEF);
        public int TileMargin { get; set; } = 20;

        public Color SafariColor { get; set; } = Color.FromArgb(0x5B, 0xBA, 0xD5);
        public int SafariThreshold { get; set; } = 128;
    }

    public static class FaviconEngine
    {
        public static string ToHex(Color c) =>
            $"#{c.R:x2}{c.G:x2}{c.B:x2}";

        public static Bitmap Compose(Bitmap src, int width, int height, Color? background,
            int marginPercent, int radiusPercent, bool circle, bool pixelArt)
        {
            var result = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            using var g = Graphics.FromImage(result);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = pixelArt
                ? InterpolationMode.NearestNeighbor
                : InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.Half;

            using var shape = BuildShape(width, height, radiusPercent, circle);
            if (background is not null)
            {
                using var brush = new SolidBrush(background.Value);
                g.FillPath(brush, shape);
            }
            if (circle || radiusPercent > 0)
                g.SetClip(shape);

            int mx = width * Math.Clamp(marginPercent, 0, 45) / 100;
            int my = height * Math.Clamp(marginPercent, 0, 45) / 100;
            int cw = Math.Max(1, width - mx * 2);
            int ch = Math.Max(1, height - my * 2);
            float scale = Math.Min((float)cw / src.Width, (float)ch / src.Height);
            int dw = Math.Max(1, (int)Math.Round(src.Width * scale));
            int dh = Math.Max(1, (int)Math.Round(src.Height * scale));
            g.DrawImage(src, new Rectangle((width - dw) / 2, (height - dh) / 2, dw, dh));
            return result;
        }

        private static GraphicsPath BuildShape(int w, int h, int radiusPercent, bool circle)
        {
            var path = new GraphicsPath();
            if (circle)
            {
                path.AddEllipse(0, 0, w, h);
                return path;
            }
            int r = Math.Min(w, h) * Math.Clamp(radiusPercent, 0, 50) / 100;
            if (r <= 0)
            {
                path.AddRectangle(new Rectangle(0, 0, w, h));
                return path;
            }
            int d = r * 2;
            path.AddArc(0, 0, d, d, 180, 90);
            path.AddArc(w - d, 0, d, d, 270, 90);
            path.AddArc(w - d, h - d, d, d, 0, 90);
            path.AddArc(0, h - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        public static byte[] PngBytes(Bitmap bmp)
        {
            using var ms = new MemoryStream();
            bmp.Save(ms, ImageFormat.Png);
            return ms.ToArray();
        }

        public static byte[] BuildIco(IReadOnlyList<Bitmap> images)
        {
            var blocks = new byte[images.Count][];
            for (int i = 0; i < images.Count; i++)
                blocks[i] = PngBytes(images[i]);

            using var ms = new MemoryStream();
            using var bw = new BinaryWriter(ms);
            bw.Write((short)0);
            bw.Write((short)1);
            bw.Write((short)images.Count);

            int offset = 6 + images.Count * 16;
            for (int i = 0; i < images.Count; i++)
            {
                int s = images[i].Width;
                bw.Write((byte)(s >= 256 ? 0 : s));
                bw.Write((byte)(s >= 256 ? 0 : s));
                bw.Write((byte)0);
                bw.Write((byte)0);
                bw.Write((short)1);
                bw.Write((short)32);
                bw.Write(blocks[i].Length);
                bw.Write(offset);
                offset += blocks[i].Length;
            }
            foreach (var b in blocks)
                bw.Write(b);
            bw.Flush();
            return ms.ToArray();
        }

        public static Bitmap BuildSilhouette(Bitmap src, int threshold, int maxSide = 48)
        {
            int side = Math.Min(maxSide, Math.Max(src.Width, src.Height));
            side = Math.Max(8, side);
            using var scaled = Compose(src, side, side, null, 0, 0, false, true);

            bool hasAlpha = false;
            for (int y = 0; y < scaled.Height && !hasAlpha; y++)
                for (int x = 0; x < scaled.Width; x++)
                    if (scaled.GetPixel(x, y).A < 250) { hasAlpha = true; break; }

            var mono = new Bitmap(side, side, PixelFormat.Format32bppArgb);
            for (int y = 0; y < side; y++)
                for (int x = 0; x < side; x++)
                {
                    var p = scaled.GetPixel(x, y);
                    bool on = hasAlpha
                        ? p.A >= 128
                        : (p.R * 299 + p.G * 587 + p.B * 114) / 1000 < threshold;
                    if (on)
                        mono.SetPixel(x, y, Color.Black);
                }
            return mono;
        }

        public static string BuildSafariSvg(Bitmap src, int threshold)
        {
            using var mono = BuildSilhouette(src, threshold);
            int w = mono.Width, h = mono.Height;
            var sb = new StringBuilder();
            sb.Append("<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 ")
              .Append(w).Append(' ').Append(h).Append("\"><path d=\"");
            for (int y = 0; y < h; y++)
            {
                int x = 0;
                while (x < w)
                {
                    if (mono.GetPixel(x, y).A < 128) { x++; continue; }
                    int run = 0;
                    while (x + run < w && mono.GetPixel(x + run, y).A >= 128)
                        run++;
                    sb.Append('M').Append(x).Append(' ').Append(y)
                      .Append('h').Append(run).Append("v1h-").Append(run).Append('z');
                    x += run;
                }
            }
            sb.Append("\" fill=\"#000000\"/></svg>");
            return sb.ToString();
        }

        private static string JsonEscape(string s) =>
            s.Replace("\\", "\\\\").Replace("\"", "\\\"");

        public static string Href(FaviconOptions o, string fileName)
        {
            string p = string.IsNullOrWhiteSpace(o.PathPrefix) ? "/" : o.PathPrefix.Trim();
            if (!p.EndsWith("/"))
                p += "/";
            return p + fileName;
        }

        public static string BuildManifest(FaviconOptions o)
        {
            var sb = new StringBuilder();
            sb.Append("{\n");
            sb.Append($"    \"name\": \"{JsonEscape(o.AppName)}\",\n");
            sb.Append($"    \"short_name\": \"{JsonEscape(o.ShortName)}\",\n");
            sb.Append("    \"icons\": [\n");
            sb.Append($"        {{ \"src\": \"{Href(o, "android-chrome-192x192.png")}\", \"sizes\": \"192x192\", \"type\": \"image/png\" }},\n");
            sb.Append($"        {{ \"src\": \"{Href(o, "android-chrome-512x512.png")}\", \"sizes\": \"512x512\", \"type\": \"image/png\" }},\n");
            sb.Append($"        {{ \"src\": \"{Href(o, "maskable-icon-512x512.png")}\", \"sizes\": \"512x512\", \"type\": \"image/png\", \"purpose\": \"maskable\" }}\n");
            sb.Append("    ],\n");
            sb.Append($"    \"theme_color\": \"{ToHex(o.ThemeColor)}\",\n");
            sb.Append($"    \"background_color\": \"{ToHex(o.ThemeColor)}\",\n");
            sb.Append("    \"display\": \"standalone\"\n");
            sb.Append("}\n");
            return sb.ToString();
        }

        public static string BuildBrowserConfig(FaviconOptions o)
        {
            var sb = new StringBuilder();
            sb.Append("<?xml version=\"1.0\" encoding=\"utf-8\"?>\n");
            sb.Append("<browserconfig>\n    <msapplication>\n        <tile>\n");
            sb.Append($"            <square70x70logo src=\"{Href(o, "mstile-70x70.png")}\"/>\n");
            sb.Append($"            <square150x150logo src=\"{Href(o, "mstile-150x150.png")}\"/>\n");
            sb.Append($"            <square310x310logo src=\"{Href(o, "mstile-310x310.png")}\"/>\n");
            sb.Append($"            <wide310x150logo src=\"{Href(o, "mstile-310x150.png")}\"/>\n");
            sb.Append($"            <TileColor>{ToHex(o.TileColor)}</TileColor>\n");
            sb.Append("        </tile>\n    </msapplication>\n</browserconfig>\n");
            return sb.ToString();
        }

        public static string BuildHtml(FaviconOptions o)
        {
            var sb = new StringBuilder();
            sb.Append($"<link rel=\"apple-touch-icon\" sizes=\"180x180\" href=\"{Href(o, "apple-touch-icon.png")}\">\n");
            sb.Append($"<meta name=\"apple-mobile-web-app-title\" content=\"{o.AppleTitle.Replace("\"", "&quot;")}\">\n");
            sb.Append($"<link rel=\"icon\" type=\"image/png\" sizes=\"48x48\" href=\"{Href(o, "favicon-48x48.png")}\">\n");
            sb.Append($"<link rel=\"icon\" type=\"image/png\" sizes=\"32x32\" href=\"{Href(o, "favicon-32x32.png")}\">\n");
            sb.Append($"<link rel=\"icon\" type=\"image/png\" sizes=\"16x16\" href=\"{Href(o, "favicon-16x16.png")}\">\n");
            sb.Append($"<link rel=\"shortcut icon\" href=\"{Href(o, "favicon.ico")}\">\n");
            sb.Append($"<link rel=\"manifest\" href=\"{Href(o, "site.webmanifest")}\">\n");
            sb.Append($"<link rel=\"mask-icon\" href=\"{Href(o, "safari-pinned-tab.svg")}\" color=\"{ToHex(o.SafariColor)}\">\n");
            sb.Append($"<meta name=\"msapplication-TileColor\" content=\"{ToHex(o.TileColor)}\">\n");
            sb.Append($"<meta name=\"msapplication-config\" content=\"{Href(o, "browserconfig.xml")}\">\n");
            sb.Append($"<meta name=\"theme-color\" content=\"{ToHex(o.ThemeColor)}\">\n");
            return sb.ToString();
        }

        public static Bitmap ComposeFavicon(Bitmap src, FaviconOptions o, int size) =>
            Compose(src, size, size,
                o.FaviconBackground ? o.FaviconBackColor : null,
                o.FaviconMargin, o.FaviconRadius, false, o.PixelArt);

        public static Bitmap ComposeIos(Bitmap src, FaviconOptions o, int size = 180) =>
            Compose(src, size, size,
                o.IosBackground ? o.IosBackColor : null,
                o.IosMargin, 0, false, o.PixelArt);

        public static Bitmap ComposeAndroid(Bitmap src, FaviconOptions o, int size) =>
            Compose(src, size, size,
                o.AndroidBackground ? o.AndroidBackColor : null,
                o.AndroidMargin, 0, o.AndroidCircle, o.PixelArt);

        public static Bitmap ComposeMaskable(Bitmap src, FaviconOptions o, int size = 512) =>
            Compose(src, size, size,
                o.AndroidBackground ? o.AndroidBackColor : o.ThemeColor,
                Math.Max(o.AndroidMargin, 20), 0, false, o.PixelArt);

        public static Bitmap ComposeTile(Bitmap src, FaviconOptions o, int w, int h) =>
            Compose(src, w, h, null, o.TileMargin, 0, false, o.PixelArt);

        public static Dictionary<string, byte[]> GenerateAll(Bitmap src, FaviconOptions o)
        {
            var files = new Dictionary<string, byte[]>();

            var icoSizes = new[] { 16, 32, 48 };
            var icoImages = new List<Bitmap>();
            try
            {
                foreach (int s in icoSizes)
                    icoImages.Add(ComposeFavicon(src, o, s));
                files["favicon.ico"] = BuildIco(icoImages);
                for (int i = 0; i < icoSizes.Length; i++)
                    files[$"favicon-{icoSizes[i]}x{icoSizes[i]}.png"] = PngBytes(icoImages[i]);
            }
            finally
            {
                foreach (var b in icoImages)
                    b.Dispose();
            }

            using (var ios = ComposeIos(src, o))
                files["apple-touch-icon.png"] = PngBytes(ios);

            using (var a192 = ComposeAndroid(src, o, 192))
                files["android-chrome-192x192.png"] = PngBytes(a192);
            using (var a512 = ComposeAndroid(src, o, 512))
                files["android-chrome-512x512.png"] = PngBytes(a512);
            using (var mask = ComposeMaskable(src, o))
                files["maskable-icon-512x512.png"] = PngBytes(mask);

            using (var t = ComposeTile(src, o, 70, 70))
                files["mstile-70x70.png"] = PngBytes(t);
            using (var t = ComposeTile(src, o, 144, 144))
                files["mstile-144x144.png"] = PngBytes(t);
            using (var t = ComposeTile(src, o, 150, 150))
                files["mstile-150x150.png"] = PngBytes(t);
            using (var t = ComposeTile(src, o, 310, 150))
                files["mstile-310x150.png"] = PngBytes(t);
            using (var t = ComposeTile(src, o, 310, 310))
                files["mstile-310x310.png"] = PngBytes(t);

            files["safari-pinned-tab.svg"] = Encoding.UTF8.GetBytes(BuildSafariSvg(src, o.SafariThreshold));
            files["site.webmanifest"] = Encoding.UTF8.GetBytes(BuildManifest(o));
            files["browserconfig.xml"] = Encoding.UTF8.GetBytes(BuildBrowserConfig(o));
            files["html-code.html"] = Encoding.UTF8.GetBytes(BuildHtml(o));
            return files;
        }

        public static void WriteZip(Dictionary<string, byte[]> files, string zipPath)
        {
            using var fs = new FileStream(zipPath, FileMode.Create);
            using var zip = new ZipArchive(fs, ZipArchiveMode.Create);
            foreach (var (name, data) in files)
            {
                var entry = zip.CreateEntry(name, CompressionLevel.Optimal);
                using var es = entry.Open();
                es.Write(data, 0, data.Length);
            }
        }

        public static void WriteFolder(Dictionary<string, byte[]> files, string folder)
        {
            Directory.CreateDirectory(folder);
            foreach (var (name, data) in files)
                File.WriteAllBytes(Path.Combine(folder, name), data);
        }
    }
}
