using System.Net;

namespace dotnet_webserver.ServerTest;

public class ServerTests
{
    [Fact]
    public void WebServer_StartAndStop()
    {
        var webServer = new WebServer();
        
        webServer.Start();
        webServer.Stop();
    }

    [Fact]
    public void WebSever_Dispose()
    {
        var webServer = new WebServer();
        
        webServer.Dispose();
    }

    [Fact]
    public async Task WebServer_Success()
    {
        var httpClient = new HttpClient();
        var webServer = new WebServer();
        
        webServer.Start();
        
        HttpResponseMessage response = await httpClient.GetAsync("http://192.168.1.159/cat/");
        
        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
    }
}