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
    private Button _languageButton = null!;
    private Button _endSessionButton = null!;
    private Label _statusLabel = null!;

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
        roomLabel = new Label();
        outputLabel = new Label();
        _roomTextBox = new TextBox();
        _outputFolderTextBox = new TextBox();
        _browseButton = new Button();
        _generateButton = new Button();
        _languageButton = new Button();
        _endSessionButton = new Button();
        _statusLabel = new Label();
        button1 = new Button();
        SuspendLayout();
        // 
        // roomLabel
        // 
        roomLabel.Location = new Point(12, 24);
        roomLabel.Name = "roomLabel";
        roomLabel.Size = new Size(65, 23);
        roomLabel.TabIndex = 0;
        roomLabel.Text = "Room No:";
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
        // _languageButton
        // 
        _languageButton.Location = new Point(271, 19);
        _languageButton.Name = "_languageButton";
        _languageButton.Size = new Size(112, 32);
        _languageButton.TabIndex = 6;
        _languageButton.Text = "Language: AR";
        _languageButton.UseVisualStyleBackColor = true;
        _languageButton.Click += LanguageButton_Click;
        // 
        // _endSessionButton
        // 
        _endSessionButton.Location = new Point(461, 19);
        _endSessionButton.Name = "_endSessionButton";
        _endSessionButton.Size = new Size(92, 32);
        _endSessionButton.TabIndex = 8;
        _endSessionButton.Text = "End Session";
        _endSessionButton.UseVisualStyleBackColor = true;
        _endSessionButton.Click += EndSessionButton_Click;
        // 
        // _statusLabel
        // 
        _statusLabel.ForeColor = Color.DarkSlateGray;
        _statusLabel.Location = new Point(25, 175);
        _statusLabel.Name = "_statusLabel";
        _statusLabel.Size = new Size(610, 50);
        _statusLabel.TabIndex = 9;
        // 
        // button1
        // 
        button1.Location = new Point(389, 19);
        button1.Name = "button1";
        button1.Size = new Size(66, 32);
        button1.TabIndex = 7;
        button1.Text = "Minimize";
        button1.UseVisualStyleBackColor = true;
        button1.Click += button1_Click;
        // 
        // MainForm
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(565, 62);
        Controls.Add(button1);
        Controls.Add(_endSessionButton);
        Controls.Add(_languageButton);
        Controls.Add(roomLabel);
        Controls.Add(_roomTextBox);
        Controls.Add(outputLabel);
        Controls.Add(_outputFolderTextBox);
        Controls.Add(_browseButton);
        Controls.Add(_generateButton);
        Controls.Add(_statusLabel);
        FormBorderStyle = FormBorderStyle.FixedToolWindow;
        MaximizeBox = false;
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Guest Registration RC PDFs";
        TopMost = true;
        FormClosing += MainForm_FormClosing;
        ResumeLayout(false);
        PerformLayout();
    }

    private Button button1;
}