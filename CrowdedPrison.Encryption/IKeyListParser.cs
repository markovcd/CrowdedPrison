using System.Collections.Generic;

namespace CrowdedPrison.Encryption
{
  public interface IKeyListParser
  {
    IReadOnlyList<GpgKey> GpgKeysFromData(IReadOnlyList<IReadOnlyList<string>> l);
  }
}
