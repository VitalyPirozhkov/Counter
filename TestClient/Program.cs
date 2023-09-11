using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace TestClient
{   
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Введите IP адрес сервера:");
            string serverIp = Console.ReadLine();
            Console.WriteLine("Введите порт сервера:");
            if (int.TryParse(Console.ReadLine(), out int serverPort))
            {
                try
                {
                    using (TcpClient client = new TcpClient(serverIp, serverPort))
                    using (NetworkStream stream = client.GetStream())
                    {
                        Console.WriteLine("Подключено к серверу.");
                        Console.WriteLine("Доступные команды:");
                        Console.WriteLine("START - Запустить счетчик");
                        Console.WriteLine("STOP - Остановить счетчик");
                        Console.WriteLine("RESET - Сбросить счетчик");
                        Console.WriteLine("GET - Получить текущее значение счетчика");
                        Console.WriteLine("EXIT - Завершить приложение");

                        while (true)
                        {
                            Console.Write("Введите команду: ");
                            string command = Console.ReadLine().ToUpper();

                            if (command == "EXIT")
                                break;

                            byte[] requestData = Encoding.UTF8.GetBytes(command);
                            stream.Write(requestData, 0, requestData.Length);

                            byte[] responseData = new byte[1024];
                            int bytesRead = stream.Read(responseData, 0, responseData.Length);
                            string response = Encoding.UTF8.GetString(responseData, 0, bytesRead);
                            Console.WriteLine($"Ответ сервера: {response}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine("Некорректный порт. Завершение работы.");
            }
        }
    }

}
