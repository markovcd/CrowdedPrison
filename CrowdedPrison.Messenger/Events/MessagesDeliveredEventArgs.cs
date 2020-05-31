using System;
using System.Collections.Generic;
using CrowdedPrison.Messenger.Entities;

namespace CrowdedPrison.Messenger.Events
{
  public class MessagesDeliveredEventArgs : EventArgs
  {
    public MessengerUser User { get; }
    public IReadOnlyList<MessengerMessage> Messages { get; }
    public MessengerThread Thread { get; }
    public DateTime At { get; }

    public MessagesDeliveredEventArgs(MessengerUser user, IReadOnlyList<MessengerMessage> messages, MessengerThread thread, DateTime at)
    {
      User = user;
      Messages = messages;
      Thread = thread;
      At = at;
    }
  }
}