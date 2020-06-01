using System;
using System.Threading.Tasks;
using CrowdedPrison.Core;
using CrowdedPrison.Messenger.Entities;

namespace CrowdedPrison.Messenger.Encryption
{
  public class GpgMessenger : MessengerWrapper
  {
    private readonly IGpg gpg;

    public GpgMessenger(IGpg gpg)
    {
      this.gpg = gpg;
    }

    public async Task<bool> GeneratePrivateKey(string password)
    {
      return await gpg.GenerateKeyAsync(Self.Id, password);
    }

    public void ImportPublicKey(MessengerUser user)
    {
      //SearchThread(user, )
      //gpg.ImportKeyAsync()
    }
  }
}
