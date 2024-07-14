using System.Net;
using System.Net.Sockets;
using System.Text;

namespace dotnet_webserver;

public class WebServer : IServer
{
    private readonly HttpListener _listener;
    private readonly string _baseFolder;

    private static readonly int _maxConnections = 20;
    private static readonly Semaphore _pool = new Semaphore(_maxConnections, _maxConnections);
    
    public WebServer()
    {
        _baseFolder = "cat/";
        List<IPAddress> ipAddresses = GetLocalHostIPs();
        _listener = InitializeListener(ipAddresses);
    }

    public async void Start()
    { 
        _listener.Start();
        await Task.Run(() => RunServer(_listener));
    }
    
    public void Stop()
    {
        _listener.Stop();
    }

    public void Dispose()
    {
        _listener.Close();
    }
    
    public async Task RunServer(HttpListener listener)
    {
        while (true)
        {
            try
            {
                _pool.WaitOne();
                await ProcessRequest(listener);
            }
            catch (HttpListenerException ex)
            {
                Console.WriteLine(ex);
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine(ex);
            }
        }
    }

    private async Task ProcessRequest(HttpListener listener)
    {
        HttpListenerContext context = await listener.GetContextAsync();
        _pool.Release();

        try
        {
            AsciiArtPicker asciiPicker = new AsciiArtPicker(_baseFolder);

            string filePath = asciiPicker.GetRandomFile();
            
            byte[] response = await GetFileBytes(context, filePath);
            context.Response.ContentLength64 = response.Length;
            
            await StreamOutput(context, response);
        }
        catch(Exception ex)
        {
            Console.WriteLine(ex);
        }
    }
    
    /**
    *  Returns list of IP addresses assigned to localhost network devices
    *
    */
    private static List<IPAddress> GetLocalHostIPs()
    {
        string hostName = Dns.GetHostName();
        var host = Dns.GetHostEntry(hostName);

        List<IPAddress> ipAddresses = host.AddressList
            .Where(ip => ip.AddressFamily == AddressFamily.InterNetwork)
            .ToList();

        return ipAddresses;
    }

    private static HttpListener InitializeListener(List<IPAddress> localhostIPs)
    {
        HttpListener listener = new HttpListener();
        
        listener.Prefixes.Add("http://localhost/");
        
        localhostIPs.ForEach(ip =>
        {
            string uriPrefix = $"http://{ip.ToString()}/";
            Console.WriteLine($"Listening on {uriPrefix}");
            
            listener.Prefixes.Add(uriPrefix);
        });

        return listener;
    }

    private static async Task StreamOutput(HttpListenerContext context, byte[] response)
    {
        await using Stream stream = context.Response.OutputStream;
        await stream.WriteAsync(response);
    }
    
    private static Task<byte[]> GetFileBytes(HttpListenerContext context, string filePath)
    {
        byte[] response;
        
        if (!File.Exists(filePath))
        {
            Console.WriteLine($"Resource not found {filePath}");

            context.Response.StatusCode = (int)HttpStatusCode.NotFound;

            response = Encoding.UTF8.GetBytes($"File does not exist {filePath}");
        }
        else
        {
            context.Response.StatusCode = (int)HttpStatusCode.OK;

            response = File.ReadAllBytes(filePath);
        }

        return Task.FromResult(response);
    }
}