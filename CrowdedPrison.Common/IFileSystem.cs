using System.Threading.Tasks;

namespace CrowdedPrison.Common
{
  public interface IFileSystem
  {
    string CombinePaths(params string[] paths);
    string GetHomeDirectoryPath(string subDirectoryName);
    bool FileExists(string path);
    Task DownloadFileAsync(string url, string path);
    bool DirectoryExists(string path);
    string GetTempFilePath(string extension = null);
    void DeleteFile(string fileName);
    Task<string> ReadAllTextAsync(string fileName);
    Task WriteAllTextAsync(string fileName, string text);
    void CreateDirectory(string directoryName);
  }
}