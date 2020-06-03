using System;
using System.Threading.Tasks;
using CrowdedPrison.Core.ViewModels;

namespace CrowdedPrison.Core.Services
{
  internal class MainDialogService : IMainDialogService
  {
    private readonly IDialogService dialogService;
    private readonly Func<LoginDialogViewModel> loginVmFactory;
    private readonly Func<InputDialogViewModel> inputVmFactory;
    private readonly Func<PasswordDialogViewModel> passwordVmFactory;
    private readonly Func<DownloadGpgDialogViewModel> downloadGpgVmFactory;
    private readonly Func<MessageDialogViewModel> messageVmFactory;
    private readonly Func<SpinnerDialogViewModel> spinnerVmFactory;

    private SpinnerDialogViewModel spinnerVm;
    private Task spinnerTask;

    public MainDialogService(IDialogService dialogService, Func<LoginDialogViewModel> loginVmFactory, 
      Func<InputDialogViewModel> inputVmFactory, Func<PasswordDialogViewModel> passwordVmFactory,
      Func<DownloadGpgDialogViewModel> downloadGpgVmFactory, Func<MessageDialogViewModel> messageVmFactory, 
      Func<SpinnerDialogViewModel> spinnerVmFactory)
    {
      this.dialogService = dialogService;
      this.loginVmFactory = loginVmFactory;
      this.inputVmFactory = inputVmFactory;
      this.passwordVmFactory = passwordVmFactory;
      this.downloadGpgVmFactory = downloadGpgVmFactory;
      this.messageVmFactory = messageVmFactory;
      this.spinnerVmFactory = spinnerVmFactory;
    }

    public async Task<(string email, string password)> ShowLoginDialogAsync(string email = null)
    {
      var vm = loginVmFactory();
      vm.Email = email;
      return await dialogService.ShowDialogAsync(vm);
    }

    public async Task<string> ShowInputDialogAsync(string message = null)
    {
      var vm = inputVmFactory();
      vm.Message = message;
      return await dialogService.ShowDialogAsync(vm);
    }

    public async Task<string> ShowPasswordDialogAsync(string message = null)
    {
      var vm = passwordVmFactory();
      vm.Message = message;
      return await dialogService.ShowDialogAsync(vm);
    }

    public async Task<string> ShowDownloadGpgDialogAsync()
    {
      var vm = downloadGpgVmFactory();
      return await dialogService.ShowDialogAsync(vm);
    }

    public Task ShowSpinnerDialogAsync(string message)
    {
      spinnerVm = spinnerVmFactory();
      spinnerVm.Message = message;
      spinnerTask = dialogService.ShowDialogAsync(spinnerVm);
      return spinnerTask;
    }

    public async Task HideSpinnerDialogAsync()
    {
      spinnerVm?.Close();
      if (spinnerTask != null) await spinnerTask;
      spinnerVm = null;
      spinnerTask = null;
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