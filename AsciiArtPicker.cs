using Random = System.Random;

namespace dotnet_webserver;

public class AsciiArtPicker
{
    private readonly string _baseDirectory;
    private readonly Random _random;

    public AsciiArtPicker(string baseDirectory)
    {
        _baseDirectory = baseDirectory;
        _random = new Random();
    }

    public string GetRandomFile()
    {
        string[] files = Directory.GetFiles(_baseDirectory);
        int size = files.Length;
        int randomIndex = _random.Next(size);

        string filePath = files[randomIndex];

        return filePath;
    }
}