using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace RoomRegistrationPdfApp;

public sealed partial class MainForm : Form
{
    private readonly IConfigurationRoot _configuration;
    private string _preferredLanguage;
    private bool _allowClose;

    public MainForm()
    {
        _configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        Text = "Guest Registration RC PDFs";
        Width = 590;
        Height = 100;
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;

        _preferredLanguage = NormalizeLanguage(_configuration["GuestGate:DefaultLanguage"]);

        InitializeComponent();
        _outputFolderTextBox.Text = ResolveDefaultOutputFolder();
        SelectPreferredLanguage();
    }

    private string ResolveDefaultOutputFolder()
    {
        var configured = _configuration["DefaultOutputFolder"];
        if (string.IsNullOrWhiteSpace(configured))
            configured = _configuration["OutputFolder"];

        if (string.IsNullOrWhiteSpace(configured))
            return Path.Combine(AppContext.BaseDirectory, "Output");

        configured = configured.Trim();

        if (configured.StartsWith(@"\\"))
            return configured;

        return Path.IsPathRooted(configured)
            ? configured
            : Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, configured));
    }

    private static string AppSettingsPath => Path.Combine(AppContext.BaseDirectory, "appsettings.json");

    private static void SaveDefaultOutputFolder(string outputFolder)
    {
        if (string.IsNullOrWhiteSpace(outputFolder))
            return;

        var fullPath = Path.GetFullPath(outputFolder.Trim());
        var root = ReadAppSettings();
        root["DefaultOutputFolder"] = ToAppSettingsPath(fullPath);
        WriteAppSettings(root);
    }

    private static void SaveDefaultGuestGateLanguage(string language)
    {
        var normalizedLanguage = NormalizeLanguage(language);
        var root = ReadAppSettings();

        if (root["GuestGate"] is not JsonObject guestGate)
        {
            guestGate = new JsonObject();
            root["GuestGate"] = guestGate;
        }

        guestGate["DefaultLanguage"] = normalizedLanguage;
        WriteAppSettings(root);
    }

    private static JsonObject ReadAppSettings()
    {
        var settingsPath = AppSettingsPath;
        if (!File.Exists(settingsPath))
            return new JsonObject();

        var json = File.ReadAllText(settingsPath, Encoding.UTF8);
        return JsonNode.Parse(json)?.AsObject() ?? new JsonObject();
    }

    private static void WriteAppSettings(JsonObject root)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        File.WriteAllText(AppSettingsPath, root.ToJsonString(options), Encoding.UTF8);
    }

    private static string NormalizeLanguage(string? language)
    {
        if (string.Equals(language?.Trim(), "en", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(language?.Trim(), "English", StringComparison.OrdinalIgnoreCase))
            return "en";

        return "ar";
    }

    private void SelectPreferredLanguage()
    {
        _languageComboBox.SelectedIndexChanged -= LanguageComboBox_SelectedIndexChanged;
        _languageComboBox.SelectedItem = _preferredLanguage == "en" ? "English" : "Arabic";
        _languageComboBox.SelectedIndexChanged += LanguageComboBox_SelectedIndexChanged;
    }

    private static string ToAppSettingsPath(string fullPath)
    {
        if (fullPath.StartsWith(@"\\"))
            return fullPath;

        var basePath = Path.GetFullPath(AppContext.BaseDirectory);
        var relative = Path.GetRelativePath(basePath, fullPath);

        if (!relative.StartsWith("..", StringComparison.Ordinal) && !Path.IsPathRooted(relative))
            return relative;

        return fullPath;
    }

    private void BrowseButton_Click(object? sender, EventArgs e)
    {
        var currentFolder = _outputFolderTextBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(currentFolder) || !Directory.Exists(currentFolder))
            currentFolder = ResolveDefaultOutputFolder();

        using var dialog = new FolderBrowserDialog
        {
            Description = "Select output folder",
            SelectedPath = Directory.Exists(currentFolder) ? currentFolder : AppContext.BaseDirectory
        };

        if (dialog.ShowDialog(this) != DialogResult.OK)
            return;

        _outputFolderTextBox.Text = dialog.SelectedPath;

        try
        {
            SaveDefaultOutputFolder(dialog.SelectedPath);
            _statusLabel.Text = $"Output folder saved to appsettings.json: {dialog.SelectedPath}";
        }
        catch (Exception ex)
        {
            _statusLabel.Text = "Output folder selected, but appsettings.json could not be updated.";
            MessageBox.Show(this, ex.Message, "appsettings.json", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private async void GenerateButton_Click(object? sender, EventArgs e)
    {
        await GenerateAsync();
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
            if (string.IsNullOrWhiteSpace(outputRoot))
                outputRoot = ResolveDefaultOutputFolder();

            try { SaveDefaultOutputFolder(outputRoot); }
            catch { /* Saving the default folder is helpful, but PDF generation should continue. */ }

            Directory.CreateDirectory(outputRoot);

            var safeRoom = SafeFileName(roomNo);

            // Folder per day only, for example: \\192.168.8.74\rc\2026-05-14
            var dayFolder = Path.Combine(outputRoot, DateTime.Now.ToString("yyyy-MM-dd"));
            Directory.CreateDirectory(dayFolder);

            var batchFolder = dayFolder;

            var guestGateOptions = GuestGateOptions.FromConfiguration(_configuration);
            guestGateOptions.DefaultLanguage = _preferredLanguage;

            using var consentClient = guestGateOptions.Enabled
                ? new GuestGateConsentClient(guestGateOptions.BaseUrl)
                : null;

            if (guestGateOptions.Enabled && !guestGateOptions.IsConfigured)
            {
                MessageBox.Show(this, "GuestGate is enabled but BaseUrl or Kid is missing in appsettings.json.", "GuestGate", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var generator = new PdfRegistrationGenerator();
            var createdFiles = new List<string>();
            var index = 1;

            foreach (var data in registrations)
            {
                if (consentClient is not null)
                {
                    _statusLabel.Text = $"Sending guest {index}/{registrations.Count} to GuestGate for signature...";
                    var consent = await consentClient.CreateConsentAsync(
                        data,
                        guestGateOptions.Kid,
                        guestGateOptions.DefaultLanguage);

                    _statusLabel.Text = $"Waiting for guest signature {index}/{registrations.Count}: {data.GuestName}";
                    var signature = await consentClient.WaitForSignatureAsync(
                        consent.Id,
                        TimeSpan.FromSeconds(Math.Max(30, guestGateOptions.SignatureWaitSeconds)));

                    data.GuestSignatureImageDataUrl = signature.SignatureImage;
                    data.GuestGateConsentPdfPath = signature.PdfPath;
                }

                var safeReg = SafeFileName(data.RegNo);
                var safeConfirmation = SafeFileName(data.ConfirmationNo);
                var outputPath = Path.Combine(batchFolder, $"RC_{index:000}_Room_{safeRoom}_Reg_{safeReg}_Conf_{safeConfirmation}.pdf");
                generator.Generate(templatePath, outputPath, data);

                createdFiles.Add(outputPath);

                index++; ;
            }

            _statusLabel.Text = $"Created {registrations.Count} RC PDF file(s): {batchFolder}";

            var fileToOpen = createdFiles.LastOrDefault();

            if (!string.IsNullOrWhiteSpace(fileToOpen) && File.Exists(fileToOpen))
            {
                if (MessageBox.Show(this, $"Do you want to review the RC", "Done", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                {
                    Process.Start(new ProcessStartInfo(fileToOpen) { UseShellExecute = true });
                }
            }
            else
            {
                MessageBox.Show(this, "ERROR.", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information);
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

    private void LanguageComboBox_SelectedIndexChanged(object? sender, EventArgs e)
    {
        _preferredLanguage = NormalizeLanguage(_languageComboBox.SelectedItem?.ToString());

        try
        {
            SaveDefaultGuestGateLanguage(_preferredLanguage);
            _statusLabel.Text = $"Preferred language saved: {_preferredLanguage.ToUpperInvariant()}";
        }
        catch (Exception ex)
        {
            _statusLabel.Text = "Preferred language changed for this session only.";
            MessageBox.Show(this, ex.Message, "appsettings.json", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private async void EndSessionButton_Click(object? sender, EventArgs e)
    {
        await EndGuestGateSessionAsync();
    }

    private async Task EndGuestGateSessionAsync()
    {
        var guestGateOptions = GuestGateOptions.FromConfiguration(_configuration);
        if (!guestGateOptions.Enabled)
        {
            _statusLabel.Text = "GuestGate is disabled.";
            return;
        }

        if (!guestGateOptions.IsConfigured)
        {
            MessageBox.Show(this, "GuestGate BaseUrl or Kid is missing in appsettings.json.", "GuestGate", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            _endSessionButton.Enabled = false;
            _statusLabel.Text = "Ending kiosk session...";

            using var consentClient = new GuestGateConsentClient(guestGateOptions.BaseUrl);
            await consentClient.EndActiveSessionAsync(guestGateOptions.Kid);

            _statusLabel.Text = "Kiosk session ended.";
        }
        catch (Exception ex)
        {
            _statusLabel.Text = "Could not end kiosk session.";
            MessageBox.Show(this, ex.Message, "End Session", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            _endSessionButton.Enabled = true;
        }
    }

    private void button1_Click(object sender, EventArgs e)
    {
        this.WindowState = FormWindowState.Minimized;
    }

    private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
    {
        if (_allowClose)
            return;

        e.Cancel = true;
        this.WindowState = FormWindowState.Minimized;
    }
}