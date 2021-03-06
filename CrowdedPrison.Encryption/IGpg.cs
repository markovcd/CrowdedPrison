﻿using System.Collections.Generic;
using System.Threading.Tasks;

namespace CrowdedPrison.Encryption
{
  public interface IGpg
  {
    Task<string> EncryptAsync(string text, string name);
    Task<string> DecryptAsync(string text, string password);
    Task<bool> ImportKeyAsync(string key);
    Task<string> ExportKeyAsync(string name);
    Task<bool> GenerateKeyAsync(string name, string password);
    Task<bool> KeyExistsAsync(string name, bool secret = false);
    Task<IReadOnlyList<IReadOnlyList<string>>> ListKeysAsync(bool secret = false);
  }
}