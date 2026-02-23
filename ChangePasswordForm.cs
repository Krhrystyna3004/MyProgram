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
            Text = "Змінити пароль";
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
                Text = "Зміна пароля",
                Font = new Font("Segoe UI Semibold", 16f),
                Location = new Point(padding, y),
                AutoSize = true
            };
            Controls.Add(lblTitle);
            y += 45;

            // Current password
            var lblCurrent = new Label
            {
                Text = "Поточний пароль",
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
                Text = "Новий пароль",
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
                Text = "Підтвердіть новий пароль",
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
                Text = "Скасувати",
                Location = new Point(padding, y),
                Size = new Size((fieldWidth - 10) / 2, 44),
                Font = new Font("Segoe UI Semibold", 10f),
                Cursor = Cursors.Hand
            };
            btnCancel.Click += (s, e) => Close();
            Controls.Add(btnCancel);

            btnSave = new Button
            {
                Text = "Зберегти",
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
                MessageBox.Show("Заповніть всі поля.", "Увага", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (newPw.Length < 4)
            {
                MessageBox.Show("Новий пароль має бути не менше 4 символів.", "Увага", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (newPw != confirm)
            {
                MessageBox.Show("Паролі не співпадають.", "Увага", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Verify current password
            var currentHash = CryptoService.HashWithPBKDF2(current, Program.CurrentUser.PasswordSalt);
            if (currentHash != Program.CurrentUser.PasswordHash)
            {
                MessageBox.Show("Невірний поточний пароль.", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

                MessageBox.Show("Пароль успішно змінено!", "Успіх", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка: {ex.Message}", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}