using CrowdedPrison.Core;
using System;
using System.Threading.Tasks;

namespace ConsoleApp2
{
  class Program
  {
    static async Task Main(string[] args)
    {
      IGpg gpg = new GpgWrapper(() => new AsyncProcess(), new FileSystem())
      {
        HomeDir = @"C:\Users\armw\Desktop\gpg\home"
      };

      var user = "siurek";
      var password = "mariusz";
      var message = "pu pu pu pu ";

      await gpg.GenerateKeyAsync(user, password);
      var encrypted = await gpg.EncryptAsync(message, user);
      var decrypted = await gpg.DecryptAsync(encrypted, password);
      
      Console.WriteLine(decrypted);
    }
  }
}
