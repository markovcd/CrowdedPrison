using System;
using System.Threading.Tasks;
using CrowdedPrison.Wpf.ViewModels;

namespace CrowdedPrison.Wpf.Services
{
  internal class MainDialogService : IMainDialogService
  {
    private readonly IDialogService dialogService;
    private readonly Func<LoginDialogViewModel> loginVmFactory;
    private readonly Func<TwoFactorDialogViewModel> twoFactorVmFactory;
    private readonly Func<DownloadGpgDialogViewModel> downloadGpgVmFactory;
    private readonly Func<MessageDialogViewModel> messageVmFactory;

    public MainDialogService(IDialogService dialogService, Func<LoginDialogViewModel> loginVmFactory, 
      Func<TwoFactorDialogViewModel> twoFactorVmFactory, Func<DownloadGpgDialogViewModel> downloadGpgVmFactory,
      Func<MessageDialogViewModel> messageVmFactory)
    {
      this.dialogService = dialogService;
      this.loginVmFactory = loginVmFactory;
      this.twoFactorVmFactory = twoFactorVmFactory;
      this.downloadGpgVmFactory = downloadGpgVmFactory;
      this.messageVmFactory = messageVmFactory;
    }

    public async Task<(string email, string password)> ShowLoginDialogAsync(string email = null)
    {
      var vm = loginVmFactory();
      vm.Email = email;
      return await dialogService.ShowDialogAsync(vm);
    }

    public async Task<string> ShowTwoFactorDialogAsync()
    {
      var vm = twoFactorVmFactory();
      return await dialogService.ShowDialogAsync(vm);
    }

    public async Task<string> ShowDownloadGpgDialogAsync()
    {
      var vm = downloadGpgVmFactory();
      return await dialogService.ShowDialogAsync(vm);
    }

    public async Task<Buttons> ShowMessageDialogAsync(string message, Buttons buttons = Buttons.Ok)
    {
      var vm = messageVmFactory();
      vm.Message = message;
      vm.Buttons = buttons;
      return await dialogService.ShowDialogAsync(vm);
    }
  }
}