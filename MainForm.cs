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
            ThemeManager.Apply(this, Program.CurrentTheme);
            ApplyThemeToCustomControls();
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

            LocalizationManager.LanguageChanged += ApplyLocalization;
            ApplyLocalization();
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
            UIHelpers.SetPlaceholder(txtSearch, LocalizationManager.Get("search"));
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
                Text = LocalizationManager.Get("themes"),
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
            // ВИПРАВЛЕНО: Скидаємо _passwordsUnlocked при переході на інші вкладки
            btnNotes.Click += (s, e) =>
            {
                _passwordsUnlocked = false; // Скидаємо розблокування паролів
                Program.SessionKey = null;
                _tab = "notes";
                UpdateSectionTitle();
                UpdateSidebarButtons();
                RenderCurrentTab();
            };
            sidebar.Controls.Add(btnNotes);
            sidebarY += 50;

            btnPasswords = new Button
            {
                Text = LocalizationManager.Get("passwords"),
                Location = new Point(8, sidebarY),
                Size = new Size(184, 44),
                Font = new Font("Segoe UI Semibold", 10f),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(12, 0, 0, 0)
            };
            btnPasswords.Click += BtnPasswords_Click;
            sidebar.Controls.Add(btnPasswords);
            sidebarY += 50;

            btnShared = new Button
            {
                Text = LocalizationManager.Get("shared_notes"),
                Location = new Point(8, sidebarY),
                Size = new Size(184, 44),
                Font = new Font("Segoe UI Semibold", 10f),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(12, 0, 0, 0)
            };
            // ВИПРАВЛЕНО: Скидаємо _passwordsUnlocked при переході на інші вкладки
            btnShared.Click += (s, e) =>
            {
                _passwordsUnlocked = false; // Скидаємо розблокування паролів
                Program.SessionKey = null;
                _tab = "shared";
                UpdateSectionTitle();
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
                Text = LocalizationManager.Get("notes"),
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
            cmbTagFilter.Items.Add(LocalizationManager.Get("all_tags"));
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
                Text = LocalizationManager.Get("invite_code") + ": --------",
                Location = new Point(0, 0),
                Font = new Font("Segoe UI", 9f),
                AutoSize = true
            };
            groupInfoPanel.Controls.Add(lblGroupCode);

            btnCopyCode = new Button
            {
                Text = LocalizationManager.Get("copy_code"),
                Location = new Point(0, 26),
                Size = new Size(176, 34),
                Font = new Font("Segoe UI", 9f)
            };
            btnCopyCode.Click += BtnCopyCode_Click;
            groupInfoPanel.Controls.Add(btnCopyCode);

            btnLeaveGroup = new Button
            {
                Text = LocalizationManager.Get("leave_group"),
                Location = new Point(0, 66),
                Size = new Size(176, 34),
                Font = new Font("Segoe UI", 9f)
            };
            btnLeaveGroup.Click += BtnLeaveGroup_Click;
            groupInfoPanel.Controls.Add(btnLeaveGroup);

            btnDeleteGroup = new Button
            {
                Text = LocalizationManager.Get("delete_group"),
                Location = new Point(0, 106),
                Size = new Size(176, 34),
                Font = new Font("Segoe UI", 9f)
            };
            btnDeleteGroup.Click += BtnDeleteGroup_Click;
            groupInfoPanel.Controls.Add(btnDeleteGroup);

            contentPanel.Controls.Add(groupInfoPanel);

            lblGroupHeader = new Label
            {
                Text = LocalizationManager.Get("shared_notes"),
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
                Text = LocalizationManager.Get("add_note"),
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
            UIHelpers.SetPlaceholder(txtSharedGroupName, LocalizationManager.Get("group_name"));
            sharedToolbarRow1.Controls.Add(txtSharedGroupName);

            btnSharedCreateGroup = new Button
            {
                Text = LocalizationManager.Get("create"),
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
            UIHelpers.SetPlaceholder(txtSharedJoinCode, LocalizationManager.Get("invite_code"));
            sharedToolbarRow1.Controls.Add(txtSharedJoinCode);

            btnSharedJoinGroup = new Button
            {
                Text = LocalizationManager.Get("join_group"),
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

        private void ApplyLocalization()
        {
            Text = $"{LocalizationManager.Get("app_title")} - {Program.CurrentUser.Username}";
            btnCreate.Text = LocalizationManager.Get("add_note");
            btnThemeToggle.Text = LocalizationManager.Get("themes");
            btnSettings.Text = LocalizationManager.Get("settings");
            btnNotes.Text = LocalizationManager.Get("notes");
            btnPasswords.Text = LocalizationManager.Get("passwords");
            btnShared.Text = LocalizationManager.Get("shared_notes");
            lblGroupHeader.Text = LocalizationManager.Get("shared_notes");
            btnCopyCode.Text = LocalizationManager.Get("copy_code");
            btnLeaveGroup.Text = LocalizationManager.Get("leave_group");
            btnDeleteGroup.Text = LocalizationManager.Get("delete_group");
            btnSharedCreateGroup.Text = LocalizationManager.Get("create");
            btnSharedJoinGroup.Text = LocalizationManager.Get("join_group");
            UIHelpers.SetPlaceholder(txtSearch, LocalizationManager.Get("search"));
            UIHelpers.SetPlaceholder(txtSharedGroupName, LocalizationManager.Get("group_name"));
            UIHelpers.SetPlaceholder(txtSharedJoinCode, LocalizationManager.Get("invite_code"));
            LoadNotes();
            UpdateSectionTitle();
            UpdateGroupInfo();
        }

        private void UpdateSectionTitle()
        {
            lblSection.Text = _tab == "passwords"
                ? LocalizationManager.Get("passwords")
                : _tab == "shared"
                    ? LocalizationManager.Get("shared_notes")
                    : LocalizationManager.Get("notes");
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
                case Theme.Dark: btnThemeToggle.Text = "Темна"; break;
                case Theme.Ocean: btnThemeToggle.Text = "Океан"; break;
                case Theme.Forest: btnThemeToggle.Text = "Ліс"; break;
                case Theme.Sunset: btnThemeToggle.Text = "Захід"; break;
                default: btnThemeToggle.Text = "Світла"; break;
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
            var themeNames = new string[] { "Світла", "Темна", "Океан", "Ліс", "Захід сонця" };

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
                // Якщо результат Abort - MainForm вже буде закрито через Owner.Close()
            }
        }

        private void UpdateSidebarButtons()
        {
            ThemeManager.StyleSidebarButton(btnNotes, Program.CurrentTheme, _tab == "notes");
            ThemeManager.StyleSidebarButton(btnPasswords, Program.CurrentTheme, _tab == "passwords");
            ThemeManager.StyleSidebarButton(btnShared, Program.CurrentTheme, _tab == "shared");
        }

        private void UpdateGroupInfo()
        {
            if (_selectedGroupId.HasValue)
            {
                var grp = _myGroups.FirstOrDefault(g => g.Id == _selectedGroupId.Value);
                if (grp != null)
                {
                    lblGroupCode.Text = $"{LocalizationManager.Get("invite_code")}: {grp.InviteCode}";
                    lblGroupHeader.Text = $"{LocalizationManager.Get("groups")}: {grp.Name}";
                    btnDeleteGroup.Visible = grp.OwnerId == Program.CurrentUser.Id;
                    btnLeaveGroup.Visible = grp.OwnerId != Program.CurrentUser.Id;
                }
            }
        }

        private void BtnCopyCode_Click(object sender, EventArgs e)
        {
            Program.TouchActivity();
            if (!_selectedGroupId.HasValue) return;

            var grp = _myGroups.FirstOrDefault(g => g.Id == _selectedGroupId.Value);
            if (grp != null)
            {
                Clipboard.SetText(grp.InviteCode);

                var originalText = btnCopyCode.Text;
                btnCopyCode.Text = LocalizationManager.Get("copied");
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
                "Видалити групу \"" + grp.Name + "\"?\nВсі нотатки групи будуть видалені!",
                "Видалення групи",
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
                    MessageBox.Show("Помилка: " + ex.Message, "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                "Покинути групу \"" + grp.Name + "\"?",
                "Покинути групу",
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
                    MessageBox.Show("Помилка: " + ex.Message, "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // ВИПРАВЛЕНО: Завжди просити PIN при вході на вкладку паролів
        private void BtnPasswords_Click(object sender, EventArgs e)
        {
            Program.TouchActivity();

            // ВИПРАВЛЕНО: Завжди просимо PIN при кожному вході на вкладку
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
            lblSection.Text = LocalizationManager.Get("passwords");
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
                        UpdateSectionTitle();
                    }
                    else
                    {
                        _tab = note.Type == "password" ? "passwords" : "notes";
                        UpdateSectionTitle();
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
                MessageBox.Show("Оберіть групу.", "Увага", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                MessageBox.Show("Введіть назву групи.", "Увага", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var group = _db.CreateGroup(Program.CurrentUser.Id, name);
            LoadGroups();
            _selectedGroupId = group.Id;
            SelectGroupById(_selectedGroupId);
            RenderCurrentTab();

            txtSharedGroupName.Text = "";
            UIHelpers.SetPlaceholder(txtSharedGroupName, LocalizationManager.Get("group_name"));
        }

        private void BtnSharedJoinGroup_Click(object sender, EventArgs e)
        {
            Program.TouchActivity();

            var code = UIHelpers.IsPlaceholder(txtSharedJoinCode) ? "" : txtSharedJoinCode.Text.Trim();
            if (string.IsNullOrEmpty(code))
            {
                MessageBox.Show("Введіть код запрошення.", "Увага", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var group = _db.GetGroupByInvite(code);
            if (group == null)
            {
                MessageBox.Show("Групу з таким кодом не знайдено.", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _db.AddMember(group.Id, Program.CurrentUser.Id);
            LoadGroups();
            _selectedGroupId = group.Id;
            SelectGroupById(_selectedGroupId);
            RenderCurrentTab();

            txtSharedJoinCode.Text = "";
            UIHelpers.SetPlaceholder(txtSharedJoinCode, LocalizationManager.Get("invite_code"));
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
                    catch { decrypted = "(помилка)"; }
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
            if (MessageBox.Show(LocalizationManager.Get("delete") + "?", LocalizationManager.Get("warning"), MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
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
                        UpdateSectionTitle();
                    }
                    else
                    {
                        _tab = updated.Type == "password" ? "passwords" : "notes";
                        UpdateSectionTitle();
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
