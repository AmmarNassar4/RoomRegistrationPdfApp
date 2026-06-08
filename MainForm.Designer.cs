#nullable enable

namespace RoomRegistrationPdfApp;

partial class MainForm
{
    private System.ComponentModel.IContainer? components = null;

    private Label roomLabel = null!;
    private Label outputLabel = null!;
    private TextBox _roomTextBox = null!;
    private TextBox _outputFolderTextBox = null!;
    private Button _browseButton = null!;
    private Button _generateButton = null!;
    private ComboBox _languageComboBox = null!;
    private Button _endSessionButton = null!;
    private StatusStrip _statusStrip = null!;
    private ToolStripStatusLabel _statusLabel = null!;
    private ToolStripStatusLabel _connectionStatusLabel = null!;
    private ToolStripStatusLabel _kioskStatusLabel = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing && components is not null)
        {
            components.Dispose();
        }

        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
        roomLabel = new Label();
        outputLabel = new Label();
        _roomTextBox = new TextBox();
        _outputFolderTextBox = new TextBox();
        _browseButton = new Button();
        _generateButton = new Button();
        _languageComboBox = new ComboBox();
        _endSessionButton = new Button();
        _statusStrip = new StatusStrip();
        _statusLabel = new ToolStripStatusLabel();
        _connectionStatusLabel = new ToolStripStatusLabel();
        _kioskStatusLabel = new ToolStripStatusLabel();
        _statusStrip.SuspendLayout();
        SuspendLayout();
        // 
        // roomLabel
        // 
        roomLabel.Location = new Point(12, 24);
        roomLabel.Name = "roomLabel";
        roomLabel.Size = new Size(65, 23);
        roomLabel.TabIndex = 0;
        roomLabel.Text = "Room No:";
        roomLabel.DoubleClick += roomLabel_DoubleClick;
        // 
        // outputLabel
        // 
        outputLabel.Location = new Point(25, 75);
        outputLabel.Name = "outputLabel";
        outputLabel.Size = new Size(120, 23);
        outputLabel.TabIndex = 2;
        outputLabel.Text = "Output folder:";
        // 
        // _roomTextBox
        // 
        _roomTextBox.Font = new Font("Segoe UI", 11F);
        _roomTextBox.Location = new Point(82, 21);
        _roomTextBox.Name = "_roomTextBox";
        _roomTextBox.Size = new Size(75, 27);
        _roomTextBox.TabIndex = 1;
        // 
        // _outputFolderTextBox
        // 
        _outputFolderTextBox.Location = new Point(150, 70);
        _outputFolderTextBox.Name = "_outputFolderTextBox";
        _outputFolderTextBox.Size = new Size(380, 23);
        _outputFolderTextBox.TabIndex = 3;
        // 
        // _browseButton
        // 
        _browseButton.Location = new Point(540, 68);
        _browseButton.Name = "_browseButton";
        _browseButton.Size = new Size(85, 27);
        _browseButton.TabIndex = 4;
        _browseButton.Text = "Browse";
        _browseButton.UseVisualStyleBackColor = true;
        _browseButton.Click += BrowseButton_Click;
        // 
        // _generateButton
        // 
        _generateButton.Location = new Point(163, 19);
        _generateButton.Name = "_generateButton";
        _generateButton.Size = new Size(102, 32);
        _generateButton.TabIndex = 5;
        _generateButton.Text = "Send To tablet";
        _generateButton.UseVisualStyleBackColor = true;
        _generateButton.Click += GenerateButton_Click;
        // 
        // _languageComboBox
        // 
        _languageComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
        _languageComboBox.FormattingEnabled = true;
        _languageComboBox.Items.AddRange(new object[] { "Arabic", "English" });
        _languageComboBox.Location = new Point(271, 23);
        _languageComboBox.Name = "_languageComboBox";
        _languageComboBox.Size = new Size(112, 23);
        _languageComboBox.TabIndex = 6;
        _languageComboBox.SelectedIndexChanged += LanguageComboBox_SelectedIndexChanged;
        // 
        // _endSessionButton
        // 
        _endSessionButton.Enabled = false;
        _endSessionButton.Location = new Point(389, 19);
        _endSessionButton.Name = "_endSessionButton";
        _endSessionButton.Size = new Size(92, 32);
        _endSessionButton.TabIndex = 8;
        _endSessionButton.Text = "End Session";
        _endSessionButton.UseVisualStyleBackColor = true;
        _endSessionButton.Click += EndSessionButton_Click;
        // 
        // _statusStrip
        // 
        _statusStrip.Items.AddRange(new ToolStripItem[] { _statusLabel, _connectionStatusLabel, _kioskStatusLabel });
        _statusStrip.Location = new Point(0, 103);
        _statusStrip.Name = "_statusStrip";
        _statusStrip.Size = new Size(650, 22);
        _statusStrip.TabIndex = 9;
        _statusStrip.Text = "statusStrip1";
        // 
        // _statusLabel
        // 
        _statusLabel.ForeColor = Color.DarkSlateGray;
        _statusLabel.Name = "_statusLabel";
        _statusLabel.Size = new Size(373, 17);
        _statusLabel.Spring = true;
        _statusLabel.Text = "Ready.";
        _statusLabel.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // _connectionStatusLabel
        // 
        _connectionStatusLabel.BorderSides = ToolStripStatusLabelBorderSides.Left;
        _connectionStatusLabel.ForeColor = Color.DarkGoldenrod;
        _connectionStatusLabel.Name = "_connectionStatusLabel";
        _connectionStatusLabel.Size = new Size(149, 17);
        _connectionStatusLabel.Text = "GuestGate: Checking...";
        // 
        // _kioskStatusLabel
        // 
        _kioskStatusLabel.BorderSides = ToolStripStatusLabelBorderSides.Left;
        _kioskStatusLabel.Name = "_kioskStatusLabel";
        _kioskStatusLabel.Size = new Size(113, 17);
        _kioskStatusLabel.Text = "Kiosk: Not set";
        // 
        // MainForm
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(650, 125);
        Controls.Add(_endSessionButton);
        Controls.Add(_languageComboBox);
        Controls.Add(roomLabel);
        Controls.Add(_roomTextBox);
        Controls.Add(outputLabel);
        Controls.Add(_outputFolderTextBox);
        Controls.Add(_browseButton);
        Controls.Add(_generateButton);
        Controls.Add(_statusStrip);
        FormBorderStyle = FormBorderStyle.FixedToolWindow;
        Icon = (Icon)resources.GetObject("$this.Icon");
        MaximizeBox = false;
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Guest Registration RC PDFs";
        TopMost = true;
        Shown += MainForm_Shown;
        FormClosing += MainForm_FormClosing;
        _statusStrip.ResumeLayout(false);
        _statusStrip.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }
}
