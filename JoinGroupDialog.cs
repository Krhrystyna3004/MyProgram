using System;
using System.Drawing;
using System.Windows.Forms;

namespace SecureNotes
{
    public class JoinGroupDialog : Form
    {
        private TextBox txtCode;
        private Label lblCode;
        private Button btnOk, btnCancel;

        public string InviteCode => txtCode.Text.Trim();

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // JoinGroupDialog
            // 
            this.ClientSize = new System.Drawing.Size(282, 253);
            this.Name = "JoinGroupDialog";
            this.Load += new System.EventHandler(this.JoinGroupDialog_Load);
            this.ResumeLayout(false);

        }

        private void JoinGroupDialog_Load(object sender, EventArgs e)
        {

        }

        public JoinGroupDialog()
        {
            Text = LocalizationManager.Get("join_group");
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(360, 140);
            FormBorderStyle = FormBorderStyle.FixedDialog; // ✅ правильне призначення
            MaximizeBox = false;
            MinimizeBox = false;

            lblCode = new Label { Text = LocalizationManager.Get("invite_code"), Location = new Point(16, 16) };
            txtCode = new TextBox { Location = new Point(16, 36), Width = 320 };

            btnOk = new Button { Text = LocalizationManager.Get("join_group"), Location = new Point(176, 80), Width = 100 };
            btnCancel = new Button { Text = LocalizationManager.Get("cancel"), Location = new Point(286, 80), Width = 80 };

            btnOk.Click += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(InviteCode))
                {
                    MessageBox.Show(LocalizationManager.Get("invite_code"));
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

            LocalizationManager.LanguageChanged += ApplyLocalization;
            FormClosed += (s, e) => LocalizationManager.LanguageChanged -= ApplyLocalization;

            Controls.AddRange(new Control[] { lblCode, txtCode, btnOk, btnCancel });
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