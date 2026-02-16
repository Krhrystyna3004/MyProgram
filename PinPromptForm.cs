using System;
using System.Drawing;
using System.Windows.Forms;

namespace SecureNotes
{
    public class PinPromptForm : Form
    {
        private TextBox txtPin;
        private Button btnOk;
        private Button btnSetPin;
        private readonly DatabaseHelper _db = new DatabaseHelper(AppConfig.ConnStr);
        private readonly User _user;

        public PinPromptForm(User user)
        {
            _user = user;
            InitializeModernUI();
            ThemeManager.Apply(this, Program.CurrentTheme);
        }

        private void InitializeModernUI()
        {
            Text = "PIN для паролів";
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(400, 280);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;

            int padding = 28;
            int y = padding;

            var lblTitle = new Label
            {
                Text = "Введіть PIN-код",
                Font = new Font("Segoe UI Semibold", 16f),
                Location = new Point(padding, y),
                AutoSize = true
            };
            Controls.Add(lblTitle);
            y += 40;

            var lblSubtitle = new Label
            {
                Text = "PIN потрібен для доступу до зашифрованих паролів",
                Font = new Font("Segoe UI", 9f),
                ForeColor = ThemeManager.GetTextSecondary(Program.CurrentTheme),
                Location = new Point(padding, y),
                AutoSize = true,
                Tag = "secondary"
            };
            Controls.Add(lblSubtitle);
            y += 40;

            txtPin = new TextBox
            {
                Location = new Point(padding, y),
                Size = new Size(340, 40),
                Font = new Font("Segoe UI", 14f),
                UseSystemPasswordChar = true,
                TextAlign = HorizontalAlignment.Center
            };
            Controls.Add(txtPin);
            y += 55;

            btnOk = new Button
            {
                Text = "Розблокувати",
                Location = new Point(padding, y),
                Size = new Size(165, 44),
                Font = new Font("Segoe UI Semibold", 10f),
                Cursor = Cursors.Hand
            };
            btnOk.Click += BtnOk_Click;
            Controls.Add(btnOk);

            btnSetPin = new Button
            {
                Text = string.IsNullOrEmpty(_user.PinHash) ? "Створити PIN" : "Змінити PIN",
                Location = new Point(padding + 175, y),
                Size = new Size(165, 44),
                Font = new Font("Segoe UI Semibold", 10f),
                Cursor = Cursors.Hand
            };
            btnSetPin.Click += BtnSetPin_Click;
            Controls.Add(btnSetPin);

            this.Load += (s, e) =>
            {
                ThemeManager.StyleAccentButton(btnOk);
                ThemeManager.StyleGhostButton(btnSetPin, Program.CurrentTheme);
                btnSetPin.FlatAppearance.BorderColor = ThemeManager.GetBorder(Program.CurrentTheme);
                btnSetPin.FlatAppearance.BorderSize = 1;
            };

            this.AcceptButton = btnOk;
        }

        private void BtnSetPin_Click(object sender, EventArgs e)
        {
            Program.TouchActivity();

            var pin = txtPin.Text.Trim();
            if (pin.Length < 4)
            {
                MessageBox.Show("PIN має бути щонайменше 4 символи.", "Увага", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!string.IsNullOrEmpty(_user.PinHash) && !string.IsNullOrEmpty(_user.PinSalt))
            {
                var oldHash = CryptoService.HashWithPBKDF2(pin, _user.PinSalt);
                if (oldHash != _user.PinHash)
                {
                    MessageBox.Show("Невірний поточний PIN.", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var newPin = Prompt.Show("Введіть новий PIN:", "Новий PIN");
                if (string.IsNullOrWhiteSpace(newPin) || newPin.Length < 4)
                {
                    MessageBox.Show("Новий PIN має бути щонайменше 4 символи.", "Увага", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var newSalt = CryptoService.GenerateSalt();
                var newHash = CryptoService.HashWithPBKDF2(newPin, newSalt);

                try
                {
                    _db.UpdateUserPin(_user.Id, oldHash, newHash, newSalt);
                    _user.PinSalt = newSalt;
                    _user.PinHash = newHash;
                    MessageBox.Show("PIN успішно змінено!", "Успіх", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Не вдалося змінити PIN: " + ex.Message, "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                var newSalt = CryptoService.GenerateSalt();
                var newHash = CryptoService.HashWithPBKDF2(pin, newSalt);

                try
                {
                    _db.UpdateUserPin(_user.Id, "", newHash, newSalt);
                    _user.PinSalt = newSalt;
                    _user.PinHash = newHash;
                    MessageBox.Show("PIN успішно створено!", "Успіх", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Не вдалося створити PIN: " + ex.Message, "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BtnOk_Click(object sender, EventArgs e)
        {
            Program.TouchActivity();

            var pin = txtPin.Text.Trim();
            if (string.IsNullOrEmpty(_user.PinHash) || string.IsNullOrEmpty(_user.PinSalt))
            {
                MessageBox.Show("Спочатку встановіть PIN.", "Увага", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var hash = CryptoService.HashWithPBKDF2(pin, _user.PinSalt);
            if (hash != _user.PinHash)
            {
                MessageBox.Show("Невірний PIN.", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Program.SessionKey = CryptoService.DeriveKeyFromPin(pin, _user.PinSalt);
            DialogResult = DialogResult.OK;
            Close();
        }
    }

    public static class Prompt
    {
        public static string Show(string text, string caption)
        {
            var form = new Form
            {
                Width = 400,
                Height = 180,
                Text = caption,
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false
            };

            ThemeManager.Apply(form, Program.CurrentTheme);

            var lbl = new Label { Left = 28, Top = 24, Text = text, Width = 340, Font = new Font("Segoe UI", 10f) };
            var txt = new TextBox { Left = 28, Top = 54, Width = 340, Height = 36, UseSystemPasswordChar = true, Font = new Font("Segoe UI", 12f) };
            var btnOk = new Button { Text = "OK", Left = 218, Width = 150, Top = 100, Height = 40, DialogResult = DialogResult.OK };

            ThemeManager.StyleAccentButton(btnOk);

            form.Controls.AddRange(new Control[] { lbl, txt, btnOk });
            form.AcceptButton = btnOk;

            return form.ShowDialog() == DialogResult.OK ? txt.Text : "";
        }
    }
}

