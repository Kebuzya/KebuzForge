using System;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Windows.Forms;
using KebuzForge.App.Core;
using KebuzForge.Canvas;

namespace KebuzForge.App
{
    public partial class MainForm
    {
        private CanvasDocument? _canvas;
        private bool _quietRebuild;
        private bool _syncingCanvasUi;
        private object? _lastCanvasEditSource;
        private const int MaxCanvasArea = 2048 * 2048;
        private readonly System.Windows.Forms.Timer _canvasTimer = new() { Interval = 50 };
        private (ToolStripButton Button, CanvasTool Tool)[] _canvasTools = [];

        private void WireCanvas()
        {
            _canvasTools =
            [
                (btnCanvasSelect, CanvasTool.Select), (btnCanvasRect, CanvasTool.Rectangle),
                (btnCanvasEllipse, CanvasTool.Ellipse), (btnCanvasPolygon, CanvasTool.Polygon),
                (btnCanvasLine, CanvasTool.Line), (btnCanvasArc, CanvasTool.Arc), (btnCanvasText, CanvasTool.Text)
            ];
            foreach (var (button, tool) in _canvasTools)
                button.Click += (s, e) => canvasView.Tool = tool;
            canvasView.ToolChanged += (s, e) =>
            {
                foreach (var (button, tool) in _canvasTools)
                    button.Checked = tool == canvasView.Tool;
                UpdateCanvasButtons();
            };

            using (var fonts = new InstalledFontCollection())
                foreach (var family in fonts.Families)
                    cmbCanvasFont.Items.Add(family.Name);
            int arial = cmbCanvasFont.Items.IndexOf("Arial");
            if (cmbCanvasFont.Items.Count > 0)
                cmbCanvasFont.SelectedIndex = arial >= 0 ? arial : 0;

            btnStartOpen.Click += (s, e) => OpenFileDialog();
            btnStartCanvas.Click += (s, e) => NewCanvas();
            btnStartPixelArt.Click += (s, e) => NewPixelArt();

            canvasView.ConfigureNewObject = ConfigureNewCanvasObject;
            canvasView.EditCommitting += (s, e) =>
            {
                if (_processedImage is not null) PushHistory();
                UpdateUndoRedoButtons();
            };
            canvasView.DocumentEdited += (s, e) =>
            {
                OnCanvasEdited(e.Final);
                if (e.Final) SyncCanvasUi();
            };
            canvasView.SelectionChanged += (s, e) => SyncCanvasUi();
            canvasView.ZoomChanged += (s, e) =>
            {
                if (_canvas is not null)
                    lblStatusZoom.Text = $"{Lang.T("Масштаб")}: {Math.Round(canvasView.ZoomPercent)}%";
            };
            btnCanvasFit.Click += (s, e) => canvasView.FitToWindow();
            btnCanvasSnap.CheckedChanged += (s, e) => canvasView.SnapEnabled = btnCanvasSnap.Checked;
            canvasView.PickRequested += ShowCanvasPickMenu;
            _canvasTimer.Tick += (s, e) =>
            {
                _canvasTimer.Stop();
                RecomputeFromCanvas();
            };

            menuFileNewCanvas.Click += (s, e) => NewCanvas();
            btnCanvasNew.Click += (s, e) => NewCanvas();
            btnCanvasAddImage.Click += (s, e) => AddImageFromDialog();

            chkCanvasFill.CheckedChanged += (s, e) =>
                ApplyToCanvasSelection(null, o => o.Fill = CanvasFill, o => !o.IsStrokeOnly);
            btnCanvasFillColor.Click += (s, e) =>
            {
                if (!PickCanvasColor(btnCanvasFillColor)) return;
                SetCanvasChecked(chkCanvasFill, true);
                ApplyToCanvasSelection(null, o => o.Fill = CanvasFill, o => !o.IsStrokeOnly);
            };
            btnCanvasFillPick.Click += (s, e) =>
            {
                if (!PickCanvasColorFromScreen(btnCanvasFillColor)) return;
                SetCanvasChecked(chkCanvasFill, true);
                ApplyToCanvasSelection(null, o => o.Fill = CanvasFill, o => !o.IsStrokeOnly);
            };
            btnCanvasStrokePick.Click += (s, e) =>
            {
                if (!PickCanvasColorFromScreen(btnCanvasStrokeColor)) return;
                SetCanvasChecked(chkCanvasStroke, true);
                ApplyToCanvasSelection(null, o => o.Stroke = CanvasStroke(o), o => o is not IntersectionObject);
            };
            btnCanvasShadowPick.Click += (s, e) =>
            {
                if (!PickCanvasColorFromScreen(btnCanvasShadowColor)) return;
                SetCanvasChecked(chkCanvasShadow, true);
                ApplyToCanvasSelection(null, o => o.Shadow = CanvasShadow);
            };
            chkCanvasStroke.CheckedChanged += (s, e) =>
                ApplyToCanvasSelection(null, o => o.Stroke = CanvasStroke(o), o => !o.IsStrokeOnly && o is not IntersectionObject);
            btnCanvasStrokeColor.Click += (s, e) =>
            {
                if (!PickCanvasColor(btnCanvasStrokeColor)) return;
                SetCanvasChecked(chkCanvasStroke, true);
                ApplyToCanvasSelection(null, o => o.Stroke = CanvasStroke(o), o => o is not IntersectionObject);
            };
            nudCanvasWidth.ValueChanged += (s, e) =>
                ApplyToCanvasSelection(nudCanvasWidth, o => o.StrokeWidth = (float)nudCanvasWidth.Value, o => o is not IntersectionObject);
            nudCanvasOpacity.ValueChanged += (s, e) =>
                ApplyToCanvasSelection(nudCanvasOpacity, o => o.Opacity = (float)nudCanvasOpacity.Value / 100f);

            chkCanvasShadow.CheckedChanged += (s, e) => ApplyToCanvasSelection(null, o => o.Shadow = CanvasShadow);
            btnCanvasShadowColor.Click += (s, e) =>
            {
                if (!PickCanvasColor(btnCanvasShadowColor)) return;
                SetCanvasChecked(chkCanvasShadow, true);
                ApplyToCanvasSelection(null, o => o.Shadow = CanvasShadow);
            };
            nudCanvasShadowX.ValueChanged += (s, e) =>
                ApplyToCanvasSelection(nudCanvasShadowX, o => o.Shadow = CanvasShadow, o => o.Shadow is not null);
            nudCanvasShadowY.ValueChanged += (s, e) =>
                ApplyToCanvasSelection(nudCanvasShadowY, o => o.Shadow = CanvasShadow, o => o.Shadow is not null);

            txtCanvasText.TextChanged += (s, e) =>
            {
                if (!txtCanvasText.Focused) return;
                ApplyToCanvasSelection(txtCanvasText, o => RetextKeepHeight((TextObject)o, t => t.Text = txtCanvasText.Text), o => o is TextObject);
            };
            cmbCanvasFont.SelectedIndexChanged += (s, e) =>
                ApplyToCanvasSelection(null, o => RetextKeepHeight((TextObject)o, t => t.FontName = CanvasFontName), o => o is TextObject);
            chkCanvasBold.CheckedChanged += (s, e) =>
                ApplyToCanvasSelection(null, o => RetextKeepHeight((TextObject)o, t => t.Bold = chkCanvasBold.Checked), o => o is TextObject);

            chkCanvasArrow.CheckedChanged += (s, e) => ApplyToCanvasSelection(null, o =>
            {
                if (o is LineObject l) l.ArrowEnd = chkCanvasArrow.Checked;
                if (o is ArcObject a) a.ArrowEnd = chkCanvasArrow.Checked;
            }, o => o is LineObject or ArcObject);
            nudCanvasRadius.ValueChanged += (s, e) =>
                ApplyToCanvasSelection(nudCanvasRadius, o => ((RectObject)o).CornerRadius = (float)nudCanvasRadius.Value, o => o is RectObject);
            nudCanvasRotation.ValueChanged += (s, e) =>
                ApplyToCanvasSelection(nudCanvasRotation, o => o.Rotation = (float)nudCanvasRotation.Value, o => o.CanRotate);

            btnCanvasUp.Click += (s, e) => ReorderCanvasSelection(up: true);
            btnCanvasDown.Click += (s, e) => ReorderCanvasSelection(up: false);
            btnCanvasDelete.Click += (s, e) => DeleteCanvasSelection();
            btnCanvasIntersect.Click += (s, e) => IntersectCanvasSelection();
            btnCanvasGroup.Click += (s, e) => canvasView.GroupSelection();
            btnCanvasUngroup.Click += (s, e) => canvasView.UngroupSelection();

            UpdateCanvasButtons();
        }

