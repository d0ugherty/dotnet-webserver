using System.Net;
using System.Text;

namespace dotnet_webserver;

public class WebServer : IDisposable
{
    private readonly HttpListener _listener;
    private readonly string _baseFolder;
    
    public WebServer(string uriPrefix, string baseFolder)
    {
        _baseFolder = baseFolder;
        _listener = new HttpListener();
        _listener.Prefixes.Add(uriPrefix);
    }

    public async void Start()
    {
        _listener.Start();

        while (true)
        {
            try
            {
                var context = await _listener.GetContextAsync();

                await Task.Run(() => ProcessRequestAsync(context));
            }
            catch (HttpListenerException ex)
            {
                break;
            }
            catch (InvalidOperationException ex)
            {
                break;
            }
        }
    }

    private async Task ProcessRequestAsync(HttpListenerContext context)
    {
        try
        {
            if (context.Request.RawUrl != null)
            {
                string fileName = Path.GetFileName(context.Request.RawUrl);
                string filePath = Path.Combine(_baseFolder, fileName);

                byte[] response = await GetFileBytes(context, filePath);

                SetContentLength(context, response);

                await StreamOutput(context, response);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }

    private static async Task StreamOutput(HttpListenerContext context, byte[] response)
    {
        using (Stream stream = context.Response.OutputStream)
        {
            await stream.WriteAsync(response, 0, response.Length);
        }
    }

    private static void SetContentLength(HttpListenerContext context, byte[] response)
    {
        context.Response.ContentLength64 = response.Length;
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
            Console.WriteLine($"Resource has been {filePath}");

            context.Response.StatusCode = (int)HttpStatusCode.OK;

            response = File.ReadAllBytes(filePath);
        }

        return Task.FromResult(response);
    }
    
    public void Stop()
    {
        _listener.Stop();
    }

    public void Dispose()
    {
        _listener.Close();
    }
}