using fbchat_sharp.API;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace TestFacebook
{
  public class MessengerWrapper : IMessengerWrapper, IAsyncDisposable
  {
    private readonly FBClient_Cookies messenger;
    private Session session;

    public event EventHandler<TwoFactorEventArgs> TwoFactorRequested;
    public event EventHandler<UserLoginEventArgs> UserLoginRequested;
    public event EventHandler<MessageReceivedEventArgs> MessageReceived;

    public IReadOnlyDictionary<string, MessengerUser> Users { get; private set; }

    public MessengerWrapper()
    {
      messenger = new FBClient_Cookies
      {
        On2FACodeCallback = GetTwoFactorCode
      };
    }

    private async Task<string> GetTwoFactorCode()
    {
      await Task.Yield();
      return OnTwoFactorRequested();
    }

    protected string OnTwoFactorRequested()
    {
      var args = new TwoFactorEventArgs();
      TwoFactorRequested?.Invoke(this, args);
      return args.TwoFactorCode;
    }

    protected (string email, string password) OnUserLoginRequested()
    {
      var args = new UserLoginEventArgs();
      UserLoginRequested?.Invoke(this, args);
      return (args.Email, args.Password);
    }

    public async Task<bool> LoginAsync()
    {
      session = await messenger.TryLogin();
      if (session == null)
      {
        var (email, password) = OnUserLoginRequested();
        session = await messenger.DoLogin(email, password);
        if (session == null) session = await messenger.DoLogin(email, password);
      }

      if (session == null) return false;
      var isLoggedIn = await session.is_logged_in();
      if (!isLoggedIn) return false;

      _ = UpdateUsersAsync();
      _ = messenger.StartListening();

      return true;
    }

    public async Task Logout()
    {
      await messenger.DoLogout();
    }

    public async Task UpdateUsersAsync()
    {
      var users = await messenger.fetchUsers();
      Users = users.Select(u => new MessengerUser(u)).ToImmutableDictionary(u => u.Id);
      await UpdateActiveUsersAsync();
    }

    public async Task UpdateActiveUsersAsync()
    {
      if (Users == null) return;

      var activeIds = new HashSet<string>(await messenger.fetchActiveUsers());

      foreach (var user in Users.Values)
      {
        user.IsActive = activeIds.Contains(user.Id);
      }
    }

    public async ValueTask DisposeAsync()
    {
      await messenger.StopListening();
    }
  }
}
