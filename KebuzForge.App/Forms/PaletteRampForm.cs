using System;
using System.Drawing;
using System.Windows.Forms;
using KebuzForge.App.Core;

namespace KebuzForge.App.Forms
{
    internal sealed partial class PaletteRampForm : Form
    {
        private readonly Color[] _source;

        public PaletteRampForm(Color[] source)
        {
            _source = source;
            InitializeComponent();
            nudDark.ValueChanged += (s, e) => UpdatePreview();
            nudLight.ValueChanged += (s, e) => UpdatePreview();
            trkStrength.ValueChanged += (s, e) => UpdatePreview();
            trkHue.ValueChanged += (s, e) => UpdatePreview();
            Shown += (s, e) =>
            {
                UpdatePreview();
                previewTimer.Stop();
                PreviewChanged?.Invoke(this, Result);
            };
            previewTimer.Tick += (s, e) =>
            {
                previewTimer.Stop();
                PreviewChanged?.Invoke(this, Result);
            };
        }

        public event EventHandler<Color[]>? PreviewChanged;

        public Color[] Result { get; private set; } = [];

        private void UpdatePreview()
        {
            Result = PaletteRamp.Build(_source, (int)nudDark.Value, (int)nudLight.Value,
                trkStrength.Value / 100f, trkHue.Value / 100f);
            lblStrength.Text = $"{Lang.T("Сила ступени")}: {trkStrength.Value}%";
            lblHue.Text = $"{Lang.T("Сдвиг оттенка")}: {trkHue.Value}%";
            lblCount.Text = $"{Lang.T("Цветов:")} {Result.Length}";
            palPreview.Palette = Result;
            previewTimer.Stop();
            previewTimer.Start();
        }
    }
}
