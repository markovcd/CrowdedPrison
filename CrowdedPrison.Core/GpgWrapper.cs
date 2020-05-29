using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Starksoft.Aspen.GnuPG;

namespace CrowdedPrison.Core
{
  public class GpgException : Exception
  {
    public GpgException(string message) : base(message) { }
  }

  public class GpgWrapper : IGpg
  {
    private readonly Func<IAsyncProcess> processFactory;
    private readonly IFileSystem fileSystem;

    public string GpgPath { get; set; } = @"C:\Users\marko\Desktop\GnuPg\gpg.exe";
    public string HomeDir { get; set; }

    public GpgWrapper(Func<IAsyncProcess> processFactory, IFileSystem fileSystem)
    {
      this.processFactory = processFactory;
      this.fileSystem = fileSystem;
    }

    public Task<string> EncryptAsync(string text, string name)
    {
      return InOutOperationAsync(
        text,
        (inFile, outFile) => EncryptFileAsync(inFile, outFile, name));
    }

    public Task<string> DecryptAsync(string text, string password)
    {
      return InOutOperationAsync(
        text, 
        (inFile, outFile) => DecryptFileAsync(inFile, outFile, password));
    }

    public Task<bool> ImportKeyAsync(string key)
    {
      return InOperationAsync(key, ImportKeyFromFileAsync);
    }

    public Task<string> ExportKeyAsync(string name)
    {
      return OutOperationAsync(f => ExportKeyToFileAsync(f, name));
    }

    public Task<bool> EncryptFileAsync(string inputFileName, string outputFileName, string name)
    {
      return RunCommandAsync($"--armor --output {outputFileName} --encrypt --recipient {name} {inputFileName}");
    }

    public Task<bool> DecryptFileAsync(string inputFileName, string outputFileName, string password)
    {
      return RunCommandAsync($"--armor --batch --passphrase-fd 0 --pinentry-mode loopback --output {outputFileName} --decrypt {inputFileName}", password);
    }

    public Task<bool> ImportKeyFromFileAsync(string fileName)
    {
      return RunCommandAsync($"--armor --import \"{fileName}\"");
    }

    public Task<bool> ExportKeyToFileAsync(string fileName, string name)
    {
      return RunCommandAsync($"--armor --output \"{fileName}\" --export {name}");
    }

    public Task<bool> GenerateKeyAsync(string name, string password)
    {
      return RunCommandAsync($"--batch --passphrase-fd 0 --pinentry-mode loopback --quick-generate-key {name}", password);
    }
     
    public async Task<bool> KeyExistsAsync(string name)
    {
      try
      {
        return await RunCommandAsync($"--batch --list-keys {name}");
      }
      catch (GpgException)
      {
        return false;
      }
    }

    public async Task<IReadOnlyList<IReadOnlyList<string>>> ListKeysAsync(bool secret = false)
    {
      var cmd = secret ? "--list-secret-keys" : "--list-keys";
      var output = await RunCommandWithOutputAsync($"{cmd} --with-fingerprint --with-colons --fixed-list-mode");
      var result = new List<IReadOnlyList<string>>();
      foreach (var line in output)
      {
        var fields = line.Split(':');
        result.Add(fields);
      }
      return result;
    }

    public async Task<IReadOnlyList<string>> RunCommandWithOutputAsync(string command)
    {
      var p = processFactory();
      using (p as IDisposable)
      {
        if (!string.IsNullOrEmpty(HomeDir)) command = $"--homedir {HomeDir} {command}";
        p.Start(GpgPath, command);

        await p.WaitForExitAsync();


        if (p.ExitCode == 0)
        {
          var output = p.Data.Where(d => !d.IsError).Select(d => d.Data).ToList();
          return output;
        }

        if (string.IsNullOrWhiteSpace(p.ErrorText)) return null;

        throw new GpgException(p.ErrorText);
      }
    }


    public async Task<bool> InOperationAsync(string data, Func<string, Task<bool>> operation)
    {
      var tempFileName = fileSystem.GetTempFilePath();
      try
      {
        await fileSystem.WriteAllTextAsync(tempFileName, data);
        return await operation(tempFileName);
      }
      finally
      {
        fileSystem.DeleteFile(tempFileName);
      }
    }

    public async Task<string> OutOperationAsync(Func<string, Task<bool>> operation)
    {
      var tempFileName = fileSystem.GetTempFilePath();

      try
      {
        if (!await operation(tempFileName)) return null;

        return await fileSystem.ReadAllTextAsync(tempFileName);
      }
      finally
      {
        fileSystem.DeleteFile(tempFileName);
      }
    }

    private async Task<string> InOutOperationAsync(string data, Func<string, string, Task<bool>> operation)
    {

      var inTempFileName = fileSystem.GetTempFilePath();
      var outTempFileName = fileSystem.GetTempFilePath();

      try
      {
        await fileSystem.WriteAllTextAsync(inTempFileName, data);
        if (!await operation(inTempFileName, outTempFileName)) return null;
        return await fileSystem.ReadAllTextAsync(outTempFileName);
      }
      finally
      {
        fileSystem.DeleteFile(inTempFileName);
        fileSystem.DeleteFile(outTempFileName);
      }
    }

    public async Task<bool> RunCommandAsync(string command, string input = null)
    {
      var p = processFactory();
      using (p as IDisposable)
      {
        if (!string.IsNullOrEmpty(HomeDir))
        {
          Directory.CreateDirectory(HomeDir);
          command = $"--homedir {HomeDir} {command}";
        }
        p.Start(GpgPath, command);

        if (input != null) await p.WriteToInputAsync(input);

        await p.WaitForExitAsync();

        if (p.ExitCode == 0) return true;
        if (string.IsNullOrWhiteSpace(p.ErrorText)) return false;

        throw new GpgException(p.ErrorText);
      }
    }
  }
}
