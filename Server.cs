using System.Net;
using System.Net.Sockets;
using System.Text;

namespace dotnet_webserver;

public class WebServer : IServer
{
    private readonly HttpListener _listener;
    private readonly string _baseFolder;
    private bool _isRunning = false;

    private static readonly int _maxConnections = 20;
    private static readonly Semaphore _pool = new Semaphore(_maxConnections, _maxConnections);
    
    public WebServer(string testDirectory)
    {
        _baseFolder = testDirectory;
        List<IPAddress> ipAddresses = GetLocalHostIPs();
        _listener = InitializeListener(ipAddresses);
    }

    public async Task Start()
    {
        _listener.Start();
        _isRunning = true;
        Console.WriteLine($"Server started.");
        await Task.Run(() => RunServer(_listener));
    }
    
    public void Stop()
    {
        Console.WriteLine("Stopping server...");
        _isRunning = false;

        if (_listener.IsListening)
        {
            _listener.Stop();
        }

        _listener.Close();
        Console.WriteLine("Server stopped.");
    }

    public void Dispose()
    {
        _isRunning = false;
        _listener.Close();
    }

    private async Task RunServer(HttpListener listener)
    {
        while (_isRunning)
        {
            try
            {
                _pool.WaitOne();
                if (!_isRunning) break;
                await ProcessRequest(listener);
            }
            catch (HttpListenerException ex)
            {
                Console.WriteLine($"HttpListener error: {ex}");
                if (!_isRunning) break;
            }
            catch (ObjectDisposedException ex)
            {
                Console.WriteLine($"HttpListener has been disposed: {ex}");
                break;
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine(ex);
            }
        }
        _pool.Release();

    }

    private async Task ProcessRequest(HttpListener listener)
    {
        if (_listener.IsListening)
        {
            HttpListenerContext context = await listener.GetContextAsync();

            try
            {
                AsciiArtPicker asciiPicker = new AsciiArtPicker(_baseFolder);

                string filePath = asciiPicker.GetRandomFile();
                byte[] response = await GetFileBytes(context, filePath);
                
                context.Response.ContentLength64 = response.Length;

                await StreamOutput(context, response);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

        }

        listener.Close();
    }
    
    /**
    *  Returns list of IP addresses assigned to localhost network devices
    *
    */
    private static List<IPAddress> GetLocalHostIPs()
    {
        string hostName = Dns.GetHostName();
        IPHostEntry host = Dns.GetHostEntry(hostName);

        List<IPAddress> ipAddresses = host.AddressList
            .Where(ip => ip.AddressFamily == AddressFamily.InterNetwork)
            .ToList();

        return ipAddresses;
    }

    private static HttpListener InitializeListener(List<IPAddress> localhostIPs)
    {
        var listener = new HttpListener();
        
        listener.Prefixes.Add("http://localhost:8080/");
        
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