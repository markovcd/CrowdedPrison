using CrowdedPrison.Messenger.Entities;
using CrowdedPrison.Messenger.Events;

namespace CrowdedPrison.Messenger.Encryption.Events
{
  public class EncryptedMessageReceivedEventArgs : MessageReceivedEventArgs
  {
    public string DecryptedText { get; }

    public EncryptedMessageReceivedEventArgs(string decryptedText, MessengerUser user, MessengerMessage message, MessengerMessage replyTo = null)
      : base(user, message, replyTo)
    {
      DecryptedText = decryptedText;
    }
  }
}
