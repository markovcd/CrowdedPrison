using System;

namespace CrowdedPrison.Encryption
{
  public enum FieldType
  {
    Unknown,
    PublicKey,
    Subkey,
    SecretKey,
    SecretSubKey,
    UserId,
    Fingerprint,
    Keygrip,
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
}
