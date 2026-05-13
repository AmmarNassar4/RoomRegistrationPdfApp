using Microsoft.Extensions.Configuration;
using System.Diagnostics;

namespace RoomRegistrationPdfApp;

public sealed class MainForm : Form
{
    private readonly TextBox _roomTextBox = new();
    private readonly TextBox _outputFolderTextBox = new();
    private readonly Label _statusLabel = new();
    private readonly Button _generateButton = new();
    private readonly Button _browseButton = new();
    private readonly IConfigurationRoot _configuration;

    public MainForm()
    {
        _configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        Text = "Guest Registration RC PDFs";
        Width = 680;
        Height = 270;
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;

        BuildUi();
    }

    private void BuildUi()
    {
        var roomLabel = new Label
        {
            Text = "Room No:",
            Left = 25,
            Top = 30,
            Width = 120
        };

        _roomTextBox.Left = 150;
        _roomTextBox.Top = 25;
        _roomTextBox.Width = 180;
        _roomTextBox.Font = new Font(Font.FontFamily, 11);
        _roomTextBox.PlaceholderText = "مثال: 201";

        var outputLabel = new Label
        {
            Text = "Output folder:",
            Left = 25,
            Top = 75,
            Width = 120
        };

        _outputFolderTextBox.Left = 150;
        _outputFolderTextBox.Top = 70;
        _outputFolderTextBox.Width = 380;
        _outputFolderTextBox.Text = ResolveDefaultOutputFolder();

        _browseButton.Text = "Browse";
        _browseButton.Left = 540;
        _browseButton.Top = 68;
        _browseButton.Width = 85;
        _browseButton.Click += BrowseButton_Click;

        _generateButton.Text = "Generate RC PDFs";
        _generateButton.Left = 150;
        _generateButton.Top = 120;
        _generateButton.Width = 190;
        _generateButton.Height = 36;
        _generateButton.Click += async (_, _) => await GenerateAsync();

        _statusLabel.Left = 25;
        _statusLabel.Top = 175;
        _statusLabel.Width = 610;
        _statusLabel.Height = 50;
        _statusLabel.ForeColor = Color.DarkSlateGray;

        Controls.Add(roomLabel);
        Controls.Add(_roomTextBox);
        Controls.Add(outputLabel);
        Controls.Add(_outputFolderTextBox);
        Controls.Add(_browseButton);
        Controls.Add(_generateButton);
        Controls.Add(_statusLabel);
    }

    private string ResolveDefaultOutputFolder()
    {
        var configured = _configuration["DefaultOutputFolder"];
        if (string.IsNullOrWhiteSpace(configured))
            return Path.Combine(AppContext.BaseDirectory, "Output");

        return Path.IsPathRooted(configured)
            ? configured
            : Path.Combine(AppContext.BaseDirectory, configured);
    }

    private void BrowseButton_Click(object? sender, EventArgs e)
    {
        using var dialog = new FolderBrowserDialog
        {
            Description = "Select output folder"
        };

        if (dialog.ShowDialog(this) == DialogResult.OK)
            _outputFolderTextBox.Text = dialog.SelectedPath;
    }

    private async Task GenerateAsync()
    {
        var roomNo = _roomTextBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(roomNo))
        {
            MessageBox.Show(this, "اكتب رقم الغرفة أولاً.", "Room No", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var connectionString = _configuration.GetConnectionString("HotelDb");
        if (string.IsNullOrWhiteSpace(connectionString) || connectionString.Contains("YOUR_", StringComparison.OrdinalIgnoreCase))
        {
            MessageBox.Show(this, "عدّل connection string داخل appsettings.json أولاً.", "Database", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            _generateButton.Enabled = false;
            _statusLabel.Text = "Getting registrations from SQL Server...";

            var receptionist = _configuration["ReceptionistName"] ?? Environment.UserName;
            var db = new DatabaseService(connectionString);
            var registrations = await db.GetGuestRegistrationsByRoomAsync(roomNo, receptionist);

            if (registrations.Count == 0)
            {
                _statusLabel.Text = $"No registrations were found for room {roomNo}.";
                return;
            }

            _statusLabel.Text = $"Generating {registrations.Count} RC PDF file(s)...";

            var templatePath = Path.Combine(AppContext.BaseDirectory, "Templates", "empty.pdf");
            var outputRoot = _outputFolderTextBox.Text.Trim();
            Directory.CreateDirectory(outputRoot);

            var safeRoom = SafeFileName(roomNo);
            var batchFolder = Path.Combine(outputRoot, $"Room_{safeRoom}_{DateTime.Now:yyyyMMdd_HHmmss}");
            Directory.CreateDirectory(batchFolder);

            var generator = new PdfRegistrationGenerator();
            var index = 1;
            foreach (var data in registrations)
            {
                var safeReg = SafeFileName(data.RegNo);
                var safeConfirmation = SafeFileName(data.ConfirmationNo);
                var outputPath = Path.Combine(batchFolder, $"RC_{index:000}_Room_{safeRoom}_Reg_{safeReg}_Conf_{safeConfirmation}.pdf");
                generator.Generate(templatePath, outputPath, data);
                index++;
            }

            _statusLabel.Text = $"Created {registrations.Count} RC PDF file(s): {batchFolder}";

            if (MessageBox.Show(this, $"تم إنشاء {registrations.Count} ملف RC. هل تريد فتح المجلد الآن؟", "Done", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
            {
                Process.Start(new ProcessStartInfo(batchFolder) { UseShellExecute = true });
            }
        }
        catch (Exception ex)
        {
            _statusLabel.Text = "Error.";
            MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            _generateButton.Enabled = true;
        }
    }

    private static string SafeFileName(string value)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var safe = string.Concat(value.Select(ch => invalid.Contains(ch) ? '_' : ch));
        return string.IsNullOrWhiteSpace(safe) ? "NA" : safe.Trim();
    }
}
