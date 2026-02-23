using System;
using System.Drawing;
using System.Windows.Forms;

namespace SecureNotes
{
    public class DeleteAccountForm : Form
    {
        private TextBox txtPassword;
        private Button btnDelete;
        private Button btnCancel;
        private DatabaseHelper _db = new DatabaseHelper(AppConfig.ConnStr);

        public DeleteAccountForm()
        {
            InitializeModernUI();
            ThemeManager.Apply(this, Program.CurrentTheme);
        }

        private void InitializeModernUI()
        {
            Text = "Видалити акаунт";
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(420, 260);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;

            int padding = 28;
            int y = padding;

            var lblTitle = new Label
            {
                Text = "Видалення акаунту",
                Font = new Font("Segoe UI Semibold", 16f),
                ForeColor = ThemeManager.Danger,
                Location = new Point(padding, y),
                AutoSize = true
            };
            Controls.Add(lblTitle);
            y += 40;

            var lblWarning = new Label
            {
                Text = "Ця дія незворотна. Всі ваші нотатки та паролі\nбудуть видалені назавжди.",
                Font = new Font("Segoe UI", 9.5f),
                Location = new Point(padding, y),
                Size = new Size(360, 40),
                Tag = "secondary"
            };
            Controls.Add(lblWarning);
            y += 50;

            var lblPassword = new Label
            {
                Text = "Введіть пароль для підтвердження:",
                Font = new Font("Segoe UI", 9.5f),
                Location = new Point(padding, y),
                AutoSize = true
            };
            Controls.Add(lblPassword);
            y += 26;

            txtPassword = new TextBox
            {
                Location = new Point(padding, y),
                Size = new Size(360, 36),
                Font = new Font("Segoe UI", 11f),
                UseSystemPasswordChar = true
            };
            Controls.Add(txtPassword);
            y += 50;

            btnCancel = new Button
            {
                Text = "Скасувати",
                Location = new Point(padding, y),
                Size = new Size(170, 44),
                Font = new Font("Segoe UI Semibold", 10f),
                Cursor = Cursors.Hand
            };
            btnCancel.Click += (s, e) => Close();
            Controls.Add(btnCancel);

            btnDelete = new Button
            {
                Text = "Видалити акаунт",
                Location = new Point(padding + 180, y),
                Size = new Size(180, 44),
                Font = new Font("Segoe UI Semibold", 10f),
                Cursor = Cursors.Hand
            };
            btnDelete.Click += BtnDelete_Click;
            Controls.Add(btnDelete);

            this.Load += (s, e) =>
            {
                ThemeManager.StyleGhostButton(btnCancel, Program.CurrentTheme);
                btnCancel.FlatAppearance.BorderColor = ThemeManager.GetBorder(Program.CurrentTheme);
                btnCancel.FlatAppearance.BorderSize = 1;
                ThemeManager.StyleDangerButton(btnDelete, Program.CurrentTheme);
            };
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            Program.TouchActivity();

            var pw = txtPassword.Text;
            var user = Program.CurrentUser;

            var hash = CryptoService.HashWithPBKDF2(pw, user.PasswordSalt);
            if (hash != user.PasswordHash)
            {
                MessageBox.Show("Невірний пароль.", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var confirm = MessageBox.Show(
                "Ви впевнені? Цю дію неможливо скасувати.",
                "Підтвердження",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (confirm == DialogResult.Yes)
            {
                _db.DeleteUser(user.Id);
                MessageBox.Show("Акаунт успішно видалено.", "Готово", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Application.Exit();
            }
        }
    }
}