using Server;

class Program
{
    static void Main()
    {
        // Always resolve full path to avoid runtime confusion
        string rootPath = Path.Combine(Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName, "webroot");

        var server = new WebServer(8080, rootPath);
        server.Start();
    }
}
