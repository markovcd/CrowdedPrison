using CrowdedPrison.Messenger;
using CrowdedPrison.Messenger.Encryption;
using CrowdedPrison.Messenger.Events;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CrowdedPrison.Wpf.ViewModels
{
  internal class MainViewModel : BindableBase
  {
    private IGpgMessenger messenger;
    private readonly Func<IGpgMessenger> messengerFactory;
    private readonly IDialogService dialogService;
    private readonly Func<LoginDialogViewModel> loginVmFactory;
    private readonly Func<TwoFactorDialogViewModel> twoFactorVmFactory;

    public ICommand ConnectCommand { get; }

    public MainViewModel(Func<IGpgMessenger> messengerFactory, IDialogService dialogService, 
      Func<LoginDialogViewModel> loginVmFactory, Func<TwoFactorDialogViewModel> twoFactorVmFactory)
    {
      this.messengerFactory = messengerFactory;
      this.dialogService = dialogService;
      this.loginVmFactory = loginVmFactory;
      this.twoFactorVmFactory = twoFactorVmFactory;

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
      //var threads = await messenger.GetThreadsAsync();
      //var thread = threads.FirstOrDefault(t => t.Name.Contains("Chrup"));
      //var m = await messenger.GetMessagesAsync(thread.Id, 100);

      try
      {
       }
      catch (Exception ex)
      {

        throw;
      }
    }


    private void Messenger_Typing(object sender, TypingEventArgs e)
    {
      
    }

    private void Messenger_MessageUnsent(object sender, MessageUnsentEventArgs e)
    {
      
    }

    private async Task Messenger_UserLoginRequested(object sender, UserLoginEventArgs e)
    {
      var vm = loginVmFactory();
      vm.Email = "markovcd@gmail.com";
      (e.Email, e.Password, e.IsCancelled) = await dialogService.ShowDialogAsync(vm);
    }

    private async Task Messenger_TwoFactorRequested(object sender, TwoFactorEventArgs e)
    {
      var vm = twoFactorVmFactory();
      (e.TwoFactorCode, e.IsCancelled) = await dialogService.ShowDialogAsync(vm);
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
