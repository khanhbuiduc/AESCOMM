using System.Net;
using System.Net.Sockets;
using System.Text;

namespace AES2;

public partial class Form1 : Form
{
    private const int DEFAULT_PORT = 5001;
    private TcpListener? tcpListener;
    private bool isListening = false;
    private CancellationTokenSource? cts;
    private uint[] encryptionKey; // Store encryption key
    private readonly string downloadPath;
    private string selectedFilePath = string.Empty;
    private string randomName;

    public Form1()
    {
        InitializeComponent();
        btnSend.Click += BtnSend_Click;
        lblDeviceName.Click += LblDeviceName_Click;

        // Load saved name or generate a new one
        LoadSavedName();

        // Load logo
        try
        {
            string logoPath = Path.Combine(Application.StartupPath, "logo.png");
            if (File.Exists(logoPath))
            {
                logoPictureBox.Image = Image.FromFile(logoPath);
            }
        }
        catch
        {
            // If logo loading fails, continue without image
        }

        // Generate random key for AES-256
        encryptionKey = new uint[]
        {
            0x00010203, 0x04050607, 0x08090a0b, 0x0c0d0e0f,
            0x10111213, 0x14151617, 0x18191a1b, 0x1c1d1e1f
        };

        // Create download directory and start listening
        downloadPath = Path.Combine(Application.StartupPath, "Downloads");
        Directory.CreateDirectory(downloadPath);
        StartListening();
    }

    private void GenerateRandomName()
    {
        try
        {
            string jsonPath = Path.Combine(Application.StartupPath, "name_random.json");
            string nameConfigPath = Path.Combine(Application.StartupPath, "saved_name.txt");

            // Read and parse JSON
            string jsonContent = File.ReadAllText(jsonPath);
            var jsonDoc = System.Text.Json.JsonDocument.Parse(jsonContent);
            var adjectives = jsonDoc.RootElement.GetProperty("adjectives").EnumerateArray()
                                  .Select(e => e.GetString()).ToList();
            var nouns = jsonDoc.RootElement.GetProperty("nouns").EnumerateArray()
                             .Select(e => e.GetString()).ToList();

            // Generate random name
            var random = new Random();
            string adj = adjectives[random.Next(adjectives.Count)];
            string noun = nouns[random.Next(nouns.Count)];
            randomName = $"{char.ToUpper(adj[0])}{adj.Substring(1)} {char.ToUpper(noun[0])}{noun.Substring(1)}";

            // Save name
            File.WriteAllText(nameConfigPath, randomName);
            lblDeviceName.Text = randomName;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error generating name: {ex.Message}");
            randomName = "UnnamedDevice";
            lblDeviceName.Text = randomName;
        }
    }

    private void LoadSavedName()
    {
        try
        {
            string nameConfigPath = Path.Combine(Application.StartupPath, "saved_name.txt");
            if (File.Exists(nameConfigPath))
            {
                randomName = File.ReadAllText(nameConfigPath);
                lblDeviceName.Text = randomName;
            }
            else
            {
                GenerateRandomName();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading name: {ex.Message}");
            randomName = "UnnamedDevice";
            lblDeviceName.Text = randomName;
        }
    }

    private async void StartListening()
    {
        try
        {
            tcpListener = new TcpListener(IPAddress.Any, DEFAULT_PORT);
            tcpListener.Start();
            isListening = true;
            cts = new CancellationTokenSource();

            while (!cts.Token.IsCancellationRequested && tcpListener != null)
            {
                TcpClient client = await tcpListener.AcceptTcpClientAsync();
                _ = Task.Run(async () =>
                {
                    using (client)
                    using (NetworkStream stream = client.GetStream())
                    {
                        // Send the device name to the connecting client
                        byte[] nameBytes = Encoding.UTF8.GetBytes(randomName);
                        await stream.WriteAsync(nameBytes, 0, nameBytes.Length);

                        // Handle further communication if needed
                        await HandleClientAsync(client);
                    }
                });
            }
        }
        catch (Exception ex)
        {
            if (!cts?.Token.IsCancellationRequested ?? true)
            {
                MessageBox.Show($"Error in listener: {ex.Message}");
            }
        }
    }

    private void BtnSelectFile_Click(object? sender, EventArgs e)
    {
        using (OpenFileDialog openFileDialog = new OpenFileDialog())
        {
            openFileDialog.Filter = "All files (*.*)|*.*";
            openFileDialog.FilterIndex = 1;

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                selectedFilePath = openFileDialog.FileName;
                txtMessage.Text = Path.GetFileName(selectedFilePath); // Simplified display
                txtMessage.ForeColor = Color.Blue; // Optional: to indicate it's a file
            }
        }
    }

