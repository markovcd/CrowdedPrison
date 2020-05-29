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
      IMessenger messenger = new MessengerWrapper();

      messenger.MessageReceived += Messenger_MessageReceived;
      messenger.TwoFactorRequested += Messenger_TwoFactorRequested;
      messenger.UserLoginRequested += Messenger_UserLoginRequested;

      if (!await messenger.LoginAsync())
      {
        Console.WriteLine("Login failed");
        return;
      }

      await messenger.SendTextAsync(messenger.Self, "eLOOOOOOOOOOOOO");
      Console.WriteLine("Listening... Press Ctrl+C to exit.");
      Console.CancelKeyPress += (s, e) => { e.Cancel = true; _closing.Set(); };
      _closing.WaitOne();
      await messenger.DisposeAsync();
    }

    private static void Messenger_UserLoginRequested(object sender, UserLoginEventArgs e)
    {
      e.Email = "markovcd@gmail.com";
      e.Password = "";
      Console.WriteLine($"Login as {e.Email}");
    }

    private static void Messenger_TwoFactorRequested(object sender, TwoFactorEventArgs e)
    {
      Console.WriteLine("Enter 2FA:");
      e.TwoFactorCode = Console.ReadLine();
    }

    private static void Messenger_MessageReceived(object sender, MessageReceivedEventArgs e)
    {
      Console.WriteLine($"{e.User.Id} {e.User.Name} {e.Message.Text}");
    }
  }
}
