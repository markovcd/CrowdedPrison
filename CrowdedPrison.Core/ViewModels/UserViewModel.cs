using CrowdedPrison.Messenger;
using CrowdedPrison.Messenger.Encryption;
using CrowdedPrison.Messenger.Entities;
using Prism.Mvvm;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

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

    public UserViewModel(IGpgMessenger gpgMessenger, IMessenger messenger)
    {
      this.gpgMessenger = gpgMessenger;
      this.messenger = messenger;
      
    }

    public async Task RefreshMessagesAsync(int limit = 20)
    {
      var messages = await messenger.GetMessagesAsync(User, limit);
      Messages.Clear();
      Messages.AddRange(messages);
    }

  }
}
