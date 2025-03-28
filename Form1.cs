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

    public Form1()
    {
        InitializeComponent();
        btnSend.Click += BtnSend_Click;

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
                _ = HandleClientAsync(client);
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

                    // Save file with unique name
                    string basePath = Path.Combine(downloadPath, fileName);
                    string savePath = GetUniqueFilePath(basePath);
                    await File.WriteAllBytesAsync(savePath, fileContent);

                    this.Invoke(() =>
                    {
                        txtReceived.AppendText($"{DateTime.Now}: Received and saved file: {savePath}{Environment.NewLine}");
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
                        txtReceived.AppendText($"{DateTime.Now}: {message}{Environment.NewLine}");
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
            if (deviceList.SelectedItem == null || deviceList.SelectedItem.ToString() == "No devices found" || deviceList.SelectedItem.ToString() == "Scanning...")
            {
                MessageBox.Show("Please select a device from the list first");
                return;
            }

            string host = deviceList.SelectedItem.ToString();
            int port = DEFAULT_PORT;

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
            foreach (var host in hosts)
            {
                deviceList.Items.Add(host);
            }

            if (deviceList.Items.Count == 0)
            {
                deviceList.Items.Add("No devices found");
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
        // No need to update text boxes anymore since they're removed
        // Just let the BtnSend_Click handle the selected device
    }

    private async Task<List<string>> ScanNetworkAsync()
    {
        var hosts = new List<string>();
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

    private async Task CheckHostAsync(string ip, List<string> hosts)
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
                        lock (hosts)
                        {
                            hosts.Add(ip);
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
}
