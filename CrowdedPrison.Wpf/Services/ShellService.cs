namespace CrowdedPrison.Wpf.Services
{
  internal class ShellService : IShellService
  {
    private readonly Shell shell;

    public ShellService(Shell shell)
    {
      this.shell = shell;
    }

    public void Close()
    {
      shell.Dispatcher.Invoke(shell.Close);
    }
  }
}
