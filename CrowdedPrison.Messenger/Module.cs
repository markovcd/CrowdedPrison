using Prism.Ioc;
using Prism.Modularity;

namespace CrowdedPrison.Messenger
{
  public class Module : IModule
  {
    public void OnInitialized(IContainerProvider containerProvider)
    {
      
    }

    public void RegisterTypes(IContainerRegistry containerRegistry)
    {
      containerRegistry.Register<IMessenger, MessengerWrapper>();
    }
  }
}
