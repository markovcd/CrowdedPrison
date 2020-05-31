using System;

namespace CrowdedPrison.Messenger.Events
{
  public class TwoFactorEventArgs : EventArgs
  {
    public string TwoFactorCode { get; set; }
    public bool IsCancelled { get; set; }
  }
}
