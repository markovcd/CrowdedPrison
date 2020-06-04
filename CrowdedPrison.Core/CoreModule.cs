using CrowdedPrison.Core.Services;
using CrowdedPrison.Core.ViewModels;
using CrowdedPrison.Messenger.Entities;
using Prism.Ioc;
using Prism.Modularity;
using System;
using Unity;
using Unity.Injection;
using Unity.Resolution;

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
      containerRegistry.Register<IUserViewModel, UserViewModel>();
    }
  }
}
