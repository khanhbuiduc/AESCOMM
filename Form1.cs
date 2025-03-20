using System.Net;
using System.Net.Sockets;
using System.Text;

namespace AES2;

public partial class Form1 : Form
{
    private TcpListener? tcpListener;
    private bool isListening = false;
    private CancellationTokenSource? cts;

    public Form1()
    {
        InitializeComponent();
        btnListen.Click += BtnListen_Click;
        btnSend.Click += BtnSend_Click;
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
                byte[] buffer = new byte[1024];
                int bytesRead = await stream.ReadAsync(buffer);
                string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);

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

            using (TcpClient client = new TcpClient())
            {
                await client.ConnectAsync(host, port);
                using (NetworkStream stream = client.GetStream())
                {
                    byte[] data = Encoding.UTF8.GetBytes(message);
                    await stream.WriteAsync(data);
                }
            }

            MessageBox.Show("Message sent successfully!");
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
