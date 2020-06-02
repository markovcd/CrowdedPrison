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

    public async Task DownloadGpgAsync()
    {
      configuration.GpgPath = downloader.GetGpgPath();
      if (configuration.GpgPath == null)
        configuration.GpgPath = await dialogService.ShowDownloadGpgDialogAsync();

      if (configuration.GpgPath == null)
      {
        await dialogService.ShowMessageDialogAsync("Error during GnuPG installation. Application will be closed.");
        shellService.Close();
      }
    }

    private async Task ConnectAsync()
    {
      await messenger.LoginAsync();
     
    }


    private void Messenger_Typing(object sender, TypingEventArgs e)
    {
      
    }

    private void Messenger_MessageUnsent(object sender, MessageUnsentEventArgs e)
    {
      
    }

    private async Task Messenger_UserLoginRequested(object sender, UserLoginEventArgs e)
    {
      (e.Email, e.Password) = await dialogService.ShowLoginDialogAsync(configuration.MessengerEmail);
      e.IsCancelled = e.Email == null;
    }

    private async Task Messenger_TwoFactorRequested(object sender, TwoFactorEventArgs e)
    {
      e.TwoFactorCode = await dialogService.ShowTwoFactorDialogAsync();
      e.IsCancelled = e.TwoFactorCode == null;
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
      DownloadGpgAsync();
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
