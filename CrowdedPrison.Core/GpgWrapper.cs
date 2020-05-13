using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CrowdedPrison.Core
{

  public class GpgWrapper
  {
    private readonly Func<IAsyncProcess> processFactory;

    public string GpgPath => @"C:\Users\armw\Desktop\gpg\gpg.exe";

    public GpgWrapper(Func<IAsyncProcess> processFactory)
    {
      this.processFactory = processFactory;
    }

    

    public async Task<bool> GenerateKey(string name)
    {
      var p = processFactory();
      using (p as IDisposable)
      {
        p.Start(GpgPath, $"--batch --quick-generate-key {name}");
        
        
        await p.WaitForExitAsync();

        await foreach (var d in p.AsyncDataStream)
        {
          Console.WriteLine(d.IsError ? $"Error: {d.Data}" : $"Data: {d.Data}");
        }
        return p.ExitCode == 0;
      }
    }
  }
}
