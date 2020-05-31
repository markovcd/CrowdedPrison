using System;

namespace TestFacebook.Events
{
  public class UserLoginEventArgs : EventArgs
  {
    public string Email { get; set; }
    public string Password { get; set; }
  }
}
