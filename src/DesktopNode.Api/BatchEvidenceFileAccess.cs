namespace DesktopNode.Api;

internal interface IBatchEvidenceFileAccess
{
    bool FileExists(string path);

    bool DirectoryExists(string path);

    FileAttributes GetAttributes(string path);

    string ReadAllText(string path);

    IEnumerable<string> ReadLines(string path);

    string[] GetFiles(string directory, string searchPattern);

    string[] GetDirectories(string directory);

    DateTime GetLastWriteTimeUtc(string path);
}

internal sealed class PhysicalBatchEvidenceFileAccess : IBatchEvidenceFileAccess
{
    public static PhysicalBatchEvidenceFileAccess Instance { get; } = new();

    private PhysicalBatchEvidenceFileAccess()
    {
    }

    public bool FileExists(string path) => File.Exists(path);

    public bool DirectoryExists(string path) => Directory.Exists(path);

    public FileAttributes GetAttributes(string path) => File.GetAttributes(path);

    public string ReadAllText(string path) => File.ReadAllText(path);

    public IEnumerable<string> ReadLines(string path) => File.ReadLines(path);

    public string[] GetFiles(string directory, string searchPattern) => Directory.GetFiles(directory, searchPattern);

    public string[] GetDirectories(string directory) => Directory.GetDirectories(directory);

    public DateTime GetLastWriteTimeUtc(string path) => File.GetLastWriteTimeUtc(path);
}
