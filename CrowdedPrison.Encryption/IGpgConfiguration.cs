namespace CrowdedPrison.Encryption
{
  public interface IGpgConfiguration
  {
    string GpgPath { get; }
    string HomeDir { get; }
  }
}