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
        private DatabaseHelper _db;
        private readonly User _user;

        private DatabaseHelper Db
        {
            get
            {
                if (_db == null)
                {
                    _db = new DatabaseHelper(AppConfig.ConnStr);
                }

                return _db;
            }
        }

        public PinPromptForm(User user)
        {
            _user = user;
            TryHydrateUserPinData();
            InitializeModernUI();
            ThemeManager.Apply(this, Program.CurrentTheme);
        }

        private void InitializeModernUI()
        {
            Text = LocalizationManager.Get("pin_for_passwords");
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(400, 280);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;

            int padding = 28;
            int y = padding;

            var lblTitle = new Label
            {
                Text = LocalizationManager.Get("enter_pin_code"),
                Font = new Font("Segoe UI Semibold", 16f),
                Location = new Point(padding, y),
                AutoSize = true
            };
            Controls.Add(lblTitle);
            y += 40;

            var lblSubtitle = new Label
            {
                Text = LocalizationManager.Get("pin_required"),
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
                Text = LocalizationManager.Get("unlock"),
                Location = new Point(padding, y),
                Size = new Size(165, 44),
                Font = new Font("Segoe UI Semibold", 10f),
                Cursor = Cursors.Hand
            };
            btnOk.Click += BtnOk_Click;
            Controls.Add(btnOk);

            btnSetPin = new Button
            {
                Text = string.IsNullOrEmpty(_user.PinHash) ? LocalizationManager.Get("create_pin") : LocalizationManager.Get("change_pin"),
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

        private void TryHydrateUserPinData()
        {
            if (_user == null || !string.IsNullOrWhiteSpace(_user.PinSalt))
            {
                return;
            }

            try
            {
                var fromDb = Db.GetUserByUsername(_user.Username);
                if (fromDb != null)
                {
                    _user.PinHash = fromDb.PinHash;
                    _user.PinSalt = fromDb.PinSalt;
                }
            }
            catch
            {
                // ignore hydration failures
            }
        }

        private ApiClient CreateApiClient()
        {
            if (string.IsNullOrWhiteSpace(ApiConfig.BaseUrl) || string.IsNullOrWhiteSpace(SessionStore.AccessToken))
            {
                return null;
            }

            var api = new ApiClient(ApiConfig.BaseUrl);
            api.SetAccessToken(SessionStore.AccessToken);
            return api;
        }

        private void ApplyPinData(string pin, string pinSalt)
        {
            if (string.IsNullOrWhiteSpace(pinSalt))
            {
                return;
            }

            _user.PinSalt = pinSalt;
            _user.PinHash = CryptoService.HashWithPBKDF2(pin, pinSalt);
        }

        private bool TryRefreshPinDataFromApi(string pin)
        {
            try
            {
                var api = CreateApiClient();
                if (api == null)
                {
                    return false;
                }

                var verify = api.VerifyPinDetailed(pin);
                if (verify?.IsValid == true && !string.IsNullOrWhiteSpace(verify.PinSalt))
                {
                    ApplyPinData(pin, verify.PinSalt);
                    return true;
                }
            }
            catch
            {
                // ignored
            }

            return false;
        }

        private void BtnSetPin_Click(object sender, EventArgs e)
        {
            Program.TouchActivity();

            var pin = txtPin.Text.Trim();
            if (pin.Length < 4)
            {
                MessageBox.Show(LocalizationManager.Get("pin_min_length"), LocalizationManager.Get("warning"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var api = CreateApiClient();
                if (api != null)
                {
                    if (!string.IsNullOrEmpty(_user.PinHash) && !string.IsNullOrEmpty(_user.PinSalt))
                    {
                        var newPin = Prompt.Show(LocalizationManager.Get("new_pin_prompt"), LocalizationManager.Get("new_pin"));
                        if (string.IsNullOrWhiteSpace(newPin) || newPin.Length < 4)
                        {
                            MessageBox.Show(LocalizationManager.Get("new_pin_min_length"), LocalizationManager.Get("warning"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        api.ChangePin(pin, newPin);
                        TryRefreshPinDataFromApi(newPin);
                        MessageBox.Show(LocalizationManager.Get("pin_changed_success"), LocalizationManager.Get("success"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    api.SetPin(pin);
                    TryRefreshPinDataFromApi(pin);
                    MessageBox.Show(LocalizationManager.Get("pin_created_success"), LocalizationManager.Get("success"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(LocalizationManager.Get("pin_change_failed") + ex.Message, LocalizationManager.Get("error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // DB fallback
            if (!string.IsNullOrEmpty(_user.PinHash) && !string.IsNullOrEmpty(_user.PinSalt))
            {
                var oldHash = CryptoService.HashWithPBKDF2(pin, _user.PinSalt);
                if (oldHash != _user.PinHash)
                {
                    MessageBox.Show(LocalizationManager.Get("current_pin_invalid"), LocalizationManager.Get("error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var newPin = Prompt.Show(LocalizationManager.Get("new_pin_prompt"), LocalizationManager.Get("new_pin"));
                if (string.IsNullOrWhiteSpace(newPin) || newPin.Length < 4)
                {
                    MessageBox.Show(LocalizationManager.Get("new_pin_min_length"), LocalizationManager.Get("warning"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var newSalt = CryptoService.GenerateSalt();
                var newHash = CryptoService.HashWithPBKDF2(newPin, newSalt);

                try
                {
                    Db.UpdateUserPin(_user.Id, oldHash, newHash, newSalt);
                    _user.PinSalt = newSalt;
                    _user.PinHash = newHash;
                    MessageBox.Show(LocalizationManager.Get("pin_changed_success"), LocalizationManager.Get("success"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(LocalizationManager.Get("pin_change_failed") + ex.Message, LocalizationManager.Get("error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                var newSalt = CryptoService.GenerateSalt();
                var newHash = CryptoService.HashWithPBKDF2(pin, newSalt);

                try
                {
                    Db.UpdateUserPin(_user.Id, string.Empty, newHash, newSalt);
                    _user.PinSalt = newSalt;
                    _user.PinHash = newHash;
                    MessageBox.Show(LocalizationManager.Get("pin_created_success"), LocalizationManager.Get("success"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(LocalizationManager.Get("pin_create_failed") + ex.Message, LocalizationManager.Get("error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BtnOk_Click(object sender, EventArgs e)
        {
            Program.TouchActivity();

            var pin = txtPin.Text.Trim();

            try
            {
                var api = CreateApiClient();
                if (api != null)
                {
                    var verify = api.VerifyPinDetailed(pin);
                    if (verify == null || !verify.IsSet)
                    {
                        MessageBox.Show(LocalizationManager.Get("set_pin_first"), LocalizationManager.Get("warning"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    if (!verify.IsValid)
                    {
                        MessageBox.Show(LocalizationManager.Get("pin_invalid"), LocalizationManager.Get("error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    if (!string.IsNullOrWhiteSpace(verify.PinSalt))
                    {
                        ApplyPinData(pin, verify.PinSalt);
                    }

                    if (string.IsNullOrWhiteSpace(_user.PinSalt))
                    {
                        MessageBox.Show(LocalizationManager.Get("pin_invalid"), LocalizationManager.Get("error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    Program.SessionKey = CryptoService.DeriveKeyFromPin(pin, _user.PinSalt);
                    DialogResult = DialogResult.OK;
                    Close();
                    return;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, LocalizationManager.Get("error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // DB fallback
            if (string.IsNullOrEmpty(_user.PinHash) || string.IsNullOrEmpty(_user.PinSalt))
            {
                MessageBox.Show(LocalizationManager.Get("set_pin_first"), LocalizationManager.Get("warning"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var hash = CryptoService.HashWithPBKDF2(pin, _user.PinSalt);
            if (hash != _user.PinHash)
            {
                MessageBox.Show(LocalizationManager.Get("pin_invalid"), LocalizationManager.Get("error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
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
