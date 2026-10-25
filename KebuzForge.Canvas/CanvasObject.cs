using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Globalization;
using System.Linq;
using System.Text;

namespace KebuzForge.Canvas
{
    public readonly record struct ShadowSpec(float OffsetX, float OffsetY, Color Color);

    public abstract class CanvasObject
    {
        public int Id { get; internal set; }
        public virtual RectangleF Bounds { get; set; }
        public Color? Fill { get; set; }
        public Color? Stroke { get; set; }
        public float StrokeWidth { get; set; } = 4f;
        public float Opacity { get; set; } = 1f;
        public ShadowSpec? Shadow { get; set; }

        public float Rotation { get; set; }

        public int GroupId { get; set; }

        public virtual bool IsStrokeOnly => false;
        public virtual bool CanTransform => true;
        public virtual bool CanRotate => CanTransform;
        public virtual bool HasArrow => false;

        public PointF Center => new(Bounds.X + Bounds.Width / 2f, Bounds.Y + Bounds.Height / 2f);

        public GraphicsPath BuildPath()
        {
            var path = BuildLocalPath();
            if (Rotation != 0f && CanRotate && path.PointCount > 0)
            {
                using var m = new Matrix();
                m.RotateAt(Rotation, Center);
                path.Transform(m);
            }
            return path;
        }

        protected abstract GraphicsPath BuildLocalPath();

        public PointF ToWorld(PointF local) => CanRotate ? Rotate(local, Center, Rotation) : local;

        public PointF ToLocal(PointF world) => CanRotate ? Rotate(world, Center, -Rotation) : world;

        public static PointF Rotate(PointF p, PointF center, float degrees)
        {
            if (degrees == 0f) return p;
            double a = degrees * Math.PI / 180.0;
            double cos = Math.Cos(a), sin = Math.Sin(a);
            double dx = p.X - center.X, dy = p.Y - center.Y;
            return new PointF((float)(center.X + dx * cos - dy * sin), (float)(center.Y + dx * sin + dy * cos));
        }

        public virtual CanvasObject Clone() => (CanvasObject)MemberwiseClone();

        public void Offset(float dx, float dy)
        {
            var b = Bounds;
            b.Offset(dx, dy);
            Bounds = b;
        }
    }

    public sealed class RectObject : CanvasObject
    {
        public float CornerRadius { get; set; }

