using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace KebuzForge.App.Core
{
    internal static class PaletteManager
    {

        public static Color[] MedianCut(Bitmap source, int colorCount)
        {
            colorCount = Math.Clamp(colorCount, 2, 256);

            var pixels = SamplePixels(source, maxSamples: 65_536);
            if (pixels.Count == 0) return [Color.Black];

            var buckets = new List<List<Color>> { pixels };

            while (buckets.Count < colorCount)
            {
                int splitIdx = FindWidestBucket(buckets);
                if (splitIdx < 0) break;

                var bucket = buckets[splitIdx];
                buckets.RemoveAt(splitIdx);

                int ch = FindLargestChannel(bucket);
                bucket.Sort((a, b) => Channel(a, ch) - Channel(b, ch));

                int mid = bucket.Count / 2;
                buckets.Add(bucket.Take(mid).ToList());
                buckets.Add(bucket.Skip(mid).ToList());
            }

            return buckets.Select(AverageColor).ToArray();
        }

        public static Bitmap ApplyPalette(Bitmap source, Color[] palette)
        {
            int w = source.Width, h = source.Height;
            var result  = new Bitmap(w, h);
            var srcRect = new Rectangle(0, 0, w, h);

            var srcData = source.LockBits(srcRect, ImageLockMode.ReadOnly,  PixelFormat.Format32bppArgb);
            var dstData = result.LockBits(srcRect, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            int stride  = srcData.Stride;

            var src = new byte[h * stride];
            var dst = new byte[h * stride];
            Marshal.Copy(srcData.Scan0, src, 0, src.Length);

            for (int i = 0; i < src.Length; i += 4)
            {
                byte a = src[i + 3];
                if (a == 0) { dst[i] = dst[i+1] = dst[i+2] = dst[i+3] = 0; continue; }

                Color nearest = FindNearest(src[i+2], src[i+1], src[i], palette);
                dst[i]     = nearest.B;
                dst[i + 1] = nearest.G;
                dst[i + 2] = nearest.R;
                dst[i + 3] = a;
            }

            Marshal.Copy(dst, 0, dstData.Scan0, dst.Length);
            source.UnlockBits(srcData);
            result.UnlockBits(dstData);
            return result;
        }

        public static Bitmap ApplyPaletteIndexed(Bitmap source, Color[] palette, out byte[] indices)
        {
            int w = source.Width, h = source.Height;
            indices = new byte[w * h];
            var result  = new Bitmap(w, h);
            var srcRect = new Rectangle(0, 0, w, h);

            var srcData = source.LockBits(srcRect, ImageLockMode.ReadOnly,  PixelFormat.Format32bppArgb);
            var dstData = result.LockBits(srcRect, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            int stride  = srcData.Stride;

            var src = new byte[h * stride];
            var dst = new byte[h * stride];
            Marshal.Copy(srcData.Scan0, src, 0, src.Length);

            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                {
                    int i  = y * stride + x * 4;
                    int pi = y * w + x;
                    byte a = src[i + 3];
                    if (a == 0) { dst[i] = dst[i+1] = dst[i+2] = dst[i+3] = 0; continue; }

                    int idx = FindNearestIndex(src[i+2], src[i+1], src[i], palette);
                    indices[pi] = (byte)idx;
                    Color c    = palette[idx];
                    dst[i]     = c.B;
                    dst[i + 1] = c.G;
                    dst[i + 2] = c.R;
                    dst[i + 3] = a;
                }

            Marshal.Copy(dst, 0, dstData.Scan0, dst.Length);
            source.UnlockBits(srcData);
            result.UnlockBits(dstData);
            return result;
        }

        public static Bitmap FromIndices(byte[] indices, Bitmap alphaSource, Color[] palette)
        {
            int w = alphaSource.Width, h = alphaSource.Height;
            var result  = new Bitmap(w, h);
            var rect    = new Rectangle(0, 0, w, h);

            var srcData = alphaSource.LockBits(rect, ImageLockMode.ReadOnly,  PixelFormat.Format32bppArgb);
            var dstData = result.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            int stride  = srcData.Stride;

            var src = new byte[h * stride];
            var dst = new byte[h * stride];
            Marshal.Copy(srcData.Scan0, src, 0, src.Length);

            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                {
                    int i  = y * stride + x * 4;
                    int pi = y * w + x;
                    byte a = src[i + 3];
                    if (a == 0) { dst[i] = dst[i+1] = dst[i+2] = dst[i+3] = 0; continue; }

                    Color c = palette[Math.Min(indices[pi], (byte)(palette.Length - 1))];
                    dst[i]     = c.B;
                    dst[i + 1] = c.G;
                    dst[i + 2] = c.R;
                    dst[i + 3] = a;
                }

            Marshal.Copy(dst, 0, dstData.Scan0, dst.Length);
            alphaSource.UnlockBits(srcData);
            result.UnlockBits(dstData);
            return result;
        }

        public static void RecolorIndexed(Bitmap target, byte[] indices, int index, Color color)
        {
            int w = target.Width, h = target.Height;
            var rect = new Rectangle(0, 0, w, h);
            var data = target.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            int stride = data.Stride;
            var buf = new byte[h * stride];
            Marshal.Copy(data.Scan0, buf, 0, buf.Length);

            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                {
                    int pi = y * w + x;
                    if (indices[pi] != index) continue;
                    int i = y * stride + x * 4;
                    if (buf[i + 3] == 0) continue;
                    buf[i]     = color.B;
                    buf[i + 1] = color.G;
                    buf[i + 2] = color.R;
                }

            Marshal.Copy(buf, 0, data.Scan0, buf.Length);
            target.UnlockBits(data);
        }

        internal static int FindNearestIndex(byte r, byte g, byte b, Color[] palette)
        {
            int best = 0;
            int bestDist = int.MaxValue;
            for (int i = 0; i < palette.Length; i++)
            {
                int dr = r - palette[i].R, dg = g - palette[i].G, db = b - palette[i].B;
                int dist = dr*dr + dg*dg + db*db;
                if (dist < bestDist) { bestDist = dist; best = i; }
                if (dist == 0) break;
            }
            return best;
        }

        public static Color[]? GetRetroPalette(string name) => name switch
        {
            "CGA4"       => CGA4,
            "CGA16"      => CGA16,
            "EGA16"      => EGA16,
            "GameBoy"    => GameBoyDMG,
            "ZXSpectrum" => ZXSpectrum,
            "Amiga32"    => Amiga32,
            "Amiga64"    => Amiga64,
            _            => null
        };

        public static string? ComboIndexToKey(int index) => index switch
        {
            1 => "CGA4",
            2 => "CGA16",
            3 => "EGA16",
            4 => "GameBoy",
            5 => "ZXSpectrum",
            6 => "Amiga32",
            7 => "Amiga64",
            _ => null
        };

        public static void SavePalette(Color[] palette, string path)
        {
            switch (Path.GetExtension(path).ToLowerInvariant())
            {
                case ".act": SaveACT(palette, path); break;
                case ".gpl": SaveGPL(palette, path); break;
                default:     SavePAL(palette, path); break;
            }
        }

        public static Color[] LoadPalette(string path)
        {
            return Path.GetExtension(path).ToLowerInvariant() switch
            {
                ".act" => LoadACT(path),
                ".gpl" => LoadGPL(path),
                _      => LoadPAL(path)
            };
        }

        private static void SavePAL(Color[] pal, string path)
        {
            var sb = new StringBuilder();
            sb.AppendLine("JASC-PAL");
            sb.AppendLine("0100");
            sb.AppendLine(pal.Length.ToString());
            foreach (var c in pal)
                sb.AppendLine($"{c.R} {c.G} {c.B}");
            File.WriteAllText(path, sb.ToString());
        }

        private static Color[] LoadPAL(string path)
        {
            var lines = File.ReadAllLines(path);
            var colors = new List<Color>();
            bool reading = false;
            foreach (var raw in lines)
            {
                var line = raw.Trim();
                if (line == "JASC-PAL" || line == "0100") continue;
                if (!reading && int.TryParse(line, out _)) { reading = true; continue; }
                if (!reading) continue;
                var parts = line.Split(' ');
                if (parts.Length >= 3 &&
                    byte.TryParse(parts[0], out byte r) &&
                    byte.TryParse(parts[1], out byte g) &&
                    byte.TryParse(parts[2], out byte b))
                    colors.Add(Color.FromArgb(r, g, b));
            }
            return colors.Count > 0 ? colors.ToArray() : [Color.Black];
        }

        private static void SaveACT(Color[] pal, string path)
        {
            var buf = new byte[768];
            for (int i = 0; i < Math.Min(pal.Length, 256); i++)
            {
                buf[i * 3]     = pal[i].R;
                buf[i * 3 + 1] = pal[i].G;
                buf[i * 3 + 2] = pal[i].B;
            }
            File.WriteAllBytes(path, buf);
        }

        private static Color[] LoadACT(string path)
        {
            var buf = File.ReadAllBytes(path);
            int count = buf.Length / 3;
            var colors = new Color[count];
            for (int i = 0; i < count; i++)
                colors[i] = Color.FromArgb(buf[i*3], buf[i*3+1], buf[i*3+2]);
            return colors;
        }

        private static void SaveGPL(Color[] pal, string path)
        {
            var sb = new StringBuilder();
            sb.AppendLine("GIMP Palette");
            sb.AppendLine("Name: KebuzForge");
            sb.AppendLine("#");
            foreach (var c in pal)
                sb.AppendLine($"{c.R,3} {c.G,3} {c.B,3}\t#{c.R:X2}{c.G:X2}{c.B:X2}");
            File.WriteAllText(path, sb.ToString());
        }

        private static Color[] LoadGPL(string path)
        {
            var colors = new List<Color>();
            foreach (var raw in File.ReadAllLines(path))
            {
                var line = raw.Trim();
                if (line.StartsWith('#') || line.StartsWith("GIMP") || line.StartsWith("Name")) continue;
                var parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 3 &&
                    byte.TryParse(parts[0], out byte r) &&
                    byte.TryParse(parts[1], out byte g) &&
                    byte.TryParse(parts[2], out byte b))
                    colors.Add(Color.FromArgb(r, g, b));
            }
            return colors.Count > 0 ? colors.ToArray() : [Color.Black];
        }

        private static List<Color> SamplePixels(Bitmap bmp, int maxSamples)
        {
            var rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            var data = bmp.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            int stride = data.Stride;
            var buf = new byte[bmp.Height * stride];
            Marshal.Copy(data.Scan0, buf, 0, buf.Length);
            bmp.UnlockBits(data);

            int total = bmp.Width * bmp.Height;
            int step  = Math.Max(1, total / maxSamples);
            var result = new List<Color>(Math.Min(total, maxSamples));

            for (int idx = 0; idx < buf.Length; idx += 4 * step)
            {
                if (idx + 3 >= buf.Length) break;
                if (buf[idx + 3] == 0) continue;
                result.Add(Color.FromArgb(buf[idx+2], buf[idx+1], buf[idx]));
            }
            return result;
        }

        private static int FindWidestBucket(List<List<Color>> buckets)
        {
            int best = -1, maxRange = -1;
            for (int i = 0; i < buckets.Count; i++)
            {
                if (buckets[i].Count <= 1) continue;
                int range = BucketRange(buckets[i]);
                if (range > maxRange) { maxRange = range; best = i; }
            }
            return best;
        }

        private static int BucketRange(List<Color> bucket)
        {
            int minR = 255, maxR = 0, minG = 255, maxG = 0, minB = 255, maxB = 0;
            foreach (var c in bucket)
            {
                if (c.R < minR) minR = c.R; if (c.R > maxR) maxR = c.R;
                if (c.G < minG) minG = c.G; if (c.G > maxG) maxG = c.G;
                if (c.B < minB) minB = c.B; if (c.B > maxB) maxB = c.B;
            }
            return (maxR - minR) + (maxG - minG) + (maxB - minB);
        }

        private static int FindLargestChannel(List<Color> bucket)
        {
            int minR = 255, maxR = 0, minG = 255, maxG = 0, minB = 255, maxB = 0;
            foreach (var c in bucket)
            {
                if (c.R < minR) minR = c.R; if (c.R > maxR) maxR = c.R;
                if (c.G < minG) minG = c.G; if (c.G > maxG) maxG = c.G;
                if (c.B < minB) minB = c.B; if (c.B > maxB) maxB = c.B;
            }
            int rr = maxR - minR, gg = maxG - minG, bb = maxB - minB;
            return gg >= rr && gg >= bb ? 1 : (rr >= bb ? 0 : 2);
        }

        private static int Channel(Color c, int ch) => ch switch { 0 => c.R, 1 => c.G, _ => c.B };

        private static Color AverageColor(List<Color> bucket)
        {
            if (bucket.Count == 0) return Color.Black;
            long r = 0, g = 0, b = 0;
            foreach (var c in bucket) { r += c.R; g += c.G; b += c.B; }
            int n = bucket.Count;
            return Color.FromArgb((int)(r/n), (int)(g/n), (int)(b/n));
        }

        internal static Color FindNearest(byte r, byte g, byte b, Color[] palette)
        {
            Color best = palette[0];
            int bestDist = int.MaxValue;
            foreach (var c in palette)
            {
                int dr = r - c.R, dg = g - c.G, db = b - c.B;
                int dist = dr*dr + dg*dg + db*db;
                if (dist < bestDist) { bestDist = dist; best = c; }
                if (dist == 0) break;
            }
            return best;
        }

        private static readonly Color[] CGA4 =
        [
            Color.FromArgb(0,0,0), Color.FromArgb(85,255,255),
            Color.FromArgb(255,85,255), Color.FromArgb(255,255,255)
        ];

        private static readonly Color[] CGA16 =
        [
            Color.FromArgb(0,0,0),       Color.FromArgb(0,0,170),
            Color.FromArgb(0,170,0),     Color.FromArgb(0,170,170),
            Color.FromArgb(170,0,0),     Color.FromArgb(170,0,170),
            Color.FromArgb(170,85,0),    Color.FromArgb(170,170,170),
            Color.FromArgb(85,85,85),    Color.FromArgb(85,85,255),
            Color.FromArgb(85,255,85),   Color.FromArgb(85,255,255),
            Color.FromArgb(255,85,85),   Color.FromArgb(255,85,255),
            Color.FromArgb(255,255,85),  Color.FromArgb(255,255,255)
        ];

        private static readonly Color[] EGA16 = CGA16;

        private static readonly Color[] GameBoyDMG =
        [
            Color.FromArgb(15,56,15),   Color.FromArgb(48,98,48),
            Color.FromArgb(139,172,15), Color.FromArgb(155,188,15)
        ];

        private static readonly Color[] ZXSpectrum =
        [
            Color.FromArgb(0,0,0),       Color.FromArgb(0,0,192),
            Color.FromArgb(192,0,0),     Color.FromArgb(192,0,192),
            Color.FromArgb(0,192,0),     Color.FromArgb(0,192,192),
            Color.FromArgb(192,192,0),   Color.FromArgb(192,192,192),
            Color.FromArgb(0,0,255),     Color.FromArgb(255,0,0),
            Color.FromArgb(255,0,255),   Color.FromArgb(0,255,0),
            Color.FromArgb(0,255,255),   Color.FromArgb(255,255,0),
            Color.FromArgb(255,255,255)
        ];

        private static readonly Color[] Amiga32 =
        [
            Color.FromArgb(0,0,0),       Color.FromArgb(255,255,255),
            Color.FromArgb(136,0,0),     Color.FromArgb(170,34,34),
            Color.FromArgb(255,85,85),   Color.FromArgb(255,170,170),
            Color.FromArgb(0,136,0),     Color.FromArgb(34,170,34),
            Color.FromArgb(85,255,85),   Color.FromArgb(170,255,170),
            Color.FromArgb(0,0,136),     Color.FromArgb(34,34,170),
            Color.FromArgb(85,85,255),   Color.FromArgb(170,170,255),
            Color.FromArgb(136,136,0),   Color.FromArgb(170,170,34),
            Color.FromArgb(255,255,85),  Color.FromArgb(255,255,170),
            Color.FromArgb(0,136,136),   Color.FromArgb(34,170,170),
            Color.FromArgb(85,255,255),  Color.FromArgb(170,255,255),
            Color.FromArgb(136,0,136),   Color.FromArgb(170,34,170),
            Color.FromArgb(255,85,255),  Color.FromArgb(255,170,255),
            Color.FromArgb(85,85,85),    Color.FromArgb(119,119,119),
            Color.FromArgb(153,153,153), Color.FromArgb(187,187,187),
            Color.FromArgb(221,221,221), Color.FromArgb(238,238,238)
        ];

        private static readonly Color[] Amiga64 = BuildAmiga64();

        private static Color[] BuildAmiga64()
        {
            var list = new List<Color>(Amiga32);

            Color[] extras =
            [
                Color.FromArgb(68,0,0),    Color.FromArgb(204,68,0),
                Color.FromArgb(0,68,0),    Color.FromArgb(0,204,68),
                Color.FromArgb(0,0,68),    Color.FromArgb(68,0,204),
                Color.FromArgb(68,68,0),   Color.FromArgb(204,204,68),
                Color.FromArgb(0,68,68),   Color.FromArgb(68,204,204),
                Color.FromArgb(68,0,68),   Color.FromArgb(204,68,204),
                Color.FromArgb(34,17,0),   Color.FromArgb(119,68,17),
                Color.FromArgb(204,136,68),Color.FromArgb(255,204,136),
                Color.FromArgb(17,34,51),  Color.FromArgb(51,85,119),
                Color.FromArgb(85,136,187),Color.FromArgb(136,187,221),
                Color.FromArgb(51,34,17),  Color.FromArgb(102,68,34),
                Color.FromArgb(153,102,51),Color.FromArgb(204,153,102),
                Color.FromArgb(17,51,34),  Color.FromArgb(51,102,68),
                Color.FromArgb(102,153,102),Color.FromArgb(153,204,153),
                Color.FromArgb(255,136,0), Color.FromArgb(255,170,68),
                Color.FromArgb(0,170,255), Color.FromArgb(68,204,255)
            ];
            list.AddRange(extras);
            return list.Take(64).ToArray();
        }
    }
}
