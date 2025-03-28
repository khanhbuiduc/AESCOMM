namespace AES2;

partial class HistoryForm
{
    private System.ComponentModel.IContainer components = null;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        this.txtHistory = new TextBox();
        this.btnOpenFolder = new Button();
        this.btnClearHistory = new Button();
        this.panelBottom = new Panel();

        // Panel Bottom
        this.panelBottom.SuspendLayout();
        this.SuspendLayout();

        // txtHistory
        this.txtHistory.Dock = DockStyle.Fill;
        this.txtHistory.Font = new Font("Consolas", 10F);
        this.txtHistory.Location = new Point(10, 10);
        this.txtHistory.Multiline = true;
        this.txtHistory.Name = "txtHistory";
        this.txtHistory.ReadOnly = true;
        this.txtHistory.ScrollBars = ScrollBars.Both;
        this.txtHistory.Size = new Size(780, 440);
        this.txtHistory.TabIndex = 0;

        // btnOpenFolder
        this.btnOpenFolder.Dock = DockStyle.Right;
        this.btnOpenFolder.Location = new Point(630, 5);
        this.btnOpenFolder.Name = "btnOpenFolder";
        this.btnOpenFolder.Size = new Size(150, 30);
        this.btnOpenFolder.TabIndex = 1;
        this.btnOpenFolder.Text = "Open Downloads Folder";
        this.btnOpenFolder.UseVisualStyleBackColor = true;
        this.btnOpenFolder.Click += new EventHandler(BtnOpenFolder_Click);

        // btnClearHistory
        this.btnClearHistory.Text = "Clear History";
        this.btnClearHistory.Size = new Size(100, 30);
        this.btnClearHistory.Location = new Point(5, 5);
        this.btnClearHistory.Dock = DockStyle.Left;
        this.btnClearHistory.Click += new EventHandler(BtnClearHistory_Click);

        // panelBottom
        this.panelBottom.Controls.Add(this.btnOpenFolder);
        this.panelBottom.Controls.Add(this.btnClearHistory);
        this.panelBottom.Dock = DockStyle.Bottom;
        this.panelBottom.Height = 40;
        this.panelBottom.Padding = new Padding(5);

        // HistoryForm
        this.AutoScaleDimensions = new SizeF(7F, 15F);
        this.AutoScaleMode = AutoScaleMode.Font;
        this.ClientSize = new Size(800, 500);
        this.Controls.Add(this.txtHistory);
        this.Controls.Add(this.panelBottom);
        this.Name = "HistoryForm";
        this.StartPosition = FormStartPosition.CenterParent;
        this.Text = "Download History";

        this.panelBottom.ResumeLayout(false);
        this.ResumeLayout(false);
        this.PerformLayout();
    }

    private TextBox txtHistory;
    private Button btnOpenFolder;
    private Button btnClearHistory;
    private Panel panelBottom;
}