        private void ShowCanvasPickMenu(object? sender, CanvasPickEventArgs e)
        {
            cmsCanvasPick.Items.Clear();
            cmsCanvasPick.Renderer = menuStrip.Renderer;
            cmsCanvasPick.Items.Add(new ToolStripMenuItem(Lang.T("Объекты под курсором")) { Enabled = false });
            foreach (var obj in e.Objects)
            {
                var target = obj;
                var item = new ToolStripMenuItem(CanvasObjectName(obj)) { Checked = canvasView.SelectedObjects.Contains(obj) };
                item.Click += (s, a) =>
                {
                    canvasView.Tool = CanvasTool.Select;
                    canvasView.SelectOnly(target);
                };
                cmsCanvasPick.Items.Add(item);
            }
            cmsCanvasPick.Show(canvasView, e.Location);
        }

        private static string CanvasObjectName(CanvasObject o) => o switch
        {
            TextObject t => $"{Lang.T("Текст")} «{(t.Text.Length > 16 ? t.Text[..16] + "..." : t.Text)}»",
            RectObject => Lang.T("Прямоугольник"),
            EllipseObject => Lang.T("Эллипс"),
            PolygonObject => Lang.T("Треугольник"),
            LineObject => Lang.T("Линия"),
            ArcObject => Lang.T("Дуга"),
            ImageObject => Lang.T("Картинка"),
            IntersectionObject => Lang.T("Пересечение"),
            _ => o.GetType().Name
        };

