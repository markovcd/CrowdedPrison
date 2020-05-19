using System.Collections.Generic;
using System.Threading.Tasks;

namespace CrowdedPrison.Core
{
  public enum ProcessState
  {
    NotStarted,
    Started,
    Exited,
  }

  public interface IAsyncProcess
  {
    IAsyncEnumerable<OutputData> AsyncDataStream { get; }
    IReadOnlyList<OutputData> Data { get; }
    ProcessState State { get; }
    int ExitCode { get; }
    string ErrorText { get; }
    string OutputText { get; }
    Task WaitForExitAsync();
    bool Start(string fileName, string arguments = default);
    Task WriteToInputAsync(string command);
    void Kill();
  }
}
