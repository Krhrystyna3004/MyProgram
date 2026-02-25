using System;
using System.Drawing;
using System.Windows.Forms;

namespace SecureNotes
{
    public partial class SettingsForm : Form
    {
        private readonly UserAccountService _accountService = new UserAccountService();

        private ComboBox cmbTheme, cmbLanguage;
        private Panel previewPanel;
        private Button btnSwitchAccount, btnChangePassword, btnDeleteAccount, btnSave;
        private Label lblTitle, lblLanguage, lblTheme, lblPreview, lblAccount;

        public SettingsForm()
        {
            InitializeModernUI();
            ThemeManager.Apply(this, Program.CurrentTheme);
            LocalizationManager.LanguageChanged += ApplyLocalization;
            FormClosed += (s, e) => LocalizationManager.LanguageChanged -= ApplyLocalization;
        }

        private void InitializeModernUI()
        {
            Text = LocalizationManager.Get("settings");
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(480, 650);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;

            int padding = 28;
            int y = padding;
            int fieldWidth = ClientSize.Width - padding * 2;

            lblTitle = new Label
            {
                Text = LocalizationManager.Get("settings"),
                Font = new Font("Segoe UI Semibold", 18f),
                Location = new Point(padding, y),
                AutoSize = true
            };
            Controls.Add(lblTitle);
            y += 50;

            // === МОВА ===
            lblLanguage = new Label
            {
                Text = LocalizationManager.Get("language"),
                Font = new Font("Segoe UI Semibold", 11f),
                Location = new Point(padding, y),
                AutoSize = true
            };
            Controls.Add(lblLanguage);
            y += 30;

            cmbLanguage = new ComboBox
            {
                Location = new Point(padding, y),
                Size = new Size(fieldWidth, 36),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 11f)
            };

            foreach (Language lang in Enum.GetValues(typeof(Language)))
            {
                cmbLanguage.Items.Add(new LanguageItem(lang, LocalizationManager.GetLanguageName(lang)));
            }
            cmbLanguage.SelectedIndex = (int)LocalizationManager.CurrentLanguage;
            cmbLanguage.SelectedIndexChanged += CmbLanguage_SelectedIndexChanged;
            Controls.Add(cmbLanguage);
            y += 50;

            // === ТЕМА ===
            lblTheme = new Label
            {
                Text = LocalizationManager.Get("theme_settings"),
                Font = new Font("Segoe UI Semibold", 11f),
                Location = new Point(padding, y),
                AutoSize = true
            };
            Controls.Add(lblTheme);
            y += 30;

            cmbTheme = new ComboBox
            {
                Location = new Point(padding, y),
                Size = new Size(fieldWidth, 36),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 11f),
                DrawMode = DrawMode.OwnerDrawFixed,
                ItemHeight = 32
            };
            cmbTheme.DrawItem += CmbTheme_DrawItem;

            var themes = new ThemeItem[]
            {
                new ThemeItem("Light", LocalizationManager.Get("theme_light"), Color.FromArgb(250, 251, 252), ThemeManager.Accent),
                new ThemeItem("Dark", LocalizationManager.Get("theme_dark"), Color.FromArgb(17, 17, 21), ThemeManager.Accent),
                new ThemeItem("Ocean", LocalizationManager.Get("theme_ocean"), Color.FromArgb(15, 23, 42), Color.FromArgb(56, 189, 248)),
                new ThemeItem("Forest", LocalizationManager.Get("theme_forest"), Color.FromArgb(20, 26, 20), Color.FromArgb(74, 222, 128)),
                new ThemeItem("Sunset", LocalizationManager.Get("theme_sunset"), Color.FromArgb(30, 20, 20), Color.FromArgb(251, 146, 60))
            };

            foreach (var t in themes) cmbTheme.Items.Add(t);

            var currentThemeName = ThemeManager.GetThemeName(Program.CurrentTheme);
            for (int i = 0; i < themes.Length; i++)
            {
                if (themes[i].Value == currentThemeName)
                {
                    cmbTheme.SelectedIndex = i;
                    break;
                }
            }

            cmbTheme.SelectedIndexChanged += (s, e) => UpdatePreview();
            Controls.Add(cmbTheme);
            y += 50;

            lblPreview = new Label
            {
                Text = LocalizationManager.Get("preview"),
                Font = new Font("Segoe UI", 9.5f),
                Location = new Point(padding, y),
                AutoSize = true,
                Tag = "secondary"
            };
            Controls.Add(lblPreview);
            y += 26;

            previewPanel = new Panel
            {
                Location = new Point(padding, y),
                Size = new Size(fieldWidth, 100),
                BorderStyle = BorderStyle.FixedSingle
            };
            Controls.Add(previewPanel);
            y += 120;

            // === АКАУНТ ===
            lblAccount = new Label
            {
                Text = LocalizationManager.Get("account"),
                Font = new Font("Segoe UI Semibold", 11f),
                Location = new Point(padding, y),
                AutoSize = true
            };
            Controls.Add(lblAccount);
            y += 35;

