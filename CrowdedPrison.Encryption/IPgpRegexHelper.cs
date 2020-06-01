namespace CrowdedPrison.Encryption
{
  public interface IPgpRegexHelper
  {
    string GetPublicKeyBlock(string text);
    string GetMessageBlock(string text);

  }
}