        private Color? CanvasFill => chkCanvasFill.Checked ? btnCanvasFillColor.BackColor : null;

        private Color? CanvasStroke(CanvasObject o) =>
            chkCanvasStroke.Checked || o.IsStrokeOnly ? btnCanvasStrokeColor.BackColor : null;

        private ShadowSpec? CanvasShadow =>
            chkCanvasShadow.Checked
                ? new ShadowSpec((float)nudCanvasShadowX.Value, (float)nudCanvasShadowY.Value, btnCanvasShadowColor.BackColor)
                : null;

        private string CanvasFontName => cmbCanvasFont.SelectedItem as string ?? "Arial";

        private void ConfigureNewCanvasObject(CanvasObject o)
        {
            o.Fill = o.IsStrokeOnly ? null : CanvasFill;
            o.Stroke = CanvasStroke(o);
            o.StrokeWidth = Math.Max(1f, (float)nudCanvasWidth.Value);
            o.Opacity = (float)nudCanvasOpacity.Value / 100f;
            o.Shadow = CanvasShadow;
            switch (o)
            {
                case TextObject t:
                    t.Text = string.IsNullOrEmpty(txtCanvasText.Text) ? "A" : txtCanvasText.Text;
                    t.FontName = CanvasFontName;
                    t.Bold = chkCanvasBold.Checked;
                    break;
                case LineObject l:
                    l.ArrowEnd = chkCanvasArrow.Checked;
                    break;
                case ArcObject a:
                    a.ArrowEnd = chkCanvasArrow.Checked;
                    break;
                case RectObject r:
                    r.CornerRadius = (float)nudCanvasRadius.Value;
                    break;
            }
        }

