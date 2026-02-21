using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace SecureNotes
{
    public partial class MainForm : Form
    {
        private readonly DatabaseHelper _db = new DatabaseHelper(AppConfig.ConnStr);

        private List<Note> _allNotes = new List<Note>();
        private List<Group> _myGroups = new List<Group>();
        private string _tab = "notes";
        private bool _passwordsUnlocked = false;
        private int? _selectedGroupId = null;

        private Panel header, sidebar, contentPanel;
        private Button btnNotes, btnPasswords, btnShared;
        private Button btnThemeToggle, btnSettings, btnCreate;
        private Label lblTitle, lblSection;
        private TextBox txtSearch;
        private ComboBox cmbTagFilter;
        private FlowLayoutPanel flowCards;
        private Timer idleLockTimer;

        private ListBox lstGroups;
        private Label lblGroupHeader, lblGroupCode;
        private Panel groupInfoPanel, sharedToolbarRow1;
        private Button btnSharedCreateNote, btnSharedCreateGroup, btnSharedJoinGroup;
        private Button btnCopyCode, btnDeleteGroup, btnLeaveGroup;
        private TextBox txtSharedGroupName, txtSharedJoinCode;

        private bool _isRendering = false;

        public MainForm()
        {
            BuildUI();
            LocalizationManager.LanguageChanged += ApplyLocalization;
            ThemeManager.Apply(this, Program.CurrentTheme);
            ApplyThemeToCustomControls();
            ApplyLocalization();
            UpdateSidebarButtons();

            LoadGroups();
            LoadNotes();
            RenderCurrentTab();

            idleLockTimer = new Timer { Interval = 30000 };
            idleLockTimer.Tick += (s, e) =>
            {
                var idle = DateTime.Now - Program.LastActivity;
                if (idle.TotalMinutes >= 5)
                {
                    _passwordsUnlocked = false;
                    Program.SessionKey = null;
                    if (_tab == "passwords") RenderCurrentTab();
                }
            };
            idleLockTimer.Start();

            this.MouseDown += (s, e) => Program.TouchActivity();
            this.KeyPress += (s, e) => Program.TouchActivity();
            this.Resize += (s, e) => AdjustLayout();
            this.FormClosed += (s, e) => LocalizationManager.LanguageChanged -= ApplyLocalization;
        }

        private void BuildUI()
        {
            Text = $"{LocalizationManager.Get("app_title")} - {Program.CurrentUser.Username}";
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(1200, 750);
            MinimumSize = new Size(900, 550);
            Font = new Font("Segoe UI", 10f);

            // === HEADER ===
            header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60
            };

            lblTitle = new Label
            {
                Text = Program.CurrentUser.Username,
                Font = new Font("Segoe UI Semibold", 14f),
                Location = new Point(220, 18),
                AutoSize = true
            };
            header.Controls.Add(lblTitle);

            txtSearch = new TextBox
            {
                Location = new Point(380, 14),
                Size = new Size(250, 32),
                Font = new Font("Segoe UI", 10f)
            };
            UIHelpers.SetPlaceholder(txtSearch, LocalizationManager.Get("search") + "...");
            txtSearch.TextChanged += (s, e) => RenderCurrentTab();
            header.Controls.Add(txtSearch);

            int buttonY = 10;
            int rightMargin = 20;
            int buttonSpacing = 10;

            btnCreate = new Button
            {
                Text = LocalizationManager.Get("add_note"),
                Size = new Size(140, 40),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI Semibold", 10f),
                Cursor = Cursors.Hand
            };
            btnCreate.Location = new Point(ClientSize.Width - rightMargin - btnCreate.Width, buttonY);
            btnCreate.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnCreate.Click += BtnCreate_Click;
            header.Controls.Add(btnCreate);

            btnThemeToggle = new Button
            {
                Text = LocalizationManager.Get("theme_style"),
                Size = new Size(80, 40),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9f),
                Cursor = Cursors.Hand,
                TextAlign = ContentAlignment.MiddleCenter
            };
            btnThemeToggle.FlatAppearance.BorderSize = 1;
            btnThemeToggle.Location = new Point(btnCreate.Left - buttonSpacing - btnThemeToggle.Width, buttonY);
            btnThemeToggle.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnThemeToggle.Click += BtnThemeToggle_Click;
            header.Controls.Add(btnThemeToggle);

            btnSettings = new Button
            {
                Text = LocalizationManager.Get("settings"),
                Size = new Size(110, 40),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9f),
                Cursor = Cursors.Hand,
                TextAlign = ContentAlignment.MiddleCenter
            };
            btnSettings.FlatAppearance.BorderSize = 1;
            btnSettings.Location = new Point(btnThemeToggle.Left - buttonSpacing - btnSettings.Width, buttonY);
            btnSettings.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnSettings.Click += BtnSettings_Click;
            header.Controls.Add(btnSettings);

            btnCreate.BringToFront();
            btnThemeToggle.BringToFront();
            btnSettings.BringToFront();

            Controls.Add(header);

            this.Load += (s, e) =>
            {
                int y = 10;
                int margin = 20;
                int spacing = 10;

                btnCreate.Location = new Point(header.Width - margin - btnCreate.Width, y);
                btnThemeToggle.Location = new Point(btnCreate.Left - spacing - btnThemeToggle.Width, y);
                btnSettings.Location = new Point(btnThemeToggle.Left - spacing - btnSettings.Width, y);

                btnCreate.BringToFront();
                btnThemeToggle.BringToFront();
                btnSettings.BringToFront();
            };

            // === SIDEBAR ===
            sidebar = new Panel
            {
                Location = new Point(0, 60),
                Size = new Size(200, ClientSize.Height - 60),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left
            };

            int sidebarY = 16;

            btnNotes = new Button
            {
                Text = LocalizationManager.Get("notes"),
                Location = new Point(8, sidebarY),
                Size = new Size(184, 44),
                Font = new Font("Segoe UI Semibold", 10f),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(12, 0, 0, 0)
            };
                lblSection.Text = LocalizationManager.Get("my_notes");
                Text = LocalizationManager.Get("passwords"),
            {
                Text = LocalizationManager.Get("groups"),
                Program.SessionKey = null;
                lblSection.Text = LocalizationManager.Get("shared_notes");
                Text = LocalizationManager.Get("my_notes"),
            cmbTagFilter.Items.Add(LocalizationManager.Get("all_tags"));
                Text = LocalizationManager.Get("invite_code") + ": --------",
            };
                Text = LocalizationManager.Get("copy") + " " + LocalizationManager.Get("invite_code"),
                Text = LocalizationManager.Get("leave_group"),

                Text = LocalizationManager.Get("delete_group"),
            {
                Text = LocalizationManager.Get("shared_notes"),
                Text = "+ " + LocalizationManager.Get("create_note"),
            UIHelpers.SetPlaceholder(txtSharedGroupName, LocalizationManager.Get("group_name"));
                Text = LocalizationManager.Get("create_group"),
            UIHelpers.SetPlaceholder(txtSharedJoinCode, LocalizationManager.Get("invite_code"));
                Text = LocalizationManager.Get("join_group"),
            };
        private void ApplyLocalization()
        {
            Text = $"{LocalizationManager.Get("app_title")} - {Program.CurrentUser.Username}";
            btnNotes.Text = LocalizationManager.Get("notes");
            btnPasswords.Text = LocalizationManager.Get("passwords");
            btnShared.Text = LocalizationManager.Get("groups");
            btnSettings.Text = LocalizationManager.Get("settings");
            btnCreate.Text = LocalizationManager.Get("add_note");
            btnSharedCreateNote.Text = "+ " + LocalizationManager.Get("create_note");
            btnSharedCreateGroup.Text = LocalizationManager.Get("create_group");
            btnSharedJoinGroup.Text = LocalizationManager.Get("join_group");
            btnCopyCode.Text = LocalizationManager.Get("copy") + " " + LocalizationManager.Get("invite_code");
            btnLeaveGroup.Text = LocalizationManager.Get("leave_group");
            btnDeleteGroup.Text = LocalizationManager.Get("delete_group");
            cmbTagFilter.Items.Clear();
            cmbTagFilter.Items.Add(LocalizationManager.Get("all_tags"));
            UIHelpers.SetPlaceholder(txtSearch, LocalizationManager.Get("search") + "...");
            UIHelpers.SetPlaceholder(txtSharedGroupName, LocalizationManager.Get("group_name"));
            UIHelpers.SetPlaceholder(txtSharedJoinCode, LocalizationManager.Get("invite_code"));
            UpdateSectionHeader();
            LoadNotes();
            UpdateGroupInfo();
            RenderCurrentTab();
        }

        private void UpdateSectionHeader()
        {
            lblSection.Text = _tab == "passwords"
                ? LocalizationManager.Get("passwords")
                : _tab == "shared"
                    ? LocalizationManager.Get("shared_notes")
                    : LocalizationManager.Get("my_notes");
        }

            btnPasswords.Click += BtnPasswords_Click;
            sidebar.Controls.Add(btnPasswords);
            sidebarY += 50;

            btnShared = new Button
            {
                Text = "Ãðóïè",
                Location = new Point(8, sidebarY),
                Size = new Size(184, 44),
                Font = new Font("Segoe UI Semibold", 10f),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(12, 0, 0, 0)
            };
            // ÂÈÏÐÀÂËÅÍÎ: Ñêèäàºìî _passwordsUnlocked ïðè ïåðåõîä³ íà ³íø³ âêëàäêè
            btnShared.Click += (s, e) =>
            {
                _passwordsUnlocked = false; // Ñêèäàºìî ðîçáëîêóâàííÿ ïàðîë³â
                Program.SessionKey = null;
                _tab = "shared";
                lblSection.Text = "Ñï³ëüí³ íîòàòêè";
                UpdateSidebarButtons();
                RenderCurrentTab();
            };
            sidebar.Controls.Add(btnShared);

            Controls.Add(sidebar);

            // === CONTENT PANEL ===
            contentPanel = new Panel
            {
                Location = new Point(200, 60),
                Size = new Size(ClientSize.Width - 200, ClientSize.Height - 60),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };
            contentPanel.Tag = "transparent";

            lblSection = new Label
            {
                Text = "Ìî¿ íîòàòêè",
                Location = new Point(24, 16),
                Font = new Font("Segoe UI Semibold", 14f),
                AutoSize = true
            };
            contentPanel.Controls.Add(lblSection);

            cmbTagFilter = new ComboBox
            {
                Location = new Point(24, 52),
                Size = new Size(150, 32),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10f)
            };
            cmbTagFilter.Items.Add("(óñ³ òåãè)");
            cmbTagFilter.SelectedIndex = 0;
            cmbTagFilter.SelectedIndexChanged += (s, e) => RenderCurrentTab();
            contentPanel.Controls.Add(cmbTagFilter);

            // === SHARED TAB UI ===
            lstGroups = new ListBox
            {
                Location = new Point(24, 100),
                Size = new Size(180, 160),
                Visible = false,
                Font = new Font("Segoe UI", 10f)
            };
            lstGroups.SelectedIndexChanged += (s, e) =>
            {
                var grp = lstGroups.SelectedItem as Group;
                _selectedGroupId = grp?.Id;
                UpdateGroupInfo();
                RenderSharedCards();
            };
            contentPanel.Controls.Add(lstGroups);

            groupInfoPanel = new Panel
            {
                Location = new Point(24, 270),
                Size = new Size(180, 160),
                Visible = false
            };
            groupInfoPanel.Tag = "transparent";

            lblGroupCode = new Label
            {
                Text = "Êîä: --------",
                Location = new Point(0, 0),
                Font = new Font("Segoe UI", 9f),
                AutoSize = true
            };
            groupInfoPanel.Controls.Add(lblGroupCode);

            btnCopyCode = new Button
            {
                Text = "Êîï³þâàòè êîä",
                Location = new Point(0, 26),
                Size = new Size(176, 34),
                Font = new Font("Segoe UI", 9f)
            };
            btnCopyCode.Click += BtnCopyCode_Click;
            groupInfoPanel.Controls.Add(btnCopyCode);

            btnLeaveGroup = new Button
            {
                Text = "Ïîêèíóòè ãðóïó",
                Location = new Point(0, 66),
                Size = new Size(176, 34),
                Font = new Font("Segoe UI", 9f)
            };
            btnLeaveGroup.Click += BtnLeaveGroup_Click;
            groupInfoPanel.Controls.Add(btnLeaveGroup);

            btnDeleteGroup = new Button
            {
                Text = "Âèäàëèòè ãðóïó",
                Location = new Point(0, 106),
                Size = new Size(176, 34),
                Font = new Font("Segoe UI", 9f)
            };
            btnDeleteGroup.Click += BtnDeleteGroup_Click;
            groupInfoPanel.Controls.Add(btnDeleteGroup);

            contentPanel.Controls.Add(groupInfoPanel);

            lblGroupHeader = new Label
            {
                Text = "Ñï³ëüí³ íîòàòêè",
                Location = new Point(220, 16),
                Font = new Font("Segoe UI Semibold", 14f),
                Visible = false,
                AutoSize = true
            };
            contentPanel.Controls.Add(lblGroupHeader);

            sharedToolbarRow1 = new Panel
            {
                Location = new Point(24, 52),
                Size = new Size(700, 44),
                Visible = false
            };
            sharedToolbarRow1.Tag = "transparent";

            btnSharedCreateNote = new Button
            {
                Text = "+ Íîòàòêà",
                Location = new Point(0, 0),
                Size = new Size(110, 38),
                Font = new Font("Segoe UI Semibold", 9f)
            };
            btnSharedCreateNote.Click += BtnSharedCreateNote_Click;
            sharedToolbarRow1.Controls.Add(btnSharedCreateNote);

            txtSharedGroupName = new TextBox
            {
                Location = new Point(130, 4),
                Size = new Size(140, 32),
                Font = new Font("Segoe UI", 9f)
            };
            UIHelpers.SetPlaceholder(txtSharedGroupName, "Íàçâà ãðóïè");
            sharedToolbarRow1.Controls.Add(txtSharedGroupName);

            btnSharedCreateGroup = new Button
            {
                Text = "Ñòâîðèòè",
                Location = new Point(280, 0),
                Size = new Size(90, 38),
                Font = new Font("Segoe UI", 9f)
            };
            btnSharedCreateGroup.Click += BtnSharedCreateGroup_Click;
            sharedToolbarRow1.Controls.Add(btnSharedCreateGroup);

            txtSharedJoinCode = new TextBox
            {
                Location = new Point(390, 4),
                Size = new Size(100, 32),
                Font = new Font("Segoe UI", 9f)
            };
            UIHelpers.SetPlaceholder(txtSharedJoinCode, "Êîä");
            sharedToolbarRow1.Controls.Add(txtSharedJoinCode);

            btnSharedJoinGroup = new Button
            {
                Text = "Ïðèºäíàòèñÿ",
                Location = new Point(500, 0),
                Size = new Size(110, 38),
                Font = new Font("Segoe UI", 9f)
            };
            btnSharedJoinGroup.Click += BtnSharedJoinGroup_Click;
            sharedToolbarRow1.Controls.Add(btnSharedJoinGroup);

            contentPanel.Controls.Add(sharedToolbarRow1);

            flowCards = new FlowLayoutPanel
            {
                Location = new Point(24, 100),
                Size = new Size(contentPanel.Width - 48, contentPanel.Height - 120),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                AutoScroll = true,
                Padding = new Padding(0),
                WrapContents = true
            };
            contentPanel.Controls.Add(flowCards);

            Controls.Add(contentPanel);
        }

        private void AdjustLayout()
        {
            var availableWidth = flowCards.Width - 30;
            int cardWidth = availableWidth >= 700 ? 320 : (availableWidth >= 500 ? 280 : 240);

            foreach (Control c in flowCards.Controls)
            {
                if (c is NoteCard card)
                {
                    card.Width = cardWidth;
                }
            }
        }

        private void ApplyThemeToCustomControls()
        {
            ThemeManager.StyleButton(btnThemeToggle, Program.CurrentTheme);
            ThemeManager.StyleButton(btnSettings, Program.CurrentTheme);
            ThemeManager.StyleAccentButton(btnCreate);

            ThemeManager.StyleAccentButton(btnSharedCreateNote);
            ThemeManager.StyleButton(btnSharedCreateGroup, Program.CurrentTheme);
            ThemeManager.StyleButton(btnSharedJoinGroup, Program.CurrentTheme);
            ThemeManager.StyleButton(btnCopyCode, Program.CurrentTheme);
            ThemeManager.StyleButton(btnLeaveGroup, Program.CurrentTheme);
            ThemeManager.StyleDangerButton(btnDeleteGroup, Program.CurrentTheme);

            UpdateThemeButtonText();
        }

        private void UpdateThemeButtonText()
        {
            switch (Program.CurrentTheme)
            {
                case Theme.Dark: btnThemeToggle.Text = "Òåìíà"; break;
                case Theme.Ocean: btnThemeToggle.Text = "Îêåàí"; break;
                case Theme.Forest: btnThemeToggle.Text = "Ë³ñ"; break;
                case Theme.Sunset: btnThemeToggle.Text = "Çàõ³ä"; break;
                default: btnThemeToggle.Text = "Ñâ³òëà"; break;
            }
        }

        private void BtnThemeToggle_Click(object sender, EventArgs e)
        {
            Program.TouchActivity();

            var menu = new ContextMenuStrip();
            menu.Font = new Font("Segoe UI", 10f);
            menu.BackColor = ThemeManager.GetSurface(Program.CurrentTheme);
            menu.ForeColor = ThemeManager.GetTextColor(Program.CurrentTheme);

            var themes = new string[] { "Light", "Dark", "Ocean", "Forest", "Sunset" };
            var themeNames = new string[] { "Ñâ³òëà", "Òåìíà", "Îêåàí", "Ë³ñ", "Çàõ³ä ñîíöÿ" };

            for (int i = 0; i < themes.Length; i++)
            {
                var item = new ToolStripMenuItem(themeNames[i]);
                var themeName = themes[i];

                if (ThemeManager.GetThemeName(Program.CurrentTheme) == themeName)
                {
                    item.Checked = true;
                }

                item.Click += (s, ev) =>
                {
                    Program.CurrentTheme = ThemeManager.FromString(themeName);
                    Program.CurrentUser.PreferredTheme = themeName;

                    try { _db.UpdateUserTheme(Program.CurrentUser.Id, themeName); } catch { }

                    ThemeManager.Apply(this, Program.CurrentTheme);
                    ApplyThemeToCustomControls();
                    UpdateSidebarButtons();
                    RenderCurrentTab();
                };

                menu.Items.Add(item);
            }

            menu.Show(btnThemeToggle, new Point(0, btnThemeToggle.Height));
        }

        private void BtnSettings_Click(object sender, EventArgs e)
        {
            Program.TouchActivity();
            using (var settingsForm = new SettingsForm())
            {
                var result = settingsForm.ShowDialog(this);
                if (result == DialogResult.OK)
                {
                    ThemeManager.Apply(this, Program.CurrentTheme);
                    ApplyThemeToCustomControls();
                    UpdateSidebarButtons();
                    UpdateThemeButtonText();
                    RenderCurrentTab();
                }
                    lblGroupCode.Text = LocalizationManager.Get("invite_code") + ": " + grp.InviteCode;
                    lblGroupHeader.Text = grp.Name;
        }

                btnCopyCode.Text = LocalizationManager.Get("copied");
        {
                string.Format(LocalizationManager.Get("delete_group_confirm"), grp.Name),
                LocalizationManager.Get("delete_group"),
                    MessageBox.Show(LocalizationManager.Get("error") + ": " + ex.Message, LocalizationManager.Get("error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

                string.Format(LocalizationManager.Get("leave_group_confirm"), grp.Name),
                LocalizationManager.Get("leave_group"),
                    MessageBox.Show(LocalizationManager.Get("error") + ": " + ex.Message, LocalizationManager.Get("error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            {
            lblSection.Text = LocalizationManager.Get("passwords");
                        lblSection.Text = LocalizationManager.Get("shared_notes");
                        lblSection.Text = _tab == "passwords" ? LocalizationManager.Get("passwords") : LocalizationManager.Get("my_notes");
                MessageBox.Show(LocalizationManager.Get("select_group"), LocalizationManager.Get("info"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                MessageBox.Show(LocalizationManager.Get("enter_group_name"), LocalizationManager.Get("info"), MessageBoxButtons.OK, MessageBoxIcon.Information);
            UIHelpers.SetPlaceholder(txtSharedGroupName, LocalizationManager.Get("group_name"));
                MessageBox.Show(LocalizationManager.Get("enter_invite_code"), LocalizationManager.Get("info"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                MessageBox.Show(LocalizationManager.Get("group_not_found"), LocalizationManager.Get("warning"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            UIHelpers.SetPlaceholder(txtSharedJoinCode, LocalizationManager.Get("invite_code"));

        private void BtnCopyCode_Click(object sender, EventArgs e)
        {
            Program.TouchActivity();
            if (!_selectedGroupId.HasValue) return;

            var grp = _myGroups.FirstOrDefault(g => g.Id == _selectedGroupId.Value);
            if (grp != null)
            {
                Clipboard.SetText(grp.InviteCode);

                var originalText = btnCopyCode.Text;
                btnCopyCode.Text = "Ñêîï³éîâàíî!";
                btnCopyCode.ForeColor = ThemeManager.Success;

                var timer = new Timer { Interval = 2000 };
                timer.Tick += (s, ev) =>
                {
                    btnCopyCode.Text = originalText;
                    ThemeManager.StyleButton(btnCopyCode, Program.CurrentTheme);
                    timer.Stop();
                    timer.Dispose();
                };
                timer.Start();
            }
        }

        private void BtnDeleteGroup_Click(object sender, EventArgs e)
        {
            Program.TouchActivity();
            if (!_selectedGroupId.HasValue) return;

            var grp = _myGroups.FirstOrDefault(g => g.Id == _selectedGroupId.Value);
            if (grp == null) return;

            var result = MessageBox.Show(
                "Âèäàëèòè ãðóïó \"" + grp.Name + "\"?\nÂñ³ íîòàòêè ãðóïè áóäóòü âèäàëåí³!",
                "Âèäàëåííÿ ãðóïè",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (result == DialogResult.Yes)
            {
                try
                {
                    _db.DeleteGroup(grp.Id, Program.CurrentUser.Id);
                    _selectedGroupId = null;
                    LoadGroups();
                    RenderCurrentTab();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ïîìèëêà: " + ex.Message, "Ïîìèëêà", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BtnLeaveGroup_Click(object sender, EventArgs e)
        {
            Program.TouchActivity();
            if (!_selectedGroupId.HasValue) return;

            var grp = _myGroups.FirstOrDefault(g => g.Id == _selectedGroupId.Value);
            if (grp == null) return;

            var result = MessageBox.Show(
                "Ïîêèíóòè ãðóïó \"" + grp.Name + "\"?",
                "Ïîêèíóòè ãðóïó",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (result == DialogResult.Yes)
            {
                try
                {
                    _db.LeaveGroup(grp.Id, Program.CurrentUser.Id);
                    _selectedGroupId = null;
                    LoadGroups();
                    RenderCurrentTab();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ïîìèëêà: " + ex.Message, "Ïîìèëêà", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // ÂÈÏÐÀÂËÅÍÎ: Çàâæäè ïðîñèòè PIN ïðè âõîä³ íà âêëàäêó ïàðîë³â
        private void BtnPasswords_Click(object sender, EventArgs e)
        {
            Program.TouchActivity();

            // ÂÈÏÐÀÂËÅÍÎ: Çàâæäè ïðîñèìî PIN ïðè êîæíîìó âõîä³ íà âêëàäêó
            using (var pinForm = new PinPromptForm(Program.CurrentUser))
            {
                if (pinForm.ShowDialog(this) == DialogResult.OK)
                {
                    _passwordsUnlocked = true;
                }
                else
                {
                    return;
                }
            }

            _tab = "passwords";
            lblSection.Text = "Ïàðîë³";
            UpdateSidebarButtons();
            RenderCurrentTab();
        }

        private void BtnCreate_Click(object sender, EventArgs e)
        {
            Program.TouchActivity();

            string defaultType = _tab == "passwords" ? "password" : "note";
            int? preselectGroupId = _tab == "shared" ? _selectedGroupId : null;

            using (var form = new CreateNoteForm(defaultType, null, _myGroups, preselectGroupId))
            {
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    var note = form.CreatedOrUpdatedNote;
                    note.Id = _db.AddNote(note);
                    LoadNotes();

                    if (note.GroupId.HasValue)
                    {
                        _selectedGroupId = note.GroupId;
                        SelectGroupById(_selectedGroupId);
                        _tab = "shared";
                        lblSection.Text = "Ñï³ëüí³ íîòàòêè";
                    }
                    else
                    {
                        _tab = note.Type == "password" ? "passwords" : "notes";
                        lblSection.Text = _tab == "passwords" ? "Ïàðîë³" : "Ìî¿ íîòàòêè";
                    }

                    UpdateSidebarButtons();
                    RenderCurrentTab();
                }
            }
        }

        private void BtnSharedCreateNote_Click(object sender, EventArgs e)
        {
            Program.TouchActivity();

            if (!_selectedGroupId.HasValue)
            {
                MessageBox.Show("Îáåð³òü ãðóïó.", "Óâàãà", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (var form = new CreateNoteForm("note", null, _myGroups, _selectedGroupId))
            {
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    var note = form.CreatedOrUpdatedNote;
                    note.Id = _db.AddNote(note);
                    LoadNotes();
                    RenderSharedCards();
                }
            }
        }

        private void BtnSharedCreateGroup_Click(object sender, EventArgs e)
        {
            Program.TouchActivity();

            var name = UIHelpers.IsPlaceholder(txtSharedGroupName) ? "" : txtSharedGroupName.Text.Trim();
            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Ââåä³òü íàçâó ãðóïè.", "Óâàãà", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var group = _db.CreateGroup(Program.CurrentUser.Id, name);
            LoadGroups();
            _selectedGroupId = group.Id;
            SelectGroupById(_selectedGroupId);
            RenderCurrentTab();

            txtSharedGroupName.Text = "";
            UIHelpers.SetPlaceholder(txtSharedGroupName, "Íàçâà ãðóïè");
        }

        private void BtnSharedJoinGroup_Click(object sender, EventArgs e)
        {
            Program.TouchActivity();

            var code = UIHelpers.IsPlaceholder(txtSharedJoinCode) ? "" : txtSharedJoinCode.Text.Trim();
            if (string.IsNullOrEmpty(code))
            {
                MessageBox.Show("Ââåä³òü êîä çàïðîøåííÿ.", "Óâàãà", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var group = _db.GetGroupByInvite(code);
            if (group == null)
            {
                MessageBox.Show("Ãðóïó ç òàêèì êîäîì íå çíàéäåíî.", "Ïîìèëêà", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _db.AddMember(group.Id, Program.CurrentUser.Id);
            LoadGroups();
            _selectedGroupId = group.Id;
            SelectGroupById(_selectedGroupId);
            RenderCurrentTab();

            txtSharedJoinCode.Text = "";
            UIHelpers.SetPlaceholder(txtSharedJoinCode, "Êîä");
        }

        private void RenderCurrentTab()
        {
            if (_isRendering) return;
            _isRendering = true;

            try
            {
                if (_tab == "shared")
                {
                    lstGroups.Visible = true;
                    groupInfoPanel.Visible = true;
                    lblGroupHeader.Visible = true;
                    sharedToolbarRow1.Visible = true;
                    cmbTagFilter.Visible = false;

                    flowCards.Location = new Point(220, 100);
                    flowCards.Size = new Size(contentPanel.Width - 244, contentPanel.Height - 120);

                    ApplyThemeToCustomControls();
                    UpdateGroupInfo();
                    RenderSharedCards();
                }
                else
                {
                    lstGroups.Visible = false;
                    groupInfoPanel.Visible = false;
                    lblGroupHeader.Visible = false;
                    sharedToolbarRow1.Visible = false;
                    cmbTagFilter.Visible = true;

                    flowCards.Location = new Point(24, 100);
                    flowCards.Size = new Size(contentPanel.Width - 48, contentPanel.Height - 120);

                    if (_tab == "notes")
                    {
                        RenderNotesCards();
                    }
                    else if (_tab == "passwords")
                    {
                        RenderPasswordCards();
                    }
                }
            }
            finally
            {
                _isRendering = false;
            }
        }

        private bool IsPlaceholder(TextBox tb) => UIHelpers.IsPlaceholder(tb);

        private void RenderNotesCards()
        {
            flowCards.SuspendLayout();
            flowCards.Controls.Clear();

            var notes = _allNotes.Where(n => n.Type == "note" && !n.GroupId.HasValue).Where(TagMatches).Where(TextMatches);

            foreach (var note in notes)
            {
                var card = new NoteCard();
                card.ApplyTheme(Program.CurrentTheme);
                card.Bind(note, null);
                card.DeleteRequested += Card_DeleteRequested;
                card.EditRequested += Card_EditRequested;
                flowCards.Controls.Add(card);
            }

            flowCards.ResumeLayout();
        }

        private void RenderPasswordCards()
        {
            flowCards.SuspendLayout();
            flowCards.Controls.Clear();

            var notes = _allNotes.Where(n => n.Type == "password" && !n.GroupId.HasValue).Where(TagMatches).Where(TextMatches);

            foreach (var note in notes)
            {
                string decrypted = null;
                if (_passwordsUnlocked && Program.SessionKey != null && note.IsEncrypted)
                {
                    try
                    {
                        decrypted = CryptoService.DecryptAes(note.IvBase64, note.Content, Program.SessionKey);
                    }
                    catch { decrypted = "(ïîìèëêà)"; }
                }

                var card = new NoteCard();
                card.ApplyTheme(Program.CurrentTheme);
                card.Bind(note, decrypted);
                card.DeleteRequested += Card_DeleteRequested;
                card.EditRequested += Card_EditRequested;
                flowCards.Controls.Add(card);
            }

            flowCards.ResumeLayout();
        }

        private void RenderSharedCards()
        {
            flowCards.SuspendLayout();
            flowCards.Controls.Clear();

            if (_selectedGroupId == null && lstGroups.Items.Count > 0 && lstGroups.SelectedItem == null)
            {
                lstGroups.SelectedIndex = 0;
                _selectedGroupId = (lstGroups.SelectedItem as Group)?.Id;
                UpdateGroupInfo();
            }

            if (!_selectedGroupId.HasValue)
            {
                var hint = new Label
                {
                    Text = LocalizationManager.Get("select_group"),
                    AutoSize = true,
                    Font = new Font("Segoe UI", 10f),
                    ForeColor = ThemeManager.GetTextSecondary(Program.CurrentTheme)
                };
                flowCards.Controls.Add(hint);
                flowCards.ResumeLayout();
                return;
            }

            var notes = _db.GetNotesForGroup(_selectedGroupId.Value).Where(TagMatches).Where(TextMatches);

            foreach (var note in notes)
            {
                var card = new NoteCard();
                card.ApplyTheme(Program.CurrentTheme);
                card.Bind(note, null);
                card.DeleteRequested += Card_DeleteRequested;
                card.EditRequested += Card_EditRequested;
                flowCards.Controls.Add(card);
            }

            flowCards.ResumeLayout();
        }

        private void Card_DeleteRequested(object sender, int noteId)
        {
            if (MessageBox.Show(LocalizationManager.Get("confirm_delete_note"), LocalizationManager.Get("confirmation"), MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                _db.DeleteNote(noteId);
                LoadNotes();
                RenderCurrentTab();
            }
        }

        private void Card_EditRequested(object sender, int noteId)
        {
            var n = _db.GetNoteById(noteId);
            using (var modal = new CreateNoteForm(n.Type, n, _myGroups, n.GroupId))
            {
                if (modal.ShowDialog(this) == DialogResult.OK)
                {
                    var updated = modal.CreatedOrUpdatedNote;
                    _db.UpdateNote(updated);
                    LoadNotes();

                    if (updated.GroupId.HasValue)
                    {
                        _selectedGroupId = updated.GroupId;
                        SelectGroupById(_selectedGroupId);
                        _tab = "shared";
                        lblSection.Text = LocalizationManager.Get("shared_notes");
                    }
                    else
                    {
                        _tab = updated.Type == "password" ? "passwords" : "notes";
                        lblSection.Text = _tab == "passwords" ? LocalizationManager.Get("passwords") : LocalizationManager.Get("my_notes");
                    }

                    UpdateSidebarButtons();
                    RenderCurrentTab();
                }
            }
        }

        private void LoadNotes()
        {
            _allNotes = _db.GetNotesForUser(Program.CurrentUser.Id);

            var tags = _allNotes
                .SelectMany(n => (n.Tags ?? "").Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
                .Select(t => t.Trim())
                .Where(t => t.Length > 0)
                .Distinct()
                .OrderBy(t => t)
                .ToList();

            cmbTagFilter.Items.Clear();
            cmbTagFilter.Items.Add(LocalizationManager.Get("all_tags"));
            foreach (var t in tags) cmbTagFilter.Items.Add(t);
            if (cmbTagFilter.Items.Count > 0) cmbTagFilter.SelectedIndex = 0;
        }

        private void LoadGroups()
        {
            _myGroups = _db.GetGroupsForUser(Program.CurrentUser.Id);
            lstGroups.Items.Clear();
            foreach (var g in _myGroups) lstGroups.Items.Add(g);
            lstGroups.DisplayMember = "Name";

            if (_selectedGroupId.HasValue) SelectGroupById(_selectedGroupId);
        }

        private void SelectGroupById(int? groupId)
        {
            if (!groupId.HasValue) return;
            var idx = _myGroups.FindIndex(g => g.Id == groupId.Value);
            if (idx >= 0) lstGroups.SelectedIndex = idx;
        }

        private bool TagMatches(Note n)
        {
            var selectedTag = cmbTagFilter.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(selectedTag) || selectedTag == LocalizationManager.Get("all_tags")) return true;
            var tags = (n.Tags ?? "").Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim());
            return tags.Contains(selectedTag);
        }

        private bool TextMatches(Note n)
        {
            var q = IsPlaceholder(txtSearch) ? "" : txtSearch.Text.Trim();
            if (string.IsNullOrEmpty(q)) return true;

            string contentToSearch = "";
            if (!string.IsNullOrEmpty(n.Content))
            {
                if (n.Content.StartsWith("{\\rtf"))
                {
                    contentToSearch = ExtractPlainTextFromRtf(n.Content);
                }
                else
                {
                    contentToSearch = n.Content;
                }
            }

            return (n.Title ?? "").IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0
                   || (n.Type == "password" ? "" : contentToSearch).IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private string ExtractPlainTextFromRtf(string rtf)
        {
            try
            {
                using (var rtb = new RichTextBox())
                {
                    rtb.Rtf = rtf;
                    return rtb.Text;
                }
            }
            catch
            {
                return rtf;
            }
        }
    }
}
