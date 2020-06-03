using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace CrowdedPrison.Common
{
  internal class FileSystem : IFileSystem
  {
    public string CombinePaths(params string[] paths)
    {
      return Path.Combine(paths);
    }

    public string GetHomeDirectoryPath(string subDirectoryName)
    {
      string folderBase = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
      string dir = Path.Combine(folderBase, subDirectoryName);
      Directory.CreateDirectory(dir);
      return dir;
    }

    public bool FileExists(string path)
    {
      return File.Exists(path);
    }

    public bool DirectoryExists(string path)
    {
      return Directory.Exists(path);
    }

    public async Task DownloadFileAsync(string url, string path)
    {
      using var client = new WebClient();
      await client.DownloadFileTaskAsync(url, path);
    }

    public string GetTempFilePath(string extension = null)
    {
      var randomName = Path.GetRandomFileName();
      var tempPath = Path.GetTempPath();
      
      return string.IsNullOrEmpty(extension) 
        ? Path.Combine(tempPath, randomName)
        : Path.Combine(tempPath, $"{randomName}.{extension}");
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
      await using var streamWriter = new StreamWriter(fileName);
      await streamWriter.WriteAsync(text);
    }

    public void CreateDirectory(string directoryName)
    {
      Directory.CreateDirectory(directoryName);
    }
  }
}