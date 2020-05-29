using System;
using System.Collections.Generic;
using System.Linq;

namespace CrowdedPrison.Core
{
  public enum FieldType
  {
    Unknown,
    PublicKey,
    CertX509,
    CertX509WithPrivateKey,
    Subkey,
    SecretKey,
    SecretSubKey,
    UserId,
    UserAttribute,
    Signature,
    RevocationSignature,
    RevocationSignatureStandalone,
    Fingerprint,
    PublicKeyData,
    Keygrip,
    RevocationKey,
    TofuStatistics,
    TrustDatabaseInformation,
    SignatureSubpacket,
    ConfigurationData
  }

  [Flags]
  public enum KeyValidity
  {
    None = 0,
    Unknown = 1 << 0,
    Invalid = 1 << 1,
    Disabled = 1 << 2,
    Revoked = 1 << 3,
    Expired = 1 << 4,
    UnknownValidity = 1 << 5,
    UndefinedValidity = 1 << 6,
    NotValid = 1 << 7,
    MarginalValid = 1 << 8,
    FullyValid = 1 << 9,
    UltimatelyValid = 1 << 10,
    PrivatePart = 1 << 11,
    SpecialValidity = 1 << 12
  }

  [Flags]
  public enum KeyCababilities
  {
    None = 0,
    Encrypt = 1 << 0,
    Sign = 1 << 1,
    Certify = 1 << 2,
    Authentication = 1 << 3,
    UnknownCapability = 1 << 4,
    DisabledKey = 1 << 5,
    WholeKeyEncrypt = 1 << 6,
    WholeKeySign = 1 << 7,
    WholeKeyCertify = 1 << 8,
    WholeKeyAuthentication = 1 << 9,
  }

  public enum PublicKeyAlgorithm
  {
    Unknown,
    Rsa,
    RsaEncryptOnly,
    RsaSignOnly,    
  }

  public class KeyListField
  {
    public FieldType Type { get; set; }
    public KeyValidity Validity { get; set; }
    public int KeyLength { get; set; }
    public PublicKeyAlgorithm Algorithm { get; set; }
    public string KeyId { get; set; }
    public DateTime CreationDate { get; set; }
    public DateTime ExpirationDate { get; set; }
    public string UserIdHash { get; set; }
    public string UserId { get; set; }
    public KeyCababilities Cababilities { get; set; }
  }

  public class KeyListParser
  {
    private static void AddToFlag(ref KeyValidity addTo, KeyValidity add)
    {
      addTo |= add;
    }

    public static IReadOnlyList<KeyListField> ParseFields(IReadOnlyList<IReadOnlyList<string>> l)
    {
      return l.Select(ParseField).ToList();
    }

    public static KeyListField ParseField(IReadOnlyList<string> f)
    {
      var result = new KeyListField();

      result.Type = GetFieldType(f[0]);
      result.Validity = GetValidity(f[1]);
      result.KeyLength = GetKeyLength(f[2]);
      result.Algorithm = GetAlgorithm(f[3]);
      result.KeyId = f[4];
      result.CreationDate = GetDate(f[5]);
      result.ExpirationDate = GetDate(f[6]);
      result.UserIdHash = f[7];
      result.UserId = f[9];
      result.Cababilities = GetCapabilities(f[11]);
      return result;
    }

    private static KeyCababilities GetCapabilities(string s)
    {
      KeyCababilities result = default;
      if (s.Contains('e')) result |= KeyCababilities.Encrypt;
      if (s.Contains('s')) result |= KeyCababilities.Sign;
      if (s.Contains('c')) result |= KeyCababilities.Certify;
      if (s.Contains('a')) result |= KeyCababilities.Authentication;
      if (s.Contains('?')) result |= KeyCababilities.UnknownCapability;
      if (s.Contains('D')) result |= KeyCababilities.DisabledKey;
      if (s.Contains('E')) result |= KeyCababilities.WholeKeyEncrypt;
      if (s.Contains('S')) result |= KeyCababilities.WholeKeySign;
      if (s.Contains('C')) result |= KeyCababilities.WholeKeyCertify;
      if (s.Contains('A')) result |= KeyCababilities.WholeKeyAuthentication;

      return result;
    }

    private static DateTime GetDate(string s)
    {
      var i = ParseLong(s);
      return DateTimeOffset.FromUnixTimeSeconds(i).DateTime;
    }

    private static PublicKeyAlgorithm GetAlgorithm(string s)
    {
      var i = ParseInteger(s);
      return (PublicKeyAlgorithm)i;
    }

    private static int GetKeyLength(string s)
    {
      return ParseInteger(s);
    }

    private static int ParseInteger(string s, int @default = default)
    {
      if (int.TryParse(s, out var i)) return i;
      return @default;
    }

    private static long ParseLong(string s, long @default = default)
    {
      if (long.TryParse(s, out var i)) return i;
      return @default;
    }

    private static KeyValidity GetValidity(string s)
    {
      KeyValidity result = default;
      if (s.Contains('o')) result |= KeyValidity.Unknown;
      if (s.Contains('i')) result |= KeyValidity.Invalid;
      if (s.Contains('d')) result |= KeyValidity.Disabled;
      if (s.Contains('r')) result |= KeyValidity.Revoked;
      if (s.Contains('e')) result |= KeyValidity.Expired;
      if (s.Contains('-')) result |= KeyValidity.UnknownValidity;
      if (s.Contains('q')) result |= KeyValidity.UndefinedValidity;
      if (s.Contains('n')) result |= KeyValidity.NotValid;
      if (s.Contains('m')) result |= KeyValidity.MarginalValid;
      if (s.Contains('f')) result |= KeyValidity.FullyValid;
      if (s.Contains('u')) result |= KeyValidity.UltimatelyValid;
      if (s.Contains('w')) result |= KeyValidity.PrivatePart;
      if (s.Contains('s')) result |= KeyValidity.SpecialValidity;

      return result;
    }

    private static FieldType GetFieldType(string s)
    {

      switch (s)
      {
        case "pub": return FieldType.PublicKey;
        case "crt": return FieldType.CertX509;
        case "crs": return FieldType.CertX509WithPrivateKey;
        case "sub": return FieldType.Subkey;
        case "sec": return FieldType.SecretKey;
        case "ssb": return FieldType.SecretSubKey;
        case "uid": return FieldType.UserId;
        case "uat": return FieldType.UserAttribute;
        case "sig": return FieldType.Signature;
        case "rev": return FieldType.RevocationSignature;
        case "rvs": return FieldType.RevocationSignatureStandalone;
        case "fpr": return FieldType.Fingerprint;
        case "pkd": return FieldType.PublicKeyData;
        case "grp": return FieldType.Keygrip;
        case "rvk": return FieldType.RevocationKey;
        case "tfs": return FieldType.TofuStatistics;
        case "tru": return FieldType.TrustDatabaseInformation;
        case "spk": return FieldType.SignatureSubpacket;
        case "cfg": return FieldType.ConfigurationData;
        default: return default;
      }
    }

  }
}
