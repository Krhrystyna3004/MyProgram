using System;
using System.Drawing;
using System.Windows.Forms;

namespace SecureNotes
{
    public class CreateGroupDialog : Form
    {
        private TextBox txtName;
        private Button btnOk;
        private Button btnCancel;
        private Label lblName;

        public string GroupName => txtName.Text.Trim();

  

        public CreateGroupDialog()
        {
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(360, 140);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;

            lblName = new Label { Location = new Point(16, 16) };
            txtName = new TextBox { Location = new Point(16, 36), Width = 320 };

            btnOk = new Button { Location = new Point(176, 80), Width = 80 };
            btnCancel = new Button { Location = new Point(256, 80), Width = 80 };

            btnOk.Click += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(GroupName))
                {
                    MessageBox.Show(LocalizationManager.Get("enter_group_name"));
                    return;
                }
                DialogResult = DialogResult.OK;
                Close();
            };
            btnCancel.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

            Controls.AddRange(new Control[] { lblName, txtName, btnOk, btnCancel });

            LocalizationManager.LanguageChanged += ApplyLocalization;
            ApplyLocalization();
            FormClosed += (s, e) => LocalizationManager.LanguageChanged -= ApplyLocalization;
        }

        private void ApplyLocalization()
        {
            Text = LocalizationManager.Get("create_group");
            lblName.Text = LocalizationManager.Get("group_name");
            btnOk.Text = LocalizationManager.Get("create");
            btnCancel.Text = LocalizationManager.Get("cancel");
        }
    }
}