using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace SecureNotes
{
    public class LoginForm : Form
    {
        private TextBox txtUsername;
        private TextBox txtPassword;
        private Button btnLogin;
        private Button btnRegister;
        private Label lblTitle;
        private Label lblSubtitle;
        private Panel cardPanel;
        private DatabaseHelper _db = new DatabaseHelper(AppConfig.ConnStr);

        public User LoggedInUser { get; private set; }

        public LoginForm()
        {
            InitializeModernUI();
        }

        private void InitializeModernUI()
        {
            Text = "SecureNotes";
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(440, 520);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            BackColor = ThemeManager.GetBg(Theme.Dark);

            // Card panel
            cardPanel = new Panel
            {
                Size = new Size(380, 420),
                Location = new Point(30, 50),
                BackColor = ThemeManager.GetSurface(Theme.Dark)
            };
            cardPanel.Paint += (s, e) =>
            {
                var rect = new Rectangle(0, 0, cardPanel.Width - 1, cardPanel.Height - 1);
                using var path = ThemeManager.GetRoundedRect(rect, 16);
                using var brush = new SolidBrush(ThemeManager.GetSurface(Theme.Dark));
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.FillPath(brush, path);
            };

            int padding = 32;
            int y = padding;

            // Logo / Title
            lblTitle = new Label
            {
                Text = "SecureNotes",
                Font = new Font("Segoe UI Semibold", 24f),
                ForeColor = ThemeManager.Accent,
                Location = new Point(padding, y),
                AutoSize = true
            };
            cardPanel.Controls.Add(lblTitle);
            y += 50;

            lblSubtitle = new Label
            {
                Text = "Безпечне зберігання нотаток та паролів",
                Font = new Font("Segoe UI", 10f),
                ForeColor = ThemeManager.GetTextSecondary(Theme.Dark),
                Location = new Point(padding, y),
                AutoSize = true
            };
            cardPanel.Controls.Add(lblSubtitle);
            y += 50;

            // Username
            var lblUsername = new Label
            {
                Text = "Логін",
                Font = new Font("Segoe UI", 9.5f),
                ForeColor = ThemeManager.GetTextColor(Theme.Dark),
                Location = new Point(padding, y),
                AutoSize = true
            };
            cardPanel.Controls.Add(lblUsername);
            y += 24;

            txtUsername = new TextBox
            {
                Location = new Point(padding, y),
                Size = new Size(316, 36),
                Font = new Font("Segoe UI", 11f),
                BackColor = ThemeManager.GetInputColor(Theme.Dark),
                ForeColor = ThemeManager.GetTextColor(Theme.Dark),
                BorderStyle = BorderStyle.FixedSingle
            };
            cardPanel.Controls.Add(txtUsername);
            y += 50;

            // Password
            var lblPassword = new Label
            {
                Text = "Пароль",
                Font = new Font("Segoe UI", 9.5f),
                ForeColor = ThemeManager.GetTextColor(Theme.Dark),
                Location = new Point(padding, y),
                AutoSize = true
            };
            cardPanel.Controls.Add(lblPassword);
            y += 24;

            txtPassword = new TextBox
            {
                Location = new Point(padding, y),
                Size = new Size(316, 36),
                Font = new Font("Segoe UI", 11f),
                BackColor = ThemeManager.GetInputColor(Theme.Dark),
                ForeColor = ThemeManager.GetTextColor(Theme.Dark),
                BorderStyle = BorderStyle.FixedSingle,
                UseSystemPasswordChar = true
            };
            cardPanel.Controls.Add(txtPassword);
            y += 60;

            // Login button
            btnLogin = new Button
            {
                Text = "Увійти",
                Location = new Point(padding, y),
                Size = new Size(316, 48),
                Font = new Font("Segoe UI Semibold", 11f),
                Cursor = Cursors.Hand
            };
            ThemeManager.StyleAccentButton(btnLogin);
            btnLogin.Click += BtnLogin_Click;
            cardPanel.Controls.Add(btnLogin);
            y += 60;

            // Register button
            btnRegister = new Button
            {
                Text = "Створити акаунт",
                Location = new Point(padding, y),
                Size = new Size(316, 48),
                Font = new Font("Segoe UI Semibold", 11f),
                Cursor = Cursors.Hand,
                Tag = "ghost"
            };
            ThemeManager.StyleButton(btnRegister, Theme.Dark);
            btnRegister.FlatAppearance.BorderColor = ThemeManager.GetBorder(Theme.Dark);
            btnRegister.FlatAppearance.BorderSize = 1;
            btnRegister.Click += BtnRegister_Click;
            cardPanel.Controls.Add(btnRegister);

            Controls.Add(cardPanel);

            // Enter key support
            this.AcceptButton = btnLogin;
        }

        private void BtnRegister_Click(object sender, EventArgs e)
        {
            var un = txtUsername.Text.Trim();
            var pw = txtPassword.Text;

            if (string.IsNullOrWhiteSpace(un) || string.IsNullOrWhiteSpace(pw))
            {
                ShowError("Введіть логін і пароль.");
                return;
            }

            if (pw.Length < 4)
            {
                ShowError("Пароль має бути не менше 4 символів.");
                return;
            }

            var salt = CryptoService.GenerateSalt();
            var hash = CryptoService.HashWithPBKDF2(pw, salt);

            var user = new User
            {
                Username = un,
                PasswordSalt = salt,
                PasswordHash = hash,
                PreferredTheme = "Dark"
            };

            try
            {
                user.Id = _db.CreateUser(user);
                LoggedInUser = user;
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                ShowError("Користувач вже існує або помилка БД.");
            }
        }

        private void BtnLogin_Click(object sender, EventArgs e)
        {
            var un = txtUsername.Text.Trim();
            var pw = txtPassword.Text;

            var user = _db.GetUserByUsername(un);
            if (user == null)
            {
                ShowError("Користувача не знайдено.");
                return;
            }

            var hash = CryptoService.HashWithPBKDF2(pw, user.PasswordSalt);
            if (hash != user.PasswordHash)
            {
                ShowError("Невірний пароль.");
                return;
            }

            LoggedInUser = user;
            DialogResult = DialogResult.OK;
            Close();
        }

        private void ShowError(string message)
        {
            MessageBox.Show(message, "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }
}