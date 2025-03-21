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

    public Form1()
    {
        InitializeComponent();
        btnListen.Click += BtnListen_Click;
        btnSend.Click += BtnSend_Click;

        // Generate random key for AES-256
        encryptionKey = new uint[]
        {
            0x00010203, 0x04050607, 0x08090a0b, 0x0c0d0e0f,
            0x10111213, 0x14151617, 0x18191a1b, 0x1c1d1e1f
        };
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
                int totalSize = BitConverter.ToInt32(sizeBuffer, 0);

                // Read encrypted data
                byte[] buffer = new byte[totalSize];
                int bytesRead = 0;
                while (bytesRead < totalSize)
                {
                    int read = await stream.ReadAsync(buffer, bytesRead, totalSize - bytesRead);
                    bytesRead += read;
                }

                // Convert to uint arrays (blocks)
                uint[][] encryptedBlocks = new uint[totalSize / 16][];
                for (int i = 0; i < encryptedBlocks.Length; i++)
                {
                    encryptedBlocks[i] = new uint[4];
                    Buffer.BlockCopy(buffer, i * 16, encryptedBlocks[i], 0, 16);
                }

                // Decrypt using CBC mode
                string message = Aes256Helper.DecryptCBC(encryptedBlocks, encryptionKey);

                this.Invoke(() =>
                {
                    txtReceived.AppendText($"{DateTime.Now}: {message}{Environment.NewLine}");
                });
            }
        }
        catch (Exception ex)
        {
            this.Invoke(() =>
            {
                MessageBox.Show($"Error handling client: {ex.Message}");
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
            string message = txtMessage.Text;

            // Generate IV and encrypt using CBC mode
            uint[] iv = Aes256Helper.GenerateIV();
            uint[][] encryptedBlocks = Aes256Helper.EncryptCBC(message, encryptionKey, iv);

            // Convert all blocks to single byte array
            byte[] dataToSend = new byte[encryptedBlocks.Length * 16];
            for (int i = 0; i < encryptedBlocks.Length; i++)
            {
                Buffer.BlockCopy(encryptedBlocks[i], 0, dataToSend, i * 16, 16);
            }

            using (TcpClient client = new TcpClient())
            {
                await client.ConnectAsync(host, port);
                using (NetworkStream stream = client.GetStream())
                {
                    // Send total size first
                    await stream.WriteAsync(BitConverter.GetBytes(dataToSend.Length));
                    // Send encrypted data
                    await stream.WriteAsync(dataToSend);
                }
            }

            MessageBox.Show("Encrypted message sent successfully!");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error sending message: {ex.Message}");
        }
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        StopListening();
        base.OnFormClosing(e);
    }
}
