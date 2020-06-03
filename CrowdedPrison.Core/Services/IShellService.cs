using System;
using System.ComponentModel;

namespace CrowdedPrison.Core.Services
{
  public interface IShellService
  {
    event EventHandler<CancelEventArgs> Cloing;
    void Close();
  }
}
