using System.Threading.Tasks;

namespace CrowdedPrison.Core
{
  public interface IProcess
  {
    Task WaitForExitAsync();
    bool Start();
    void Kill();
    void Close();
  }
}
