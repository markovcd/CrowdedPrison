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
    public MessengerUser Self { get; private set; }

    public MessengerWrapper()
    {
      messenger = new FBClient_Cookies
      {
        On2FACodeCallback = GetTwoFactorCode,
        OnMessageReceivedCallback = OnMessageReceived
      };
    }

    public async Task<bool> CheckConnectionStateAsync()
    {
      if (session == null) return false;
      return await session.is_logged_in();
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

      if (!await CheckConnectionStateAsync())
        return false;

      await messenger.StartListening();
      
      Self = new MessengerUser(await messenger.fetchProfile());
      await UpdateActiveUsersAsync();

      return true;
    }

    public async Task<bool> SendTextAsync(string userId, string message)
    {
      var thread = new FB_Thread(userId, session);
      var msgId = await thread.sendText(message);
      return !string.IsNullOrEmpty(msgId);
    }

    public async Task<bool> SendTextAsync(MessengerUser user, string message)
    {
      return await SendTextAsync(user.Id, message);
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

    protected virtual void OnMessageReceived(MessengerUser user, MessengerMessage message)
    {
      var args = new MessageReceivedEventArgs(user, message);
      MessageReceived?.Invoke(this, args);
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

    private void OnMessageReceived(FB_MessageEvent messageArgs)
    {
      if (!Users.TryGetValue(messageArgs.author.uid, out var user))
      {
        user = messageArgs.author.uid == Self?.Id 
          ? Self 
          : new MessengerUser(messageArgs.author);
      }

      var message = new MessengerMessage(messageArgs.message);

      OnMessageReceived(user, message);
    }

    private async Task<string> GetTwoFactorCode()
    {
      await Task.Yield();
      return OnTwoFactorRequested();
    }

  }
}
