using CrowdedPrison.Wpf.ViewModels;
using System.Threading.Tasks;

namespace CrowdedPrison.Wpf.Services
{
  internal interface IMainDialogService
  {
    Task<(string email, string password)> ShowLoginDialogAsync(string email = null);
    Task<string> ShowTwoFactorDialogAsync();
    Task<string> ShowDownloadGpgDialogAsync();
    Task<Buttons> ShowMessageDialogAsync(string message, Buttons buttons = Buttons.Ok);
  }
}