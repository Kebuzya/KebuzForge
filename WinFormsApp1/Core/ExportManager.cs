using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace WinFormsApp1.Core
{
    internal static class ExportManager
    {

        public static void SavePng(Bitmap bmp, string path) =>
            bmp.Save(path, ImageFormat.Png);

        public static void SaveIndexedPng(Bitmap bmp, Color[] palette, string path)
        {
            if (palette.Length == 0 || palette.Length > 256)
                throw new ArgumentException("Palette must have 1–256 colors.");

            var indexed = ToIndexed8(bmp, palette);
            try { indexed.Save(path, ImageFormat.Png); }
            finally { indexed.Dispose(); }
        }

        public static readonly int[] IcoSizes = { 16, 24, 32, 48, 64, 128, 256 };

        public static void SaveIco(Bitmap src, string path)
        {

            int[]  sizes    = IcoSizes;
            byte[][] pngData = new byte[sizes.Length][];

            for (int i = 0; i < sizes.Length; i++)
            {
                int s = sizes[i];
                using var resized = Resize(src, s, s);
                using var ms = new MemoryStream();
                resized.Save(ms, ImageFormat.Png);
                pngData[i] = ms.ToArray();
            }

            using var fs = new FileStream(path, FileMode.Create);
            using var bw = new BinaryWriter(fs);

            bw.Write((short)0);
            bw.Write((short)1);
            bw.Write((short)sizes.Length);

            int headerBytes = 6 + sizes.Length * 16;
            int offset = headerBytes;

            for (int i = 0; i < sizes.Length; i++)
            {
                int s = sizes[i];
                bw.Write((byte)(s >= 256 ? 0 : s));
                bw.Write((byte)(s >= 256 ? 0 : s));
                bw.Write((byte)0);
                bw.Write((byte)0);
                bw.Write((short)1);
                bw.Write((short)32);
                bw.Write(pngData[i].Length);
                bw.Write(offset);
                offset += pngData[i].Length;
            }

            foreach (var data in pngData)
                bw.Write(data);
        }

        private static Bitmap Resize(Bitmap src, int w, int h)
        {
            var result = new Bitmap(w, h, PixelFormat.Format32bppArgb);
            using var g = Graphics.FromImage(result);
            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            g.PixelOffsetMode   = PixelOffsetMode.Half;
            g.DrawImage(src, 0, 0, w, h);
            return result;
        }

        private static Bitmap ToIndexed8(Bitmap src, Color[] palette)
        {
            var indexed = new Bitmap(src.Width, src.Height, PixelFormat.Format8bppIndexed);

            var cp = indexed.Palette;
            for (int i = 0; i < palette.Length; i++)
                cp.Entries[i] = palette[i];

            for (int i = palette.Length; i < 256; i++)
                cp.Entries[i] = Color.Black;
            indexed.Palette = cp;

            byte[] raw  = ImageProcessor.LockCopy(src, out int stride);
            var dstData = indexed.LockBits(new Rectangle(0, 0, src.Width, src.Height),
                              ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);

            int w = src.Width, h = src.Height;
            var dst = new byte[h * dstData.Stride];

            for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                int si = y * stride + x * 4;

                byte r = raw[si + 2], g2 = raw[si + 1], b = raw[si];

                int bestIdx = 0, bestDist = int.MaxValue;
                for (int pi = 0; pi < palette.Length; pi++)
                {
                    int dr = r - palette[pi].R;
                    int dg = g2 - palette[pi].G;
                    int db = b - palette[pi].B;
                    int dist = dr*dr + dg*dg + db*db;
                    if (dist < bestDist) { bestDist = dist; bestIdx = pi; }
                }
                dst[y * dstData.Stride + x] = (byte)bestIdx;
            }

            Marshal.Copy(dst, 0, dstData.Scan0, dst.Length);
            indexed.UnlockBits(dstData);
            return indexed;
        }
    }
}
