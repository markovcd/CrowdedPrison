using System;
using System.Threading.Tasks;
using CrowdedPrison.Messenger.Encryption.Events;
using CrowdedPrison.Messenger.Entities;

namespace CrowdedPrison.Messenger.Encryption
{
  public interface IGpgMessenger : IMessenger
  {
    event EventHandler<EncryptedMessageReceivedEventArgs> EncryptedMessageReceived;
    string Password { get; set; }
    Task<bool> GeneratePrivateKey();
    Task<string> GetPublicKeyAsync(MessengerUser user);
    Task<bool> SendPublicKeyAsync(MessengerUser user);
    Task<bool> ImportPublicKeyAsync(MessengerUser user);
    Task<bool> SendEncryptedTextAsync(MessengerUser user, string text);
  }
}
