using fbchat_sharp.API;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using CrowdedPrison.Messenger.Entities;
using CrowdedPrison.Messenger.Events;
using System.Net;
using Newtonsoft.Json;
using System.IO;

namespace CrowdedPrison.Messenger
{
  internal class MessengerWrapper : IMessenger
  {
    private const string appName = "FBChat-Sharp";
    private const string sessionFile = "SESSION_COOKIES_core.dat";

    private readonly BaseClient messenger;
    private Session session;

    public event AsyncEventHandler<TwoFactorEventArgs> TwoFactorRequested;
    public event AsyncEventHandler<UserLoginEventArgs> UserLoginRequested;
    public event EventHandler<MessageReceivedEventArgs> MessageReceived;
    public event EventHandler<ConnectionStateEventArgs> ConnectionStateChanged;
    public event EventHandler<MessagesDeliveredEventArgs> MessagesDelivered;
    public event EventHandler<MessageUnsentEventArgs> MessageUnsent;
    public event EventHandler<TypingEventArgs> Typing;

    public IReadOnlyDictionary<string, MessengerUser> Users { get; private set; }
    public MessengerUser Self { get; private set; }

    public string UserDataFolder
    {
      get
      {
        string folderBase = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        string dir = Path.Combine(folderBase, appName.ToUpper());
        Directory.CreateDirectory(dir);
        return dir;
      }
    }

