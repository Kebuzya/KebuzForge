using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;

namespace FaviconForge
{
    public static class MockupRenderer
    {
        private const int W = 440;
        private const int H = 300;

        private static Bitmap Canvas(out Graphics g, Color bg)
        {
            var bmp = new Bitmap(W, H, PixelFormat.Format32bppArgb);
            g = Graphics.FromImage(bmp);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.Half;
            g.Clear(bg);
            return bmp;
        }

        private static GraphicsPath RoundPath(RectangleF r, float rad)
        {
            var p = new GraphicsPath();
            float d = rad * 2;
            p.AddArc(r.X, r.Y, d, d, 180, 90);
            p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            p.CloseFigure();
            return p;
        }

        private static void FillRound(Graphics g, Color c, RectangleF r, float rad)
        {
            using var p = RoundPath(r, rad);
            using var b = new SolidBrush(c);
            g.FillPath(b, p);
        }

        private static void DrawIconRound(Graphics g, Bitmap icon, RectangleF r, float radPercent)
        {
            var state = g.Save();
            using var p = RoundPath(r, Math.Min(r.Width, r.Height) * radPercent / 100f);
            g.SetClip(p);
            g.DrawImage(icon, r);
            g.Restore(state);
        }

        private static void Text(Graphics g, string s, float size, FontStyle style, Color c,
            RectangleF r, StringAlignment h = StringAlignment.Near, StringAlignment v = StringAlignment.Near)
        {
            using var f = new Font("Segoe UI", size, style, GraphicsUnit.Pixel);
            using var b = new SolidBrush(c);
            using var fmt = new StringFormat
            {
                Alignment = h,
                LineAlignment = v,
                Trimming = StringTrimming.EllipsisCharacter,
                FormatFlags = StringFormatFlags.NoWrap
            };
            g.DrawString(s, f, b, r, fmt);
        }

        public static Bitmap BrowserTab(Bitmap favicon, string title, bool dark)
        {
            Color strip = dark ? Color.FromArgb(32, 33, 36) : Color.FromArgb(222, 225, 230);
            Color tab = dark ? Color.FromArgb(53, 57, 62) : Color.White;
            Color tabIdle = dark ? Color.FromArgb(40, 42, 46) : Color.FromArgb(205, 209, 215);
            Color page = dark ? Color.FromArgb(41, 42, 45) : Color.White;
            Color input = dark ? Color.FromArgb(60, 64, 67) : Color.FromArgb(241, 243, 244);
            Color text = dark ? Color.FromArgb(228, 228, 230) : Color.FromArgb(45, 48, 52);
            Color sub = dark ? Color.FromArgb(150, 152, 158) : Color.FromArgb(118, 122, 128);
            Color line = dark ? Color.FromArgb(58, 60, 65) : Color.FromArgb(232, 234, 238);

            var bmp = Canvas(out var g, strip);
            using (g)
            {
                FillRound(g, tabIdle, new RectangleF(244, 16, 150, 60), 12);
                Text(g, "Другая вкладка", 14, FontStyle.Regular, sub, new RectangleF(262, 26, 120, 22));
                FillRound(g, tab, new RectangleF(14, 12, 222, 64), 12);
                g.DrawImage(favicon, new RectangleF(28, 24, 26, 26));
                Text(g, title, 15, FontStyle.Regular, text, new RectangleF(64, 26, 160, 24));
                using (var b = new SolidBrush(tab))
                    g.FillRectangle(b, 0, 60, W, 48);
                FillRound(g, input, new RectangleF(14, 68, 412, 34), 16);
                Text(g, "https://example.com", 14, FontStyle.Regular, sub, new RectangleF(34, 76, 380, 20));
                using (var b = new SolidBrush(page))
                    g.FillRectangle(b, 0, 108, W, H - 108);
                using (var b = new SolidBrush(line))
                {
                    g.FillRectangle(b, 24, 138, 280, 14);
                    g.FillRectangle(b, 24, 166, 392, 10);
                    g.FillRectangle(b, 24, 186, 360, 10);
                    g.FillRectangle(b, 24, 206, 380, 10);
                    g.FillRectangle(b, 24, 240, 200, 10);
                }
            }
            return bmp;
        }

