using Prism.Ioc;
using Prism.Modularity;

namespace CrowdedPrison.Common
{
  public class CommonModule : IModule
  {
    public void OnInitialized(IContainerProvider containerProvider)
    {
      
    }

    public void RegisterTypes(IContainerRegistry containerRegistry)
    {
      containerRegistry.RegisterSingleton<IFileSystem, FileSystem>();
      containerRegistry.Register<IAsyncProcess, AsyncProcess>();
    }
  }
}
