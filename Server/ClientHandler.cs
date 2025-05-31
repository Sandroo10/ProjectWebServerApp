using System;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace Server
{
    public class ClientHandler
    {
        private readonly TcpClient _client;
        private readonly string _rootFolder;

        public ClientHandler(TcpClient client, string rootFolder)
        {
            _client = client;
            _rootFolder = rootFolder;
        }

        public void Process()
        {
            using var stream = _client.GetStream();
            using var reader = new StreamReader(stream);
            using var writer = new StreamWriter(stream) { AutoFlush = true };

            string requestLine = reader.ReadLine();
            if (requestLine == null)
                return;

            Console.WriteLine($"[Request] {requestLine}");

            string[] tokens = requestLine.Split(' ');
            if (tokens.Length < 2 || tokens[0] != "GET")
            {
                HttpHelper.SendErrorResponse(stream, 405, "Method Not Allowed", _rootFolder);
                return;
            }

            string urlPath = tokens[1].Split('?')[0];
            string filename = urlPath.TrimStart('/');
            if (filename == "") filename = "index.html";

            if (filename.Contains(".."))
            {
                HttpHelper.SendErrorResponse(stream, 403, "Forbidden", _rootFolder);
                return;
            }

            string filePath = Path.Combine(_rootFolder, filename);
            filePath = Path.GetFullPath(filePath); 

            if (!filePath.StartsWith(_rootFolder))
            {
                HttpHelper.SendErrorResponse(stream, 403, "Access Denied", _rootFolder);
                return;
            }

            string extension = Path.GetExtension(filePath).ToLower();

            if (!(extension == ".html" || extension == ".css" || extension == ".js"))
            {
                HttpHelper.SendErrorResponse(stream, 403, "Forbidden", _rootFolder);
                return;
            }

            if (!File.Exists(filePath))
            {
                HttpHelper.SendErrorResponse(stream, 404, "Not Found", _rootFolder);
                return;
            }


            byte[] body = File.ReadAllBytes(filePath);
            string contentType = HttpHelper.GetMimeType(extension);

            HttpHelper.SendResponse(writer, 200, "OK", contentType, body.Length);
            stream.Write(body, 0, body.Length);
            stream.Flush();

            _client.Close();
        }
    }
}