        public static Bitmap SearchResult(Bitmap favicon, string site, bool dark)
        {
            Color bg = dark ? Color.FromArgb(32, 33, 36) : Color.White;
            Color text = dark ? Color.FromArgb(218, 220, 224) : Color.FromArgb(32, 33, 36);
            Color sub = dark ? Color.FromArgb(154, 160, 166) : Color.FromArgb(77, 81, 86);
            Color link = dark ? Color.FromArgb(138, 180, 248) : Color.FromArgb(26, 13, 171);
            Color circle = dark ? Color.FromArgb(48, 49, 52) : Color.FromArgb(241, 243, 244);
            Color line = dark ? Color.FromArgb(60, 64, 67) : Color.FromArgb(218, 220, 224);

            var bmp = Canvas(out var g, bg);
            using (g)
            {
                using (var b = new SolidBrush(circle))
                    g.FillEllipse(b, 24, 30, 56, 56);
                g.DrawImage(favicon, new RectangleF(36, 42, 32, 32));
                Text(g, site, 17, FontStyle.Regular, text, new RectangleF(96, 36, 320, 24));
                Text(g, "https://example.com", 14, FontStyle.Regular, sub, new RectangleF(96, 62, 320, 20));
                Text(g, $"{site} — официальный сайт", 21, FontStyle.Regular, link, new RectangleF(24, 110, 400, 28));
                using (var b = new SolidBrush(line))
                {
                    g.FillRectangle(b, 24, 152, 392, 11);
                    g.FillRectangle(b, 24, 172, 356, 11);
                    g.FillRectangle(b, 24, 192, 300, 11);
                }
                Text(g, "12 мая 2026 г.", 13, FontStyle.Regular, sub, new RectangleF(24, 224, 200, 18));
            }
            return bmp;
        }

        private static readonly Color[] DummyColors =
        {
            Color.FromArgb(236, 112, 99),
            Color.FromArgb(93, 173, 226),
            Color.FromArgb(244, 208, 63),
            Color.FromArgb(88, 214, 141)
        };

        private static void Wallpaper(Graphics g, Color top, Color bottom)
        {
            using var lg = new LinearGradientBrush(new Rectangle(0, 0, W, H), top, bottom, 90f);
            g.FillRectangle(lg, 0, 0, W, H);
        }

        public static Bitmap IosHome(Bitmap icon, string title)
        {
            var bmp = Canvas(out var g, Color.Black);
            using (g)
            {
                Wallpaper(g, Color.FromArgb(86, 101, 200), Color.FromArgb(150, 90, 175));
                for (int i = 0; i < 4; i++)
                {
                    float x = 28 + i * 102;
                    FillRound(g, DummyColors[i], new RectangleF(x, 30, 76, 76), 17);
                    FillRound(g, Color.FromArgb(150, 255, 255, 255), new RectangleF(x + 14, 114, 48, 9), 4.5f);
                }
                DrawIconRound(g, icon, new RectangleF(28, 152, 76, 76), 22);
                Text(g, title, 15, FontStyle.Regular, Color.White,
                    new RectangleF(28 - 14, 234, 104, 20), StringAlignment.Center);
                FillRound(g, Color.FromArgb(70, 255, 255, 255), new RectangleF(16, 264, 408, 60), 22);
            }
            return bmp;
        }

        public static Bitmap AndroidHome(Bitmap icon, string label)
        {
            var bmp = Canvas(out var g, Color.Black);
            using (g)
            {
                Wallpaper(g, Color.FromArgb(46, 110, 175), Color.FromArgb(38, 160, 140));
                for (int i = 0; i < 4; i++)
                {
                    float x = 28 + i * 102;
                    FillRound(g, DummyColors[3 - i], new RectangleF(x, 30, 74, 74), 20);
                    FillRound(g, Color.FromArgb(150, 255, 255, 255), new RectangleF(x + 13, 112, 48, 9), 4.5f);
                }
                DrawIconRound(g, icon, new RectangleF(28, 152, 74, 74), 27);
                Text(g, label, 15, FontStyle.Regular, Color.White,
                    new RectangleF(28 - 14, 232, 102, 20), StringAlignment.Center);
                using (var b = new SolidBrush(Color.FromArgb(110, 255, 255, 255)))
                {
                    g.FillPolygon(b, new[] { new PointF(150, 282), new PointF(150, 298), new PointF(138, 290) });
                    g.FillEllipse(b, 212, 282, 16, 16);
                    g.FillRectangle(b, 286, 282, 15, 15);
                }
            }
            return bmp;
        }

