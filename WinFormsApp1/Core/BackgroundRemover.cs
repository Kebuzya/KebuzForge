using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace WinFormsApp1.Core
{
    internal static class BackgroundRemover
    {

        public static void RemoveFromCorners(Bitmap bmp, int tolerance)
        {

            var seeds = new[]
            {
                new Point(0, 0),
                new Point(bmp.Width - 1, 0),
                new Point(0, bmp.Height - 1),
                new Point(bmp.Width - 1, bmp.Height - 1)
            };

            foreach (var seed in seeds)
                FloodFill(bmp, seed, tolerance);
        }

        public static void RemoveFromPoint(Bitmap bmp, Point start, Color targetColor, int tolerance)
            => FloodFill(bmp, start, tolerance, targetColor);

        private static void FloodFill(Bitmap bmp, Point seed, int tolerance,
                                      Color? overrideTarget = null)
        {
            int w = bmp.Width, h = bmp.Height;

            var bmpData = bmp.LockBits(new Rectangle(0, 0, w, h),
                              ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            int stride = bmpData.Stride;
            var raw    = new byte[h * stride];
            Marshal.Copy(bmpData.Scan0, raw, 0, raw.Length);

            Color tc = overrideTarget ?? ColorAt(raw, stride, seed.X, seed.Y);
            if (tc.A == 0) { bmp.UnlockBits(bmpData); return; }

            var visited = new bool[w * h];
            var queue   = new Queue<Point>();
            Enqueue(queue, visited, w, h, seed);

            while (queue.Count > 0)
            {
                var p = queue.Dequeue();
                int idx = p.Y * stride + p.X * 4;

                Color c = ColorAt(raw, stride, p.X, p.Y);
                if (!Within(c, tc, tolerance)) continue;

                raw[idx] = raw[idx+1] = raw[idx+2] = raw[idx+3] = 0;

                Enqueue(queue, visited, w, h, new Point(p.X + 1, p.Y));
                Enqueue(queue, visited, w, h, new Point(p.X - 1, p.Y));
                Enqueue(queue, visited, w, h, new Point(p.X, p.Y + 1));
                Enqueue(queue, visited, w, h, new Point(p.X, p.Y - 1));
            }

            Marshal.Copy(raw, 0, bmpData.Scan0, raw.Length);
            bmp.UnlockBits(bmpData);
        }

        private static void Enqueue(Queue<Point> q, bool[] visited, int w, int h, Point p)
        {
            if (p.X < 0 || p.X >= w || p.Y < 0 || p.Y >= h) return;
            int idx = p.Y * w + p.X;
            if (visited[idx]) return;
            visited[idx] = true;
            q.Enqueue(p);
        }

        private static Color ColorAt(byte[] raw, int stride, int x, int y)
        {
            int i = y * stride + x * 4;
            return Color.FromArgb(raw[i+3], raw[i+2], raw[i+1], raw[i]);
        }

        private static bool Within(Color a, Color b, int tol)
        {
            int dr = a.R - b.R, dg = a.G - b.G, db = a.B - b.B;
            return dr*dr + dg*dg + db*db <= tol * tol * 3;
        }
    }
}
