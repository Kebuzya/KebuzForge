using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace WinFormsApp1.Core
{
    internal static class ImageProcessor
    {

        public static Bitmap Pixelize(Bitmap source, int targetW, int targetH, string algorithm)
        {
            if (source is null)   throw new ArgumentNullException(nameof(source));
            if (targetW <= 0 || targetH <= 0)
                throw new ArgumentException("Target dimensions must be positive.");

            return algorithm == "Center"
                ? PixelizeCenter(source, targetW, targetH)
                : PixelizeAverage(source, targetW, targetH);
        }

        public static Bitmap ScaleNearestNeighbor(Bitmap source, int targetW, int targetH)
        {
            var result = new Bitmap(targetW, targetH);
            using var g = Graphics.FromImage(result);
            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            g.PixelOffsetMode   = PixelOffsetMode.Half;
            g.SmoothingMode     = SmoothingMode.None;
            g.DrawImage(source, 0, 0, targetW, targetH);
            return result;
        }

        public static (int w, int h) CalcProportional(int origW, int origH, int targetW, int targetH, bool lockByWidth)
        {
            if (lockByWidth)
                return (targetW, Math.Max(1, (int)Math.Round(targetW * (double)origH / origW)));
            else
                return (Math.Max(1, (int)Math.Round(targetH * (double)origW / origH)), targetH);
        }

        public static (int w, int h) DefaultPixelSize(int origW, int origH, int maxSide = 64)
        {
            if (origW >= origH)
                return CalcProportional(origW, origH, maxSide, 0, lockByWidth: true);
            else
                return CalcProportional(origW, origH, 0, maxSide, lockByWidth: false);
        }

        private static Bitmap PixelizeAverage(Bitmap src, int tw, int th)
        {
            int sw = src.Width, sh = src.Height;
            byte[] pixels = LockCopy(src, out int stride);
            var result = new Bitmap(tw, th);

            for (int ty = 0; ty < th; ty++)
            {
                for (int tx = 0; tx < tw; tx++)
                {
                    int x1 = tx * sw / tw;
                    int x2 = Math.Max(x1 + 1, (tx + 1) * sw / tw);
                    int y1 = ty * sh / th;
                    int y2 = Math.Max(y1 + 1, (ty + 1) * sh / th);

                    long r = 0, g = 0, b = 0, a = 0;
                    int count = 0;

                    for (int sy = y1; sy < y2; sy++)
                    {
                        for (int sx = x1; sx < x2; sx++)
                        {
                            int idx = sy * stride + sx * 4;
                            b += pixels[idx];
                            g += pixels[idx + 1];
                            r += pixels[idx + 2];
                            a += pixels[idx + 3];
                            count++;
                        }
                    }

                    result.SetPixel(tx, ty, Color.FromArgb(
                        (int)(a / count), (int)(r / count),
                        (int)(g / count), (int)(b / count)));
                }
            }

            return result;
        }

        private static Bitmap PixelizeCenter(Bitmap src, int tw, int th)
        {
            int sw = src.Width, sh = src.Height;
            byte[] pixels = LockCopy(src, out int stride);
            var result = new Bitmap(tw, th);

            for (int ty = 0; ty < th; ty++)
            {
                for (int tx = 0; tx < tw; tx++)
                {
                    int sx = Math.Clamp((int)((tx + 0.5) * sw / tw), 0, sw - 1);
                    int sy = Math.Clamp((int)((ty + 0.5) * sh / th), 0, sh - 1);
                    int idx = sy * stride + sx * 4;
                    result.SetPixel(tx, ty, Color.FromArgb(
                        pixels[idx + 3], pixels[idx + 2],
                        pixels[idx + 1], pixels[idx]));
                }
            }

            return result;
        }

        internal static byte[] LockCopy(Bitmap bmp, out int stride)
        {
            var rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            var data = bmp.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            stride = data.Stride;
            var buf = new byte[bmp.Height * stride];
            Marshal.Copy(data.Scan0, buf, 0, buf.Length);
            bmp.UnlockBits(data);
            return buf;
        }
    }
}
