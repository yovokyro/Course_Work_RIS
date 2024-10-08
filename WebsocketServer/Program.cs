using System.Net.Sockets;
using System.Net;
using System;

namespace WebsocketServer
{
    internal class Program
    {
        const string IP = "127.0.0.1";
        const int PORT = 80;

        static private Server _server;

        static void Main(string[] args)
        {
            _server = new Server(IP, PORT);
            _server.Start();
            Console.ReadKey();
        }
    }
}
