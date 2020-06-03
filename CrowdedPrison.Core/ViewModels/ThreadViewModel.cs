using CrowdedPrison.Messenger;
using CrowdedPrison.Messenger.Encryption;
using CrowdedPrison.Messenger.Entities;

namespace CrowdedPrison.Core.ViewModels
{
  public class ThreadViewModel : BaseThreadViewModel<MessengerThread>
  {
    public ThreadViewModel(IGpgMessenger gpgMessenger, IMessenger messenger) : base(gpgMessenger, messenger)
    {
    }
  }
}
