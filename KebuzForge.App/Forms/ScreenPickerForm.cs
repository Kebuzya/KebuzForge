using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace KebuzForge.App.Forms
{
    internal sealed partial class ScreenPickerForm : Form
    {
        private const int Preview = 24;
        private readonly Bitmap _shot;
        private Point _mouse = new(-1000, -1000);

        public ScreenPickerForm()
        {
            InitializeComponent();
            var bounds = SystemInformation.VirtualScreen;
            _shot = new Bitmap(bounds.Width, bounds.Height, PixelFormat.Format32bppArgb);
            using (var g = Graphics.FromImage(_shot))
                g.CopyFromScreen(bounds.Location, Point.Empty, bounds.Size);
            Bounds = bounds;
        }

        public Color? PickedColor { get; private set; }

        private Color ColorAt(Point p) =>
            p.X >= 0 && p.Y >= 0 && p.X < _shot.Width && p.Y < _shot.Height ? _shot.GetPixel(p.X, p.Y) : Color.Black;

        private Rectangle PreviewRect(Point p) => new(p.X + 16, p.Y + 16, Preview + 70, Preview + 2);

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.DrawImage(_shot, e.ClipRectangle, e.ClipRectangle, GraphicsUnit.Pixel);
            var r = PreviewRect(_mouse);
            if (!e.ClipRectangle.IntersectsWith(r)) return;
            var c = ColorAt(_mouse);
            g.FillRectangle(Brushes.Black, r);
            using (var swatch = new SolidBrush(Color.FromArgb(255, c)))
                g.FillRectangle(swatch, r.X + 1, r.Y + 1, Preview, Preview);
            using var font = new Font("Segoe UI", 9f);
            g.DrawString($"#{c.R:X2}{c.G:X2}{c.B:X2}", font, Brushes.White, r.X + Preview + 4, r.Y + 5);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            var old = PreviewRect(_mouse);
            _mouse = e.Location;
            Invalidate(Rectangle.Inflate(old, 2, 2));
            Invalidate(Rectangle.Inflate(PreviewRect(_mouse), 2, 2));
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button == MouseButtons.Left)
            {
                var c = ColorAt(e.Location);
                PickedColor = Color.FromArgb(c.R, c.G, c.B);
                DialogResult = DialogResult.OK;
            }
            else
            {
                DialogResult = DialogResult.Cancel;
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.KeyCode == Keys.Escape) DialogResult = DialogResult.Cancel;
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);
            _shot.Dispose();
        }
    }
}
