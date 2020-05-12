using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace CrowdedPrison.Core
{
  public class ProcessWrapper : IProcess, IDisposable
  {
    private TaskCompletionSource<object> tcs;
    private readonly Process process;

    public ProcessWrapper(string fileName, string arguments)
    {
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
      var process = new Process
      {
        StartInfo = startInfo,
        EnableRaisingEvents = true
      };

      process.ErrorDataReceived += Process_ErrorDataReceived;
      process.Exited += Process_Exited;
      process.OutputDataReceived += Process_OutputDataReceived;
      return process;
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

    public void Dispose()
    {
      process.Dispose();
    }




    private void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
    {

    }

    private void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
    {

    }

    private void Process_Exited(object sender, EventArgs e)
    {
      tcs?.TrySetResult(default);
    }

  }
}
