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
            Text = LocalizationManager.Get("delete_account");
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(420, 260);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;

            int padding = 28;
            int y = padding;

            var lblTitle = new Label
            {
                Text = LocalizationManager.Get("account_deletion"),
                Font = new Font("Segoe UI Semibold", 16f),
                ForeColor = ThemeManager.Danger,
                Location = new Point(padding, y),
                AutoSize = true
            };
            Controls.Add(lblTitle);
            y += 40;

            var lblWarning = new Label
            {
                Text = LocalizationManager.Get("irreversible_delete_warning"),
                Font = new Font("Segoe UI", 9.5f),
                Location = new Point(padding, y),
                Size = new Size(360, 40),
                Tag = "secondary"
            };
            Controls.Add(lblWarning);
            y += 50;

            var lblPassword = new Label
            {
                Text = LocalizationManager.Get("enter_password_confirm"),
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
                Text = LocalizationManager.Get("cancel"),
                Location = new Point(padding, y),
                Size = new Size(170, 44),
                Font = new Font("Segoe UI Semibold", 10f),
                Cursor = Cursors.Hand
            };
            btnCancel.Click += (s, e) => Close();
            Controls.Add(btnCancel);

            btnDelete = new Button
            {
                Text = LocalizationManager.Get("delete_account"),
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
                MessageBox.Show(LocalizationManager.Get("invalid_password"), LocalizationManager.Get("error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var confirm = MessageBox.Show(
                LocalizationManager.Get("are_you_sure_irreversible"),
                LocalizationManager.Get("confirmation"),
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (confirm == DialogResult.Yes)
            {
                _db.DeleteUser(user.Id);
                MessageBox.Show(LocalizationManager.Get("account_deleted_successfully"), LocalizationManager.Get("success"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                Application.Exit();
            }
        }
    }
}