    public MessengerWrapper()
    {
      messenger = new BaseClient
      {
        On2FACodeCallback = OnTwoFactorRequestedAsync,
        FbEventCallback = OnFbEventCallback,
        DeleteCookiesCallback = OnDeleteCookiesCallback,
        ReadCookiesFromDiskCallback = OnReadCookiesFromDiskCallback,
        WriteCookiesToDiskCallback = OnWriteCookiesToDiskCallback
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
        var (email, password) = await OnUserLoginRequestedAsync();
        session = await messenger.DoLogin(email, password) 
                  ?? await messenger.DoLogin(email, password);
      }

      if (!await CheckConnectionStateAsync())
        return false;

      await messenger.StartListening();
      
      Self = new MessengerUser(await messenger.fetchProfile());
      await UpdateUsersAsync();

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

    public async Task<IReadOnlyList<MessengerThread>> GetThreadsAsync()
    {
      var threads = await messenger.fetchThreadList();
      var threads2 = threads.Select(t => new MessengerThread(t)).ToImmutableList();
      return threads2;
    }

    public async Task<IReadOnlyList<MessengerMessage>> GetMessagesAsync(MessengerThread thread)
    {
      var thread2 = new FB_Thread(thread.Id, session);
      var messages = await thread2.fetchMessages();
      var messages2 = messages.Select(m => new MessengerMessage(m)).ToImmutableList();
      return messages2;
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

    protected virtual void OnMessageUnsent(MessengerUser user, MessengerMessage message, MessengerThread thread, DateTime at)
    {
      var args = new MessageUnsentEventArgs(user, message, thread, at);
      MessageUnsent?.Invoke(this, args);
    }

    protected virtual void OnMessagesDelivered(MessengerUser user, IReadOnlyList<MessengerMessage> messages, MessengerThread thread, DateTime at)
    {
      var args = new MessagesDeliveredEventArgs(user, messages, thread, at);
      MessagesDelivered?.Invoke(this, args);
    }

    protected virtual void OnConnectionStateChanged(MessengerConnectionState state, string email = null, string reason = null)
    {
      var args = new ConnectionStateEventArgs(state, email, reason);
      ConnectionStateChanged?.Invoke(this, args);
    }

    protected virtual void OnMessageReceived(MessengerUser user, MessengerMessage message, MessengerMessage replyTo = null)
    {
      var args = new MessageReceivedEventArgs(user, message, replyTo);
      MessageReceived?.Invoke(this, args);
    }

    protected virtual async Task<string> OnTwoFactorRequestedAsync()
    {
      var args = new TwoFactorEventArgs();
      if (TwoFactorRequested != null)
        await TwoFactorRequested(this, args);

      if (args.IsCancelled) throw new OperationCanceledException();
      return args.TwoFactorCode;
    }

    protected virtual async Task<(string email, string password)> OnUserLoginRequestedAsync()
    {
      var args = new UserLoginEventArgs();
      if (UserLoginRequested != null)
        await UserLoginRequested(this, args);

      if (args.IsCancelled) throw new OperationCanceledException();
      return (args.Email, args.Password);
    }

    private MessengerUser GetUser(string id)
    {
      if (Users.TryGetValue(id, out var user))
        return user;

      return Self?.Id == id 
        ? Self 
        : null;
    }

    private MessengerUser GetUser(FB_User author)
    {
      if (author == null) return null;

      return GetUser(author.uid) ?? new MessengerUser(author);      
    }

    private async Task OnWriteCookiesToDiskCallback(Dictionary<string, List<Cookie>> cookieJar)
    {
      var file = Path.Combine(UserDataFolder, sessionFile);

      await using var fileStream = File.Create(file);
      try
      {
        using var jsonWriter = new JsonTextWriter(new StreamWriter(fileStream));
        var serializer = new JsonSerializer();
        serializer.Serialize(jsonWriter, cookieJar);
        await jsonWriter.FlushAsync();
      }
      catch
      {

      }
    }

    private async Task<Dictionary<string, List<Cookie>>> OnReadCookiesFromDiskCallback()
    {
      try
      {
        var file = Path.Combine(UserDataFolder, sessionFile);
        await using var fileStream = File.OpenRead(file);
        using var jsonTextReader = new JsonTextReader(new StreamReader(fileStream));
        var serializer = new JsonSerializer();
        return serializer.Deserialize<Dictionary<string, List<Cookie>>>(jsonTextReader);
      }
      catch
      {
        return null;
      }
    }

    private async Task OnDeleteCookiesCallback()
    {
      try
      {
        await Task.Yield();
        var file = Path.Combine(UserDataFolder, sessionFile);
        File.Delete(file);
      }
      catch
      {
      }
    }

    private void OnFbEventCallback(FB_Event ev)
    {
      switch (ev)
      {
        case FB_MessageEvent messageEvent:
          OnMessageReceived(messageEvent);
          break;
        case FB_MessageReplyEvent messageReplyEvent:
          OnMessageReply(messageReplyEvent);
          break;
        case FB_MessagesDelivered messagesDeliveredEvent:
          OnMessagesDelivered(messagesDeliveredEvent);
          break;
        case FB_Connect _:
          OnConnected();
          break;
        case FB_Disconnect disconnectEvent:
          OnDisconnected(disconnectEvent);
          break;
        case FB_LoggedIn loggedInEvent:
          OnLoggedIn(loggedInEvent);
          break;
        case FB_LoggedOut _:
          OnLoggedOut();
          break;
        case FB_LoggingIn loggingInEvent:
          OnLoggingIn(loggingInEvent);
          break;
        case FB_Presence presenceEvent:
          OnPresence(presenceEvent);
          break;
        case FB_TypingStatus typingEvent:
          OnTyping(typingEvent);
          break;
        case FB_UnsendEvent unsendEvent:
          OnMessageUnsent(unsendEvent);
          break;
      }
    }

    private void OnMessageUnsent(FB_UnsendEvent unsendEvent)
    {
      var user = GetUser(unsendEvent.author);
      var message = new MessengerMessage(unsendEvent.message);
      var thread = new MessengerThread(unsendEvent.thread);
      var at = DateTimeOffset.FromUnixTimeSeconds(unsendEvent.at).DateTime;
      OnMessageUnsent(user, message, thread, at);
    }

    private void OnTyping(FB_TypingStatus typingEvent)
    {
      var user = GetUser(typingEvent.author);
      var thread = new MessengerThread(typingEvent.thread);
      OnTyping(user, thread, typingEvent.status);
    }

    protected virtual void OnTyping(MessengerUser user, MessengerThread thread, bool isTyping)
    {
      var args = new TypingEventArgs(user, thread, isTyping);
      Typing?.Invoke(this, args);
    }

    private void OnPresence(FB_Presence presenceEvent)
    {
      foreach (var (fbUser, fbStatus) in presenceEvent.statuses)
      {
        var user = GetUser(fbUser);
        if (user == null) continue;
        user.IsActive = fbStatus.active;
        user.LastActive = fbStatus.last_active;
      }
    }

    private void OnLoggingIn(FB_LoggingIn loggingInEvent)
    {
      OnConnectionStateChanged(MessengerConnectionState.LoggingIn, loggingInEvent.Email);
    }

    private void OnLoggedOut()
    {
      OnConnectionStateChanged(MessengerConnectionState.LoggedOut);
    }

    private void OnLoggedIn(FB_LoggedIn loggedInEvent)
    {
      OnConnectionStateChanged(MessengerConnectionState.LoggedIn, loggedInEvent.Email);
    }

    private void OnDisconnected(FB_Disconnect disconnectEvent)
    {
      OnConnectionStateChanged(MessengerConnectionState.Disconnected, reason: disconnectEvent.Reason);
    }

    private void OnConnected()
    {
      OnConnectionStateChanged(MessengerConnectionState.Connected);
    }

    private void OnMessagesDelivered(FB_MessagesDelivered messagesDeliveredEvent)
    {
      var user = GetUser(messagesDeliveredEvent.author);
      var messages = messagesDeliveredEvent.messages?.Select(m => new MessengerMessage(m)).ToImmutableList();
      var thread = new MessengerThread(messagesDeliveredEvent.thread);
      var at = DateTimeOffset.FromUnixTimeSeconds(messagesDeliveredEvent.at).DateTime;
      OnMessagesDelivered(user, messages, thread, at);
    }

    private void OnMessageReply(FB_MessageReplyEvent messageReplyEvent)
    {
      var user = GetUser(messageReplyEvent.author);
      var message = new MessengerMessage(messageReplyEvent.message);
      var replyTo = new MessengerMessage(messageReplyEvent.replied_to);
      OnMessageReceived(user, message, replyTo);
    }

    private void OnMessageReceived(FB_MessageEvent messageArgs)
    {
      var user = GetUser(messageArgs.author);

      var message = new MessengerMessage(messageArgs.message);

      OnMessageReceived(user, message);
    }
  }
}
