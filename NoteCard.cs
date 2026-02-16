using Org.BouncyCastle.Asn1.Crmf;
using SecureNotes;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System;
using System.Linq;
public class NoteCard : Panel
{
    public event EventHandler<int> DeleteRequested;
    public event EventHandler<int> EditRequested;

    private Label lblTitle, lblTags;
    private RichTextBox rtbPreview;
    private Label lblAttachmentIcon;
    private FlowLayoutPanel attachmentsPreviewPanel;
    private Button btnEdit, btnCopy, btnDelete;
    private Note _note;

    public NoteCard()
    {
        Size = new Size(300, 200);
        Margin = new Padding(8);
        Cursor = Cursors.Hand;

        // Заголовок
        lblTitle = new Label
        {
            Location = new Point(12, 12),
            Size = new Size(240, 24),
            Font = new Font("Segoe UI Semibold", 11f)
        };
        Controls.Add(lblTitle);

        // Іконка вкладень
        lblAttachmentIcon = new Label
        {
            Location = new Point(258, 12),
            Size = new Size(30, 24),
            Font = new Font("Segoe UI Emoji", 12f),
            Text = "",
            Visible = false,
            TextAlign = ContentAlignment.MiddleRight
        };
        Controls.Add(lblAttachmentIcon);

        // RichTextBox для збереження форматування (закреслений, кольоровий текст)
        rtbPreview = new RichTextBox
        {
            Location = new Point(12, 40),
            Size = new Size(276, 50),
            Font = new Font("Segoe UI", 9f),
            ReadOnly = true,
            BorderStyle = BorderStyle.None,
            ScrollBars = RichTextBoxScrollBars.None,
            Cursor = Cursors.Hand,
            DetectUrls = false
        };
        rtbPreview.Click += (s, e) => EditRequested?.Invoke(this, _note.Id);
        rtbPreview.MouseEnter += (s, e) => rtbPreview.Cursor = Cursors.Hand;
        Controls.Add(rtbPreview);

        // Панель превью вкладень
        attachmentsPreviewPanel = new FlowLayoutPanel
        {
            Location = new Point(12, 92),
            Size = new Size(276, 24),
            Visible = false,
            WrapContents = false,
            AutoScroll = false
        };
        Controls.Add(attachmentsPreviewPanel);

        // Теги
        lblTags = new Label
        {
            Location = new Point(12, 118),
            Size = new Size(276, 20),
            Font = new Font("Segoe UI", 8f)
        };
        Controls.Add(lblTags);

        // Кнопки
        var btnPanel = new Panel
        {
            Location = new Point(12, 144),
            Size = new Size(276, 36)
        };
        btnPanel.Tag = "transparent";

        btnEdit = new Button
        {
            Text = LocalizationManager.Get("edit"),
            Location = new Point(0, 0),
            Size = new Size(85, 32),
            Font = new Font("Segoe UI", 9f),
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        btnEdit.Click += (s, e) => EditRequested?.Invoke(this, _note.Id);
        btnPanel.Controls.Add(btnEdit);

        btnCopy = new Button
        {
            Text = LocalizationManager.Get("copy"),
            Location = new Point(91, 0),
            Size = new Size(85, 32),
            Font = new Font("Segoe UI", 9f),
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        btnCopy.Click += BtnCopy_Click;
        btnPanel.Controls.Add(btnCopy);

        btnDelete = new Button
        {
            Text = LocalizationManager.Get("delete"),
            Location = new Point(182, 0),
            Size = new Size(85, 32),
            Font = new Font("Segoe UI", 9f),
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        btnDelete.Click += (s, e) => DeleteRequested?.Invoke(this, _note.Id);
        btnPanel.Controls.Add(btnDelete);

        Controls.Add(btnPanel);

        this.Click += (s, e) => EditRequested?.Invoke(this, _note.Id);
        lblTitle.Click += (s, e) => EditRequested?.Invoke(this, _note.Id);
        lblTitle.Cursor = Cursors.Hand;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        var rect = new Rectangle(0, 0, Width - 1, Height - 1);
        using (var path = GetRoundedRect(rect, 12))
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using (var brush = new SolidBrush(BackColor))
            {
                e.Graphics.FillPath(brush, path);
            }
            using (var pen = new Pen(Color.FromArgb(200, 200, 200), 1))
            {
                e.Graphics.DrawPath(pen, path);
            }
        }
    }

    private GraphicsPath GetRoundedRect(Rectangle rect, int radius)
    {
        var path = new GraphicsPath();
        int d = radius * 2;
        path.AddArc(rect.X, rect.Y, d, d, 180, 90);
        path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
        path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
        path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
        path.CloseFigure();
        return path;
    }

    private string GetFileIcon(string filePath)
    {
        string ext = Path.GetExtension(filePath).ToLower();

        if (ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".gif" || ext == ".bmp" || ext == ".webp")
            return "🖼";
        if (ext == ".pdf")
            return "📄";
        if (ext == ".doc" || ext == ".docx" || ext == ".txt" || ext == ".rtf")
            return "📝";
        if (ext == ".xls" || ext == ".xlsx")
            return "📊";
        if (ext == ".ppt" || ext == ".pptx")
            return "📽";
        if (ext == ".zip" || ext == ".rar" || ext == ".7z")
            return "📦";
        if (ext == ".mp3" || ext == ".wav" || ext == ".flac" || ext == ".ogg")
            return "🎵";
        if (ext == ".mp4" || ext == ".avi" || ext == ".mkv" || ext == ".mov")
            return "🎬";

        return "📎";
    }

    public void Bind(Note note, string decryptedPassword)
    {
        _note = note;
        lblTitle.Text = note.Title ?? "";

        // Показуємо вкладення
        attachmentsPreviewPanel.Controls.Clear();
        attachmentsPreviewPanel.Visible = false;

        if (!string.IsNullOrEmpty(note.Attachments))
        {
            var attachments = note.Attachments.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);

            if (attachments.Length > 0)
            {
                attachmentsPreviewPanel.Visible = true;

                // Показуємо іконку
                bool hasImages = attachments.Any(a =>
                    a.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                    a.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) ||
                    a.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
                    a.EndsWith(".gif", StringComparison.OrdinalIgnoreCase) ||
                    a.EndsWith(".bmp", StringComparison.OrdinalIgnoreCase));

                lblAttachmentIcon.Text = hasImages ? "🖼" : "📎";
                lblAttachmentIcon.Visible = true;

                // Показуємо список файлів (до 3)
                int count = 0;
                foreach (var attachment in attachments)
                {
                    if (count >= 3) break;

                    string fileName = Path.GetFileName(attachment);
                    string icon = GetFileIcon(attachment);

                    var fileLabel = new Label
                    {
                        Text = icon + " " + (fileName.Length > 12 ? fileName.Substring(0, 9) + "..." : fileName),
                        AutoSize = true,
                        Font = new Font("Segoe UI", 7.5f),
                        Margin = new Padding(0, 0, 6, 0),
                        Cursor = Cursors.Hand
                    };

                    string filePath = attachment;
                    fileLabel.Click += (s, e) =>
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
                        }
                        catch { }
                    };

                    attachmentsPreviewPanel.Controls.Add(fileLabel);
                    count++;
                }

                // Якщо більше 3 файлів, показуємо кількість
                if (attachments.Length > 3)
                {
                    var moreLabel = new Label
                    {
                        Text = $"+{attachments.Length - 3} {LocalizationManager.Get("file_count")}",
                        AutoSize = true,
                        Font = new Font("Segoe UI", 7.5f, FontStyle.Italic),
                        Margin = new Padding(0, 0, 0, 0)
                    };
                    attachmentsPreviewPanel.Controls.Add(moreLabel);
                }
            }
        }
        else
        {
            lblAttachmentIcon.Visible = false;
        }

        // Завантажуємо вміст зі збереженням форматування
        if (note.Type == "password")
        {
            rtbPreview.Text = decryptedPassword ?? "**********";
        }
        else if (!string.IsNullOrEmpty(note.Content) && note.Content.StartsWith("{\\rtf"))
        {
            try
            {
                rtbPreview.Rtf = note.Content;

                if (rtbPreview.Text.Length > 60)
                {
                    rtbPreview.Select(60, rtbPreview.Text.Length - 60);
                    rtbPreview.SelectedText = "...";
                }
            }
            catch
            {
                rtbPreview.Text = note.Content?.Length > 60
                    ? note.Content.Substring(0, 60) + "..."
                    : note.Content ?? "";
            }
        }
        else
        {
            rtbPreview.Text = note.Content?.Length > 60
                ? note.Content.Substring(0, 60) + "..."
                : note.Content ?? "";
        }

        // Теги
        lblTags.Text = string.IsNullOrEmpty(note.Tags) ? "" : "# " + note.Tags.Replace(";", " # ");

        // Колір картки
        Color cardColor = Color.White;
        try
        {
            if (!string.IsNullOrEmpty(note.Color))
            {
                cardColor = ColorTranslator.FromHtml(note.Color);
            }
        }
        catch
        {
            cardColor = Color.White;
        }

        BackColor = cardColor;
        rtbPreview.BackColor = cardColor;
        attachmentsPreviewPanel.BackColor = cardColor;

        // Визначаємо колір тексту на основі яскравості фону
        float brightness = cardColor.GetBrightness();
        Color textColor = brightness > 0.6f ? Color.FromArgb(30, 30, 30) : Color.FromArgb(240, 240, 240);
        Color textSecondary = brightness > 0.6f ? Color.FromArgb(100, 100, 100) : Color.FromArgb(180, 180, 180);

        lblTitle.ForeColor = textColor;
        rtbPreview.ForeColor = textColor;
        lblTags.ForeColor = textSecondary;
        lblAttachmentIcon.ForeColor = textSecondary;

        foreach (Control c in attachmentsPreviewPanel.Controls)
        {
            if (c is Label lbl)
            {
                lbl.ForeColor = textSecondary;
            }
        }
    }

    private void BtnCopy_Click(object sender, EventArgs e)
    {
        string textToCopy = "";
        if (_note.Type == "password" && _note.IsEncrypted && Program.SessionKey != null)
        {
            try
            {
                textToCopy = CryptoService.DecryptAes(_note.IvBase64, _note.Content, Program.SessionKey);
            }
            catch
            {
                textToCopy = "";
            }
        }
        else if (!string.IsNullOrEmpty(_note.Content))
        {
            if (_note.Content.StartsWith("{\\rtf"))
            {
                try
                {
                    using (var rtb = new RichTextBox())
                    {
                        rtb.Rtf = _note.Content;
                        textToCopy = rtb.Text;
                    }
                }
                catch
                {
                    textToCopy = _note.Content;
                }
            }
            else
            {
                textToCopy = _note.Content;
            }
        }

        if (!string.IsNullOrEmpty(textToCopy))
        {
            Clipboard.SetText(textToCopy);
            MessageBox.Show(LocalizationManager.Get("copied"), "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    public void ApplyTheme(Theme theme)
    {
        btnEdit.FlatAppearance.BorderColor = Color.FromArgb(180, 180, 180);
        btnEdit.FlatAppearance.BorderSize = 1;
        btnEdit.BackColor = Color.FromArgb(240, 240, 240);
        btnEdit.ForeColor = Color.FromArgb(50, 50, 50);

        btnCopy.FlatAppearance.BorderColor = Color.FromArgb(180, 180, 180);
        btnCopy.FlatAppearance.BorderSize = 1;
        btnCopy.BackColor = Color.FromArgb(240, 240, 240);
        btnCopy.ForeColor = Color.FromArgb(50, 50, 50);

        btnDelete.FlatAppearance.BorderColor = ThemeManager.Danger;
        btnDelete.FlatAppearance.BorderSize = 1;
        btnDelete.BackColor = Color.FromArgb(255, 240, 240);
        btnDelete.ForeColor = ThemeManager.Danger;
    }
}