            btnSwitchAccount = new Button
            {
                Text = LocalizationManager.Get("change_account"),
                Location = new Point(padding, y),
                Size = new Size(fieldWidth, 44),
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                Cursor = Cursors.Hand,
                FlatStyle = FlatStyle.Flat
            };
            btnSwitchAccount.Click += BtnSwitchAccount_Click;
            Controls.Add(btnSwitchAccount);
            y += 54;

            btnChangePassword = new Button
            {
                Text = LocalizationManager.Get("change_password"),
                Location = new Point(padding, y),
                Size = new Size(fieldWidth, 44),
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                Cursor = Cursors.Hand,
                FlatStyle = FlatStyle.Flat
            };
            btnChangePassword.Click += BtnChangePassword_Click;
            Controls.Add(btnChangePassword);
            y += 54;

            btnDeleteAccount = new Button
            {
                Text = LocalizationManager.Get("delete_account"),
                Location = new Point(padding, y),
                Size = new Size(fieldWidth, 44),
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                Cursor = Cursors.Hand,
                FlatStyle = FlatStyle.Flat
            };
            btnDeleteAccount.Click += BtnDeleteAccount_Click;
            Controls.Add(btnDeleteAccount);
            y += 60;

            btnSave = new Button
            {
                Text = LocalizationManager.Get("save"),
                Location = new Point(padding, y),
                Size = new Size(fieldWidth, 48),
                Font = new Font("Segoe UI Semibold", 11f),
                Cursor = Cursors.Hand
            };
            btnSave.Click += BtnSave_Click;
            Controls.Add(btnSave);

