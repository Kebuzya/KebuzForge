using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Windows.Forms;
using WinFormsApp1.Core;
using WinFormsApp1.Models;
using WinFormsApp1.UI;

namespace WinFormsApp1
{
    public partial class MainForm : Form
    {
        private Bitmap? _originalImage;
        private Bitmap? _pixelizedImage;
        private Bitmap? _processedImage;
        private Bitmap? _pipelineResult;
        private Bitmap? _editorOverlay;
        private Bitmap? _displayBitmap;
        private Color _bgColor = Color.White;
        private AppState _state = new();
        private bool _updatingAspect;
        private double _aspectRatio = 1.0;
        private Color[] _currentPalette = [];
        private byte[]? _paletteIndices;
        private Size _paletteIndexSize;
        private Color _edgeColor = Color.Black;
        private bool _sliderDragging;
        private bool _sliderHistoryPushed;
        private AppSettings _historySettings = new();
        private Bitmap? _historySource;
        private byte[]? _historyIndices;
        private Color[] _historyPalette = [];
        private readonly HistoryManager _history = new();
        private Bitmap? _effectResult;
        private bool _effectActive;
        private bool _flatActive;
        private bool _ditherActive;
        private bool _loadingSettings;
        private bool _eyedropperActive;
        private Color _eyedropperOrigBack, _eyedropperOrigFore;
        private readonly Dictionary<string, Form> _detachedForms = new();
        private UI.AppTheme _currentTheme = UI.AppTheme.Light;
        private ToolStripMenuItem[] _themeMenuItems = [];
        private static readonly string[] ThemeLabels = ["Светлая", "Тёмная", "Гибридная"];

        private List<Bitmap> _animFrames = [];
        private List<int> _animDelays = [];
        private List<Bitmap> _processedFrames = [];
        private int _animCurrentFrame;
        private System.Windows.Forms.Timer _animTimer = new() { Interval = 100 };

        public MainForm()
        {
            InitializeComponent();
            var asm = System.Reflection.Assembly.GetExecutingAssembly();
            var iconName = asm.GetManifestResourceNames()
                              .FirstOrDefault(n => n.EndsWith("kebuforg.ico"));
            if (iconName is not null)
                using (var stream = asm.GetManifestResourceStream(iconName)!)
                    this.Icon = new Icon(stream);
            WireEvents();
        }

        private ToolStripButton? _activeToolBtn;
        private bool _syncingEditor;

        private void WireEvents()
        {

            this.Shown += (s, e) =>
            {
                mainSplit.Panel1MinSize = 240;
                mainSplit.Panel2MinSize = 400;
                mainSplit.SplitterDistance = Math.Clamp(282, 240, mainSplit.Width - 400);

                rightSplit.Panel1MinSize = 150;
                rightSplit.Panel2MinSize = 100;
                rightSplit.SplitterDistance = Math.Clamp(420, 150, rightSplit.Height - 100);

                LoadSettings();
            };

            this.FormClosing += (s, e) => SaveSettings();

            this.AllowDrop = true;
            picBefore.AllowDrop = true;
            foreach (Control c in new Control[] { this, picBefore, btnDropHint })
            {
                c.DragEnter += OnDragEnter;
                c.DragDrop += OnDragDrop;
            }
            btnDropHint.Click += (s, e) => OpenFileDialog();

            foreach (var trk in new[] { trkSphereScale, trkSphereBulge, trkOffsetU, trkOffsetV,
                trkCornerSharp, trkAmbient, trkDiffuse, trkLightAzimuth, trkLightElev, trkDitherIntensity })
            {
                trk.MouseDown += (s, e) =>
                {
                    _sliderDragging = true;
                    _sliderHistoryPushed = false;
                };
                trk.MouseUp += (s, e) => _sliderDragging = false;
            }

            this.KeyPreview = true;
            this.KeyDown += OnKeyDown;

            rbEffectCylinder.CheckedChanged += (s, e) =>
                cmbCylinderDir.Enabled = rbEffectCylinder.Checked;

            rbEffectRounded.CheckedChanged += (s, e) =>
            {
                bool on = rbEffectRounded.Checked;
                lblCornerSharp.Enabled = on;
                trkCornerSharp.Enabled = on;
                chkShapeFromRect.Enabled = on;
            };
            chkShapeFromRect.CheckedChanged += (s, e) =>
            {
                bool inv = chkShapeFromRect.Checked;
                lblCornerSharp.Text = inv
                    ? $"{Lang.T("Скругление")}: {trkCornerSharp.Value}%"
                    : $"{Lang.T("Форма")}: {trkCornerSharp.Value}%";
                ReapplyEffectIfActive();
            };
            trkCornerSharp.ValueChanged += (s, e) =>
            {
                lblCornerSharp.Text = chkShapeFromRect.Checked
                    ? $"{Lang.T("Скругление")}: {trkCornerSharp.Value}%"
                    : $"{Lang.T("Форма")}: {trkCornerSharp.Value}%";
                ReapplyEffectIfActive();
            };

            trkDitherIntensity.ValueChanged += (s, e) =>
            {
                lblDitherVal.Text = $"{Lang.T("Интенсивность")}: {trkDitherIntensity.Value}%";
                ReapplyDitherIfActive();
            };
            cmbDitherMode.SelectedIndexChanged += (s, e) => ReapplyDitherIfActive();

            nudWidth.ValueChanged += (s, e) => SyncHeightToWidth();
            nudWidth.Leave += (s, e) => SyncHeightToWidth();
            nudHeight.ValueChanged += (s, e) => SyncWidthToHeight();
            nudHeight.Leave += (s, e) => SyncWidthToHeight();

            btnDetachBefore.Click += (s, e) => ToggleDetach("before", panelBefore, Lang.T("ДО"));
            btnDetachAfter.Click += (s, e) => ToggleDetach("after", panelAfter, Lang.T("ПОСЛЕ"));
            btnDetachEditor.Click += (s, e) => ToggleDetachEditor();
            btnDetachPalette.Click += (s, e) => ToggleDetachPalette();
            btnDetachAnim.Click += (s, e) => ToggleDetachTab("anim", tabPageAnimation, Lang.T("Анимация"));

            btnPickPaletteColor.Click += (s, e) => StartEyedropper();
            picBefore.MouseClick += OnImageMouseClickForEyedropper;
            picAfter.MouseClick += OnImageMouseClickForEyedropper;

            menuFileOpen.Click += menuFileOpen_Click;
            menuFileExit.Click += menuFileExit_Click;
            btnOpen.Click += btnOpen_Click;

            menuEditUndo.Click += (s, e) => PerformUndo();
            menuEditRedo.Click += (s, e) => PerformRedo();
            menuEditReset.Click += (s, e) => ResetSettings();

            miLight.Click  += (s, e) => SetTheme(UI.AppTheme.Light);
            miDark.Click   += (s, e) => SetTheme(UI.AppTheme.Dark);
            miHybrid.Click += (s, e) => SetTheme(UI.AppTheme.Hybrid);
            _themeMenuItems = [miLight, miDark, miHybrid];

            miLangRu.Click += (s, e) => SetLanguage(false);
            miLangEn.Click += (s, e) => SetLanguage(true);

            menuPresets.DropDownOpening += (s, e) => RefreshPresetsMenu();
            menuPresetSave.Click += (s, e) => SavePreset();

            menuPlugins.DropDownOpening += (s, e) => RefreshPluginsMenu();
            menuPluginsFolder.Click += (s, e) => PluginManager.OpenPluginFolder();

            menuExportPng.Click += (s, e) => ExportPng();
            menuExportIndexed.Click += (s, e) => ExportIndexedPng();
            menuExportIco.Click += (s, e) => ExportIco();
            menuExportBatch.Click += (s, e) => OpenBatchForm();

            btnUndo.Click += (s, e) => PerformUndo();
            btnRedo.Click += (s, e) => PerformRedo();

            btnApplyPixelize.Click += (s, e) => ApplyPixelization();

            btnApplyEffect.Click += (s, e) => ApplyEffect();
            trkSphereScale.ValueChanged += (s, e) =>
            {
                lblSphereScale.Text = $"{Lang.T("Масштаб")}: {trkSphereScale.Value}%";
                ReapplyEffectIfActive();
            };
            trkSphereBulge.ValueChanged += (s, e) =>
            {
                lblSphereBulge.Text = $"{Lang.T("Выпуклость")}: {trkSphereBulge.Value}%";
                ReapplyEffectIfActive();
            };
            trkOffsetU.ValueChanged += (s, e) =>
            {
                lblOffsetU.Text = $"{Lang.T("Смещение X")}: {trkOffsetU.Value}%";
                ReapplyEffectIfActive();
            };
            trkOffsetV.ValueChanged += (s, e) =>
            {
                lblOffsetV.Text = $"{Lang.T("Смещение Y")}: {trkOffsetV.Value}%";
                ReapplyEffectIfActive();
            };

            trkAmbient.ValueChanged += (s, e) =>
            {
                lblAmbient.Text = $"{Lang.T("Общий свет")}: {trkAmbient.Value}%";
                ReapplyEffectIfActive();
            };
            trkDiffuse.ValueChanged += (s, e) =>
            {
                lblDiffuse.Text = $"{Lang.T("Рассеянный свет")}: {trkDiffuse.Value}%";
                ReapplyEffectIfActive();
            };
            trkLightAzimuth.ValueChanged += (s, e) =>
            {
                lblLightAzimuth.Text = $"{Lang.T("Горизонталь")}: {trkLightAzimuth.Value}°";
                ReapplyEffectIfActive();
            };
            trkLightElev.ValueChanged += (s, e) =>
            {
                lblLightElev.Text = $"{Lang.T("Высота")}: {trkLightElev.Value}°";
                ReapplyEffectIfActive();
            };
            chkSpecular.CheckedChanged += (s, e) => ReapplyEffectIfActive();

            chkEdges.CheckedChanged += (s, e) => UpdatePicAfter();
            nudEdgeThickness.ValueChanged += (s, e) => { if (chkEdges.Checked) UpdatePicAfter(); };
            cmbEdgeMode.SelectedIndex = 0;
            cmbEdgeMode.SelectedIndexChanged += (s, e) => { if (chkEdges.Checked) UpdatePicAfter(); };
            btnEdgeColor.Click += (s, e) =>
            {
                using var dlg = new ColorDialog { Color = _edgeColor, FullOpen = true };
                if (dlg.ShowDialog() != DialogResult.OK) return;
                _edgeColor = dlg.Color;
                btnEdgeColor.BackColor = _edgeColor;
                btnEdgeColor.ForeColor = _edgeColor.GetBrightness() < 0.5f ? Color.White : Color.Black;
                if (chkEdges.Checked) UpdatePicAfter();
            };

            btnPickBgColor.Click += (s, e) =>
            {
                using var dlg = new ColorDialog { Color = _bgColor, FullOpen = true };
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    _bgColor = dlg.Color;
                    btnPickBgColor.BackColor = _bgColor;
                    btnPickBgColor.ForeColor = _bgColor.GetBrightness() < 0.5f ? Color.White : Color.Black;
                }
            };
            btnRemoveBg.Click += (s, e) => RemoveBackground();

