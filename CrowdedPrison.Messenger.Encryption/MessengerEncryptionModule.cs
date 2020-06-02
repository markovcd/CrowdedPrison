using Prism.Ioc;
using Prism.Modularity;

namespace CrowdedPrison.Messenger.Encryption
{
  public class MessengerEncryptionModule : IModule
  {
    public void OnInitialized(IContainerProvider containerProvider)
    {
      
    }

    public void RegisterTypes(IContainerRegistry containerRegistry)
    {
      containerRegistry.RegisterSingleton<IGpgMessenger, GpgMessenger>();
    }
  }
}
