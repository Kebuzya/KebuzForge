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

        public static Bitmap SwapColor(Bitmap source, Color from, Color to)
        {
            int w = source.Width, h = source.Height;
            var result = new Bitmap(w, h, PixelFormat.Format32bppArgb);
            var rect = new Rectangle(0, 0, w, h);

            var srcData = source.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            var dstData = result.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            int stride = srcData.Stride;

            var buf = new byte[h * stride];
            Marshal.Copy(srcData.Scan0, buf, 0, buf.Length);

            for (int i = 0; i < buf.Length; i += 4)
            {
                if (buf[i + 3] == 0) continue;
                if (buf[i] == from.B && buf[i + 1] == from.G && buf[i + 2] == from.R)
                {
                    buf[i]     = to.B;
                    buf[i + 1] = to.G;
                    buf[i + 2] = to.R;
                }
            }

            Marshal.Copy(buf, 0, dstData.Scan0, buf.Length);
            source.UnlockBits(srcData);
            result.UnlockBits(dstData);
            return result;
        }

        public static byte[] ComputeIndices(Bitmap source, Color[] palette)
        {
            int w = source.Width, h = source.Height;
            var indices = new byte[w * h];
            byte[] src = ImageProcessor.LockCopy(source, out int stride);

            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                {
                    int i = y * stride + x * 4;
                    if (src[i + 3] == 0) continue;
                    indices[y * w + x] = (byte)FindNearestIndex(src[i + 2], src[i + 1], src[i], palette);
                }

            return indices;
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

        public static bool Perceptual { get; set; } = true;

        internal static int FindNearestIndex(byte r, byte g, byte b, Color[] palette)
        {
            if (!Perceptual) return NearestRgb(r, g, b, palette);
            var lab = PaletteLab(palette);
            var cache = _nearestCache!;
            int rgb = r << 16 | g << 8 | b;
            if (cache.TryGetValue(rgb, out int hit)) return hit;

            var p = Lab(r, g, b);
            double c1 = Math.Sqrt(p.A * p.A + p.B * p.B);
            double sc = 1 + 0.045 * c1, sh = 1 + 0.015 * c1;
            int best = 0;
            double bestDist = double.MaxValue;
            for (int i = 0; i < lab.Length; i++)
            {
                double c2 = Math.Sqrt(lab[i].A * lab[i].A + lab[i].B * lab[i].B);
                double dl = p.L - lab[i].L, dc = c1 - c2, da = p.A - lab[i].A, db = p.B - lab[i].B;
                double dh2 = Math.Max(0, da * da + db * db - dc * dc);
                double dist = dl * dl + dc * dc / (sc * sc) + dh2 / (sh * sh);
                if (dist < bestDist) { bestDist = dist; best = i; }
            }
            if (cache.Count > 1 << 18) cache.Clear();
            cache[rgb] = best;
            return best;
        }

        private static int NearestRgb(byte r, byte g, byte b, Color[] palette)
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

        [ThreadStatic] private static int[]? _labKey;
        [ThreadStatic] private static (double L, double A, double B)[]? _labPalette;
        [ThreadStatic] private static Dictionary<int, int>? _nearestCache;

        private static (double L, double A, double B)[] PaletteLab(Color[] palette)
        {
            var key = _labKey;
            bool same = key is not null && key.Length == palette.Length;
            for (int i = 0; same && i < palette.Length; i++)
                same = key![i] == palette[i].ToArgb();
            if (!same)
            {
                _labKey = palette.Select(c => c.ToArgb()).ToArray();
                _labPalette = palette.Select(c => Lab(c.R, c.G, c.B)).ToArray();
                _nearestCache = new Dictionary<int, int>();
            }
            return _labPalette!;
        }

        private static readonly double[] Linear = Enumerable.Range(0, 256)
            .Select(v => v / 255.0)
            .Select(x => x <= 0.04045 ? x / 12.92 : Math.Pow((x + 0.055) / 1.055, 2.4))
            .ToArray();

        private static (double L, double A, double B) Lab(int r, int g, int b)
        {
            double lr = Linear[r], lg = Linear[g], lb = Linear[b];
            double fx = LabF((0.4124564 * lr + 0.3575761 * lg + 0.1804375 * lb) / 0.95047);
            double fy = LabF(0.2126729 * lr + 0.7151522 * lg + 0.0721750 * lb);
            double fz = LabF((0.0193339 * lr + 0.1191920 * lg + 0.9503041 * lb) / 1.08883);
            return (116 * fy - 16, 500 * (fx - fy), 200 * (fy - fz));
        }

        private static double LabF(double t) => t > 0.008856 ? Math.Cbrt(t) : 7.787 * t + 16.0 / 116.0;

        private readonly record struct ColorBin(Color Color, int Count);

        private const int MaxDist = 3 * 255 * 255 + 1;

        private static int Dist(Color a, Color b)
        {
            int dr = a.R - b.R, dg = a.G - b.G, db = a.B - b.B;
            return dr * dr + dg * dg + db * db;
        }

        private static List<ColorBin> ColorBins(Bitmap source)
        {
            byte[] buf = ImageProcessor.LockCopy(source, out int stride);
            var count = new int[32768];
            var sr = new long[32768];
            var sg = new long[32768];
            var sb = new long[32768];
            for (int y = 0; y < source.Height; y++)
                for (int x = 0; x < source.Width; x++)
                {
                    int i = y * stride + x * 4;
                    if (buf[i + 3] == 0) continue;
                    int key = (buf[i + 2] >> 3) << 10 | (buf[i + 1] >> 3) << 5 | buf[i] >> 3;
                    count[key]++;
                    sr[key] += buf[i + 2];
                    sg[key] += buf[i + 1];
                    sb[key] += buf[i];
                }

            var bins = new List<ColorBin>();
            for (int k = 0; k < count.Length; k++)
            {
                int n = count[k];
                if (n == 0) continue;
                bins.Add(new ColorBin(Color.FromArgb((int)(sr[k] / n), (int)(sg[k] / n), (int)(sb[k] / n)), n));
            }
            return bins;
        }

        private const double AccentBoost = 2.0;

        private static double Weight(ColorBin bin)
        {
            var c = bin.Color;
            int max = Math.Max(c.R, Math.Max(c.G, c.B)), min = Math.Min(c.R, Math.Min(c.G, c.B));
            return Math.Sqrt(bin.Count) * (1.0 + AccentBoost * (max - min) / 255.0);
        }

        public static Color? MostMissingColor(Bitmap source, Color[] palette, int minDistance = 0)
        {
            var bins = ColorBins(source);
            if (bins.Count == 0) return null;
            var weight = bins.Select(Weight).ToArray();

            var err = new int[bins.Count];
            for (int i = 0; i < bins.Count; i++)
            {
                var c = bins[i].Color;
                err[i] = palette.Length == 0 ? MaxDist : Dist(c, palette[NearestRgb(c.R, c.G, c.B, palette)]);
            }

            double Gain(Color c)
            {
                double gain = 0;
                for (int i = 0; i < bins.Count; i++)
                {
                    int d = Dist(bins[i].Color, c);
                    if (d < err[i]) gain += weight[i] * (err[i] - d);
                }
                return gain;
            }

            int minDist2 = minDistance * minDistance;
            var candidates = Enumerable.Range(0, bins.Count)
                .Where(i => err[i] > 0 && err[i] >= minDist2)
                .OrderByDescending(i => weight[i] * err[i])
                .Take(256);

            int best = -1;
            double bestGain = 0;
            foreach (int ci in candidates)
            {
                double gain = Gain(bins[ci].Color);
                if (gain > bestGain) { bestGain = gain; best = ci; }
            }
            if (best < 0) return null;

            int peak = best;
            for (int i = 0; i < bins.Count; i++)
                if (bins[i].Count > bins[peak].Count && Dist(bins[i].Color, bins[best].Color) <= PeakRadius * PeakRadius && err[i] > PeakRadius * PeakRadius && err[i] >= minDist2)
                    peak = i;
            var snapped = ExactModes(source, [bins[peak].Color])[0];
            if (palette.Length > 0 && Dist(snapped, palette[NearestRgb(snapped.R, snapped.G, snapped.B, palette)]) < Math.Max(1, minDist2))
                return bins[peak].Color;
            return snapped;
        }

        private const int PeakRadius = 24;

        private static int BinKey(int r, int g, int b) => (r >> 3) << 10 | (g >> 3) << 5 | b >> 3;

        private static Color[] ExactModes(Bitmap source, Color[] colors)
        {
            var keys = colors.Select(c => BinKey(c.R, c.G, c.B)).ToArray();
            var counts = new Dictionary<int, Dictionary<int, int>>();
            foreach (int k in keys) counts.TryAdd(k, new Dictionary<int, int>());
            byte[] buf = ImageProcessor.LockCopy(source, out int stride);
            for (int y = 0; y < source.Height; y++)
                for (int x = 0; x < source.Width; x++)
                {
                    int i = y * stride + x * 4;
                    if (buf[i + 3] == 0) continue;
                    if (!counts.TryGetValue(BinKey(buf[i + 2], buf[i + 1], buf[i]), out var bin)) continue;
                    int rgb = buf[i + 2] << 16 | buf[i + 1] << 8 | buf[i];
                    bin[rgb] = bin.GetValueOrDefault(rgb) + 1;
                }
            return colors.Select((c, n) =>
            {
                var bin = counts[keys[n]];
                if (bin.Count == 0) return c;
                int rgb = bin.MaxBy(p => p.Value).Key;
                return Color.FromArgb(rgb >> 16 & 255, rgb >> 8 & 255, rgb & 255);
            }).ToArray();
        }

        private static Color[] KMeansRefine(Bitmap source, Color[] seeds)
        {
            var bins = ColorBins(source);
            int k = seeds.Length;
            if (bins.Count == 0 || k == 0) return seeds;
            var cr = seeds.Select(c => (double)c.R).ToArray();
            var cg = seeds.Select(c => (double)c.G).ToArray();
            var cb = seeds.Select(c => (double)c.B).ToArray();
            var assign = new int[bins.Count];
            for (int it = 0; it < 15; it++)
            {
                bool changed = it == 0;
                for (int i = 0; i < bins.Count; i++)
                {
                    var c = bins[i].Color;
                    int best = 0;
                    double bestDist = double.MaxValue;
                    for (int j = 0; j < k; j++)
                    {
                        double dr = c.R - cr[j], dg = c.G - cg[j], db = c.B - cb[j];
                        double d = dr * dr + dg * dg + db * db;
                        if (d < bestDist) { bestDist = d; best = j; }
                    }
                    if (assign[i] != best) { assign[i] = best; changed = true; }
                }
                if (!changed) break;

                for (int j = 0; j < k; j++)
                {
                    var members = Enumerable.Range(0, bins.Count).Where(i => assign[i] == j).ToList();
                    if (members.Count == 0) continue;
                    cr[j] = WeightedMedian(members, i => bins[i].Color.R, i => bins[i].Count);
                    cg[j] = WeightedMedian(members, i => bins[i].Color.G, i => bins[i].Count);
                    cb[j] = WeightedMedian(members, i => bins[i].Color.B, i => bins[i].Count);
                }
            }

            var centers = Enumerable.Range(0, k).Select(j => Color.FromArgb((int)cr[j], (int)cg[j], (int)cb[j])).ToArray();
            var result = new List<Color>();
            foreach (var c in ExactModes(source, centers))
                if (result.All(r => Dist(r, c) >= SameColor * SameColor))
                    result.Add(c);
            int distinct = k <= 32 ? (int)(50.0 / Math.Cbrt(k)) : SameColor;
            while (result.Count < k)
            {
                var add = MostMissingColor(source, result.ToArray(), distinct);
                if (add is null) break;
                result.Add(add.Value);
            }
            return result.ToArray();
        }

        private const int SameColor = 8;

        private static int WeightedMedian(List<int> members, Func<int, int> value, Func<int, int> weight)
        {
            var sorted = members.OrderBy(value).ToList();
            long half = (sorted.Sum(i => (long)weight(i)) + 1) / 2, acc = 0;
            foreach (int i in sorted)
            {
                acc += weight(i);
                if (acc >= half) return value(i);
            }
            return value(sorted[^1]);
        }

        public static Color[] OwnPalette(Bitmap source, int colorCount)
        {
            colorCount = Math.Clamp(colorCount, 2, 256);
            var cols = ExactColors(source);
            if (cols is null) return SpreadOut(source, KMeansRefine(source, MedianCut(source, colorCount)));
            if (cols.Count == 0) return [Color.Black];
            if (cols.Count <= colorCount) return cols.Select(c => c.Color).ToArray();

            long total = cols.Sum(c => (long)c.Count);
            long top = cols.Take(colorCount).Sum(c => (long)c.Count);
            if (top * 10 < total * 7) return SpreadOut(source, KMeansRefine(source, MedianCut(source, colorCount)));

            var err = new int[cols.Count];
            var weight = cols.Select(Weight).ToArray();
            var palette = new List<Color> { cols[0].Color };
            for (int i = 0; i < cols.Count; i++)
                err[i] = Dist(cols[i].Color, cols[0].Color);

            int candidateLimit = cols.Count > 1024 ? 64 : 256;
            while (palette.Count < colorCount)
            {
                var candidates = Enumerable.Range(0, cols.Count)
                    .Where(i => err[i] > 0)
                    .OrderByDescending(i => weight[i] * err[i])
                    .Take(candidateLimit)
                    .ToArray();
                int best = -1;
                double bestGain = 0;
                foreach (int ci in candidates)
                {
                    double gain = 0;
                    var c = cols[ci].Color;
                    for (int i = 0; i < cols.Count; i++)
                    {
                        int d = Dist(cols[i].Color, c);
                        if (d < err[i]) gain += weight[i] * (err[i] - d);
                    }
                    if (gain > bestGain) { bestGain = gain; best = ci; }
                }
                if (best < 0) break;
                palette.Add(cols[best].Color);
                for (int i = 0; i < cols.Count; i++)
                    err[i] = Math.Min(err[i], Dist(cols[i].Color, cols[best].Color));
            }

            var seeds = palette.ToArray();
            var snapped = palette.ToArray();
            var topCount = new int[seeds.Length];
            foreach (var col in cols)
            {
                int near = NearestRgb(col.Color.R, col.Color.G, col.Color.B, seeds);
                if (col.Count > topCount[near]) { topCount[near] = col.Count; snapped[near] = col.Color; }
            }
            return SpreadOut(source, snapped.Distinct().ToArray());
        }

        private static Color[] SpreadOut(Bitmap source, Color[] palette)
        {
            if (palette.Length < 2 || palette.Length > 32) return palette;
            int minDistance = (int)(50.0 / Math.Cbrt(palette.Length));
            int minDist2 = minDistance * minDistance;
            byte[] buf = ImageProcessor.LockCopy(source, out int stride);
            int w = source.Width, h = source.Height;
            var list = palette.ToList();
            var separate = new HashSet<(int, int)>();
            for (int guard = 0; guard < palette.Length; guard++)
            {
                var close = new List<(int I, int J)>();
                for (int i = 0; i < list.Count; i++)
                    for (int j = i + 1; j < list.Count; j++)
                        if (Dist(list[i], list[j]) < minDist2 && !separate.Contains((list[i].ToArgb(), list[j].ToArgb())))
                            close.Add((i, j));
                if (close.Count == 0) break;

                var (touch, border, area) = Contacts(buf, stride, w, h, list);
                int n = list.Count;
                int drop = -1;
                foreach (var (i, j) in close)
                {
                    double share = touch[i * n + j] / (double)Math.Max(1, Math.Min(border[i], border[j]));
                    if (share >= TouchShare)
                    {
                        drop = area[i] < area[j] ? i : j;
                        break;
                    }
                    separate.Add((list[i].ToArgb(), list[j].ToArgb()));
                }
                if (drop < 0) break;

                var dropped = list[drop];
                list.RemoveAt(drop);
                var add = MostMissingColor(source, list.ToArray(), minDistance);
                if (add is null)
                {
                    list.Insert(drop, dropped);
                    break;
                }
                list.Add(add.Value);
            }
            return list.ToArray();
        }

        private const double TouchShare = 0.2;

        private static (int[] Touch, int[] Border, int[] Area) Contacts(byte[] buf, int stride, int w, int h, List<Color> palette)
        {
            int n = palette.Count;
            var arr = palette.ToArray();
            var idx = new int[w * h];
            var area = new int[n];
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                {
                    int i = y * stride + x * 4;
                    if (buf[i + 3] == 0) { idx[y * w + x] = -1; continue; }
                    int k = FindNearestIndex(buf[i + 2], buf[i + 1], buf[i], arr);
                    idx[y * w + x] = k;
                    area[k]++;
                }

            var touch = new int[n * n];
            var border = new int[n];
            void Pair(int a, int b)
            {
                if (a == b) return;
                if (a >= 0) border[a]++;
                if (b >= 0) border[b]++;
                if (a < 0 || b < 0) return;
                touch[a * n + b]++;
                touch[b * n + a]++;
            }
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                {
                    int a = idx[y * w + x];
                    if (x + 1 < w) Pair(a, idx[y * w + x + 1]);
                    if (y + 1 < h) Pair(a, idx[(y + 1) * w + x]);
                }
            return (touch, border, area);
        }

        private static List<ColorBin>? ExactColors(Bitmap source)
        {
            byte[] buf = ImageProcessor.LockCopy(source, out int stride);
            var counts = new Dictionary<int, int>();
            for (int y = 0; y < source.Height; y++)
                for (int x = 0; x < source.Width; x++)
                {
                    int i = y * stride + x * 4;
                    if (buf[i + 3] == 0) continue;
                    int key = buf[i + 2] << 16 | buf[i + 1] << 8 | buf[i];
                    counts[key] = counts.GetValueOrDefault(key) + 1;
                    if (counts.Count > 4096) return null;
                }
            return counts
                .OrderByDescending(p => p.Value)
                .Select(p => new ColorBin(Color.FromArgb(p.Key >> 16 & 255, p.Key >> 8 & 255, p.Key & 255), p.Value))
                .ToList();
        }

        public static Color[] BestSubset(Bitmap source, Color[] palette, int count)
        {
            if (count >= palette.Length) return (Color[])palette.Clone();
            count = Math.Max(1, count);
            var bins = ColorBins(source);
            if (bins.Count == 0) return palette.Take(count).ToArray();
            if (bins.Count > 4096)
                bins = bins.OrderByDescending(b => b.Count).Take(4096).ToList();

            var dist = new int[bins.Count, palette.Length];
            for (int i = 0; i < bins.Count; i++)
                for (int p = 0; p < palette.Length; p++)
                    dist[i, p] = Dist(bins[i].Color, palette[p]);

            var cur = new int[bins.Count];
            Array.Fill(cur, int.MaxValue);
            var chosen = new bool[palette.Length];
            for (int k = 0; k < count; k++)
            {
                int bestIdx = -1;
                long bestTotal = long.MaxValue;
                for (int p = 0; p < palette.Length; p++)
                {
                    if (chosen[p]) continue;
                    long total = 0;
                    for (int i = 0; i < bins.Count; i++)
                        total += (long)bins[i].Count * Math.Min(cur[i], dist[i, p]);
                    if (total < bestTotal) { bestTotal = total; bestIdx = p; }
                }
                chosen[bestIdx] = true;
                for (int i = 0; i < bins.Count; i++)
                    cur[i] = Math.Min(cur[i], dist[i, bestIdx]);
            }

            long Total()
            {
                var picked = Enumerable.Range(0, palette.Length).Where(p => chosen[p]).ToArray();
                long total = 0;
                for (int i = 0; i < bins.Count; i++)
                {
                    int best = int.MaxValue;
                    foreach (int p in picked)
                        best = Math.Min(best, dist[i, p]);
                    total += (long)bins[i].Count * best;
                }
                return total;
            }

            long current = Total();
            for (int pass = 0; pass < 8; pass++)
            {
                bool improved = false;
                for (int a = 0; a < palette.Length; a++)
                {
                    if (!chosen[a]) continue;
                    for (int b = 0; b < palette.Length; b++)
                    {
                        if (chosen[b]) continue;
                        chosen[a] = false;
                        chosen[b] = true;
                        long total = Total();
                        if (total < current)
                        {
                            current = total;
                            improved = true;
                            break;
                        }
                        chosen[b] = false;
                        chosen[a] = true;
                    }
                }
                if (!improved) break;
            }
            return palette.Where((c, i) => chosen[i]).ToArray();
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

        internal static Color FindNearest(byte r, byte g, byte b, Color[] palette) =>
            palette[FindNearestIndex(r, g, b, palette)];

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
