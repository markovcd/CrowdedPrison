namespace CrowdedPrison.Encryption
{
  public interface ITwoWayEncryption
  {
    string Encrypt(string s, string entropy = null);
    string Decrypt(string s, string entropy = null);
    bool TryDecrypt(string input, out string output, string entropy = null);
  }
}
