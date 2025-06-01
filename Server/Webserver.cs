using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Server
{
    public class WebServer
    {
        private readonly int _port;
        private readonly string _rootFolder;

        public WebServer(int port, string rootFolder)
        {
            _port = port;
            _rootFolder = rootFolder;
        }

        public void Start()
        {
            var listener = new TcpListener(IPAddress.Any, _port);
            listener.Start();
            Console.WriteLine($"[Server] Listening on port {_port}...");

            while (true)
            {
                var client = listener.AcceptTcpClient();
                Console.WriteLine("[Server] Client connected.");

                ThreadPool.QueueUserWorkItem(_ =>
                {
                    try
                    {
                        var handler = new ClientHandler(client, _rootFolder);
                        handler.Process();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[Error] {ex.Message}");
                    }
                });
            }

        }
    }
}
