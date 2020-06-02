using CrowdedPrison.Encryption;
using CrowdedPrison.Messenger;
using CrowdedPrison.Messenger.Encryption;

namespace CrowdedPrison.Wpf
{
  public class AppConfiguration : IGpgConfiguration, IMessengerConfiguration, IGpgMessengerConfiguration
  {
    public string GpgPath { get; set; }
    public string HomeDir { get; }
    public string GpgPassword { get; }
    public string MessengerEmail { get; }
  }
}