        private static void RetextKeepHeight(TextObject t, Action<TextObject> change)
        {
            var b = t.Bounds;
            change(t);
            var natural = t.NaturalSize(100f);
            if (natural.Width > 0f && natural.Height > 0f && b.Height > 0f)
            {
                float w = b.Height * natural.Width / natural.Height;
                float cx = b.X + b.Width / 2f;
                t.Bounds = new RectangleF(cx - w / 2f, b.Y, w, b.Height);
            }
        }

        private bool PickCanvasColor(Button button)
        {
            var picked = ShowFreeColorPicker(button.BackColor);
            if (picked is null) return false;
            button.BackColor = picked.Value;
            return true;
        }

        private bool PickCanvasColorFromScreen(Button button)
        {
            using var picker = new Forms.ScreenPickerForm();
            if (picker.ShowDialog(this) != DialogResult.OK || picker.PickedColor is null) return false;
            button.BackColor = picker.PickedColor.Value;
            return true;
        }

        private void TranslateCanvasTips()
        {
            foreach (var b in new[] { btnCanvasFillPick, btnCanvasStrokePick, btnCanvasShadowPick })
                toolTipCanvas.SetToolTip(b, Lang.T("Пипетка: щёлкните в любом месте экрана (Esc - отмена)"));
            toolTipCanvas.SetToolTip(btnRemoveBg, Lang.T("Нажмите и щёлкайте по областям на «ПОСЛЕ», чтобы сделать их прозрачными. Похожие соседние цвета берутся по «Допуску». Каждый щелчок отменяется Ctrl+Z."));
            toolTipCanvas.SetToolTip(chkPerceptual, Lang.T("Вкл: пиксели раскладываются по цветам так, как их видит глаз, меньше случайных точек на контурах. Выкл: простое сравнение RGB, плавный градиент на маленькой палитре не рвётся."));
        }

        private void SetCanvasChecked(CheckBox box, bool value)
        {
            _syncingCanvasUi = true;
            box.Checked = value;
            _syncingCanvasUi = false;
        }

        private static void SetNud(NumericUpDown nud, float value) =>
            nud.Value = Math.Clamp((decimal)Math.Round(value), nud.Minimum, nud.Maximum);

        private void ApplyToCanvasSelection(object? source, Action<CanvasObject> apply, Func<CanvasObject, bool>? filter = null)
        {
            if (_syncingCanvasUi || _canvas is null) return;
            var targets = canvasView.SelectedObjects.Where(filter ?? (_ => true)).ToList();
            if (targets.Count == 0) return;
            if (source is null || source != _lastCanvasEditSource)
            {
                if (_processedImage is not null) PushHistory();
                _lastCanvasEditSource = source;
            }
            foreach (var o in targets) apply(o);
            _canvas.NotifyChanged();
            OnCanvasEdited(true);
            UpdateUndoRedoButtons();
        }

        private void ReorderCanvasSelection(bool up)
        {
            if (_canvas is null || canvasView.SelectedObjects.Count == 0) return;
            var ordered = canvasView.SelectedObjects.OrderBy(o => _canvas.IndexOf(o)).ToList();
            if (up) ordered.Reverse();
            if (_processedImage is not null) PushHistory();
            bool moved = false;
            foreach (var o in ordered)
                moved |= up ? _canvas.MoveUp(o) : _canvas.MoveDown(o);
            _canvas.NotifyChanged();
            if (moved) OnCanvasEdited(true);
            UpdateUndoRedoButtons();
        }

        private void DeleteCanvasSelection()
        {
            if (_canvas is null || canvasView.SelectedObjects.Count == 0) return;
            if (_processedImage is not null) PushHistory();
            foreach (var o in canvasView.SelectedObjects.ToList()) _canvas.Remove(o);
            _canvas.NotifyChanged();
            canvasView.SelectOnly(null);
            OnCanvasEdited(true);
            UpdateUndoRedoButtons();
        }

        private void IntersectCanvasSelection()
        {
            if (_canvas is null || !_canvas.CanIntersect(canvasView.SelectedObjects)) return;
            if (_processedImage is not null) PushHistory();
            var ix = _canvas.AddIntersection(canvasView.SelectedObjects, btnCanvasFillColor.BackColor);
            if (ix is null) return;
            _canvas.NotifyChanged();
            canvasView.SelectOnly(ix);
            OnCanvasEdited(true);
            UpdateUndoRedoButtons();
        }

