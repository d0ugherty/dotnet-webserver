using dotnet_webserver;


async Task Main(string[] args)
{
    string baseDirectory = args[0];

    using (var server = new WebServer(baseDirectory))
    {
        try
        {
            await server.Start();
            Console.WriteLine("Server running...");
            Console.ReadLine();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }
}


