namespace KebuzForge.App.Forms
{
    partial class NewCanvasForm
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
            lblWidth = new Label();
            nudWidth = new NumericUpDown();
            lblHeight = new Label();
            nudHeight = new NumericUpDown();
            btnPresetSmall = new Button();
            btnPresetMedium = new Button();
            btnPresetLarge = new Button();
            lblBackground = new Label();
            rbTransparent = new RadioButton();
            rbColor = new RadioButton();
            btnBgColor = new Button();
            btnOk = new Button();
            btnCancel = new Button();
            ((System.ComponentModel.ISupportInitialize)nudWidth).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudHeight).BeginInit();
            SuspendLayout();

            lblWidth.AutoSize = true;
            lblWidth.Location = new Point(16, 20);
            lblWidth.Name = "lblWidth";
            lblWidth.Size = new Size(52, 15);
            lblWidth.TabIndex = 0;
            lblWidth.Text = "Ширина";

            nudWidth.Location = new Point(96, 16);
            nudWidth.Maximum = new decimal(new int[] { 4096, 0, 0, 0 });
            nudWidth.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            nudWidth.Name = "nudWidth";
            nudWidth.Size = new Size(80, 23);
            nudWidth.TabIndex = 1;
            nudWidth.Value = new decimal(new int[] { 512, 0, 0, 0 });

            lblHeight.AutoSize = true;
            lblHeight.Location = new Point(16, 52);
            lblHeight.Name = "lblHeight";
            lblHeight.Size = new Size(46, 15);
            lblHeight.TabIndex = 2;
            lblHeight.Text = "Высота";

            nudHeight.Location = new Point(96, 48);
            nudHeight.Maximum = new decimal(new int[] { 4096, 0, 0, 0 });
            nudHeight.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            nudHeight.Name = "nudHeight";
            nudHeight.Size = new Size(80, 23);
            nudHeight.TabIndex = 3;
            nudHeight.Value = new decimal(new int[] { 512, 0, 0, 0 });

            btnPresetSmall.Location = new Point(192, 30);
            btnPresetSmall.Name = "btnPresetSmall";
            btnPresetSmall.Size = new Size(56, 28);
            btnPresetSmall.TabIndex = 4;
            btnPresetSmall.Text = "256";

            btnPresetMedium.Location = new Point(254, 30);
            btnPresetMedium.Name = "btnPresetMedium";
            btnPresetMedium.Size = new Size(56, 28);
            btnPresetMedium.TabIndex = 5;
            btnPresetMedium.Text = "512";

            btnPresetLarge.Location = new Point(316, 30);
            btnPresetLarge.Name = "btnPresetLarge";
            btnPresetLarge.Size = new Size(56, 28);
            btnPresetLarge.TabIndex = 6;
            btnPresetLarge.Text = "1024";

            lblBackground.AutoSize = true;
            lblBackground.Location = new Point(16, 92);
            lblBackground.Name = "lblBackground";
            lblBackground.Size = new Size(30, 15);
            lblBackground.TabIndex = 7;
            lblBackground.Text = "Фон";

            rbTransparent.AutoSize = true;
            rbTransparent.Checked = true;
            rbTransparent.Location = new Point(96, 90);
            rbTransparent.Name = "rbTransparent";
            rbTransparent.Size = new Size(94, 19);
            rbTransparent.TabIndex = 8;
            rbTransparent.TabStop = true;
            rbTransparent.Text = "Прозрачный";

            rbColor.AutoSize = true;
            rbColor.Location = new Point(96, 118);
            rbColor.Name = "rbColor";
            rbColor.Size = new Size(52, 19);
            rbColor.TabIndex = 9;
            rbColor.Text = "Цвет";

            btnBgColor.BackColor = Color.Black;
            btnBgColor.FlatStyle = FlatStyle.Flat;
            btnBgColor.Location = new Point(192, 114);
            btnBgColor.Name = "btnBgColor";
            btnBgColor.Size = new Size(56, 26);
            btnBgColor.TabIndex = 10;
            btnBgColor.Tag = "notheme";
            btnBgColor.UseVisualStyleBackColor = false;

            btnOk.DialogResult = DialogResult.OK;
            btnOk.Location = new Point(196, 160);
            btnOk.Name = "btnOk";
            btnOk.Size = new Size(84, 30);
            btnOk.TabIndex = 11;
            btnOk.Text = "Создать";

            btnCancel.DialogResult = DialogResult.Cancel;
            btnCancel.Location = new Point(288, 160);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(84, 30);
            btnCancel.TabIndex = 12;
            btnCancel.Text = "Отмена";

            AcceptButton = btnOk;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnCancel;
            ClientSize = new Size(388, 206);
            Controls.Add(lblWidth);
            Controls.Add(nudWidth);
            Controls.Add(lblHeight);
            Controls.Add(nudHeight);
            Controls.Add(btnPresetSmall);
            Controls.Add(btnPresetMedium);
            Controls.Add(btnPresetLarge);
            Controls.Add(lblBackground);
            Controls.Add(rbTransparent);
            Controls.Add(rbColor);
            Controls.Add(btnBgColor);
            Controls.Add(btnOk);
            Controls.Add(btnCancel);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "NewCanvasForm";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Новый холст";
            ((System.ComponentModel.ISupportInitialize)nudWidth).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudHeight).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        private Label lblWidth, lblHeight, lblBackground;
        private NumericUpDown nudWidth, nudHeight;
        private Button btnPresetSmall, btnPresetMedium, btnPresetLarge;
        private RadioButton rbTransparent, rbColor;
        private Button btnBgColor, btnOk, btnCancel;
    }
}