            btnApplyPalette.Click += (s, e) => ApplyPalette();
            btnApplyDither.Click += (s, e) => ApplyDitherOnly();
            btnSavePalette.Click += (s, e) => SavePaletteToFile();
            btnLoadPalette.Click += (s, e) => LoadPaletteFromFile();

            cmbRetroPalette.SelectedIndexChanged += (s, e) =>
            {
                var key = PaletteManager.ComboIndexToKey(cmbRetroPalette.SelectedIndex);
                var pal = key is not null ? PaletteManager.GetRetroPalette(key) : null;
                if (pal is not null)
                {
                    _updatingAspect = true;
                    nudColorCount.Value = pal.Length;
                    _updatingAspect = false;
                }
            };

            _palettePanel.PaletteChanged += OnPaletteChanged;

            _pixelEditor.ImageChanged += (s, bmp) =>
            {
                _syncingEditor = true;

                if (_processedImage is not null)
                    PushHistory();

                if (_pipelineResult is not null &&
                    _pipelineResult.Width == bmp.Width && _pipelineResult.Height == bmp.Height)
                {
                    _editorOverlay?.Dispose();
                    _editorOverlay = ExtractOverlay(bmp, _pipelineResult);
                }

                _processedImage = new Bitmap(bmp);
                _historySettings = BuildCurrentSettings();
                UpdateHistoryBaseline();
                lblAfterTitle.Text = $"{Lang.T("ПОСЛЕ")}  —  {bmp.Width} × {bmp.Height} px";
                UpdatePicAfter();
                UpdateStatus();
                UpdateUndoRedoButtons();

                _syncingEditor = false;
            };
            _pixelEditor.ColorPicked += (s, c) =>
            {
                _pixelEditor.ForeColor2 = c;
                btnEditorColor.ForeColor = c;
            };

            btnToolPencil.Click += EditorToolSelected;
            btnToolLine.Click += EditorToolSelected;
            btnToolRect.Click += EditorToolSelected;
            btnToolFill.Click += EditorToolSelected;
            btnToolEyedrop.Click += EditorToolSelected;
            btnToolEraser.Click += EditorToolSelected;

            btnZoom1.Click += (s, e) => SetEditorZoom(1);
            btnZoom2.Click += (s, e) => SetEditorZoom(2);
            btnZoom4.Click += (s, e) => SetEditorZoom(4);
            btnZoom8.Click += (s, e) => SetEditorZoom(8);
            btnZoom16.Click += (s, e) => SetEditorZoom(16);

            btnEditorColor.Click += (s, e) => PickEditorColor();
            chkEraserTransparent.CheckedChanged += (s, e) =>
                _pixelEditor.EraserTransparent = chkEraserTransparent.Checked;
            btnEditorApply.Click += (s, e) => ApplyEditorToResult();

            _activeToolBtn = btnToolPencil;

            _animTimer.Tick += OnAnimTimerTick;
            btnAnimPlay.Click += (s, e) => AnimPlay();
            btnAnimStop.Click += (s, e) => AnimStop();
            btnProcessFrames.Click += (s, e) => ProcessAllFrames();
            btnExportGif.Click += (s, e) => AnimExportGif();
            nudAnimFps.ValueChanged += (s, e) =>
            {
                _animTimer.Interval = Math.Max(20, 1000 / (int)nudAnimFps.Value);
            };
        }

        private void OnDragEnter(object? sender, DragEventArgs e)
        {
            if (e.Data?.GetDataPresent(DataFormats.FileDrop) == true)
                e.Effect = DragDropEffects.Copy;
        }

        private void OnDragDrop(object? sender, DragEventArgs e)
        {
            var files = e.Data?.GetData(DataFormats.FileDrop) as string[];
            if (files?.Length > 0)
                LoadImageFromFile(files[0]);
        }

