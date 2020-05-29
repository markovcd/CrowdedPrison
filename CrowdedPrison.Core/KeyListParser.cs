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

  public class GpgKey
  {
    public bool IsSecret { get; set; }
    public string Fingerprint { get; set; }
    public string Keygrip { get; set; }
    public DateTime CreationDate { get; set; }
    public DateTime ExpirationDate { get; set; }
    public string UserIdHash { get; set; }
    public string UserId { get; set; }
    public KeyCababilities Cababilities { get; set; }
    public PublicKeyAlgorithm Algorithm { get; set; }
    public KeyValidity Validity { get; set; }
    public int KeyLength { get; set; }
    public string KeyId { get; set; }
  }

  public class KeyListParser
  {
    public static IReadOnlyList<GpgKey> GpgKeysFromFields(IEnumerable<IEnumerable<KeyListField>> fields)
    {
      return fields.Select(GpgKeyFromFields).ToList();
    }

    public static GpgKey GpgKeyFromFields(IEnumerable<KeyListField> fields)
    {
      var key = new GpgKey();
      var addFingerprint = false;
      var addKeygrip = false;

      foreach (var field in fields)
      {
        if (field.Type == FieldType.SecretKey)
        { 
          key.IsSecret = true;
          addKeygrip = true;
        }
        
        if (field.Type == FieldType.SecretKey || field.Type == FieldType.PublicKey)
        {
          key.Algorithm = field.Algorithm;
          key.Cababilities = field.Cababilities;
          key.CreationDate = field.CreationDate;
          key.ExpirationDate = field.ExpirationDate;
          key.KeyId = field.KeyId;
          key.KeyLength = field.KeyLength;
          key.Validity = field.Validity;

          addFingerprint = true;
        }

        if (field.Type == FieldType.Fingerprint && addFingerprint)
        {
          key.Fingerprint = field.UserId;
          addFingerprint = false;
        }

        if (field.Type == FieldType.Keygrip && addKeygrip)
        {
          key.Keygrip = field.UserId;
          addKeygrip = false;
        }

        if (field.Type == FieldType.UserId)
        {
          key.UserId = field.UserId;
          key.UserIdHash = field.UserIdHash;
        }
      }

      return key;
    }

    public static IReadOnlyList<IReadOnlyList<KeyListField>> ParseFields(IReadOnlyList<IReadOnlyList<string>> l)
    {
      List<KeyListField> current = null;
      var total = new List<IReadOnlyList<KeyListField>>();

      foreach (var inner in l)
      {
        var field = ParseField(inner);
        if (field.Type == FieldType.TrustDatabaseInformation) continue;

        if (field.Type == FieldType.PublicKey || field.Type == FieldType.SecretKey)
        {
          if (current != null) total.Add(current);
          current = new List<KeyListField>();
        }

        current.Add(field);
      }

      if (current != null) total.Add(current);

      return total; 
    }

    public static KeyListField ParseField(IReadOnlyList<string> f)
    {
      var result = new KeyListField();

      result.Type = GetFieldType(GetItem(f, 0));
      result.Validity = GetValidity(GetItem(f, 1));
      result.KeyLength = GetKeyLength(GetItem(f, 2));
      result.Algorithm = GetAlgorithm(GetItem(f, 3));
      result.KeyId = GetItem(f, 4);
      result.CreationDate = GetDate(GetItem(f, 5));
      result.ExpirationDate = GetDate(GetItem(f, 6));
      result.UserIdHash = GetItem(f, 7);
      result.UserId = GetItem(f, 9);
      result.Cababilities = GetCapabilities(GetItem(f, 11));
      return result;
    }

    private static string GetItem(IReadOnlyList<string> f, int index)
    {
      if (index >= f.Count) return string.Empty;
      return f[index];
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
