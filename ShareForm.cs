using System;
using System.Drawing;
using System.Windows.Forms;

namespace SecureNotes
{
    public class ShareForm : Form
    {
        private TextBox txtInviteCode;
        private Button btnCreateGroup;
        private Button btnJoinGroup;
        private Label lblYourCode;
        private TextBox txtGroupName;
        private readonly DatabaseHelper _db = new DatabaseHelper(AppConfig.ConnStr);

        public ShareForm()
        {
            Text = LocalizationManager.Get("shared_notes");
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(420, 260);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;

            lblYourCode = new Label { Text = LocalizationManager.Get("your_code_not_created"), Location = new Point(16, 16), Width = 380 };

            txtGroupName = new TextBox { Location = new Point(16, 40), Width = 380 };
            UIHelpers.SetPlaceholder(txtGroupName, LocalizationManager.Get("optional_group_name"));
            btnCreateGroup = new Button { Text = LocalizationManager.Get("create_group"), Location = new Point(16, 72), Width = 380 };
            btnCreateGroup.Click += (s, e) =>
            {
                Program.TouchActivity();

                var grp = _db.CreateGroup(Program.CurrentUser.Id,
                    string.IsNullOrWhiteSpace(txtGroupName.Text) || txtGroupName.ForeColor == Color.Gray
                     ? LocalizationManager.Get("my_group")
                        : txtGroupName.Text.Trim());

                _db.AddMember(grp.Id, Program.CurrentUser.Id, "edit");
                lblYourCode.Text = $"{LocalizationManager.Get("invite_code")}: {grp.InviteCode}";
                MessageBox.Show(LocalizationManager.Get("group_created_share_code"));
            };

            var lblJoin = new Label { Text = LocalizationManager.Get("enter_invite_code_to_join"), Location = new Point(16, 112), Width = 380 };
            txtInviteCode = new TextBox { Location = new Point(16, 132), Width = 380 };
            UIHelpers.SetPlaceholder(txtInviteCode, LocalizationManager.Get("group_invite_code_placeholder"));

            btnJoinGroup = new Button { Text = LocalizationManager.Get("join_group"), Location = new Point(16, 164), Width = 380 };
            btnJoinGroup.Click += (s, e) =>
            {
                Program.TouchActivity();

                var code = txtInviteCode.Text.Trim();
                if (txtInviteCode.ForeColor == Color.Gray) code = "";


                var grp = _db.GetGroupByInvite(code);
                if (grp == null)
                {
                    MessageBox.Show(LocalizationManager.Get("group_not_found"));
                    return;
                }
                _db.AddMember(grp.Id, Program.CurrentUser.Id, "edit");
                MessageBox.Show(LocalizationManager.Get("joined_group_check_shared"));
            };

            var btnClose = new Button { Text = LocalizationManager.Get("close"), Location = new Point(16, 204), Width = 380 };
            btnClose.Click += (s, e) => Close();

            Controls.AddRange(new Control[] { lblYourCode, txtGroupName, btnCreateGroup, lblJoin, txtInviteCode, btnJoinGroup, btnClose });
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // ShareForm
            // 
            this.ClientSize = new System.Drawing.Size(282, 253);
            this.Name = "ShareForm";
            this.Load += new System.EventHandler(this.ShareForm_Load);
            this.ResumeLayout(false);

        }

        private void ShareForm_Load(object sender, EventArgs e)
        {

        }
    }
}