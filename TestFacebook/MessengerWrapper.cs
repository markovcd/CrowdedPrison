using fbchat_sharp.API;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace TestFacebook
{
  public class MessengerWrapper : IMessenger
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
        On2FACodeCallback = GetTwoFactorCode,
        OnMessageReceivedCallback = OnMessageReceived
      };
    }

    private void OnMessageReceived(FB_MessageEvent messageArgs)
    {
      if (!Users.TryGetValue(messageArgs.author.uid, out var user))
        user = new MessengerUser(messageArgs.author);

      var message = new MessengerMessage(messageArgs.message);

      OnMessageReceived(user, message);
    }

    protected virtual void OnMessageReceived(MessengerUser user, MessengerMessage message)
    {
      var args = new MessageReceivedEventArgs(user, message);
      MessageReceived?.Invoke(this, args);
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
        session = await messenger.DoLogin(email, password) 
                  ?? await messenger.DoLogin(email, password);
      }

      if (session == null) return false;
      var isLoggedIn = await session.is_logged_in();
      if (!isLoggedIn) return false;

      await messenger.StartListening();

      return true;
    }

    public async Task LogoutAsync()
    {
      await messenger.StopListening();
      await messenger.DoLogout();
    }

    public async Task UpdateUsersAsync()
    {
      var users = await messenger.fetchUsers();
      Users = users.Select(u => new MessengerUser(u)).ToImmutableDictionary(u => u.Id);
    }

    public async Task UpdateActiveUsersAsync()
    {
      if (Users == null) await UpdateUsersAsync();

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
