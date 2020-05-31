using CrowdedPrison.Messenger;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Text;

namespace CrowdedPrison.Wpf.ViewModels
{
  internal class MainViewModel : BindableBase
  {
    private readonly IMessenger messenger;

    public MainViewModel(IMessenger messenger)
    {
      this.messenger = messenger;
    }
  }
}
