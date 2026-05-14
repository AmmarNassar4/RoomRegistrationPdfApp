using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace RoomRegistrationPdfApp;

public sealed partial class MainForm : Form
{
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

        InitializeComponent();
        _outputFolderTextBox.Text = ResolveDefaultOutputFolder();
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
        var settingsPath = AppSettingsPath;

        JsonObject root;
        if (File.Exists(settingsPath))
        {
            var json = File.ReadAllText(settingsPath, Encoding.UTF8);
            root = JsonNode.Parse(json)?.AsObject() ?? new JsonObject();
        }
        else
        {
            root = new JsonObject();
        }

        root["DefaultOutputFolder"] = ToAppSettingsPath(fullPath);

        var options = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        File.WriteAllText(settingsPath, root.ToJsonString(options), Encoding.UTF8);
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
            var batchFolder = Path.Combine(outputRoot, $"Room_{safeRoom}_{DateTime.Now:yyyyMMdd_HHmmss}");
            Directory.CreateDirectory(batchFolder);

            var guestGateOptions = GuestGateOptions.FromConfiguration(_configuration);
            using var consentClient = guestGateOptions.Enabled
                ? new GuestGateConsentClient(guestGateOptions.BaseUrl)
                : null;

            if (guestGateOptions.Enabled && !guestGateOptions.IsConfigured)
            {
                MessageBox.Show(this, "GuestGate is enabled but BaseUrl or Kid is missing in appsettings.json.", "GuestGate", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var generator = new PdfRegistrationGenerator();
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

    private void button1_Click(object sender, EventArgs e)
    {
        this.WindowState = FormWindowState.Minimized;
    }
}
