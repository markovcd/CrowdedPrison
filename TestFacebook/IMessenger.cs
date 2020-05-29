using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using fbchat_sharp.API;

namespace TestFacebook
{
  public class TwoFactorEventArgs : EventArgs
  {
    public string TwoFactorCode { get; set; }
  }

  public class UserLoginEventArgs : EventArgs
  {
    public string Email { get; set; }
    public string Password { get; set; }
  }

  public class MessengerMessage
  {
    public string Text { get; }

    internal MessengerMessage(FB_Message message)
    {
      Text = message.text;
    }
  }

  public class MessageReceivedEventArgs : EventArgs
  {
    public MessengerUser User { get; }
    public MessengerMessage Message { get; }

    public MessageReceivedEventArgs(MessengerUser user, MessengerMessage message)
    {
      User = user;
      Message = message;
    }
  }

  public interface IMessenger : IAsyncDisposable
  {
    IReadOnlyDictionary<string, MessengerUser> Users { get; }

    event EventHandler<TwoFactorEventArgs> TwoFactorRequested;
    event EventHandler<UserLoginEventArgs> UserLoginRequested;
    event EventHandler<MessageReceivedEventArgs> MessageReceived;

    Task<bool> LoginAsync();
    Task LogoutAsync();
    Task UpdateUsersAsync();
    Task UpdateActiveUsersAsync();
  }
}
