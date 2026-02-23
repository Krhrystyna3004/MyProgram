// ChangePasswordForm.cs
using System;
using System.Drawing;
using System.Windows.Forms;

namespace SecureNotes
{
    public class ChangePasswordForm : Form
    {
        private TextBox txtCurrentPassword;
        private TextBox txtNewPassword;
        private TextBox txtConfirmPassword;
        private Button btnSave;
        private Button btnCancel;
        private readonly DatabaseHelper _db = new DatabaseHelper(AppConfig.ConnStr);

        public ChangePasswordForm()
        {
            InitializeModernUI();
            ThemeManager.Apply(this, Program.CurrentTheme);
        }

        private void InitializeModernUI()
        {
            Text = LocalizationManager.Get("change_password");
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(420, 340);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;

            int padding = 28;
            int y = padding;
            int fieldWidth = ClientSize.Width - padding * 2;

            var lblTitle = new Label
            {
                Text = LocalizationManager.Get("change_password"),
                Font = new Font("Segoe UI Semibold", 16f),
                Location = new Point(padding, y),
                AutoSize = true
            };
            Controls.Add(lblTitle);
            y += 45;

            // Current password
            var lblCurrent = new Label
            {
                Text = LocalizationManager.Get("current_password"),
                Font = new Font("Segoe UI", 9.5f),
                Location = new Point(padding, y),
                AutoSize = true
            };
            Controls.Add(lblCurrent);
            y += 24;

            txtCurrentPassword = new TextBox
            {
                Location = new Point(padding, y),
                Size = new Size(fieldWidth, 36),
                Font = new Font("Segoe UI", 11f),
                UseSystemPasswordChar = true
            };
            Controls.Add(txtCurrentPassword);
            y += 48;

            // New password
            var lblNew = new Label
            {
                Text = LocalizationManager.Get("new_password"),
                Font = new Font("Segoe UI", 9.5f),
                Location = new Point(padding, y),
                AutoSize = true
            };
            Controls.Add(lblNew);
            y += 24;

            txtNewPassword = new TextBox
            {
                Location = new Point(padding, y),
                Size = new Size(fieldWidth, 36),
                Font = new Font("Segoe UI", 11f),
                UseSystemPasswordChar = true
            };
            Controls.Add(txtNewPassword);
            y += 48;

            // Confirm password
            var lblConfirm = new Label
            {
                Text = LocalizationManager.Get("confirm_new_password"),
                Font = new Font("Segoe UI", 9.5f),
                Location = new Point(padding, y),
                AutoSize = true
            };
            Controls.Add(lblConfirm);
            y += 24;

            txtConfirmPassword = new TextBox
            {
                Location = new Point(padding, y),
                Size = new Size(fieldWidth, 36),
                Font = new Font("Segoe UI", 11f),
                UseSystemPasswordChar = true
            };
            Controls.Add(txtConfirmPassword);
            y += 50;

            // Buttons
            btnCancel = new Button
            {
                Text = LocalizationManager.Get("cancel"),
                Location = new Point(padding, y),
                Size = new Size((fieldWidth - 10) / 2, 44),
                Font = new Font("Segoe UI Semibold", 10f),
                Cursor = Cursors.Hand
            };
            btnCancel.Click += (s, e) => Close();
            Controls.Add(btnCancel);

            btnSave = new Button
            {
                Text = LocalizationManager.Get("save"),
                Location = new Point(padding + (fieldWidth - 10) / 2 + 10, y),
                Size = new Size((fieldWidth - 10) / 2, 44),
                Font = new Font("Segoe UI Semibold", 10f),
                Cursor = Cursors.Hand
            };
            btnSave.Click += BtnSave_Click;
            Controls.Add(btnSave);

            this.Load += (s, e) =>
            {
                ThemeManager.StyleGhostButton(btnCancel, Program.CurrentTheme);
                btnCancel.FlatAppearance.BorderColor = ThemeManager.GetBorder(Program.CurrentTheme);
                btnCancel.FlatAppearance.BorderSize = 1;
                ThemeManager.StyleAccentButton(btnSave);
            };
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            Program.TouchActivity();

            var current = txtCurrentPassword.Text;
            var newPw = txtNewPassword.Text;
            var confirm = txtConfirmPassword.Text;

            if (string.IsNullOrWhiteSpace(current) || string.IsNullOrWhiteSpace(newPw))
            {
                MessageBox.Show(LocalizationManager.Get("fill_required_fields"), LocalizationManager.Get("warning"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (newPw.Length < 4)
            {
                MessageBox.Show(LocalizationManager.Get("password_min_length"), LocalizationManager.Get("warning"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (newPw != confirm)
            {
                MessageBox.Show(LocalizationManager.Get("passwords_do_not_match"), LocalizationManager.Get("warning"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Verify current password
            var currentHash = CryptoService.HashWithPBKDF2(current, Program.CurrentUser.PasswordSalt);
            if (currentHash != Program.CurrentUser.PasswordHash)
            {
                MessageBox.Show(LocalizationManager.Get("current_password_incorrect"), LocalizationManager.Get("error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Update password
            var newSalt = CryptoService.GenerateSalt();
            var newHash = CryptoService.HashWithPBKDF2(newPw, newSalt);

            try
            {
                _db.UpdateUserPassword(Program.CurrentUser.Id, newHash, newSalt);
                Program.CurrentUser.PasswordHash = newHash;
                Program.CurrentUser.PasswordSalt = newSalt;

                MessageBox.Show(LocalizationManager.Get("password_changed_success"), LocalizationManager.Get("success"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{LocalizationManager.Get("password_change_failed")}{ex.Message}", LocalizationManager.Get("error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // ChangePasswordForm
            // 
            this.ClientSize = new System.Drawing.Size(282, 253);
            this.Name = "ChangePasswordForm";
            this.Load += new System.EventHandler(this.ChangePasswordForm_Load);
            this.ResumeLayout(false);

        }

        private void ChangePasswordForm_Load(object sender, EventArgs e)
        {

        }
    }
}