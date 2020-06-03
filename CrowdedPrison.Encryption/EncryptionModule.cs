using Prism.Ioc;
using Prism.Modularity;

namespace CrowdedPrison.Encryption
{
  public class EncryptionModule : IModule
  {
    public void OnInitialized(IContainerProvider containerProvider)
    {
      
    }

    public void RegisterTypes(IContainerRegistry containerRegistry)
    {
      containerRegistry.RegisterSingleton<IGpg, GpgWrapper>();
      containerRegistry.RegisterSingleton<IPgpRegexHelper, PgpRegexHelper>();
      containerRegistry.RegisterSingleton<IGpgDownloader, GpgDownloader>();
      containerRegistry.RegisterSingleton<ITwoWayEncryption, TwoWayEncryption>();
      containerRegistry.RegisterSingleton<IKeyListParser, KeyListParser>();

    }
  }
}
