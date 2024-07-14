namespace dotnet_webserver;

public interface IServer : IDisposable
{
    public void Start();
    public void Stop();
}