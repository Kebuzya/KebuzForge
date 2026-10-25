namespace KebuzForge.App.Forms
{
    partial class PaletteRampForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            previewTimer = new System.Windows.Forms.Timer(components);
            lblDark = new Label();
            nudDark = new NumericUpDown();
            lblLight = new Label();
            nudLight = new NumericUpDown();
            lblStrength = new Label();
            trkStrength = new TrackBar();
            lblHue = new Label();
            trkHue = new TrackBar();
            lblCount = new Label();
            panelPreview = new Panel();
            palPreview = new UI.PalettePanel();
            btnOk = new Button();
            btnCancel = new Button();
            ((System.ComponentModel.ISupportInitialize)nudDark).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudLight).BeginInit();
            ((System.ComponentModel.ISupportInitialize)trkStrength).BeginInit();
            ((System.ComponentModel.ISupportInitialize)trkHue).BeginInit();
            panelPreview.SuspendLayout();
            SuspendLayout();

            previewTimer.Interval = 120;

            lblDark.AutoSize = true;
            lblDark.Location = new Point(16, 18);
            lblDark.Name = "lblDark";
            lblDark.Size = new Size(90, 15);
            lblDark.TabIndex = 0;
            lblDark.Text = "Ступеней тени";

            nudDark.Location = new Point(170, 14);
            nudDark.Maximum = new decimal(new int[] { 3, 0, 0, 0 });
            nudDark.Name = "nudDark";
            nudDark.Size = new Size(60, 23);
            nudDark.TabIndex = 1;
            nudDark.Value = new decimal(new int[] { 1, 0, 0, 0 });

            lblLight.AutoSize = true;
            lblLight.Location = new Point(16, 50);
            lblLight.Name = "lblLight";
            lblLight.Size = new Size(96, 15);
            lblLight.TabIndex = 2;
            lblLight.Text = "Ступеней блика";

            nudLight.Location = new Point(170, 46);
            nudLight.Maximum = new decimal(new int[] { 3, 0, 0, 0 });
            nudLight.Name = "nudLight";
            nudLight.Size = new Size(60, 23);
            nudLight.TabIndex = 3;
            nudLight.Value = new decimal(new int[] { 1, 0, 0, 0 });

            lblStrength.Location = new Point(16, 82);
            lblStrength.Name = "lblStrength";
            lblStrength.Size = new Size(240, 16);
            lblStrength.TabIndex = 4;
            lblStrength.Text = "Сила ступени: 25%";

            trkStrength.AutoSize = false;
            trkStrength.Location = new Point(12, 100);
            trkStrength.Maximum = 60;
            trkStrength.Minimum = 5;
            trkStrength.Name = "trkStrength";
            trkStrength.Size = new Size(332, 26);
            trkStrength.TabIndex = 5;
            trkStrength.TickStyle = TickStyle.None;
            trkStrength.Value = 25;

            lblHue.Location = new Point(16, 132);
            lblHue.Name = "lblHue";
            lblHue.Size = new Size(240, 16);
            lblHue.TabIndex = 6;
            lblHue.Text = "Сдвиг оттенка: 50%";

            trkHue.AutoSize = false;
            trkHue.Location = new Point(12, 150);
            trkHue.Maximum = 100;
            trkHue.Name = "trkHue";
            trkHue.Size = new Size(332, 26);
            trkHue.TabIndex = 7;
            trkHue.TickStyle = TickStyle.None;
            trkHue.Value = 50;

            lblCount.Location = new Point(16, 184);
            lblCount.Name = "lblCount";
            lblCount.Size = new Size(240, 16);
            lblCount.TabIndex = 8;
            lblCount.Text = "Цветов: 0";

            panelPreview.AutoScroll = true;
            panelPreview.BorderStyle = BorderStyle.FixedSingle;
            panelPreview.Controls.Add(palPreview);
            panelPreview.Location = new Point(16, 206);
            panelPreview.Name = "panelPreview";
            panelPreview.Size = new Size(328, 110);
            panelPreview.TabIndex = 9;

            palPreview.Dock = DockStyle.Top;
            palPreview.Enabled = false;
            palPreview.Location = new Point(0, 0);
            palPreview.Name = "palPreview";
            palPreview.ShowAddTile = false;
            palPreview.Size = new Size(309, 60);
            palPreview.TabIndex = 0;

            btnOk.DialogResult = DialogResult.OK;
            btnOk.Location = new Point(168, 328);
            btnOk.Name = "btnOk";
            btnOk.Size = new Size(84, 30);
            btnOk.TabIndex = 10;
            btnOk.Text = "Применить";

            btnCancel.DialogResult = DialogResult.Cancel;
            btnCancel.Location = new Point(260, 328);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(84, 30);
            btnCancel.TabIndex = 11;
            btnCancel.Text = "Отмена";

            AcceptButton = btnOk;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnCancel;
            ClientSize = new Size(360, 372);
            Controls.Add(lblDark);
            Controls.Add(nudDark);
            Controls.Add(lblLight);
            Controls.Add(nudLight);
            Controls.Add(lblStrength);
            Controls.Add(trkStrength);
            Controls.Add(lblHue);
            Controls.Add(trkHue);
            Controls.Add(lblCount);
            Controls.Add(panelPreview);
            Controls.Add(btnOk);
            Controls.Add(btnCancel);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "PaletteRampForm";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Тени и блики";
            ((System.ComponentModel.ISupportInitialize)nudDark).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudLight).EndInit();
            ((System.ComponentModel.ISupportInitialize)trkStrength).EndInit();
            ((System.ComponentModel.ISupportInitialize)trkHue).EndInit();
            panelPreview.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        private Label lblDark, lblLight, lblStrength, lblHue, lblCount;
        private NumericUpDown nudDark, nudLight;
        private TrackBar trkStrength, trkHue;
        private Panel panelPreview;
        private UI.PalettePanel palPreview;
        private Button btnOk, btnCancel;
        private System.Windows.Forms.Timer previewTimer;
    }
}
