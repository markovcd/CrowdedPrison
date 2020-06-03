using System;
using System.Linq;
using System.Security;
using System.Threading.Tasks;
using CrowdedPrison.Encryption;
using CrowdedPrison.Messenger.Encryption.Events;
using CrowdedPrison.Messenger.Entities;

namespace CrowdedPrison.Messenger.Encryption
{
  public class GpgMessenger : IGpgMessenger
  {
    private readonly IGpg gpg;
    private readonly IPgpRegexHelper pgpHelper;
    private readonly IMessenger messenger;
    private readonly IGpgMessengerConfiguration configuration;
    private readonly ITwoWayEncryption encryption;

    public event EventHandler<EncryptedMessageReceivedEventArgs> EncryptedMessageReceived;


    public GpgMessenger(IGpg gpg, IPgpRegexHelper pgpHelper, IMessenger messenger, IGpgMessengerConfiguration configuration,
      ITwoWayEncryption encryption)
    { 
      this.gpg = gpg;
      this.pgpHelper = pgpHelper;
      this.messenger = messenger;
      this.configuration = configuration;
      this.encryption = encryption;

      messenger.MessageReceived += Messenger_MessageReceived;
    }

    public async Task<bool> GeneratePrivateKey()
    {
      return await gpg.GenerateKeyAsync(messenger.Self.Id, GetGpgPassword());
    }

    private string GetGpgPassword()
    {
      if (string.IsNullOrEmpty(configuration.GpgPasswordHash)) throw new InvalidOperationException("Gpg password is missing.");

      return encryption.Decrypt(configuration.GpgPasswordHash);
    }

    public async Task<string> GetPublicKeyAsync(MessengerUser user)
    {
      const string pattern = "-----BEGIN PGP PUBLIC KEY BLOCK----- -----END PGP PUBLIC KEY BLOCK-----";
      var messages = await messenger.SearchThread(user, pattern, 50);
      
      return messages
        .OrderByDescending(m => m.Timestamp)
        .Select(m => pgpHelper.GetPublicKeyBlock(m.Text))
        .FirstOrDefault(s => !string.IsNullOrEmpty(s));
    }

    public async Task<bool> SendPublicKeyAsync(MessengerUser user)
    {
      var publicKey = await gpg.ExportKeyAsync(messenger.Self.Id);
      return await messenger.SendTextAsync(user, publicKey);
    }

    public async Task<bool> ImportPublicKeyAsync(MessengerUser user)
    {
      var publicKey = await GetPublicKeyAsync(user);
      if (string.IsNullOrEmpty(publicKey)) return false;
      return await gpg.ImportKeyAsync(publicKey);
    }

    public async Task<bool> SendEncryptedTextAsync(MessengerUser user, string text)
    {
      var encrypted = await gpg.EncryptAsync(text, user.Id);
      return await messenger.SendTextAsync(user, encrypted);
    }

    private async void Messenger_MessageReceived(object sender, Messenger.Events.MessageReceivedEventArgs e)
    {
      var encrypted = pgpHelper.GetMessageBlock(e.Message.Text);
      if (string.IsNullOrEmpty(encrypted)) 
        return;

      var decrypted = await gpg.DecryptAsync(encrypted, GetGpgPassword());
      if (!string.IsNullOrEmpty(decrypted))
        OnEncryptedMessageReceived(decrypted, e.User, e.Message, e.ReplyTo);
    }

    protected virtual void OnEncryptedMessageReceived(string decrypted, MessengerUser user, MessengerMessage message, MessengerMessage replyTo = null)
    {
      var args = new EncryptedMessageReceivedEventArgs(decrypted, user, message, replyTo);
      EncryptedMessageReceived?.Invoke(this, args);
    }
  }
}
