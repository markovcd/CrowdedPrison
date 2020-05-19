using System.IO;
using System.Threading.Tasks;

namespace CrowdedPrison.Core
{

  public class FileSystem : IFileSystem
  {
    public string GetTempFilePath()
    {
      var randomName = Path.GetRandomFileName();
      var tempPath = Path.GetTempPath();
      return Path.Combine(tempPath, randomName);
    }

    public void DeleteFile(string fileName)
    {
      File.Delete(fileName);
    }

    public async Task<string> ReadAllTextAsync(string fileName)
    {
      using var streamReader = new StreamReader(fileName);
      return await streamReader.ReadToEndAsync();
    }

    public async Task WriteAllTextAsync(string fileName, string text)
    {
      using var streamWriter = new StreamWriter(fileName);
      await streamWriter.WriteAsync(text);
    }
  }
}