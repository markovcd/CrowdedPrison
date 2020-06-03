using CrowdedPrison.Wpf.Views;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using System.Windows;
using CrowdedPrison.Common;
using CrowdedPrison.Encryption;
using CrowdedPrison.Messenger;
using CrowdedPrison.Messenger.Encryption;
using CrowdedPrison.Core.ViewModels;
using CrowdedPrison.Wpf.Services;
using CrowdedPrison.Core.Services;
using CrowdedPrison.Core;

namespace CrowdedPrison.Wpf
{
  internal partial class App
  {
    private readonly AppConfiguration appConfig = new AppConfiguration();

    protected override Window CreateShell()
    {
      return Container.Resolve<Shell>();
    }

    protected override void OnInitialized()
    {
      Container.Resolve<IRegionManager>().RequestNavigate(GlobalConstants.ShellRegionName, nameof(MainView));
      base.OnInitialized();
    }

    protected override void RegisterTypes(IContainerRegistry containerRegistry)
    {
      containerRegistry.RegisterSingleton<Shell>();

      containerRegistry.RegisterInstance(appConfig);
      containerRegistry.RegisterInstance<IGpgConfiguration>(appConfig);
      containerRegistry.RegisterInstance<IMessengerConfiguration>(appConfig);
      containerRegistry.RegisterInstance<IGpgMessengerConfiguration>(appConfig);

      containerRegistry.RegisterForNavigation<MainView, MainViewModel>();
      containerRegistry.Register<IDialogService, DialogService>();
      containerRegistry.Register<IShellService, ShellService>();

      DialogService.Register<LoginDialogViewModel, LoginDialogView>();
      DialogService.Register<InputDialogViewModel, InputDialogView>();
      DialogService.Register<PasswordDialogViewModel, InputPasswordDialogView>();
      DialogService.Register<DownloadGpgDialogViewModel, DownloadGpgDialogView>();
      DialogService.Register<MessageDialogViewModel, MessageDialogView>();
      DialogService.Register<SpinnerDialogViewModel, SpinnerDialogView>();
    }

    protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
    {
      moduleCatalog.AddModule<CommonModule>();
      moduleCatalog.AddModule<EncryptionModule>();
      moduleCatalog.AddModule<MessengerModule>();
      moduleCatalog.AddModule<MessengerEncryptionModule>();
      moduleCatalog.AddModule<CoreModule>();
    }
  }
}
