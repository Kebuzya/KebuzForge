using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using KebuzForge.App.Core;
using KebuzForge.Canvas;

namespace KebuzForge.App
{
    public partial class MainForm
    {
        private string? _projectPath;
        private string _documentName = "";
        private bool _projectDirty;
        private bool _autosaveDirty;
        private readonly System.Windows.Forms.Timer _autosaveTimer = new() { Interval = 120_000 };

        private static string AutosaveFile =>
            Path.Combine(AppPaths.AutosaveDirectory, $"autosave-{Environment.ProcessId}{ProjectFile.Extension}");

        private void WireProject()
        {
            menuFileSave.Click += (s, e) => SaveProject(saveAs: false);
            menuFileSaveAs.Click += (s, e) => SaveProject(saveAs: true);
            _autosaveTimer.Tick += (s, e) => Autosave();
            _autosaveTimer.Start();
            FormClosing += (s, e) =>
            {
                if (!ConfirmDiscardChanges())
                {
                    e.Cancel = true;
                    return;
                }
                _autosaveTimer.Stop();
                DeleteAutosave();
            };
        }

        private void MarkDirty()
        {
            _autosaveDirty = true;
            if (_projectDirty || _canvas is null) return;
            _projectDirty = true;
            UpdateTitle();
        }

        private void UpdateTitle()
        {
            if (_canvas is null) return;
            string name = _projectPath is not null ? Path.GetFileName(_projectPath) : _documentName;
            Text = $"KebuzForge - {name}{(_projectDirty ? " *" : "")}";
        }

        private ProjectData CaptureProject() => new()
        {
            Canvas = _canvas!,
            Settings = BuildCurrentSettings(),
            Palette = _currentPalette,
            PaletteIndices = _paletteIndices,
            PaletteActive = _paletteActive,
            DitherActive = _ditherActive,
            EffectActive = _effectActive,
            FlatActive = _flatActive,
            BackgroundRemoved = _bgRemoveActive,
            Pixelized = _pixelizedImage,
            Result = _pipelineResult,
            Overlay = _editorOverlay,
            PreOverlay = _preOverlay,
            SourcePath = _projectPath
        };

