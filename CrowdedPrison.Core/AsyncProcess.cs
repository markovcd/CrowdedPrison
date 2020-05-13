using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace CrowdedPrison.Core
{
  public class AsyncProcess : IAsyncProcess, IDisposable
  {
    private TaskCompletionSource<object> tcs;
    private Process process;

    public ProcessState State { get; private set; }

    private readonly AsyncStream<OutputData> asyncStream = new AsyncStream<OutputData>();

    public IAsyncEnumerable<OutputData> AsyncDataStream => asyncStream;

    public int ExitCode => process.ExitCode;

    public bool Start(string fileName, string arguments = default)
    {
      if (State != ProcessState.NotStarted) 
        throw new InvalidOperationException("Process already started");

      tcs = new TaskCompletionSource<object>();
      var startInfo = CreateStartInfo(fileName, arguments);
      process = CreateProcess(startInfo);
      var result =  process.Start();
      process.BeginOutputReadLine();
      process.BeginErrorReadLine();

      State = ProcessState.Started;
      return result;
    }

    public async Task WriteToInputAsync(string command)
    {
      var sw = process.StandardInput;
      await sw.WriteLineAsync(command);
    }

    public void Kill()
    {
      process.CancelErrorRead();
      process.CancelOutputRead();
      process.Kill();
    }

    public async Task WaitForExitAsync()
    {
      await tcs.Task;
      tcs = null;
      process.WaitForExit();
    }

    public void Dispose()
    {
      process.Dispose();
    }

    private void AddData(string data, bool isError = false)
    {
      if (!string.IsNullOrEmpty(data))
        asyncStream.Add(new OutputData(data, isError));
    }

    private static ProcessStartInfo CreateStartInfo(string fileName, string arguments)
    {
      return new ProcessStartInfo
      {
        Arguments = arguments,
        FileName = fileName,
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        RedirectStandardInput = true,
        CreateNoWindow = false,
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

    private void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
    {
      AddData(e.Data);
    }

    private void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
    {
      AddData(e.Data, true);
    }

    private void Process_Exited(object sender, EventArgs e)
    {
      asyncStream.Finish();
      tcs?.TrySetResult(default);
      State = ProcessState.Exited;
    }
  }
}
