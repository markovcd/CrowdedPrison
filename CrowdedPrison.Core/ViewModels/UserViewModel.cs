using CrowdedPrison.Core.Services;
using CrowdedPrison.Messenger;
using CrowdedPrison.Messenger.Encryption;
using CrowdedPrison.Messenger.Entities;
using CrowdedPrison.Messenger.Events;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CrowdedPrison.Core.ViewModels
{

  public class UserViewModel : BindableBase, IUserViewModel
  {
    private MessengerUser user;

    private readonly IGpgMessenger gpgMessenger;
    private readonly IMessenger messenger;
    private readonly IMainDialogService dialogService;
    private readonly IEventAggregator events;

    public string PublicKey { get; private set; }

    public MessengerUser User
    {
      get => user;
      set => SetProperty(ref user, value);
    }

    public ObservableCollection<MessengerMessage> Messages { get; } = new ObservableCollection<MessengerMessage>();

    public ICommand SendMessageCommand { get; }
    public ICommand SendPublicKeyCommand { get; }

    public ICommand RequestPublicKeyCommand { get; }

    public UserViewModel(IGpgMessenger gpgMessenger, IMessenger messenger, IMainDialogService dialogService, IEventAggregator events)
    {
      this.gpgMessenger = gpgMessenger;
      this.messenger = messenger;
      this.dialogService = dialogService;
      this.events = events;
      SendMessageCommand = new DelegateCommand<string>(SendEncryptedMessage);
      SendPublicKeyCommand = new DelegateCommand(SendPublicKey);
      RequestPublicKeyCommand = new DelegateCommand(RequestPublicKey);

      Subscribe<MessageReceivedEvent, MessageReceivedEventArgs>(OnMessageReceived, e => e.Message.ThreadId == User.Id);
    }
    
    private void Subscribe<TEvent, TArgs>(Action<TArgs> action, Predicate<TArgs> predicate) where TEvent : PubSubEvent<TArgs>, new()
    {
      events.GetEvent<TEvent>().Subscribe(action, ThreadOption.UIThread, false, predicate);
    }

    private void OnMessageReceived(MessageReceivedEventArgs e)
    {
      Messages.Add(e.Message);
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

    private void RequestPublicKey()
    {
      
    }

    public async void SendPublicKey()
    {
      if (!await gpgMessenger.SendPublicKeyAsync(User))
        await dialogService.ShowMessageDialogAsync("Sending public key failed");
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