        private void SyncCanvasUi()
        {
            _lastCanvasEditSource = null;
            UpdateCanvasButtons();
            if (canvasView.SelectedObjects.Count == 0) return;
            var o = canvasView.SelectedObjects[0];

            _syncingCanvasUi = true;
            try
            {
                if (!o.IsStrokeOnly)
                {
                    chkCanvasFill.Checked = o.Fill is not null;
                    if (o.Fill is { } fill) btnCanvasFillColor.BackColor = fill;
                    chkCanvasStroke.Checked = o.Stroke is not null;
                }
                if (o.Stroke is { } stroke) btnCanvasStrokeColor.BackColor = stroke;
                if (o is not IntersectionObject) SetNud(nudCanvasWidth, o.StrokeWidth);
                SetNud(nudCanvasOpacity, o.Opacity * 100f);
                chkCanvasShadow.Checked = o.Shadow is not null;
                if (o.Shadow is { } sh)
                {
                    btnCanvasShadowColor.BackColor = sh.Color;
                    SetNud(nudCanvasShadowX, sh.OffsetX);
                    SetNud(nudCanvasShadowY, sh.OffsetY);
                }
                if (o.CanRotate) SetNud(nudCanvasRotation, o.Rotation > 180f ? o.Rotation - 360f : o.Rotation);
                switch (o)
                {
                    case TextObject t:
                        txtCanvasText.Text = t.Text;
                        int idx = cmbCanvasFont.Items.IndexOf(t.FontName);
                        if (idx >= 0) cmbCanvasFont.SelectedIndex = idx;
                        chkCanvasBold.Checked = t.Bold;
                        break;
                    case LineObject l:
                        chkCanvasArrow.Checked = l.ArrowEnd;
                        break;
                    case ArcObject a:
                        chkCanvasArrow.Checked = a.ArrowEnd;
                        break;
                    case RectObject r:
                        SetNud(nudCanvasRadius, r.CornerRadius);
                        break;
                }
            }
            finally
            {
                _syncingCanvasUi = false;
            }
        }

        private void UpdateCanvasButtons()
        {
            bool active = _canvas is not null;
            var sel = canvasView.SelectedObjects;
            var tool = canvasView.Tool;
            int count = sel.Count;

            canvasToolStrip.Visible = active;
            grpCanvasLook.Enabled = active;
            grpCanvasShadow.Enabled = active;
            grpCanvasObject.Enabled = active;
            grpCanvasShape.Enabled = active;
            grpCanvasText.Enabled = active && (tool == CanvasTool.Text || sel.Any(o => o is TextObject));
            chkCanvasArrow.Enabled = tool is CanvasTool.Line or CanvasTool.Arc || sel.Any(o => o is LineObject or ArcObject);
            bool radius = tool == CanvasTool.Rectangle || sel.Any(o => o is RectObject);
            lblCanvasRadius.Enabled = radius;
            nudCanvasRadius.Enabled = radius;
            bool rotation = sel.Any(o => o.CanRotate);
            lblCanvasRotation.Enabled = rotation;
            nudCanvasRotation.Enabled = rotation;

            btnCanvasUp.Enabled = count > 0;
            btnCanvasDown.Enabled = count > 0;
            btnCanvasDelete.Enabled = count > 0;
            btnCanvasIntersect.Enabled = _canvas is not null && _canvas.CanIntersect(sel);
            btnCanvasGroup.Enabled = canvasView.CanGroupSelection;
            btnCanvasUngroup.Enabled = canvasView.CanUngroupSelection;
            btnCanvasAddImage.Enabled = active;
        }

        private void PrepareDialog(Form form)
        {
            Lang.Apply(form);
            UI.ThemeManager.Apply(form, _currentTheme);
        }

