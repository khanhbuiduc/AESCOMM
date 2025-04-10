using System.Net;
using System.Net.Sockets;
using System.Text;

namespace AES2;

public partial class Form1 : Form
{
    private const int DEFAULT_PORT = 5001;
    private const byte AES_TYPE_256 = 1;
    private const byte AES_TYPE_192 = 2;
    private const byte AES_TYPE_128 = 3;
    private TcpListener? tcpListener;
    private bool isListening = false;
    private CancellationTokenSource? cts;
    private uint[]? encryptionKey; // Changed to nullable since it will be set by user
    private readonly string downloadPath;
    private string selectedFilePath = string.Empty;
    private string randomName;
    private string selectedEncryptedFilePath = string.Empty;

    public Form1()
    {
        InitializeComponent();
        btnSend.Click += BtnSend_Click;
        lblDeviceName.Click += LblDeviceName_Click;
        btnDecrypt.Click += BtnDecrypt_Click;
        btnSelectEncrypted.Click += BtnSelectEncrypted_Click;

        // Add event handlers for radio buttons
        radioAes256.CheckedChanged += RadioAesType_CheckedChanged;
        radioAes192.CheckedChanged += RadioAesType_CheckedChanged;
        radioAes128.CheckedChanged += RadioAesType_CheckedChanged;

        // Set default placeholder text for AES-256
        txtEncryptionKey.PlaceholderText = "Enter 64 hex characters (256-bit key)";

        // Load saved name or generate a new one
        LoadSavedName();

        // // Load logo
        // try
        // {
        //     string logoPath = Path.Combine(Application.StartupPath, "logo.png");
        //     if (File.Exists(logoPath))
        //     {
        //         logoPictureBox.Image = Image.FromFile(logoPath);
        //     }
        // }
        // catch
        // {
        //     // If logo loading fails, continue without image
        // }

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

            // Check if it's a file and get AES type
            bool isFile = buffer[0] == 1;
            byte aesType = buffer[1];

            if (isFile)
            {
                // Read file name length and file name
                int fileNameLength = BitConverter.ToInt32(buffer, 2);
                string fileName = Encoding.UTF8.GetString(buffer, 6, fileNameLength);

                // Extract encrypted blocks
                int encryptedDataStart = 6 + fileNameLength;
                int encryptedDataLength = totalSize - encryptedDataStart;

                // Create Base64 representation of the encrypted data
                byte[] encryptedPortion = new byte[encryptedDataLength];
                Buffer.BlockCopy(buffer, encryptedDataStart, encryptedPortion, 0, encryptedDataLength);
                string base64EncryptedData = Convert.ToBase64String(encryptedPortion);

                // Create encrypted folder and save encrypted file as Base64
                string encryptedPath = Path.Combine(downloadPath, "Encrypted");
                Directory.CreateDirectory(encryptedPath);
                string encryptedFileName = $"{fileName}";
                string encryptedFilePath = Path.Combine(encryptedPath, encryptedFileName);
                encryptedFilePath = GetUniqueFilePath(encryptedFilePath);

                // Save file metadata and Base64 content
                using (var writer = new StreamWriter(encryptedFilePath))
                {
                    // Write file information as a small header
                    writer.WriteLine($"FILENAME:{fileName}");
                    writer.WriteLine($"TIMESTAMP:{DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                    writer.WriteLine($"ENCRYPTED_SIZE:{encryptedDataLength}");
                    writer.WriteLine($"AES_TYPE:{aesType}");
                    writer.WriteLine("---BEGIN ENCRYPTED DATA---");
                    writer.WriteLine(base64EncryptedData);
                    writer.WriteLine("---END ENCRYPTED DATA---");
                }

                // Add to history
                string historyEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | {fileName} | Encrypted: {encryptedFilePath}";
                string historyPath = Path.Combine(Application.StartupPath, "DownloadHistory.txt");
                await File.AppendAllTextAsync(historyPath, historyEntry + Environment.NewLine);

                // Ask user if they want to decrypt now
                await this.Invoke(async () =>
                {
                    var result = MessageBox.Show(
                        $"Encrypted file saved to: {encryptedFilePath}\n\nDo you want to decrypt it now?",
                        "File Received",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        // Prompt user for decryption key
                        using (var keyForm = new Form())
                        {
                            keyForm.Text = "Enter Decryption Key";
                            keyForm.Size = new Size(400, 150);
                            keyForm.FormBorderStyle = FormBorderStyle.FixedDialog;
                            keyForm.StartPosition = FormStartPosition.CenterParent;
                            keyForm.MaximizeBox = false;
                            keyForm.MinimizeBox = false;

                            var keyLabel = new Label() { Text = "Decryption Key:", AutoSize = true, Location = new Point(10, 20) };
                            var keyTextBox = new TextBox() { Width = 360, Location = new Point(10, 50) };

                            // Set appropriate placeholder text based on AES type
                            switch (aesType)
                            {
                                case AES_TYPE_256:
                                    keyTextBox.PlaceholderText = "Enter 64 hex characters (256-bit key)";
                                    break;
                                case AES_TYPE_192:
                                    keyTextBox.PlaceholderText = "Enter 48 hex characters (192-bit key)";
                                    break;
                                case AES_TYPE_128:
                                    keyTextBox.PlaceholderText = "Enter 32 hex characters (128-bit key)";
                                    break;
                                default:
                                    keyTextBox.PlaceholderText = "Enter encryption key in hex format";
                                    break;
                            }

                            var okButton = new Button() { Text = "Decrypt", DialogResult = DialogResult.OK, Location = new Point(200, 80) };
                            var cancelButton = new Button() { Text = "Cancel", DialogResult = DialogResult.Cancel, Location = new Point(280, 80) };

                            keyForm.Controls.AddRange(new Control[] { keyLabel, keyTextBox, okButton, cancelButton });
                            keyForm.AcceptButton = okButton;
                            keyForm.CancelButton = cancelButton;

                            if (keyForm.ShowDialog() == DialogResult.OK && !string.IsNullOrWhiteSpace(keyTextBox.Text))
                            {
                                try
                                {
                                    uint[] decryptionKey = ParseEncryptionKey(keyTextBox.Text);

                                    // Process the encrypted blocks from the saved Base64 format
                                    string base64Content = await ReadEncryptedBase64ContentAsync(encryptedFilePath);
                                    byte[] encryptedBytes = Convert.FromBase64String(base64Content);

                                    // Convert to blocks format
                                    int numBlocks = encryptedBytes.Length / 16;
                                    uint[][] encryptedBlocks = new uint[numBlocks][];

                                    for (int i = 0; i < numBlocks; i++)
                                    {
                                        encryptedBlocks[i] = new uint[4];
                                        for (int j = 0; j < 4; j++)
                                        {
                                            byte[] wordBytes = new byte[4];
                                            Buffer.BlockCopy(encryptedBytes, i * 16 + j * 4, wordBytes, 0, 4);
                                            encryptedBlocks[i][j] = BitConverter.ToUInt32(wordBytes, 0);
                                        }
                                    }

                                    // Decrypt data using the appropriate AES type
                                    string base64FileContent;
                                    switch (aesType)
                                    {
                                        case AES_TYPE_256:
                                            base64FileContent = AES256CBC.DecryptCBC(encryptedBlocks, decryptionKey);
                                            break;
                                        case AES_TYPE_192:
                                            base64FileContent = Aes192Helper.DecryptCBC(encryptedBlocks, decryptionKey);
                                            break;
                                        case AES_TYPE_128:
                                            base64FileContent = Aes128Helper.DecryptCBC(encryptedBlocks, decryptionKey);
                                            break;
                                        default:
                                            base64FileContent = AES256CBC.DecryptCBC(encryptedBlocks, decryptionKey);
                                            break;
                                    }

                                    byte[] fileContent = Convert.FromBase64String(base64FileContent);

                                    // Save decrypted file
                                    string decryptedPath = Path.Combine(downloadPath, "Decrypted");
                                    Directory.CreateDirectory(decryptedPath);
                                    string decryptedFilePath = Path.Combine(decryptedPath, fileName);
                                    decryptedFilePath = GetUniqueFilePath(decryptedFilePath);
                                    await File.WriteAllBytesAsync(decryptedFilePath, fileContent);

                                    // Update history
                                    historyEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | {fileName} | Encrypted: {encryptedFilePath} | Decrypted: {decryptedFilePath}";
                                    await File.AppendAllTextAsync(historyPath, historyEntry + Environment.NewLine);

                                    var openResult = MessageBox.Show(
                                        $"File decrypted successfully and saved to:\n{decryptedFilePath}\n\nDo you want to open the file?",
                                        "Decryption Successful",
                                        MessageBoxButtons.YesNo,
                                        MessageBoxIcon.Information);

                                    if (openResult == DialogResult.Yes)
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
                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show($"Decryption failed: {ex.Message}", "Decryption Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }
                            }
                        }
                    }
                });
            }
            else
            {
                // For messages, prompt for the decryption key
                this.Invoke(() =>
                {
                    using (var keyForm = new Form())
                    {
                        keyForm.Text = "Enter Decryption Key";
                        keyForm.Size = new Size(400, 150);
                        keyForm.FormBorderStyle = FormBorderStyle.FixedDialog;
                        keyForm.StartPosition = FormStartPosition.CenterParent;
                        keyForm.MaximizeBox = false;
                        keyForm.MinimizeBox = false;

                        var keyLabel = new Label() { Text = "Decryption Key:", AutoSize = true, Location = new Point(10, 20) };
                        var keyTextBox = new TextBox() { Width = 360, Location = new Point(10, 50) };

                        // Set appropriate placeholder text based on AES type
                        switch (aesType)
                        {
                            case AES_TYPE_256:
                                keyTextBox.PlaceholderText = "Enter 64 hex characters (256-bit key)";
                                break;
                            case AES_TYPE_192:
                                keyTextBox.PlaceholderText = "Enter 48 hex characters (192-bit key)";
                                break;
                            case AES_TYPE_128:
                                keyTextBox.PlaceholderText = "Enter 32 hex characters (128-bit key)";
                                break;
                            default:
                                keyTextBox.PlaceholderText = "Enter encryption key in hex format";
                                break;
                        }

                        var okButton = new Button() { Text = "Decrypt", DialogResult = DialogResult.OK, Location = new Point(200, 80) };
                        var cancelButton = new Button() { Text = "Cancel", DialogResult = DialogResult.Cancel, Location = new Point(280, 80) };

                        keyForm.Controls.AddRange(new Control[] { keyLabel, keyTextBox, okButton, cancelButton });
                        keyForm.AcceptButton = okButton;
                        keyForm.CancelButton = cancelButton;

                        if (keyForm.ShowDialog() == DialogResult.OK && !string.IsNullOrWhiteSpace(keyTextBox.Text))
                        {
                            try
                            {
                                uint[] decryptionKey = ParseEncryptionKey(keyTextBox.Text);

                                // Calculate number of blocks (excluding the flags)
                                int numBlocks = (totalSize - 2) / 16;
                                uint[][] encryptedBlocks = new uint[numBlocks][];

                                for (int i = 0; i < numBlocks; i++)
                                {
                                    encryptedBlocks[i] = new uint[4];
                                    for (int j = 0; j < 4; j++)
                                    {
                                        byte[] wordBytes = new byte[4];
                                        Buffer.BlockCopy(buffer, 2 + (i * 16) + (j * 4), wordBytes, 0, 4);
                                        encryptedBlocks[i][j] = BitConverter.ToUInt32(wordBytes, 0);
                                    }
                                }

                                // Decrypt message using the appropriate AES type
                                string message;
                                switch (aesType)
                                {
                                    case AES_TYPE_256:
                                        message = AES256CBC.DecryptCBC(encryptedBlocks, decryptionKey);
                                        break;
                                    case AES_TYPE_192:
                                        message = Aes192Helper.DecryptCBC(encryptedBlocks, decryptionKey);
                                        break;
                                    case AES_TYPE_128:
                                        message = Aes128Helper.DecryptCBC(encryptedBlocks, decryptionKey);
                                        break;
                                    default:
                                        message = AES256CBC.DecryptCBC(encryptedBlocks, decryptionKey);
                                        break;
                                }
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show($"Decryption failed: {ex.Message}", "Decryption Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                });
            }
        }
    }

    private async Task<string> ReadEncryptedBase64ContentAsync(string filePath)
    {
        string content = await File.ReadAllTextAsync(filePath);

        // Extract the Base64 content between the markers
        int startMarker = content.IndexOf("---BEGIN ENCRYPTED DATA---");
        int endMarker = content.IndexOf("---END ENCRYPTED DATA---");

        if (startMarker == -1 || endMarker == -1 || endMarker <= startMarker)
            throw new FormatException("Invalid encrypted file format");

        // Extract the Base64 data (trim whitespace and newlines)
        string base64Data = content.Substring(
            startMarker + "---BEGIN ENCRYPTED DATA---".Length,
            endMarker - (startMarker + "---BEGIN ENCRYPTED DATA---".Length));

        return base64Data.Trim();
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

            // Get encryption key from user input
            if (string.IsNullOrWhiteSpace(txtEncryptionKey.Text))
            {
                MessageBox.Show("Please enter an encryption key");
                return;
            }

            // Parse the encryption key from hexadecimal input
            uint[] encryptionKey;
            try
            {
                // Use the extended version that supports different key lengths
                encryptionKey = ParseEncryptionKey(txtEncryptionKey.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Invalid encryption key format: {ex.Message}\nPlease use a valid key (64, 48, or 32 hex characters depending on AES type).");
                return;
            }

            // Determine AES type and validate key length
            byte aesType;
            int requiredKeyLength = 0;  // In uint entries (4 bytes each)
            string requiredHexLength = "";

            if (radioAes256.Checked)
            {
                aesType = AES_TYPE_256;
                requiredKeyLength = 8;  // 256 bits = 8 uints
                requiredHexLength = "64";
            }
            else if (radioAes192.Checked)
            {
                aesType = AES_TYPE_192;
                requiredKeyLength = 6;  // 192 bits = 6 uints
                requiredHexLength = "48";
            }
            else // AES-128
            {
                aesType = AES_TYPE_128;
                requiredKeyLength = 4;  // 128 bits = 4 uints
                requiredHexLength = "32";
            }

            // Validate key length against selected AES type
            if (encryptionKey.Length != requiredKeyLength)
            {
                MessageBox.Show($"Key length mismatch. Selected AES type requires {requiredHexLength} hex characters.");
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

            // Generate IV and encrypt using CBC mode based on selected AES type
            uint[] iv = AES256CBC.GenerateIV(); // IV is always 128-bit
            uint[][] encryptedBlocks;

            switch (aesType)
            {
                case AES_TYPE_256:
                    encryptedBlocks = AES256CBC.EncryptCBC(message, encryptionKey, iv);
                    break;
                case AES_TYPE_192:
                    encryptedBlocks = Aes192Helper.EncryptCBC(message, encryptionKey, iv);
                    break;
                case AES_TYPE_128:
                    encryptedBlocks = Aes128Helper.EncryptCBC(message, encryptionKey, iv);
                    break;
                default:
                    encryptedBlocks = AES256CBC.EncryptCBC(message, encryptionKey, iv);
                    break;
            }

            using (TcpClient client = new TcpClient())
            {
                await client.ConnectAsync(host, port);
                using (NetworkStream stream = client.GetStream())
                {
                    // Prepare the full message
                    byte[] dataToSend;
                    if (!string.IsNullOrEmpty(selectedFilePath))
                    {
                        // Format: [IsFile(1)][AesType(1)][FileNameLength(4)][FileName(var)][EncryptedData]
                        int totalLength = 2 + 4 + fileNameBytes.Length + (encryptedBlocks.Length * 16);
                        dataToSend = new byte[4 + totalLength]; // Total length prefix

                        using (var ms = new MemoryStream(dataToSend))
                        using (var writer = new BinaryWriter(ms))
                        {
                            writer.Write(totalLength);
                            writer.Write((byte)1); // IsFile flag
                            writer.Write(aesType); // AES type
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
                        // Format: [IsFile(0)][AesType(1)][EncryptedData]
                        int totalLength = 2 + (encryptedBlocks.Length * 16);
                        dataToSend = new byte[4 + totalLength]; // Total length prefix

                        using (var ms = new MemoryStream(dataToSend))
                        using (var writer = new BinaryWriter(ms))
                        {
                            writer.Write(totalLength);
                            writer.Write((byte)0); // IsFile flag
                            writer.Write(aesType); // AES type

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

    // Helper method to parse encryption key from hexadecimal string
    private uint[] ParseEncryptionKey(string keyText)
    {
        // Remove any spaces and ensure lowercase for consistency
        keyText = keyText.Replace(" ", "").ToLower();

        // Check if it's a valid hex string
        if (!System.Text.RegularExpressions.Regex.IsMatch(keyText, "^[0-9a-f]+$"))
            throw new ArgumentException("Key must contain only hexadecimal characters (0-9, a-f)");

        // Determine key size based on input length
        int keySize;
        if (keyText.Length == 64) // 256-bit key (64 hex chars)
            keySize = 8;  // 8 uint values
        else if (keyText.Length == 48) // 192-bit key (48 hex chars)
            keySize = 6;  // 6 uint values
        else if (keyText.Length == 32) // 128-bit key (32 hex chars)
            keySize = 4;  // 4 uint values
        else
            throw new ArgumentException($"Key must be 32, 48, or 64 hexadecimal characters (128, 192, or 256 bits), got {keyText.Length}");

        uint[] key = new uint[keySize];

        for (int i = 0; i < keySize; i++)
        {
            string chunk = keyText.Substring(i * 8, 8);
            key[i] = Convert.ToUInt32(chunk, 16);
        }

        return key;
    }

    // New method to handle decryption of selected file
    private async void BtnDecrypt_Click(object? sender, EventArgs e)
    {
        try
        {
            if (string.IsNullOrEmpty(selectedEncryptedFilePath))
            {
                MessageBox.Show("Please select an encrypted file first");
                return;
            }

            if (string.IsNullOrWhiteSpace(txtDecryptionKey.Text))
            {
                MessageBox.Show("Please enter a decryption key");
                return;
            }

            // Parse the decryption key from hexadecimal input
            uint[] decryptionKey;
            try
            {
                decryptionKey = ParseEncryptionKey(txtDecryptionKey.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Invalid key format: {ex.Message}\nPlease use a valid 256-bit key (64 hex characters).");
                return;
            }

            // Check if this is our Base64 text format by reading first few bytes
            bool isTextFormat = IsTextFile(selectedEncryptedFilePath);

            if (isTextFormat)
            {
                // Handle our text-based encrypted file format
                try
                {
                    // Get file content and extract the relevant information
                    string fileContent = await File.ReadAllTextAsync(selectedEncryptedFilePath);
                    string fileName = ExtractHeaderValue(fileContent, "FILENAME:");
                    byte aesType = byte.Parse(ExtractHeaderValue(fileContent, "AES_TYPE:"));

                    if (string.IsNullOrWhiteSpace(fileName))
                    {
                        fileName = "decrypted_file"; // Default name if not found in header
                    }

                    // Extract and decode the Base64 encrypted content
                    string base64Content = await ReadEncryptedBase64ContentAsync(selectedEncryptedFilePath);
                    byte[] encryptedBytes = Convert.FromBase64String(base64Content);

                    // Convert to AES block format for decryption
                    int numBlocks = encryptedBytes.Length / 16;
                    uint[][] encryptedBlocks = new uint[numBlocks][];

                    for (int i = 0; i < numBlocks; i++)
                    {
                        encryptedBlocks[i] = new uint[4];
                        Buffer.BlockCopy(encryptedBytes, i * 16, encryptedBlocks[i], 0, 16);
                    }

                    // Decrypt the data
                    string base64FileContent;
                    switch (aesType)
                    {
                        case AES_TYPE_256:
                            base64FileContent = AES256CBC.DecryptCBC(encryptedBlocks, decryptionKey);
                            break;
                        case AES_TYPE_192:
                            base64FileContent = Aes192Helper.DecryptCBC(encryptedBlocks, decryptionKey);
                            break;
                        case AES_TYPE_128:
                            base64FileContent = Aes128Helper.DecryptCBC(encryptedBlocks, decryptionKey);
                            break;
                        default:
                            base64FileContent = AES256CBC.DecryptCBC(encryptedBlocks, decryptionKey);
                            break;
                    }

                    byte[] decryptedFileContent = Convert.FromBase64String(base64FileContent); // Renamed to avoid variable conflict

                    // Save the decrypted file
                    string decryptedPath = Path.Combine(downloadPath, "Decrypted");
                    Directory.CreateDirectory(decryptedPath);
                    string decryptedFilePath = Path.Combine(decryptedPath, fileName);
                    decryptedFilePath = GetUniqueFilePath(decryptedFilePath);
                    await File.WriteAllBytesAsync(decryptedFilePath, decryptedFileContent); // Use the renamed variable

                    var result = MessageBox.Show(
                        $"File decrypted successfully and saved to:\n{decryptedFilePath}\n\nDo you want to open the file?",
                        "Decryption Successful",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Information);

                    if (result == DialogResult.Yes)
                    {
                        var startInfo = new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = decryptedFilePath,
                            UseShellExecute = true
                        };
                        System.Diagnostics.Process.Start(startInfo);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Decryption failed: {ex.Message}\nThis could be due to an incorrect decryption key or corrupted file.",
                        "Decryption Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                // Handle legacy binary format for backward compatibility
                try
                {
                    // Read the encrypted file
                    byte[] encryptedData = File.ReadAllBytes(selectedEncryptedFilePath);

                    // Parse the file format
                    using (var ms = new MemoryStream(encryptedData))
                    using (var reader = new BinaryReader(ms))
                    {
                        int totalLength = reader.ReadInt32();
                        bool isFile = reader.ReadByte() == 1;
                        byte aesType = reader.ReadByte();

                        if (isFile)
                        {
                            // Read filename
                            int fileNameLength = reader.ReadInt32();
                            string fileName = Encoding.UTF8.GetString(reader.ReadBytes(fileNameLength));

                            // Get encrypted blocks
                            int encryptedDataLength = totalLength - (2 + 4 + fileNameLength);
                            int numBlocks = encryptedDataLength / 16;
                            uint[][] encryptedBlocks = new uint[numBlocks][];

                            for (int i = 0; i < numBlocks; i++)
                            {
                                encryptedBlocks[i] = new uint[4];
                                byte[] blockBytes = reader.ReadBytes(16);
                                Buffer.BlockCopy(blockBytes, 0, encryptedBlocks[i], 0, 16);
                            }

                            // Decrypt data
                            string base64Content;
                            switch (aesType)
                            {
                                case AES_TYPE_256:
                                    base64Content = AES256CBC.DecryptCBC(encryptedBlocks, decryptionKey);
                                    break;
                                case AES_TYPE_192:
                                    base64Content = Aes192Helper.DecryptCBC(encryptedBlocks, decryptionKey);
                                    break;
                                case AES_TYPE_128:
                                    base64Content = Aes128Helper.DecryptCBC(encryptedBlocks, decryptionKey);
                                    break;
                                default:
                                    base64Content = AES256CBC.DecryptCBC(encryptedBlocks, decryptionKey);
                                    break;
                            }

                            byte[] fileContent = Convert.FromBase64String(base64Content);

                            // Save decrypted file
                            string decryptedPath = Path.Combine(downloadPath, "Decrypted");
                            Directory.CreateDirectory(decryptedPath);
                            string decryptedFilePath = Path.Combine(decryptedPath, fileName);
                            decryptedFilePath = GetUniqueFilePath(decryptedFilePath);
                            File.WriteAllBytes(decryptedFilePath, fileContent);

                            var result = MessageBox.Show(
                                $"File decrypted successfully and saved to:\n{decryptedFilePath}\n\nDo you want to open the file?",
                                "Decryption Successful",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Information);

                            if (result == DialogResult.Yes)
                            {
                                var startInfo = new System.Diagnostics.ProcessStartInfo
                                {
                                    FileName = decryptedFilePath,
                                    UseShellExecute = true
                                };
                                System.Diagnostics.Process.Start(startInfo);
                            }
                        }
                        else
                        {
                            // Plain message decryption
                            int encryptedDataLength = totalLength - 2;
                            int numBlocks = encryptedDataLength / 16;
                            uint[][] encryptedBlocks = new uint[numBlocks][];

                            for (int i = 0; i < numBlocks; i++)
                            {
                                encryptedBlocks[i] = new uint[4];
                                byte[] blockBytes = reader.ReadBytes(16);
                                Buffer.BlockCopy(blockBytes, 0, encryptedBlocks[i], 0, 16);
                            }

                            string message;
                            switch (aesType)
                            {
                                case AES_TYPE_256:
                                    message = AES256CBC.DecryptCBC(encryptedBlocks, decryptionKey);
                                    break;
                                case AES_TYPE_192:
                                    message = Aes192Helper.DecryptCBC(encryptedBlocks, decryptionKey);
                                    break;
                                case AES_TYPE_128:
                                    message = Aes128Helper.DecryptCBC(encryptedBlocks, decryptionKey);
                                    break;
                                default:
                                    message = AES256CBC.DecryptCBC(encryptedBlocks, decryptionKey);
                                    break;
                            }

                            MessageBox.Show($"Decrypted message: {message}", "Message Decrypted", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Decryption failed: {ex.Message}\nThis could be due to an incorrect decryption key or invalid file format.",
                        "Decryption Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error during decryption: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    // Helper method to check if a file is a text file
    private bool IsTextFile(string filePath)
    {
        try
        {
            // Read the first few bytes to check if this is a text file
            using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                byte[] buffer = new byte[Math.Min(4096, (int)fileStream.Length)];
                fileStream.Read(buffer, 0, buffer.Length);

                // Check for text file markers - headers we added
                string content = Encoding.UTF8.GetString(buffer);
                return content.Contains("FILENAME:") &&
                       content.Contains("---BEGIN ENCRYPTED DATA---");
            }
        }
        catch
        {
            return false;
        }
    }

    // Helper method to extract values from file headers
    private string ExtractHeaderValue(string content, string header)
    {
        int startIndex = content.IndexOf(header);
        if (startIndex == -1) return string.Empty;

        startIndex += header.Length;
        int endIndex = content.IndexOf('\n', startIndex);
        if (endIndex == -1) return string.Empty;

        return content.Substring(startIndex, endIndex - startIndex).Trim();
    }

    private void BtnSelectEncrypted_Click(object? sender, EventArgs e)
    {
        using (OpenFileDialog openFileDialog = new OpenFileDialog())
        {
            openFileDialog.Filter = "Encrypted files (*.encrypted)|*.encrypted|All files (*.*)|*.*";
            openFileDialog.FilterIndex = 1;

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                selectedEncryptedFilePath = openFileDialog.FileName;
                lblSelectedFile.Text = Path.GetFileName(selectedEncryptedFilePath);
            }
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
            // Skip the local host
            string localIp = GetLocalIPAddress();
            if (ip == localIp)
                return;

            using (var client = new TcpClient())
            {
                var connectTask = client.ConnectAsync(ip, DEFAULT_PORT);
                if (await Task.WhenAny(connectTask, Task.Delay(1000)) == connectTask)
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
                // Add connection timeout
                var connectTask = client.ConnectAsync(ip, DEFAULT_PORT);
                if (await Task.WhenAny(connectTask, Task.Delay(1000)) != connectTask)
                {
                    throw new TimeoutException("Connection timeout");
                }

                using (NetworkStream stream = client.GetStream())
                {
                    // Set a read timeout to avoid hanging indefinitely
                    stream.ReadTimeout = 1000; // 1 second timeout

                    // Try to read the device name with timeout protection
                    var readTask = ReadWithTimeoutAsync(stream);
                    if (await Task.WhenAny(readTask, Task.Delay(1500)) != readTask)
                    {
                        return $"Device at {ip}"; // No response, just return the IP
                    }

                    string deviceName = await readTask;
                    return string.IsNullOrWhiteSpace(deviceName) ? $"Device at {ip}" : deviceName;
                }
            }
        }
        catch
        {
            // Any exception means we couldn't properly communicate
            return $"Device at {ip}";
        }
    }

    private async Task<string> ReadWithTimeoutAsync(NetworkStream stream)
    {
        try
        {
            byte[] buffer = new byte[256];
            int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
            if (bytesRead > 0)
            {
                return Encoding.UTF8.GetString(buffer, 0, bytesRead);
            }
            return string.Empty;
        }
        catch
        {
            return string.Empty;
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

    private void LblInformation_Click(object? sender, EventArgs e)
    {
        try
        {
            string localIp = GetLocalIPAddress();
            string info = $"Name: {randomName}\nHost: {localIp}\nPort: {DEFAULT_PORT}";
            MessageBox.Show(info, "Device Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error retrieving information: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
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

    // Add event handler for AES type radio buttons
    private void RadioAesType_CheckedChanged(object sender, EventArgs e)
    {
        if (sender is RadioButton rb && rb.Checked)
        {
            // Update placeholder text based on selected AES type
            if (rb == radioAes256)
            {
                txtEncryptionKey.PlaceholderText = "Enter 64 hex characters (256-bit key)";
            }
            else if (rb == radioAes192)
            {
                txtEncryptionKey.PlaceholderText = "Enter 48 hex characters (192-bit key)";
            }
            else if (rb == radioAes128)
            {
                txtEncryptionKey.PlaceholderText = "Enter 32 hex characters (128-bit key)";
            }

            // Clear the encryption key field when changing AES type to avoid confusion
            txtEncryptionKey.Text = string.Empty;
        }
    }

    private void logoPictureBox_Click(object sender, EventArgs e)
    {

    }

    private void Form1_Load(object sender, EventArgs e)
    {

    }
}
