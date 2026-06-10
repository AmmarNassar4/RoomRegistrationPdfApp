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
        roomLabel.Location = new Point(14, 32);
        roomLabel.Name = "roomLabel";
        roomLabel.Size = new Size(74, 31);
        roomLabel.TabIndex = 0;
        roomLabel.Text = "Room No:";
        roomLabel.DoubleClick += roomLabel_DoubleClick;
        // 
        // outputLabel
        // 
        outputLabel.Location = new Point(29, 100);
        outputLabel.Name = "outputLabel";
        outputLabel.Size = new Size(137, 31);
        outputLabel.TabIndex = 2;
        outputLabel.Text = "Output folder:";
        outputLabel.Visible = false;
        // 
        // _roomTextBox
        // 
        _roomTextBox.Font = new Font("Segoe UI", 11F);
        _roomTextBox.Location = new Point(94, 28);
        _roomTextBox.Margin = new Padding(3, 4, 3, 4);
        _roomTextBox.Name = "_roomTextBox";
        _roomTextBox.Size = new Size(85, 32);
        _roomTextBox.TabIndex = 1;
        // 
        // _outputFolderTextBox
        // 
        _outputFolderTextBox.Location = new Point(171, 93);
        _outputFolderTextBox.Margin = new Padding(3, 4, 3, 4);
        _outputFolderTextBox.Name = "_outputFolderTextBox";
        _outputFolderTextBox.Size = new Size(434, 27);
        _outputFolderTextBox.TabIndex = 3;
        _outputFolderTextBox.Visible = false;
        // 
        // _browseButton
        // 
        _browseButton.Location = new Point(617, 91);
        _browseButton.Margin = new Padding(3, 4, 3, 4);
        _browseButton.Name = "_browseButton";
        _browseButton.Size = new Size(97, 36);
        _browseButton.TabIndex = 4;
        _browseButton.Text = "Browse";
        _browseButton.UseVisualStyleBackColor = true;
        _browseButton.Visible = false;
        _browseButton.Click += BrowseButton_Click;
        // 
        // _generateButton
        // 
        _generateButton.Location = new Point(186, 25);
        _generateButton.Margin = new Padding(3, 4, 3, 4);
        _generateButton.Name = "_generateButton";
        _generateButton.Size = new Size(117, 34);
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
        _languageComboBox.Location = new Point(310, 31);
        _languageComboBox.Margin = new Padding(3, 4, 3, 4);
        _languageComboBox.Name = "_languageComboBox";
        _languageComboBox.Size = new Size(127, 28);
        _languageComboBox.TabIndex = 6;
        _languageComboBox.SelectedIndexChanged += LanguageComboBox_SelectedIndexChanged;
        // 
        // _endSessionButton
        // 
        _endSessionButton.Enabled = false;
        _endSessionButton.Location = new Point(445, 25);
        _endSessionButton.Margin = new Padding(3, 4, 3, 4);
        _endSessionButton.Name = "_endSessionButton";
        _endSessionButton.Size = new Size(105, 38);
        _endSessionButton.TabIndex = 8;
        _endSessionButton.Text = "End Session";
        _endSessionButton.UseVisualStyleBackColor = true;
        _endSessionButton.Click += EndSessionButton_Click;
        // 
        // _statusStrip
        // 
        _statusStrip.ImageScalingSize = new Size(20, 20);
        _statusStrip.Items.AddRange(new ToolStripItem[] { _statusLabel, _connectionStatusLabel, _kioskStatusLabel });
        _statusStrip.Location = new Point(0, 73);
        _statusStrip.Name = "_statusStrip";
        _statusStrip.Padding = new Padding(1, 0, 16, 0);
        _statusStrip.Size = new Size(570, 30);
        _statusStrip.TabIndex = 9;
        _statusStrip.Text = "statusStrip1";
        // 
        // _statusLabel
        // 
        _statusLabel.ForeColor = Color.DarkSlateGray;
        _statusLabel.Name = "_statusLabel";
        _statusLabel.Size = new Size(293, 24);
        _statusLabel.Spring = true;
        _statusLabel.Text = "Ready.";
        _statusLabel.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // _connectionStatusLabel
        // 
        _connectionStatusLabel.BorderSides = ToolStripStatusLabelBorderSides.Left;
        _connectionStatusLabel.ForeColor = Color.DarkGoldenrod;
        _connectionStatusLabel.Name = "_connectionStatusLabel";
        _connectionStatusLabel.Size = new Size(157, 24);
        _connectionStatusLabel.Text = "GuestGate: Checking...";
        // 
        // _kioskStatusLabel
        // 
        _kioskStatusLabel.BorderSides = ToolStripStatusLabelBorderSides.Left;
        _kioskStatusLabel.Name = "_kioskStatusLabel";
        _kioskStatusLabel.Size = new Size(103, 24);
        _kioskStatusLabel.Text = "Kiosk: Not set";
        // 
        // MainForm
        // 
        AutoScaleDimensions = new SizeF(8F, 20F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(570, 103);
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
        Margin = new Padding(3, 4, 3, 4);
        MaximizeBox = false;
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Guest Registration RC PDFs";
        TopMost = true;
        FormClosing += MainForm_FormClosing;
        Shown += MainForm_Shown;
        _statusStrip.ResumeLayout(false);
        _statusStrip.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }
}
