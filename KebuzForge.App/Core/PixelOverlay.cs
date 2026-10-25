using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;

namespace KebuzForge.App.Core
{
    internal sealed class PixelOverlay
    {
        private readonly int[] _argb;
        private readonly bool[] _mask;

        private PixelOverlay(int width, int height, int[] argb, bool[] mask)
        {
            Width = width;
            Height = height;
            _argb = argb;
            _mask = mask;
        }

        public int Width { get; }
        public int Height { get; }
        public Size Size => new(Width, Height);
        public int Count => _mask.Count(m => m);

        public bool IsEdited(int x, int y) => _mask[y * Width + x];

        public Color this[int x, int y] => Color.FromArgb(_argb[y * Width + x]);

        public static PixelOverlay? Extract(Bitmap edited, Bitmap baseImage)
        {
            if (edited.Size != baseImage.Size) return null;
            var e = Read(edited);
            var b = Read(baseImage);
            var mask = new bool[e.Length];
            bool any = false;
            for (int i = 0; i < e.Length; i++)
            {
                if (e[i] == b[i]) continue;
                mask[i] = true;
                any = true;
            }
            return any ? new PixelOverlay(edited.Width, edited.Height, e, mask) : null;
        }

        public static PixelOverlay? FromPixels(Bitmap pixels, byte[] mask)
        {
            if (mask.Length != pixels.Width * pixels.Height) return null;
            var m = mask.Select(v => v != 0).ToArray();
            return m.Any(v => v) ? new PixelOverlay(pixels.Width, pixels.Height, Read(pixels), m) : null;
        }

        public Bitmap ApplyTo(Bitmap baseImage)
        {
            var px = Read(baseImage);
            if (baseImage.Size == Size)
                for (int i = 0; i < px.Length; i++)
                    if (_mask[i]) px[i] = _argb[i];
            return Write(baseImage.Width, baseImage.Height, px);
        }

        public Bitmap ToBitmap()
        {
            var px = new int[_argb.Length];
            for (int i = 0; i < px.Length; i++)
                if (_mask[i]) px[i] = _argb[i];
            return Write(Width, Height, px);
        }

        public byte[] MaskBytes() => _mask.Select(m => m ? (byte)1 : (byte)0).ToArray();

        public PixelOverlay Clone() => new(Width, Height, (int[])_argb.Clone(), (bool[])_mask.Clone());

        public static Bitmap CopyOf(Bitmap source) => Write(source.Width, source.Height, Read(source));

        private static int[] Read(Bitmap bmp)
        {
            var rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            var data = bmp.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            try
            {
                var px = new int[bmp.Width * bmp.Height];
                for (int y = 0; y < bmp.Height; y++)
                    Marshal.Copy(data.Scan0 + y * data.Stride, px, y * bmp.Width, bmp.Width);
                return px;
            }
            finally
            {
                bmp.UnlockBits(data);
            }
        }

        private static Bitmap Write(int width, int height, int[] px)
        {
            var bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            var data = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            try
            {
                for (int y = 0; y < height; y++)
                    Marshal.Copy(px, y * width, data.Scan0 + y * data.Stride, width);
            }
            finally
            {
                bmp.UnlockBits(data);
            }
            return bmp;
        }
    }
}
