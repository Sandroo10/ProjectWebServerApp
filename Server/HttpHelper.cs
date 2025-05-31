using System.IO;
using System.Text;

namespace Server
{
    public static class HttpHelper
    {
        public static void SendResponse(StreamWriter writer, int code, string message, string contentType = "text/html", int length = 0)
        {
            writer.WriteLine($"HTTP/1.1 {code} {message}");
            writer.WriteLine($"Content-Type: {contentType}");
            writer.WriteLine($"Content-Length: {length}");
            writer.WriteLine("Connection: close");
            writer.WriteLine();
        }

        public static string GetMimeType(string extension)
        {
            return extension switch
            {
                ".html" => "text/html",
                ".css" => "text/css",
                ".js" => "application/javascript",
                _ => "application/octet-stream"
            };
        }
        public static void SendErrorResponse(Stream stream, int code, string message, string rootFolder)
        {
            string errorPagePath = Path.Combine(rootFolder, "error.html");

            string errorHtml;

            if (File.Exists(errorPagePath))
            {
                errorHtml = File.ReadAllText(errorPagePath);

                // Replace either placeholders OR default text
                if (errorHtml.Contains("__ERROR_TITLE__") && errorHtml.Contains("__ERROR_DESCRIPTION__"))
                {
                    errorHtml = errorHtml
                        .Replace("__ERROR_TITLE__", $"{code} {message}")
                        .Replace("__ERROR_DESCRIPTION__", $"An error occurred: {code} {message}");
                }
                else
                {
                    errorHtml = errorHtml
                        .Replace("Oops! Something went wrong.", $"{code} {message}")
                        .Replace("The page you are looking for doesn't exist or another error occurred.", $"Error {code}: {message}");
                }
            }
            else
            {
                // Fallback HTML if no error.html exists
                errorHtml = $"<html><body><h1>{code} {message}</h1><p>An error occurred: {message}</p></body></html>";
            }

            byte[] body = Encoding.UTF8.GetBytes(errorHtml);

            using var writer = new StreamWriter(stream, leaveOpen: true) { AutoFlush = true };
            SendResponse(writer, code, message, "text/html", body.Length);
            stream.Write(body, 0, body.Length);
            stream.Flush();
        }


    }
}
