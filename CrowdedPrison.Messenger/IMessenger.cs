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
    IReadOnlyDictionary<string, MessengerUser> Users { get; }
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
    Task UpdateActiveUsersAsync();
    Task<bool> SendTextAsync(string userId, string message);
    Task<bool> SendTextAsync(MessengerUser user, string message);
    Task<bool> CheckConnectionStateAsync();
    Task<IReadOnlyList<MessengerThread>> GetThreadsAsync();
    Task<IReadOnlyList<MessengerMessage>> GetMessagesAsync(MessengerThread thread);
  }
}
