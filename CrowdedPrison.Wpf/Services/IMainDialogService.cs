using CrowdedPrison.Wpf.ViewModels;
using System.Threading.Tasks;

namespace CrowdedPrison.Wpf.Services
{
  internal interface IMainDialogService
  {
    Task<(string email, string password)> ShowLoginDialogAsync(string email = null);
    Task<string> ShowInputDialogAsync(string message = null);
    Task<string> ShowPasswordDialogAsync(string message = null);
    Task<string> ShowDownloadGpgDialogAsync();
    Task<Buttons> ShowMessageDialogAsync(string message, Buttons buttons = Buttons.Ok);
    Task ShowSpinnerDialogAsync(string message);
    Task HideSpinnerDialogAsync();
  }
}