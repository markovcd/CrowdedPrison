using CrowdedPrison.Encryption;
using CrowdedPrison.Messenger;
using CrowdedPrison.Messenger.Encryption;
using Newtonsoft.Json;

namespace CrowdedPrison.Core
{
  public class AppConfiguration : IGpgConfiguration, IMessengerConfiguration, IGpgMessengerConfiguration
  {
    [JsonIgnore]
    public string GpgPath { get; set; }

    [JsonIgnore]
    public string HomeDir { get; set; }
    
    public string GpgPasswordHash { get; set; }

    public string MessengerEmail { get; set; }

    [JsonIgnore]
    public string SettingsFilePath { get; set; }
  }
}