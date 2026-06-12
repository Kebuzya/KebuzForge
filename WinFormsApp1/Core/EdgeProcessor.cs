using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace WinFormsApp1.Core
{
    internal static class EdgeProcessor
    {
        public static Bitmap Apply(Bitmap src, int thickness, Color color, bool outer)
        {
            int w = src.Width, h = src.Height;
            byte[] raw = ImageProcessor.LockCopy(src, out int stride);

            var solid = new bool[w * h];
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                    solid[y * w + x] = raw[y * stride + x * 4 + 3] > 0;

            var outline = new bool[w * h];
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                {
                    int pi = y * w + x;
                    if (outer)
                    {
                        if (!solid[pi] && NearSolid(solid, w, h, x, y, thickness))
                            outline[pi] = true;
                    }
                    else
                    {
                        if (solid[pi] && NearEmpty(solid, w, h, x, y, thickness))
                            outline[pi] = true;
                    }
                }

            var result  = new Bitmap(w, h, PixelFormat.Format32bppArgb);
            var dstData = result.LockBits(new Rectangle(0, 0, w, h),
                              ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            var dst = new byte[h * dstData.Stride];
            int ds  = dstData.Stride;

            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                {
                    int si = y * stride + x * 4;
                    int di = y * ds    + x * 4;
                    if (outline[y * w + x])
                    {
                        dst[di]     = color.B;
                        dst[di + 1] = color.G;
                        dst[di + 2] = color.R;
                        dst[di + 3] = 255;
                    }
                    else
                    {
                        dst[di]     = raw[si];
                        dst[di + 1] = raw[si + 1];
                        dst[di + 2] = raw[si + 2];
                        dst[di + 3] = raw[si + 3];
                    }
                }

            Marshal.Copy(dst, 0, dstData.Scan0, dst.Length);
            result.UnlockBits(dstData);
            return result;
        }

        private static bool NearSolid(bool[] solid, int w, int h, int x, int y, int t)
        {
            for (int dy = -t; dy <= t; dy++)
                for (int dx = -t; dx <= t; dx++)
                {
                    if (dx == 0 && dy == 0) continue;
                    int nx = x + dx, ny = y + dy;
                    if (nx < 0 || nx >= w || ny < 0 || ny >= h) continue;
                    if (solid[ny * w + nx]) return true;
                }
            return false;
        }

        private static bool NearEmpty(bool[] solid, int w, int h, int x, int y, int t)
        {
            for (int dy = -t; dy <= t; dy++)
                for (int dx = -t; dx <= t; dx++)
                {
                    if (dx == 0 && dy == 0) continue;
                    int nx = x + dx, ny = y + dy;
                    if (nx < 0 || nx >= w || ny < 0 || ny >= h) return true;
                    if (!solid[ny * w + nx]) return true;
                }
            return false;
        }
    }
}
