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
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    ///  Required method for Designer support - do not modify
    ///  the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        tabControl1 = new TabControl();
        tabPage1 = new TabPage();
        txtReceived = new TextBox();
        btnListen = new Button();
        txtListenPort = new TextBox();
        label1 = new Label();
        tabPage2 = new TabPage();
        txtMessage = new TextBox();
        btnSend = new Button();
        txtSendPort = new TextBox();
        txtHost = new TextBox();
        label3 = new Label();
        label2 = new Label();

        tabControl1.SuspendLayout();
        tabPage1.SuspendLayout();
        tabPage2.SuspendLayout();
        SuspendLayout();

        // tabControl1
        tabControl1.Controls.Add(tabPage1);
        tabControl1.Controls.Add(tabPage2);
        tabControl1.Dock = DockStyle.Fill;
        tabControl1.Location = new Point(0, 0);
        tabControl1.Name = "tabControl1";
        tabControl1.SelectedIndex = 0;
        tabControl1.Size = new Size(800, 450);
        tabControl1.TabIndex = 0;

        // tabPage1 (Listen)
        tabPage1.Controls.Add(txtReceived);
        tabPage1.Controls.Add(btnListen);
        tabPage1.Controls.Add(txtListenPort);
        tabPage1.Controls.Add(label1);
        tabPage1.Location = new Point(4, 29);
        tabPage1.Name = "tabPage1";
        tabPage1.Size = new Size(792, 417);
        tabPage1.TabIndex = 0;
        tabPage1.Text = "Listen";

        // Listen controls
        label1.AutoSize = true;
        label1.Location = new Point(10, 20);
        label1.Name = "label1";
        label1.Size = new Size(35, 20);
        label1.Text = "Port:";

        txtListenPort.Location = new Point(50, 17);
        txtListenPort.Name = "txtListenPort";
        txtListenPort.Size = new Size(100, 27);

        btnListen.Location = new Point(160, 16);
        btnListen.Name = "btnListen";
        btnListen.Size = new Size(94, 29);
        btnListen.Text = "Listen";

        txtReceived.Location = new Point(10, 60);
        txtReceived.Multiline = true;
        txtReceived.Name = "txtReceived";
        txtReceived.ReadOnly = true;
        txtReceived.ScrollBars = ScrollBars.Vertical;
        txtReceived.Size = new Size(770, 340);

        // tabPage2 (Send)
        tabPage2.Controls.Add(txtMessage);
        tabPage2.Controls.Add(btnSend);
        tabPage2.Controls.Add(txtSendPort);
        tabPage2.Controls.Add(txtHost);
        tabPage2.Controls.Add(label3);
        tabPage2.Controls.Add(label2);
        tabPage2.Location = new Point(4, 29);
        tabPage2.Name = "tabPage2";
        tabPage2.Size = new Size(792, 417);
        tabPage2.TabIndex = 1;
        tabPage2.Text = "Send";

        // Send controls
        label2.AutoSize = true;
        label2.Location = new Point(10, 20);
        label2.Name = "label2";
        label2.Size = new Size(40, 20);
        label2.Text = "Host:";

        label3.AutoSize = true;
        label3.Location = new Point(220, 20);
        label3.Name = "label3";
        label3.Size = new Size(35, 20);
        label3.Text = "Port:";

        txtHost.Location = new Point(60, 17);
        txtHost.Name = "txtHost";
        txtHost.Size = new Size(150, 27);

        txtSendPort.Location = new Point(260, 17);
        txtSendPort.Name = "txtSendPort";
        txtSendPort.Size = new Size(100, 27);

        btnSend.Location = new Point(370, 16);
        btnSend.Name = "btnSend";
        btnSend.Size = new Size(94, 29);
        btnSend.Text = "Send";

        txtMessage.Location = new Point(10, 60);
        txtMessage.Multiline = true;
        txtMessage.Name = "txtMessage";
        txtMessage.ScrollBars = ScrollBars.Vertical;
        txtMessage.Size = new Size(770, 340);

        // Form1
        AutoScaleDimensions = new SizeF(8F, 20F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(800, 450);
        Controls.Add(tabControl1);
        Name = "Form1";
        Text = "TCP Messenger";

        tabControl1.ResumeLayout(false);
        tabPage1.ResumeLayout(false);
        tabPage1.PerformLayout();
        tabPage2.ResumeLayout(false);
        tabPage2.PerformLayout();
        ResumeLayout(false);
    }

    private TabControl tabControl1;
    private TabPage tabPage1;
    private TabPage tabPage2;
    private TextBox txtReceived;
    private Button btnListen;
    private TextBox txtListenPort;
    private Label label1;
    private TextBox txtMessage;
    private Button btnSend;
    private TextBox txtSendPort;
    private TextBox txtHost;
    private Label label3;
    private Label label2;

    #endregion
}
