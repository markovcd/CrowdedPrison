using System;
using System.Collections.Generic;
using System.Linq;
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
    private readonly IKeyListParser keyListParser;
    public event EventHandler<EncryptedMessageReceivedEventArgs> EncryptedMessageReceived;


    public GpgMessenger(IGpg gpg, IPgpRegexHelper pgpHelper, IMessenger messenger, IGpgMessengerConfiguration configuration,
      ITwoWayEncryption encryption, IKeyListParser keyListParser)
    { 
      this.gpg = gpg;
      this.pgpHelper = pgpHelper;
      this.messenger = messenger;
      this.configuration = configuration;
      this.encryption = encryption;
      this.keyListParser = keyListParser;

      messenger.MessageReceived += Messenger_MessageReceived;
    }

    public async Task<bool> GeneratePrivateKeyAsync()
    {
      return await gpg.GenerateKeyAsync(messenger.Self.Id, GetGpgPassword());
    }

    public async Task<bool> IsPrivateKeyPresentAsync()
    {
      var key = await GetPrivateKeyAsync();
      return key != null;  
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

    private async Task<IReadOnlyList<GpgKey>> GetKeysAsync(bool secret = false)
    {
      var data = await gpg.ListKeysAsync(secret);
      return keyListParser.GpgKeysFromData(data);
    }

    private async Task<GpgKey> GetPrivateKeyAsync()
    {
      var keys = await GetKeysAsync(true);
      return keys.FirstOrDefault(k => k.UserId == messenger.Self.Id);
    }
  }
}
