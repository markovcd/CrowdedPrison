using System;
using System.Linq;
using System.Security;
using System.Threading.Tasks;
using CrowdedPrison.Encryption;
using CrowdedPrison.Messenger.Encryption.Events;
using CrowdedPrison.Messenger.Entities;

namespace CrowdedPrison.Messenger.Encryption
{
  public class GpgMessenger : MessengerWrapper, IGpgMessenger
  {
    private readonly IGpg gpg;
    private readonly IPgpRegexHelper pgpHelper;
    private SecureString password;

    public event EventHandler<EncryptedMessageReceivedEventArgs> EncryptedMessageReceived;

    public string Password
    {
      get => password.ToString();
      set
      {
        password = new SecureString();
        if (value == null) return;

        foreach (var c in value)
          password.AppendChar(c);
      }
    }

    public GpgMessenger(IGpg gpg, IPgpRegexHelper pgpHelper)
    { 
      this.gpg = gpg;
      this.pgpHelper = pgpHelper;
    }

    public async Task<bool> GeneratePrivateKey()
    {
      return await gpg.GenerateKeyAsync(Self.Id, Password);
    }

    public async Task<string> GetPublicKeyAsync(MessengerUser user)
    {
      var pattern = "-----BEGIN PGP PUBLIC KEY BLOCK----- -----END PGP PUBLIC KEY BLOCK-----";
      var message = (await SearchThread(user, pattern, 1)).FirstOrDefault();
      if (message == null) return null;

      return pgpHelper.GetPublicKeyBlock(message.Text);
    }

    public async Task<bool> SendPublicKeyAsync(MessengerUser user)
    {
      var publicKey = await gpg.ExportKeyAsync(Self.Id);
      return await SendTextAsync(user, publicKey);
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
      return await SendTextAsync(user, encrypted);
    }

    protected override void OnMessageReceived(MessengerUser user, MessengerMessage message, MessengerMessage replyTo = null)
    {
      var encrypted = pgpHelper.GetMessageBlock(message.Text);
      if (string.IsNullOrEmpty(encrypted))
        base.OnMessageReceived(user, message, replyTo);
      else
        OnEncryptedMessageReceived(encrypted, user, message, replyTo);
    }

    protected async void OnEncryptedMessageReceived(string encrypted, MessengerUser user, MessengerMessage message, MessengerMessage replyTo = null)
    {
      var decrypted = await gpg.DecryptAsync(encrypted, Password);
      var args = new EncryptedMessageReceivedEventArgs(decrypted, user, message, replyTo);
      EncryptedMessageReceived?.Invoke(this, args);
    }
  }
}
