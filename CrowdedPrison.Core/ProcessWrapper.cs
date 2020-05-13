using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace CrowdedPrison.Core
{
  public class OutputData
  {
    public string Data { get; }
    public bool IsError { get; }
    public DateTime Timestamp { get; }

    public OutputData(string data, bool isError = false)
    {
      Data = data;
      IsError = isError;
      Timestamp = DateTime.Now;
    }
  }

  public class ProcessWrapper : IProcess
  {
    private TaskCompletionSource<object> tcs;
    private readonly Process process;
    private readonly AsyncStream<OutputData> asyncStream;
    public IAsyncEnumerable<OutputData> AsyncDataStream => asyncStream;

    public ProcessWrapper(string fileName, string arguments)
    {
      asyncStream = new AsyncStream<OutputData>();
      var startInfo = CreateStartInfo(fileName, arguments);
      process = CreateProcess(startInfo);
    }

    private static ProcessStartInfo CreateStartInfo(string fileName, string arguments)
    {
      return new ProcessStartInfo
      {
        Arguments = arguments,
        FileName = fileName,
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        CreateNoWindow = true,
        UseShellExecute = false,
      };
    }

    private Process CreateProcess(ProcessStartInfo startInfo)
    {
      var p = new Process
      {
        StartInfo = startInfo,
        EnableRaisingEvents = true
      };

      p.Exited += Process_Exited;
      p.ErrorDataReceived += Process_ErrorDataReceived;
      p.OutputDataReceived += Process_OutputDataReceived;
      return p;
    }

    public bool Start()
    {
      var result =  process.Start();
      process.BeginOutputReadLine();
      process.BeginErrorReadLine();
      return result;
    }

    public void Kill()
    {
      process.CancelErrorRead();
      process.CancelOutputRead();
      process.Kill();
    }

    public void Close()
    {
      process.CancelErrorRead();
      process.CancelOutputRead();
      process.Close();
    }


    public async Task WaitForExitAsync()
    {
      tcs = new TaskCompletionSource<object>();
      await tcs.Task;
      tcs = null;
      process.WaitForExit();
    }

    private void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
    {
      asyncStream.Add(new OutputData(e.Data));
    }

    private void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
    {
      asyncStream.Add(new OutputData(e.Data, true));
    }

    private void Process_Exited(object sender, EventArgs e)
    {
      asyncStream.Finish();
      tcs?.TrySetResult(default);
    }

  }
}
