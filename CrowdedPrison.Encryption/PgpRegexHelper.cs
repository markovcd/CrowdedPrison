using System.Text.RegularExpressions;

namespace CrowdedPrison.Core
{
  public class PgpRegexHelper : IPgpRegexHelper
  {
    private const RegexOptions DefaultOptions = RegexOptions.Compiled | RegexOptions.Multiline;
    
    private const string PublicKeyBlockPattern =
      @"-----BEGIN PGP PUBLIC KEY BLOCK-----\r?\n[\s\S]+\r?\n-----END PGP PUBLIC KEY BLOCK-----";

    private const string MessageBlockPattern =
      @"-----BEGIN PGP MESSAGE-----\r?\n[\s\S]+\r?\n-----END PGP MESSAGE-----";

    private static readonly Regex PublicKeyBlockRegex = new Regex(PublicKeyBlockPattern, DefaultOptions);
    private static readonly Regex MessageBlockRegex = new Regex(MessageBlockPattern, DefaultOptions);

    public string GetPublicKeyBlock(string text)
    {
      return GetBlock(text, PublicKeyBlockRegex);
    }

    public string GetMessageBlock(string text)
    {
      return GetBlock(text, MessageBlockRegex);
    }

    private static string GetBlock(string text, Regex regex)
    {
      var match = regex.Match(text);
      return match.Success ? match.Value : null;
    }
  }
}