        private void OnKeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape && _eyedropperActive) { StopEyedropper(); e.Handled = true; }
            if (e.Control && e.KeyCode == Keys.V) { PasteFromClipboard(); e.Handled = true; }
            if (e.Control && e.KeyCode == Keys.Z) { PerformUndo(); e.Handled = true; }
            if (e.Control && e.KeyCode == Keys.Y) { PerformRedo(); e.Handled = true; }
        }

        private void menuFileOpen_Click(object? sender, EventArgs e) => OpenFileDialog();
        private void menuFileExit_Click(object? sender, EventArgs e) => Application.Exit();
        private void btnOpen_Click(object? sender, EventArgs e) => OpenFileDialog();

        private void OpenFileDialog()
        {
            using var dlg = new OpenFileDialog
            {
                Title = Lang.T("Открыть изображение"),
                Filter = Lang.T("Изображения|*.png;*.jpg;*.jpeg;*.bmp;*.gif|Все файлы|*.*")
            };
            if (dlg.ShowDialog() == DialogResult.OK)
                LoadImageFromFile(dlg.FileName);
        }

        private void LoadImageFromFile(string path)
        {
            try
            {
                ClearAnimation();
                if (string.Equals(Path.GetExtension(path), ".gif", StringComparison.OrdinalIgnoreCase)
                    && AnimationProcessor.IsAnimated(path))
                {
                    var (frames, delays) = AnimationProcessor.ExtractFrames(path);
                    _animFrames = frames;
                    _animDelays = delays;
                    _animCurrentFrame = 0;
                    RebuildFrameStrip();
                    var first = new Bitmap(frames[0]);
                    SetOriginalImage(first, path);
                    lblBeforeTitle.Text = $"{Lang.T("ДО")}  —  GIF  {frames[0].Width}×{frames[0].Height}  ({frames.Count} {Lang.T("кадров")})";
                    lblAnimFrame.Text = $"{Lang.T("Кадр")}: 1 / {frames.Count}";
                    btnAnimPlay.Enabled = true;
                }
                else
                {
                    SetOriginalImage(new Bitmap(path), path);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{Lang.T("Ошибка загрузки")}:\n{ex.Message}", Lang.T("Ошибка"),
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PasteFromClipboard()
        {
            if (Clipboard.ContainsImage())
            {
                var img = Clipboard.GetImage();
                if (img is not null)
                    SetOriginalImage(new Bitmap(img));
            }
            else if (Clipboard.ContainsFileDropList())
            {
                var files = Clipboard.GetFileDropList();
                if (files.Count > 0 && files[0] is not null)
                    LoadImageFromFile(files[0]!);
            }
        }

        private void SetOriginalImage(Bitmap bmp, string? filename = null)
        {
            _originalImage?.Dispose();
            _originalImage = bmp;

            btnDropHint.Visible = false;
            picBefore.Image = _originalImage;
            lblBeforeTitle.Text = $"{Lang.T("ДО")}  —  {bmp.Width} × {bmp.Height} px";
            lblStatusZoom.Text = bmp.Width != bmp.Height
                ? $"{Lang.T("Совет: для иконок лучше формат 1:1")} ({Lang.T("у вас")} {bmp.Width}×{bmp.Height})"
                : "";
            this.Text = filename is not null
                ? $"KebuzForge — {Path.GetFileName(filename)}"
                : "KebuzForge";

            _aspectRatio = (double)bmp.Height / bmp.Width;
            var (dw, dh) = ImageProcessor.DefaultPixelSize(bmp.Width, bmp.Height, maxSide: 64);
            _updatingAspect = true;
            nudWidth.Value = dw;
            nudHeight.Value = dh;
            _updatingAspect = false;

            _history.Clear();
            _pixelizedImage?.Dispose();
            _pixelizedImage = null;
            _currentPalette = [];
            _paletteIndices = null;
            _historySource?.Dispose();
            _historySource = null;
            _historyIndices = null;
            _historyPalette = [];
            _historySettings = BuildCurrentSettings();
            ClearResult();
            UpdateStatus();
        }

        private void ApplyPixelization()
        {
            if (_originalImage is null)
            {
                MessageBox.Show(Lang.T("Сначала загрузите изображение."), "KebuzForge",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                Cursor = Cursors.WaitCursor;
                string algo = cmbPixelAlgorithm.SelectedIndex == 1 ? "Center" : "Average";
                var pixelized = ImageProcessor.Pixelize(
                    _originalImage, (int)nudWidth.Value, (int)nudHeight.Value, algo);

                _pixelizedImage?.Dispose();
                _pixelizedImage = pixelized;
                _paletteIndices = null;
                SetProcessedImage(new Bitmap(pixelized));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{Lang.T("Ошибка пикселизации")}:\n{ex.Message}", Lang.T("Ошибка"),
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void ClearResult()
        {
            _displayBitmap?.Dispose();
            _displayBitmap = null;
            _processedImage?.Dispose();
            _processedImage = null;
            _pipelineResult?.Dispose();
            _pipelineResult = null;
            _editorOverlay?.Dispose();
            _editorOverlay = null;
            _effectResult?.Dispose();
            _effectResult = null;
            _effectActive = false;
            _flatActive = false;
            _ditherActive = false;
            picAfter.Image = null;
            lblAfterTitle.Text = Lang.T("ПОСЛЕ");
        }

        internal void SetProcessedImage(Bitmap bmp)
        {

            if (!_sliderDragging)
            {
                PushHistory();
            }
            else if (!_sliderHistoryPushed)
            {
                PushHistory();
                _sliderHistoryPushed = true;
            }

            _pipelineResult?.Dispose();
            _pipelineResult = bmp;

            Bitmap display;
            if (_editorOverlay is not null &&
                _editorOverlay.Width == bmp.Width && _editorOverlay.Height == bmp.Height)
            {
                display = CompositeOverlay(bmp, _editorOverlay);
            }
            else
            {
                _editorOverlay?.Dispose();
                _editorOverlay = null;
                display = new Bitmap(bmp);
            }

            _processedImage = display;
            _historySettings = BuildCurrentSettings();
            UpdateHistoryBaseline();
            lblAfterTitle.Text = $"{Lang.T("ПОСЛЕ")}  —  {bmp.Width} × {bmp.Height} px";
            UpdatePicAfter();
            SyncEditorImage();
            UpdateStatus();
            UpdateUndoRedoButtons();
        }

        private void PushHistory() =>
            _history.Push(_processedImage, _historySettings, _historySource, _historyIndices, _historyPalette);

        private void UpdateHistoryBaseline()
        {
            _historySource?.Dispose();
            _historySource = _pixelizedImage is null ? null : new Bitmap(_pixelizedImage);
            _historyIndices = _paletteIndices?.ToArray();
            _historyPalette = (Color[])_currentPalette.Clone();
        }

        private static Bitmap CompositeOverlay(Bitmap pipeline, Bitmap overlay)
        {
            var result = new Bitmap(pipeline.Width, pipeline.Height, PixelFormat.Format32bppArgb);
            using var g = Graphics.FromImage(result);
            g.DrawImageUnscaled(pipeline, 0, 0);
            g.DrawImageUnscaled(overlay, 0, 0);
            return result;
        }

        private static Bitmap ExtractOverlay(Bitmap composite, Bitmap pipeline)
        {
            int w = composite.Width, h = composite.Height;
            byte[] comp = ImageProcessor.LockCopy(composite, out int cs);
            byte[] pipe = ImageProcessor.LockCopy(pipeline, out int ps);

            var overlay = new Bitmap(w, h, PixelFormat.Format32bppArgb);
            var dstData = overlay.LockBits(new Rectangle(0, 0, w, h),
                               ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            var dst = new byte[h * dstData.Stride];
            int ds = dstData.Stride;

            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                {
                    int ci = y * cs + x * 4;
                    int pi = y * ps + x * 4;
                    int di = y * ds + x * 4;

                    if (comp[ci] != pipe[pi] || comp[ci + 1] != pipe[pi + 1] ||
                        comp[ci + 2] != pipe[pi + 2] || comp[ci + 3] != pipe[pi + 3])
                    {
                        dst[di] = comp[ci];
                        dst[di + 1] = comp[ci + 1];
                        dst[di + 2] = comp[ci + 2];
                        dst[di + 3] = comp[ci + 3];
                    }

                }

            Marshal.Copy(dst, 0, dstData.Scan0, dst.Length);
            overlay.UnlockBits(dstData);
            return overlay;
        }

        private void ToggleDetach(string key, Control panel, string title)
        {
            if (_detachedForms.TryGetValue(key, out var existing))
            {
                existing.Close();
                return;
            }
            var form = CreateDetachForm(title);
            panel.Parent?.Controls.Remove(panel);
            panel.Dock = DockStyle.Fill;
            form.Controls.Add(panel);
            _detachedForms[key] = form;
            RefreshPreviewLayout();
            form.FormClosed += (s, e) =>
            {
                _detachedForms.Remove(key);
                try { panel.Parent?.Controls.Remove(panel); } catch { }
                panel.Dock = DockStyle.Fill;
                RefreshPreviewLayout();
            };
            form.Show(this);
        }

        private void ToggleDetachTab(string key, TabPage tab, string title)
        {
            if (_detachedForms.TryGetValue(key, out var existing))
            {
                existing.Close();
                return;
            }
            var form = CreateDetachForm(title);

            var host = new Panel { Dock = DockStyle.Fill, BackColor = tab.BackColor };
            foreach (Control c in tab.Controls.Cast<Control>().ToList())
                host.Controls.Add(c);
            form.Controls.Add(host);

            tabBottom.TabPages.Remove(tab);
            _detachedForms[key] = form;

            if (tabBottom.TabCount == 0)
                BeginInvoke(new Action(() =>
                {
                    try { rightSplit.Panel2Collapsed = true; } catch { }
                }));

            form.FormClosed += (s, e) =>
            {
                _detachedForms.Remove(key);
                foreach (Control c in host.Controls.Cast<Control>().ToList())
                    tab.Controls.Add(c);
                tabBottom.TabPages.Add(tab);
                if (rightSplit.Panel2Collapsed)
                    rightSplit.Panel2Collapsed = false;
            };
            form.Show(this);
        }

        private void ToggleDetachEditor()
        {
            if (_detachedForms.TryGetValue("editor", out var ex)) { ex.Close(); return; }
            var form = CreateDetachForm(Lang.T("Редактор пикселей"));
            var host = new Panel { Dock = DockStyle.Fill, BackColor = tabPageEditor.BackColor };
            foreach (Control c in tabPageEditor.Controls.Cast<Control>().ToList())
                host.Controls.Add(c);
            form.Controls.Add(host);
            tabBottom.TabPages.Remove(tabPageEditor);
            _detachedForms["editor"] = form;
            if (tabBottom.TabCount == 0)
                BeginInvoke(new Action(() => { try { rightSplit.Panel2Collapsed = true; } catch { } }));
            form.Shown += (s, e) => _pixelEditor.FitZoomToParent();
            form.SizeChanged += (s, e) => _pixelEditor.FitZoomToParent();
            form.FormClosed += (s, e) =>
            {
                _detachedForms.Remove("editor");
                foreach (Control c in host.Controls.Cast<Control>().ToList())
                    tabPageEditor.Controls.Add(c);
                tabBottom.TabPages.Add(tabPageEditor);
                if (rightSplit.Panel2Collapsed) rightSplit.Panel2Collapsed = false;
            };
            form.Show(this);
        }

        private void ToggleDetachPalette()
        {
            if (_detachedForms.TryGetValue("palette", out var ex)) { ex.Close(); return; }
            var form = CreateDetachForm(Lang.T("Палитра"));
            var host = new Panel { Dock = DockStyle.Fill, BackColor = tabPagePalette.BackColor, AutoScroll = true };
            foreach (Control c in tabPagePalette.Controls.Cast<Control>().ToList())
                host.Controls.Add(c);

            _palettePanel.Dock = DockStyle.Fill;
            _palettePanel.BringToFront();
            form.Controls.Add(host);
            tabBottom.TabPages.Remove(tabPagePalette);
            _detachedForms["palette"] = form;
            if (tabBottom.TabCount == 0)
                BeginInvoke(new Action(() => { try { rightSplit.Panel2Collapsed = true; } catch { } }));
            form.FormClosed += (s, e) =>
            {
                _detachedForms.Remove("palette");
                _palettePanel.Dock = DockStyle.Top;
                foreach (Control c in host.Controls.Cast<Control>().ToList())
                    tabPagePalette.Controls.Add(c);
                panelPaletteBar.BringToFront();
                tabBottom.TabPages.Add(tabPagePalette);
                if (rightSplit.Panel2Collapsed) rightSplit.Panel2Collapsed = false;
            };
            form.Show(this);
        }

        private Form CreateDetachForm(string title)
        {
            return new Form
            {
                Text = title,
                Size = new Size(960, 720),
                MinimumSize = new Size(480, 360),
                StartPosition = FormStartPosition.CenterParent,
                Icon = this.Icon
            };
        }

        private void RefreshPreviewLayout()
        {
            bool beforeOut = _detachedForms.ContainsKey("before");
            bool afterOut = _detachedForms.ContainsKey("after");

            rightSplit.Panel1.SuspendLayout();
            rightSplit.Panel1.Controls.Clear();

            if (!beforeOut && !afterOut)
            {
                previewSplit.Panel1.Controls.Clear();
                previewSplit.Panel2.Controls.Clear();
                panelBefore.Dock = DockStyle.Fill;
                panelAfter.Dock = DockStyle.Fill;
                previewSplit.Panel1.Controls.Add(panelBefore);
                previewSplit.Panel2.Controls.Add(panelAfter);
                previewSplit.Dock = DockStyle.Fill;
                rightSplit.Panel1.Controls.Add(previewSplit);
                if (rightSplit.Orientation != Orientation.Horizontal)
                    rightSplit.Orientation = Orientation.Horizontal;
            }
            else if (!beforeOut)
            {
                panelBefore.Dock = DockStyle.Fill;
                rightSplit.Panel1.Controls.Add(panelBefore);
                if (rightSplit.Orientation != Orientation.Vertical)
                    rightSplit.Orientation = Orientation.Vertical;
            }
            else if (!afterOut)
            {
                panelAfter.Dock = DockStyle.Fill;
                rightSplit.Panel1.Controls.Add(panelAfter);
                if (rightSplit.Orientation != Orientation.Vertical)
                    rightSplit.Orientation = Orientation.Vertical;
            }
            else
            {
                if (rightSplit.Orientation != Orientation.Vertical)
                    rightSplit.Orientation = Orientation.Vertical;
            }

            rightSplit.Panel1.ResumeLayout(true);

            BeginInvoke(new Action(() => ApplySplitterDistances(beforeOut, afterOut)));
        }

        private void ApplySplitterDistances(bool beforeOut, bool afterOut)
        {
            try
            {
                if (!beforeOut && !afterOut)
                {
                    if (rightSplit.Height > rightSplit.Panel1MinSize + rightSplit.Panel2MinSize)
                        rightSplit.SplitterDistance = Math.Clamp(
                            420, rightSplit.Panel1MinSize,
                            rightSplit.Height - rightSplit.Panel2MinSize);
                    if (previewSplit.Width > previewSplit.Panel1MinSize + previewSplit.Panel2MinSize)
                        previewSplit.SplitterDistance = previewSplit.Width / 2;
                }
                else if (beforeOut && afterOut)
                {
                    if (rightSplit.Width > rightSplit.Panel1MinSize)
                        rightSplit.SplitterDistance = rightSplit.Panel1MinSize;
                }
                else
                {
                    if (rightSplit.Width > rightSplit.Panel1MinSize + rightSplit.Panel2MinSize)
                        rightSplit.SplitterDistance = Math.Clamp(
                            rightSplit.Width * 3 / 5,
                            rightSplit.Panel1MinSize,
                            rightSplit.Width - rightSplit.Panel2MinSize);
                }
                rightSplit.Refresh();
            }
            catch { }
        }

        private void StartEyedropper()
        {
            _eyedropperOrigBack = btnPickPaletteColor.BackColor;
            _eyedropperOrigFore = btnPickPaletteColor.ForeColor;
            _eyedropperActive   = true;
            picAfter.Cursor     = Cursors.Cross;
            btnPickPaletteColor.Text      = Lang.T("ESC — отмена");
            btnPickPaletteColor.BackColor = Color.FromArgb(80, 40, 40);
            btnPickPaletteColor.ForeColor = Color.White;
        }

        private void StopEyedropper()
        {
            _eyedropperActive = false;
            picAfter.Cursor   = Cursors.Default;
            btnPickPaletteColor.Text      = Lang.T("Пипетка");
            btnPickPaletteColor.BackColor = _eyedropperOrigBack;
            btnPickPaletteColor.ForeColor = _eyedropperOrigFore;
        }

        private void OnImageMouseClickForEyedropper(object? sender, MouseEventArgs e)
        {
            if (!_eyedropperActive || sender != picAfter) return;
            var pb = picAfter;
            var bmp = pb.Image as Bitmap;
            if (bmp is null) { StopEyedropper(); return; }

            float scale = Math.Min((float)pb.Width / bmp.Width, (float)pb.Height / bmp.Height);
            int x0 = (pb.Width - (int)(bmp.Width * scale)) / 2;
            int y0 = (pb.Height - (int)(bmp.Height * scale)) / 2;
            int ix = (int)((e.X - x0) / scale);
            int iy = (int)((e.Y - y0) / scale);

            StopEyedropper();

            if (ix < 0 || ix >= bmp.Width || iy < 0 || iy >= bmp.Height) return;

            var color = bmp.GetPixel(ix, iy);
            int nearest = FindNearestPaletteColor(color);
            if (nearest >= 0)
            {
                _palettePanel.SetHighlight(nearest);
                if (!_detachedForms.ContainsKey("palette"))
                    tabBottom.SelectedTab = tabPagePalette;
            }
        }

        private int FindNearestPaletteColor(Color c)
        {
            if (_currentPalette.Length == 0) return -1;
            int best = 0, bestDist = int.MaxValue;
            for (int i = 0; i < _currentPalette.Length; i++)
            {
                int dr = c.R - _currentPalette[i].R;
                int dg = c.G - _currentPalette[i].G;
                int db = c.B - _currentPalette[i].B;
                int dist = dr * dr + dg * dg + db * db;
                if (dist < bestDist) { bestDist = dist; best = i; }
            }
            return best;
        }

        private void SyncHeightToWidth()
        {
            if (!chkKeepAspect.Checked || _updatingAspect) return;
            _updatingAspect = true;
            try { nudHeight.Value = Math.Max(1, (int)Math.Round((double)nudWidth.Value * _aspectRatio)); }
            finally { _updatingAspect = false; }
        }

        private void SyncWidthToHeight()
        {
            if (!chkKeepAspect.Checked || _updatingAspect || _aspectRatio == 0) return;
            _updatingAspect = true;
            try { nudWidth.Value = Math.Max(1, (int)Math.Round((double)nudHeight.Value / _aspectRatio)); }
            finally { _updatingAspect = false; }
        }

        private void UpdateStatus()
        {
            lblStatusOriginal.Text = _originalImage is not null
                ? $"{Lang.T("Оригинал")}: {_originalImage.Width} × {_originalImage.Height} px"
                : Lang.T("Оригинал: —");

            lblStatusResult.Text = _processedImage is not null
                ? $"{Lang.T("Результат")}: {_processedImage.Width} × {_processedImage.Height} px"
                : Lang.T("Результат: —");
        }

        private void UpdatePicAfter()
        {
            _displayBitmap?.Dispose();
            _displayBitmap = null;

            if (_processedImage is null) { picAfter.Image = null; return; }

            if (chkEdges.Checked)
            {
                _displayBitmap = EdgeProcessor.Apply(_processedImage, (int)nudEdgeThickness.Value,
                    _edgeColor, cmbEdgeMode.SelectedIndex == 0);
                picAfter.Image = _displayBitmap;
            }
            else
            {
                picAfter.Image = _processedImage;
            }
        }

        private Bitmap? EnsurePixelizedSource()
        {
            if (_pixelizedImage is null && _originalImage is not null)
                _pixelizedImage = new Bitmap(_originalImage);
            return _pixelizedImage;
        }

        private void ApplyPalette()
        {
            var source = EnsurePixelizedSource();
            if (source is null)
            {
                MessageBox.Show(Lang.T("Сначала загрузите изображение."), "KebuzForge",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                Cursor = Cursors.WaitCursor;

                var key = PaletteManager.ComboIndexToKey(cmbRetroPalette.SelectedIndex);
                var palette = key is not null
                    ? PaletteManager.GetRetroPalette(key)!
                    : PaletteManager.MedianCut(source, (int)nudColorCount.Value);

                _currentPalette = palette;
                _palettePanel.Palette = palette;
                tabBottom.SelectedTab = tabPagePalette;

                var result = PaletteManager.ApplyPaletteIndexed(source, palette, out var indices);
                _paletteIndices = indices;
                _paletteIndexSize = source.Size;
                SetProcessedImage(result);

                lblStatusZoom.Text = $"{Lang.T("Палитра")}: {palette.Length} {Lang.T("цветов")}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{Lang.T("Ошибка квантования")}:\n{ex.Message}", Lang.T("Ошибка"),
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void OnPaletteChanged(object? sender, PaletteChangedEventArgs e)
        {
            if (e.Index == -1)
            {
                lblStatusZoom.Text = $"#{e.Color.R:X2}{e.Color.G:X2}{e.Color.B:X2}";
                return;
            }
            _palettePanel.ClearHighlight();

            var source = EnsurePixelizedSource();
            if (source is null) return;
            try
            {
                Cursor = Cursors.WaitCursor;
                if (_paletteIndices is not null && _paletteIndexSize == source.Size)
                {
                    PaletteManager.RecolorIndexed(source, _paletteIndices, e.Index, e.Color);
                    if (_effectActive || _flatActive)
                        ApplyEffect();
                    else if (_ditherActive)
                        ApplyDitherOnly();
                    else
                        SetProcessedImage(PaletteManager.FromIndices(_paletteIndices, source, _currentPalette));
                }
                else
                {
                    SetProcessedImage(PaletteManager.ApplyPalette(source, _currentPalette));
                }
            }
            finally { Cursor = Cursors.Default; }
        }

        private void SavePaletteToFile()
        {
            if (_currentPalette.Length == 0)
            {
                MessageBox.Show(Lang.T("Палитра пуста. Сначала примените палитру."), "KebuzForge",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            using var dlg = new SaveFileDialog
            {
                Title = Lang.T("Сохранить палитру"),
                Filter = "JASC PAL|*.pal|Photoshop ACT|*.act|GIMP GPL|*.gpl"
            };
            if (dlg.ShowDialog() == DialogResult.OK)
                PaletteManager.SavePalette(_currentPalette, dlg.FileName);
        }

        private void PerformUndo()
        {
            if (!_history.CanUndo) return;
            var snap = _history.Undo(_processedImage, _historySettings, _historySource, _historyIndices, _historyPalette);
            if (snap is null) return;
            RestoreSnapshot(snap);
        }

        private void PerformRedo()
        {
            if (!_history.CanRedo) return;
            var snap = _history.Redo(_processedImage, _historySettings, _historySource, _historyIndices, _historyPalette);
            if (snap is null) return;
            RestoreSnapshot(snap);
        }

        private void RestoreSnapshot(HistorySnapshot snap)
        {
            _editorOverlay?.Dispose();
            _editorOverlay = null;

            picAfter.Image = null;
            _processedImage?.Dispose();
            _processedImage = snap.Image;
            _historySettings = snap.Settings;

            _pixelizedImage?.Dispose();
            _pixelizedImage = snap.Source;
            _paletteIndices = snap.PaletteIndices;
            _paletteIndexSize = snap.Source?.Size ?? Size.Empty;
            _currentPalette = snap.Palette;
            _palettePanel.Palette = _currentPalette;

            UpdateHistoryBaseline();
            ApplySettings(snap.Settings);
            lblAfterTitle.Text = _processedImage is not null
                ? $"{Lang.T("ПОСЛЕ")}  —  {_processedImage.Width} × {_processedImage.Height} px"
                : Lang.T("ПОСЛЕ");
            UpdatePicAfter();
            SyncEditorImage();
            UpdateStatus();
            UpdateUndoRedoButtons();
        }

        private void UpdateUndoRedoButtons()
        {
            btnUndo.Enabled = _history.CanUndo;
            btnRedo.Enabled = _history.CanRedo;
        }

        private void ApplyDitherOnly()
        {
            var source = ((_effectActive || _flatActive) ? _effectResult : null)
                         ?? _pixelizedImage
                         ?? _originalImage;
            if (source is null)
            {
                MessageBox.Show(Lang.T("Сначала загрузите изображение."), "KebuzForge",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (_currentPalette.Length == 0)
            {
                MessageBox.Show(Lang.T("Сначала примените палитру."), "KebuzForge",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            try
            {
                Cursor = Cursors.WaitCursor;
                var result = DitheringEngine.Apply(source, _currentPalette, GetDitherMode(), GetDitherIntensity());
                _ditherActive = true;
                SetProcessedImage(result);
            }
            finally { Cursor = Cursors.Default; }
        }

        private string GetDitherMode() => cmbDitherMode.SelectedIndex switch
        {
            1 => "FloydSteinberg",
            2 => "Bayer4",
            3 => "Bayer8",
            _ => "None"
        };

        private float GetDitherIntensity() => trkDitherIntensity.Value / 100f;

        private void LoadPaletteFromFile()
        {
            using var dlg = new OpenFileDialog
            {
                Title = Lang.T("Загрузить палитру"),
                Filter = Lang.T("Палитры|*.pal;*.act;*.gpl|Все файлы|*.*")
            };
            if (dlg.ShowDialog() != DialogResult.OK) return;

            try
            {
                _currentPalette = PaletteManager.LoadPalette(dlg.FileName);
                nudColorCount.Value = Math.Clamp(_currentPalette.Length, 2, 256);
                _palettePanel.Palette = _currentPalette;

                var source = EnsurePixelizedSource();
                if (source is not null)
                {
                    var result = PaletteManager.ApplyPaletteIndexed(source, _currentPalette, out var indices);
                    _paletteIndices = indices;
                    _paletteIndexSize = source.Size;
                    SetProcessedImage(result);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{Lang.T("Ошибка загрузки палитры")}:\n{ex.Message}", Lang.T("Ошибка"),
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ReapplyEffectIfActive()
        {
            if (!_loadingSettings && (_effectActive || _flatActive))
                ApplyEffect();
        }

        private void ReapplyDitherIfActive()
        {
            if (_ditherActive && !_loadingSettings)
                ApplyDitherOnly();
        }

        private void ApplyEffect()
        {
            var source = _pixelizedImage ?? _originalImage;
            if (source is null)
            {
                MessageBox.Show(Lang.T("Сначала загрузите изображение."), "KebuzForge",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (rbEffectNone.Checked)
            {
                float scale   = trkSphereScale.Value / 100f;
                float offsetU = trkOffsetU.Value / 100f;
                float offsetV = trkOffsetV.Value / 100f;

                _effectActive = false;
                _effectResult?.Dispose();

                Bitmap flatResult = (scale != 1f || offsetU != 0f || offsetV != 0f)
                    ? EffectsProcessor.FlatTransform(source, scale, offsetU, offsetV)
                    : new Bitmap(source);

                _effectResult = flatResult;
                _flatActive = true;

                Bitmap final = (_ditherActive && _currentPalette.Length > 0)
                    ? DitheringEngine.Apply(flatResult, _currentPalette, GetDitherMode(), GetDitherIntensity())
                    : new Bitmap(flatResult);

                SetProcessedImage(final);
                return;
            }

            _flatActive = false;

            try
            {
                Cursor = Cursors.WaitCursor;

                float ambient = trkAmbient.Value / 100f;
                float diffuse = trkDiffuse.Value / 100f;
                float azimuth = trkLightAzimuth.Value;
                float elevation = trkLightElev.Value;
                float scale = trkSphereScale.Value / 100f;
                float bulge = trkSphereBulge.Value / 100f;
                float offsetU = trkOffsetU.Value / 100f;
                float offsetV = trkOffsetV.Value / 100f;

                Bitmap effResult;
                if (rbEffectSphere.Checked)
                {
                    effResult = EffectsProcessor.Spherize(source, scale, bulge, offsetU, offsetV,
                        ambient, diffuse, azimuth, elevation, chkSpecular.Checked);
                }
                else if (rbEffectRounded.Checked)
                {
                    float sharpness = trkCornerSharp.Value;
                    bool fromRect   = chkShapeFromRect.Checked;
                    effResult = EffectsProcessor.RoundedRect(source, scale, bulge, sharpness, fromRect,
                        offsetU, offsetV, ambient, diffuse, azimuth, elevation, chkSpecular.Checked);
                }
                else
                {
                    bool horizontal = cmbCylinderDir.SelectedIndex == 0;
                    effResult = EffectsProcessor.Cylinderize(source, horizontal, scale, bulge, offsetU, offsetV,
                        ambient, diffuse, azimuth, elevation);
                }

                _effectResult?.Dispose();
                _effectResult = effResult;
                _effectActive = true;

                Bitmap final = (_ditherActive && _currentPalette.Length > 0)
                    ? DitheringEngine.Apply(effResult, _currentPalette, GetDitherMode(), GetDitherIntensity())
                    : new Bitmap(effResult);

                SetProcessedImage(final);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{Lang.T("Ошибка эффекта")}:\n{ex.Message}", Lang.T("Ошибка"),
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void EditorToolSelected(object? sender, EventArgs e)
        {
            if (sender is not ToolStripButton btn) return;

            if (_activeToolBtn is not null && _activeToolBtn != btn)
                _activeToolBtn.Checked = false;
            _activeToolBtn = btn;
            btn.Checked = true;

            _pixelEditor.Tool = btn == btnToolPencil ? UI.EditorTool.Pencil
                              : btn == btnToolLine ? UI.EditorTool.Line
                              : btn == btnToolRect ? UI.EditorTool.Rectangle
                              : btn == btnToolFill ? UI.EditorTool.FloodFill
                              : btn == btnToolEyedrop ? UI.EditorTool.Eyedropper
                              : UI.EditorTool.Eraser;
        }

        private void SetEditorZoom(int zoom)
        {
            _pixelEditor.Zoom = zoom;

            editorScroll.AutoScrollPosition = new Point(
                Math.Max(0, (_pixelEditor.Width - editorScroll.ClientSize.Width) / 2),
                Math.Max(0, (_pixelEditor.Height - editorScroll.ClientSize.Height) / 2));
        }

        private void ApplyEditorToResult()
        {
            if (_editorOverlay is null) return;

            var source = _pixelizedImage ?? _originalImage;
            if (source is null) return;

            if (_effectActive)
                BakeOverlayThroughEffect(source);
            else
                BakeOverlayFlat(source);

            _editorOverlay?.Dispose();
            _editorOverlay = null;

            if (_effectActive)
                ApplyEffect();
            else if (_ditherActive)
                ApplyDitherOnly();
            else
                SetProcessedImage(new Bitmap(source));
        }

        private void BakeOverlayThroughEffect(Bitmap source)
        {
            if (_editorOverlay is null) return;

            float scale = trkSphereScale.Value / 100f;
            float bulge = trkSphereBulge.Value / 100f;
            float offsetU = trkOffsetU.Value / 100f;
            float offsetV = trkOffsetV.Value / 100f;
            float sharpness = trkCornerSharp.Value;
            bool fromRect   = chkShapeFromRect.Checked;
            bool horiz = cmbCylinderDir.SelectedIndex == 0;
            bool isSphere = rbEffectSphere.Checked;
            bool isRounded = rbEffectRounded.Checked;

            int w = source.Width, h = source.Height;
            byte[] ovl = ImageProcessor.LockCopy(_editorOverlay, out int os);

            for (int py = 0; py < _editorOverlay.Height; py++)
                for (int px = 0; px < _editorOverlay.Width; px++)
                {
                    int oi = py * os + px * 4;
                    if (ovl[oi + 3] == 0) continue;

                    int sx, sy;
                    bool hit = isSphere
                        ? EffectsProcessor.TrySphereUV(px, py, w, h, scale, bulge, offsetU, offsetV, out sx, out sy)
                        : isRounded
                            ? EffectsProcessor.TryRoundedRectUV(px, py, w, h, scale, bulge, sharpness, fromRect, offsetU, offsetV, out sx, out sy)
                            : EffectsProcessor.TryCylinderUV(px, py, w, h, horiz, scale, bulge, offsetU, offsetV, out sx, out sy);

                    if (!hit) continue;

                    source.SetPixel(sx, sy, Color.FromArgb(ovl[oi + 3], ovl[oi + 2], ovl[oi + 1], ovl[oi]));
                }
        }

        private void BakeOverlayFlat(Bitmap source)
        {
            if (_editorOverlay is null) return;
            using var g = Graphics.FromImage(source);
            g.DrawImageUnscaled(_editorOverlay, 0, 0);
        }

        private void RemoveBackground()
        {
            var source = _pixelizedImage ?? _originalImage;
            if (source is null)
            {
                MessageBox.Show(Lang.T("Сначала загрузите изображение."), "KebuzForge",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                Cursor = Cursors.WaitCursor;
                int tol = (int)nudBgTolerance.Value;

                var target = new Bitmap(source);
                BackgroundRemover.RemoveFromPoint(target, new Point(0, 0),
                    target.GetPixel(0, 0), tol);

                _pixelizedImage?.Dispose();
                _pixelizedImage = target;
                _paletteIndices = null;

                if (_effectActive) ApplyEffect();
                else if (_ditherActive) ApplyDitherOnly();
                else SetProcessedImage(new Bitmap(target));
            }
            finally { Cursor = Cursors.Default; }
        }

        private Bitmap? GetExportBitmap() => _displayBitmap ?? _processedImage;

        private void ExportPng()
        {
            var bmp = GetExportBitmap();
            if (bmp is null) { NoResultMessage(); return; }

            using var dlg = new SaveFileDialog
            {
                Title = Lang.T("Сохранить PNG"),
                Filter = "PNG|*.png",
                FileName = "result"
            };
            if (dlg.ShowDialog() == DialogResult.OK)
                ExportManager.SavePng(bmp, dlg.FileName);
        }

        private void ExportIndexedPng()
        {
            var bmp = GetExportBitmap();
            if (bmp is null) { NoResultMessage(); return; }
            if (_currentPalette.Length == 0)
            {
                MessageBox.Show(Lang.T("Сначала примените палитру."), "KebuzForge",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using var dlg = new SaveFileDialog
            {
                Title = Lang.T("Сохранить Indexed PNG"),
                Filter = "PNG|*.png",
                FileName = "result_indexed"
            };
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                try { ExportManager.SaveIndexedPng(bmp, _currentPalette, dlg.FileName); }
                catch (Exception ex)
                {
                    MessageBox.Show($"{Lang.T("Ошибка экспорта")}:\n{ex.Message}", Lang.T("Ошибка"),
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ExportIco()
        {
            var bmp = GetExportBitmap();
            if (bmp is null) { NoResultMessage(); return; }

            using var dlg = new SaveFileDialog
            {
                Title = Lang.T("Сохранить ICO"),
                Filter = "ICO|*.ico",
                FileName = "icon"
            };
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                try { ExportManager.SaveIco(bmp, dlg.FileName); }
                catch (Exception ex)
                {
                    MessageBox.Show($"{Lang.T("Ошибка экспорта")}:\n{ex.Message}", Lang.T("Ошибка"),
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void NoResultMessage() =>
            MessageBox.Show(Lang.T("Нет результата для экспорта."), "KebuzForge",
                MessageBoxButtons.OK, MessageBoxIcon.Information);

        private void PickEditorColor()
        {
            Color? picked = chkEditorPaletteOnly.Checked && _currentPalette.Length > 0
                ? ShowPaletteColorPicker(_currentPalette)
                : ShowFreeColorPicker();

            if (picked is not null)
            {
                _pixelEditor.ForeColor2 = picked.Value;
                btnEditorColor.ForeColor = picked.Value;
            }
        }

        private Color? ShowFreeColorPicker()
        {
            using var dlg = new ColorDialog { Color = _pixelEditor.ForeColor2, FullOpen = true };
            return dlg.ShowDialog() == DialogResult.OK ? dlg.Color : null;
        }

        private Color? ShowPaletteColorPicker(Color[] palette)
        {
            const int sz = 24, pad = 3, cols = 8;
            int rows = (palette.Length + cols - 1) / cols;
            int pw = cols * (sz + pad) + pad;
            int ph = rows * (sz + pad) + pad;

            Color? picked = null;

            using var popup = new Form
            {
                FormBorderStyle = FormBorderStyle.FixedSingle,
                StartPosition = FormStartPosition.Manual,
                ClientSize = new Size(pw + 2, ph + 2),
                Text = Lang.T("Цвет из палитры"),
                ShowInTaskbar = false,
                MinimizeBox = false,
                MaximizeBox = false,
                TopMost = true
            };

            var pt = editorToolStrip.PointToScreen(
                new Point(btnEditorColor.Bounds.Left, editorToolStrip.Height));
            popup.Location = pt;

            var canvas = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(45, 45, 45) };
            canvas.Paint += (s, e) =>
            {
                var g = e.Graphics;
                for (int i = 0; i < palette.Length; i++)
                {
                    int row = i / cols, col = i % cols;
                    int x = pad + col * (sz + pad);
                    int y = pad + row * (sz + pad);
                    using var b = new SolidBrush(palette[i]);
                    g.FillRectangle(b, x, y, sz, sz);
                    bool active = palette[i].ToArgb() == _pixelEditor.ForeColor2.ToArgb();
                    g.DrawRectangle(active ? Pens.White : Pens.Black, x, y, sz - 1, sz - 1);
                    if (active)
                        g.DrawRectangle(Pens.White, x - 1, y - 1, sz + 1, sz + 1);
                }
            };
            canvas.MouseClick += (s, e) =>
            {
                int col = (e.X - pad) / (sz + pad);
                int row = (e.Y - pad) / (sz + pad);
                int idx = row * cols + col;
                if (idx >= 0 && idx < palette.Length)
                {
                    picked = palette[idx];
                    popup.Close();
                }
            };

            popup.Controls.Add(canvas);
            popup.Deactivate += (s, e) => popup.Close();
            popup.ShowDialog(this);
            return picked;
        }

        private static string SettingsPath => AppPaths.SettingsFile;

        private AppSettings BuildCurrentSettings() => new AppSettings
        {
            PixelWidth = (int)nudWidth.Value,
            PixelHeight = (int)nudHeight.Value,
            KeepAspect = chkKeepAspect.Checked,
            PixelAlgorithm = cmbPixelAlgorithm.SelectedIndex,
            ColorCount = (int)nudColorCount.Value,
            RetroPaletteIndex = cmbRetroPalette.SelectedIndex,
            DitherMode = cmbDitherMode.SelectedIndex,
            DitherIntensity = trkDitherIntensity.Value,
            EffectIndex = rbEffectSphere.Checked ? 1 : rbEffectCylinder.Checked ? 2 : rbEffectRounded.Checked ? 3 : 0,
            CylinderDir = cmbCylinderDir.SelectedIndex,
            SphereScale = trkSphereScale.Value,
            SphereBulge = trkSphereBulge.Value,
            OffsetU = trkOffsetU.Value,
            OffsetV = trkOffsetV.Value,
            CornerSharpness = trkCornerSharp.Value,
            ShapeFromRect   = chkShapeFromRect.Checked,
            Ambient = trkAmbient.Value,
            Diffuse = trkDiffuse.Value,
            LightAzimuth = trkLightAzimuth.Value,
            LightElev = trkLightElev.Value,
            Specular = chkSpecular.Checked,
            ShowEdges = chkEdges.Checked,
            EdgeThickness = (int)nudEdgeThickness.Value,
            EdgeMode = cmbEdgeMode.SelectedIndex,
            EdgeColor = _edgeColor.ToArgb(),
            BgTolerance = (int)nudBgTolerance.Value,
            ThemeIndex  = (int)_currentTheme,
            LanguageIndex = Lang.English ? 1 : 0,
        };

        private void SaveSettings()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(SettingsPath)!);
                File.WriteAllText(SettingsPath,
                    JsonSerializer.Serialize(BuildCurrentSettings(),
                        new JsonSerializerOptions { WriteIndented = true }));
            }
            catch {   }
        }

        private void LoadSettings()
        {
            if (!File.Exists(SettingsPath))
            {
                SetTheme(UI.AppTheme.Light);
                SetLanguage(false);
                return;
            }
            try
            {
                var s = JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(SettingsPath));
                if (s is not null)
                {
                    ApplySettings(s);
                    SetTheme((UI.AppTheme)Math.Clamp(s.ThemeIndex, 0, 2));
                    SetLanguage(s.LanguageIndex == 1);
                }
                else
                {
                    SetTheme(UI.AppTheme.Light);
                    SetLanguage(false);
                }
            }
            catch { _loadingSettings = false; SetTheme(UI.AppTheme.Light); SetLanguage(false); }
        }

        private void ApplySettings(AppSettings s)
        {
            _loadingSettings = true;
            _updatingAspect = true;
            try
            {
                nudWidth.Value = Math.Clamp(s.PixelWidth, 1, 9999);
                nudHeight.Value = Math.Clamp(s.PixelHeight, 1, 9999);

                chkKeepAspect.Checked = s.KeepAspect;
                cmbPixelAlgorithm.SelectedIndex = Math.Clamp(s.PixelAlgorithm, 0, 1);

                nudColorCount.Value = Math.Clamp(s.ColorCount, 2, 256);
                cmbRetroPalette.SelectedIndex = Math.Clamp(s.RetroPaletteIndex, 0, cmbRetroPalette.Items.Count - 1);

                cmbDitherMode.SelectedIndex = Math.Clamp(s.DitherMode, 0, 3);
                trkDitherIntensity.Value = Math.Clamp(s.DitherIntensity, 0, 100);

                if (s.EffectIndex == 1) rbEffectSphere.Checked = true;
                else if (s.EffectIndex == 2) rbEffectCylinder.Checked = true;
                else if (s.EffectIndex == 3) rbEffectRounded.Checked = true;
                else rbEffectNone.Checked = true;

                cmbCylinderDir.SelectedIndex = Math.Clamp(s.CylinderDir, 0, 1);
                trkSphereScale.Value = Math.Clamp(s.SphereScale, trkSphereScale.Minimum, trkSphereScale.Maximum);
                trkSphereBulge.Value = Math.Clamp(s.SphereBulge, 0, 100);
                trkOffsetU.Value = Math.Clamp(s.OffsetU, -100, 100);
                trkOffsetV.Value = Math.Clamp(s.OffsetV, -100, 100);
                trkCornerSharp.Value = Math.Clamp(s.CornerSharpness, 0, 100);
                chkShapeFromRect.Checked = s.ShapeFromRect;

                trkAmbient.Value = Math.Clamp(s.Ambient, 0, 100);
                trkDiffuse.Value = Math.Clamp(s.Diffuse, 0, 100);
                trkLightAzimuth.Value = Math.Clamp(s.LightAzimuth, -180, 180);
                trkLightElev.Value = Math.Clamp(s.LightElev, -90, 90);
                chkSpecular.Checked = s.Specular;

                chkEdges.Checked = s.ShowEdges;
                nudEdgeThickness.Value = Math.Clamp(s.EdgeThickness, 1, 3);
                cmbEdgeMode.SelectedIndex = Math.Clamp(s.EdgeMode, 0, 1);
                _edgeColor = Color.FromArgb(s.EdgeColor);
                btnEdgeColor.BackColor = _edgeColor;
                btnEdgeColor.ForeColor = _edgeColor.GetBrightness() < 0.5f ? Color.White : Color.Black;
                nudBgTolerance.Value = Math.Clamp(s.BgTolerance, 0, 255);

                lblDitherVal.Text = $"{Lang.T("Интенсивность")}: {trkDitherIntensity.Value}%";
                lblSphereScale.Text = $"{Lang.T("Масштаб")}: {trkSphereScale.Value}%";
                lblSphereBulge.Text = $"{Lang.T("Выпуклость")}: {trkSphereBulge.Value}%";
                lblCornerSharp.Text = $"{Lang.T("Форма")}: {trkCornerSharp.Value}%";
                lblOffsetU.Text = $"{Lang.T("Смещение X")}: {trkOffsetU.Value}%";
                lblOffsetV.Text = $"{Lang.T("Смещение Y")}: {trkOffsetV.Value}%";
                lblAmbient.Text = $"{Lang.T("Общий свет")}: {trkAmbient.Value}%";
                lblDiffuse.Text = $"{Lang.T("Рассеянный свет")}: {trkDiffuse.Value}%";
                lblLightAzimuth.Text = $"{Lang.T("Горизонталь")}: {trkLightAzimuth.Value}°";
                lblLightElev.Text = $"{Lang.T("Высота")}: {trkLightElev.Value}°";

            }
            finally
            {
                _updatingAspect = false;
                _loadingSettings = false;
            }
        }

        private void SetLanguage(bool english)
        {
            Lang.English = english;
            Lang.Apply(this);
            foreach (Form owned in OwnedForms)
                Lang.Apply(owned);
            miLangRu.Text = english ? "  Русский" : "● Русский";
            miLangEn.Text = english ? "● English" : "  English";
            SetTheme(_currentTheme);
            ApplySettings(BuildCurrentSettings());
            RefreshTitles();
            UpdateStatus();
        }

        private void RefreshTitles()
        {
            lblBeforeTitle.Text = _originalImage is not null
                ? $"{Lang.T("ДО")}  —  {_originalImage.Width} × {_originalImage.Height} px"
                : Lang.T("ДО");
            lblAfterTitle.Text = _processedImage is not null
                ? $"{Lang.T("ПОСЛЕ")}  —  {_processedImage.Width} × {_processedImage.Height} px"
                : Lang.T("ПОСЛЕ");
        }

        private void SetTheme(UI.AppTheme theme)
        {
            _currentTheme = theme;
            UI.ThemeManager.Apply(this, theme);
            foreach (Form owned in OwnedForms)
                UI.ThemeManager.Apply(owned, theme);
            for (int i = 0; i < _themeMenuItems.Length; i++)
            {
                bool active = i == (int)theme;
                _themeMenuItems[i].Checked = false;
                _themeMenuItems[i].Text = active ? $"● {Lang.T(ThemeLabels[i])}" : $"  {Lang.T(ThemeLabels[i])}";
            }
        }

        private string PresetsDir => AppPaths.PresetsDirectory;

        private void RefreshPresetsMenu()
        {
            menuPresets.DropDownItems.Clear();
            menuPresets.DropDownItems.Add(
                new ToolStripMenuItem(Lang.T("Сохранить пресет..."), null, (s, e) => SavePreset()));

            if (!Directory.Exists(PresetsDir)) return;
            var files = Directory.GetFiles(PresetsDir, "*.json").OrderBy(f => f).ToArray();
            if (files.Length == 0) return;

            menuPresets.DropDownItems.Add(new ToolStripSeparator());
            foreach (var file in files)
            {
                string name = Path.GetFileNameWithoutExtension(file);
                string path = file;
                menuPresets.DropDownItems.Add(
                    new ToolStripMenuItem(name, null, (s, e) => LoadPreset(path)));
            }
        }

        private void SavePreset()
        {
            string? name = ShowInputDialog(Lang.T("Сохранить пресет"), Lang.T("Название пресета:"), Lang.T("Пресет 1"));
            if (string.IsNullOrWhiteSpace(name)) return;

            foreach (char c in Path.GetInvalidFileNameChars())
                name = name.Replace(c, '_');

            try
            {
                Directory.CreateDirectory(PresetsDir);
                string path = Path.Combine(PresetsDir, name + ".json");
                File.WriteAllText(path,
                    JsonSerializer.Serialize(BuildCurrentSettings(),
                        new JsonSerializerOptions { WriteIndented = true }));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{Lang.T("Ошибка сохранения пресета")}:\n{ex.Message}", Lang.T("Ошибка"),
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadPreset(string path)
        {
            try
            {
                var s = JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(path));
                if (s is not null) ApplySettings(s);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{Lang.T("Ошибка загрузки пресета")}:\n{ex.Message}", Lang.T("Ошибка"),
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ResetSettings() => ApplySettings(new AppSettings());

        private static string? ShowInputDialog(string title, string prompt, string defaultValue = "")
        {
            using var form = new Form
            {
                Text = title,
                Width = 340,
                Height = 140,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                StartPosition = FormStartPosition.CenterParent,
                MinimizeBox = false,
                MaximizeBox = false
            };
            var lbl = new Label { Text = prompt, Left = 12, Top = 12, Width = 300, AutoSize = true };
            var txt = new TextBox { Left = 12, Top = 34, Width = 300, Text = defaultValue };
            var btnOk = new Button { Text = "OK", Left = 148, Top = 66, Width = 80, DialogResult = DialogResult.OK };
            var btnCnl = new Button { Text = Lang.T("Отмена"), Left = 232, Top = 66, Width = 80, DialogResult = DialogResult.Cancel };
            form.Controls.AddRange(new Control[] { lbl, txt, btnOk, btnCnl });
            form.AcceptButton = btnOk;
            form.CancelButton = btnCnl;
            return form.ShowDialog() == DialogResult.OK ? txt.Text.Trim() : null;
        }

        private void SyncEditorImage()
        {
            if (_syncingEditor) return;
            if (_processedImage is not null)
                _pixelEditor.EditImage = new Bitmap(_processedImage);
        }

        internal PipelineSettings GetCurrentPipelineSettings() => new PipelineSettings
        {
            PixelWidth = (int)nudWidth.Value,
            PixelHeight = (int)nudHeight.Value,
            PixelAlgorithm = cmbPixelAlgorithm.SelectedIndex == 1 ? "Center" : "Average",
            Palette = _currentPalette,
            DitherMode = GetDitherMode(),
            DitherIntensity = GetDitherIntensity(),
            EffectIndex = rbEffectSphere.Checked ? 1 : rbEffectCylinder.Checked ? 2 : rbEffectRounded.Checked ? 3 : 0,
            CylinderHorizontal = cmbCylinderDir.SelectedIndex == 0,
            Scale = trkSphereScale.Value / 100f,
            Bulge = trkSphereBulge.Value / 100f,
            CornerSharpness = trkCornerSharp.Value,
            ShapeFromRect   = chkShapeFromRect.Checked,
            OffsetU = trkOffsetU.Value / 100f,
            OffsetV = trkOffsetV.Value / 100f,
            Ambient = trkAmbient.Value / 100f,
            Diffuse = trkDiffuse.Value / 100f,
            Azimuth = trkLightAzimuth.Value,
            Elevation = trkLightElev.Value,
            Specular = chkSpecular.Checked,
        };

        private void ClearAnimation()
        {
            _animTimer.Stop();
            flpFrames.Controls.Clear();
            if (_animFrames.Count > 0)
                picBefore.Image = null;
            foreach (var f in _animFrames) f.Dispose();
            _animFrames.Clear();
            foreach (var f in _processedFrames) f.Dispose();
            _processedFrames.Clear();
            _animDelays.Clear();
            _animCurrentFrame = 0;
            lblAnimFrame.Text = Lang.T("Кадр: —");
            btnAnimPlay.Enabled = false;
            btnAnimStop.Enabled = false;
        }

        private void RebuildFrameStrip()
        {
            flpFrames.Controls.Clear();
            for (int i = 0; i < _animFrames.Count; i++)
            {
                int idx = i;
                var thumb = new PictureBox
                {
                    Width = 56,
                    Height = 56,
                    SizeMode = PictureBoxSizeMode.Zoom,
                    Image = _animFrames[i],
                    BorderStyle = BorderStyle.FixedSingle,
                    BackColor = Color.FromArgb(45, 45, 45),
                    Margin = new Padding(2),
                    Cursor = Cursors.Hand,
                };
                var lbl = new Label
                {
                    Text = $"{i + 1}",
                    TextAlign = ContentAlignment.MiddleCenter,
                    Width = 56,
                    Height = 14,
                    ForeColor = Color.Silver,
                    Font = new Font("Segoe UI", 7f),
                    Margin = new Padding(2, 0, 2, 4),
                };
                thumb.Click += (s, e) => SelectAnimFrame(idx);
                flpFrames.Controls.Add(thumb);
                flpFrames.Controls.Add(lbl);
            }
            HighlightFrameThumb(0);
        }

        private void HighlightFrameThumb(int idx)
        {
            for (int i = 0; i < flpFrames.Controls.Count; i += 2)
            {
                if (flpFrames.Controls[i] is PictureBox pb)
                    pb.BackColor = (i / 2 == idx)
                        ? Color.FromArgb(0, 120, 215)
                        : Color.FromArgb(45, 45, 45);
            }
        }

        private void SelectAnimFrame(int idx)
        {
            if (idx < 0 || idx >= _animFrames.Count) return;
            _animCurrentFrame = idx;
            picBefore.Image = _animFrames[idx];
            if (idx < _processedFrames.Count)
                SetProcessedImage(new Bitmap(_processedFrames[idx]));
            lblAnimFrame.Text = $"{Lang.T("Кадр")}: {idx + 1} / {_animFrames.Count}";
            HighlightFrameThumb(idx);
        }

        private void AnimPlay()
        {
            if (_animFrames.Count == 0) return;
            _animTimer.Interval = _animDelays.Count > 0
                ? Math.Max(20, _animDelays[_animCurrentFrame])
                : 100;
            _animTimer.Start();
            btnAnimPlay.Enabled = false;
            btnAnimStop.Enabled = true;
        }

        private void AnimStop()
        {
            _animTimer.Stop();
            btnAnimPlay.Enabled = _animFrames.Count > 0;
            btnAnimStop.Enabled = false;
        }

        private void OnAnimTimerTick(object? sender, EventArgs e)
        {
            _animCurrentFrame = (_animCurrentFrame + 1) % _animFrames.Count;
            picBefore.Image = _animFrames[_animCurrentFrame];
            if (_animCurrentFrame < _processedFrames.Count)
            {
                picAfter.Image = null;
                _processedImage?.Dispose();
                _processedImage = new Bitmap(_processedFrames[_animCurrentFrame]);
                UpdatePicAfter();
            }
            lblAnimFrame.Text = $"{Lang.T("Кадр")}: {_animCurrentFrame + 1} / {_animFrames.Count}";
            HighlightFrameThumb(_animCurrentFrame);

            int nextDelay = _animDelays.Count > _animCurrentFrame
                ? _animDelays[_animCurrentFrame]
                : 100;
            _animTimer.Interval = Math.Max(20, nextDelay);
        }

        private void ProcessAllFrames()
        {
            if (_animFrames.Count == 0) return;
            var settings = GetCurrentPipelineSettings();

            foreach (var f in _processedFrames) f.Dispose();
            _processedFrames.Clear();

            Cursor = Cursors.WaitCursor;
            try
            {
                for (int i = 0; i < _animFrames.Count; i++)
                {
                    _processedFrames.Add(AnimationProcessor.ProcessFrame(_animFrames[i], settings));
                    lblAnimFrame.Text = $"{Lang.T("Обработка")}: {i + 1} / {_animFrames.Count}";
                    Application.DoEvents();
                }
                lblAnimFrame.Text = $"{Lang.T("Кадр")}: {_animCurrentFrame + 1} / {_animFrames.Count}";
                SelectAnimFrame(_animCurrentFrame);
            }
            finally { Cursor = Cursors.Default; }
        }

        private void AnimExportGif()
        {
            if (_processedFrames.Count == 0)
            {
                MessageBox.Show(Lang.T("Сначала обработайте кадры."), "KebuzForge",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            using var dlg = new SaveFileDialog
            {
                Title = Lang.T("Экспорт анимированного GIF"),
                Filter = "GIF|*.gif",
                FileName = "animation"
            };
            if (dlg.ShowDialog() != DialogResult.OK) return;
            try
            {
                Cursor = Cursors.WaitCursor;
                AnimationProcessor.ExportGif(_processedFrames, _animDelays, dlg.FileName);
            }
            finally { Cursor = Cursors.Default; }
        }

        private void OpenBatchForm()
        {
            using var form = new Forms.BatchForm(GetCurrentPipelineSettings(), _currentPalette);
            form.ShowDialog(this);
        }

        private void RefreshPluginsMenu()
        {
            menuPlugins.DropDownItems.Clear();
            menuPlugins.DropDownItems.Add(new ToolStripMenuItem(
                Lang.T("Открыть папку плагинов..."), null, (s, e) => PluginManager.OpenPluginFolder()));
            menuPlugins.DropDownItems.Add(new ToolStripSeparator());

            var plugins = PluginManager.Discover();
            if (plugins.Count == 0)
            {
                menuPlugins.DropDownItems.Add(new ToolStripMenuItem(Lang.T("Плагины не найдены"))
                {
                    Enabled = false
                });
                return;
            }
            foreach (var plugin in plugins)
            {
                var item = new ToolStripMenuItem(
                    $"{plugin.Name}  v{plugin.Version}", null, (s, e) => ShowPlugin(plugin))
                {
                    ToolTipText = plugin.Description
                };
                menuPlugins.DropDownItems.Add(item);
            }
        }

        private void ShowPlugin(PluginInfo plugin)
        {
            try
            {
                plugin.SetLanguage?.Invoke(Lang.English);
                var form = plugin.CreateForm();
                form.Icon = this.Icon;
                UI.ThemeManager.Apply(form, _currentTheme);
                form.Show(this);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{Lang.T("Ошибка запуска плагина")}:\n{ex.Message}", "KebuzForge",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void rightSplit_SplitterMoved(object sender, SplitterEventArgs e)
        {

        }

        private void mainSplit_SplitterMoved(object sender, SplitterEventArgs e)
        {

        }

        private void btnDetachPalette_Click(object sender, EventArgs e)
        {

        }
    }
}
