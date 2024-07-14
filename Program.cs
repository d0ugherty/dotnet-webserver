using dotnet_webserver;

using (var server = new WebServer())
{
    try
    {
        server.Start();
        Console.WriteLine("Server running...");
        Console.ReadLine();
    } 
    catch (Exception ex)
    {
        Console.WriteLine(ex);
    }
    finally
    {
        server.Stop();
    }
}


