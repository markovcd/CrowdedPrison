using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TestFacebook
{
  public static class Program
  {
    private static readonly AutoResetEvent _closing = new AutoResetEvent(false);
    public static async Task Main()
    {
      var messenger = new FBClient_Cookies
      {
        On2FACodeCallback = get2FACode
      };
      var s = await messenger.TryLogin();
      if (s == null) s = await messenger.DoLogin("markovcd@gmail.com", "oyM7kIE4JVd6");
      Console.WriteLine(await s.is_logged_in());

      var users = await messenger.fetchUsers();
      await messenger.StartListening();

      Console.WriteLine("Listening... Press Ctrl+C to exit.");
      Console.CancelKeyPress += new ConsoleCancelEventHandler((s, e) => { e.Cancel = true; _closing.Set(); });
      _closing.WaitOne();
      await messenger.StopListening();

    }

    private static async Task<string> get2FACode()
    {
      await Task.Yield();
      Console.WriteLine("Insert 2FA code:");
      return Console.ReadLine();
    }

  }
}