        public static Bitmap AndroidSplash(Bitmap icon, string name, Color background)
        {
            var bmp = Canvas(out var g, background);
            using (g)
            {
                Color fg = background.GetBrightness() < 0.5f ? Color.White : Color.FromArgb(40, 40, 40);
                g.DrawImage(icon, new RectangleF(W / 2f - 60, 72, 120, 120));
                Text(g, name, 19, FontStyle.Regular, fg,
                    new RectangleF(20, 230, W - 40, 28), StringAlignment.Center);
            }
            return bmp;
        }

        public static Bitmap AndroidSwitcher(Bitmap icon, string name)
        {
            var bmp = Canvas(out var g, Color.FromArgb(38, 39, 43));
            using (g)
            {
                FillRound(g, Color.FromArgb(55, 57, 62), new RectangleF(-60, 50, 110, 250), 16);
                FillRound(g, Color.FromArgb(55, 57, 62), new RectangleF(390, 50, 110, 250), 16);
                g.DrawImage(icon, new RectangleF(112, 22, 26, 26));
                Text(g, name, 15, FontStyle.Regular, Color.FromArgb(228, 228, 230),
                    new RectangleF(148, 26, 200, 20));
                FillRound(g, Color.White, new RectangleF(110, 58, 220, 242), 16);
                using (var b = new SolidBrush(Color.FromArgb(228, 230, 234)))
                {
                    g.FillRectangle(b, 130, 84, 150, 12);
                    g.FillRectangle(b, 130, 108, 180, 9);
                    g.FillRectangle(b, 130, 126, 165, 9);
                    g.FillRectangle(b, 130, 144, 180, 9);
                    g.FillRectangle(b, 130, 176, 120, 9);
                }
            }
            return bmp;
        }

        public static Bitmap WindowsTile(Bitmap tileLogo, Color tileColor, string name)
        {
            var bmp = Canvas(out var g, Color.FromArgb(16, 16, 18));
            using (g)
            {
                var tile = new RectangleF(130, 52, 180, 180);
                using (var b = new SolidBrush(tileColor))
                    g.FillRectangle(b, tile);
                g.DrawImage(tileLogo, new RectangleF(145, 60, 150, 150));
                Color fg = tileColor.GetBrightness() < 0.5f ? Color.White : Color.FromArgb(40, 40, 40);
                Text(g, name, 13, FontStyle.Regular, fg,
                    new RectangleF(tile.X + 10, tile.Bottom - 26, tile.Width - 20, 18));
                using (var b = new SolidBrush(Color.FromArgb(40, 42, 46)))
                {
                    g.FillRectangle(b, 20, 52, 86, 86);
                    g.FillRectangle(b, 20, 146, 86, 86);
                    g.FillRectangle(b, 334, 52, 86, 86);
                    g.FillRectangle(b, 334, 146, 86, 86);
                }
            }
            return bmp;
        }

        public static Bitmap SafariPinnedTab(Bitmap silhouette, Color color, string title)
        {
            var bmp = Canvas(out var g, Color.FromArgb(214, 211, 215));
            using (g)
            {
                using (var b = new SolidBrush(Color.FromArgb(196, 193, 198)))
                    g.FillRectangle(b, 0, 0, W, 64);
                FillRound(g, Color.FromArgb(228, 226, 230), new RectangleF(14, 14, 36, 36), 8);
                using (var tinted = Tint(silhouette, color))
                    g.DrawImage(tinted, new RectangleF(20, 20, 24, 24));
                FillRound(g, Color.FromArgb(228, 226, 230), new RectangleF(60, 14, 366, 36), 8);
                Text(g, title, 14, FontStyle.Regular, Color.FromArgb(90, 90, 95),
                    new RectangleF(76, 22, 330, 20));
                using (var b = new SolidBrush(Color.White))
                    g.FillRectangle(b, 0, 64, W, H - 64);
                using (var b = new SolidBrush(Color.FromArgb(234, 234, 238)))
                {
                    g.FillRectangle(b, 24, 96, 300, 13);
                    g.FillRectangle(b, 24, 122, 392, 10);
                    g.FillRectangle(b, 24, 142, 360, 10);
                }
            }
            return bmp;
        }

        public static Bitmap Tint(Bitmap mono, Color color)
        {
            var result = new Bitmap(mono.Width, mono.Height, PixelFormat.Format32bppArgb);
            for (int y = 0; y < mono.Height; y++)
                for (int x = 0; x < mono.Width; x++)
                {
                    var p = mono.GetPixel(x, y);
                    if (p.A > 0)
                        result.SetPixel(x, y, Color.FromArgb(p.A, color));
                }
            return result;
        }
    }
}
