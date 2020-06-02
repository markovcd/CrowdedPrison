using Prism.Ioc;
using Prism.Modularity;

namespace CrowdedPrison.Messenger
{
  public class MessengerModule : IModule
  {
    public void OnInitialized(IContainerProvider containerProvider)
    {
      
    }

    public void RegisterTypes(IContainerRegistry containerRegistry)
    {
      containerRegistry.RegisterSingleton<IMessenger, MessengerWrapper>();
    }
  }
}
