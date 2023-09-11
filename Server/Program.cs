using Server.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
namespace Server
{
    public class Program
    {
        static void Main(string[] args)
        {
            int port;
            string serverPortStr = ConfigurationManager.AppSettings["ServerPort"];
            if (!int.TryParse(serverPortStr, out port))
            {
                Console.WriteLine("В конфигурационном файле неверно указан порт, выбран порт по умолчанию 8080");
                port = 8080;
            }
            Counter counter = new Counter();
            SocketServer server = new SocketServer(port, counter);
            server.Start().Wait();
        }
    }
}
