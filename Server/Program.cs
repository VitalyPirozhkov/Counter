using Server.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Введите порт для сервера:");
            if (int.TryParse(Console.ReadLine(), out int port))
            {
                Counter counter = new Counter();
                SocketServer server = new SocketServer(port, counter);
                server.Start().Wait();
            }
            else
            {
                Console.WriteLine("Некорректный порт. Завершение работы.");
            }
        }
    }
}
