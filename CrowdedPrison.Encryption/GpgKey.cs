using System;

namespace CrowdedPrison.Encryption
{
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
    public int Length { get; set; }
    public string Id { get; set; }
  }
}
