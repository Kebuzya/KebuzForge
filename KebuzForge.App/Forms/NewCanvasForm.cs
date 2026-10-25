using System;
using System.Drawing;
using System.Windows.Forms;

namespace KebuzForge.App.Forms
{
    internal sealed partial class NewCanvasForm : Form
    {
        private int[] _presets = [256, 512, 1024];

        public NewCanvasForm()
        {
            InitializeComponent();
            btnPresetSmall.Click += (s, e) => SetSize(_presets[0]);
            btnPresetMedium.Click += (s, e) => SetSize(_presets[1]);
            btnPresetLarge.Click += (s, e) => SetSize(_presets[2]);
            btnBgColor.Click += (s, e) =>
            {
                using var dlg = new ColorDialog { Color = btnBgColor.BackColor, FullOpen = true };
                if (dlg.ShowDialog(this) != DialogResult.OK) return;
                btnBgColor.BackColor = dlg.Color;
                rbColor.Checked = true;
            };
        }

        public int CanvasWidth => (int)nudWidth.Value;
        public int CanvasHeight => (int)nudHeight.Value;
        public Color BackgroundColor => rbTransparent.Checked ? Color.Transparent : btnBgColor.BackColor;

        public void UsePixelArtDefaults()
        {
            Text = "Новый пиксель-арт";
            _presets = [16, 32, 64];
            btnPresetSmall.Text = "16";
            btnPresetMedium.Text = "32";
            btnPresetLarge.Text = "64";
            nudWidth.Maximum = 1024;
            nudHeight.Maximum = 1024;
            SetSize(32);
        }

        private void SetSize(int side)
        {
            nudWidth.Value = Math.Clamp(side, (int)nudWidth.Minimum, (int)nudWidth.Maximum);
            nudHeight.Value = Math.Clamp(side, (int)nudHeight.Minimum, (int)nudHeight.Maximum);
        }
    }
}
