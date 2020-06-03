using CrowdedPrison.Messenger;
using CrowdedPrison.Messenger.Encryption;
using CrowdedPrison.Messenger.Entities;
using Prism.Mvvm;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace CrowdedPrison.Core.ViewModels
{
  public interface IThreadViewModel<out T>
        where T : MessengerThread
  {
    T Metadata { get; }
    ObservableCollection<MessengerMessage> Messages { get; }
    ObservableCollection<MessengerUser> Users { get; }
  }

  public abstract class BaseThreadViewModel<T> : BindableBase, IThreadViewModel<T>
    where T : MessengerThread
  {
    private T metadata;

    private readonly IGpgMessenger gpgMessenger;
    private readonly IMessenger messenger;

    public T Metadata
    {
      get => metadata;
      set => SetProperty(ref metadata, value);
    }

    public ObservableCollection<MessengerMessage> Messages { get; } = new ObservableCollection<MessengerMessage>();
    public ObservableCollection<MessengerUser> Users { get; } = new ObservableCollection<MessengerUser>();

    public BaseThreadViewModel(IGpgMessenger gpgMessenger, IMessenger messenger)
    {
      this.gpgMessenger = gpgMessenger;
      this.messenger = messenger;
      
    }

    public async Task RefreshMessagesAsync(int limit = 20)
    {
      var messages = await messenger.GetMessagesAsync(Metadata, limit);
      Messages.Clear();
      Messages.AddRange(messages);
    }

    public async Task RefreshUsersAsync()
    {
      var users = await messenger.GetUsersAsync(new[] { Metadata });
      Users.Clear();
      Users.AddRange(users);
    }
  }
}
