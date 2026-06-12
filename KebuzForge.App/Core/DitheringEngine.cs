using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace KebuzForge.App.Core
{
    internal static class DitheringEngine
    {

        public static Bitmap Apply(Bitmap source, Color[] palette, string mode, float intensity)
        {
            if (palette.Length == 0)
                return new Bitmap(source);

            if (intensity < 0.001f || mode == "None")
                return PaletteManager.ApplyPalette(source, palette);

            return mode switch
            {
                "FloydSteinberg" => FloydSteinberg(source, palette, intensity),
                "Bayer4"         => BayerOrdered(source, palette, intensity, Bayer4x4, 16),
                "Bayer8"         => BayerOrdered(source, palette, intensity, Bayer8x8, 64),
                _                => PaletteManager.ApplyPalette(source, palette)
            };
        }

        private static Bitmap FloydSteinberg(Bitmap src, Color[] palette, float intensity)
        {
            int w = src.Width, h = src.Height;
            byte[] raw = ImageProcessor.LockCopy(src, out int stride);

            var exact = new HashSet<int>();
            foreach (var c in palette)
                exact.Add((c.R << 16) | (c.G << 8) | c.B);

            float[] er = new float[w * h];
            float[] eg = new float[w * h];
            float[] eb = new float[w * h];
            byte[]  ea = new byte[w * h];
            bool[]  locked = new bool[w * h];

            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                {
                    int si = y * stride + x * 4;
                    int pi = y * w + x;
                    eb[pi] = raw[si];
                    eg[pi] = raw[si + 1];
                    er[pi] = raw[si + 2];
                    ea[pi] = raw[si + 3];
                    locked[pi] = ea[pi] != 0 &&
                        exact.Contains((raw[si + 2] << 16) | (raw[si + 1] << 8) | raw[si]);
                }

            var result  = new Bitmap(w, h);
            var dstRect = new Rectangle(0, 0, w, h);
            var dstData = result.LockBits(dstRect, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            var dst     = new byte[h * dstData.Stride];
            int dStride = dstData.Stride;

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    int pi = y * w + x;
                    byte a = ea[pi];

                    if (a == 0) { WritePixel(dst, dStride, x, y, 0, 0, 0, 0); continue; }

                    if (locked[pi])
                    {
                        WritePixel(dst, dStride, x, y, Clamp(eb[pi]), Clamp(eg[pi]), Clamp(er[pi]), a);
                        continue;
                    }

                    byte qr = Clamp(er[pi]);
                    byte qg = Clamp(eg[pi]);
                    byte qb = Clamp(eb[pi]);

                    Color nearest = PaletteManager.FindNearest(qr, qg, qb, palette);
                    WritePixel(dst, dStride, x, y, nearest.B, nearest.G, nearest.R, a);

                    float errR = (er[pi] - nearest.R) * intensity;
                    float errG = (eg[pi] - nearest.G) * intensity;
                    float errB = (eb[pi] - nearest.B) * intensity;

                    Spread(er, eg, eb, ea, locked, w, h, y, x + 1,     errR * (7f/16), errG * (7f/16), errB * (7f/16));
                    Spread(er, eg, eb, ea, locked, w, h, y + 1, x - 1, errR * (3f/16), errG * (3f/16), errB * (3f/16));
                    Spread(er, eg, eb, ea, locked, w, h, y + 1, x,     errR * (5f/16), errG * (5f/16), errB * (5f/16));
                    Spread(er, eg, eb, ea, locked, w, h, y + 1, x + 1, errR * (1f/16), errG * (1f/16), errB * (1f/16));
                }
            }

            Marshal.Copy(dst, 0, dstData.Scan0, dst.Length);
            result.UnlockBits(dstData);
            return result;
        }

        private static Bitmap BayerOrdered(Bitmap src, Color[] palette, float intensity, int[,] matrix, int levels)
        {
            int w = src.Width, h = src.Height;
            int matSize = matrix.GetLength(0);
            byte[] raw = ImageProcessor.LockCopy(src, out int stride);

            var exact = new HashSet<int>();
            foreach (var c in palette)
                exact.Add((c.R << 16) | (c.G << 8) | c.B);

            var result  = new Bitmap(w, h);
            var dstRect = new Rectangle(0, 0, w, h);
            var dstData = result.LockBits(dstRect, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            var dst     = new byte[h * dstData.Stride];
            int dStride = dstData.Stride;

            float spread = 255f * intensity * 0.5f;

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    int si = y * stride + x * 4;
                    byte a = raw[si + 3];

                    if (a == 0) { WritePixel(dst, dStride, x, y, 0, 0, 0, 0); continue; }

                    if (exact.Contains((raw[si + 2] << 16) | (raw[si + 1] << 8) | raw[si]))
                    {
                        WritePixel(dst, dStride, x, y, raw[si], raw[si + 1], raw[si + 2], a);
                        continue;
                    }

                    float t = ((float)matrix[y % matSize, x % matSize] / levels - 0.5f) * spread * 2f;

                    byte adjR = Clamp(raw[si + 2] + t);
                    byte adjG = Clamp(raw[si + 1] + t);
                    byte adjB = Clamp(raw[si]     + t);

                    Color nearest = PaletteManager.FindNearest(adjR, adjG, adjB, palette);
                    WritePixel(dst, dStride, x, y, nearest.B, nearest.G, nearest.R, a);
                }
            }

            Marshal.Copy(dst, 0, dstData.Scan0, dst.Length);
            result.UnlockBits(dstData);
            return result;
        }

        private static void Spread(float[] er, float[] eg, float[] eb, byte[] ea, bool[] locked,
            int w, int h, int y, int x, float vr, float vg, float vb)
        {
            if (x < 0 || x >= w || y < 0 || y >= h) return;
            int pi = y * w + x;
            if (ea[pi] == 0 || locked[pi]) return;
            er[pi] += vr;
            eg[pi] += vg;
            eb[pi] += vb;
        }

        private static void WritePixel(byte[] buf, int stride, int x, int y, byte b, byte g, byte r, byte a)
        {
            int i = y * stride + x * 4;
            buf[i]     = b;
            buf[i + 1] = g;
            buf[i + 2] = r;
            buf[i + 3] = a;
        }

        private static byte Clamp(float v) => (byte)Math.Clamp((int)v, 0, 255);

        private static readonly int[,] Bayer4x4 =
        {
            {  0,  8,  2, 10 },
            { 12,  4, 14,  6 },
            {  3, 11,  1,  9 },
            { 15,  7, 13,  5 }
        };

        private static readonly int[,] Bayer8x8 =
        {
            {  0, 32,  8, 40,  2, 34, 10, 42 },
            { 48, 16, 56, 24, 50, 18, 58, 26 },
            { 12, 44,  4, 36, 14, 46,  6, 38 },
            { 60, 28, 52, 20, 62, 30, 54, 22 },
            {  3, 35, 11, 43,  1, 33,  9, 41 },
            { 51, 19, 59, 27, 49, 17, 57, 25 },
            { 15, 47,  7, 39, 13, 45,  5, 37 },
            { 63, 31, 55, 23, 61, 29, 53, 21 }
        };
    }
}
