using CrowdedPrison.Encryption;
using CrowdedPrison.Messenger.Encryption;

namespace CrowdedPrison.Wpf
{
  public class AppConfiguration : IGpgConfiguration, IGpgMessengerConfiguration
  {
    public string GpgPath { get; }
    public string HomeDir { get; }
    public string GpgPassword { get; }
    public string MessengerEmail { get; }
  }
}