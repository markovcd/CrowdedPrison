using CrowdedPrison.Messenger;
using CrowdedPrison.Messenger.Events;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CrowdedPrison.Wpf.ViewModels
{
  internal class MainViewModel : BindableBase
  {
    private IMessenger messenger;
    private readonly Func<IMessenger> messengerFactory;
    private readonly IDialogService dialogService;

    public ICommand ConnectCommand { get; }

    public MainViewModel(Func<IMessenger> messengerFactory, IDialogService dialogService)
    {
      this.messengerFactory = messengerFactory;
      this.dialogService = dialogService;

      ConnectCommand = new DelegateCommand(() => ConnectAsync());

    }

    private void CreateMessenger()
    {
      messenger = messengerFactory();

      messenger.ConnectionStateChanged += Messenger_ConnectionStateChanged;
      messenger.MessageReceived += Messenger_MessageReceived;
      messenger.MessagesDelivered += Messenger_MessagesDelivered;
      messenger.TwoFactorRequested += Messenger_TwoFactorRequested;
      messenger.UserLoginRequested += Messenger_UserLoginRequested;
      messenger.MessageUnsent += Messenger_MessageUnsent;
      messenger.Typing += Messenger_Typing;
    }

    private async Task ConnectAsync()
    {
      CreateMessenger();
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
      var vm = new LoginDialogViewModel();
      vm.WelcomeText = "fgsdfgsdgd";
      var b = await dialogService.ShowDialogAsync(vm);

    }

    private async Task Messenger_TwoFactorRequested(object sender, TwoFactorEventArgs e)
    {

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
