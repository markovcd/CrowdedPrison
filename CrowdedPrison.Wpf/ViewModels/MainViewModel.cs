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

namespace CrowdedPrison.Wpf.ViewModels
{
  internal class MainViewModel : BindableBase
  {
    private readonly IMessenger messenger;
    private readonly IGpgMessenger gpgMessenger;
    private readonly ILoginDialogService dialogService;
    private readonly AppConfiguration configuration;

    public ICommand ConnectCommand { get; }

    public MainViewModel(IMessenger messenger, IGpgMessenger gpgMessenger, ILoginDialogService dialogService, AppConfiguration configuration)
    {
      this.messenger = messenger;
      this.gpgMessenger = gpgMessenger;
      this.dialogService = dialogService;
      this.configuration = configuration;

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

    private async Task ConnectAsync()
    {
      await messenger.LoginAsync();
      //var threads = await messenger.GetThreadsAsync();
      //var thread = threads.FirstOrDefault(t => t.Name.Contains("Chrup"));
      //var m = await messenger.GetMessagesAsync(thread.Id, 100);

 
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
  }
}
