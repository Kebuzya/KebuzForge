using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace KebuzForge.App.UI
{
    public enum EditorTool { Pencil, Line, Rectangle, Ellipse, FloodFill, Eyedropper, Eraser }

    public sealed class PixelEditorPanel : Control
    {

        public event EventHandler?        BeforeChange;
        public event EventHandler<Bitmap>? ImageChanged;
        public event EventHandler<Color>?  ColorPicked;
        public event EventHandler<Color>?  SecondaryColorPicked;

        private Bitmap? _image;
        public Bitmap? EditImage
        {
            get => _image;
            set
            {
                _image = value;
                UpdatePreferredSize();
                Invalidate();
            }
        }

        private int _zoom = 4;
        public int Zoom
        {
            get => _zoom;
            set
            {
                _zoom = Math.Clamp(value, 1, 32);
                UpdatePreferredSize();
                Invalidate();
            }
        }

        public EditorTool Tool       { get; set; } = EditorTool.Pencil;
        public Color      ForeColor2 { get; set; } = Color.Black;
        public Color      SecondaryColor { get; set; } = Color.White;
        public bool       EraserTransparent { get; set; } = true;
        public bool       RightButtonEraser { get; set; }

        private bool    _drawing;
        private bool    _strokeRight;
        private Point   _startPixel;
        private Point   _lastPixel;
        private Bitmap? _previewOverlay;
        private bool[,]? _previewMask;

        private bool  _panning;
        private Point _panLastScreen;

        public PixelEditorPanel()
        {
            SetStyle(
                ControlStyles.AllPaintingInWmPaint  |
                ControlStyles.UserPaint             |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw          |
                ControlStyles.Selectable,
                true);
            TabStop   = true;
            BackColor = Color.FromArgb(40, 40, 40);
        }

        private void UpdatePreferredSize()
        {
            if (_image is null) return;
            int w = _image.Width  * _zoom;
            int h = _image.Height * _zoom;
            MinimumSize = new Size(w, h);
            Size        = new Size(Math.Max(w, Parent?.ClientSize.Width  ?? w),
                                   Math.Max(h, Parent?.ClientSize.Height ?? h));
        }

        protected override void OnParentChanged(EventArgs e)
        {
            base.OnParentChanged(e);
            if (Parent is not null) UpdatePreferredSize();
        }

        public void FitZoomToParent()
        {
            if (_image is null || Parent is null) return;
            int zw = Parent.ClientSize.Width  / Math.Max(1, _image.Width);
            int zh = Parent.ClientSize.Height / Math.Max(1, _image.Height);
            Zoom = Math.Max(1, Math.Min(32, Math.Min(zw, zh)));
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.InterpolationMode  = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
            g.PixelOffsetMode    = System.Drawing.Drawing2D.PixelOffsetMode.Half;
            g.SmoothingMode      = System.Drawing.Drawing2D.SmoothingMode.None;

            int offX = ImageOffsetX, offY = ImageOffsetY;

            if (_image is null)
            {
                g.DrawString(Core.Lang.T("Нет изображения"), Font, Brushes.Gray, 8, 8);
                return;
            }

            int iw = _image.Width * _zoom, ih = _image.Height * _zoom;
            DrawCheckerboard(g, offX, offY, iw, ih);

            g.DrawImage(_image, offX, offY, iw, ih);

            if (_previewOverlay is not null)
                g.DrawImage(_previewOverlay, offX, offY, iw, ih);

            if (_zoom >= 4)
                DrawGrid(g, offX, offY, iw, ih);

            if (_drawing && _previewMask is not null)
                DrawPreviewOutline(g, offX, offY);
        }

        private void DrawPreviewOutline(Graphics g, int ox, int oy)
        {
            if (_previewMask is null || _image is null) return;
            int w = _image.Width, h = _image.Height, z = _zoom;
            using var outer = new SolidBrush(Color.Black);
            using var inner = new SolidBrush(Color.White);
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                {
                    if (!_previewMask[x, y]) continue;
                    int rx = ox + x * z, ry = oy + y * z;
                    if (y == 0 || !_previewMask[x, y - 1])
                    {
                        g.FillRectangle(outer, rx, ry - 1, z, 1);
                        g.FillRectangle(inner, rx, ry, z, 1);
                    }
                    if (y == h - 1 || !_previewMask[x, y + 1])
                    {
                        g.FillRectangle(inner, rx, ry + z - 1, z, 1);
                        g.FillRectangle(outer, rx, ry + z, z, 1);
                    }
                    if (x == 0 || !_previewMask[x - 1, y])
                    {
                        g.FillRectangle(outer, rx - 1, ry, 1, z);
                        g.FillRectangle(inner, rx, ry, 1, z);
                    }
                    if (x == w - 1 || !_previewMask[x + 1, y])
                    {
                        g.FillRectangle(inner, rx + z - 1, ry, 1, z);
                        g.FillRectangle(outer, rx + z, ry, 1, z);
                    }
                }
        }

        private void DrawCheckerboard(Graphics g, int ox, int oy, int iw, int ih)
        {
            int cell = Math.Max(4, _zoom / 2);
            using var b1 = new SolidBrush(Color.FromArgb(180, 180, 180));
            using var b2 = new SolidBrush(Color.FromArgb(220, 220, 220));
            for (int y = oy; y < oy + ih; y += cell)
                for (int x = ox; x < ox + iw; x += cell)
                {
                    int col = (x / cell + y / cell) % 2;
                    int w = Math.Min(cell, ox + iw - x);
                    int h = Math.Min(cell, oy + ih - y);
                    g.FillRectangle(col == 0 ? b1 : b2, x, y, w, h);
                }
        }

        private void DrawGrid(Graphics g, int ox, int oy, int iw, int ih)
        {
            using var pen = new Pen(Color.FromArgb(60, 0, 0, 0));
            for (int x = ox; x <= ox + iw; x += _zoom)
                g.DrawLine(pen, x, oy, x, oy + ih);
            for (int y = oy; y <= oy + ih; y += _zoom)
                g.DrawLine(pen, ox, y, ox + iw, y);
        }

        private int ImageOffsetX => Math.Max(0, (Width  - (_image?.Width  ?? 0) * _zoom) / 2);
        private int ImageOffsetY => Math.Max(0, (Height - (_image?.Height ?? 0) * _zoom) / 2);

        private Point ScreenToPixel(Point screen)
        {
            if (_image is null) return Point.Empty;
            var raw = ScreenToPixelRaw(screen);
            return new Point(
                Math.Clamp(raw.X, 0, _image.Width  - 1),
                Math.Clamp(raw.Y, 0, _image.Height - 1));
        }

        private Point ScreenToPixelRaw(Point screen)
        {
            int px = (int)Math.Floor((screen.X - ImageOffsetX) / (double)_zoom);
            int py = (int)Math.Floor((screen.Y - ImageOffsetY) / (double)_zoom);
            return new Point(px, py);
        }

        private bool InBounds(Point p) =>
            _image is not null &&
            p.X >= 0 && p.X < _image.Width &&
            p.Y >= 0 && p.Y < _image.Height;

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            if (!ModifierKeys.HasFlag(Keys.Control) || _image is null) return;
            Zoom = e.Delta > 0 ? Math.Min(_zoom + 1, 32) : Math.Max(_zoom - 1, 1);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            Focus();
            if (_image is null) return;

            if (e.Button == MouseButtons.Middle)
            {
                _panning = true;
                _panLastScreen = Control.MousePosition;
                Cursor = Cursors.SizeAll;
                return;
            }

            if (!InBounds(ScreenToPixelRaw(e.Location))) return;

            _strokeRight = e.Button == MouseButtons.Right;
            _startPixel = ScreenToPixel(e.Location);
            _lastPixel  = _startPixel;

            switch (Tool)
            {
                case EditorTool.Eyedropper:
                    PickColor(_startPixel, _strokeRight);
                    return;

                case EditorTool.FloodFill:
                    BeforeChange?.Invoke(this, EventArgs.Empty);
                    FloodFill(_image, _startPixel, GetDrawColor());
                    ImageChanged?.Invoke(this, _image);
                    Invalidate();
                    return;

                case EditorTool.Pencil:
                case EditorTool.Eraser:
                    BeforeChange?.Invoke(this, EventArgs.Empty);
                    _drawing = true;
                    DrawPixelAt(_image, _startPixel);
                    Invalidate();
                    return;

                case EditorTool.Line:
                case EditorTool.Rectangle:
                case EditorTool.Ellipse:
                    BeforeChange?.Invoke(this, EventArgs.Empty);
                    _drawing = true;
                    EnsureOverlay();
                    Invalidate();
                    return;
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (_panning)
            {
                if (Parent is ScrollableControl sc)
                {
                    Point now = Control.MousePosition;
                    int dx = _panLastScreen.X - now.X;
                    int dy = _panLastScreen.Y - now.Y;
                    _panLastScreen = now;
                    sc.AutoScrollPosition = new Point(
                        Math.Max(0, -sc.AutoScrollPosition.X + dx),
                        Math.Max(0, -sc.AutoScrollPosition.Y + dy));
                }
                return;
            }
            if (!_drawing || _image is null) return;

            var cur = ScreenToPixel(e.Location);

            switch (Tool)
            {
                case EditorTool.Pencil:
                case EditorTool.Eraser:
                    Bresenham(_image, _lastPixel, cur);
                    _lastPixel = cur;
                    Invalidate();
                    break;

                case EditorTool.Line:
                    RefreshOverlay();
                    DrawShapeOnOverlay(EditorTool.Line, _startPixel, cur);
                    Invalidate();
                    break;

                case EditorTool.Rectangle:
                case EditorTool.Ellipse:
                {
                    RefreshOverlay();
                    var (pa, pb) = ShapeCorners(cur);
                    DrawShapeOnOverlay(Tool, pa, pb);
                    Invalidate();
                    break;
                }
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Middle)
            {
                _panning = false;
                Cursor = Cursors.Default;
                return;
            }
            base.OnMouseUp(e);
            if (!_drawing || _image is null) return;
            _drawing = false;

            var cur = ScreenToPixel(e.Location);

            switch (Tool)
            {
                case EditorTool.Line:
                    CommitOverlay();
                    Bresenham(_image, _startPixel, cur);
                    break;

                case EditorTool.Rectangle:
                {
                    CommitOverlay();
                    var (pa, pb) = ShapeCorners(cur);
                    DrawRectOnImage(_image, pa, pb);
                    break;
                }

                case EditorTool.Ellipse:
                {
                    CommitOverlay();
                    var (pa, pb) = ShapeCorners(cur);
                    DrawEllipseOnImage(_image, pa, pb);
                    break;
                }
            }

            ImageChanged?.Invoke(this, _image);
            Invalidate();
        }

        private void DrawPixelAt(Bitmap bmp, Point p)
        {
            if (!InBounds(p)) return;
            bmp.SetPixel(p.X, p.Y, GetDrawColor());
        }

        private Color GetDrawColor()
        {
            if (Tool == EditorTool.Eraser) return EraserColor;
            if (_strokeRight) return RightButtonEraser ? EraserColor : SecondaryColor;
            return ForeColor2;
        }

        private Color EraserColor =>
            EraserTransparent ? Color.Transparent : BackColor;

        private (Point a, Point b) ShapeCorners(Point cur)
        {
            int dx = cur.X - _startPixel.X;
            int dy = cur.Y - _startPixel.Y;
            if (ModifierKeys.HasFlag(Keys.Shift))
            {
                int side = Math.Max(Math.Abs(dx), Math.Abs(dy));
                dx = Math.Sign(dx == 0 ? 1 : dx) * side;
                dy = Math.Sign(dy == 0 ? 1 : dy) * side;
            }
            var corner = new Point(_startPixel.X + dx, _startPixel.Y + dy);
            if (ModifierKeys.HasFlag(Keys.Control))
                return (new Point(_startPixel.X - dx, _startPixel.Y - dy), corner);
            return (_startPixel, corner);
        }

        private void Bresenham(Bitmap bmp, Point a, Point b)
        {
            Color c = GetDrawColor();
            BresenhamCore(a, b, (x, y) => PlotPixel(bmp, x, y, c));
        }

        private static void BresenhamCore(Point a, Point b, Action<int, int> plot)
        {
            int dx = Math.Abs(b.X - a.X), dy = Math.Abs(b.Y - a.Y);
            int sx = a.X < b.X ? 1 : -1, sy = a.Y < b.Y ? 1 : -1;
            int err = dx - dy, x = a.X, y = a.Y;

            while (true)
            {
                plot(x, y);
                if (x == b.X && y == b.Y) break;
                int e2 = 2 * err;
                if (e2 > -dy) { err -= dy; x += sx; }
                if (e2 <  dx) { err += dx; y += sy; }
            }
        }

        private void DrawRectOnImage(Bitmap bmp, Point a, Point b)
        {
            Color c = GetDrawColor();
            RectCore(a, b, (x, y) => PlotPixel(bmp, x, y, c));
        }

        private static void RectCore(Point a, Point b, Action<int, int> plot)
        {
            int x1 = Math.Min(a.X, b.X), y1 = Math.Min(a.Y, b.Y);
            int x2 = Math.Max(a.X, b.X), y2 = Math.Max(a.Y, b.Y);
            for (int x = x1; x <= x2; x++)
            {
                plot(x, y1);
                plot(x, y2);
            }
            for (int y = y1; y <= y2; y++)
            {
                plot(x1, y);
                plot(x2, y);
            }
        }

        private void DrawEllipseOnImage(Bitmap bmp, Point a, Point b)
        {
            Color c = GetDrawColor();
            EllipseCore(a, b, (x, y) => PlotPixel(bmp, x, y, c));
        }

        private static void EllipseCore(Point a, Point b, Action<int, int> plot)
        {
            int x0 = Math.Min(a.X, b.X), y0 = Math.Min(a.Y, b.Y);
            int x1 = Math.Max(a.X, b.X), y1 = Math.Max(a.Y, b.Y);

            long w = x1 - x0, h = y1 - y0, h1 = h & 1;
            long dx = 4 * (1 - w) * h * h;
            long dy = 4 * (h1 + 1) * w * w;
            long err = dx + dy + h1 * w * w;

            y0 += (int)((h + 1) / 2);
            y1 = y0 - (int)h1;
            long wa = 8 * w * w, ha = 8 * h * h;

            do
            {
                plot(x1, y0);
                plot(x0, y0);
                plot(x0, y1);
                plot(x1, y1);
                long e2 = 2 * err;
                if (e2 <= dy) { y0++; y1--; err += dy += wa; }
                if (e2 >= dx || 2 * err > dy) { x0++; x1--; err += dx += ha; }
            } while (x0 <= x1);

            while (y0 - y1 < h)
            {
                plot(x0 - 1, y0);
                plot(x1 + 1, y0);
                y0++;
                plot(x0 - 1, y1);
                plot(x1 + 1, y1);
                y1--;
            }
        }

        private void PlotPixel(Bitmap bmp, int x, int y, Color c)
        {
            if (InBounds(new Point(x, y))) bmp.SetPixel(x, y, c);
        }

        private void DrawShapeOnOverlay(EditorTool tool, Point a, Point b)
        {
            if (_previewOverlay is null || _previewMask is null) return;
            var ovl = _previewOverlay;
            var mask = _previewMask;
            Color c = GetDrawColor();
            void Plot(int x, int y)
            {
                if (!InBounds(new Point(x, y))) return;
                ovl.SetPixel(x, y, c);
                mask[x, y] = true;
            }

            switch (tool)
            {
                case EditorTool.Line:      BresenhamCore(a, b, Plot); break;
                case EditorTool.Rectangle: RectCore(a, b, Plot);      break;
                case EditorTool.Ellipse:   EllipseCore(a, b, Plot);   break;
            }
        }

        private void EnsureOverlay()
        {
            if (_image is null) return;
            _previewOverlay?.Dispose();
            _previewOverlay = new Bitmap(_image.Width, _image.Height, PixelFormat.Format32bppArgb);
            _previewMask = new bool[_image.Width, _image.Height];
        }

        private void RefreshOverlay()
        {
            if (_previewOverlay is null) EnsureOverlay();
            else
            {

                using var g = Graphics.FromImage(_previewOverlay!);
                g.Clear(Color.Transparent);
                if (_previewMask is not null)
                    Array.Clear(_previewMask, 0, _previewMask.Length);
            }
        }

        private void CommitOverlay()
        {
            _previewOverlay?.Dispose();
            _previewOverlay = null;
            _previewMask = null;
        }

        private static void FloodFill(Bitmap bmp, Point start, Color fillColor)
        {
            Color target = bmp.GetPixel(start.X, start.Y);
            if (SameColor(target, fillColor)) return;

            var queue = new Queue<Point>();
            queue.Enqueue(start);

            while (queue.Count > 0)
            {
                var p = queue.Dequeue();
                if (p.X < 0 || p.X >= bmp.Width || p.Y < 0 || p.Y >= bmp.Height) continue;
                if (!SameColor(bmp.GetPixel(p.X, p.Y), target)) continue;

                bmp.SetPixel(p.X, p.Y, fillColor);
                queue.Enqueue(new Point(p.X + 1, p.Y));
                queue.Enqueue(new Point(p.X - 1, p.Y));
                queue.Enqueue(new Point(p.X, p.Y + 1));
                queue.Enqueue(new Point(p.X, p.Y - 1));
            }
        }

        private static bool SameColor(Color a, Color b) =>
            a.A == b.A && a.R == b.R && a.G == b.G && a.B == b.B;

        private void PickColor(Point p, bool secondary)
        {
            if (!InBounds(p) || _image is null) return;
            var c = _image.GetPixel(p.X, p.Y);
            if (secondary)
                SecondaryColorPicked?.Invoke(this, c);
            else
                ColorPicked?.Invoke(this, c);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _previewOverlay?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
