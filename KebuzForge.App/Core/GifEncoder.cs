using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace KebuzForge.App.Core
{
    internal static class GifEncoder
    {
        private const int MaxCode = 4096;

        public static void Write(IReadOnlyList<Bitmap> frames, IReadOnlyList<int> delaysMs, int defaultDelayMs, string path)
        {
            using var stream = File.Create(path);
            Write(frames, delaysMs, defaultDelayMs, stream);
        }

        public static void Write(IReadOnlyList<Bitmap> frames, IReadOnlyList<int> delaysMs, int defaultDelayMs, Stream stream)
        {
            if (frames.Count == 0)
                throw new ArgumentException("No frames to encode.");

            int width = 0, height = 0;
            foreach (var f in frames)
            {
                width = Math.Max(width, f.Width);
                height = Math.Max(height, f.Height);
            }

            using var w = new BinaryWriter(stream, System.Text.Encoding.ASCII, leaveOpen: true);
            w.Write("GIF89a"u8);
            w.Write((ushort)width);
            w.Write((ushort)height);
            w.Write((byte)0);
            w.Write((byte)0);
            w.Write((byte)0);

            w.Write([0x21, 0xFF, 0x0B]);
            w.Write("NETSCAPE2.0"u8);
            w.Write([0x03, 0x01, 0x00, 0x00, 0x00]);

            for (int i = 0; i < frames.Count; i++)
            {
                int delayMs = i < delaysMs.Count ? delaysMs[i] : defaultDelayMs;
                WriteFrame(w, frames[i], delayMs);
            }

            w.Write((byte)0x3B);
        }

        private static void WriteFrame(BinaryWriter w, Bitmap frame, int delayMs)
        {
            var (palette, indices, transparentIndex) = Quantize(frame);

            int tableBits = 1;
            while ((1 << tableBits) < palette.Length) tableBits++;

            w.Write([0x21, 0xF9, 0x04]);
            w.Write((byte)((2 << 2) | (transparentIndex >= 0 ? 1 : 0)));
            w.Write((ushort)Math.Clamp((delayMs + 5) / 10, 0, ushort.MaxValue));
            w.Write((byte)Math.Max(transparentIndex, 0));
            w.Write((byte)0);

            w.Write((byte)0x2C);
            w.Write((ushort)0);
            w.Write((ushort)0);
            w.Write((ushort)frame.Width);
            w.Write((ushort)frame.Height);
            w.Write((byte)(0x80 | (tableBits - 1)));

            for (int i = 0; i < 1 << tableBits; i++)
            {
                var c = i < palette.Length ? palette[i] : Color.Black;
                w.Write(c.R);
                w.Write(c.G);
                w.Write(c.B);
            }

            int minCodeSize = Math.Max(2, tableBits);
            w.Write((byte)minCodeSize);
            WriteLzw(w, indices, minCodeSize);
        }

        private static (Color[] Palette, byte[] Indices, int TransparentIndex) Quantize(Bitmap frame)
        {
            int fw = frame.Width, fh = frame.Height;
            byte[] src = ImageProcessor.LockCopy(frame, out int stride);

            bool hasTransparent = false;
            var exact = new Dictionary<int, int>();
            bool overflow = false;
            for (int y = 0; y < fh && !overflow; y++)
                for (int x = 0; x < fw; x++)
                {
                    int i = y * stride + x * 4;
                    if (src[i + 3] == 0) { hasTransparent = true; continue; }
                    int rgb = (src[i + 2] << 16) | (src[i + 1] << 8) | src[i];
                    if (!exact.ContainsKey(rgb))
                    {
                        if (exact.Count == 256) { overflow = true; break; }
                        exact[rgb] = exact.Count;
                    }
                }

            if (!overflow && hasTransparent && exact.Count == 256)
                overflow = true;

            Color[] colors;
            if (overflow)
            {
                for (int y = 0; y < fh && !hasTransparent; y++)
                    for (int x = 0; x < fw; x++)
                        if (src[y * stride + x * 4 + 3] == 0) { hasTransparent = true; break; }

                colors = PaletteManager.MedianCut(frame, hasTransparent ? 255 : 256);
                exact.Clear();
            }
            else
            {
                colors = new Color[exact.Count];
                foreach (var (rgb, idx) in exact)
                    colors[idx] = Color.FromArgb(rgb >> 16, (rgb >> 8) & 0xFF, rgb & 0xFF);
            }

            int transparentIndex = hasTransparent ? colors.Length : -1;
            var palette = new Color[colors.Length + (hasTransparent ? 1 : 0)];
            Array.Copy(colors, palette, colors.Length);
            if (hasTransparent) palette[^1] = Color.Black;
            if (palette.Length == 0) palette = [Color.Black];

            var indices = new byte[fw * fh];
            for (int y = 0; y < fh; y++)
                for (int x = 0; x < fw; x++)
                {
                    int i = y * stride + x * 4;
                    if (src[i + 3] == 0) { indices[y * fw + x] = (byte)transparentIndex; continue; }
                    int rgb = (src[i + 2] << 16) | (src[i + 1] << 8) | src[i];
                    if (!exact.TryGetValue(rgb, out int idx))
                    {
                        idx = PaletteManager.FindNearestIndex(src[i + 2], src[i + 1], src[i], colors);
                        exact[rgb] = idx;
                    }
                    indices[y * fw + x] = (byte)idx;
                }

            return (palette, indices, transparentIndex);
        }

        private static void WriteLzw(BinaryWriter w, byte[] indices, int minCodeSize)
        {
            var output = new SubBlockWriter(w);
            int clearCode = 1 << minCodeSize;
            int endCode = clearCode + 1;
            int codeSize = minCodeSize + 1;
            int next = endCode + 1;
            var table = new Dictionary<int, int>();

            output.WriteCode(clearCode, codeSize);
            if (indices.Length == 0)
            {
                output.WriteCode(endCode, codeSize);
                output.Finish();
                return;
            }

            int prefix = indices[0];
            for (int i = 1; i < indices.Length; i++)
            {
                int c = indices[i];
                int key = (prefix << 8) | c;
                if (table.TryGetValue(key, out int code))
                {
                    prefix = code;
                    continue;
                }

                output.WriteCode(prefix, codeSize);
                table[key] = next;
                if (next >= 1 << codeSize) codeSize++;
                next++;
                if (next == MaxCode)
                {
                    output.WriteCode(clearCode, codeSize);
                    table.Clear();
                    codeSize = minCodeSize + 1;
                    next = endCode + 1;
                }
                prefix = c;
            }

            output.WriteCode(prefix, codeSize);
            if (next >= 1 << codeSize && codeSize < 12) codeSize++;
            output.WriteCode(endCode, codeSize);
            output.Finish();
        }

        private sealed class SubBlockWriter(BinaryWriter w)
        {
            private readonly byte[] _block = new byte[255];
            private int _count;
            private int _bits;
            private int _bitCount;

            public void WriteCode(int code, int size)
            {
                _bits |= code << _bitCount;
                _bitCount += size;
                while (_bitCount >= 8)
                {
                    Put((byte)_bits);
                    _bits >>= 8;
                    _bitCount -= 8;
                }
            }

            public void Finish()
            {
                if (_bitCount > 0) Put((byte)_bits);
                if (_count > 0) Flush();
                w.Write((byte)0);
            }

            private void Put(byte b)
            {
                _block[_count++] = b;
                if (_count == 255) Flush();
            }

            private void Flush()
            {
                w.Write((byte)_count);
                w.Write(_block, 0, _count);
                _count = 0;
            }
        }
    }
}
