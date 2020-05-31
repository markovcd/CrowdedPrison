using System;
using CrowdedPrison.Messenger.Entities;

namespace CrowdedPrison.Messenger.Events
{
  public class MessageUnsentEventArgs
  {
    public MessengerUser User { get; }
    public MessengerMessage Message { get; }
    public MessengerThread Thread { get; }
    public DateTime At { get; }

    public MessageUnsentEventArgs(MessengerUser user, MessengerMessage message, MessengerThread thread, DateTime at)
    {
      User = user;
      Message = message;
      Thread = thread;
      At = at;
    }
  }
}