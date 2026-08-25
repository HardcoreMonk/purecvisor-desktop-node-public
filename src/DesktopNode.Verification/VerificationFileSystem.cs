using System.Text;

namespace DesktopNode.Verification;

internal sealed class PhysicalVerificationFileSystem : IVerificationFileSystem
{
    public string ReadAllText(string path) => File.ReadAllText(path);
    public void CreateDirectory(string path) => Directory.CreateDirectory(path);
    public Task WriteAllTextAsync(string path, string contents, CancellationToken cancellationToken) => File.WriteAllTextAsync(path, contents, new UTF8Encoding(false), cancellationToken);
    public bool FileExists(string path) => File.Exists(path);
    public void MoveFile(string source, string destination, bool overwrite) => File.Move(source, destination, overwrite);
    public void DeleteFile(string path) => File.Delete(path);
}
