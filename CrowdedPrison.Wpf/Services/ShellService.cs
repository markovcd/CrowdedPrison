using System;
using System.ComponentModel;

namespace CrowdedPrison.Wpf.Services
{
  internal class ShellService : IShellService
  {
    private readonly Shell shell;

    public event EventHandler<CancelEventArgs> Cloing;

    public ShellService(Shell shell)
    {
      this.shell = shell;
      shell.Closing += Shell_Closing;
    }

    private void Shell_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
      Cloing?.Invoke(this, e);
    }

    public void Close()
    {
      shell.Dispatcher.Invoke(shell.Close);
    }
  }
}