        protected override GraphicsPath BuildLocalPath()
        {
            var path = new GraphicsPath();
            var b = Bounds;
            float r = Math.Min(CornerRadius, Math.Min(b.Width, b.Height) / 2f);
            if (r <= 0f || b.Width <= 0f || b.Height <= 0f)
            {
                path.AddRectangle(b);
                return path;
            }
            float d = r * 2f;
            path.AddArc(b.X, b.Y, d, d, 180, 90);
            path.AddArc(b.Right - d, b.Y, d, d, 270, 90);
            path.AddArc(b.Right - d, b.Bottom - d, d, d, 0, 90);
            path.AddArc(b.X, b.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }

    public sealed class EllipseObject : CanvasObject
    {
        protected override GraphicsPath BuildLocalPath()
        {
            var path = new GraphicsPath();
            path.AddEllipse(Bounds);
            return path;
        }
    }

    public sealed class PolygonObject : CanvasObject
    {
        public PointF[] Points { get; set; } = [new(0f, 0f), new(1f, 0.5f), new(0f, 1f)];

        protected override GraphicsPath BuildLocalPath()
        {
            var path = new GraphicsPath();
            if (Points.Length < 3) return path;
            var b = Bounds;
            var pts = new PointF[Points.Length];
            for (int i = 0; i < pts.Length; i++)
                pts[i] = new PointF(b.X + Points[i].X * b.Width, b.Y + Points[i].Y * b.Height);
            path.AddPolygon(pts);
            return path;
        }

        public override CanvasObject Clone()
        {
            var copy = (PolygonObject)base.Clone();
            copy.Points = (PointF[])Points.Clone();
            return copy;
        }
    }

    public sealed class LineObject : CanvasObject
    {
        public PointF Start { get; set; }
        public PointF End { get; set; }
        public bool ArrowEnd { get; set; }

        public override bool IsStrokeOnly => true;
        public override bool CanRotate => false;
        public override bool HasArrow => ArrowEnd;

        public override RectangleF Bounds
        {
            get => RectangleF.FromLTRB(Math.Min(Start.X, End.X), Math.Min(Start.Y, End.Y),
                                       Math.Max(Start.X, End.X), Math.Max(Start.Y, End.Y));
            set
            {
                var from = Bounds;
                Start = Map(Start, from, value);
                End = Map(End, from, value);
            }
        }

        private static PointF Map(PointF p, RectangleF from, RectangleF to)
        {
            float x = from.Width > 0f ? to.X + (p.X - from.X) * to.Width / from.Width : to.X;
            float y = from.Height > 0f ? to.Y + (p.Y - from.Y) * to.Height / from.Height : to.Y;
            return new PointF(x, y);
        }

        protected override GraphicsPath BuildLocalPath()
        {
            var path = new GraphicsPath();
            path.AddLine(Start, End);
            return path;
        }
    }

    public sealed class ArcObject : CanvasObject
    {
        public float StartAngle { get; set; } = -60f;
        public float SweepAngle { get; set; } = 300f;
        public bool ArrowEnd { get; set; }

        public override bool IsStrokeOnly => true;
        public override bool HasArrow => ArrowEnd;

        protected override GraphicsPath BuildLocalPath()
        {
            var path = new GraphicsPath();
            var b = Bounds;
            if (b.Width <= 0f || b.Height <= 0f || SweepAngle == 0f) return path;
            path.AddArc(b, StartAngle, SweepAngle);
            return path;
        }

        public PointF LocalPointAt(float degrees)
        {
            var b = Bounds;
            var c = Center;
            double rx = b.Width / 2.0, ry = b.Height / 2.0;
            double t = degrees * Math.PI / 180.0;
            double cos = Math.Cos(t), sin = Math.Sin(t);
            double denom = Math.Sqrt(ry * ry * cos * cos + rx * rx * sin * sin);
            double r = denom > 0 ? rx * ry / denom : 0;
            return new PointF((float)(c.X + r * cos), (float)(c.Y + r * sin));
        }

        public PointF StartPoint => ToWorld(LocalPointAt(StartAngle));

        public PointF EndPoint => ToWorld(LocalPointAt(StartAngle + SweepAngle));

        public float AngleTo(PointF world)
        {
            var p = ToLocal(world);
            var c = Center;
            return (float)(Math.Atan2(p.Y - c.Y, p.X - c.X) * 180.0 / Math.PI);
        }

        public void MoveStartTo(PointF world, float step = 0f)
        {
            float end = StartAngle + SweepAngle;
            float start = AngleTo(world);
            SweepAngle = Stepped(NormalizeSweep(end - start, SweepAngle >= 0f), step);
            StartAngle = step > 0f ? end - SweepAngle : start;
        }

        public void MoveEndTo(PointF world, float step = 0f) =>
            SweepAngle = Stepped(NormalizeSweep(AngleTo(world) - StartAngle, SweepAngle >= 0f), step);

        private static float Stepped(float sweep, float step)
        {
            if (step <= 0f) return sweep;
            float snapped = MathF.Round(sweep / step) * step;
            if (snapped == 0f) snapped = MathF.Sign(sweep) * step;
            return snapped;
        }

        public static float NormalizeSweep(float sweep, bool positive)
        {
            float v = ((sweep % 360f) + 360f) % 360f;
            if (v < 1f) v = 360f;
            return positive ? v : v - 360f == 0f ? -360f : v - 360f;
        }
    }

    public sealed class TextObject : CanvasObject
    {
        public string Text { get; set; } = "";
        public string FontName { get; set; } = "Arial";
        public bool Bold { get; set; } = true;

        public static FontFamily ResolveFamily(string name)
        {
            try { return new FontFamily(name); }
            catch (ArgumentException) { return new FontFamily(GenericFontFamilies.SansSerif); }
        }

        public SizeF NaturalSize(float emSize)
        {
            using var path = BuildRawPath(emSize);
            var r = path.GetBounds();
            return r.IsEmpty ? SizeF.Empty : r.Size;
        }

        private static readonly string[] FallbackFonts = ["Segoe UI Symbol", "Segoe UI Emoji"];
        private static readonly Dictionary<(string Font, int Style, string Element), bool> GlyphCache = new();
        private static readonly Bitmap MeasureBitmap = new(1, 1);
        private static readonly Graphics MeasureGraphics = Graphics.FromImage(MeasureBitmap);

        private static FontStyle StyleFor(FontFamily family, bool bold)
        {
            if (bold && family.IsStyleAvailable(FontStyle.Bold)) return FontStyle.Bold;
            return family.IsStyleAvailable(FontStyle.Regular) ? FontStyle.Regular : FontStyle.Bold;
        }

        private static (int Points, RectangleF Bounds) Outline(FontFamily family, FontStyle style, string text)
        {
            using var p = new GraphicsPath();
            p.AddString(text, family, (int)style, 100f, PointF.Empty, StringFormat.GenericTypographic);
            return (p.PointCount, p.PointCount > 0 ? p.GetBounds() : RectangleF.Empty);
        }

        public static bool HasGlyph(FontFamily family, FontStyle style, string element)
        {
            var key = (family.Name, (int)style, element);
            if (GlyphCache.TryGetValue(key, out bool known)) return known;
            var glyph = Outline(family, style, element);
            var missing = Outline(family, style, "\U000F0000");
            bool has = glyph.Points == 0 || glyph.Points != missing.Points ||
                       Math.Abs(glyph.Bounds.Width - missing.Bounds.Width) > 0.5f ||
                       Math.Abs(glyph.Bounds.Height - missing.Bounds.Height) > 0.5f;
            GlyphCache[key] = has;
            return has;
        }

        private static string StripJoiners(string element) =>
            element.Replace("\u200D", "").Replace("\uFE0E", "").Replace("\uFE0F", "");

        private GraphicsPath BuildRawPath(float emSize)
        {
            var path = new GraphicsPath(FillMode.Winding);
            if (string.IsNullOrEmpty(Text)) return path;

            var families = new List<FontFamily> { ResolveFamily(FontName) };
            foreach (var name in FallbackFonts)
                if (!string.Equals(name, families[0].Name, StringComparison.OrdinalIgnoreCase))
                    try { families.Add(new FontFamily(name)); } catch (ArgumentException) { }
            try
            {
                using var format = (StringFormat)StringFormat.GenericTypographic.Clone();
                format.FormatFlags |= StringFormatFlags.MeasureTrailingSpaces;
                float x = 0f;
                var run = new StringBuilder();
                FontFamily? runFamily = null;

                void Flush()
                {
                    if (run.Length == 0 || runFamily is null) return;
                    var style = StyleFor(runFamily, Bold);
                    string text = run.ToString();
                    path.AddString(text, runFamily, (int)style, emSize, new PointF(x, 0f), format);
                    using var font = new Font(runFamily, emSize, style, GraphicsUnit.Pixel);
                    x += MeasureGraphics.MeasureString(text, font, PointF.Empty, format).Width;
                    run.Clear();
                }

                var e = StringInfo.GetTextElementEnumerator(Text);
                while (e.MoveNext())
                {
                    string element = StripJoiners(e.GetTextElement());
                    if (element.Length == 0) continue;
                    var family = families.FirstOrDefault(f => HasGlyph(f, StyleFor(f, Bold), element)) ?? families[0];
                    if (family != runFamily)
                    {
                        Flush();
                        runFamily = family;
                    }
                    run.Append(element);
                }
                Flush();
            }
            finally
            {
                foreach (var family in families) family.Dispose();
            }
            return path;
        }

        protected override GraphicsPath BuildLocalPath()
        {
            var path = BuildRawPath(100f);
            var src = path.GetBounds();
            var dst = Bounds;
            if (src.Width <= 0f || src.Height <= 0f || dst.Width <= 0f || dst.Height <= 0f) return path;
            using var m = new Matrix();
            m.Translate(dst.X, dst.Y);
            m.Scale(dst.Width / src.Width, dst.Height / src.Height);
            m.Translate(-src.X, -src.Y);
            path.Transform(m);
            return path;
        }
    }

    public sealed class ImageObject : CanvasObject
    {
        public ImageObject(Bitmap image) => Image = image;

        public Bitmap Image { get; }

        protected override GraphicsPath BuildLocalPath()
        {
            var path = new GraphicsPath();
            if (Bounds.Width > 0f && Bounds.Height > 0f) path.AddRectangle(Bounds);
            return path;
        }

        public bool IsIdentityOn(CanvasDocument doc) =>
            Rotation == 0f && Opacity >= 1f && Shadow is null && Stroke is null &&
            Image.Width == doc.Width && Image.Height == doc.Height &&
            Bounds == new RectangleF(0f, 0f, Image.Width, Image.Height);
    }

    public sealed class IntersectionObject : CanvasObject
    {
        public int FirstId { get; set; }
        public int SecondId { get; set; }
        public bool FirstIsGroup { get; set; }
        public bool SecondIsGroup { get; set; }

        public override bool CanTransform => false;

        public override RectangleF Bounds
        {
            get => CachedBounds;
            set { }
        }

        internal RectangleF CachedBounds { get; set; }

        protected override GraphicsPath BuildLocalPath() => new();
    }
}
