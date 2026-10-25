using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace KebuzForge.Canvas
{
    public static class CanvasRenderer
    {
        private static readonly Bitmap MeasureBitmap = new(1, 1);
        private static readonly Graphics MeasureGraphics = Graphics.FromImage(MeasureBitmap);

        public static Bitmap Render(CanvasDocument doc)
        {
            var bmp = new Bitmap(doc.Width, doc.Height, PixelFormat.Format32bppArgb);
            var copied = TryCopyBaseImage(doc, bmp);
            using var g = Graphics.FromImage(bmp);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            if (copied is null) g.Clear(doc.BackgroundColor);
            foreach (var obj in doc.Objects)
                if (obj != copied) DrawObject(g, doc, obj);
            return bmp;
        }

        public static Bitmap Render(CanvasDocument doc, float scale, RectangleF area)
        {
            int w = Math.Max(1, (int)Math.Ceiling(area.Width * scale));
            int h = Math.Max(1, (int)Math.Ceiling(area.Height * scale));
            var bmp = new Bitmap(w, h, PixelFormat.Format32bppArgb);
            using var g = Graphics.FromImage(bmp);
            g.Clear(doc.BackgroundColor);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.ScaleTransform(scale, scale);
            g.TranslateTransform(-area.X, -area.Y);
            foreach (var obj in doc.Objects)
            {
                bool basePixels = obj is ImageObject img && img.IsIdentityOn(doc);
                g.PixelOffsetMode = basePixels ? PixelOffsetMode.Half : PixelOffsetMode.HighQuality;
                g.InterpolationMode = basePixels ? InterpolationMode.NearestNeighbor : InterpolationMode.HighQualityBicubic;
                DrawObject(g, doc, obj);
            }
            return bmp;
        }

        private static ImageObject? TryCopyBaseImage(CanvasDocument doc, Bitmap target)
        {
            if (doc.BackgroundColor.A != 0 || doc.Objects.Count == 0 || doc.Objects[0] is not ImageObject img || !img.IsIdentityOn(doc))
                return null;
            var src = img.Image;
            var rect = new Rectangle(0, 0, doc.Width, doc.Height);
            var sd = src.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            try
            {
                var td = target.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
                try
                {
                    var row = new byte[rect.Width * 4];
                    for (int y = 0; y < rect.Height; y++)
                    {
                        Marshal.Copy(sd.Scan0 + y * sd.Stride, row, 0, row.Length);
                        Marshal.Copy(row, 0, td.Scan0 + y * td.Stride, row.Length);
                    }
                }
                finally
                {
                    target.UnlockBits(td);
                }
            }
            finally
            {
                src.UnlockBits(sd);
            }
            return img;
        }

        public static RectangleF GetBounds(CanvasDocument doc, CanvasObject obj)
        {
            if (obj is not IntersectionObject) return obj.Bounds;
            using var region = RegionOf(doc, obj);
            var r = region?.GetBounds(MeasureGraphics) ?? RectangleF.Empty;
            ((IntersectionObject)obj).CachedBounds = r;
            return r;
        }

        public static Region? RegionOf(CanvasDocument doc, CanvasObject obj)
        {
            if (obj is IntersectionObject ix)
            {
                var ra = SideRegion(doc, ix.FirstId, ix.FirstIsGroup);
                using var rb = SideRegion(doc, ix.SecondId, ix.SecondIsGroup);
                if (ra is null || rb is null) { ra?.Dispose(); return null; }
                ra.Intersect(rb);
                return ra;
            }

            using var path = obj.BuildPath();
            if (path.PointCount == 0) return null;
            Region? region = null;
            if (!obj.IsStrokeOnly && (obj.Fill is not null || obj is ImageObject))
                region = new Region(path);
            if (obj.Stroke is not null && obj.StrokeWidth > 0f)
            {
                using var widened = (GraphicsPath)path.Clone();
                using var pen = CreatePen(obj, Color.Black);
                try { widened.Widen(pen); }
                catch (OutOfMemoryException) { return region; }
                if (region is null) region = new Region(widened);
                else region.Union(widened);
            }
            return region;
        }

        private static Region? SideRegion(CanvasDocument doc, int id, bool isGroup)
        {
            if (!isGroup)
            {
                var single = doc.Find(id);
                return single is null || single is IntersectionObject ? null : RegionOf(doc, single);
            }
            Region? union = null;
            foreach (var member in doc.GroupMembers(id))
            {
                if (member is IntersectionObject) continue;
                using var r = RegionOf(doc, member);
                if (r is null) continue;
                if (union is null) union = r.Clone();
                else union.Union(r);
            }
            return union;
        }

        public static Pen CreatePen(CanvasObject obj, Color color)
        {
            var pen = new Pen(color, obj.StrokeWidth) { LineJoin = LineJoin.Miter };
            if (obj.HasArrow)
                pen.CustomEndCap = new AdjustableArrowCap(3f, 3f, true);
            return pen;
        }

        private static Color WithOpacity(Color c, float opacity)
        {
            float o = Math.Clamp(opacity, 0f, 1f);
            return Color.FromArgb((int)Math.Round(c.A * o), c.R, c.G, c.B);
        }

        private static void DrawObject(Graphics g, CanvasDocument doc, CanvasObject obj)
        {
            if (obj is IntersectionObject ix)
            {
                if (ix.Fill is null) return;
                using var region = RegionOf(doc, ix);
                if (region is null) return;
                ix.CachedBounds = region.GetBounds(MeasureGraphics);
                if (ix.Shadow is { } s)
                {
                    using var shadow = region.Clone();
                    shadow.Translate(s.OffsetX, s.OffsetY);
                    using var sb = new SolidBrush(WithOpacity(s.Color, ix.Opacity));
                    g.FillRegion(sb, shadow);
                }
                using var brush = new SolidBrush(WithOpacity(ix.Fill.Value, ix.Opacity));
                g.FillRegion(brush, region);
                return;
            }

            if (obj is ImageObject image)
            {
                DrawImageObject(g, image);
                return;
            }

            using var path = obj.BuildPath();
            if (path.PointCount == 0) return;

            if (obj.Shadow is { } shadowSpec)
            {
                using var shifted = (GraphicsPath)path.Clone();
                using var m = new Matrix();
                m.Translate(shadowSpec.OffsetX, shadowSpec.OffsetY);
                shifted.Transform(m);
                PaintPath(g, obj, shifted, shadowSpec.Color, shadowSpec.Color);
            }

            PaintPath(g, obj, path, obj.Fill, obj.Stroke);
        }

        private static void DrawImageObject(Graphics g, ImageObject img)
        {
            var b = img.Bounds;
            if (b.Width <= 0f || b.Height <= 0f) return;
            var corners = new[] { new PointF(b.Left, b.Top), new PointF(b.Right, b.Top), new PointF(b.Left, b.Bottom) }
                .Select(img.ToWorld).ToArray();
            var src = new RectangleF(0, 0, img.Image.Width, img.Image.Height);
            float opacity = Math.Clamp(img.Opacity, 0f, 1f);

            if (img.Shadow is { } s)
            {
                var shifted = corners.Select(p => new PointF(p.X + s.OffsetX, p.Y + s.OffsetY)).ToArray();
                var silhouette = new ColorMatrix(new[]
                {
                    new float[] { 0, 0, 0, 0, 0 },
                    new float[] { 0, 0, 0, 0, 0 },
                    new float[] { 0, 0, 0, 0, 0 },
                    new float[] { 0, 0, 0, s.Color.A / 255f * opacity, 0 },
                    new float[] { s.Color.R / 255f, s.Color.G / 255f, s.Color.B / 255f, 0, 1 }
                });
                using var shadowAttr = new ImageAttributes();
                shadowAttr.SetColorMatrix(silhouette);
                g.DrawImage(img.Image, shifted, src, GraphicsUnit.Pixel, shadowAttr);
            }

            if (opacity >= 1f)
            {
                g.DrawImage(img.Image, corners, src, GraphicsUnit.Pixel);
            }
            else
            {
                using var attr = new ImageAttributes();
                attr.SetColorMatrix(new ColorMatrix { Matrix33 = opacity });
                g.DrawImage(img.Image, corners, src, GraphicsUnit.Pixel, attr);
            }

            if (img.Stroke is not null && img.StrokeWidth > 0f)
            {
                using var path = img.BuildPath();
                PaintPath(g, img, path, null, img.Stroke);
            }
        }

        private static void PaintPath(Graphics g, CanvasObject obj, GraphicsPath path, Color? fill, Color? stroke)
        {
            if (!obj.IsStrokeOnly && obj.Fill is not null && fill is not null)
            {
                using var brush = new SolidBrush(WithOpacity(fill.Value, obj.Opacity));
                g.FillPath(brush, path);
            }
            if (obj.Stroke is not null && stroke is not null && obj.StrokeWidth > 0f)
            {
                using var pen = CreatePen(obj, WithOpacity(stroke.Value, obj.Opacity));
                g.DrawPath(pen, path);
            }
        }

        public static CanvasObject? HitTest(CanvasDocument doc, PointF p, float tolerance) =>
            HitTestAll(doc, p, tolerance).FirstOrDefault();

        public static IReadOnlyList<CanvasObject> HitTestAll(CanvasDocument doc, PointF p, float tolerance)
        {
            var exact = new List<CanvasObject>();
            var loose = new List<CanvasObject>();
            for (int i = doc.Objects.Count - 1; i >= 0; i--)
            {
                var obj = doc.Objects[i];
                if (Hits(doc, obj, p, tolerance))
                    exact.Add(obj);
                else if (obj is TextObject or ImageObject && obj.Bounds.Contains(obj.ToLocal(p)))
                    loose.Add(obj);
            }
            exact.AddRange(loose);
            return exact;
        }

        private static bool Hits(CanvasDocument doc, CanvasObject obj, PointF p, float tolerance)
        {
            if (obj is IntersectionObject)
            {
                using var region = RegionOf(doc, obj);
                return region is not null && region.IsVisible(p);
            }
            if (obj is ImageObject image) return HitsOpaquePixel(image, p);

            using var path = obj.BuildPath();
            if (path.PointCount == 0) return false;
            if (obj is TextObject)
            {
                if (path.IsVisible(p)) return true;
                using var edge = new Pen(Color.Black, Math.Max(obj.StrokeWidth, 0f) + tolerance * 2f);
                return path.IsOutlineVisible(p, edge);
            }
            if (!obj.IsStrokeOnly && obj.Fill is not null && path.IsVisible(p)) return true;
            if (obj.Stroke is not null || obj.IsStrokeOnly)
            {
                using var pen = new Pen(Color.Black, Math.Max(obj.StrokeWidth, 0f) + tolerance * 2f);
                if (path.IsOutlineVisible(p, pen)) return true;
            }
            return false;
        }

        private static bool HitsOpaquePixel(ImageObject image, PointF p)
        {
            var b = image.Bounds;
            var local = image.ToLocal(p);
            if (!b.Contains(local) || b.Width <= 0f || b.Height <= 0f) return false;
            int x = Math.Clamp((int)((local.X - b.X) / b.Width * image.Image.Width), 0, image.Image.Width - 1);
            int y = Math.Clamp((int)((local.Y - b.Y) / b.Height * image.Image.Height), 0, image.Image.Height - 1);
            return image.Image.GetPixel(x, y).A > 8;
        }
    }
}
