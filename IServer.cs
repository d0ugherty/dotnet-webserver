namespace dotnet_webserver;

public interface IServer : IDisposable
{
    public Task Start();
    public void Stop();
}