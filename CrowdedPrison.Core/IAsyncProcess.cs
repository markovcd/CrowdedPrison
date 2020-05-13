using System.Collections.Generic;
using System.Threading.Tasks;

namespace CrowdedPrison.Core
{
  public interface IAsyncProcess
  {
    IAsyncEnumerable<OutputData> AsyncDataStream { get; }

    Task WaitForExitAsync();
    bool Start(string fileName, string arguments = default);
    Task WriteToInputAsync(string command);
    void Kill();
  }
}
