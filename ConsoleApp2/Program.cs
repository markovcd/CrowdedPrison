using CrowdedPrison.Core;
using System;
using System.Threading.Tasks;

namespace ConsoleApp2
{
  class Program
  {
    static async Task Main(string[] args)
    {
      var username = "testowy";

      var gpg = new GpgWrapper(() => new AsyncProcess(), new FileSystem());

      var encrypted = await gpg.EncryptAsync("czesc", username);
      var decrypted = await gpg.DecryptAsync(encrypted);

      Console.WriteLine(encrypted);
      Console.WriteLine(decrypted);
    }
  }
}