        private void NewCanvas()
        {
            using var dlg = new Forms.NewCanvasForm();
            PrepareDialog(dlg);
            if (dlg.ShowDialog(this) != DialogResult.OK) return;
            if (!ConfirmLeaveCanvas()) return;
            EnterCanvasMode(new CanvasDocument(dlg.CanvasWidth, dlg.CanvasHeight) { BackgroundColor = dlg.BackgroundColor }, fresh: true);
        }

        private void NewPixelArt()
        {
            using var dlg = new Forms.NewCanvasForm();
            dlg.UsePixelArtDefaults();
            PrepareDialog(dlg);
            if (dlg.ShowDialog(this) != DialogResult.OK) return;
            if (!ConfirmLeaveCanvas()) return;

            EnterCanvasMode(new CanvasDocument(dlg.CanvasWidth, dlg.CanvasHeight) { BackgroundColor = dlg.BackgroundColor }, fresh: true);
            _updatingAspect = true;
            nudWidth.Value = dlg.CanvasWidth;
            nudHeight.Value = dlg.CanvasHeight;
            _updatingAspect = false;
            RecomputeFromCanvas();
            tabBottom.SelectedTab = tabPageEditor;
        }

        private static Bitmap LoadBitmapCopy(string path)
        {
            using var file = new Bitmap(path);
            return new Bitmap(file);
        }

        private static Bitmap LimitArea(Bitmap bmp)
        {
            long area = (long)bmp.Width * bmp.Height;
            if (area <= MaxCanvasArea) return bmp;
            double k = Math.Sqrt((double)MaxCanvasArea / area);
            int w = Math.Max(1, (int)Math.Round(bmp.Width * k));
            int h = Math.Max(1, (int)Math.Round(bmp.Height * k));
            var small = new Bitmap(w, h, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            using (var g = Graphics.FromImage(small))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                using var wrap = new System.Drawing.Imaging.ImageAttributes();
                wrap.SetWrapMode(System.Drawing.Drawing2D.WrapMode.TileFlipXY);
                g.DrawImage(bmp, new Rectangle(0, 0, w, h), 0, 0, bmp.Width, bmp.Height, GraphicsUnit.Pixel, wrap);
            }
            bmp.Dispose();
            return small;
        }

        private void OpenImageAsCanvas(Bitmap bmp, string? path)
        {
            var (ow, oh) = (bmp.Width, bmp.Height);
            bmp = LimitArea(bmp);
            var doc = new CanvasDocument(bmp.Width, bmp.Height);
            doc.Add(new ImageObject(bmp) { Bounds = new RectangleF(0, 0, bmp.Width, bmp.Height) });
            EnterCanvasMode(doc, fresh: true, path);
            if (bmp.Width != ow)
                lblStatusZoom.Text = $"{Lang.T("Картинка уменьшена для скорости:")} {ow} × {oh} -> {bmp.Width} × {bmp.Height}";
        }

