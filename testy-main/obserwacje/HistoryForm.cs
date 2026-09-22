namespace GeneratorHasel;

sealed class HistoryForm : Form
{
    private readonly ListView _list;

    public HistoryForm(IReadOnlyList<PasswordEntry> entries)
    {
        Text = "Historia haseł";
        StartPosition = FormStartPosition.CenterParent;
        MinimumSize = new Size(560, 360);
        Size = new Size(720, 480);
        BackColor = Color.FromArgb(14, 14, 16);
        ForeColor = Color.White;
        Font = new Font("Segoe UI", 10f);

        var header = new Panel
        {
            Dock = DockStyle.Top,
            Height = 72,
            BackColor = Color.FromArgb(22, 22, 26),
            Padding = new Padding(20, 16, 20, 16),
        };

        var title = new Label
        {
            Text = "Wszystkie wygenerowane hasła",
            AutoSize = true,
            Font = new Font("Segoe UI", 16f, FontStyle.Bold),
            Location = new Point(20, 18),
        };

        header.Controls.Add(title);

        var footer = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 64,
            BackColor = Color.FromArgb(22, 22, 26),
            Padding = new Padding(16),
        };

        var copyButton = MakeButton("Kopiuj zaznaczone", Color.FromArgb(250, 84, 0));
        copyButton.Dock = DockStyle.Right;
        copyButton.Width = 180;
        copyButton.Click += (_, _) => CopySelected();

        var hint = new Label
        {
            Text = "Dwuklik też kopiuje hasło.",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            ForeColor = Color.FromArgb(170, 170, 175),
        };

        footer.Controls.Add(copyButton);
        footer.Controls.Add(hint);

        _list = new ListView
        {
            Dock = DockStyle.Fill,
            View = View.Details,
            FullRowSelect = true,
            GridLines = false,
            BorderStyle = BorderStyle.None,
            BackColor = Color.FromArgb(28, 28, 32),
            ForeColor = Color.White,
            HeaderStyle = ColumnHeaderStyle.Nonclickable,
            MultiSelect = false,
        };
        _list.Columns.Add("Data i godzina", 200);
        _list.Columns.Add("Długość", 90);
        _list.Columns.Add("Hasło", 380);
        _list.DoubleClick += (_, _) => CopySelected();

        foreach (var entry in entries)
        {
            var item = new ListViewItem(entry.CreatedAt.ToString("dd.MM.yyyy  HH:mm:ss"));
            item.SubItems.Add(entry.Length.ToString());
            item.SubItems.Add(entry.Password);
            item.Tag = entry;
            _list.Items.Add(item);
        }

        if (_list.Items.Count > 0)
        {
            _list.Items[0].Selected = true;
        }

        Controls.Add(_list);
        Controls.Add(footer);
        Controls.Add(header);

        Resize += (_, _) => SizeLastColumn();
        Shown += (_, _) => SizeLastColumn();
    }

    private void SizeLastColumn()
    {
        if (_list.Columns.Count < 3)
        {
            return;
        }

        var used = _list.Columns[0].Width + _list.Columns[1].Width;
        _list.Columns[2].Width = Math.Max(180, _list.ClientSize.Width - used - 8);
    }

    private void CopySelected()
    {
        if (_list.SelectedItems.Count == 0)
        {
            return;
        }

        if (_list.SelectedItems[0].Tag is PasswordEntry entry)
        {
            Clipboard.SetText(entry.Password);
        }
    }

    private static Button MakeButton(string text, Color background)
    {
        var button = new Button
        {
            Text = text,
            FlatStyle = FlatStyle.Flat,
            BackColor = background,
            ForeColor = Color.White,
            Cursor = Cursors.Hand,
            Font = new Font("Segoe UI Semibold", 10f),
        };
        button.FlatAppearance.BorderSize = 0;
        return button;
    }
}
