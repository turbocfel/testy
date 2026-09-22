using System.Security.Cryptography;
using System.Text;

namespace GeneratorHasel;

sealed class MainForm : Form
{
    private const string LettersLower = "abcdefghijklmnopqrstuvwxyz";
    private const string LettersUpper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private const string Digits = "0123456789";
    private const string Symbols = "!@#$%^&*()-_=+[]{};:,.?";
    private const int MinPasswordLength = 8;
    private const int MaxPasswordLength = 32;
    private const int DefaultPasswordLength = 16;

    private readonly List<PasswordEntry> _history = [];
    private readonly TrackBar _lengthBar;
    private readonly Label _lengthValue;
    private readonly TextBox _passwordBox;
    private readonly Label _status;
    private readonly Label _historyCount;

    public MainForm()
    {
        Text = "Generator haseł";
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.Sizable;
        MaximizeBox = true;
        MinimumSize = new Size(560, 440);
        Size = new Size(680, 500);
        BackColor = Color.FromArgb(14, 14, 16);
        ForeColor = Color.White;
        Font = new Font("Segoe UI", 10f);
        DoubleBuffered = true;

        var header = new Panel
        {
            Dock = DockStyle.Top,
            Height = 112,
            BackColor = Color.FromArgb(22, 22, 26),
            Padding = new Padding(28, 16, 28, 16),
        };

        var accent = new Panel
        {
            Dock = DockStyle.Top,
            Height = 4,
            BackColor = Color.FromArgb(250, 84, 0),
        };

        var title = new Label
        {
            Text = "Generator haseł",
            AutoSize = false,
            Dock = DockStyle.Top,
            Height = 42,
            Font = new Font("Segoe UI", 22f, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleLeft,
        };

        var subtitle = new Label
        {
            Text = "Twórz silne hasła i wracaj do ich historii.",
            AutoSize = false,
            Dock = DockStyle.Top,
            Height = 28,
            ForeColor = Color.FromArgb(170, 170, 175),
            TextAlign = ContentAlignment.TopLeft,
        };

        header.Controls.Add(subtitle);
        header.Controls.Add(title);

        var footer = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 44,
            BackColor = Color.FromArgb(22, 22, 26),
            Padding = new Padding(24, 0, 24, 0),
        };

        _status = new Label
        {
            Text = "Ustaw długość i kliknij Generuj.",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            ForeColor = Color.FromArgb(160, 160, 165),
        };

        _historyCount = new Label
        {
            Text = "Historia: 0",
            Dock = DockStyle.Right,
            AutoSize = true,
            TextAlign = ContentAlignment.MiddleRight,
            ForeColor = Color.FromArgb(250, 84, 0),
            Padding = new Padding(0, 12, 0, 0),
        };

        footer.Controls.Add(_status);
        footer.Controls.Add(_historyCount);

        var body = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(28, 24, 28, 20),
            BackColor = Color.FromArgb(14, 14, 16),
        };