        private void AddOrOpenImageFile(string path)
        {
            if (string.Equals(System.IO.Path.GetExtension(path), ProjectFile.Extension, StringComparison.OrdinalIgnoreCase))
            {
                OpenProject(path);
                return;
            }
            if (string.Equals(System.IO.Path.GetExtension(path), ".gif", StringComparison.OrdinalIgnoreCase)
                && AnimationProcessor.IsAnimated(path))
            {
                LoadImageFromFile(path);
                return;
            }
            Bitmap bmp;
            try
            {
                bmp = LoadBitmapCopy(path);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{Lang.T("Ошибка загрузки")}:\n{ex.Message}", Lang.T("Ошибка"),
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            AddOrOpenImage(bmp, path);
        }

        private void AddOrOpenImage(Bitmap bmp, string? path)
        {
            if (_canvas is not null)
            {
                AddImageObject(bmp);
                return;
            }
            if (!ConfirmLeaveCanvas())
            {
                bmp.Dispose();
                return;
            }
            ClearAnimation();
            OpenImageAsCanvas(bmp, path);
        }

        private void AddImageObject(Bitmap bmp)
        {
            if (_canvas is null) return;
            bmp = LimitArea(bmp);
            float k = Math.Min(1f, Math.Min((float)_canvas.Width / bmp.Width, (float)_canvas.Height / bmp.Height));
            float w = bmp.Width * k, h = bmp.Height * k;
            var obj = new ImageObject(bmp)
            {
                Bounds = new RectangleF((_canvas.Width - w) / 2f, (_canvas.Height - h) / 2f, w, h)
            };
            if (_processedImage is not null) PushHistory();
            _canvas.Add(obj);
            _canvas.NotifyChanged();
            canvasView.Tool = CanvasTool.Select;
            canvasView.SelectOnly(obj);
            OnCanvasEdited(true);
            UpdateUndoRedoButtons();
        }

        private void AddImageFromDialog()
        {
            using var dlg = new OpenFileDialog
            {
                Title = Lang.T("Добавить картинку..."),
                Filter = Lang.T("Изображения|*.png;*.jpg;*.jpeg;*.bmp;*.gif|Все файлы|*.*")
            };
            if (dlg.ShowDialog(this) == DialogResult.OK)
                AddOrOpenImageFile(dlg.FileName);
        }

        private void EnterCanvasMode(CanvasDocument doc, bool fresh, string? path = null, bool recompute = true)
        {
            if (fresh)
            {
                ClearAnimation();
                SetOriginalImage(CanvasRenderer.Render(doc), path);
            }
            _canvas = doc;
            DeleteAutosave();
            _projectPath = null;
            _projectDirty = false;
            _autosaveDirty = false;
            _documentName = path is not null ? System.IO.Path.GetFileNameWithoutExtension(path) : Lang.T("Новый холст");
            canvasView.Document = doc;
            canvasView.Visible = true;
            canvasView.BringToFront();
            tlpStart.Visible = false;
            tabBottom.SelectedTab = tabPageCanvas;
            canvasView.Tool = CanvasTool.Select;
            if (fresh && recompute) RecomputeFromCanvas();
            UpdateCanvasButtons();
            UpdateTitle();
        }

        private void ExitCanvasMode()
        {
            _canvasTimer.Stop();
            DeleteAutosave();
            _canvas = null;
            _projectPath = null;
            _projectDirty = false;
            canvasView.Document = null;
            canvasView.Visible = false;
            UpdateCanvasButtons();
        }

        private bool ConfirmLeaveCanvas()
        {
            if (_canvas is null) return true;
            if (!ConfirmDiscardChanges()) return false;
            ExitCanvasMode();
            return true;
        }

        private void OnCanvasEdited(bool final)
        {
            if (_canvas is null) return;
            MarkDirty();
            if (final)
            {
                _canvasTimer.Stop();
                RecomputeFromCanvas();
            }
            else if (!_canvasTimer.Enabled)
            {
                _canvasTimer.Start();
            }
        }

        private void RenderCanvasToSource()
        {
            if (_canvas is null) return;
            var old = _originalImage;
            _originalImage = CanvasRenderer.Render(_canvas);
            picBefore.Image = _originalImage;
            old?.Dispose();
        }

        private void RecomputeFromCanvas()
        {
            if (_canvas is null) return;
            RenderCanvasToSource();
            if (_originalImage is null) return;
            _quietRebuild = true;
            var watch = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                RunPixelization(keepStages: true);
                _canvasTimer.Interval = (int)Math.Clamp(watch.ElapsedMilliseconds * 2, 50, 400);
            }
            catch (Exception ex)
            {
                lblStatusZoom.Text = $"{Lang.T("Ошибка пикселизации")}: {ex.Message}";
            }
            finally
            {
                _quietRebuild = false;
            }
            UpdateUndoRedoButtons();
        }

        private void RestoreCanvas(CanvasDocument? snapshot)
        {
            if (_canvas is null) return;
            var doc = snapshot ?? new CanvasDocument(_canvas.Width, _canvas.Height)
            {
                BackgroundColor = _canvas.BackgroundColor
            };
            _canvas = doc;
            canvasView.Document = doc;
            RenderCanvasToSource();
        }
    }
}
