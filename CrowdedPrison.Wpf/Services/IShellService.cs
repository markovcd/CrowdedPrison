using System;
using System.ComponentModel;

namespace CrowdedPrison.Wpf.Services
{
  internal interface IShellService
  {
    event EventHandler<CancelEventArgs> Cloing;
    void Close();
  }
}
