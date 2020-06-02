using System;
using System.Threading.Tasks;
using CrowdedPrison.Wpf.ViewModels;

namespace CrowdedPrison.Wpf
{
  public interface ILoginDialogService
  {
    Task<(string email, string password)> ShowLoginDialogAsync(string email = null);
    Task<string> ShowTwoFactorDialogAsync();
  }

  internal class LoginDialogService : ILoginDialogService
  {
    private readonly IDialogService dialogService;
    private readonly Func<LoginDialogViewModel> loginVmFactory;
    private readonly Func<TwoFactorDialogViewModel> twoFactorVmFactory;

    public LoginDialogService(IDialogService dialogService, Func<LoginDialogViewModel> loginVmFactory, Func<TwoFactorDialogViewModel> twoFactorVmFactory)
    {
      this.dialogService = dialogService;
      this.loginVmFactory = loginVmFactory;
      this.twoFactorVmFactory = twoFactorVmFactory;
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
  }
}