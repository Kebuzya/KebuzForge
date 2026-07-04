using System.Drawing;
using System.Windows.Forms;

namespace KebuzForge.App.UI
{

    internal class StableScrollPanel : Panel
    {
        protected override Point ScrollToControl(Control activeControl)
        {
            return DisplayRectangle.Location;
        }
    }
}
