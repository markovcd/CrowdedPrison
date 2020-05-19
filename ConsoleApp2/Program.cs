using CrowdedPrison.Core;
using System;
using System.Threading.Tasks;

namespace ConsoleApp2
{
  class Program
  {
    static async Task Main(string[] args)
    {
      var username = "testowy2";

      var gpg = new GpgWrapper(() => new AsyncProcess(), new FileSystem());

      var result = await gpg.RunCommandAsync($"--delete-secret-key {username}");

      Console.WriteLine(result);
    }
  }
}
