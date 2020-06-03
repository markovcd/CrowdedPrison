using CrowdedPrison.Core.Services;
using Prism.Ioc;
using Prism.Modularity;

namespace CrowdedPrison.Core
{
  public class CoreModule : IModule
  {
    public void OnInitialized(IContainerProvider containerProvider)
    {
      
    }

    public void RegisterTypes(IContainerRegistry containerRegistry)
    {
      containerRegistry.Register<IMainDialogService, MainDialogService>();

    }
  }
}
