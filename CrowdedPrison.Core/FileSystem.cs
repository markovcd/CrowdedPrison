using System.IO;

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

    public string ReadAllText(string fileName)
    {
      return File.ReadAllText(fileName);
    }

    public void DeleteFile(string fileName)
    {
      File.Delete(fileName);
    }

    public void WriteAllText(string fileName, string text)
    {
      File.WriteAllText(fileName, text);
    }
  }
}