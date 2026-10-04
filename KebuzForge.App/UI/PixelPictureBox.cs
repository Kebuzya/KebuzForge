using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace KebuzForge.App.UI
{

    internal class PixelPictureBox : PictureBox
    {
        protected override void OnPaint(PaintEventArgs pe)
        {
            pe.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
            pe.Graphics.PixelOffsetMode   = PixelOffsetMode.Half;
            base.OnPaint(pe);
        }
    }
}
