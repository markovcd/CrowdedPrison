﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace CrowdedPrison.Common
{
  internal class AsyncProcess : IAsyncProcess, IDisposable
  {
    private TaskCompletionSource<object> tcs;
    private Process process;
    private readonly List<OutputData> dataList = new List<OutputData>();
    public ProcessState State { get; private set; }
    public IReadOnlyList<OutputData> Data => dataList;

    public int ExitCode => process.ExitCode;
    public string ErrorText => AggregateData(true);
    public string OutputText => AggregateData();


    public bool Start(string fileName, string arguments = default, bool elevated = false)
    {
      if (State != ProcessState.NotStarted) 
        throw new InvalidOperationException("Process already started");

      tcs = new TaskCompletionSource<object>();
      var startInfo = CreateStartInfo(fileName, arguments, elevated);
      process = CreateProcess(startInfo);
      var result = process.Start();
      
      if (!elevated)
      {
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
      }

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

    private string AggregateData(bool isError = false)
    {
      var data = Data.Where(d => d.IsError == isError).Select(d => d.Data);
      return data.Any() ? data.Aggregate((a, b) => $"{a}\r\n{b}") : null;
    }

    private void AddData(string data, bool isError = false)
    {
      if (string.IsNullOrEmpty(data)) return;
      var o = new OutputData(data, isError);
      dataList.Add(o);
    }

    private static ProcessStartInfo CreateStartInfo(string fileName, string arguments, bool elevated = false)
    {
      return new ProcessStartInfo
      {
        Arguments = arguments,
        FileName = fileName,
        RedirectStandardOutput = !elevated,
        RedirectStandardError = !elevated,
        RedirectStandardInput = !elevated,
        CreateNoWindow = true,
        UseShellExecute = elevated,
        Verb = elevated ? "runas" : null
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
      tcs?.TrySetResult(default);
      State = ProcessState.Exited;
    }
  }
}
