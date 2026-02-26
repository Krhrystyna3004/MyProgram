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
        private readonly ApiClient _api = new ApiClient(ApiConfig.BaseUrl);

        public User LoggedInUser { get; private set; }

        public LoginForm()
        {
            InitializeModernUI();
        }

        private void InitializeModernUI()
        {
            Text = LocalizationManager.Get("app_title");
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
                Text = LocalizationManager.Get("app_title"),
                Font = new Font("Segoe UI Semibold", 24f),
                ForeColor = ThemeManager.Accent,
                Location = new Point(padding, y),
                AutoSize = true
            };
            cardPanel.Controls.Add(lblTitle);
            y += 50;

            lblSubtitle = new Label
            {
                Text = LocalizationManager.Get("login_subtitle"),
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
                Text = LocalizationManager.Get("login"),
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
                Text = LocalizationManager.Get("password"),
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
                Text = LocalizationManager.Get("sign_in"),
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
                Text = LocalizationManager.Get("create_account"),
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
                ShowError(LocalizationManager.Get("enter_login_and_password"));
                return;
            }

            if (pw.Length < 4)
            {
                ShowError(LocalizationManager.Get("password_min_length"));
                return;
            }

            try
            {
                var auth = _api.Register(un, pw);
                SessionStore.AccessToken = auth.AccessToken;
                var localUser = TryLoadLocalUser(auth.Username);
                LoggedInUser = localUser ?? new User
                {
                    Id = auth.UserId,
                    Username = auth.Username,
                    PreferredTheme = "Dark"
                };

                LoggedInUser.Id = auth.UserId;
                LoggedInUser.Username = auth.Username;

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                ShowError("API register error: " + ex.Message);
            }
        }

        private void BtnLogin_Click(object sender, EventArgs e)
        {
            var un = txtUsername.Text.Trim();
            var pw = txtPassword.Text;

            if (string.IsNullOrWhiteSpace(un) || string.IsNullOrWhiteSpace(pw))
            {
                ShowError(LocalizationManager.Get("enter_login_and_password"));
                return;
            }

            try
            {
                var auth = _api.Login(un, pw);
                SessionStore.AccessToken = auth.AccessToken;
                var localUser = TryLoadLocalUser(auth.Username);
                LoggedInUser = localUser ?? new User
                {
                    Id = auth.UserId,
                    Username = auth.Username,
                    PreferredTheme = "Dark"
                };

                LoggedInUser.Id = auth.UserId;
                LoggedInUser.Username = auth.Username;

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                ShowError("API login error: " + ex.Message);
            }
        }
        private User TryLoadLocalUser(string username)
        {
            try
            {
                var db = new DatabaseHelper(AppConfig.ConnStr);
                return db.GetUserByUsername(username);
            }
            catch
            {
                return null;
            }
        }

        private void ShowError(string message)
        {
            MessageBox.Show(message, LocalizationManager.Get("error"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // LoginForm
            // 
            this.ClientSize = new System.Drawing.Size(282, 253);
            this.Name = "LoginForm";
            this.Load += new System.EventHandler(this.LoginForm_Load);
            this.ResumeLayout(false);

        }

        private void LoginForm_Load(object sender, EventArgs e)
        {

        }
    }
}