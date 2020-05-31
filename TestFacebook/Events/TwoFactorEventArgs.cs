using System;

namespace TestFacebook.Events
{
  public class TwoFactorEventArgs : EventArgs
  {
    public string TwoFactorCode { get; set; }
  }
}
