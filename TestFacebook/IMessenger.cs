using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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
    MessengerUser Self { get; }

    event EventHandler<TwoFactorEventArgs> TwoFactorRequested;
    event EventHandler<UserLoginEventArgs> UserLoginRequested;
    event EventHandler<MessageReceivedEventArgs> MessageReceived;

    Task<bool> LoginAsync();
    Task LogoutAsync();
    Task UpdateUsersAsync();
    Task UpdateActiveUsersAsync();
    Task<bool> SendTextAsync(string userId, string message);
    Task<bool> SendTextAsync(MessengerUser user, string message);
    Task<bool> CheckConnectionStateAsync();
  }
}
