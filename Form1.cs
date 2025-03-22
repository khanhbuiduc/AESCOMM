using System.Net;
using System.Net.Sockets;
using System.Text;

namespace AES2;

public partial class Form1 : Form
{
    private TcpListener? tcpListener;
    private bool isListening = false;
    private CancellationTokenSource? cts;
    private uint[] encryptionKey; // Store encryption key
    private readonly string downloadPath;
    private string selectedFilePath = string.Empty;

    public Form1()
    {
        InitializeComponent();
        btnListen.Click += BtnListen_Click;
        btnSend.Click += BtnSend_Click;

        // Add button for file selection
        Button btnSelectFile = new Button
        {
            Text = "Select File",
            Location = new Point(470, 16),
            Size = new Size(94, 29)
        };
        btnSelectFile.Click += BtnSelectFile_Click;
        tabPage2.Controls.Add(btnSelectFile);

        // Generate random key for AES-256
        encryptionKey = new uint[]
        {
            0x00010203, 0x04050607, 0x08090a0b, 0x0c0d0e0f,
            0x10111213, 0x14151617, 0x18191a1b, 0x1c1d1e1f
        };

        // Create download directory
        downloadPath = Path.Combine(Application.StartupPath, "Downloads");
        Directory.CreateDirectory(downloadPath);
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
                txtMessage.Text = $"Selected file: {Path.GetFileName(selectedFilePath)}";
            }
        }
    }

    private async void BtnListen_Click(object? sender, EventArgs e)
    {
        if (!isListening)
        {
            try
            {
                int port = int.Parse(txtListenPort.Text);
                tcpListener = new TcpListener(IPAddress.Any, port);
                tcpListener.Start();
                isListening = true;
                btnListen.Text = "Stop";
                cts = new CancellationTokenSource();

                await ListenForClientsAsync(cts.Token);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error starting listener: {ex.Message}");
            }
        }
        else
        {
            StopListening();
            btnListen.Text = "Listen";
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

                // Read data
                byte[] buffer = new byte[totalSize];
                int bytesRead = 0;
                while (bytesRead < totalSize)
                {
                    int read = await stream.ReadAsync(buffer, bytesRead, totalSize - bytesRead);
                    if (read == 0) break;
                    bytesRead += read;
                }

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

                    // Save file
                    string savePath = Path.Combine(downloadPath, fileName);
                    await File.WriteAllBytesAsync(savePath, fileContent);

                    this.Invoke(() =>
                    {
                        txtReceived.AppendText($"{DateTime.Now}: Received and saved file: {savePath}{Environment.NewLine}");
                    });
                }
                else
                {
                    // Handle regular message
                    uint[][] encryptedBlocks = new uint[totalSize / 16][];
                    for (int i = 0; i < encryptedBlocks.Length; i++)
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

    private async void BtnSend_Click(object? sender, EventArgs e)
    {
        try
        {
            string host = txtHost.Text;
            int port = int.Parse(txtSendPort.Text);

            string message;
            byte[] fileNameBytes = Array.Empty<byte>();
            byte[] fileContentBytes = Array.Empty<byte>();

            if (!string.IsNullOrEmpty(selectedFilePath) && File.Exists(selectedFilePath))
            {
                // Prepare file data
                string fileName = Path.GetFileName(selectedFilePath);
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
}
