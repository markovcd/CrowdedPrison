using System;

namespace TestFacebook.Events
{
  public enum MessengerConnectionState
  {
    Disconnected,
    Connected,
    LoggedOut,
    LoggingIn,
    LoggedIn
  }

  public class ConnectionStateEventArgs : EventArgs
  {
    public MessengerConnectionState State { get; }
    public string Email { get; }
    public string Reason { get; }

    public ConnectionStateEventArgs(MessengerConnectionState state, string email, string reason)
    {
      State = state;
      Email = email;
      Reason = reason;
    }
  }
}
