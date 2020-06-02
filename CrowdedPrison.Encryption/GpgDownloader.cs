using CrowdedPrison.Common;
using System;
using System.Threading.Tasks;
using Microsoft.Win32;


namespace CrowdedPrison.Encryption
{
  public class GpgDownloader : IGpgDownloader
  {
    private static readonly Version GpgVersion = new Version("2.2.20");
    private static readonly DateTime GpgVersionDate = new DateTime(2020, 03, 20);
    private static string GpgUrl => $"https://www.gnupg.org/ftp/gcrypt/binary/gnupg-w32-{GpgVersion}_{GpgVersionDate:yyyyMMdd}.exe";
    private readonly IFileSystem fileSystem;
    private readonly Func<IAsyncProcess> processFactory;

    public GpgDownloader(IFileSystem fileSystem, Func<IAsyncProcess> processFactory)
    {
      this.fileSystem = fileSystem;
      this.processFactory = processFactory;
    }

    public async Task<string> EnsureGpgExistsAsync()
    {
      var gpgPath = GetGpgPath();
      if (gpgPath == null) gpgPath = await DownloadGpgAsync();
      return gpgPath;
    }

    public async Task<string> DownloadGpgAsync()
    {
      var installerPath = fileSystem.GetTempFilePath("exe");

      await fileSystem.DownloadFileAsync(GpgUrl, installerPath);

      var p = processFactory();
      using (p as IDisposable) p.Start(installerPath, "/S", true);

      return await GetGpgPathAsync();  
    }

    private async Task<string> GetGpgPathAsync()
    {
      const int timeout = 30000;
      var start = Environment.TickCount;
      var delta = 0;

      while (delta < timeout)
      {
        var path = GetGpgPath();
        if (path != null && fileSystem.FileExists(path)) return path;
        await Task.Delay(100);
        delta = Environment.TickCount - start;
      }

      throw new TimeoutException();
    }

    public string GetGpgPath()
    {
      var dir = GetGpgInstallDirectory();
      if (dir == null) return null;
      return $"{dir}\\bin\\gpg.exe";
    }

    public bool IsGpgInstalled() 
    {
      var path = GetGpgPath();
      return path != null && fileSystem.FileExists(path);
    }

    public string GetGpgInstallDirectory()
    {
      using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\GnuPG") 
        ?? Registry.LocalMachine.OpenSubKey(@"SOFTWARE\GnuPG");

      return key?.GetValue("Install Directory")?.ToString();
    }

  }
}
