using CrowdedPrison.Wpf.Views;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using System.Windows;

namespace CrowdedPrison.Wpf
{
  internal partial class App
  {
    protected override Window CreateShell()
    {
      return Container.Resolve<Shell>();
    }

    protected override void OnInitialized()
    {
      Container.Resolve<IRegionManager>().RegisterViewWithRegion(RegionNames.ShellRegion, typeof(MainView));
      base.OnInitialized();
    }

    protected override void RegisterTypes(IContainerRegistry containerRegistry)
    {
      containerRegistry.RegisterForNavigation<MainView>();
    }

    protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
    {
      moduleCatalog.AddModule<Messenger.Module>();
    }
  }
}
