namespace CrowdedPrison.Core
{
  public interface IFileSystem
  {
    string GetTempFilePath();
    string ReadAllText(string fileName);
    void DeleteFile(string fileName);
    void WriteAllText(string fileName, string text);
  }
}