            this.Load += (s, e) =>
            {
                ThemeManager.StyleAccentButton(btnSave);
                ThemeManager.StyleGhostButton(btnSwitchAccount, Program.CurrentTheme);
                btnSwitchAccount.FlatAppearance.BorderColor = ThemeManager.GetBorder(Program.CurrentTheme);
                btnSwitchAccount.FlatAppearance.BorderSize = 1;
                ThemeManager.StyleGhostButton(btnChangePassword, Program.CurrentTheme);
                btnChangePassword.FlatAppearance.BorderColor = ThemeManager.GetBorder(Program.CurrentTheme);
                btnChangePassword.FlatAppearance.BorderSize = 1;
                ThemeManager.StyleDangerButton(btnDeleteAccount, Program.CurrentTheme);
                UpdatePreview();
            };
            ApplyLocalization();
        }

        private void CmbLanguage_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbLanguage.SelectedItem is LanguageItem item)
            {
                LocalizationManager.CurrentLanguage = item.Value;
                Program.SaveCurrentPreferences();
                ApplyLocalization();

                UpdatePreview();
            }
        }

        private void ApplyLocalization()
        {
            Text = LocalizationManager.Get("settings");
            lblTitle.Text = LocalizationManager.Get("settings");
            lblLanguage.Text = LocalizationManager.Get("language");
            lblTheme.Text = LocalizationManager.Get("theme_settings");
            lblPreview.Text = LocalizationManager.Get("preview");
            lblAccount.Text = LocalizationManager.Get("account");
            btnSave.Text = LocalizationManager.Get("save");
            btnSwitchAccount.Text = LocalizationManager.Get("change_account");
            btnChangePassword.Text = LocalizationManager.Get("change_password");
            btnDeleteAccount.Text = LocalizationManager.Get("delete_account");

            var currentThemeIndex = cmbTheme.SelectedIndex;
            cmbTheme.Items.Clear();
            var themes = new ThemeItem[]
            {
                new ThemeItem("Light", LocalizationManager.Get("theme_light"), Color.FromArgb(250, 251, 252), ThemeManager.Accent),
                new ThemeItem("Dark", LocalizationManager.Get("theme_dark"), Color.FromArgb(17, 17, 21), ThemeManager.Accent),
                new ThemeItem("Ocean", LocalizationManager.Get("theme_ocean"), Color.FromArgb(15, 23, 42), Color.FromArgb(56, 189, 248)),
                new ThemeItem("Forest", LocalizationManager.Get("theme_forest"), Color.FromArgb(20, 26, 20), Color.FromArgb(74, 222, 128)),
                new ThemeItem("Sunset", LocalizationManager.Get("theme_sunset"), Color.FromArgb(30, 20, 20), Color.FromArgb(251, 146, 60))
            };

            foreach (var theme in themes)
            {
                cmbTheme.Items.Add(theme);
            }

            if (currentThemeIndex >= 0 && currentThemeIndex < cmbTheme.Items.Count)
            {
                cmbTheme.SelectedIndex = currentThemeIndex;
            }

            UpdatePreview();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // SettingsForm
            // 
            this.ClientSize = new System.Drawing.Size(282, 253);
            this.Name = "SettingsForm";
            this.Load += new System.EventHandler(this.SettingsForm_Load);
            this.ResumeLayout(false);

        }

        private void SettingsForm_Load(object sender, EventArgs e)
        {

        }

        private void CmbTheme_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;

            e.DrawBackground();

            var item = (ThemeItem)cmbTheme.Items[e.Index];
            var rect = e.Bounds;

            var bgRect = new Rectangle(rect.X + 8, rect.Y + 6, 20, rect.Height - 12);
            using (var brush = new SolidBrush(item.BgColor))
            {
                e.Graphics.FillRectangle(brush, bgRect);
            }
            using (var pen = new Pen(Color.FromArgb(160, 160, 160)))
            {
                e.Graphics.DrawRectangle(pen, bgRect);
            }

            var accentRect = new Rectangle(bgRect.Right + 4, rect.Y + 6, 20, rect.Height - 12);
            using (var brush = new SolidBrush(item.AccentColor))
            {
                e.Graphics.FillRectangle(brush, accentRect);
            }
            using (var pen = new Pen(Color.FromArgb(160, 160, 160)))
            {
                e.Graphics.DrawRectangle(pen, accentRect);
            }

            var textRect = new Rectangle(accentRect.Right + 12, rect.Y, rect.Width - accentRect.Right - 12, rect.Height);
            var textColor = (e.State & DrawItemState.Selected) == DrawItemState.Selected
                ? SystemColors.HighlightText
                : ThemeManager.GetTextColor(Program.CurrentTheme);

            TextRenderer.DrawText(e.Graphics, item.DisplayName, new Font("Segoe UI", 11f), textRect, textColor,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter);

            e.DrawFocusRectangle();
        }

        private void UpdatePreview()
        {
            if (cmbTheme.SelectedItem == null) return;

            var item = (ThemeItem)cmbTheme.SelectedItem;
            var theme = ThemeManager.FromString(item.Value);

            previewPanel.BackColor = ThemeManager.GetBg(theme);
            previewPanel.Controls.Clear();

            var lblSample = new Label
            {
                Text = LocalizationManager.Get("sample_text"),
                Font = new Font("Segoe UI", 10f),
                ForeColor = ThemeManager.GetTextColor(theme),
                Location = new Point(12, 12),
                AutoSize = true
            };
            previewPanel.Controls.Add(lblSample);

            var lblSecondary = new Label
            {
                Text = LocalizationManager.Get("secondary_text"),
                Font = new Font("Segoe UI", 9f),
                ForeColor = ThemeManager.GetTextSecondary(theme),
                Location = new Point(12, 36),
                AutoSize = true
            };
            previewPanel.Controls.Add(lblSecondary);

            var btnSample = new Button
            {
                Text = LocalizationManager.Get("button"),
                Location = new Point(12, 60),
                Size = new Size(80, 30),
                FlatStyle = FlatStyle.Flat,
                BackColor = ThemeManager.GetAccent(theme),
                ForeColor = Color.White
            };
            btnSample.FlatAppearance.BorderSize = 0;
            previewPanel.Controls.Add(btnSample);
        }

        private void BtnSwitchAccount_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show(
                LocalizationManager.Get("logout_confirm"),
                 LocalizationManager.Get("change_account"),
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (result == DialogResult.Yes)
            {
                Program.SwitchAccount();
                this.DialogResult = DialogResult.Abort;
                this.Close();

                if (this.Owner != null)
                {
                    this.Owner.Close();
                }
            }
        }

        private void BtnChangePassword_Click(object sender, EventArgs e)
        {
            using (var form = new ChangePasswordForm())
            {
                form.ShowDialog(this);
            }
        }

        private void BtnDeleteAccount_Click(object sender, EventArgs e)
        {
            using (var form = new DeleteAccountForm())
            {
                form.ShowDialog(this);
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            Program.TouchActivity();

            if (cmbTheme.SelectedItem is ThemeItem item)
            {
                var newTheme = ThemeManager.FromString(item.Value);
                Program.CurrentTheme = newTheme;
                Program.CurrentUser.PreferredTheme = item.Value;

                try
                {
                    _accountService.UpdateTheme(Program.CurrentUser.Id, item.Value);
                }
                catch { }
            }

            DialogResult = DialogResult.OK;
            Close();
        }

        private class ThemeItem
        {
            public string Value { get; }
            public string DisplayName { get; }
            public Color BgColor { get; }
            public Color AccentColor { get; }

            public ThemeItem(string value, string displayName, Color bgColor, Color accentColor)
            {
                Value = value;
                DisplayName = displayName;
                BgColor = bgColor;
                AccentColor = accentColor;
            }

            public override string ToString() { return DisplayName; }
        }

        private class LanguageItem
        {
            public Language Value { get; }
            public string DisplayName { get; }

            public LanguageItem(Language value, string displayName)
            {
                Value = value;
                DisplayName = displayName;
            }

            public override string ToString() { return DisplayName; }
        }
    }
}