        var card = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(28, 28, 32),
            Padding = new Padding(24),
        };

        var lengthCaption = new Label
        {
            Text = "Długość hasła",
            AutoSize = true,
            Dock = DockStyle.Top,
            ForeColor = Color.FromArgb(190, 190, 195),
            Padding = new Padding(4, 0, 0, 8),
        };

        var sliderRow = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 56,
            ColumnCount = 2,
            RowCount = 1,
            Padding = new Padding(0),
        };
        sliderRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
        sliderRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 56f));

        _lengthBar = new TrackBar
        {
            Minimum = 0,
            Maximum = MaxPasswordLength - MinPasswordLength,
            TickFrequency = 1,
            SmallChange = 1,
            LargeChange = 1,
            AutoSize = false,
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(28, 28, 32),
            TickStyle = TickStyle.None,
            Margin = new Padding(0, 8, 8, 0),
        };
        _lengthBar.ValueChanged += (_, _) => UpdateLengthLabel();
        _lengthBar.Scroll += (_, _) => UpdateLengthLabel();

        _lengthValue = new Label
        {
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Segoe UI", 16f, FontStyle.Bold),
            ForeColor = Color.FromArgb(250, 84, 0),
            Margin = new Padding(0),
        };

        sliderRow.Controls.Add(_lengthBar, 0, 0);
        sliderRow.Controls.Add(_lengthValue, 1, 0);
        _lengthBar.Value = DefaultPasswordLength - MinPasswordLength;
        UpdateLengthLabel();

        Shown += (_, _) => SyncSliderThumb();

        var passwordCaption = new Label
        {
            Text = "Aktualne hasło",
            AutoSize = true,
            Dock = DockStyle.Top,
            ForeColor = Color.FromArgb(190, 190, 195),
            Padding = new Padding(4, 16, 0, 8),
        };

        _passwordBox = new TextBox
        {
            Dock = DockStyle.Top,
            Height = 44,
            ReadOnly = true,
            BackColor = Color.FromArgb(18, 18, 20),
            ForeColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle,
            Font = new Font("Consolas", 14f),
            PlaceholderText = "Hasło pojawi się tutaj",
        };

        var buttons = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 48,
            ColumnCount = 3,
            Padding = new Padding(0, 18, 0, 0),
        };
        buttons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34f));
        buttons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33f));
        buttons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33f));

        var generateButton = MakeButton("Generuj", Color.FromArgb(250, 84, 0));
        generateButton.Click += (_, _) => GeneratePassword();

        var copyButton = MakeButton("Kopiuj", Color.FromArgb(48, 48, 54));
        copyButton.Click += (_, _) => CopyPassword();

        var historyButton = MakeButton("Historia", Color.FromArgb(48, 48, 54));
        historyButton.Click += (_, _) => ShowHistory();

        buttons.Controls.Add(generateButton, 0, 0);
        buttons.Controls.Add(copyButton, 1, 0);
        buttons.Controls.Add(historyButton, 2, 0);
        buttons.SetColumnSpan(generateButton, 1);

        foreach (Control control in buttons.Controls)
        {
            control.Dock = DockStyle.Fill;
            control.Margin = new Padding(0, 0, 10, 0);
        }

        historyButton.Margin = new Padding(0);

        card.Controls.Add(buttons);
        card.Controls.Add(_passwordBox);
        card.Controls.Add(passwordCaption);
        card.Controls.Add(sliderRow);
        card.Controls.Add(lengthCaption);

        body.Controls.Add(card);

        Controls.Add(body);
        Controls.Add(footer);
        Controls.Add(header);
        Controls.Add(accent);
    }

    private int PasswordLength => _lengthBar.Value + MinPasswordLength;

    private void UpdateLengthLabel()
    {
        _lengthValue.Text = PasswordLength.ToString();
    }

    private void SyncSliderThumb()
    {
        var offset = _lengthBar.Value;
        _lengthBar.Value = _lengthBar.Minimum;
        _lengthBar.Value = Math.Clamp(offset, _lengthBar.Minimum, _lengthBar.Maximum);
        UpdateLengthLabel();
    }

    private void GeneratePassword()
    {
        var length = PasswordLength;
        var alphabet = LettersLower + LettersUpper + Digits + Symbols;
        var bytes = RandomNumberGenerator.GetBytes(length);
        var builder = new StringBuilder(length);

        for (var i = 0; i < length; i++)
        {
            builder.Append(alphabet[bytes[i] % alphabet.Length]);
        }

        var password = builder.ToString();
        _passwordBox.Text = password;
        _history.Insert(0, new PasswordEntry(DateTime.Now, password, length));
        _historyCount.Text = $"Historia: {_history.Count}";
        _status.Text = $"Zapisano hasło z {DateTime.Now:dd.MM.yyyy HH:mm:ss}.";
    }

    private void CopyPassword()
    {
        if (string.IsNullOrWhiteSpace(_passwordBox.Text))
        {
            _status.Text = "Najpierw wygeneruj hasło.";
            return;
        }

        Clipboard.SetText(_passwordBox.Text);
        _status.Text = "Hasło skopiowane do schowka.";
    }

    private void ShowHistory()
    {
        if (_history.Count == 0)
        {
            _status.Text = "Historia jest pusta — wygeneruj pierwsze hasło.";
            return;
        }

        using var dialog = new HistoryForm(_history);
        dialog.ShowDialog(this);
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
            Font = new Font("Segoe UI Semibold", 10.5f),
        };
        button.FlatAppearance.BorderSize = 0;
        button.MouseEnter += (_, _) => button.BackColor = ControlPaint.Light(background, 0.12f);
        button.MouseLeave += (_, _) => button.BackColor = background;
        return button;
    }
}
