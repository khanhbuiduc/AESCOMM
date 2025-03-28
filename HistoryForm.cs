namespace AES2;

public partial class HistoryForm : Form
{
    private readonly string downloadPath;

    public HistoryForm(string history)
    {
        InitializeComponent();
        downloadPath = Path.Combine(Application.StartupPath, "Downloads");
        txtHistory.Text = history;
    }

    private void BtnOpenFolder_Click(object? sender, EventArgs e)
    {
        try
        {
            if (Directory.Exists(downloadPath))
            {
                System.Diagnostics.Process.Start("explorer.exe", downloadPath);
            }
            else
            {
                MessageBox.Show("Downloads folder not found", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error opening folder: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void BtnClearHistory_Click(object? sender, EventArgs e)
    {
        try
        {
            var result = MessageBox.Show(
                "Are you sure you want to clear history?",
                "Clear History",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                string historyPath = Path.Combine(Application.StartupPath, "DownloadHistory.txt");
                if (File.Exists(historyPath))
                {
                    File.WriteAllText(historyPath, string.Empty);
                    txtHistory.Clear();
                    MessageBox.Show("History cleared successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error clearing history: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
