using System.Net;
using System.Reflection;
using System.Text;

namespace dotnet_webserver.ServerTest;

public class ServerTests
{
    private readonly string _testDirectory;

    public ServerTests()
    {
        _testDirectory = "../../../../test-data/";
    }

    [Fact]
    public void WebServer_StartAndStop()
    {
        using var webServer = new WebServer(_testDirectory);
        using var httpClient = new HttpClient();
        
        webServer.Start(); 
        webServer.Stop();
    }

    [Fact]
    public async Task WebServer_Success()
    {
        using var webServer = new WebServer(_testDirectory);
        using var httpClient = new HttpClient();
        
        webServer.Start(); 
        
        var clientTask = Task.Run(() => httpClient.GetAsync("http://localhost:8080/"));
        var completedTask = await Task.WhenAny(clientTask, Task.Delay(5000));
        
        if (completedTask == clientTask)
        {
            HttpResponseMessage response = await clientTask;
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
        else
        {
            throw new TimeoutException("The web server did not respond in a timely manner.");
        }
        
        webServer.Stop();
    }

    [Fact]
    public async Task WebServer_HasResponseContent()
    {
        using var webServer = new WebServer(_testDirectory);
        using var httpClient = new HttpClient();
        
        webServer.Start(); 
        
        var clientTask = Task.Run(() => httpClient.GetAsync("http://localhost:8080/"));
        var completedTask = await Task.WhenAny(clientTask, Task.Delay(5000));
        
        if (completedTask == clientTask)
        {
            HttpResponseMessage response = await clientTask;

            string content = await response.Content.ReadAsStringAsync();

            Assert.NotNull(content);
            Assert.NotEmpty(content);
            Assert.Equal("internet lol", content);
        }
        else
        {
            throw new TimeoutException("The web server did not respond in a timely manner.");
        }
        
        webServer.Stop();
    }
}