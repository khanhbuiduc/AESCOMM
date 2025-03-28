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
        tabPage2 = new TabPage();
        txtMessage = new TextBox();
        btnSend = new Button();
        deviceList = new ListBox();
        btnScan = new Button();
        btnSelectFile = new Button();
        tabControl1.SuspendLayout();
        tabPage1.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)logoPictureBox).BeginInit();
        tabPage2.SuspendLayout();
        SuspendLayout();
        // 
        // tabControl1
        // 
        tabControl1.Controls.Add(tabPage1);
        tabControl1.Controls.Add(tabPage2);
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
        lblHistory.Location = new Point(61, 20);
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
        logoPictureBox.Location = new Point(377, 20);
        logoPictureBox.Name = "logoPictureBox";
        logoPictureBox.Size = new Size(250, 250);
        logoPictureBox.SizeMode = PictureBoxSizeMode.Zoom;
        logoPictureBox.TabIndex = 0;
        logoPictureBox.TabStop = false;
        // 
        // listenPanel
        // 
        listenPanel.Location = new Point(131, 112);
        listenPanel.Name = "listenPanel";
        listenPanel.Size = new Size(200, 100);
        listenPanel.TabIndex = 0;
        // 
        // tabPage2
        // 
        tabPage2.Controls.Add(txtMessage);
        tabPage2.Controls.Add(btnSend);
        tabPage2.Controls.Add(deviceList);
        tabPage2.Controls.Add(btnScan);
        tabPage2.Controls.Add(btnSelectFile);
        tabPage2.Location = new Point(4, 24);
        tabPage2.Margin = new Padding(3, 2, 3, 2);
        tabPage2.Name = "tabPage2";
        tabPage2.Size = new Size(692, 310);
        tabPage2.TabIndex = 1;
        tabPage2.Text = "Send";
        // 
        // txtMessage
        // 
        txtMessage.Location = new Point(10, 50);
        txtMessage.Margin = new Padding(3, 2, 3, 2);
        txtMessage.Multiline = true;
        txtMessage.Name = "txtMessage";
        txtMessage.ScrollBars = ScrollBars.Vertical;
        txtMessage.Size = new Size(460, 74);
        txtMessage.TabIndex = 0;
        // 
        // btnSend
        // 
        btnSend.Location = new Point(248, 242);
        btnSend.Margin = new Padding(3, 2, 3, 2);
        btnSend.Name = "btnSend";
        btnSend.Size = new Size(174, 47);
        btnSend.TabIndex = 1;
        btnSend.Text = "Send";
        // 
        // deviceList
        // 
        deviceList.ItemHeight = 15;
        deviceList.Location = new Point(492, 66);
        deviceList.Margin = new Padding(3, 2, 3, 2);
        deviceList.Name = "deviceList";
        deviceList.Size = new Size(176, 169);
        deviceList.TabIndex = 6;
        deviceList.SelectedIndexChanged += DeviceList_SelectedIndexChanged;
        // 
        // btnScan
        // 
        btnScan.Location = new Point(525, 15);
        btnScan.Margin = new Padding(3, 2, 3, 2);
        btnScan.Name = "btnScan";
        btnScan.Size = new Size(105, 22);
        btnScan.TabIndex = 7;
        btnScan.Text = "Scan Network";
        btnScan.Click += BtnScan_Click;
        // 
        // btnSelectFile
        // 
        btnSelectFile.Location = new Point(10, 15);
        btnSelectFile.Margin = new Padding(3, 2, 3, 2);
        btnSelectFile.Name = "btnSelectFile";
        btnSelectFile.Size = new Size(78, 22);
        btnSelectFile.TabIndex = 8;
        btnSelectFile.Text = "Select File";
        btnSelectFile.Click += BtnSelectFile_Click;
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

    #endregion

    private Panel listenPanel;
}
