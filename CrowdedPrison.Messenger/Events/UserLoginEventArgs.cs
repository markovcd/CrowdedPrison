using System;

namespace CrowdedPrison.Messenger.Events
{
  public class UserLoginEventArgs : EventArgs
  {
    public string Email { get; set; }
    public string Password { get; set; }
    public bool IsCancelled { get; set; }
  }
}
