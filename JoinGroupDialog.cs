using System;
using System.Drawing;
using System.Windows.Forms;

namespace SecureNotes
{
    public class JoinGroupDialog : Form
    {
        private TextBox txtCode;
        private Button btnOk;
        private Button btnCancel;
        private Label lblCode;

        public string InviteCode => txtCode.Text.Trim();

        public JoinGroupDialog()
        {
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(360, 140);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;

            lblCode = new Label { Location = new Point(16, 16) };
            txtCode = new TextBox { Location = new Point(16, 36), Width = 320 };

            btnOk = new Button { Location = new Point(176, 80), Width = 100 };
            btnCancel = new Button { Location = new Point(286, 80), Width = 80 };

            btnOk.Click += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(InviteCode))
                {
                    MessageBox.Show(LocalizationManager.Get("enter_invite_code"));
                    return;
                }

                DialogResult = DialogResult.OK;
                Close();
            };

            btnCancel.Click += (s, e) =>
            {
                DialogResult = DialogResult.Cancel;
                Close();
            };

            Controls.AddRange(new Control[] { lblCode, txtCode, btnOk, btnCancel });

            LocalizationManager.LanguageChanged += ApplyLocalization;
            ApplyLocalization();
            FormClosed += (s, e) => LocalizationManager.LanguageChanged -= ApplyLocalization;
        }

        private void ApplyLocalization()
        {
            Text = LocalizationManager.Get("join_group");
            lblCode.Text = LocalizationManager.Get("invite_code");
            btnOk.Text = LocalizationManager.Get("join_group");
            btnCancel.Text = LocalizationManager.Get("cancel");
        }
    }
}
