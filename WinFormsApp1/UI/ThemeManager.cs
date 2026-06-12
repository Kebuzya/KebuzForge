using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace WinFormsApp1.UI
{
    public enum AppTheme { Light, Dark, Hybrid }

    internal static class ThemeManager
    {

        static readonly Color C_Bg      = Color.FromArgb(30, 30, 30);
        static readonly Color C_Panel   = Color.FromArgb(40, 40, 42);
        static readonly Color C_Group   = Color.FromArgb(38, 38, 40);
        static readonly Color C_TabSel  = Color.FromArgb(45, 45, 50);
        static readonly Color C_TabIdle = Color.FromArgb(33, 33, 35);
        static readonly Color C_Input   = Color.FromArgb(55, 55, 58);
        static readonly Color C_Button  = Color.FromArgb(62, 62, 66);
        static readonly Color C_Strip   = Color.FromArgb(45, 45, 50);
        static readonly Color C_Border  = Color.FromArgb(80, 80, 88);
        static readonly Color C_Fore    = Color.FromArgb(210, 210, 210);
        static readonly Color C_Track   = Color.FromArgb(45, 45, 50);

        static readonly HashSet<string> Skip = new(StringComparer.Ordinal)
        {
            "picBefore",        "picAfter",
            "panelBeforeTitle", "panelAfterTitle",
            "lblBeforeTitle",   "lblAfterTitle",
            "btnDetachBefore",  "btnDetachAfter",
            "_pixelEditor",     "editorScroll",
            "flpFrames",
            "btnEditorColor",
        };

        static readonly HashSet<TabControl> _themedTabs = new();

        public static void Apply(Form form, AppTheme theme)
        {
            ToolStripRenderer rnd = theme == AppTheme.Light
                ? new LightMenuRenderer()
                : new DarkMenuRenderer();

            Walk(form, theme, rnd);
            form.Invalidate(true);
        }

        static void Walk(Control c, AppTheme theme, ToolStripRenderer rnd)
        {
            if (Skip.Contains(c.Name)) return;
            if (c.Tag is string tag && tag == "notheme") return;
            StyleControl(c, theme, rnd);
            foreach (Control child in c.Controls)
                Walk(child, theme, rnd);
        }

        static bool IsOnDarkSurface(Control c) =>
            c.Parent is not null &&
            c.Parent.BackColor != Color.Transparent &&
            c.Parent.BackColor.GetBrightness() < 0.5f;

        static void StyleControl(Control c, AppTheme theme, ToolStripRenderer rnd)
        {
            bool dark   = theme != AppTheme.Light;
            bool hybrid = theme == AppTheme.Hybrid;

            switch (c)
            {
                case Form f:
                    f.BackColor = dark ? C_Bg   : SystemColors.Control;
                    f.ForeColor = dark ? C_Fore : SystemColors.ControlText;
                    break;

                case MenuStrip ms:
                    ms.BackColor = dark ? C_Strip : SystemColors.MenuBar;
                    ms.ForeColor = dark ? C_Fore  : SystemColors.MenuText;
                    ms.Renderer  = rnd;
                    StyleTSItems(ms.Items, dark);
                    break;

                case ToolStrip ts when ts is not StatusStrip:
                    ts.BackColor = dark ? C_Strip : SystemColors.Control;
                    ts.ForeColor = dark ? C_Fore  : SystemColors.ControlText;
                    ts.Renderer  = rnd;
                    StyleTSItems(ts.Items, dark);
                    break;

                case StatusStrip ss:
                    ss.BackColor = dark ? C_Strip : SystemColors.Control;
                    ss.ForeColor = dark ? C_Fore  : SystemColors.ControlText;
                    ss.Renderer  = rnd;
                    StyleTSItems(ss.Items, dark);
                    break;

                case SplitContainer sc:
                    sc.BackColor = dark ? C_Bg : SystemColors.Control;
                    break;

                case TabControl tc:
                    tc.BackColor = dark ? C_Panel : SystemColors.Control;
                    tc.ForeColor = dark ? C_Fore  : SystemColors.ControlText;
                    if (dark)
                    {
                        if (_themedTabs.Add(tc))
                        {
                            tc.DrawItem += OnDrawTabItem;
                            tc.Paint    += OnTabControlPaint;
                        }
                        if (tc.DrawMode != TabDrawMode.OwnerDrawFixed)
                            try { tc.DrawMode = TabDrawMode.OwnerDrawFixed; } catch { }
                    }
                    else
                    {
                        if (_themedTabs.Remove(tc))
                        {
                            tc.DrawItem -= OnDrawTabItem;
                            tc.Paint    -= OnTabControlPaint;
                        }
                        if (tc.DrawMode != TabDrawMode.Normal)
                            try { tc.DrawMode = TabDrawMode.Normal; } catch { }
                    }
                    tc.Invalidate();
                    break;

                case TabPage tp:
                    tp.BackColor = dark && !hybrid ? C_Panel : SystemColors.Control;
                    tp.ForeColor = dark && !hybrid ? C_Fore  : SystemColors.ControlText;
                    break;

                case GroupBox gb:
                    gb.BackColor = dark && !hybrid ? C_Group : SystemColors.Control;
                    gb.ForeColor = dark && !hybrid ? C_Fore  : SystemColors.ControlText;
                    break;

                case FlowLayoutPanel fp:
                    fp.BackColor = dark ? C_Panel : SystemColors.Control;
                    fp.ForeColor = dark ? C_Fore  : SystemColors.ControlText;
                    break;

                case Panel p:
                    p.BackColor = dark ? C_Panel : SystemColors.Control;
                    p.ForeColor = dark ? C_Fore  : SystemColors.ControlText;
                    break;

                case UserControl uc:
                    uc.BackColor = dark && !hybrid ? C_Panel : SystemColors.Control;
                    uc.ForeColor = dark && !hybrid ? C_Fore  : SystemColors.ControlText;
                    break;

                case Label lbl:
                    lbl.BackColor = Color.Transparent;
                    lbl.ForeColor = dark && !hybrid ? C_Fore
                        : hybrid && IsOnDarkSurface(lbl) ? C_Fore
                        : SystemColors.ControlText;
                    break;

                case CheckBox chk:
                    if (dark && !hybrid)
                    {
                        chk.FlatStyle = FlatStyle.Flat;
                        chk.BackColor = C_Group;
                        chk.ForeColor = C_Fore;
                    }
                    else if (hybrid)
                    {
                        chk.FlatStyle = FlatStyle.Standard;
                        chk.BackColor = Color.Transparent;
                        chk.ForeColor = IsOnDarkSurface(chk) ? C_Fore : SystemColors.ControlText;
                    }
                    else
                    {
                        chk.FlatStyle = FlatStyle.Standard;
                        chk.BackColor = Color.Transparent;
                        chk.ForeColor = SystemColors.ControlText;
                    }
                    break;

                case RadioButton rb:
                    if (dark && !hybrid)
                    {
                        rb.FlatStyle = FlatStyle.Flat;
                        rb.BackColor = C_Group;
                        rb.ForeColor = C_Fore;
                    }
                    else if (hybrid)
                    {
                        rb.FlatStyle = FlatStyle.Standard;
                        rb.BackColor = Color.Transparent;
                        rb.ForeColor = IsOnDarkSurface(rb) ? C_Fore : SystemColors.ControlText;
                    }
                    else
                    {
                        rb.FlatStyle = FlatStyle.Standard;
                        rb.BackColor = Color.Transparent;
                        rb.ForeColor = SystemColors.ControlText;
                    }
                    break;

                case Button btn:
                    if (dark && !hybrid)
                    {
                        btn.FlatStyle = FlatStyle.Flat;
                        btn.FlatAppearance.BorderColor = C_Border;
                        btn.BackColor = C_Button;
                        btn.ForeColor = C_Fore;
                        btn.UseVisualStyleBackColor = false;
                    }
                    else
                    {
                        btn.FlatStyle = FlatStyle.Standard;
                        btn.BackColor = SystemColors.Control;
                        btn.ForeColor = SystemColors.ControlText;
                        btn.UseVisualStyleBackColor = true;
                    }
                    break;

                case TrackBar trk:
                    trk.BackColor = dark && !hybrid ? C_Track
                        : hybrid && IsOnDarkSurface(trk) ? C_Track
                        : SystemColors.Control;
                    break;

                case ComboBox cmb:
                    if (dark && !hybrid)
                    {
                        cmb.BackColor = C_Input;
                        cmb.ForeColor = C_Fore;
                    }
                    else
                    {
                        cmb.BackColor = SystemColors.Window;
                        cmb.ForeColor = SystemColors.WindowText;
                    }
                    break;

                case NumericUpDown nud:
                    if (dark && !hybrid)
                    {
                        nud.BackColor = C_Input;
                        nud.ForeColor = C_Fore;
                    }
                    else
                    {
                        nud.BackColor = SystemColors.Window;
                        nud.ForeColor = SystemColors.WindowText;
                    }
                    break;

                case TextBox txt:
                    txt.BackColor = dark && !hybrid ? C_Input : SystemColors.Window;
                    txt.ForeColor = dark && !hybrid ? C_Fore  : SystemColors.WindowText;
                    break;
            }
        }

        static void OnTabControlPaint(object? sender, PaintEventArgs e)
        {
            if (sender is not TabControl tc || tc.TabCount == 0) return;
            var lastTab = tc.GetTabRect(tc.TabCount - 1);
            if (lastTab.Right >= tc.Width) return;
            var gap = new Rectangle(lastTab.Right, 0, tc.Width - lastTab.Right, lastTab.Bottom + 1);
            using var br = new SolidBrush(C_TabIdle);
            e.Graphics.FillRectangle(br, gap);
        }

        static void OnDrawTabItem(object? sender, DrawItemEventArgs e)
        {
            if (sender is not TabControl tc) return;
            var tab  = tc.TabPages[e.Index];
            bool sel = e.Index == tc.SelectedIndex;
            var bg   = sel ? C_TabSel : C_TabIdle;

            using var bgBr = new SolidBrush(bg);
            e.Graphics.FillRectangle(bgBr, e.Bounds);

            if (sel)
            {
                using var accentPen = new Pen(Color.FromArgb(100, 140, 200), 2);
                e.Graphics.DrawLine(accentPen,
                    e.Bounds.Left,  e.Bounds.Top,
                    e.Bounds.Right, e.Bounds.Top);
            }
            else
            {
                using var borderPen = new Pen(C_Border);
                e.Graphics.DrawRectangle(borderPen,
                    e.Bounds.X, e.Bounds.Y, e.Bounds.Width - 1, e.Bounds.Height - 1);
            }

            using var fgBr = new SolidBrush(C_Fore);
            var sf = new StringFormat
            {
                Alignment     = StringAlignment.Center,
                LineAlignment = StringAlignment.Center,
            };
            e.Graphics.DrawString(tab.Text, tc.Font, fgBr, e.Bounds, sf);
        }

        static void StyleTSItems(ToolStripItemCollection items, bool dark)
        {
            foreach (ToolStripItem item in items)
            {
                if (item.Name == "btnEditorColor") continue;
                item.BackColor = dark ? C_Strip : SystemColors.Control;
                item.ForeColor = dark ? C_Fore  : SystemColors.ControlText;
                if (item is ToolStripMenuItem mi)
                    StyleTSItems(mi.DropDownItems, dark);
            }
        }

        sealed class LightMenuRenderer : ToolStripProfessionalRenderer
        {
            protected override void OnRenderItemCheck(ToolStripItemImageRenderEventArgs e)
                => DrawThinCheck(e.Graphics, e.ImageRectangle, SystemColors.ControlText);
        }

        sealed class DarkMenuRenderer : ToolStripProfessionalRenderer
        {
            public DarkMenuRenderer() : base(new DarkColors()) { }

            protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
            {
                if (e.Item.Name == "btnEditorColor") { base.OnRenderItemText(e); return; }
                e.TextColor = e.Item.Enabled ? C_Fore : Color.FromArgb(100, 100, 100);
                base.OnRenderItemText(e);
            }

            protected override void OnRenderItemCheck(ToolStripItemImageRenderEventArgs e)
                => DrawThinCheck(e.Graphics, e.ImageRectangle, C_Fore);
        }

        static void DrawThinCheck(Graphics g, Rectangle r, Color color)
        {
            using var f  = new Font("Segoe UI", 8.5f, FontStyle.Regular, GraphicsUnit.Point);
            using var br = new SolidBrush(color);
            const string ch = "✓";
            var sz = g.MeasureString(ch, f);
            g.DrawString(ch, f, br,
                r.X + (r.Width  - sz.Width)  / 2f,
                r.Y + (r.Height - sz.Height) / 2f);
        }

        sealed class DarkColors : ProfessionalColorTable
        {
            static readonly Color S  = Color.FromArgb(45, 45, 50);
            static readonly Color H  = Color.FromArgb(60, 90, 130);
            static readonly Color D  = Color.FromArgb(48, 48, 52);
            static readonly Color Sp = Color.FromArgb(68, 68, 76);

            public override Color MenuStripGradientBegin             => S;
            public override Color MenuStripGradientEnd               => S;
            public override Color ToolStripGradientBegin             => S;
            public override Color ToolStripGradientEnd               => S;
            public override Color ToolStripGradientMiddle            => S;
            public override Color ToolStripBorder                    => Sp;
            public override Color ToolStripContentPanelGradientBegin => S;
            public override Color ToolStripContentPanelGradientEnd   => S;
            public override Color ToolStripDropDownBackground        => D;
            public override Color ImageMarginGradientBegin           => D;
            public override Color ImageMarginGradientEnd             => D;
            public override Color ImageMarginGradientMiddle          => D;
            public override Color MenuBorder                         => Sp;
            public override Color MenuItemBorder                     => H;
            public override Color MenuItemSelected                   => H;
            public override Color MenuItemSelectedGradientBegin      => H;
            public override Color MenuItemSelectedGradientEnd        => H;
            public override Color MenuItemPressedGradientBegin       => H;
            public override Color MenuItemPressedGradientEnd         => H;
            public override Color MenuItemPressedGradientMiddle      => H;
            public override Color ButtonCheckedGradientBegin         => H;
            public override Color ButtonCheckedGradientEnd           => H;
            public override Color ButtonCheckedGradientMiddle        => H;
            public override Color ButtonSelectedGradientBegin        => H;
            public override Color ButtonSelectedGradientEnd          => H;
            public override Color ButtonSelectedGradientMiddle       => H;
            public override Color ButtonSelectedBorder               => H;
            public override Color ButtonSelectedHighlight            => H;
            public override Color ButtonSelectedHighlightBorder      => H;
            public override Color ButtonPressedBorder                => H;
            public override Color CheckBackground                    => H;
            public override Color CheckPressedBackground             => H;
            public override Color CheckSelectedBackground            => H;
            public override Color SeparatorDark                      => Sp;
            public override Color SeparatorLight                     => Sp;
        }
    }
}
