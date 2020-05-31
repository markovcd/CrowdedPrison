using System;
using CrowdedPrison.Messenger.Entities;

namespace CrowdedPrison.Messenger.Events
{
  public class MessageReceivedEventArgs : EventArgs
  {
    public MessengerUser User { get; }
    public MessengerMessage Message { get; }
    public MessengerMessage ReplyTo { get; }
    public MessageReceivedEventArgs(MessengerUser user, MessengerMessage message, MessengerMessage replyTo = null)
    {
      User = user;
      Message = message;
      ReplyTo = replyTo;
    }
  }
}
