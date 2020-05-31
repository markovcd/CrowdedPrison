using System;
using TestFacebook.Entities;

namespace TestFacebook.Events
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
