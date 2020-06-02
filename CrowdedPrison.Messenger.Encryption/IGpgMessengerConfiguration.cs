namespace CrowdedPrison.Messenger.Encryption
{
  public interface IGpgMessengerConfiguration
  {
    string GpgPassword { get; }
  }
}