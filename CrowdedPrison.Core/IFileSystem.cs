using System.Threading.Tasks;

namespace CrowdedPrison.Core
{
  public interface IFileSystem
  {
    string GetTempFilePath();
    void DeleteFile(string fileName);
    Task<string> ReadAllTextAsync(string fileName);
    Task WriteAllTextAsync(string fileName, string text);
  }
}