    private async Task ListenForClientsAsync(CancellationToken token)
    {
        try
        {
            while (!token.IsCancellationRequested && tcpListener != null)
            {
                TcpClient client = await tcpListener.AcceptTcpClientAsync();
                _ = HandleClientAsync(client);
            }
        }
        catch (OperationCanceledException)
        {
            // Normal cancellation
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Listening error: {ex.Message}");
        }
    }

    private string GetUniqueFilePath(string basePath)
    {
        if (!File.Exists(basePath))
            return basePath;

        string folder = Path.GetDirectoryName(basePath);
        string fileName = Path.GetFileNameWithoutExtension(basePath);
        string extension = Path.GetExtension(basePath);
        int counter = 1;

        string newPath;
        do
        {
            newPath = Path.Combine(folder, $"{fileName}({counter}){extension}");
            counter++;
        } while (File.Exists(newPath));

        return newPath;
    }

    private async Task HandleClientAsync(TcpClient client)
    {
        try
        {
            using (client)
            using (NetworkStream stream = client.GetStream())
            {
                // Read total size first
                byte[] sizeBuffer = new byte[4];
                await stream.ReadAsync(sizeBuffer, 0, 4);
                int totalSize = BitConverter.ToInt32(sizeBuffer);

                // Validate size
                if (totalSize <= 0)
                    return;

                // Read data
                byte[] buffer = new byte[totalSize];
                int bytesRead = 0;
                while (bytesRead < totalSize)
                {
                    int read = await stream.ReadAsync(buffer, bytesRead, totalSize - bytesRead);
                    if (read == 0) break;
                    bytesRead += read;
                }

                // Validate read size
                if (bytesRead < totalSize)
                    return;

                // Check if it's a file
                bool isFile = buffer[0] == 1;

                if (isFile)
                {
                    // Read file name length and file name
                    int fileNameLength = BitConverter.ToInt32(buffer, 1);
                    string fileName = Encoding.UTF8.GetString(buffer, 5, fileNameLength);

                    // Get encrypted blocks
                    int encryptedDataStart = 5 + fileNameLength;
                    int encryptedDataLength = totalSize - encryptedDataStart;
                    uint[][] encryptedBlocks = new uint[encryptedDataLength / 16][];

                    for (int i = 0; i < encryptedBlocks.Length; i++)
                    {
                        encryptedBlocks[i] = new uint[4];
                        Buffer.BlockCopy(buffer, encryptedDataStart + (i * 16), encryptedBlocks[i], 0, 16);
                    }

                    // Decrypt data
                    string base64Content = Aes256Helper.DecryptCBC(encryptedBlocks, encryptionKey);
                    byte[] fileContent = Convert.FromBase64String(base64Content);

                    // Create encrypted and decrypted folders
                    string encryptedPath = Path.Combine(downloadPath, "Encrypted");
                    string decryptedPath = Path.Combine(downloadPath, "Decrypted");
                    Directory.CreateDirectory(encryptedPath);
                    Directory.CreateDirectory(decryptedPath);

                    // Save encrypted file
                    string encryptedFileName = $"{fileName}.encrypted";
                    string encryptedFilePath = Path.Combine(encryptedPath, encryptedFileName);
                    encryptedFilePath = GetUniqueFilePath(encryptedFilePath);
                    await File.WriteAllBytesAsync(encryptedFilePath, buffer);

                    // Save decrypted file
                    string decryptedFilePath = Path.Combine(decryptedPath, fileName);
                    decryptedFilePath = GetUniqueFilePath(decryptedFilePath);
                    await File.WriteAllBytesAsync(decryptedFilePath, fileContent);

                    // Add to download history with both paths
                    string historyEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | {fileName} | Encrypted: {encryptedFilePath} | Decrypted: {decryptedFilePath}";
                    string historyPath = Path.Combine(Application.StartupPath, "DownloadHistory.txt");
                    await File.AppendAllTextAsync(historyPath, historyEntry + Environment.NewLine);

                    this.Invoke(() =>
                    {
                        var result = MessageBox.Show(
                            $"File saved:\nEncrypted: {encryptedFilePath}\nDecrypted: {decryptedFilePath}\n\nDo you want to open the decrypted file?",
                            "File Received",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Information);

                        if (result == DialogResult.Yes)
                        {
                            try
                            {
                                var startInfo = new System.Diagnostics.ProcessStartInfo
                                {
                                    FileName = decryptedFilePath,
                                    UseShellExecute = true
                                };
                                System.Diagnostics.Process.Start(startInfo);
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show($"Error opening file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    });
                }
                else
                {
                    // Validate block size for message
                    if (totalSize < 17) // At least 1 byte flag + 16 bytes data
                        return;

                    // Calculate number of blocks (excluding the flag byte)
                    int numBlocks = (totalSize - 1) / 16;
                    uint[][] encryptedBlocks = new uint[numBlocks][];

                    for (int i = 0; i < numBlocks; i++)
                    {
                        encryptedBlocks[i] = new uint[4];
                        Buffer.BlockCopy(buffer, 1 + (i * 16), encryptedBlocks[i], 0, 16);
                    }

                    string message = Aes256Helper.DecryptCBC(encryptedBlocks, encryptionKey);

                    this.Invoke(() =>
                    {
                        MessageBox.Show($"Message received: {message}", "Message Received", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    });
                }
            }
        }
        catch (Exception ex)
        {
            this.Invoke(() =>
            {
                MessageBox.Show($"Error handling received data: {ex.Message}");
            });
        }
    }

    private void StopListening()
    {
        isListening = false;
        cts?.Cancel();
        tcpListener?.Stop();
        tcpListener = null;
    }

    private string GetUniqueFileName(string originalFileName)
    {
        string directory = Path.GetDirectoryName(originalFileName);
        string fileName = Path.GetFileNameWithoutExtension(originalFileName);
        string extension = Path.GetExtension(originalFileName);
        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");

        return Path.Combine(directory, $"{fileName}_{timestamp}{extension}");
    }

    private async void BtnSend_Click(object? sender, EventArgs e)
    {
        try
        {
            string host = txtHost.Text;
            if (string.IsNullOrEmpty(host))
            {
                MessageBox.Show("Please select a device from the list first");
                return;
            }

            if (!int.TryParse(txtPort.Text, out int port))
            {
                MessageBox.Show("Invalid port number");
                return;
            }

            string message;
            byte[] fileNameBytes = Array.Empty<byte>();
            byte[] fileContentBytes = Array.Empty<byte>();

            if (!string.IsNullOrEmpty(selectedFilePath) && File.Exists(selectedFilePath))
            {
                // Create unique file name with timestamp
                string uniqueFileName = GetUniqueFileName(selectedFilePath);
                string fileName = Path.GetFileName(uniqueFileName);
                fileNameBytes = Encoding.UTF8.GetBytes(fileName);
                fileContentBytes = await File.ReadAllBytesAsync(selectedFilePath);
                message = Convert.ToBase64String(fileContentBytes);
            }
            else
            {
                message = txtMessage.Text;
            }

            // Generate IV and encrypt using CBC mode
            uint[] iv = Aes256Helper.GenerateIV();
            uint[][] encryptedBlocks = Aes256Helper.EncryptCBC(message, encryptionKey, iv);

            using (TcpClient client = new TcpClient())
            {
                await client.ConnectAsync(host, port);
                using (NetworkStream stream = client.GetStream())
                {
                    // Prepare the full message
                    byte[] dataToSend;
                    if (!string.IsNullOrEmpty(selectedFilePath))
                    {
                        // Format: [IsFile(1)][FileNameLength(4)][FileName(var)][EncryptedData]
                        int totalLength = 1 + 4 + fileNameBytes.Length + (encryptedBlocks.Length * 16);
                        dataToSend = new byte[4 + totalLength]; // Total length prefix

                        using (var ms = new MemoryStream(dataToSend))
                        using (var writer = new BinaryWriter(ms))
                        {
                            writer.Write(totalLength);
                            writer.Write((byte)1); // IsFile flag
                            writer.Write(fileNameBytes.Length);
                            writer.Write(fileNameBytes);

                            // Write encrypted blocks
                            for (int i = 0; i < encryptedBlocks.Length; i++)
                            {
                                byte[] blockBytes = new byte[16];
                                Buffer.BlockCopy(encryptedBlocks[i], 0, blockBytes, 0, 16);
                                writer.Write(blockBytes);
                            }
                        }
                    }
                    else
                    {
                        // Format: [IsFile(1)][EncryptedData]
                        int totalLength = 1 + (encryptedBlocks.Length * 16);
                        dataToSend = new byte[4 + totalLength]; // Total length prefix

                        using (var ms = new MemoryStream(dataToSend))
                        using (var writer = new BinaryWriter(ms))
                        {
                            writer.Write(totalLength);
                            writer.Write((byte)0); // IsFile flag

                            // Write encrypted blocks
                            for (int i = 0; i < encryptedBlocks.Length; i++)
                            {
                                byte[] blockBytes = new byte[16];
                                Buffer.BlockCopy(encryptedBlocks[i], 0, blockBytes, 0, 16);
                                writer.Write(blockBytes);
                            }
                        }
                    }

                    await stream.WriteAsync(dataToSend);
                }
            }

            MessageBox.Show("Data sent successfully!");
            selectedFilePath = string.Empty; // Reset selected file
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error sending data: {ex.Message}");
        }
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        StopListening();
        base.OnFormClosing(e);
    }

    private async void BtnScan_Click(object? sender, EventArgs e)
    {
        deviceList.Items.Clear();
        deviceList.Items.Add("Scanning...");
        btnScan.Enabled = false;

        try
        {
            var hosts = await ScanNetworkAsync();
            deviceList.Items.Clear();

            // Store IP address in Tag property
            foreach (var (ip, name) in hosts)
            {
                var item = new ListViewItem(name);
                deviceList.Items.Add(new DeviceItem(name, ip));
            }

            if (deviceList.Items.Count == 0)
            {
                deviceList.Items.Add(new DeviceItem("No devices found", ""));
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Scan error: {ex.Message}");
        }
        finally
        {
            btnScan.Enabled = true;
        }
    }

    private void DeviceList_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (deviceList.SelectedItem is DeviceItem device)
        {
            txtHost.Text = device.IpAddress;
            txtPort.Text = DEFAULT_PORT.ToString();
        }
    }

    private async Task<List<(string ip, string name)>> ScanNetworkAsync()
    {
        var hosts = new List<(string ip, string name)>();
        var localIp = GetLocalIPAddress();
        var baseIp = localIp.Substring(0, localIp.LastIndexOf('.') + 1);
        var tasks = new List<Task>();

        for (int i = 1; i <= 255; i++)
        {
            var ip = baseIp + i;
            tasks.Add(CheckHostAsync(ip, hosts));
        }

        await Task.WhenAll(tasks);
        return hosts;
    }

    private async Task CheckHostAsync(string ip, List<(string ip, string name)> hosts)
    {
        try
        {
            using (var client = new TcpClient())
            {
                var connectTask = client.ConnectAsync(ip, DEFAULT_PORT);
                if (await Task.WhenAny(connectTask, Task.Delay(100)) == connectTask)
                {
                    if (client.Connected)
                    {
                        // Try to read device name from connected client
                        string deviceName = await GetDeviceNameAsync(ip);
                        lock (hosts)
                        {
                            hosts.Add((ip, deviceName));
                        }
                    }
                }
            }
        }
        catch
        {
            // Connection failed, ignore
        }
    }

    private async Task<string> GetDeviceNameAsync(string ip)
    {
        try
        {
            using (var client = new TcpClient())
            {
                await client.ConnectAsync(ip, DEFAULT_PORT);
                using (NetworkStream stream = client.GetStream())
                {
                    // Read the device name sent by the client
                    byte[] buffer = new byte[256];
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                    string deviceName = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    return string.IsNullOrWhiteSpace(deviceName) ? $"Device at {ip}" : deviceName;
                }
            }
        }
        catch
        {
            return $"Unknown Device ({ip})";
        }
    }

    private string GetLocalIPAddress()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            {
                return ip.ToString();
            }
        }
        throw new Exception("No network adapters found");
    }

    private void tabPage1_Click(object sender, EventArgs e)
    {

    }

    // Add method to view history
    private void ShowFileHistory()
    {
        try
        {
            string historyPath = Path.Combine(Application.StartupPath, "DownloadHistory.txt");
            if (File.Exists(historyPath))
            {
                string history = File.ReadAllText(historyPath);
                using (var historyForm = new HistoryForm(history))
                {
                    historyForm.ShowDialog(this);
                }
            }
            else
            {
                MessageBox.Show("No download history available", "History", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error reading history: {ex.Message}");
        }
    }

    private void LblHistory_Click(object? sender, EventArgs e)
    {
        ShowFileHistory();
    }

    private void LblDeviceName_Click(object? sender, EventArgs e)
    {
        var menu = new ContextMenuStrip();
        var changeNameItem = new ToolStripMenuItem("Change Name");
        var randomNameItem = new ToolStripMenuItem("Generate Random Name");

        changeNameItem.Click += (s, e) =>
        {
            using var inputForm = new Form
            {
                Width = 300,
                Height = 150,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = "Change Device Name",
                StartPosition = FormStartPosition.CenterParent
            };

            var textBox = new TextBox
            {
                Left = 10,
                Top = 10,
                Width = 260,
                Text = randomName
            };

            var button = new Button
            {
                Left = 10,
                Top = 50,
                Text = "Save",
                DialogResult = DialogResult.OK
            };

            inputForm.Controls.AddRange(new Control[] { textBox, button });

            if (inputForm.ShowDialog() == DialogResult.OK && !string.IsNullOrWhiteSpace(textBox.Text))
            {
                randomName = textBox.Text;
                lblDeviceName.Text = randomName;
                File.WriteAllText(Path.Combine(Application.StartupPath, "saved_name.txt"), randomName);
            }
        };

        randomNameItem.Click += (s, e) =>
        {
            GenerateRandomName();
        };

        menu.Items.Add(changeNameItem);
        menu.Items.Add(randomNameItem);

        menu.Show(lblDeviceName, new Point(0, lblDeviceName.Height));
    }

    private class DeviceItem
    {
        public string Name { get; }
        public string IpAddress { get; }

        public DeviceItem(string name, string ip)
        {
            Name = name;
            IpAddress = ip;
        }

        public override string ToString()
        {
            return Name;
        }
    }

    private void txtHost_TextChanged(object sender, EventArgs e)
    {

    }
}
