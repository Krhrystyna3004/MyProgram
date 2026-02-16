using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace SecureNotes
{

    public class CreateNoteForm : Form
    {
        public Note CreatedOrUpdatedNote { get; private set; }
        private Note EditingNote;

        private TextBox txtTitle;
        private RichTextBox rtbContent;
        private RadioButton rbNote;
        private RadioButton rbPassword;
        private ComboBox cmbColor;
        private TextBox txtTags;
        private CheckBox chkShared;
        private ComboBox cmbGroup;
        private Button btnSave;
        private Button btnFullscreen;
        private FlowLayoutPanel toolbarPanel;
        private FlowLayoutPanel attachmentsPanel;
        private FlowLayoutPanel imagePreviewPanel;
        private List<string> _attachments = new List<string>();

        private Button btnBold, btnItalic, btnUnderline, btnStrikethrough;
        private Button btnHighlight, btnTextColor, btnBulletList, btnAttach;
        private Button btnUndo, btnRedo;
        private ComboBox cmbFontSize;
        private Button btnAlignLeft, btnAlignCenter, btnAlignRight;

        private Label lblTitle, lblContent, lblType, lblColor, lblTags, lblGroup;

        private Panel bottomPanel;

        private readonly List<Group> _groups;
        private readonly int? _preselectGroupId;
        private bool _isFullscreen = false;
        private FormWindowState _previousState;
        private FormBorderStyle _previousBorder;
        private Size _previousSize;
        private Point _previousLocation;

        private readonly List<ColorItem> _colors = new List<ColorItem>
        {
            new ColorItem("pink", "#FFD6E8"),
            new ColorItem("blue", "#E6F3FF"),
            new ColorItem("mint", "#E8F5E9"),
            new ColorItem("peach", "#FFF4E6"),
            new ColorItem("lavender", "#F3E5F5"),
            new ColorItem("yellow", "#FFF9C4"),
            new ColorItem("white", "#FFFFFF"),
        };

        public CreateNoteForm(string defaultType, Note toEdit, List<Group> groups, int? preselectGroupId)
        {
            EditingNote = toEdit;
            _groups = groups ?? new List<Group>();
            _preselectGroupId = preselectGroupId;

            InitializeModernUI(defaultType, toEdit);
        }

        private void InitializeModernUI(string defaultType, Note toEdit)
        {
            Text = toEdit == null ? LocalizationManager.Get("create_note") : LocalizationManager.Get("edit_note");
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(750, 800);
            MinimumSize = new Size(600, 700);
            FormBorderStyle = FormBorderStyle.Sizable;
            MaximizeBox = true;
            MinimizeBox = true;
            Font = new Font("Segoe UI", 10f);
            AutoScroll = false;

            int padding = 20;
            int y = padding;

            // === ВЕРХНЯ ЧАСТИНА (фіксована) ===

            // Заголовок + кнопка fullscreen
            var topPanel = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(ClientSize.Width, 80),
                Dock = DockStyle.Top
            };
            topPanel.Tag = "transparent";

            lblTitle = new Label
            {
                Text = LocalizationManager.Get("title"),
                Location = new Point(padding, 10),
                AutoSize = true,
                Font = new Font("Segoe UI Semibold", 10f)
            };
            topPanel.Controls.Add(lblTitle);

            btnFullscreen = new Button
            {
                Text = "\u26F6",
                Size = new Size(36, 28),
                Location = new Point(ClientSize.Width - padding - 50, 6),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Font = new Font("Segoe UI", 12f),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnFullscreen.Click += BtnFullscreen_Click;
            topPanel.Controls.Add(btnFullscreen);

            txtTitle = new TextBox
            {
                Location = new Point(padding, 34),
                Size = new Size(ClientSize.Width - padding * 2 - 50, 32),
                Font = new Font("Segoe UI", 11f),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            topPanel.Controls.Add(txtTitle);

            Controls.Add(topPanel);

            // === НИЖНЯ ПАНЕЛЬ (звичайна Panel з AutoScroll) ===
            bottomPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 350,
                AutoScroll = true,
                Padding = new Padding(0)
            };
            bottomPanel.Tag = "transparent";

            int bottomY = 10;

            // Єдина панель для ВСІХ вкладень -- вертикальний список зі скролом
            imagePreviewPanel = new FlowLayoutPanel
            {
                Location = new Point(padding, bottomY),
                Size = new Size(ClientSize.Width - padding * 2, 0),
                AutoSize = false,
                AutoScroll = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Visible = false,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            imagePreviewPanel.Tag = "transparent";
            bottomPanel.Controls.Add(imagePreviewPanel);

            // attachmentsPanel -- не використовується окремо, зберігаємо для сумісності
            attachmentsPanel = new FlowLayoutPanel
            {
                Size = new Size(0, 0),
                Visible = false
            };

            bottomY += 10;

            // Тип нотатки
            lblType = new Label
            {
                Text = LocalizationManager.Get("type"),
                Location = new Point(padding, bottomY),
                AutoSize = true,
                Font = new Font("Segoe UI Semibold", 10f)
            };
            bottomPanel.Controls.Add(lblType);
            bottomY += 24;

            // RadioButtons
            rbNote = new RadioButton
            {
                Text = LocalizationManager.Get("regular"),
                Location = new Point(padding, bottomY),
                AutoSize = true,
                Checked = (toEdit?.Type ?? defaultType) == "note",
                Font = new Font("Segoe UI", 10f)
            };
            bottomPanel.Controls.Add(rbNote);

            rbPassword = new RadioButton
            {
                Text = LocalizationManager.Get("password"),
                Location = new Point(padding + 130, bottomY),
                AutoSize = true,
                Checked = (toEdit?.Type ?? defaultType) == "password",
                Font = new Font("Segoe UI", 10f)
            };
            bottomPanel.Controls.Add(rbPassword);
            bottomY += 34;

            // Колір + Теги
            lblColor = new Label
            {
                Text = LocalizationManager.Get("color"),
                Location = new Point(padding, bottomY),
                AutoSize = true,
                Font = new Font("Segoe UI", 9f)
            };
            bottomPanel.Controls.Add(lblColor);

            lblTags = new Label
            {
                Text = LocalizationManager.Get("tags"),
                Location = new Point(padding + 180, bottomY),
                AutoSize = true,
                Font = new Font("Segoe UI", 9f)
            };
            bottomPanel.Controls.Add(lblTags);
            bottomY += 22;

            cmbColor = new ComboBox
            {
                Location = new Point(padding, bottomY),
                Size = new Size(160, 32),
                DropDownStyle = ComboBoxStyle.DropDownList,
                DrawMode = DrawMode.OwnerDrawFixed,
                ItemHeight = 26,
                Font = new Font("Segoe UI", 10f)
            };
            cmbColor.DrawItem += CmbColor_DrawItem;
            foreach (var color in _colors) cmbColor.Items.Add(color);
            cmbColor.SelectedIndex = 0;
            bottomPanel.Controls.Add(cmbColor);

            txtTags = new TextBox
            {
                Location = new Point(padding + 180, bottomY),
                Size = new Size(ClientSize.Width - padding * 2 - 180, 32),
                Font = new Font("Segoe UI", 10f),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            bottomPanel.Controls.Add(txtTags);
            bottomY += 40;

            // Спільна нотатка
            chkShared = new CheckBox
            {
                Text = LocalizationManager.Get("shared_note"),
                Location = new Point(padding, bottomY),
                AutoSize = true,
                Font = new Font("Segoe UI", 10f)
            };
            bottomPanel.Controls.Add(chkShared);
            bottomY += 30;

            // Група
            lblGroup = new Label
            {
                Text = LocalizationManager.Get("group"),
                Location = new Point(padding, bottomY),
                AutoSize = true,
                Font = new Font("Segoe UI", 9f)
            };
            bottomPanel.Controls.Add(lblGroup);
            bottomY += 22;

            cmbGroup = new ComboBox
            {
                Location = new Point(padding, bottomY),
                Size = new Size(ClientSize.Width - padding * 2, 32),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Enabled = false,
                Font = new Font("Segoe UI", 10f),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            bottomPanel.Controls.Add(cmbGroup);
            bottomY += 40;

            chkShared.CheckedChanged += (s, e) =>
            {
                cmbGroup.Enabled = chkShared.Checked;
                if (chkShared.Checked)
                {
                    PopulateGroups();
                    PreselectGroup();
                }
            };

            // Кнопка зберегти
            btnSave = new Button
            {
                Text = toEdit == null ? LocalizationManager.Get("create") : LocalizationManager.Get("save"),
                Location = new Point(padding, bottomY),
                Size = new Size(ClientSize.Width - padding * 2, 46),
                Font = new Font("Segoe UI Semibold", 11f),
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            btnSave.Click += BtnSave_Click;
            bottomPanel.Controls.Add(btnSave);
            bottomY += 56;

            bottomPanel.AutoScrollMinSize = new Size(0, bottomY);

            Controls.Add(bottomPanel);

            // === ПАНЕЛЬ З ТУЛБАРОМ І КОНТЕНТОМ ===
            var contentPanel = new Panel
            {
                Location = new Point(0, 80),
                Size = new Size(ClientSize.Width, ClientSize.Height - 80 - bottomPanel.Height),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
            };
            contentPanel.Tag = "transparent";

            lblContent = new Label
            {
                Text = LocalizationManager.Get("content"),
                Location = new Point(padding, 0),
                AutoSize = true,
                Font = new Font("Segoe UI Semibold", 10f)
            };
            contentPanel.Controls.Add(lblContent);

            // Toolbar
            toolbarPanel = new FlowLayoutPanel
            {
                Location = new Point(padding, 26),
                Size = new Size(ClientSize.Width - padding * 2, 90),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                WrapContents = true,
                AutoSize = false
            };
            toolbarPanel.Tag = "transparent";

            btnUndo = CreateToolbarButton("\u21A9", "Undo");
            btnUndo.Click += (s, e) => rtbContent.Undo();
            toolbarPanel.Controls.Add(btnUndo);

            btnRedo = CreateToolbarButton("\u21AA", "Redo");
            btnRedo.Click += (s, e) => rtbContent.Redo();
            toolbarPanel.Controls.Add(btnRedo);

            toolbarPanel.Controls.Add(CreateSeparator());

            btnBold = CreateToolbarButton("B", "Bold");
            btnBold.Font = new Font("Segoe UI", 10f, FontStyle.Bold);
            btnBold.Click += (s, e) => ApplyStyle(FontStyle.Bold);
            toolbarPanel.Controls.Add(btnBold);

            btnItalic = CreateToolbarButton("I", "Italic");
            btnItalic.Font = new Font("Segoe UI", 10f, FontStyle.Italic);
            btnItalic.Click += (s, e) => ApplyStyle(FontStyle.Italic);
            toolbarPanel.Controls.Add(btnItalic);

            btnUnderline = CreateToolbarButton("U", "Underline");
            btnUnderline.Font = new Font("Segoe UI", 10f, FontStyle.Underline);
            btnUnderline.Click += (s, e) => ApplyStyle(FontStyle.Underline);
            toolbarPanel.Controls.Add(btnUnderline);

            btnStrikethrough = CreateToolbarButton("S", "Strikethrough");
            btnStrikethrough.Font = new Font("Segoe UI", 10f, FontStyle.Strikeout);
            btnStrikethrough.Click += (s, e) => ApplyStyle(FontStyle.Strikeout);
            toolbarPanel.Controls.Add(btnStrikethrough);

            toolbarPanel.Controls.Add(CreateSeparator());

            var lblFontSize = new Label
            {
                Text = LocalizationManager.Get("size"),
                AutoSize = true,
                Font = new Font("Segoe UI", 9f),
                Margin = new Padding(5, 10, 0, 0)
            };
            toolbarPanel.Controls.Add(lblFontSize);

            cmbFontSize = new ComboBox
            {
                Size = new Size(55, 30),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9f),
                Margin = new Padding(2, 5, 5, 0)
            };
            cmbFontSize.Items.AddRange(new object[] { "8", "9", "10", "11", "12", "14", "16", "18", "20", "24", "28", "32" });
            cmbFontSize.SelectedIndex = 4;
            cmbFontSize.SelectedIndexChanged += CmbFontSize_SelectedIndexChanged;
            toolbarPanel.Controls.Add(cmbFontSize);

            toolbarPanel.Controls.Add(CreateSeparator());

            btnHighlight = CreateToolbarButton("\U0001F58D", "Highlight");
            btnHighlight.BackColor = Color.FromArgb(255, 255, 150);
            btnHighlight.Click += BtnHighlight_Click;
            toolbarPanel.Controls.Add(btnHighlight);

            btnTextColor = CreateToolbarButton("A", "Text Color");
            btnTextColor.ForeColor = Color.Red;
            btnTextColor.Font = new Font("Segoe UI", 10f, FontStyle.Bold);
            btnTextColor.Click += BtnTextColor_Click;
            toolbarPanel.Controls.Add(btnTextColor);

            toolbarPanel.Controls.Add(CreateSeparator());

            btnAlignLeft = CreateToolbarButton("\u2B77", "Left");
            btnAlignLeft.Click += (s, e) => rtbContent.SelectionAlignment = HorizontalAlignment.Left;
            toolbarPanel.Controls.Add(btnAlignLeft);

            btnAlignCenter = CreateToolbarButton("\u2630", "Center");
            btnAlignCenter.Click += (s, e) => rtbContent.SelectionAlignment = HorizontalAlignment.Center;
            toolbarPanel.Controls.Add(btnAlignCenter);

            btnAlignRight = CreateToolbarButton("\u2B78", "Right");
            btnAlignRight.Click += (s, e) => rtbContent.SelectionAlignment = HorizontalAlignment.Right;
            toolbarPanel.Controls.Add(btnAlignRight);

            toolbarPanel.Controls.Add(CreateSeparator());

            btnBulletList = CreateToolbarButton("\u2022", "Bullet List");
            btnBulletList.Click += BtnBulletList_Click;
            toolbarPanel.Controls.Add(btnBulletList);

            btnAttach = new Button
            {
                Text = "\U0001F4CE " + LocalizationManager.Get("attach_file"),
                Size = new Size(85, 34),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 9f),
                Margin = new Padding(5, 3, 0, 0)
            };
            btnAttach.Click += BtnAttach_Click;
            toolbarPanel.Controls.Add(btnAttach);

            contentPanel.Controls.Add(toolbarPanel);

            // RichTextBox
            rtbContent = new RichTextBox
            {
                Location = new Point(padding, 120),
                Size = new Size(ClientSize.Width - padding * 2, contentPanel.Height - 130),
                Font = new Font("Segoe UI", 11f),
                BorderStyle = BorderStyle.FixedSingle,
                AcceptsTab = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
            };
            rtbContent.KeyDown += RtbContent_KeyDown;
            contentPanel.Controls.Add(rtbContent);

            Controls.Add(contentPanel);

            // Застосовуємо тему
            this.Load += (s, e) =>
            {
                ThemeManager.Apply(this, Program.CurrentTheme);
                ThemeManager.StyleAccentButton(btnSave);
                ThemeManager.StyleButton(btnFullscreen, Program.CurrentTheme);
                StyleToolbarButtons();
            };

            // Заповнюємо дані при редагуванні
            if (toEdit != null)
            {
                txtTitle.Text = toEdit.Title;
                LoadNoteContent(toEdit);
                SelectColorByHex(toEdit.Color);
                txtTags.Text = toEdit.Tags ?? "";

                if (!string.IsNullOrEmpty(toEdit.Attachments))
                {
                    _attachments = new List<string>(toEdit.Attachments.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries));
                    UpdateAttachmentsPanel();
                }

                if (toEdit.GroupId.HasValue)
                {
                    chkShared.Checked = true;
                    cmbGroup.Enabled = true;
                    PopulateGroups();
                    var idx = _groups.FindIndex(g => g.Id == toEdit.GroupId.Value);
                    if (idx >= 0) cmbGroup.SelectedIndex = idx;
                }
            }
            else if (_preselectGroupId.HasValue)
            {
                chkShared.Checked = true;
                cmbGroup.Enabled = true;
                PopulateGroups();
                PreselectGroup();
            }
        }

        private Panel CreateSeparator()
        {
            return new Panel
            {
                Size = new Size(2, 30),
                BackColor = Color.FromArgb(180, 180, 180),
                Margin = new Padding(5, 5, 5, 0)
            };
        }

        private Button CreateToolbarButton(string text, string tooltip)
        {
            var btn = new Button
            {
                Text = text,
                Size = new Size(34, 34),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 10f),
                Margin = new Padding(2, 3, 2, 0)
            };
            btn.FlatAppearance.BorderSize = 1;

            var tip = new ToolTip();
            tip.SetToolTip(btn, tooltip);

            return btn;
        }

        private void StyleToolbarButtons()
        {
            var theme = Program.CurrentTheme;
            foreach (Control c in toolbarPanel.Controls)
            {
                if (c is Button btn)
                {
                    if (btn == btnHighlight)
                    {
                        btn.FlatAppearance.BorderColor = ThemeManager.GetBorder(theme);
                    }
                    else if (btn == btnTextColor)
                    {
                        ThemeManager.StyleButton(btn, theme);
                        btn.ForeColor = Color.Red;
                    }
                    else if (btn == btnAttach)
                    {
                        ThemeManager.StyleButton(btn, theme);
                    }
                    else
                    {
                        ThemeManager.StyleButton(btn, theme);
                    }
                }
            }
        }

        private void CmbFontSize_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (rtbContent.SelectionLength == 0) return;

            if (float.TryParse(cmbFontSize.SelectedItem?.ToString(), out float size))
            {
                var currentFont = rtbContent.SelectionFont ?? rtbContent.Font;
                rtbContent.SelectionFont = new Font(currentFont.FontFamily, size, currentFont.Style);
            }
            rtbContent.Focus();
        }

        private string CopyFileToAppFolder(string sourcePath)
        {
            try
            {
                string appFolder = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "SecureNotes",
                    "Attachments"
                );

                if (!Directory.Exists(appFolder))
                {
                    Directory.CreateDirectory(appFolder);
                }

                string fileName = Path.GetFileName(sourcePath);
                string uniqueName = $"{DateTime.Now:yyyyMMdd_HHmmss}_{Guid.NewGuid().ToString("N").Substring(0, 8)}_{fileName}";
                string destPath = Path.Combine(appFolder, uniqueName);

                File.Copy(sourcePath, destPath, true);

                return destPath;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не вдалося скопіювати файл: {ex.Message}", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return sourcePath;
            }
        }

        private void BtnFullscreen_Click(object sender, EventArgs e)
        {
            if (!_isFullscreen)
            {
                _previousState = WindowState;
                _previousBorder = FormBorderStyle;
                _previousSize = Size;
                _previousLocation = Location;

                FormBorderStyle = FormBorderStyle.None;
                WindowState = FormWindowState.Maximized;
                btnFullscreen.Text = "\u2715";
                _isFullscreen = true;
            }
            else
            {
                FormBorderStyle = _previousBorder;
                WindowState = _previousState;
                Size = _previousSize;
                Location = _previousLocation;
                btnFullscreen.Text = "\u26F6";
                _isFullscreen = false;
            }
        }

        private void RtbContent_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control)
            {
                switch (e.KeyCode)
                {
                    case Keys.B:
                        ApplyStyle(FontStyle.Bold);
                        e.SuppressKeyPress = true;
                        break;
                    case Keys.I:
                        ApplyStyle(FontStyle.Italic);
                        e.SuppressKeyPress = true;
                        break;
                    case Keys.U:
                        ApplyStyle(FontStyle.Underline);
                        e.SuppressKeyPress = true;
                        break;
                }
            }

            if (e.KeyCode == Keys.Escape && _isFullscreen)
            {
                BtnFullscreen_Click(null, null);
            }
        }

        private void ApplyStyle(FontStyle style)
        {
            if (rtbContent.SelectionLength == 0) return;

            var currentFont = rtbContent.SelectionFont ?? rtbContent.Font;
            FontStyle newStyle = currentFont.Style;

            if ((newStyle & style) == style)
                newStyle &= ~style;
            else
                newStyle |= style;

            rtbContent.SelectionFont = new Font(currentFont.FontFamily, currentFont.Size, newStyle);
            rtbContent.Focus();
        }

        private void BtnHighlight_Click(object sender, EventArgs e)
        {
            if (rtbContent.SelectionLength == 0)
            {
                MessageBox.Show("Виділіть текст для виділення кольором.", "Увага", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (var colorDialog = new ColorDialog())
            {
                colorDialog.Color = Color.Yellow;
                colorDialog.CustomColors = new int[] {
                    ColorTranslator.ToOle(Color.Yellow),
                    ColorTranslator.ToOle(Color.LightGreen),
                    ColorTranslator.ToOle(Color.LightBlue),
                    ColorTranslator.ToOle(Color.LightPink),
                    ColorTranslator.ToOle(Color.Orange),
                    ColorTranslator.ToOle(Color.LightCyan)
                };

                if (colorDialog.ShowDialog() == DialogResult.OK)
                {
                    rtbContent.SelectionBackColor = colorDialog.Color;
                }
            }
            rtbContent.Focus();
        }

        private void BtnTextColor_Click(object sender, EventArgs e)
        {
            if (rtbContent.SelectionLength == 0)
            {
                MessageBox.Show("Виділіть текст для зміни кольору.", "Увага", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (var colorDialog = new ColorDialog())
            {
                colorDialog.Color = Color.Red;
                colorDialog.CustomColors = new int[] {
                    ColorTranslator.ToOle(Color.Red),
                    ColorTranslator.ToOle(Color.Blue),
                    ColorTranslator.ToOle(Color.Green),
                    ColorTranslator.ToOle(Color.Orange),
                    ColorTranslator.ToOle(Color.Purple),
                    ColorTranslator.ToOle(Color.DarkCyan)
                };

                if (colorDialog.ShowDialog() == DialogResult.OK)
                {
                    rtbContent.SelectionColor = colorDialog.Color;
                }
            }
            rtbContent.Focus();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // CreateNoteForm
            // 
            this.ClientSize = new System.Drawing.Size(282, 253);
            this.Name = "CreateNoteForm";
            this.Load += new System.EventHandler(this.CreateNoteForm_Load);
            this.ResumeLayout(false);

        }

        private void CreateNoteForm_Load(object sender, EventArgs e)
        {

        }

        private void BtnBulletList_Click(object sender, EventArgs e)
        {
            rtbContent.SelectionBullet = !rtbContent.SelectionBullet;
            rtbContent.Focus();
        }

        private void BtnAttach_Click(object sender, EventArgs e)
        {
            using (var openDialog = new OpenFileDialog())
            {
                openDialog.Title = "Прикріпити файл";
                openDialog.Filter = "Всі файли|*.*|" +
                                   "Зображення|*.jpg;*.jpeg;*.png;*.gif;*.bmp;*.webp;*.ico|" +
                                   "Документи|*.pdf;*.doc;*.docx;*.txt;*.xls;*.xlsx;*.ppt;*.pptx;*.odt;*.ods|" +
                                   "Архіви|*.zip;*.rar;*.7z;*.tar;*.gz|" +
                                   "Відео|*.mp4;*.avi;*.mkv;*.mov;*.wmv|" +
                                   "Аудіо|*.mp3;*.wav;*.flac;*.ogg;*.m4a";
                openDialog.FilterIndex = 1;
                openDialog.Multiselect = true;

                if (openDialog.ShowDialog() == DialogResult.OK)
                {
                    foreach (var file in openDialog.FileNames)
                    {
                        string savedPath = CopyFileToAppFolder(file);

                        if (!_attachments.Contains(savedPath))
                        {
                            _attachments.Add(savedPath);
                        }
                    }
                    UpdateAttachmentsPanel();
                }
            }
        }

        private bool IsImageFile(string filePath)
        {
            string ext = Path.GetExtension(filePath).ToLower();
            return ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".gif" || ext == ".bmp" || ext == ".webp" || ext == ".ico";
        }

        private void UpdateAttachmentsPanel()
        {
            imagePreviewPanel.Controls.Clear();

            if (_attachments.Count == 0)
            {
                imagePreviewPanel.Visible = false;
                imagePreviewPanel.Height = 0;
                RepositionBottomControls();
                return;
            }

            int panelWidth = bottomPanel.ClientSize.Width - 60;

            foreach (var file in _attachments)
            {
                bool isImage = IsImageFile(file);
                string fileName = Path.GetFileName(file);

                var row = new Panel
                {
                    Size = new Size(panelWidth, 28),
                    Margin = new Padding(0, 1, 0, 1),
                    BackColor = ThemeManager.GetSurface(Program.CurrentTheme),
                    Cursor = Cursors.Hand
                };

                string icon;
                string ext = Path.GetExtension(file).ToLower();
                if (isImage) icon = "\U0001F5BC";
                else if (ext == ".pdf") icon = "\U0001F4D5";
                else if (ext == ".docx" || ext == ".doc") icon = "\U0001F4C3";
                else if (ext == ".xls" || ext == ".xlsx") icon = "\U0001F4CA";
                else if (ext == ".html" || ext == ".htm") icon = "\U0001F310";
                else if (ext == ".txt") icon = "\U0001F4DD";
                else if (ext == ".zip" || ext == ".rar" || ext == ".7z") icon = "\U0001F4E6";
                else if (ext == ".mp4" || ext == ".avi" || ext == ".mkv") icon = "\U0001F3AC";
                else if (ext == ".mp3" || ext == ".wav" || ext == ".flac") icon = "\U0001F3B5";
                else icon = "\U0001F4C4";

                var lblIcon = new Label
                {
                    Text = icon,
                    Location = new Point(6, 4),
                    Size = new Size(22, 20),
                    Font = new Font("Segoe UI Emoji", 9f)
                };
                row.Controls.Add(lblIcon);

                string displayName = fileName.Length > 50 ? fileName.Substring(0, 47) + "..." : fileName;
                var lblName = new Label
                {
                    Text = displayName,
                    Location = new Point(30, 5),
                    AutoSize = true,
                    Font = new Font("Segoe UI", 8.5f),
                    ForeColor = ThemeManager.GetTextColor(Program.CurrentTheme),
                    Cursor = Cursors.Hand
                };
                row.Controls.Add(lblName);

                // Кнопка видалення
                var btnRemove = new Button
                {
                    Text = "x",
                    Size = new Size(22, 22),
                    Location = new Point(panelWidth - 28, 3),
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 7f, FontStyle.Bold),
                    ForeColor = ThemeManager.Danger,
                    Cursor = Cursors.Hand
                };
                btnRemove.FlatAppearance.BorderSize = 0;
                string fileToRemove = file;
                btnRemove.Click += (s, ev) =>
                {
                    _attachments.Remove(fileToRemove);
                    UpdateAttachmentsPanel();
                };
                row.Controls.Add(btnRemove);

                // Клік для відкриття файлу
                string fileCopy = file;
                bool isImg = isImage;
                EventHandler openFile = (s, ev) =>
                {
                    if (isImg) OpenImageFullSize(fileCopy);
                    else OpenAttachment(fileCopy);
                };
                row.Click += openFile;
                lblIcon.Click += openFile;
                lblName.Click += openFile;

                imagePreviewPanel.Controls.Add(row);
            }

            // Висота: кожен рядок 30px, максимум 4 видимі рядки, далі скрол
            int rowHeight = 30;
            int maxVisible = 4;
            int neededHeight = _attachments.Count * rowHeight + 4;
            int maxHeight = maxVisible * rowHeight + 4;
            imagePreviewPanel.Height = Math.Min(neededHeight, maxHeight);
            imagePreviewPanel.Visible = true;

            RepositionBottomControls();
        }

        /// <summary>
        /// Перераховує позиції ВСІХ контролів у bottomPanel відносно висоти панелі вкладень.
        /// </summary>
        private void RepositionBottomControls()
        {
            bottomPanel.SuspendLayout();

            int padding = 20;
            int y = 10;

            // Панель файлів
            if (imagePreviewPanel.Visible && imagePreviewPanel.Height > 0)
            {
                imagePreviewPanel.Location = new Point(padding, y);
                y += imagePreviewPanel.Height + 10;
            }

            // Тип
            lblType.Location = new Point(padding, y);
            y += 24;

            rbNote.Location = new Point(padding, y);
            rbPassword.Location = new Point(padding + 130, y);
            y += 34;

            // Колір + Теги
            lblColor.Location = new Point(padding, y);
            lblTags.Location = new Point(padding + 180, y);
            y += 22;

            cmbColor.Location = new Point(padding, y);
            txtTags.Location = new Point(padding + 180, y);
            y += 40;

            // Спільна нотатка
            chkShared.Location = new Point(padding, y);
            y += 30;

            // Група
            lblGroup.Location = new Point(padding, y);
            y += 22;

            cmbGroup.Location = new Point(padding, y);
            y += 40;

            // Зберегти
            btnSave.Location = new Point(padding, y);
            y += btnSave.Height + 20;

            // Оновити область прокрутки
            bottomPanel.AutoScrollMinSize = new Size(0, y);
            bottomPanel.ResumeLayout(true);
        }

        private void AdjustAttachmentPanelsLayout()
        {
            RepositionBottomControls();
        }

        private string GetFileIcon(string file)
        {
            string ext = Path.GetExtension(file).ToLower();

            if (ext == ".pdf") return "\U0001F4D5";
            if (ext == ".doc" || ext == ".docx" || ext == ".odt") return "\U0001F4D8";
            if (ext == ".xls" || ext == ".xlsx" || ext == ".ods") return "\U0001F4D7";
            if (ext == ".ppt" || ext == ".pptx") return "\U0001F4D9";
            if (ext == ".zip" || ext == ".rar" || ext == ".7z" || ext == ".tar" || ext == ".gz") return "\U0001F4E6";
            if (ext == ".mp4" || ext == ".avi" || ext == ".mkv" || ext == ".mov" || ext == ".wmv") return "\U0001F3AC";
            if (ext == ".mp3" || ext == ".wav" || ext == ".flac" || ext == ".ogg" || ext == ".m4a") return "\U0001F3B5";
            if (ext == ".txt") return "\U0001F4DD";

            return "\U0001F4C4";
        }

        private void OpenImageFullSize(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    MessageBox.Show("Файл не знайдено.", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                using (var viewForm = new Form())
                {
                    viewForm.Text = Path.GetFileName(filePath);
                    viewForm.StartPosition = FormStartPosition.CenterScreen;
                    viewForm.WindowState = FormWindowState.Maximized;
                    viewForm.BackColor = Color.Black;
                    viewForm.KeyPreview = true;
                    viewForm.KeyDown += (s, e) => { if (e.KeyCode == Keys.Escape) viewForm.Close(); };

                    var pictureBox = new PictureBox
                    {
                        Dock = DockStyle.Fill,
                        SizeMode = PictureBoxSizeMode.Zoom,
                        BackColor = Color.Black
                    };

                    using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                    {
                        pictureBox.Image = Image.FromStream(stream);
                    }

                    viewForm.Controls.Add(pictureBox);
                    viewForm.ShowDialog(this);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не вдалося відкрити зображення: {ex.Message}", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OpenAttachment(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = filePath,
                        UseShellExecute = true
                    });
                }
                else
                {
                    MessageBox.Show("Файл не знайдено.", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не вдалося відкрити файл: {ex.Message}", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadNoteContent(Note toEdit)
        {
            if (toEdit.Type == "password")
            {
                if (Program.SessionKey != null && toEdit.IsEncrypted)
                {
                    try
                    {
                        rtbContent.Text = CryptoService.DecryptAes(toEdit.IvBase64, toEdit.Content, Program.SessionKey);
                    }
                    catch
                    {
                        rtbContent.Text = "(не вдалося розшифрувати)";
                    }
                }
                else
                {
                    rtbContent.Text = "**********";
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(toEdit.Content) && toEdit.Content.StartsWith("{\\rtf"))
                {
                    try
                    {
                        rtbContent.Rtf = toEdit.Content;
                    }
                    catch
                    {
                        rtbContent.Text = toEdit.Content;
                    }
                }
                else
                {
                    rtbContent.Text = toEdit.Content ?? "";
                }
            }
        }

        private void CmbColor_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;

            e.DrawBackground();

            var item = cmbColor.Items[e.Index] as ColorItem;
            if (item == null) return;

            var colorRect = new Rectangle(e.Bounds.X + 4, e.Bounds.Y + 4, 20, e.Bounds.Height - 8);
            using (var brush = new SolidBrush(item.Color))
            {
                e.Graphics.FillRectangle(brush, colorRect);
            }
            using (var pen = new Pen(Color.Gray))
            {
                e.Graphics.DrawRectangle(pen, colorRect);
            }

            var textRect = new Rectangle(colorRect.Right + 8, e.Bounds.Y, e.Bounds.Width - colorRect.Width - 16, e.Bounds.Height);
            TextRenderer.DrawText(e.Graphics, item.ToString(), e.Font, textRect, e.ForeColor, TextFormatFlags.Left | TextFormatFlags.VerticalCenter);

            e.DrawFocusRectangle();
        }

        private void SelectColorByHex(string hex)
        {
            if (string.IsNullOrEmpty(hex)) { cmbColor.SelectedIndex = 0; return; }

            for (int i = 0; i < _colors.Count; i++)
            {
                if (_colors[i].Hex.Equals(hex, StringComparison.OrdinalIgnoreCase))
                {
                    cmbColor.SelectedIndex = i;
                    return;
                }
            }
            cmbColor.SelectedIndex = 0;
        }

        private void PopulateGroups()
        {
            cmbGroup.Items.Clear();
            foreach (var g in _groups) cmbGroup.Items.Add(g);
            cmbGroup.DisplayMember = "Name";
        }

        private void PreselectGroup()
        {
            if (_preselectGroupId.HasValue)
            {
                var idx = _groups.FindIndex(g => g.Id == _preselectGroupId.Value);
                if (idx >= 0) cmbGroup.SelectedIndex = idx;
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            Program.TouchActivity();

            var title = txtTitle.Text.Trim();
            var type = rbPassword.Checked ? "password" : "note";
            var selectedColor = cmbColor.SelectedItem as ColorItem;
            var color = selectedColor?.Hex ?? "#FFFFFF";
            var tags = txtTags.Text?.Trim() ?? "";
            var attachments = string.Join("|", _attachments);

            string content;
            if (type == "password")
            {
                content = rtbContent.Text;
            }
            else
            {
                content = rtbContent.Rtf;
            }

            if (string.IsNullOrWhiteSpace(title))
            {
                MessageBox.Show("Введіть заголовок.", "Увага", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int? groupId = null;
            if (chkShared.Checked)
            {
                if (cmbGroup.SelectedItem is Group g) groupId = g.Id;
                else
                {
                    MessageBox.Show("Оберіть групу для спільної нотатки.", "Увага", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }

            if (EditingNote == null)
            {
                var note = new Note
                {
                    OwnerId = Program.CurrentUser.Id,
                    Title = title,
                    Type = type,
                    Color = color,
                    Tags = tags,
                    GroupId = groupId,
                    Attachments = attachments,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                if (type == "password")
                {
                    if (Program.SessionKey == null)
                    {
                        MessageBox.Show("Спочатку розблокуйте вкладку Паролі (PIN).", "Увага", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    var enc = CryptoService.EncryptAes(content, Program.SessionKey);
                    note.Content = enc.cipherBase64;
                    note.IvBase64 = enc.ivBase64;
                }
                else
                {
                    note.Content = content;
                    note.IvBase64 = null;
                }

                CreatedOrUpdatedNote = note;
                DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                EditingNote.Title = title;
                EditingNote.Type = type;
                EditingNote.Color = color;
                EditingNote.Tags = tags;
                EditingNote.GroupId = groupId;
                EditingNote.Attachments = attachments;
                EditingNote.UpdatedAt = DateTime.Now;

                if (type == "password")
                {
                    var isMasked = content.StartsWith("**") || content.StartsWith("(не вдалося");
                    if (!isMasked && !string.IsNullOrWhiteSpace(content))
                    {
                        if (Program.SessionKey == null)
                        {
                            MessageBox.Show("Спочатку розблокуйте вкладку Паролі (PIN).", "Увага", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                        var enc = CryptoService.EncryptAes(content, Program.SessionKey);
                        EditingNote.Content = enc.cipherBase64;
                        EditingNote.IvBase64 = enc.ivBase64;
                    }
                }
                else
                {
                    EditingNote.Content = content;
                    EditingNote.IvBase64 = null;
                }

                CreatedOrUpdatedNote = EditingNote;
                DialogResult = DialogResult.OK;
                Close();
            }
        }
    }
}
