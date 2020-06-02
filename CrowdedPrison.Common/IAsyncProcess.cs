using System.Collections.Generic;
using System.Threading.Tasks;

namespace CrowdedPrison.Common
{
  public enum ProcessState
  {
    NotStarted,
    Started,
    Exited,
  }

  public interface IAsyncProcess
  {
    IReadOnlyList<OutputData> Data { get; }
    ProcessState State { get; }
    int ExitCode { get; }
    string ErrorText { get; }
    string OutputText { get; }
    Task WaitForExitAsync();
    bool Start(string fileName, string arguments = default, bool elevated = false);
    Task WriteToInputAsync(string command);
    void Kill();
  }
}
