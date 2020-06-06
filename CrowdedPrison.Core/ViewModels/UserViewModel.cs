using CrowdedPrison.Messenger;
using CrowdedPrison.Messenger.Encryption;
using CrowdedPrison.Messenger.Entities;
using Prism.Commands;
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

      SendMessageCommand = new DelegateCommand<string>(SendMessage);
    }

    public async Task RefreshMessagesAsync(int limit = 20)
    {
      ClearMessages();
      var messages = await messenger.GetMessagesAsync(User, limit);
      Messages.AddRange(messages.Reverse());
    }

    public void ClearMessages()
    {
      Messages.Clear();
    }

    public async void SendMessage(string message)
    {
      if (!await messenger.SendTextAsync(User, message)) return;
      var m = new MessengerMessage(null, message, DateTime.Now, user.Id);
      Messages.Add(m);
    }
  }
}
