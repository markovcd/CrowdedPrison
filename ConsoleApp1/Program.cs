using System;
using System.Threading.Tasks;

namespace ConsoleApp1
{
  class Program
  {
    static async Task Main(string[] args)
    {
      for (var i = 0; i < 10; i++)
      {
        await Task.Delay(500);
        Console.WriteLine(i);
        var s = Console.ReadLine();
        Console.WriteLine(s);
      }
    }
  }
}
