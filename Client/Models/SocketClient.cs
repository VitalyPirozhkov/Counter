using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Client.Models
{
    public class SocketClient
    {
        private readonly string serverIp;
        private readonly int serverPort;
        private TcpClient client;
        private NetworkStream stream;
        private Timer timer;

        public event Action<string> CounterUpdated;
        public event Action<string> RenameButton;


        public bool IsConnected => client != null && client.Connected;
        public bool IsStoped { get;  set; }
        public int CounterValue { get; set; }

        public SocketClient(string serverIp, int serverPort)
        {
            this.serverIp = serverIp;
            this.serverPort = serverPort;
        }

        public async Task<bool> ConnectAsync()
        {
            try
            {
                client = new TcpClient();
                await client.ConnectAsync(serverIp, serverPort);
                stream = client.GetStream();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task SendCommandAsync(string command)
        {
            try
            {
                if (client == null || !client.Connected)
                    return;

                byte[] requestData = Encoding.UTF8.GetBytes(command);
                await stream.WriteAsync(requestData, 0, requestData.Length);

                byte[] responseData = new byte[1024];
                int bytesRead = await stream.ReadAsync(responseData, 0, responseData.Length);
                string response = Encoding.UTF8.GetString(responseData, 0, bytesRead);
                response = response.TrimEnd();
                HandleResponse(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void StartTimer()
        {
            timer = new Timer(1000);
            timer.Elapsed += async (sender, e) =>
            {
                if (IsConnected)
                {
                    await GetCounterValueAsync();
                }
            };
            timer.Start();
        }

        public async Task GetCounterValueAsync()
        {
            if (IsConnected)
            {
                await SendCommandAsync("GET");
            }
        }

        private void HandleResponse(string response)
        {
            if (response.StartsWith("STOP"))
            {
                IsStoped = true;
                RenameButton("STOP");
            }
            else if (response.StartsWith("CONTINUE"))
            {
                IsStoped = false;
                RenameButton("CONTINUE");
            }
            else if (response.StartsWith("COUNT "))
            {
                int count = int.Parse(response.Substring(6));
                CounterValue = count;
                CounterUpdated?.Invoke(response);
            }
        }

        public void Disconnect()
        {
            stream?.Close();
            client?.Close();
        }
    }
}
