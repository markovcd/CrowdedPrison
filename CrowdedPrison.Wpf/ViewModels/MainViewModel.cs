using CrowdedPrison.Messenger;
using CrowdedPrison.Messenger.Encryption;
using CrowdedPrison.Messenger.Events;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using CrowdedPrison.Messenger.Encryption.Events;
using CrowdedPrison.Encryption;
using Prism.Regions;
using CrowdedPrison.Wpf.Services;

namespace CrowdedPrison.Wpf.ViewModels
{
  internal class MainViewModel : BindableBase, INavigationAware
  {
    private readonly IMessenger messenger;
    private readonly IGpgMessenger gpgMessenger;
    private readonly IMainDialogService dialogService;
    private readonly AppConfiguration configuration;
    private readonly IGpgDownloader downloader;
    private readonly IShellService shellService;

    public ICommand ConnectCommand { get; }

    public MainViewModel(IMessenger messenger, IGpgMessenger gpgMessenger, IMainDialogService dialogService,
      AppConfiguration configuration, IGpgDownloader downloader, IShellService shellService)
    {
      this.messenger = messenger;
      this.gpgMessenger = gpgMessenger;
      this.dialogService = dialogService;
      this.configuration = configuration;
      this.downloader = downloader;
      this.shellService = shellService;

      ConnectCommand = new DelegateCommand(() => ConnectAsync());

      messenger.ConnectionStateChanged += Messenger_ConnectionStateChanged;
      messenger.MessageReceived += Messenger_MessageReceived;
      messenger.MessagesDelivered += Messenger_MessagesDelivered;
      messenger.TwoFactorRequested += Messenger_TwoFactorRequested;
      messenger.UserLoginRequested += Messenger_UserLoginRequested;
      messenger.MessageUnsent += Messenger_MessageUnsent;
      messenger.Typing += Messenger_Typing;
      gpgMessenger.EncryptedMessageReceived += GpgMessenger_EncryptedMessageReceived;
    }

    private void GpgMessenger_EncryptedMessageReceived(object sender, EncryptedMessageReceivedEventArgs e)
    {
      Debug.WriteLine(e.DecryptedText);
    }

    private async Task<bool> DownloadGpgAsync()
    {
      configuration.GpgPath = downloader.GetGpgPath();
      if (configuration.GpgPath == null)
        configuration.GpgPath = await dialogService.ShowDownloadGpgDialogAsync();
      else return true;

      if (configuration.GpgPath == null)
      {
        await dialogService.ShowMessageDialogAsync("Error during GnuPG installation. Application will be closed.");
        shellService.Close();
        return false;
      }
      await dialogService.ShowMessageDialogAsync("GnuPG installation was successful.");
      return true;
    }

    private async Task ConnectAsync()
    {
      bool success;

      try
      {
        ShowSpinner();
        success = await messenger.LoginAsync();
      }
      catch (OperationCanceledException)
      {
        success = true;
      }
     
      finally
      {
        await HideSpinnerAsync();
      }

      if (!success) await dialogService.ShowMessageDialogAsync("Login to Facebook failed. Make sure you entered correct password or two factor code.");
    }

    private void ShowSpinner()
    {
      dialogService.ShowSpinnerDialogAsync("Logging in...");
    }

    private async Task HideSpinnerAsync()
    {
      await dialogService.HideSpinnerDialogAsync();
    }


    private void Messenger_Typing(object sender, TypingEventArgs e)
    {
      
    }

    private void Messenger_MessageUnsent(object sender, MessageUnsentEventArgs e)
    {
      
    }

    private async Task Messenger_UserLoginRequested(object sender, UserLoginEventArgs e)
    {
      await HideSpinnerAsync();
      (e.Email, e.Password) = await dialogService.ShowLoginDialogAsync(configuration.MessengerEmail);
      e.IsCancelled = e.Email == null;
      ShowSpinner();
    }

    private async Task Messenger_TwoFactorRequested(object sender, TwoFactorEventArgs e)
    {
      await HideSpinnerAsync();
      e.TwoFactorCode = await dialogService.ShowTwoFactorDialogAsync();
      e.IsCancelled = e.TwoFactorCode == null;
      ShowSpinner();
    }

    private void Messenger_MessagesDelivered(object sender, MessagesDeliveredEventArgs e)
    {
      
    }

    private void Messenger_MessageReceived(object sender, MessageReceivedEventArgs e)
    {

    }

    private void Messenger_ConnectionStateChanged(object sender, ConnectionStateEventArgs e)
    {
      Debug.WriteLine($"{e.State} {e.Reason}");
    }

    public async void OnNavigatedTo(NavigationContext navigationContext)
    {
      if (await DownloadGpgAsync()) await ConnectAsync();
    }

    public bool IsNavigationTarget(NavigationContext navigationContext)
    {
      return true;
    }

    public void OnNavigatedFrom(NavigationContext navigationContext)
    {
    }
  }
}
