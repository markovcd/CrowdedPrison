using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TestFacebook.Entities;
using TestFacebook.Events;

namespace TestFacebook
{
  public interface IMessenger : IAsyncDisposable
  {
    IReadOnlyDictionary<string, MessengerUser> Users { get; }
    MessengerUser Self { get; }

    event EventHandler<TwoFactorEventArgs> TwoFactorRequested;
    event EventHandler<UserLoginEventArgs> UserLoginRequested;
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
  }
}
