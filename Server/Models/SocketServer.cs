using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Server.Models
{
    public class SocketServer
    {
        private const int bufferSize = 1024;
        private readonly int port;
        private readonly Counter counter;
        private TcpListener listener;
        private List<TcpClient> clients = new List<TcpClient>();

        public SocketServer(int port, Counter counter)
        {
            this.port = port;
            this.counter = counter;
        }

        public async Task Start()
        {
            try
            {
                listener = new TcpListener(IPAddress.Any, port);
                listener.Start();
                Console.WriteLine($"Сокет сервер запущен на порту {port}");

                while (true)
                {
                    TcpClient client = await listener.AcceptTcpClientAsync();
                    Console.WriteLine($"Установлено соединение с клиентом {((IPEndPoint)client.Client.RemoteEndPoint).Address}:{((IPEndPoint)client.Client.RemoteEndPoint).Port}");
                    _ = HandleClientAsync(client);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }
        }

        private async Task HandleClientAsync(TcpClient client)
        {
            clients.Add(client);
            try
            {
                using (NetworkStream stream = client.GetStream())
                {
                    byte[] buffer = new byte[bufferSize];
                    int bytesRead;

                    while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                    {
                        string request = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        string response = ProcessRequest(request);
                        if (response.TrimEnd() == "STOP" || response.TrimEnd() == "CONTINUE")
                        {
                            await SendToAllClients(response);
                        }
                        else
                        {
                            byte[] responseData = Encoding.UTF8.GetBytes(response);
                            await stream.WriteAsync(responseData, 0, responseData.Length);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при обработке клиента: {ex.Message}");
            }
            finally
            {
                clients.Remove(client);
                client.Close();
            }
        }

        private string ProcessRequest(string request)
        {
            string response = "OK";
            if (request == "START")
            {
                counter.Start();
            }
            else if (request == "STOP")
            {
                counter.Stop();
                response = "STOP";
            }
            else if (request == "CONTINUE")
            {
                counter.Continue();
                response = "CONTINUE";
            }
            else if (request == "RESET")
            {
                counter.Reset();
            }
            else if (request == "GET")
            {
                response = $"COUNT {counter.GetCount()}";
            }
            if (response.Length < bufferSize)
            {
                response = response.PadRight(bufferSize);
            }
            return response;
        }
        private async Task SendToAllClients(string message)
        {
            byte[] messageData = Encoding.UTF8.GetBytes(message);
            foreach (TcpClient connectedClient in clients)
            {
                await connectedClient.GetStream().WriteAsync(messageData, 0, messageData.Length);
            }
        }
    }
}
