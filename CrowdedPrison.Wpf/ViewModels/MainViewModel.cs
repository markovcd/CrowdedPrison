using CrowdedPrison.Messenger;
using CrowdedPrison.Messenger.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Text;

namespace CrowdedPrison.Wpf.ViewModels
{
  internal class MainViewModel : BindableBase
  {
    private readonly IMessenger messenger;

    public MainViewModel(IMessenger messenger)
    {
      this.messenger = messenger;

      messenger.ConnectionStateChanged += Messenger_ConnectionStateChanged;
      messenger.MessageReceived += Messenger_MessageReceived;
      messenger.MessagesDelivered += Messenger_MessagesDelivered;
      messenger.TwoFactorRequested += Messenger_TwoFactorRequested;
      messenger.UserLoginRequested += Messenger_UserLoginRequested;
      messenger.MessageUnsent += Messenger_MessageUnsent;
      messenger.Typing += Messenger_Typing;
    }

    private void Messenger_Typing(object sender, TypingEventArgs e)
    {
      
    }

    private void Messenger_MessageUnsent(object sender, MessageUnsentEventArgs e)
    {
      
    }

    private void Messenger_UserLoginRequested(object sender, UserLoginEventArgs e)
    {
      
    }

    private void Messenger_TwoFactorRequested(object sender, TwoFactorEventArgs e)
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
      
    }
  }
}
