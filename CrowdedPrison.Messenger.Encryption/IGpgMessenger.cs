using System;
using System.Threading.Tasks;
using CrowdedPrison.Messenger.Encryption.Events;
using CrowdedPrison.Messenger.Entities;

namespace CrowdedPrison.Messenger.Encryption
{
  public interface IGpgMessenger
  {
    event EventHandler<EncryptedMessageReceivedEventArgs> EncryptedMessageReceived;
    Task<bool> GeneratePrivateKeyAsync();
    Task<bool> IsPrivateKeyPresentAsync();
    Task<bool> IsKeyPresentAsync(MessengerUser user);
    Task<string> GetPublicKeyAsync(MessengerUser user);
    Task<bool> SendPublicKeyAsync(MessengerUser user);
    Task<bool> ImportPublicKeyAsync(MessengerUser user);
    Task<bool> SendEncryptedTextAsync(MessengerUser user, string text);
  }
}
