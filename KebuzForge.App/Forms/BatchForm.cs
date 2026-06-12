using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using KebuzForge.App.Core;

namespace KebuzForge.App.Forms
{
    internal sealed class BatchForm : Form
    {

        private readonly PipelineSettings _settings;

        private TextBox    txtInputFolder  = null!;
        private TextBox    txtOutputFolder = null!;
        private ComboBox   cmbOutputFormat = null!;
        private ProgressBar pbBatch        = null!;
        private ListBox    lstLog          = null!;
        private Button     btnStart        = null!;
        private Button     btnClose        = null!;

        public BatchForm(PipelineSettings settings, Color[] palette)
        {

            _settings = settings;
            _settings.Palette = palette;

            BuildUI();
        }

        private void BuildUI()
        {
            Text            = Lang.T("Пакетная обработка");
            Size            = new Size(560, 420);
            MinimumSize     = new Size(560, 420);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox     = false;
            StartPosition   = FormStartPosition.CenterParent;

            var grpIn = new GroupBox
            {
                Text   = Lang.T("Входная папка"),
                Left   = 10, Top = 10,
                Width  = 520, Height = 60
            };
            txtInputFolder = new TextBox { Left = 10, Top = 22, Width = 390, Height = 24 };
            var btnBrowseIn = new Button { Text = Lang.T("Обзор..."), Left = 408, Top = 20, Width = 96, Height = 26 };
            btnBrowseIn.Click += (s, e) => BrowseFolder(txtInputFolder);
            grpIn.Controls.AddRange(new Control[] { txtInputFolder, btnBrowseIn });

            var grpOut = new GroupBox
            {
                Text   = Lang.T("Выходная папка"),
                Left   = 10, Top = 78,
                Width  = 520, Height = 60
            };
            txtOutputFolder = new TextBox { Left = 10, Top = 22, Width = 390, Height = 24 };
            var btnBrowseOut = new Button { Text = Lang.T("Обзор..."), Left = 408, Top = 20, Width = 96, Height = 26 };
            btnBrowseOut.Click += (s, e) => BrowseFolder(txtOutputFolder);
            grpOut.Controls.AddRange(new Control[] { txtOutputFolder, btnBrowseOut });

            var lblFmt = new Label { Text = Lang.T("Формат:"), Left = 10, Top = 150, Width = 60, Height = 24, TextAlign = ContentAlignment.MiddleLeft };
            cmbOutputFormat = new ComboBox
            {
                Left          = 74, Top = 148,
                Width         = 200,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbOutputFormat.Items.AddRange(new object[] { "PNG", Lang.T("ICO (мульти-размер)"), "Indexed PNG" });
            cmbOutputFormat.SelectedIndex = 0;

            pbBatch = new ProgressBar
            {
                Left    = 10, Top  = 184,
                Width   = 520, Height = 20,
                Minimum = 0, Maximum = 100
            };

            lstLog = new ListBox
            {
                Left          = 10, Top    = 212,
                Width         = 520, Height = 136,
                ScrollAlwaysVisible = true,
                HorizontalScrollbar = true
            };

            btnStart = new Button { Text = Lang.T("Начать"), Left = 352, Top = 356, Width = 80, Height = 28 };
            btnClose = new Button { Text = Lang.T("Закрыть"), Left = 442, Top = 356, Width = 88, Height = 28 };

            btnStart.Click += async (s, e) => await RunBatch();
            btnClose.Click += (s, e) => Close();

            Controls.AddRange(new Control[]
            {
                grpIn, grpOut, lblFmt, cmbOutputFormat,
                pbBatch, lstLog, btnStart, btnClose
            });
        }

        private static void BrowseFolder(TextBox target)
        {
            using var dlg = new FolderBrowserDialog { ShowNewFolderButton = true };
            if (!string.IsNullOrEmpty(target.Text) && Directory.Exists(target.Text))
                dlg.SelectedPath = target.Text;
            if (dlg.ShowDialog() == DialogResult.OK)
                target.Text = dlg.SelectedPath;
        }

        private async Task RunBatch()
        {
            string inputDir  = txtInputFolder.Text.Trim();
            string outputDir = txtOutputFolder.Text.Trim();

            if (!Directory.Exists(inputDir))
            {
                MessageBox.Show(Lang.T("Входная папка не существует."), "KebuzForge",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrEmpty(outputDir))
            {
                MessageBox.Show(Lang.T("Укажите выходную папку."), "KebuzForge",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Directory.CreateDirectory(outputDir);

            var extensions = new[] { "*.png", "*.jpg", "*.jpeg", "*.bmp", "*.gif" };
            var files = new List<string>();
            foreach (var ext in extensions)
                files.AddRange(Directory.GetFiles(inputDir, ext, SearchOption.TopDirectoryOnly));

            if (files.Count == 0)
            {
                MessageBox.Show(Lang.T("Изображений не найдено в выбранной папке."), "KebuzForge",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int fmtIndex = cmbOutputFormat.SelectedIndex;
            bool useIndexed = fmtIndex == 2;
            bool useIco     = fmtIndex == 1;

            if (useIndexed && _settings.Palette.Length == 0)
            {
                Log(Lang.T("Предупреждение: палитра пуста - используется PNG вместо Indexed PNG."));
                useIndexed = false;
            }

            btnStart.Enabled = false;
            pbBatch.Value    = 0;
            lstLog.Items.Clear();

            int processed = 0;

            await Task.Run(() =>
            {
                for (int i = 0; i < files.Count; i++)
                {
                    string file = files[i];
                    string baseName = Path.GetFileNameWithoutExtension(file);

                    try
                    {
                        using var src = new System.Drawing.Bitmap(file);
                        using var result = AnimationProcessor.ProcessFrame(src, _settings);

                        string outPath;
                        if (useIco)
                        {
                            outPath = Path.Combine(outputDir, baseName + ".ico");
                            ExportManager.SaveIco(result, outPath);
                        }
                        else if (useIndexed)
                        {
                            outPath = Path.Combine(outputDir, baseName + ".png");
                            ExportManager.SaveIndexedPng(result, _settings.Palette, outPath);
                        }
                        else
                        {
                            outPath = Path.Combine(outputDir, baseName + ".png");
                            ExportManager.SavePng(result, outPath);
                        }

                        processed++;

                        int pct = (int)((i + 1) * 100.0 / files.Count);
                        Invoke(() =>
                        {
                            pbBatch.Value = pct;
                            Log($"[OK] {Path.GetFileName(file)} → {Path.GetFileName(outPath)}");
                        });
                    }
                    catch (Exception ex)
                    {
                        Invoke(() => Log($"[ERr] {Path.GetFileName(file)}: {ex.Message}"));
                    }
                }
            });

            pbBatch.Value    = 100;
            btnStart.Enabled = true;
            Log($"{Lang.T("Готово!")} {processed} {Lang.T("файлов обработано")}.");
            MessageBox.Show($"{Lang.T("Готово!")} {processed} {Lang.T("файлов обработано")}.", "KebuzForge",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void Log(string message)
        {
            lstLog.Items.Add(message);
            lstLog.TopIndex = lstLog.Items.Count - 1;
        }
    }
}
