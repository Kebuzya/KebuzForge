namespace KebuzForge.App.Forms
{
    partial class ScreenPickerForm
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
            SuspendLayout();

            AutoScaleMode = AutoScaleMode.None;
            ClientSize = new Size(300, 200);
            Cursor = Cursors.Cross;
            DoubleBuffered = true;
            FormBorderStyle = FormBorderStyle.None;
            KeyPreview = true;
            Name = "ScreenPickerForm";
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.Manual;
            Text = "Пипетка";
            TopMost = true;
            ResumeLayout(false);
        }
    }
}
