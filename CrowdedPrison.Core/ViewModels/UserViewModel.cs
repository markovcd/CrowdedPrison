using CrowdedPrison.Messenger;
using CrowdedPrison.Messenger.Encryption;
using CrowdedPrison.Messenger.Entities;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace CrowdedPrison.Core.ViewModels
{

  public class UserViewModel : BindableBase, IUserViewModel
  {
    private MessengerUser user;

    private readonly IGpgMessenger gpgMessenger;
    private readonly IMessenger messenger;

    public string PublicKey { get; private set; }

    public MessengerUser User
    {
      get => user;
      set => SetProperty(ref user, value);
    }

    public ObservableCollection<MessengerMessage> Messages { get; } = new ObservableCollection<MessengerMessage>();

    public ICommand SendMessageCommand { get; }

    public UserViewModel(IGpgMessenger gpgMessenger, IMessenger messenger)
    {
      this.gpgMessenger = gpgMessenger;
      this.messenger = messenger;

      SendMessageCommand = new DelegateCommand<string>(SendEncryptedMessage);
    }

    public async void RefreshMessages(int limit = 20)
    {
      ClearMessages();
      var messages = await messenger.GetMessagesAsync(User, limit);
      Messages.AddRange(messages.Reverse());
    }

    public void ClearMessages()
    {
      Messages.Clear();
    }

    public async void SendEncryptedMessage(string message)
    {
      if (!await gpgMessenger.IsKeyPresentAsync(User))
        return;

      if (!await gpgMessenger.SendEncryptedTextAsync(User, message))
        return;

      var m = new MessengerMessage(messenger.Self.Id, message, DateTime.Now, User.Id);
      Messages.Add(m);
    }

    public void ChatOpened()
    {
      RefreshMessages();
      LoadPublicKey();
    }

    public async void LoadPublicKey()
    {
      if (string.IsNullOrEmpty(PublicKey))
      {
        if (await gpgMessenger.IsKeyPresentAsync(User))
          PublicKey = await gpgMessenger.LoadPublicKeyAsync(user);
        else
        {
          var isImported = await gpgMessenger.ImportPublicKeyAsync(user);
          if (isImported) PublicKey = await gpgMessenger.LoadPublicKeyAsync(user);
        }
      }
    }

    public void ChatClosed()
    {
      ClearMessages();
    }
  }
}
