// ThemeManager.cs
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace SecureNotes
{
    public enum Theme { Light, Dark, Ocean, Forest, Sunset }

    public static class ThemeManager
    {
        public static Color Accent = Color.FromArgb(59, 130, 246);
        public static Color AccentHover = Color.FromArgb(37, 99, 235);
        public static Color Danger = Color.FromArgb(239, 68, 68);
        public static Color Success = Color.FromArgb(34, 197, 94);

        public static Color GetBg(Theme theme)
        {
            switch (theme)
            {
                case Theme.Dark: return Color.FromArgb(17, 17, 21);
                case Theme.Ocean: return Color.FromArgb(15, 23, 42);
                case Theme.Forest: return Color.FromArgb(20, 26, 20);
                case Theme.Sunset: return Color.FromArgb(30, 20, 20);
                default: return Color.FromArgb(250, 251, 252);
            }
        }

        public static Color GetSurface(Theme theme)
        {
            switch (theme)
            {
                case Theme.Dark: return Color.FromArgb(28, 28, 35);
                case Theme.Ocean: return Color.FromArgb(30, 41, 59);
                case Theme.Forest: return Color.FromArgb(34, 45, 34);
                case Theme.Sunset: return Color.FromArgb(45, 30, 30);
                default: return Color.White;
            }
        }

        public static Color GetSurfaceHover(Theme theme)
        {
            switch (theme)
            {
                case Theme.Dark: return Color.FromArgb(38, 38, 45);
                case Theme.Ocean: return Color.FromArgb(51, 65, 85);
                case Theme.Forest: return Color.FromArgb(45, 60, 45);
                case Theme.Sunset: return Color.FromArgb(60, 40, 40);
                default: return Color.FromArgb(243, 244, 246);
            }
        }

        public static Color GetTextColor(Theme theme)
        {
            switch (theme)
            {
                case Theme.Dark:
                case Theme.Ocean:
                case Theme.Forest:
                case Theme.Sunset:
                    return Color.FromArgb(245, 245, 250);
                default:
                    return Color.FromArgb(17, 24, 39);
            }
        }

        public static Color GetTextSecondary(Theme theme)
        {
            switch (theme)
            {
                case Theme.Dark:
                case Theme.Ocean:
                case Theme.Forest:
                case Theme.Sunset:
                    return Color.FromArgb(156, 163, 175);
                default:
                    return Color.FromArgb(107, 114, 128);
            }
        }

        public static Color GetBorder(Theme theme)
        {
            switch (theme)
            {
                case Theme.Dark: return Color.FromArgb(55, 55, 65);
                case Theme.Ocean: return Color.FromArgb(71, 85, 105);
                case Theme.Forest: return Color.FromArgb(55, 75, 55);
                case Theme.Sunset: return Color.FromArgb(80, 55, 55);
                default: return Color.FromArgb(229, 231, 235);
            }
        }

        public static Color GetInputColor(Theme theme)
        {
            switch (theme)
            {
                case Theme.Dark: return Color.FromArgb(38, 38, 45);
                case Theme.Ocean: return Color.FromArgb(51, 65, 85);
                case Theme.Forest: return Color.FromArgb(45, 60, 45);
                case Theme.Sunset: return Color.FromArgb(55, 35, 35);
                default: return Color.White;
            }
        }

        public static Color GetAccent(Theme theme)
        {
            switch (theme)
            {
                case Theme.Ocean: return Color.FromArgb(56, 189, 248);
                case Theme.Forest: return Color.FromArgb(74, 222, 128);
                case Theme.Sunset: return Color.FromArgb(251, 146, 60);
                default: return Accent;
            }
        }

        public static string GetThemeName(Theme theme)
        {
            switch (theme)
            {
                case Theme.Dark: return "Dark";
                case Theme.Ocean: return "Ocean";
                case Theme.Forest: return "Forest";
                case Theme.Sunset: return "Sunset";
                default: return "Light";
            }
        }

        public static Theme FromString(string name)
        {
            if (string.IsNullOrEmpty(name)) return Theme.Light;
            switch (name.ToLower())
            {
                case "dark": return Theme.Dark;
                case "ocean": return Theme.Ocean;
                case "forest": return Theme.Forest;
                case "sunset": return Theme.Sunset;
                default: return Theme.Light;
            }
        }

        public static void Apply(Form f, Theme theme)
        {
            f.BackColor = GetBg(theme);
            f.ForeColor = GetTextColor(theme);
            foreach (Control c in f.Controls) ApplyControl(c, theme);
        }

        private static void ApplyControl(Control c, Theme theme)
        {
            var bg = GetBg(theme);
            var surface = GetSurface(theme);
            var textColor = GetTextColor(theme);
            var textSecondary = GetTextSecondary(theme);
            var inputColor = GetInputColor(theme);

            if (c.Tag?.ToString() == "transparent")
            {
                c.BackColor = bg;
            }
            else if (c is Panel || c is FlowLayoutPanel)
            {
                c.BackColor = surface;
            }
            else if (c is ListBox lb)
            {
                lb.BackColor = surface;
                lb.ForeColor = textColor;
                lb.BorderStyle = BorderStyle.None;
            }

            c.ForeColor = textColor;

            if (c is TextBox tb)
            {
                tb.BackColor = inputColor;
                tb.ForeColor = UIHelpers.IsPlaceholder(tb) ? textSecondary : textColor;
                tb.BorderStyle = BorderStyle.FixedSingle;
            }

            if (c is RichTextBox rtb)
            {
                rtb.BackColor = inputColor;
                rtb.ForeColor = textColor;
                rtb.BorderStyle = BorderStyle.None;
            }

            if (c is ComboBox cb)
            {
                cb.BackColor = inputColor;
                cb.ForeColor = textColor;
                cb.FlatStyle = FlatStyle.Flat;
            }

            if (c is Label lbl)
            {
                lbl.ForeColor = lbl.Tag?.ToString() == "secondary" ? textSecondary : textColor;
            }

            if (c is CheckBox chk)
            {
                chk.ForeColor = textColor;
            }

            if (c is RadioButton rb)
            {
                rb.ForeColor = textColor;
            }

            foreach (Control child in c.Controls) ApplyControl(child, theme);
        }

        public static void StyleButton(Button btn, Theme theme)
        {
            btn.FlatStyle = FlatStyle.Flat;
            btn.BackColor = GetSurface(theme);
            btn.ForeColor = GetTextColor(theme);
            btn.FlatAppearance.BorderColor = GetBorder(theme);
            btn.FlatAppearance.BorderSize = 1;
            btn.FlatAppearance.MouseOverBackColor = GetSurfaceHover(theme);
            btn.Cursor = Cursors.Hand;
        }

        public static void StyleAccentButton(Button btn)
        {
            var accent = GetAccent(Program.CurrentTheme);
            btn.FlatStyle = FlatStyle.Flat;
            btn.BackColor = accent;
            btn.ForeColor = Color.White;
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = ControlPaint.Dark(accent, 0.1f);
            btn.Cursor = Cursors.Hand;
        }

        public static void StyleGhostButton(Button btn, Theme theme)
        {
            btn.FlatStyle = FlatStyle.Flat;
            btn.BackColor = Color.Transparent;
            btn.ForeColor = GetTextColor(theme);
            btn.FlatAppearance.BorderColor = GetBorder(theme);
            btn.FlatAppearance.BorderSize = 1;
            btn.FlatAppearance.MouseOverBackColor = GetSurfaceHover(theme);
            btn.Cursor = Cursors.Hand;
        }

        public static void StyleDangerButton(Button btn, Theme theme)
        {
            btn.FlatStyle = FlatStyle.Flat;
            btn.BackColor = Color.FromArgb(30, Danger);
            btn.ForeColor = Danger;
            btn.FlatAppearance.BorderColor = Danger;
            btn.FlatAppearance.BorderSize = 1;
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(50, Danger);
            btn.Cursor = Cursors.Hand;
        }

        public static void StyleSidebarButton(Button btn, Theme theme, bool isActive)
        {
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.Cursor = Cursors.Hand;

            if (isActive)
            {
                btn.BackColor = Color.FromArgb(40, GetAccent(theme));
                btn.ForeColor = GetAccent(theme);
            }
            else
            {
                btn.BackColor = Color.Transparent;
                btn.ForeColor = GetTextSecondary(theme);
                btn.FlatAppearance.MouseOverBackColor = GetSurfaceHover(theme);
            }
        }

        public static GraphicsPath GetRoundedRect(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();
            int d = radius * 2;
            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}