using CrowdedPrison.Wpf.Views;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using System.Windows;
using CrowdedPrison.Wpf.ViewModels;

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
      containerRegistry.RegisterSingleton<Shell>();

      containerRegistry.RegisterForNavigation<MainView>();
      containerRegistry.Register<IDialogService, DialogService>();
      DialogService.Register<LoginDialogViewModel, LoginDialogView>();
    }

    protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
    {
      moduleCatalog.AddModule<Messenger.Module>();
    }
  }
}
