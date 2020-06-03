using CrowdedPrison.Messenger;
using CrowdedPrison.Messenger.Encryption;
using CrowdedPrison.Messenger.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace CrowdedPrison.Core.ViewModels
{
  public class UserViewModel : BaseThreadViewModel<MessengerUser>
  {
    public UserViewModel(IGpgMessenger gpgMessenger, IMessenger messenger) : base(gpgMessenger, messenger)
    {
    }
  }
}
