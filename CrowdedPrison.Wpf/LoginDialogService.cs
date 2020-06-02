using System;
using System.Threading.Tasks;
using CrowdedPrison.Wpf.ViewModels;

namespace CrowdedPrison.Wpf
{
  internal class LoginDialogService : ILoginDialogService
  {
    private readonly IDialogService dialogService;
    private readonly Func<LoginDialogViewModel> loginVmFactory;
    private readonly Func<TwoFactorDialogViewModel> twoFactorVmFactory;
    private readonly Func<DownloadGpgDialogViewModel> downloadGpgVmFactory;

    public LoginDialogService(IDialogService dialogService, Func<LoginDialogViewModel> loginVmFactory, 
      Func<TwoFactorDialogViewModel> twoFactorVmFactory, Func<DownloadGpgDialogViewModel> downloadGpgVmFactory)
    {
      this.dialogService = dialogService;
      this.loginVmFactory = loginVmFactory;
      this.twoFactorVmFactory = twoFactorVmFactory;
      this.downloadGpgVmFactory = downloadGpgVmFactory;
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
  }
}