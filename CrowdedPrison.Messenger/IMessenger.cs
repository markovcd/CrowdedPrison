using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CrowdedPrison.Messenger.Entities;
using CrowdedPrison.Messenger.Events;

namespace CrowdedPrison.Messenger
{
  public delegate Task AsyncEventHandler<in TEventArgs>(object sender, TEventArgs e) where TEventArgs : EventArgs;

  public interface IMessenger : IAsyncDisposable
  {
    IReadOnlyList<MessengerUser> Users { get; }
    MessengerUser Self { get; }

    event AsyncEventHandler<TwoFactorEventArgs> TwoFactorRequested;
    event AsyncEventHandler<UserLoginEventArgs> UserLoginRequested;
    event EventHandler<MessageReceivedEventArgs> MessageReceived;
    event EventHandler<ConnectionStateEventArgs> ConnectionStateChanged;
    event EventHandler<MessagesDeliveredEventArgs> MessagesDelivered;
    event EventHandler<MessageUnsentEventArgs> MessageUnsent;
    event EventHandler<TypingEventArgs> Typing;

    Task<bool> LoginAsync();
    Task LogoutAsync();
    Task UpdateUsersAsync();
    Task<bool> SendTextAsync(string threadId, string message);
    Task<bool> SendTextAsync(MessengerUser user, string message);
    Task<bool> SendTextAsync(MessengerThread thread, string message);
    Task<bool> CheckConnectionStateAsync();
    Task<IReadOnlyList<MessengerThread>> GetThreadsAsync(int limit = 20);
    Task<MessengerThread> GetThreadAsync(string threadId);
    Task<MessengerThread> GetThreadAsync(MessengerMessage message);
    Task<IReadOnlyList<MessengerThread>> GetThreadsAsync(IEnumerable<string> threadIds);
    Task<IReadOnlyList<MessengerMessage>> GetMessagesAsync(string threadId, int limit = 20);
    Task<IReadOnlyList<MessengerMessage>> GetMessagesAsync(MessengerUser user, int limit = 20);
    Task<IReadOnlyList<MessengerMessage>> GetMessagesAsync(MessengerThread thread, int limit = 20);
    Task<IReadOnlyList<MessengerMessage>> SearchThreadAsync(string threadId, string query, int limit = 5);
    Task<IReadOnlyList<MessengerMessage>> SearchThreadAsync(MessengerThread thread, string query, int limit = 5);
    Task<IReadOnlyList<MessengerMessage>> SearchThreadAsync(MessengerUser user, string query, int limit = 5);
    Task<IReadOnlyList<MessengerThread>> SearchThreadsAsync(string name, int limit = 1);
    Task<IReadOnlyList<MessengerUser>> SearchUsersAsync(string name, int limit = 10);
    Task<IReadOnlyList<MessengerUser>> GetUsersAsync(IEnumerable<MessengerThread> threads);
    MessengerUser GetUser(MessengerMessage message);
    Task<IReadOnlyList<string>> GetUnreadThreadIds();
  }
}
