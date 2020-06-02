using CrowdedPrison.Wpf.Views;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using System.Windows;
using CrowdedPrison.Common;
using CrowdedPrison.Encryption;
using CrowdedPrison.Messenger;
using CrowdedPrison.Messenger.Encryption;
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
      containerRegistry.RegisterSingleton<AppConfiguration>();

      var appConfig = new AppConfiguration();
      containerRegistry.RegisterInstance(appConfig);
      containerRegistry.RegisterInstance<IGpgConfiguration>(appConfig);
      containerRegistry.RegisterInstance<IMessengerConfiguration>(appConfig);
      containerRegistry.RegisterInstance<IGpgMessengerConfiguration>(appConfig);

      containerRegistry.RegisterForNavigation<MainView>();
      containerRegistry.Register<IDialogService, DialogService>();
      containerRegistry.Register<ILoginDialogService, LoginDialogService>();

      DialogService.Register<LoginDialogViewModel, LoginDialogView>();
      DialogService.Register<TwoFactorDialogViewModel, TwoFactorDialogView>();
    }

    protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
    {
      moduleCatalog.AddModule<CommonModule>();
      moduleCatalog.AddModule<EncryptionModule>();
      moduleCatalog.AddModule<MessengerModule>();
      moduleCatalog.AddModule<MessengerEncryptionModule>();
    }
  }
}
