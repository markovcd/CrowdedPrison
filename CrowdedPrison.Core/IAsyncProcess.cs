﻿using System.Collections.Generic;
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
    ProcessState State { get; }
    int ExitCode { get; }

    Task WaitForExitAsync();
    bool Start(string fileName, string arguments = default);
    Task WriteToInputAsync(string command);
    void Kill();
  }
}
