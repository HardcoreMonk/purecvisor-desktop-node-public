using DesktopNode.Api;

namespace DesktopNode.Api.Tests;

internal sealed class RecordingBatchEvidenceFileAccess : IBatchEvidenceFileAccess
{
    public List<string> Calls { get; } = [];

    public Func<string, bool> FileExistsHandler { get; init; } = _ => false;

    public Func<string, bool> DirectoryExistsHandler { get; init; } = _ => false;

    public Func<string, FileAttributes> GetAttributesHandler { get; init; } = _ => FileAttributes.Normal;

    public Func<string, string> ReadAllTextHandler { get; init; } = path => throw new FileNotFoundException(null, path);

    public Func<string, IEnumerable<string>> ReadLinesHandler { get; init; } = _ => [];

    public Func<string, string, string[]> GetFilesHandler { get; init; } = (_, _) => [];

    public Func<string, string[]> GetDirectoriesHandler { get; init; } = _ => [];

    public Func<string, DateTime> GetLastWriteTimeUtcHandler { get; init; } = _ => DateTime.MinValue;

    public bool FileExists(string path)
    {
        Calls.Add($"FileExists:{path}");
        return FileExistsHandler(path);
    }

    public bool DirectoryExists(string path)
    {
        Calls.Add($"DirectoryExists:{path}");
        return DirectoryExistsHandler(path);
    }

    public FileAttributes GetAttributes(string path)
    {
        Calls.Add($"GetAttributes:{path}");
        return GetAttributesHandler(path);
    }

    public string ReadAllText(string path)
    {
        Calls.Add($"ReadAllText:{path}");
        return ReadAllTextHandler(path);
    }

    public IEnumerable<string> ReadLines(string path)
    {
        Calls.Add($"ReadLines:{path}");
        return ReadLinesHandler(path);
    }

    public string[] GetFiles(string directory, string searchPattern)
    {
        Calls.Add($"GetFiles:{directory}:{searchPattern}");
        return GetFilesHandler(directory, searchPattern);
    }

    public string[] GetDirectories(string directory)
    {
        Calls.Add($"GetDirectories:{directory}");
        return GetDirectoriesHandler(directory);
    }

    public DateTime GetLastWriteTimeUtc(string path)
    {
        Calls.Add($"GetLastWriteTimeUtc:{path}");
        return GetLastWriteTimeUtcHandler(path);
    }
}
