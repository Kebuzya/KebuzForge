using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace KebuzForge.Canvas
{
    public enum CanvasTool { Select, Rectangle, Ellipse, Polygon, Line, Arc, Text }

    public sealed class CanvasPickEventArgs : EventArgs
    {
        public CanvasPickEventArgs(IReadOnlyList<CanvasObject> objects, Point location)
        {
            Objects = objects;
            Location = location;
        }

        public IReadOnlyList<CanvasObject> Objects { get; }
        public Point Location { get; }
    }

    public sealed class CanvasEditedEventArgs : EventArgs
    {
        public CanvasEditedEventArgs(bool final) => Final = final;
        public bool Final { get; }
    }

    public sealed class CanvasView : Control
    {
        private const int HandleSize = 8;
        private const float RotateHandleOffset = 26f;
        private const float ViewMargin = 16f;
        private const float SnapDistance = 6f;

        private enum Drag { None, Create, Move, Resize, Rotate, LineStart, LineEnd, ArcStart, ArcEnd, FrameResize, FrameRotate }

        private CanvasDocument? _document;
        private Bitmap? _cache;
        private bool _dirty = true;
        private Bitmap? _zoomCache;
        private bool _zoomDirty = true;
        private (RectangleF Area, float Scale) _zoomKey;
        private readonly List<CanvasObject> _selection = new();
        private CanvasTool _tool = CanvasTool.Select;

        private Drag _drag;
        private bool _committed;
        private PointF _dragStart;
        private PointF _lastPoint;
        private int _handle = -1;
        private RectangleF _resizeFrom;
        private CanvasObject? _creating;
        private bool _duplicateOnDrag;

        private float _zoom = 1f;
        private PointF _pan;
        private bool _panning;
        private Point _panLast;
        private bool _spaceDown;

        private readonly List<float> _snapX = new();
        private readonly List<float> _snapY = new();
        private readonly List<(bool Vertical, float At)> _guides = new();
        private RectangleF _moveBox;
        private PointF _moveApplied;
        private bool _altUsed;
        private readonly List<(CanvasObject Obj, CanvasObject Original)> _frameOriginals = new();
        private Point _lastMouse;

        public CanvasView()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw |
                     ControlStyles.Selectable, true);
            TabStop = true;
        }

        public event EventHandler? EditCommitting;
        public event EventHandler<CanvasEditedEventArgs>? DocumentEdited;
        public event EventHandler? SelectionChanged;
        public event EventHandler? ToolChanged;
        public event EventHandler? ZoomChanged;
        public event EventHandler<CanvasPickEventArgs>? PickRequested;

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public CanvasDocument? Document
        {
            get => _document;
            set
            {
                if (_document == value) return;
                bool sameSize = _document is not null && value is not null &&
                                _document.Width == value.Width && _document.Height == value.Height;
                if (_document is not null) _document.Changed -= OnDocumentChanged;
                if (!sameSize)
                {
                    _zoom = 1f;
                    _pan = PointF.Empty;
                }
                var keep = _selection.Select(o => o.Id).ToHashSet();
                _document = value;
                if (_document is not null) _document.Changed += OnDocumentChanged;
                _selection.Clear();
                if (_document is not null)
                    _selection.AddRange(_document.Objects.Where(o => keep.Contains(o.Id)));
                _drag = Drag.None;
                _dirty = true;
                _zoomDirty = true;
                Invalidate();
                SelectionChanged?.Invoke(this, EventArgs.Empty);
                if (!sameSize) ZoomChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        [Browsable(false)]
        public float ZoomPercent => ViewScale * 100f;

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public CanvasTool Tool
        {
            get => _tool;
            set
            {
                if (_tool == value) return;
                _tool = value;
                ToolChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Action<CanvasObject>? ConfigureNewObject { get; set; }

        [DefaultValue(true)]
        public bool SnapEnabled { get; set; } = true;

        private bool SnapActive => SnapEnabled ^ ((ModifierKeys & Keys.Alt) != 0);

        [Browsable(false)]
        public IReadOnlyList<CanvasObject> SelectedObjects => _selection;

        public void SelectOnly(CanvasObject? obj)
        {
            _selection.Clear();
            if (obj is not null)
            {
                if (_document is not null && obj.GroupId != 0) _selection.AddRange(_document.WithGroups([obj]));
                else _selection.Add(obj);
            }
            Invalidate();
            SelectionChanged?.Invoke(this, EventArgs.Empty);
        }

        public bool CanGroupSelection => _document is not null && _document.CanGroup(_selection);

        public bool CanUngroupSelection => _document is not null && _document.CanUngroup(_selection);

        public void GroupSelection()
        {
            if (_document is null || !_document.CanGroup(_selection)) return;
            _committed = false;
            BeginEdit();
            _document.Group(_selection);
            var members = _document.WithGroups(_selection);
            _selection.Clear();
            _selection.AddRange(members);
            SelectionChanged?.Invoke(this, EventArgs.Empty);
            _committed = false;
            Edited(true);
        }

        public void UngroupSelection()
        {
            if (_document is null || !_document.CanUngroup(_selection)) return;
            _committed = false;
            BeginEdit();
            _document.Ungroup(_selection);
            _selection.RemoveAll(o => !_document.Objects.Contains(o));
            SelectionChanged?.Invoke(this, EventArgs.Empty);
            _committed = false;
            Edited(true);
        }

        private bool IsFrame => _selection.Count(o => o.CanTransform) >= 2;

        private PointF[] FrameHandles(RectangleF b)
        {
            float cx = b.X + b.Width / 2f, cy = b.Y + b.Height / 2f;
            PointF[] world =
            [
                new(b.Left, b.Top), new(cx, b.Top), new(b.Right, b.Top),
                new(b.Right, cy), new(b.Right, b.Bottom), new(cx, b.Bottom),
                new(b.Left, b.Bottom), new(b.Left, cy)
            ];
            return world.Select(ToScreen).ToArray();
        }

        private PointF FrameRotateHandle(RectangleF b)
        {
            var top = ToScreen(new PointF(b.X + b.Width / 2f, b.Top));
            return new PointF(top.X, top.Y - RotateHandleOffset);
        }

        private void CaptureFrame()
        {
            _frameOriginals.Clear();
            foreach (var o in _selection.Where(o => o.CanTransform))
                _frameOriginals.Add((o, o.Clone()));
        }

        private void ApplyFrameScale(RectangleF from, RectangleF to)
        {
            float sx = from.Width > 0f ? to.Width / from.Width : 1f;
            float sy = from.Height > 0f ? to.Height / from.Height : 1f;
            PointF Map(PointF q) => new(to.X + (q.X - from.X) * sx, to.Y + (q.Y - from.Y) * sy);
            foreach (var (obj, original) in _frameOriginals)
            {
                if (obj is LineObject line && original is LineObject was)
                {
                    line.Start = Map(was.Start);
                    line.End = Map(was.End);
                    continue;
                }
                var b = original.Bounds;
                var c = Map(original.Center);
                if (!original.CanRotate || original.Rotation == 0f)
                {
                    obj.Bounds = new RectangleF(c.X - b.Width * sx / 2f, c.Y - b.Height * sy / 2f, b.Width * sx, b.Height * sy);
                    continue;
                }
                double t = original.Rotation * Math.PI / 180.0;
                double ux = sx * Math.Cos(t), uy = sy * Math.Sin(t);
                double vx = -sx * Math.Sin(t), vy = sy * Math.Cos(t);
                float w = b.Width * (float)Math.Sqrt(ux * ux + uy * uy);
                float h = b.Height * (float)Math.Sqrt(vx * vx + vy * vy);
                obj.Bounds = new RectangleF(c.X - w / 2f, c.Y - h / 2f, w, h);
                obj.Rotation = (float)((Math.Atan2(uy, ux) * 180.0 / Math.PI % 360.0 + 360.0) % 360.0);
            }
        }

        private void ApplyFrameRotation(PointF center, float delta)
        {
            foreach (var (obj, original) in _frameOriginals)
            {
                if (obj is LineObject line && original is LineObject was)
                {
                    line.Start = CanvasObject.Rotate(was.Start, center, delta);
                    line.End = CanvasObject.Rotate(was.End, center, delta);
                    continue;
                }
                var b = original.Bounds;
                var c = CanvasObject.Rotate(original.Center, center, delta);
                obj.Bounds = new RectangleF(c.X - b.Width / 2f, c.Y - b.Height / 2f, b.Width, b.Height);
                if (obj.CanRotate)
                    obj.Rotation = (float)(((original.Rotation + delta) % 360.0 + 360.0) % 360.0);
            }
        }

        private void OnDocumentChanged(object? sender, EventArgs e)
        {
            if (_document is not null)
                _selection.RemoveAll(o => !_document.Objects.Contains(o));
            _dirty = true;
            _zoomDirty = true;
            Invalidate();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_document is not null) _document.Changed -= OnDocumentChanged;
                _cache?.Dispose();
                _zoomCache?.Dispose();
            }
            base.Dispose(disposing);
        }

        private float FitScale
        {
            get
            {
                if (_document is null) return 1f;
                float w = Math.Max(1f, ClientSize.Width - ViewMargin * 2);
                float h = Math.Max(1f, ClientSize.Height - ViewMargin * 2);
                return Math.Max(0.01f, Math.Min(w / _document.Width, h / _document.Height));
            }
        }

        private float ViewScale => FitScale * _zoom;

        private PointF CenteredOrigin(float scale) =>
            _document is null ? PointF.Empty
                : new PointF((ClientSize.Width - _document.Width * scale) / 2f, (ClientSize.Height - _document.Height * scale) / 2f);

        private PointF Origin
        {
            get
            {
                var c = CenteredOrigin(ViewScale);
                return new PointF(c.X + _pan.X, c.Y + _pan.Y);
            }
        }

        public void FitToWindow()
        {
            _zoom = 1f;
            _pan = PointF.Empty;
            Invalidate();
            ZoomChanged?.Invoke(this, EventArgs.Empty);
        }

        public void ZoomAt(PointF screen, float factor)
        {
            if (_document is null) return;
            var docPoint = ToDoc(screen);
            float fit = FitScale;
            float maxZoom = Math.Max(1f, 64f / fit);
            _zoom = Math.Clamp(_zoom * factor, 0.25f, maxZoom);
            float scale = fit * _zoom;
            var centered = CenteredOrigin(scale);
            _pan = new PointF(screen.X - docPoint.X * scale - centered.X, screen.Y - docPoint.Y * scale - centered.Y);
            ClampPan();
            Invalidate();
            ZoomChanged?.Invoke(this, EventArgs.Empty);
        }

        private void PanBy(float dx, float dy)
        {
            _pan = new PointF(_pan.X + dx, _pan.Y + dy);
            ClampPan();
            Invalidate();
        }

        private void ClampPan()
        {
            if (_document is null) return;
            const float keep = 48f;
            float s = ViewScale;
            var c = CenteredOrigin(s);
            float w = _document.Width * s, h = _document.Height * s;
            float minX = keep - w - c.X, maxX = ClientSize.Width - keep - c.X;
            float minY = keep - h - c.Y, maxY = ClientSize.Height - keep - c.Y;
            _pan = new PointF(Math.Clamp(_pan.X, Math.Min(minX, maxX), Math.Max(minX, maxX)),
                              Math.Clamp(_pan.Y, Math.Min(minY, maxY), Math.Max(minY, maxY)));
        }

        private PointF ToDoc(PointF p)
        {
            var o = Origin;
            float s = ViewScale;
            return new PointF((p.X - o.X) / s, (p.Y - o.Y) / s);
        }

        private PointF ToScreen(PointF p)
        {
            var o = Origin;
            float s = ViewScale;
            return new PointF(o.X + p.X * s, o.Y + p.Y * s);
        }

        private RectangleF ToScreen(RectangleF r)
        {
            var o = Origin;
            float s = ViewScale;
            return new RectangleF(o.X + r.X * s, o.Y + r.Y * s, r.Width * s, r.Height * s);
        }

        private PointF[] Corners(CanvasObject obj)
        {
            var b = obj.Bounds;
            return new[] { new PointF(b.Left, b.Top), new PointF(b.Right, b.Top), new PointF(b.Right, b.Bottom), new PointF(b.Left, b.Bottom) }
                .Select(p => ToScreen(obj.ToWorld(p))).ToArray();
        }

        private PointF[] BoxHandles(CanvasObject obj)
        {
            var b = obj.Bounds;
            float cx = b.X + b.Width / 2f, cy = b.Y + b.Height / 2f;
            PointF[] local =
            [
                new(b.Left, b.Top), new(cx, b.Top), new(b.Right, b.Top),
                new(b.Right, cy), new(b.Right, b.Bottom), new(cx, b.Bottom),
                new(b.Left, b.Bottom), new(b.Left, cy)
            ];
            return local.Select(p => ToScreen(obj.ToWorld(p))).ToArray();
        }

        private PointF RotateHandle(CanvasObject obj)
        {
            var b = obj.Bounds;
            var top = ToScreen(obj.ToWorld(new PointF(b.X + b.Width / 2f, b.Top)));
            double a = obj.Rotation * Math.PI / 180.0;
            return new PointF(top.X + (float)(Math.Sin(a) * RotateHandleOffset), top.Y - (float)(Math.Cos(a) * RotateHandleOffset));
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Clear(BackColor);
            if (_document is null) return;

            if (ViewScale <= 1f && (_dirty || _cache is null))
            {
                _cache?.Dispose();
                _cache = CanvasRenderer.Render(_document);
                _dirty = false;
            }

            var area = ToScreen(new RectangleF(0, 0, _document.Width, _document.Height));
            var visibleScreen = RectangleF.Intersect(area, new RectangleF(0, 0, ClientSize.Width, ClientSize.Height));
            if (visibleScreen.Width > 0 && visibleScreen.Height > 0)
            {
                DrawChecker(g, Rectangle.Round(visibleScreen));
                var tl = ToDoc(new PointF(visibleScreen.Left, visibleScreen.Top));
                var br = ToDoc(new PointF(visibleScreen.Right, visibleScreen.Bottom));
                var src = RectangleF.FromLTRB((float)Math.Floor(Math.Max(0, tl.X)), (float)Math.Floor(Math.Max(0, tl.Y)),
                                              (float)Math.Ceiling(Math.Min(_document.Width, br.X)), (float)Math.Ceiling(Math.Min(_document.Height, br.Y)));
                if (ViewScale > 1f)
                {
                    var key = (src, ViewScale);
                    if (_zoomDirty || _zoomCache is null || key != _zoomKey)
                    {
                        _zoomCache?.Dispose();
                        _zoomCache = CanvasRenderer.Render(_document, ViewScale, src);
                        _zoomKey = key;
                        _zoomDirty = false;
                    }
                    g.InterpolationMode = InterpolationMode.NearestNeighbor;
                    g.PixelOffsetMode = PixelOffsetMode.Half;
                    var dest = ToScreen(src);
                    g.DrawImage(_zoomCache, new RectangleF(dest.X, dest.Y, _zoomCache.Width, _zoomCache.Height),
                        new RectangleF(0, 0, _zoomCache.Width, _zoomCache.Height), GraphicsUnit.Pixel);
                    g.PixelOffsetMode = PixelOffsetMode.Default;
                }
                else
                {
                    g.InterpolationMode = InterpolationMode.HighQualityBilinear;
                    g.PixelOffsetMode = PixelOffsetMode.Half;
                    g.DrawImage(_cache!, ToScreen(src), src, GraphicsUnit.Pixel);
                    g.PixelOffsetMode = PixelOffsetMode.Default;
                }
            }
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using var outline = new Pen(Color.FromArgb(0, 150, 255), 1f) { DashStyle = DashStyle.Dash };
            foreach (var obj in _selection)
            {
                if (obj is LineObject line)
                    g.DrawLine(outline, ToScreen(line.Start), ToScreen(line.End));
                else if (obj is IntersectionObject)
                {
                    var r = ToScreen(CanvasRenderer.GetBounds(_document, obj));
                    g.DrawRectangle(outline, r.X, r.Y, r.Width, r.Height);
                }
                else
                    g.DrawPolygon(outline, Corners(obj));
            }

            if (_guides.Count > 0)
            {
                using var guide = new Pen(Color.FromArgb(255, 0, 200));
                foreach (var (vertical, at) in _guides)
                {
                    if (vertical)
                        g.DrawLine(guide, ToScreen(new PointF(at, 0f)), ToScreen(new PointF(at, _document.Height)));
                    else
                        g.DrawLine(guide, ToScreen(new PointF(0f, at)), ToScreen(new PointF(_document.Width, at)));
                }
            }

            if (_drag == Drag.Create) return;
            if (IsFrame)
            {
                var box = SelectionBox();
                var screenBox = ToScreen(box);
                using var frame = new Pen(Color.FromArgb(0, 120, 215)) { DashStyle = DashStyle.Dash };
                g.DrawRectangle(frame, screenBox.X, screenBox.Y, screenBox.Width, screenBox.Height);
                using var frameFill = new SolidBrush(Color.White);
                using var frameBorder = new Pen(Color.FromArgb(0, 120, 215));
                foreach (var h in FrameHandles(box))
                {
                    var r = new RectangleF(h.X - HandleSize / 2f, h.Y - HandleSize / 2f, HandleSize, HandleSize);
                    g.FillRectangle(frameFill, r);
                    g.DrawRectangle(frameBorder, r.X, r.Y, r.Width, r.Height);
                }
                var topMid = ToScreen(new PointF(box.X + box.Width / 2f, box.Top));
                var rh = FrameRotateHandle(box);
                g.DrawLine(frameBorder, topMid, rh);
                DrawRound(g, rh, frameFill, frameBorder);
                return;
            }
            if (_selection.Count != 1) return;
            var single = _selection[0];
            using var fill = new SolidBrush(Color.White);
            using var border = new Pen(Color.FromArgb(0, 120, 215));
            using var anchorFill = new SolidBrush(Color.Gold);

            if (single is LineObject l)
            {
                DrawRound(g, ToScreen(l.Start), anchorFill, border);
                DrawRound(g, ToScreen(l.End), anchorFill, border);
                return;
            }
            if (!single.CanTransform) return;

            foreach (var h in BoxHandles(single))
            {
                var r = new RectangleF(h.X - HandleSize / 2f, h.Y - HandleSize / 2f, HandleSize, HandleSize);
                g.FillRectangle(fill, r);
                g.DrawRectangle(border, r.X, r.Y, r.Width, r.Height);
            }
            if (single.CanRotate)
            {
                var b = single.Bounds;
                var top = ToScreen(single.ToWorld(new PointF(b.X + b.Width / 2f, b.Top)));
                var rh = RotateHandle(single);
                g.DrawLine(border, top, rh);
                DrawRound(g, rh, fill, border);
            }
            if (single is ArcObject arc)
            {
                DrawDiamond(g, ToScreen(arc.StartPoint), anchorFill, border);
                DrawDiamond(g, ToScreen(arc.EndPoint), anchorFill, border);
            }
        }

        private static void DrawRound(Graphics g, PointF p, Brush fill, Pen border)
        {
            float d = HandleSize + 2;
            g.FillEllipse(fill, p.X - d / 2f, p.Y - d / 2f, d, d);
            g.DrawEllipse(border, p.X - d / 2f, p.Y - d / 2f, d, d);
        }

        private static void DrawDiamond(Graphics g, PointF p, Brush fill, Pen border)
        {
            float r = HandleSize / 2f + 2;
            PointF[] pts = [new(p.X, p.Y - r), new(p.X + r, p.Y), new(p.X, p.Y + r), new(p.X - r, p.Y)];
            g.FillPolygon(fill, pts);
            g.DrawPolygon(border, pts);
        }

        private static void DrawChecker(Graphics g, Rectangle area)
        {
            const int cell = 8;
            using var light = new SolidBrush(Color.FromArgb(70, 70, 76));
            using var dark = new SolidBrush(Color.FromArgb(52, 52, 58));
            g.FillRectangle(dark, area);
            var clip = g.Clip;
            g.SetClip(area);
            for (int y = area.Top; y < area.Bottom; y += cell)
                for (int x = area.Left + ((y - area.Top) / cell % 2) * cell; x < area.Right; x += cell * 2)
                    g.FillRectangle(light, x, y, cell, cell);
            g.Clip = clip;
        }

        private static bool Near(PointF a, Point b, float radius) =>
            (a.X - b.X) * (a.X - b.X) + (a.Y - b.Y) * (a.Y - b.Y) <= radius * radius;

        private (Drag Mode, int Handle) GripAt(Point p)
        {
            if (IsFrame)
            {
                var box = SelectionBox();
                float g = HandleSize / 2f + 4;
                if (Near(FrameRotateHandle(box), p, g)) return (Drag.FrameRotate, -1);
                var hs = FrameHandles(box);
                for (int i = 0; i < hs.Length; i++)
                    if (Near(hs[i], p, g)) return (Drag.FrameResize, i);
                return (Drag.None, -1);
            }
            if (_selection.Count != 1) return (Drag.None, -1);
            var single = _selection[0];
            float grab = HandleSize / 2f + 4;
            if (single is LineObject line)
            {
                if (Near(ToScreen(line.End), p, grab)) return (Drag.LineEnd, -1);
                if (Near(ToScreen(line.Start), p, grab)) return (Drag.LineStart, -1);
                return (Drag.None, -1);
            }
            if (!single.CanTransform) return (Drag.None, -1);
            if (single is ArcObject arc)
            {
                if (Near(ToScreen(arc.EndPoint), p, grab)) return (Drag.ArcEnd, -1);
                if (Near(ToScreen(arc.StartPoint), p, grab)) return (Drag.ArcStart, -1);
            }
            if (single.CanRotate && Near(RotateHandle(single), p, grab)) return (Drag.Rotate, -1);
            var handles = BoxHandles(single);
            for (int i = 0; i < handles.Length; i++)
                if (Near(handles[i], p, grab)) return (Drag.Resize, i);
            return (Drag.None, -1);
        }

        private void BeginEdit()
        {
            if (_committed) return;
            _committed = true;
            EditCommitting?.Invoke(this, EventArgs.Empty);
        }

        private void Edited(bool final)
        {
            _document?.NotifyChanged();
            DocumentEdited?.Invoke(this, new CanvasEditedEventArgs(final));
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            Focus();
            if (_document is null) return;
            if (e.Button == MouseButtons.Middle || (e.Button == MouseButtons.Left && _spaceDown))
            {
                _panning = true;
                _panLast = e.Location;
                Cursor = Cursors.SizeAll;
                return;
            }
            if (e.Button == MouseButtons.Right)
            {
                var under = CanvasRenderer.HitTestAll(_document, ToDoc(e.Location), 4f / ViewScale);
                if (under.Count > 0) PickRequested?.Invoke(this, new CanvasPickEventArgs(under, e.Location));
                return;
            }
            if (e.Button != MouseButtons.Left) return;

            var p = ToDoc(e.Location);
            _dragStart = p;
            _lastPoint = p;
            _committed = false;

            var (mode, handle) = GripAt(e.Location);
            if (mode != Drag.None)
            {
                _drag = mode;
                _handle = handle;
                if (mode is Drag.FrameResize or Drag.FrameRotate)
                {
                    _resizeFrom = SelectionBox();
                    CaptureFrame();
                }
                else
                {
                    _resizeFrom = _selection[0].Bounds;
                }
                CollectSnapTargets();
                return;
            }

            if (Tool != CanvasTool.Select)
            {
                BeginEdit();
                _creating = CreateObject(Tool, p);
                _document.Add(_creating);
                _drag = Drag.Create;
                SelectOnly(_creating);
                CollectSnapTargets();
                _dragStart = SnapPoint(p);
                _guides.Clear();
                Edited(false);
                return;
            }

            var hit = CanvasRenderer.HitTest(_document, p, 4f / ViewScale);
            bool shift = (ModifierKeys & Keys.Shift) != 0;
            if (hit is null)
            {
                if (!shift) SelectOnly(null);
                _drag = Drag.None;
                return;
            }

            if (shift)
            {
                var unit = _document.WithGroups([hit]);
                if (unit.All(_selection.Contains)) _selection.RemoveAll(unit.Contains);
                else _selection.AddRange(unit.Where(o => !_selection.Contains(o)));
                Invalidate();
                SelectionChanged?.Invoke(this, EventArgs.Empty);
                _drag = Drag.None;
                return;
            }

            if (!_selection.Contains(hit)) SelectOnly(hit);
            _drag = Drag.Move;
            _duplicateOnDrag = (ModifierKeys & Keys.Control) != 0;
            _moveApplied = PointF.Empty;
            _moveBox = SelectionBox();
            CollectSnapTargets();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (_document is null) return;
            if (_panning)
            {
                PanBy(e.X - _panLast.X, e.Y - _panLast.Y);
                _panLast = e.Location;
                return;
            }
            _lastMouse = e.Location;
            var p = ToDoc(e.Location);
            bool shift = (ModifierKeys & Keys.Shift) != 0;
            var single = _selection.Count == 1 ? _selection[0] : null;
            if (_drag != Drag.None)
            {
                if ((ModifierKeys & Keys.Alt) != 0) _altUsed = true;
                bool hadGuides = _guides.Count > 0;
                _guides.Clear();
                if (hadGuides) Invalidate();
            }

            switch (_drag)
            {
                case Drag.Create when _creating is not null:
                    ShapeCreated(_creating, _dragStart, shift ? p : SnapPoint(p), shift, (ModifierKeys & Keys.Control) != 0);
                    Invalidate();
                    Edited(false);
                    break;
                case Drag.Move:
                    float rx = p.X - _dragStart.X, ry = p.Y - _dragStart.Y;
                    if (!_committed && rx == 0f && ry == 0f) break;
                    BeginEdit();
                    if (_duplicateOnDrag)
                    {
                        _duplicateOnDrag = false;
                        var copies = _document.Duplicate(_selection, 0f, 0f);
                        _selection.Clear();
                        _selection.AddRange(copies);
                        SelectionChanged?.Invoke(this, EventArgs.Empty);
                        CollectSnapTargets();
                    }
                    if (SnapActive)
                    {
                        var box = _moveBox;
                        box.Offset(rx, ry);
                        float tol = SnapDistance / ViewScale;
                        rx += SnapAxis(_snapX, tol, true, box.Left, box.Left + box.Width / 2f, box.Right);
                        ry += SnapAxis(_snapY, tol, false, box.Top, box.Top + box.Height / 2f, box.Bottom);
                    }
                    float dx = rx - _moveApplied.X, dy = ry - _moveApplied.Y;
                    if (dx != 0f || dy != 0f)
                        foreach (var o in _selection.Where(o => o.CanTransform)) o.Offset(dx, dy);
                    _moveApplied = new PointF(rx, ry);
                    _lastPoint = p;
                    Edited(false);
                    Invalidate();
                    break;
                case Drag.Resize when single is not null:
                    BeginEdit();
                    var rp = p;
                    if (SnapActive && Math.Abs(Math.IEEERemainder(single.Rotation, 90.0)) < 0.01)
                    {
                        float tol = SnapDistance / ViewScale;
                        bool horiz = _handle is 0 or 2 or 3 or 4 or 6 or 7;
                        bool vert = _handle is 0 or 1 or 2 or 4 or 5 or 6;
                        if (((int)Math.Round(single.Rotation / 90.0) % 2 + 2) % 2 == 1) (horiz, vert) = (vert, horiz);
                        if (horiz) rp.X += SnapAxis(_snapX, tol, true, p.X);
                        if (vert) rp.Y += SnapAxis(_snapY, tol, false, p.Y);
                    }
                    single.Bounds = ResizeRotated(_resizeFrom, single.Rotation, _handle, rp, shift, (ModifierKeys & Keys.Control) != 0);
                    Edited(false);
                    Invalidate();
                    break;
                case Drag.FrameResize:
                {
                    BeginEdit();
                    var fp = p;
                    if (SnapActive)
                    {
                        float tol = SnapDistance / ViewScale;
                        if (_handle is 0 or 2 or 3 or 4 or 6 or 7) fp.X += SnapAxis(_snapX, tol, true, p.X);
                        if (_handle is 0 or 1 or 2 or 4 or 5 or 6) fp.Y += SnapAxis(_snapY, tol, false, p.Y);
                    }
                    bool tilted = _frameOriginals.Any(f => f.Original is not LineObject && f.Original.CanRotate
                        && Math.Abs(Math.IEEERemainder(f.Original.Rotation, 90.0)) > 0.01);
                    ApplyFrameScale(_resizeFrom, ResizeRect(_resizeFrom, _handle, fp, shift || tilted, (ModifierKeys & Keys.Control) != 0));
                    Edited(false);
                    Invalidate();
                    break;
                }
                case Drag.FrameRotate:
                {
                    BeginEdit();
                    var c = new PointF(_resizeFrom.X + _resizeFrom.Width / 2f, _resizeFrom.Y + _resizeFrom.Height / 2f);
                    double a0 = Math.Atan2(_dragStart.Y - c.Y, _dragStart.X - c.X) * 180.0 / Math.PI;
                    double a1 = Math.Atan2(p.Y - c.Y, p.X - c.X) * 180.0 / Math.PI;
                    double delta = a1 - a0;
                    if (shift) delta = Math.Round(delta / 15.0) * 15.0;
                    ApplyFrameRotation(c, (float)delta);
                    Edited(false);
                    Invalidate();
                    break;
                }
                case Drag.Rotate when single is not null:
                    BeginEdit();
                    single.Rotation = RotationTo(single.Center, p, shift);
                    Edited(false);
                    break;
                case Drag.LineStart when single is LineObject line:
                    BeginEdit();
                    line.Start = shift ? SnapAngle(line.End, p) : SnapPoint(p);
                    Edited(false);
                    break;
                case Drag.LineEnd when single is LineObject line:
                    BeginEdit();
                    line.End = shift ? SnapAngle(line.Start, p) : SnapPoint(p);
                    Edited(false);
                    break;
                case Drag.ArcStart when single is ArcObject arc:
                    BeginEdit();
                    arc.MoveStartTo(p, shift ? 15f : 0f);
                    Edited(false);
                    break;
                case Drag.ArcEnd when single is ArcObject arc:
                    BeginEdit();
                    arc.MoveEndTo(p, shift ? 15f : 0f);
                    Edited(false);
                    break;
                case Drag.None:
                    UpdateCursor(e.Location);
                    break;
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (_panning)
            {
                _panning = false;
                Cursor = _spaceDown ? Cursors.Hand : Cursors.Default;
                return;
            }
            if (_document is null || _drag == Drag.None) { _drag = Drag.None; return; }

            bool created = _drag == Drag.Create;
            if (created && _creating is not null)
            {
                var b = _creating.Bounds;
                if (b.Width < 3f && b.Height < 3f)
                    ApplyDefaultSize(_creating, _dragStart);
            }

            bool changed = _committed;
            _drag = Drag.None;
            _guides.Clear();
            _frameOriginals.Clear();
            _duplicateOnDrag = false;
            _creating = null;
            _committed = false;
            if (created) Tool = CanvasTool.Select;
            Invalidate();
            if (changed) Edited(true);
        }

        private RectangleF WorldBox(CanvasObject obj)
        {
            if (_document is null) return RectangleF.Empty;
            if (obj is IntersectionObject) return CanvasRenderer.GetBounds(_document, obj);
            using var path = obj.BuildPath();
            return path.PointCount > 0 ? path.GetBounds() : obj.Bounds;
        }

        private RectangleF SelectionBox()
        {
            RectangleF? box = null;
            foreach (var o in _selection.Where(o => o.CanTransform))
            {
                var b = WorldBox(o);
                box = box is null ? b : RectangleF.Union(box.Value, b);
            }
            return box ?? RectangleF.Empty;
        }

        private void CollectSnapTargets()
        {
            _snapX.Clear();
            _snapY.Clear();
            if (_document is null) return;
            _snapX.AddRange([0f, _document.Width / 2f, _document.Width]);
            _snapY.AddRange([0f, _document.Height / 2f, _document.Height]);
            foreach (var o in _document.Objects)
            {
                if (_selection.Contains(o)) continue;
                var b = WorldBox(o);
                if (b.Width <= 0f && b.Height <= 0f) continue;
                _snapX.AddRange([b.Left, b.Left + b.Width / 2f, b.Right]);
                _snapY.AddRange([b.Top, b.Top + b.Height / 2f, b.Bottom]);
            }
        }

        private float SnapAxis(List<float> targets, float tolerance, bool vertical, params float[] edges)
        {
            float best = float.MaxValue, at = 0f;
            foreach (var t in targets)
                foreach (var e in edges)
                {
                    float d = t - e;
                    if (Math.Abs(d) <= tolerance && Math.Abs(d) < Math.Abs(best)) { best = d; at = t; }
                }
            if (best == float.MaxValue) return 0f;
            _guides.Add((vertical, at));
            return best;
        }

        private PointF SnapPoint(PointF p)
        {
            if (!SnapActive) return p;
            float tol = SnapDistance / ViewScale;
            return new PointF(p.X + SnapAxis(_snapX, tol, true, p.X), p.Y + SnapAxis(_snapY, tol, false, p.Y));
        }

        protected override void WndProc(ref Message m)
        {
            const int WM_SYSKEYDOWN = 0x104, WM_SYSKEYUP = 0x105, VK_MENU = 0x12;
            bool alt = (m.Msg == WM_SYSKEYDOWN || m.Msg == WM_SYSKEYUP) && (int)m.WParam == VK_MENU;
            if (alt && m.Msg == WM_SYSKEYUP && (_altUsed || _drag != Drag.None))
            {
                _altUsed = false;
                if (_drag != Drag.None)
                    OnMouseMove(new MouseEventArgs(MouseButtons.Left, 0, _lastMouse.X, _lastMouse.Y, 0));
                return;
            }
            base.WndProc(ref m);
            if (alt && m.Msg == WM_SYSKEYDOWN && _drag != Drag.None)
            {
                _altUsed = true;
                OnMouseMove(new MouseEventArgs(MouseButtons.Left, 0, _lastMouse.X, _lastMouse.Y, 0));
            }
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            if (_document is null) return;
            float notches = e.Delta / 120f;
            if ((ModifierKeys & Keys.Control) != 0)
                ZoomAt(e.Location, MathF.Pow(1.25f, notches));
            else if ((ModifierKeys & Keys.Shift) != 0)
                PanBy(notches * 60f, 0f);
            else
                PanBy(0f, notches * 60f);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            ClampPan();
            ZoomChanged?.Invoke(this, EventArgs.Empty);
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);
            if (e.KeyCode == Keys.Space)
            {
                _spaceDown = false;
                if (!_panning) Cursor = Cursors.Default;
                e.Handled = true;
            }
        }

        private void UpdateCursor(Point p)
        {
            if (_spaceDown) { Cursor = Cursors.Hand; return; }
            var (mode, handle) = GripAt(p);
            if (mode == Drag.Resize)
            {
                Cursor = ResizeCursor(handle, _selection[0].Rotation);
                return;
            }
            if (mode == Drag.FrameResize)
            {
                Cursor = ResizeCursor(handle, 0f);
                return;
            }
            if (mode != Drag.None)
            {
                Cursor = Cursors.Hand;
                return;
            }
            if (Tool != CanvasTool.Select) { Cursor = Cursors.Cross; return; }
            Cursor = _document is not null && CanvasRenderer.HitTest(_document, ToDoc(p), 4f / ViewScale) is not null
                ? Cursors.SizeAll : Cursors.Default;
        }

        private static Cursor ResizeCursor(int handle, float rotation)
        {
            int[] baseAngles = [135, 90, 45, 0, 135, 90, 45, 0];
            int a = (int)Math.Round(((baseAngles[handle] - rotation) % 180 + 180) % 180 / 45.0) % 4;
            return a switch
            {
                0 => Cursors.SizeWE,
                1 => Cursors.SizeNESW,
                2 => Cursors.SizeNS,
                _ => Cursors.SizeNWSE
            };
        }

        private CanvasObject CreateObject(CanvasTool tool, PointF p)
        {
            CanvasObject obj = tool switch
            {
                CanvasTool.Rectangle => new RectObject(),
                CanvasTool.Ellipse => new EllipseObject(),
                CanvasTool.Polygon => new PolygonObject(),
                CanvasTool.Line => new LineObject { Start = p, End = p },
                CanvasTool.Arc => new ArcObject(),
                _ => new TextObject()
            };
            ConfigureNewObject?.Invoke(obj);
            if (obj is not LineObject) obj.Bounds = new RectangleF(p, SizeF.Empty);
            return obj;
        }

        private static void ShapeCreated(CanvasObject obj, PointF a, PointF b, bool shift, bool fromCenter)
        {
            if (obj is LineObject line)
            {
                line.Start = a;
                line.End = shift ? SnapAngle(a, b) : b;
                return;
            }
            float w = b.X - a.X, h = b.Y - a.Y;
            if (shift)
            {
                float m = Math.Max(Math.Abs(w), Math.Abs(h));
                w = Math.Sign(w == 0 ? 1 : w) * m;
                h = Math.Sign(h == 0 ? 1 : h) * m;
            }
            obj.Bounds = fromCenter
                ? Normalize(a.X - w, a.Y - h, a.X + w, a.Y + h)
                : Normalize(a.X, a.Y, a.X + w, a.Y + h);
        }

        private void ApplyDefaultSize(CanvasObject obj, PointF p)
        {
            if (_document is null) return;
            float side = Math.Max(8f, Math.Min(_document.Width, _document.Height) / 4f);
            if (obj is LineObject line)
            {
                line.Start = p;
                line.End = new PointF(p.X + side, p.Y);
                return;
            }
            if (obj is TextObject text)
            {
                var natural = text.NaturalSize(100f);
                float w = natural.Height > 0f ? side * natural.Width / natural.Height : side;
                obj.Bounds = new RectangleF(p.X, p.Y, Math.Max(1f, w), side);
                return;
            }
            obj.Bounds = new RectangleF(p.X, p.Y, side, side);
        }

        private static PointF SnapAngle(PointF a, PointF b)
        {
            double dx = b.X - a.X, dy = b.Y - a.Y;
            double len = Math.Sqrt(dx * dx + dy * dy);
            double angle = Math.Round(Math.Atan2(dy, dx) / (Math.PI / 4)) * (Math.PI / 4);
            return new PointF(a.X + (float)(Math.Cos(angle) * len), a.Y + (float)(Math.Sin(angle) * len));
        }

        public static float RotationTo(PointF center, PointF p, bool snap)
        {
            double deg = Math.Atan2(p.Y - center.Y, p.X - center.X) * 180.0 / Math.PI + 90.0;
            if (snap) deg = Math.Round(deg / 15.0) * 15.0;
            deg = ((deg % 360.0) + 360.0) % 360.0;
            return (float)deg;
        }

        private static RectangleF Normalize(float x1, float y1, float x2, float y2) =>
            RectangleF.FromLTRB(Math.Min(x1, x2), Math.Min(y1, y2), Math.Max(x1, x2), Math.Max(y1, y2));

        public static RectangleF ResizeRotated(RectangleF from, float rotation, int handle, PointF world, bool keepAspect, bool fromCenter = false)
        {
            var c = new PointF(from.X + from.Width / 2f, from.Y + from.Height / 2f);
            var local = CanvasObject.Rotate(world, c, -rotation);
            var r = ResizeRect(from, handle, local, keepAspect, fromCenter);
            var rc = CanvasObject.Rotate(new PointF(r.X + r.Width / 2f, r.Y + r.Height / 2f), c, rotation);
            return new RectangleF(rc.X - r.Width / 2f, rc.Y - r.Height / 2f, r.Width, r.Height);
        }

        public static RectangleF ResizeRect(RectangleF from, int handle, PointF p, bool keepAspect, bool fromCenter = false)
        {
            bool left = handle is 0 or 6 or 7, right = handle is 2 or 3 or 4;
            bool top = handle is 0 or 1 or 2, bottom = handle is 4 or 5 or 6;
            bool horiz = left || right, vert = top || bottom;
            float cx = from.X + from.Width / 2f, cy = from.Y + from.Height / 2f;

            float w = !horiz ? from.Width : fromCenter ? 2f * Math.Abs(p.X - cx) : Math.Abs(p.X - (left ? from.Right : from.Left));
            float h = !vert ? from.Height : fromCenter ? 2f * Math.Abs(p.Y - cy) : Math.Abs(p.Y - (top ? from.Bottom : from.Top));

            if (keepAspect && from.Width > 0f && from.Height > 0f)
            {
                float aspect = from.Width / from.Height;
                if (horiz && vert)
                {
                    if (w / aspect > h) h = w / aspect; else w = h * aspect;
                }
                else if (horiz)
                {
                    h = w / aspect;
                }
                else if (vert)
                {
                    w = h * aspect;
                }
            }

            var (l, r) = Axis(horiz && !fromCenter, left, from.Left, from.Right, p.X, w);
            var (t, b) = Axis(vert && !fromCenter, top, from.Top, from.Bottom, p.Y, h);
            return Normalize(l, t, r, b);
        }

        private static (float Low, float High) Axis(bool anchored, bool lowSide, float lo, float hi, float pointer, float size)
        {
            if (!anchored)
            {
                float c = (lo + hi) / 2f;
                return (c - size / 2f, c + size / 2f);
            }
            float fixedEdge = lowSide ? hi : lo;
            float dir = Math.Sign(pointer - fixedEdge);
            if (dir == 0f) dir = lowSide ? -1f : 1f;
            float other = fixedEdge + dir * size;
            return (Math.Min(fixedEdge, other), Math.Max(fixedEdge, other));
        }

        protected override bool IsInputKey(Keys keyData)
        {
            switch (keyData & Keys.KeyCode)
            {
                case Keys.Left:
                case Keys.Right:
                case Keys.Up:
                case Keys.Down:
                    return true;
            }
            return base.IsInputKey(keyData);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (_document is null) return;
            if (e.KeyCode == Keys.Space)
            {
                _spaceDown = true;
                if (!_panning) Cursor = Cursors.Hand;
                e.Handled = true;
                return;
            }
            if (e.Control)
            {
                var center = new PointF(ClientSize.Width / 2f, ClientSize.Height / 2f);
                switch (e.KeyCode)
                {
                    case Keys.D0:
                    case Keys.NumPad0:
                        FitToWindow();
                        e.Handled = true;
                        return;
                    case Keys.Oemplus:
                    case Keys.Add:
                        ZoomAt(center, 1.25f);
                        e.Handled = true;
                        return;
                    case Keys.OemMinus:
                    case Keys.Subtract:
                        ZoomAt(center, 0.8f);
                        e.Handled = true;
                        return;
                    case Keys.G when e.Shift:
                        UngroupSelection();
                        e.Handled = true;
                        return;
                    case Keys.G:
                        GroupSelection();
                        e.Handled = true;
                        return;
                    case Keys.D when _selection.Count > 0:
                        float shift = Math.Max(4f, Math.Min(_document.Width, _document.Height) / 32f);
                        _committed = false;
                        BeginEdit();
                        var copies = _document.Duplicate(_selection, shift, shift);
                        _selection.Clear();
                        _selection.AddRange(copies);
                        SelectionChanged?.Invoke(this, EventArgs.Empty);
                        _committed = false;
                        Edited(true);
                        e.Handled = true;
                        return;
                }
            }
            if (_selection.Count == 0 || e.Control || e.Alt) return;

            if (e.KeyCode == Keys.Delete)
            {
                _committed = false;
                BeginEdit();
                foreach (var o in _selection.ToList()) _document.Remove(o);
                _selection.Clear();
                SelectionChanged?.Invoke(this, EventArgs.Empty);
                _committed = false;
                Edited(true);
                e.Handled = true;
                return;
            }

            float step = e.Shift ? 10f : 1f;
            (float dx, float dy) = e.KeyCode switch
            {
                Keys.Left => (-step, 0f),
                Keys.Right => (step, 0f),
                Keys.Up => (0f, -step),
                Keys.Down => (0f, step),
                _ => (0f, 0f)
            };
            if (dx == 0f && dy == 0f) return;
            var movable = _selection.Where(o => o.CanTransform).ToList();
            if (movable.Count == 0) return;
            _committed = false;
            BeginEdit();
            foreach (var o in movable) o.Offset(dx, dy);
            _committed = false;
            Edited(true);
            e.Handled = true;
        }
    }
}
