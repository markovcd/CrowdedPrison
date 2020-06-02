using System.Threading.Tasks;

namespace CrowdedPrison.Encryption
{
  public interface IGpgDownloader
  {
    Task<string> EnsureGpgExistsAsync();
    Task<string> DownloadGpgAsync();
    string GetGpgPath();
    bool IsGpgInstalled();
  }
}
