using CrowdedPrison.Common;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace CrowdedPrison.Encryption
{
  internal class KeyListParser
  {
    public static IReadOnlyList<GpgKey> GpgKeysFromFields(IEnumerable<IEnumerable<KeyListField>> fields)
    {
      return fields.Select(GpgKeyFromFields).ToImmutableList();
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
          key.Id = field.KeyId;
          key.Length = field.KeyLength;
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
        if (field.Type == FieldType.Unknown) continue;

        if (field.Type == FieldType.PublicKey || field.Type == FieldType.SecretKey)
        {
          if (current != null) total.Add(current);
          current = new List<KeyListField>();
        }

        current?.Add(field);
      }

      if (current != null) total.Add(current);

      return total; 
    }

    public static KeyListField ParseField(IReadOnlyList<string> f)
    {
      var result = new KeyListField
      {
        Type = GetFieldType(GetItem(f, 0)),
        Validity = GetValidity(GetItem(f, 1)),
        KeyLength = GetItem(f, 2).ToInteger(),
        Algorithm = (PublicKeyAlgorithm)GetItem(f, 3).ToInteger(),
        KeyId = GetItem(f, 4),
        CreationDate = GetItem(f, 5).FromUnixEpoch(),
        ExpirationDate = GetItem(f, 6).FromUnixEpoch(),
        UserIdHash = GetItem(f, 7),
        UserId = GetItem(f, 9),
        Cababilities = GetCapabilities(GetItem(f, 11))
      };

      return result;
    }

    private static string GetItem(IReadOnlyList<string> f, int index)
    {
      return index >= f.Count
        ? string.Empty 
        : f[index];
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
      return s switch
      {
        "pub" => FieldType.PublicKey,
        "sub" => FieldType.Subkey,
        "sec" => FieldType.SecretKey,
        "ssb" => FieldType.SecretSubKey,
        "uid" => FieldType.UserId,
        "fpr" => FieldType.Fingerprint,
        "grp" => FieldType.Keygrip,
        _ => default,
      };
    }

  }
}
