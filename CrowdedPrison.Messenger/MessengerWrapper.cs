using fbchat_sharp.API;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using CrowdedPrison.Messenger.Entities;
using CrowdedPrison.Messenger.Events;
using System.Net;
using System.IO;
using CrowdedPrison.Common;

namespace CrowdedPrison.Messenger
{
  internal class MessengerWrapper : IMessenger
  {
    private const string sessionFileName = "cookies.dat";
    private readonly IMessengerConfiguration configuration;
    private readonly IFileSerializer serializer;
    private readonly BaseClient messenger;
    private Session session;

    private string SessionFilePath 
    {
      get
      {
        Directory.CreateDirectory(configuration.HomeDir);
        return Path.Combine(configuration.HomeDir, sessionFileName);
      }
    }

    public event AsyncEventHandler<TwoFactorEventArgs> TwoFactorRequested;
    public event AsyncEventHandler<UserLoginEventArgs> UserLoginRequested;
    public event EventHandler<MessageReceivedEventArgs> MessageReceived;
    public event EventHandler<ConnectionStateEventArgs> ConnectionStateChanged;
    public event EventHandler<MessagesDeliveredEventArgs> MessagesDelivered;
    public event EventHandler<MessageUnsentEventArgs> MessageUnsent;
    public event EventHandler<TypingEventArgs> Typing;

    public IReadOnlyDictionary<string, MessengerUser> Users { get; private set; }
    public MessengerUser Self { get; private set; }

    public MessengerWrapper(IMessengerConfiguration configuration, IFileSerializer serializer)
    {
      this.configuration = configuration;
      this.serializer = serializer;

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
        session = await messenger.DoLogin(email, password);
      }

      if (!await CheckConnectionStateAsync())
        return false;

      await messenger.StartListening();
      
      Self = new MessengerUser(await messenger.fetchProfile());
      await UpdateUsersAsync();

      return true;
    }

    public async Task<bool> SendTextAsync(string threadId, string message)
    {
      var thread = new FB_Thread(threadId, session);
      var msgId = await thread.sendText(message);
      return !string.IsNullOrEmpty(msgId);
    }


    public async Task<bool> SendTextAsync(MessengerThread thread, string message)
    {
      return await SendTextAsync(thread.Id, message);
    }

    public async Task<bool> SendTextAsync(MessengerUser user, string message)
    {
      return await SendTextAsync(user.Id, message);
    }

    public async Task<IReadOnlyList<MessengerMessage>> SearchThread(string threadId, string query, int limit = 5)
    {
      var thread = new FB_Thread(threadId, session);
      var m = await thread.searchMessages(query, offset: 0, limit);
      var m2 = m.ToImmutableList();
      return m2.Select(s => new MessengerMessage(s)).ToImmutableList();
    }

    public async Task<IReadOnlyList<MessengerMessage>> SearchThread(MessengerThread thread, string query, int limit = 5)
    {
      return await SearchThread(thread.Id, query, limit);
    }

    public async Task<IReadOnlyList<MessengerMessage>> SearchThread(MessengerUser user, string query, int limit = 5)
    {
      return await SearchThread(user.Id, query, limit);
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

    public async Task<IReadOnlyList<MessengerThread>> GetThreadsAsync(int limit = 20)
    {
      var threads = await messenger.fetchThreadList(limit);
      var threads2 = threads.Select(t => new MessengerThread(t)).ToImmutableList();
      return threads2;
    }

    public MessengerUser GetUser(MessengerMessage message)
    {
      return GetUser(message.AuthorId);
    }

    public async Task<IReadOnlyList<MessengerThread>> GetThreadsAsync(IEnumerable<string> threadIds)
    {
      var fbThreads = await messenger.fetchThreadInfo(threadIds.ToList());
      return fbThreads.Values.Select(t => new MessengerThread(t)).ToImmutableList();
    }

    public async Task<MessengerThread> GetThreadAsync(string threadId)
    {
      return (await GetThreadsAsync(new[] { threadId })).FirstOrDefault();
    }

    public async Task<MessengerThread> GetThreadAsync(MessengerMessage message)
    {
      return await GetThreadAsync(message.ThreadId);
    }

    public async Task<IReadOnlyList<string>> GetUnreadThreadIds()
    {
      return await messenger.fetchUnread();
    }

    public async Task<IReadOnlyList<MessengerMessage>> GetMessagesAsync(string threadId, int limit = 20)
    {
      var thread2 = new FB_Thread(threadId, session);
      var messages = await thread2.fetchMessages(limit);
      var messages2 = messages.Select(m => new MessengerMessage(m)).ToImmutableList();
      return messages2;
    }

    public async Task<IReadOnlyList<MessengerMessage>> GetMessagesAsync(MessengerThread thread, int limit = 20)
    {
      return await GetMessagesAsync(thread.Id, limit);
    }

    public async Task<IReadOnlyList<MessengerMessage>> GetMessagesAsync(MessengerUser user, int limit = 20)
    {
      return await GetMessagesAsync(user.Id, limit);
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
      await serializer.SerializeAsync(cookieJar, SessionFilePath);
    }

    private async Task<Dictionary<string, List<Cookie>>> OnReadCookiesFromDiskCallback()
    {
      return await serializer.DeserializeAsync<Dictionary<string, List<Cookie>>>(SessionFilePath);
    }

    private async Task OnDeleteCookiesCallback()
    {
      try
      {
        await Task.Yield();
        File.Delete(SessionFilePath);
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
        user.LastActive = fbStatus.last_active.FromUnixEpoch();
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
      var at = messagesDeliveredEvent.at.FromUnixEpoch();
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
