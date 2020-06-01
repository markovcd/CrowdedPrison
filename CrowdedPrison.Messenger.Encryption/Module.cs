using CrowdedPrison.Messenger.Encryption;
using Prism.Ioc;
using Prism.Modularity;

namespace CrowdedPrison.Messenger.Encryption
{
  public class Module : IModule
  {
    public void OnInitialized(IContainerProvider containerProvider)
    {
      
    }

    public void RegisterTypes(IContainerRegistry containerRegistry)
    {
      containerRegistry.Register<IGpgMessenger, GpgMessenger>();
    }
  }
}
