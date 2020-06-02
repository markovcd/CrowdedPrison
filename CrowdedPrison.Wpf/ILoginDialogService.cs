using System.Threading.Tasks;

namespace CrowdedPrison.Wpf
{
  internal interface ILoginDialogService
  {
    Task<(string email, string password)> ShowLoginDialogAsync(string email = null);
    Task<string> ShowTwoFactorDialogAsync();
    Task<string> ShowDownloadGpgDialogAsync();
  }
}