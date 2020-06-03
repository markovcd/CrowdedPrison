namespace CrowdedPrison.Messenger.Encryption
{
  public interface IGpgMessengerConfiguration
  {
    string GpgPasswordHash { get; }
  }
}