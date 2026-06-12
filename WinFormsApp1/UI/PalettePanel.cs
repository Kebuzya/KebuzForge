using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace WinFormsApp1.UI
{
    internal class PalettePanel : UserControl
    {

        private const int SwatchSize = 30;
        private const int Cols       = 8;
        private const int Pad        = 4;

        private Color[] _palette = [];
        private int _hovered = -1;
        private int _highlighted = -1;
        private Color? _pendingColor;
        private readonly ToolTip _tip = new();

        public event EventHandler<PaletteChangedEventArgs>? PaletteChanged;

        public void SetHighlight(int index)
        {
            _highlighted = index;
            Invalidate();
            ScrollToSwatch(index);
        }

        public void ClearHighlight()
        {
            _highlighted = -1;
            Invalidate();
        }

        private void ScrollToSwatch(int index)
        {
            if (index < 0) return;
            var rect = SwatchRect(index);
            var scrollable = Parent as ScrollableControl;
            if (scrollable is null) return;
            int visibleTop    = -scrollable.AutoScrollPosition.Y;
            int visibleBottom = visibleTop + scrollable.ClientSize.Height;
            if (rect.Bottom > visibleBottom)
                scrollable.AutoScrollPosition = new Point(0, rect.Bottom - scrollable.ClientSize.Height + Pad);
            else if (rect.Top < visibleTop)
                scrollable.AutoScrollPosition = new Point(0, rect.Top - Pad);
        }

        public void SetPendingColor(Color? color)
        {
            _pendingColor = color;
            Cursor = color.HasValue ? Cursors.Cross : Cursors.Hand;
            Invalidate();
        }

        public Color[] Palette
        {
            get => _palette;
            set
            {
                _palette = value ?? [];
                UpdatePreferredSize();
                Invalidate();
            }
        }

        public PalettePanel()
        {
            DoubleBuffered = true;
            BackColor      = Color.FromArgb(40, 40, 40);
            Cursor         = Cursors.Hand;
            SetStyle(ControlStyles.ResizeRedraw, true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.None;

            if (_palette.Length == 0)
            {
                using var f = new Font("Segoe UI", 9);
                g.DrawString(Core.Lang.T("Палитра пуста"), f, Brushes.Gray, Pad, Pad);
                return;
            }

            for (int i = 0; i < _palette.Length; i++)
            {
                var rect = SwatchRect(i);
                if (!e.ClipRectangle.IntersectsWith(rect)) continue;

                using var fill = new SolidBrush(_palette[i]);
                g.FillRectangle(fill, rect);

                bool hover = i == _hovered;
                bool hl    = i == _highlighted;
                Pen pen = hl    ? new Pen(Color.Yellow, 2) :
                          hover ? Pens.White : Pens.Black;
                g.DrawRectangle(pen, rect);
                if (hl) pen.Dispose();

                var textColor = GetContrast(_palette[i]);
                using var f   = new Font("Segoe UI", 6.5f);
                g.DrawString(i.ToString(), f, new SolidBrush(textColor),
                    rect.X + 2, rect.Y + 2);
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            int idx = IndexAt(e.X, e.Y);
            if (idx != _hovered)
            {
                _hovered = idx;
                Invalidate();
                if (idx >= 0 && idx < _palette.Length)
                    _tip.SetToolTip(this, $"#{_palette[idx].R:X2}{_palette[idx].G:X2}{_palette[idx].B:X2}  [{idx}]");
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _hovered = -1;
            Invalidate();
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            int idx = IndexAt(e.X, e.Y);
            if (idx < 0 || idx >= _palette.Length) return;

            if (e.Button == MouseButtons.Left)
            {
                if (_pendingColor.HasValue)
                {
                    _palette[idx] = _pendingColor.Value;
                    _pendingColor = null;
                    Cursor = Cursors.Hand;
                    Invalidate(SwatchRect(idx));
                    PaletteChanged?.Invoke(this, new PaletteChangedEventArgs(idx, _palette[idx]));
                }
                else
                {
                    using var dlg = new ColorDialog { Color = _palette[idx], FullOpen = true };
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        _palette[idx] = dlg.Color;
                        Invalidate(SwatchRect(idx));
                        PaletteChanged?.Invoke(this, new PaletteChangedEventArgs(idx, dlg.Color));
                    }
                }
            }
            else if (e.Button == MouseButtons.Right)
            {

                var c = _palette[idx];
                PaletteChanged?.Invoke(this,
                    new PaletteChangedEventArgs(-1, c));
            }
        }

        private Rectangle SwatchRect(int idx)
        {
            int col = idx % Cols;
            int row = idx / Cols;
            return new Rectangle(
                Pad + col * (SwatchSize + 2),
                Pad + row * (SwatchSize + 2),
                SwatchSize,
                SwatchSize);
        }

        private int IndexAt(int x, int y)
        {
            int col = (x - Pad) / (SwatchSize + 2);
            int row = (y - Pad) / (SwatchSize + 2);
            if (col < 0 || col >= Cols || row < 0) return -1;
            int idx = row * Cols + col;
            return idx < _palette.Length ? idx : -1;
        }

        private void UpdatePreferredSize()
        {
            int rows = Math.Max(1, (_palette.Length + Cols - 1) / Cols);
            Height = Pad * 2 + rows * (SwatchSize + 2);
        }

        private static Color GetContrast(Color c) =>
            (c.R * 299 + c.G * 587 + c.B * 114) / 1000 > 128
                ? Color.FromArgb(0, 0, 0)
                : Color.FromArgb(200, 200, 200);
    }

    internal class PaletteChangedEventArgs(int index, Color color) : EventArgs
    {
        public int   Index { get; } = index;
        public Color Color { get; } = color;
    }
}
