using System;
using System.Text;
using System.Security.Cryptography;

namespace CrowdedPrison.Encryption
{
  internal class TwoWayEncryption : ITwoWayEncryption
  {
    public string Encrypt(string s, string entropy = null)
    {
      var bytes = Encoding.Unicode.GetBytes(s ?? string.Empty);
      var e = entropy == null ? null : Encoding.Unicode.GetBytes(entropy);

      var encrypted = ProtectedData.Protect(bytes, e, DataProtectionScope.CurrentUser);
      return Convert.ToBase64String(encrypted);
    }

    public string Decrypt(string s, string entropy = null)
    {
      var bytes = Convert.FromBase64String(s ?? string.Empty);
      var e = entropy == null ? null : Encoding.Unicode.GetBytes(entropy);

      var decrypted = ProtectedData.Unprotect(bytes, e, DataProtectionScope.CurrentUser);
      return Encoding.Unicode.GetString(decrypted);
    }

    public bool TryDecrypt(string input, out string output, string entropy = null)
    {
      try
      {
        output = Decrypt(input, entropy);
        return true;
      }
      catch
      {
        output = null;
        return false;
      }
    }
  }
}
