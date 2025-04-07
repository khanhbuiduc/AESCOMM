namespace AES2;

partial class Form1
{
    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    ///  Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    // protected override void Dispose(bool(disposing)
    // {
    //     if (disposing && (components != null))
    //     {
    //         components.Dispose();
    //     }
    //     base.Dispose(disposing);
    // }

    #region Windows Form Designer generated code

    /// <summary>
    ///  Required method for Designer support - do not modify
    ///  the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        tabControl1 = new TabControl();
        tabPage1 = new TabPage();
        lblHistory = new Label();
        logoPictureBox = new PictureBox();
        listenPanel = new Panel();
        lblDeviceName = new Label();
        lblInformation = new Label();
        tabPage2 = new TabPage();
        lblEncryptionKey = new Label();
        txtEncryptionKey = new TextBox();
        txtMessage = new TextBox();
        btnSend = new Button();
        deviceList = new ListBox();
        btnScan = new Button();
        btnSelectFile = new Button();
        lblHost = new Label();
        txtHost = new TextBox();
        lblPort = new Label();
        txtPort = new TextBox();
        tabPage3 = new TabPage();
        btnDecrypt = new Button();
        lblSelectedFile = new Label();
        btnSelectEncrypted = new Button();
        lblDecryptionKey = new Label();
        txtDecryptionKey = new TextBox();
        tabControl1.SuspendLayout();
        tabPage1.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)logoPictureBox).BeginInit();
        tabPage2.SuspendLayout();
        tabPage3.SuspendLayout();
        SuspendLayout();
        // 
        // tabControl1
        // 
        tabControl1.Controls.Add(tabPage1);
        tabControl1.Controls.Add(tabPage2);
        tabControl1.Controls.Add(tabPage3);
        tabControl1.Dock = DockStyle.Fill;
        tabControl1.Location = new Point(0, 0);
        tabControl1.Margin = new Padding(3, 2, 3, 2);
        tabControl1.Name = "tabControl1";
        tabControl1.SelectedIndex = 0;
        tabControl1.Size = new Size(700, 338);
        tabControl1.TabIndex = 0;
        // 
        // tabPage1
        // 
        tabPage1.Controls.Add(lblHistory);
        tabPage1.Controls.Add(logoPictureBox);
        tabPage1.Controls.Add(listenPanel);
        tabPage1.Controls.Add(lblDeviceName);
        tabPage1.Controls.Add(lblInformation);
        tabPage1.Location = new Point(4, 24);
        tabPage1.Margin = new Padding(3, 2, 3, 2);
        tabPage1.Name = "tabPage1";
        tabPage1.Size = new Size(692, 310);
        tabPage1.TabIndex = 0;
        tabPage1.Text = "Listen";
        tabPage1.Click += tabPage1_Click;
        // 
        // lblHistory
        // 
        lblHistory.BackColor = Color.Transparent;
        lblHistory.Cursor = Cursors.Hand;
        lblHistory.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
        lblHistory.ForeColor = Color.SlateGray;
        lblHistory.Image = Properties.Resources.history_301;
        lblHistory.ImageAlign = ContentAlignment.MiddleLeft;
        lblHistory.Location = new Point(117, 129);
        lblHistory.Name = "lblHistory";
        lblHistory.Size = new Size(101, 21);
        lblHistory.TabIndex = 1;
        lblHistory.Text = "History";
        lblHistory.TextAlign = ContentAlignment.MiddleRight;
        lblHistory.Click += LblHistory_Click;
        // 
        // logoPictureBox
        // 
        logoPictureBox.Anchor = AnchorStyles.None;
        logoPictureBox.ErrorImage = Properties.Resources.security_aes;
        logoPictureBox.Location = new Point(343, 0);
        logoPictureBox.Name = "logoPictureBox";
        logoPictureBox.Size = new Size(346, 307);
        logoPictureBox.SizeMode = PictureBoxSizeMode.Zoom;
        logoPictureBox.TabIndex = 0;
        logoPictureBox.TabStop = false;
        // 
        // listenPanel
        // 
        listenPanel.Location = new Point(67, 202);
        listenPanel.Name = "listenPanel";
        listenPanel.Size = new Size(200, 100);
        listenPanel.TabIndex = 0;
        // 
        // lblDeviceName
        // 
        lblDeviceName.BackColor = Color.Transparent;
        lblDeviceName.Cursor = Cursors.Hand;
        lblDeviceName.Font = new Font("Segoe UI", 32F, FontStyle.Bold);
        lblDeviceName.ForeColor = Color.LightSlateGray;
        lblDeviceName.Location = new Point(-4, 20);
        lblDeviceName.Name = "lblDeviceName";
        lblDeviceName.Size = new Size(375, 90);
        lblDeviceName.TabIndex = 2;
        lblDeviceName.Text = "Device name";
        lblDeviceName.TextAlign = ContentAlignment.MiddleCenter;
        // 
        // lblInformation
        // 
        lblInformation.BackColor = Color.Transparent;
        lblInformation.Cursor = Cursors.Hand;
        lblInformation.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
        lblInformation.ForeColor = Color.DarkSlateGray;
        lblInformation.Image = Properties.Resources.infor_30;
        lblInformation.ImageAlign = ContentAlignment.MiddleLeft;
        lblInformation.Location = new Point(117, 160);
        lblInformation.Name = "lblInformation";
        lblInformation.Size = new Size(138, 30);
        lblInformation.TabIndex = 3;
        lblInformation.Text = "Information";
        lblInformation.TextAlign = ContentAlignment.MiddleRight;
        lblInformation.Click += LblInformation_Click;
        // 
        // tabPage2
        // 
        tabPage2.Controls.Add(lblEncryptionKey);
        tabPage2.Controls.Add(txtEncryptionKey);
        tabPage2.Controls.Add(txtMessage);
        tabPage2.Controls.Add(btnSend);
        tabPage2.Controls.Add(deviceList);
        tabPage2.Controls.Add(btnScan);
        tabPage2.Controls.Add(btnSelectFile);
        tabPage2.Controls.Add(lblHost);
        tabPage2.Controls.Add(txtHost);
        tabPage2.Controls.Add(lblPort);
        tabPage2.Controls.Add(txtPort);
        tabPage2.Location = new Point(4, 24);
        tabPage2.Margin = new Padding(3, 2, 3, 2);
        tabPage2.Name = "tabPage2";
        tabPage2.Size = new Size(692, 310);
        tabPage2.TabIndex = 1;
        tabPage2.Text = "Send";
        // 
        // lblEncryptionKey
        // 
        lblEncryptionKey.Location = new Point(215, 130);
        lblEncryptionKey.Name = "lblEncryptionKey";
        lblEncryptionKey.Size = new Size(100, 20);
        lblEncryptionKey.TabIndex = 13;
        lblEncryptionKey.Text = "Encryption Key:";
        // 
        // txtEncryptionKey
        // 
        txtEncryptionKey.Location = new Point(215, 150);
        txtEncryptionKey.Name = "txtEncryptionKey";
        txtEncryptionKey.Size = new Size(260, 23);
        txtEncryptionKey.TabIndex = 14;
        txtEncryptionKey.PlaceholderText = "Enter 64 hex characters (256-bit key)";
        // 
        // txtMessage
        // 
        txtMessage.Location = new Point(28, 50);
        txtMessage.Margin = new Padding(3, 2, 3, 2);
        txtMessage.Multiline = true;
        txtMessage.Name = "txtMessage";
        txtMessage.Size = new Size(168, 169);
        txtMessage.TabIndex = 0;
        // 
        // btnSend
        // 
        btnSend.BackColor = SystemColors.ActiveCaption;
        btnSend.Font = new Font("Segoe UI", 24F, FontStyle.Bold, GraphicsUnit.Point, 0);
        btnSend.ForeColor = SystemColors.Window;
        btnSend.Location = new Point(215, 235);
        btnSend.Margin = new Padding(3, 2, 3, 2);
        btnSend.Name = "btnSend";
        btnSend.Size = new Size(260, 54);
        btnSend.TabIndex = 1;
        btnSend.Text = "Send";
        btnSend.UseVisualStyleBackColor = false;
        // 
        // deviceList
        // 
        deviceList.ItemHeight = 15;
        deviceList.Location = new Point(492, 50);
        deviceList.Margin = new Padding(3, 2, 3, 2);
        deviceList.Name = "deviceList";
        deviceList.Size = new Size(176, 169);
        deviceList.TabIndex = 6;
        deviceList.SelectedIndexChanged += DeviceList_SelectedIndexChanged;
        // 
        // btnScan
        // 
        btnScan.BackColor = SystemColors.ControlDark;
        btnScan.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
        btnScan.ForeColor = SystemColors.Window;
        btnScan.Location = new Point(492, 223);
        btnScan.Margin = new Padding(3, 2, 3, 2);
        btnScan.Name = "btnScan";
        btnScan.Size = new Size(176, 35);
        btnScan.TabIndex = 7;
        btnScan.Text = "Scan Network";
        btnScan.UseVisualStyleBackColor = false;
        btnScan.Click += BtnScan_Click;
        // 
        // btnSelectFile
        // 
        btnSelectFile.BackColor = SystemColors.ControlDark;
        btnSelectFile.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
        btnSelectFile.ForeColor = SystemColors.Window;
        btnSelectFile.Location = new Point(28, 223);
        btnSelectFile.Margin = new Padding(3, 2, 3, 2);
        btnSelectFile.Name = "btnSelectFile";
        btnSelectFile.Size = new Size(168, 35);
        btnSelectFile.TabIndex = 8;
        btnSelectFile.Text = "Select File";
        btnSelectFile.UseVisualStyleBackColor = false;
        btnSelectFile.Click += BtnSelectFile_Click;
        // 
        // lblHost
        // 
        lblHost.Location = new Point(215, 50);
        lblHost.Name = "lblHost";
        lblHost.Size = new Size(40, 20);
        lblHost.TabIndex = 9;
        lblHost.Text = "Host:";
        // 
        // txtHost
        // 
        txtHost.Location = new Point(275, 50);
        txtHost.Name = "txtHost";
        txtHost.Size = new Size(200, 23);
        txtHost.TabIndex = 10;
        txtHost.TextChanged += txtHost_TextChanged;
        // 
        // lblPort
        // 
        lblPort.Location = new Point(215, 91);
        lblPort.Name = "lblPort";
        lblPort.Size = new Size(40, 20);
        lblPort.TabIndex = 11;
        lblPort.Text = "Port:";
        // 
        // txtPort
        // 
        txtPort.Location = new Point(275, 88);
        txtPort.Name = "txtPort";
        txtPort.Size = new Size(80, 23);
        txtPort.TabIndex = 12;
        // 
        // tabPage3
        // 
        tabPage3.Controls.Add(btnDecrypt);
        tabPage3.Controls.Add(lblSelectedFile);
        tabPage3.Controls.Add(btnSelectEncrypted);
        tabPage3.Controls.Add(lblDecryptionKey);
        tabPage3.Controls.Add(txtDecryptionKey);
        tabPage3.Location = new Point(4, 24);
        tabPage3.Margin = new Padding(3, 2, 3, 2);
        tabPage3.Name = "tabPage3";
        tabPage3.Size = new Size(692, 310);
        tabPage3.TabIndex = 2;
        tabPage3.Text = "Decrypt";
        tabPage3.UseVisualStyleBackColor = true;
        // 
        // btnDecrypt
        // 
        btnDecrypt.BackColor = SystemColors.ActiveCaption;
        btnDecrypt.Font = new Font("Segoe UI", 18F, FontStyle.Bold, GraphicsUnit.Point, 0);
        btnDecrypt.ForeColor = SystemColors.Window;
        btnDecrypt.Location = new Point(215, 220);
        btnDecrypt.Name = "btnDecrypt";
        btnDecrypt.Size = new Size(260, 54);
        btnDecrypt.TabIndex = 4;
        btnDecrypt.Text = "Decrypt File";
        btnDecrypt.UseVisualStyleBackColor = false;
        // 
        // lblSelectedFile
        // 
        lblSelectedFile.AutoSize = true;
        lblSelectedFile.Location = new Point(215, 100);
        lblSelectedFile.Name = "lblSelectedFile";
        lblSelectedFile.Size = new Size(126, 15);
        lblSelectedFile.TabIndex = 3;
        lblSelectedFile.Text = "No encrypted file selected";
        // 
        // btnSelectEncrypted
        // 
        btnSelectEncrypted.BackColor = SystemColors.ControlDark;
        btnSelectEncrypted.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        btnSelectEncrypted.ForeColor = SystemColors.Window;
        btnSelectEncrypted.Location = new Point(215, 50);
        btnSelectEncrypted.Name = "btnSelectEncrypted";
        btnSelectEncrypted.Size = new Size(170, 35);
        btnSelectEncrypted.TabIndex = 2;
        btnSelectEncrypted.Text = "Select Encrypted File";
        btnSelectEncrypted.UseVisualStyleBackColor = false;
        // 
        // lblDecryptionKey
        // 
        lblDecryptionKey.AutoSize = true;
        lblDecryptionKey.Location = new Point(215, 140);
        lblDecryptionKey.Name = "lblDecryptionKey";
        lblDecryptionKey.Size = new Size(88, 15);
        lblDecryptionKey.TabIndex = 1;
        lblDecryptionKey.Text = "Decryption Key:";
        // 
        // txtDecryptionKey
        // 
        txtDecryptionKey.Location = new Point(215, 160);
        txtDecryptionKey.Name = "txtDecryptionKey";
        txtDecryptionKey.Size = new Size(260, 23);
        txtDecryptionKey.TabIndex = 0;
        txtDecryptionKey.PlaceholderText = "Enter 64 hex characters (256-bit key)";
        // 
        // Form1
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(700, 338);
        Controls.Add(tabControl1);
        Margin = new Padding(3, 2, 3, 2);
        Name = "Form1";
        Text = "TCP Messenger";
        tabControl1.ResumeLayout(false);
        tabPage1.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)logoPictureBox).EndInit();
        tabPage2.ResumeLayout(false);
        tabPage2.PerformLayout();
        tabPage3.ResumeLayout(false);
        tabPage3.PerformLayout();
        ResumeLayout(false);
    }

    private TabControl tabControl1;
    private TabPage tabPage1;
    private TabPage tabPage2;
    private TextBox txtMessage;
    private Button btnSend;
    private ListBox deviceList;
    private Button btnScan;
    private Button btnSelectFile;
    private PictureBox logoPictureBox;
    private Label lblHistory;
    private Label lblDeviceName;
    private Label lblHost;
    private TextBox txtHost;
    private Label lblPort;
    private TextBox txtPort;
    private Label lblInformation;
    private Label lblEncryptionKey;
    private TextBox txtEncryptionKey;
    private TabPage tabPage3;
    private Button btnDecrypt;
    private Label lblSelectedFile;
    private Button btnSelectEncrypted;
    private Label lblDecryptionKey;
    private TextBox txtDecryptionKey;

    #endregion

    private Panel listenPanel;
}