        private bool SaveProject(bool saveAs)
        {
            if (_canvas is null)
            {
                MessageBox.Show(Lang.T("Нечего сохранять: откройте картинку или создайте холст."), "KebuzForge",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }

            string? path = saveAs ? null : _projectPath;
            if (path is null)
            {
                using var dlg = new SaveFileDialog
                {
                    Title = Lang.T("Сохранить проект как..."),
                    Filter = Lang.T("Проект KebuzForge|*.kforge"),
                    FileName = Path.GetFileNameWithoutExtension(_projectPath ?? _documentName),
                    DefaultExt = "kforge",
                    AddExtension = true
                };
                if (dlg.ShowDialog(this) != DialogResult.OK) return false;
                path = dlg.FileName;
            }

            try
            {
                Cursor = Cursors.WaitCursor;
                ProjectFile.Save(path, CaptureProject());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{Lang.T("Не удалось сохранить проект")}:\n{ex.Message}", Lang.T("Ошибка"),
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            finally
            {
                Cursor = Cursors.Default;
            }

            _projectPath = path;
            _projectDirty = false;
            _autosaveDirty = false;
            DeleteAutosave();
            UpdateTitle();
            return true;
        }

        private bool ConfirmDiscardChanges()
        {
            if (_canvas is null || !_projectDirty) return true;
            var answer = MessageBox.Show(Lang.T("Сохранить изменения в проекте?"), "KebuzForge",
                MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            return answer switch
            {
                DialogResult.Yes => SaveProject(saveAs: false),
                DialogResult.No => true,
                _ => false
            };
        }

        private void OpenProject(string path, bool fromAutosave = false)
        {
            if (!fromAutosave && !ConfirmLeaveCanvas()) return;
            ProjectData data;
            try
            {
                Cursor = Cursors.WaitCursor;
                data = ProjectFile.Load(path);
            }
            catch (NotSupportedException)
            {
                MessageBox.Show(Lang.T("Проект создан более новой версией программы."), Lang.T("Ошибка"),
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{Lang.T("Не удалось открыть проект")}:\n{ex.Message}", Lang.T("Ошибка"),
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            finally
            {
                Cursor = Cursors.Default;
            }

            ApplyProject(data, fromAutosave ? data.SourcePath : path);
            if (fromAutosave)
            {
                _projectDirty = true;
                _autosaveDirty = true;
            }
            UpdateTitle();
        }

        private void ApplyProject(ProjectData data, string? path)
        {
            ClearAnimation();
            EnterCanvasMode(data.Canvas, fresh: true, path, recompute: false);
            _projectPath = path;

            var settings = data.Settings;
            settings.AutoCascade = _autoCascade;
            ApplySettings(settings);

            _currentPalette = data.Palette;
            _palettePanel.Palette = _currentPalette;
            _paletteActive = data.PaletteActive;
            _ditherActive = data.DitherActive;
            _effectActive = data.EffectActive;
            _flatActive = data.FlatActive;
            _bgRemoveActive = data.BackgroundRemoved;

            if (data.Pixelized is null || data.Result is null)
            {
                RecomputeFromCanvas();
                return;
            }

            _pixelizedImage?.Dispose();
            _pixelizedImage = data.Pixelized;
            _paletteIndices = data.PaletteIndices;
            _paletteIndexSize = data.PaletteIndices is not null ? data.Pixelized.Size : Size.Empty;
            _resultIndices = null;

            _pipelineResult?.Dispose();
            _pipelineResult = data.Result;
            _editorOverlay = data.Overlay is { } o && o.Size == data.Result.Size ? o : null;
            _preOverlay = data.PreOverlay is { } pre && pre.Size == data.Pixelized.Size ? pre : null;
            _processedImage?.Dispose();
            _processedImage = _editorOverlay is not null
                ? _editorOverlay.ApplyTo(data.Result)
                : PixelOverlay.CopyOf(data.Result);

            _historySettings = BuildCurrentSettings();
            UpdateHistoryBaseline();
            lblAfterTitle.Text = $"{Lang.T("ПОСЛЕ")}  -  {_processedImage.Width} × {_processedImage.Height} px";
            UpdatePicAfter();
            SyncEditorImage();
            UpdateStatus();
            UpdateUndoRedoButtons();
        }

        private void Autosave()
        {
            if (_canvas is null || !_autosaveDirty) return;
            try
            {
                ProjectFile.Save(AutosaveFile, CaptureProject());
                _autosaveDirty = false;
            }
            catch
            {
            }
        }

        private void DeleteAutosave()
        {
            try
            {
                if (File.Exists(AutosaveFile)) File.Delete(AutosaveFile);
            }
            catch
            {
            }
        }

        private void OfferAutosaveRecovery()
        {
            FileInfo[] orphans;
            try
            {
                if (!Directory.Exists(AppPaths.AutosaveDirectory)) return;
                orphans = new DirectoryInfo(AppPaths.AutosaveDirectory)
                    .GetFiles("autosave-*" + ProjectFile.Extension)
                    .Where(f => !IsLiveInstance(f.Name))
                    .OrderByDescending(f => f.LastWriteTime)
                    .ToArray();
            }
            catch
            {
                return;
            }
            if (orphans.Length == 0) return;

            var newest = orphans[0];
            var answer = MessageBox.Show(
                $"{Lang.T("Программа была закрыта без сохранения. Найдено автосохранение от")} {newest.LastWriteTime:g}.\n{Lang.T("Восстановить?")}",
                "KebuzForge", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (answer == DialogResult.Yes)
                OpenProject(newest.FullName, fromAutosave: true);
            foreach (var f in orphans)
            {
                try { f.Delete(); } catch { }
            }
            if (answer == DialogResult.Yes) Autosave();
        }

        private static bool IsLiveInstance(string fileName)
        {
            string digits = new string(Path.GetFileNameWithoutExtension(fileName).SkipWhile(c => !char.IsDigit(c)).ToArray());
            if (!int.TryParse(digits, out int pid) || pid == Environment.ProcessId) return false;
            try
            {
                using var p = Process.GetProcessById(pid);
                return string.Equals(p.ProcessName, Process.GetCurrentProcess().ProcessName, StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }
    }
}
