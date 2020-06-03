using CrowdedPrison.Messenger;
using CrowdedPrison.Messenger.Encryption;
using CrowdedPrison.Messenger.Entities;
using Prism.Mvvm;
using System.Collections.ObjectModel;

namespace CrowdedPrison.Core.ViewModels
{
  public abstract class BaseThreadViewModel<T> : BindableBase
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
  }
}
