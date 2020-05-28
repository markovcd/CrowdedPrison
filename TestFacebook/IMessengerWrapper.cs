using System;
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
    public string Message { get; }
  }

  public interface IMessengerWrapper
  {
    event EventHandler<TwoFactorEventArgs> TwoFactorRequested;
    event EventHandler<UserLoginEventArgs> UserLoginRequested;
    event EventHandler<MessageReceivedEventArgs> MessageReceived;

    Task<bool> LoginAsync();
  }
}
