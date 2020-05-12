using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace CrowdedPrison.Core
{
  public class OutputData
  {
    public string Data { get; }
    public bool IsError { get; }

    public OutputData(string data, bool isError = false)
    {
      Data = data;
      IsError = isError;
    }
  }

  public interface IProcess
  {
    IAsyncEnumerable<OutputData> StartProcessAsync(string fileName, string arguments);
  }

  public class ProcessWrapper : IProcess
  {
    private TaskCompletionSource<OutputData> tcs;

    private ProcessStartInfo CreateStartInfo(string fileName, string arguments)
    {
      return new ProcessStartInfo
      {
        Arguments = arguments,
        FileName = fileName,
        WindowStyle = ProcessWindowStyle.Hidden,
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        CreateNoWindow = true,
        UseShellExecute = false,
      };
    }

    private void StartProcess(ProcessStartInfo startInfo)
    {
      var process = Process.Start(startInfo);
      if (process == null) throw new NullReferenceException(nameof(process));

      process.ErrorDataReceived += Process_ErrorDataReceived;
      process.Exited += Process_Exited;
      process.OutputDataReceived += Process_OutputDataReceived;
    }

    public async IAsyncEnumerable<OutputData> StartProcessAsync(string fileName, string arguments)
    {
      var startInfo = CreateStartInfo(fileName, arguments);

      StartProcess(startInfo);

      while (true)
      {
        var data = await GetData();
        if (data == null) yield break;
        yield return data;
      }
    }

    private async Task<OutputData> GetData()
    {
      if (tcs == null) tcs = new TaskCompletionSource<OutputData>();
      var data = await tcs.Task;
      tcs = new TaskCompletionSource<OutputData>();
      return data;
    }

    private void AddOutput(OutputData data)
    {
      tcs.SetResult(data);
    }

    private void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
    {
      AddOutput(new OutputData(e.Data));
    }

    private void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
    {
      AddOutput(new OutputData(e.Data, true));
    }

    private void Process_Exited(object sender, EventArgs e)
    {
      AddOutput(null);
    }
  }

  public class GpgWrapper
  {
    public string GpgPath => @"C:\Users\armw\Desktop\gpg\gpg.exe";

    
  }
}
