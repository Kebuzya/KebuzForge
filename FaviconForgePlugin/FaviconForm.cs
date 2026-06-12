using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace FaviconForge
{
    public partial class FaviconForm : Form
    {
        private Bitmap? _source;
        private Color _themeColor = Color.White;
        private Color _favColor = Color.White;
        private Color _iosColor = Color.White;
        private Color _androidColor = Color.White;
        private Color _tileColor = Color.FromArgb(0x2D, 0x89, 0xEF);
        private Color _safariColor = Color.FromArgb(0x5B, 0xBA, 0xD5);

        public FaviconForm()
        {
            InitializeComponent();
            Lang.Apply(this);
            DoubleBuffered = true;
            Wire();
            UpdateHtml();
        }

        private void Wire()
        {
            previewTimer.Tick += (s, e) =>
            {
                previewTimer.Stop();
                UpdatePreviews();
            };

            btnOpenSource.Click += (s, e) => OpenSource();

            AllowDrop = true;
            DragEnter += (s, e) =>
            {
                if (e.Data?.GetDataPresent(DataFormats.FileDrop) == true)
                    e.Effect = DragDropEffects.Copy;
            };
            DragDrop += (s, e) =>
            {
                if (e.Data?.GetData(DataFormats.FileDrop) is string[] files && files.Length > 0)
                    LoadSource(files[0]);
            };

            btnThemeColor.Click += (s, e) => PickColor(btnThemeColor, ref _themeColor);
            btnFavColor.Click += (s, e) => PickColor(btnFavColor, ref _favColor);
            btnIosColor.Click += (s, e) => PickColor(btnIosColor, ref _iosColor);
            btnAndroidColor.Click += (s, e) => PickColor(btnAndroidColor, ref _androidColor);
            btnTileColor.Click += (s, e) => PickColor(btnTileColor, ref _tileColor);
            btnSafariColor.Click += (s, e) => PickColor(btnSafariColor, ref _safariColor);

            chkFavBg.CheckedChanged += (s, e) => ScheduleUpdate();
            chkIosBg.CheckedChanged += (s, e) => ScheduleUpdate();
            chkAndroidBg.CheckedChanged += (s, e) => ScheduleUpdate();
            chkAndroidCircle.CheckedChanged += (s, e) => ScheduleUpdate();
            chkPixelArt.CheckedChanged += (s, e) => ScheduleUpdate();

            trkFavMargin.ValueChanged += (s, e) =>
            {
                lblFavMargin.Text = $"{Lang.T("Отступ")}: {trkFavMargin.Value}%";
                ScheduleUpdate();
            };
            trkFavRadius.ValueChanged += (s, e) =>
            {
                lblFavRadius.Text = $"{Lang.T("Скругление")}: {trkFavRadius.Value}%";
                ScheduleUpdate();
            };
            trkIosMargin.ValueChanged += (s, e) =>
            {
                lblIosMargin.Text = $"{Lang.T("Отступ")}: {trkIosMargin.Value}%";
                ScheduleUpdate();
            };
            trkAndroidMargin.ValueChanged += (s, e) =>
            {
                lblAndroidMargin.Text = $"{Lang.T("Отступ")}: {trkAndroidMargin.Value}%";
                ScheduleUpdate();
            };
            trkTileMargin.ValueChanged += (s, e) =>
            {
                lblTileMargin.Text = $"{Lang.T("Отступ")}: {trkTileMargin.Value}%";
                ScheduleUpdate();
            };
            trkSafariThreshold.ValueChanged += (s, e) =>
            {
                lblSafariThreshold.Text = $"{Lang.T("Порог силуэта")}: {trkSafariThreshold.Value}";
                ScheduleUpdate();
            };

            txtAndroidName.TextChanged += (s, e) => ScheduleUpdate();
            txtAndroidShort.TextChanged += (s, e) => ScheduleUpdate();
            txtAppleTitle.TextChanged += (s, e) => ScheduleUpdate();
            txtPathPrefix.TextChanged += (s, e) => UpdateHtml();

            btnGenerateZip.Click += (s, e) => GenerateZip();
            btnGenerateFolder.Click += (s, e) => GenerateFolder();
            btnCopyHtml.Click += (s, e) =>
            {
                if (txtHtml.Text.Length > 0)
                    Clipboard.SetText(txtHtml.Text);
            };
        }

        private void ScheduleUpdate()
        {
            UpdateHtml();
            previewTimer.Stop();
            previewTimer.Start();
        }

        private void PickColor(Button btn, ref Color target)
        {
            using var dlg = new ColorDialog { Color = target, FullOpen = true };
            if (dlg.ShowDialog(this) != DialogResult.OK)
                return;
            target = dlg.Color;
            btn.BackColor = target;
            btn.ForeColor = target.GetBrightness() < 0.5f ? Color.White : Color.Black;
            btn.Text = FaviconEngine.ToHex(target);
            UpdatePreviews();
            UpdateHtml();
        }

        private void OpenSource()
        {
            using var dlg = new OpenFileDialog
            {
                Title = Lang.T("Открыть исходное изображение"),
                Filter = Lang.T("Изображения|*.png;*.jpg;*.jpeg;*.bmp;*.gif;*.ico|Все файлы|*.*")
            };
            if (dlg.ShowDialog(this) == DialogResult.OK)
                LoadSource(dlg.FileName);
        }

        private void LoadSource(string path)
        {
            try
            {
                using var loaded = new Bitmap(path);
                _source?.Dispose();
                _source = new Bitmap(loaded);
                lblSourceInfo.Text = $"{Path.GetFileName(path)}  -  {_source.Width} × {_source.Height} px";
                UpdatePreviews();
                UpdateHtml();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{Lang.T("Ошибка загрузки")}:\n{ex.Message}", "FaviconForge",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private FaviconOptions BuildOptions() => new FaviconOptions
        {
            AppName = ValueOr(txtAndroidName.Text, "My Application"),
            ShortName = ValueOr(txtAndroidShort.Text, "App"),
            AppleTitle = ValueOr(txtAppleTitle.Text, "My App"),
            PathPrefix = txtPathPrefix.Text,
            ThemeColor = _themeColor,
            PixelArt = chkPixelArt.Checked,
            FaviconBackground = chkFavBg.Checked,
            FaviconBackColor = _favColor,
            FaviconMargin = trkFavMargin.Value,
            FaviconRadius = trkFavRadius.Value,
            IosBackground = chkIosBg.Checked,
            IosBackColor = _iosColor,
            IosMargin = trkIosMargin.Value,
            AndroidBackground = chkAndroidBg.Checked,
            AndroidBackColor = _androidColor,
            AndroidMargin = trkAndroidMargin.Value,
            AndroidCircle = chkAndroidCircle.Checked,
            TileColor = _tileColor,
            TileMargin = trkTileMargin.Value,
            SafariColor = _safariColor,
            SafariThreshold = trkSafariThreshold.Value,
        };

        private static string ValueOr(string value, string fallback) =>
            string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();

        private void UpdatePreviews()
        {
            if (_source is null)
                return;
            var o = BuildOptions();

            using (var fav = FaviconEngine.ComposeFavicon(_source, o, 32))
            {
                SetPreview(picPrevBrowserLight, MockupRenderer.BrowserTab(fav, o.AppName, false));
                SetPreview(picPrevBrowserDark, MockupRenderer.BrowserTab(fav, o.AppName, true));
                SetPreview(picPrevSearchLight, MockupRenderer.SearchResult(fav, o.AppName, false));
                SetPreview(picPrevSearchDark, MockupRenderer.SearchResult(fav, o.AppName, true));
            }

            using (var ios = FaviconEngine.ComposeIos(_source, o, 152))
                SetPreview(picPrevIos, MockupRenderer.IosHome(ios, o.AppleTitle));

            using (var android = FaviconEngine.ComposeAndroid(_source, o, 148))
                SetPreview(picPrevAndroid, MockupRenderer.AndroidHome(android, o.ShortName));

            using (var splash = FaviconEngine.ComposeAndroid(_source, o, 120))
                SetPreview(picPrevSplash, MockupRenderer.AndroidSplash(splash, o.AppName, o.ThemeColor));

            using (var small = FaviconEngine.ComposeAndroid(_source, o, 52))
                SetPreview(picPrevSwitcher, MockupRenderer.AndroidSwitcher(small, o.AppName));

            using (var tile = FaviconEngine.ComposeTile(_source, o, 150, 150))
                SetPreview(picPrevTile, MockupRenderer.WindowsTile(tile, o.TileColor, o.AppName));

            using (var mono = FaviconEngine.BuildSilhouette(_source, o.SafariThreshold))
                SetPreview(picPrevSafari, MockupRenderer.SafariPinnedTab(mono, o.SafariColor, o.AppName));
        }

        private static void SetPreview(PictureBox pic, Bitmap bmp)
        {
            var old = pic.Image;
            pic.Image = bmp;
            old?.Dispose();
        }

        private void UpdateHtml() =>
            txtHtml.Text = FaviconEngine.BuildHtml(BuildOptions()).Replace("\n", Environment.NewLine);

        private bool EnsureSource()
        {
            if (_source is not null)
                return true;
            MessageBox.Show(Lang.T("Сначала загрузите исходное изображение."), "FaviconForge",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return false;
        }

        private void GenerateZip()
        {
            if (!EnsureSource())
                return;
            using var dlg = new SaveFileDialog
            {
                Title = Lang.T("Сохранить favicon-пакет"),
                Filter = Lang.T("ZIP архив|*.zip"),
                FileName = "favicon_package"
            };
            if (dlg.ShowDialog(this) != DialogResult.OK)
                return;
            try
            {
                Cursor = Cursors.WaitCursor;
                var files = FaviconEngine.GenerateAll(_source!, BuildOptions());
                FaviconEngine.WriteZip(files, dlg.FileName);
                lblSourceInfo.Text = $"{Lang.T("Готово")}: {Path.GetFileName(dlg.FileName)} ({files.Count} {Lang.T("файлов")})";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{Lang.T("Ошибка генерации")}:\n{ex.Message}", "FaviconForge",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void GenerateFolder()
        {
            if (!EnsureSource())
                return;
            using var dlg = new FolderBrowserDialog
            {
                Description = Lang.T("Папка для favicon-пакета"),
                UseDescriptionForTitle = true
            };
            if (dlg.ShowDialog(this) != DialogResult.OK)
                return;
            try
            {
                Cursor = Cursors.WaitCursor;
                var files = FaviconEngine.GenerateAll(_source!, BuildOptions());
                FaviconEngine.WriteFolder(files, dlg.SelectedPath);
                lblSourceInfo.Text = $"{Lang.T("Готово")}: {files.Count} {Lang.T("файлов в")} {dlg.SelectedPath}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{Lang.T("Ошибка генерации")}:\n{ex.Message}", "FaviconForge",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);
            previewTimer.Stop();
            _source?.Dispose();
        }
    }
}
