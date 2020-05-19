using System;
using System.Threading.Tasks;

namespace CrowdedPrison.Core
{
  public class GpgWrapper : IGpg
  {
    private readonly Func<IAsyncProcess> processFactory;
    private readonly IFileSystem fileSystem;
    public string GpgPath => @"C:\Users\armw\Desktop\gpg\gpg.exe";

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

    public async Task<bool> RunCommandAsync(string command, string input = null, string homedir = null)
    {
      var p = processFactory();
      using (p as IDisposable)
      {
        if (homedir != null) command = $"--homedir {homedir} {command}";
        p.Start(GpgPath, command);

        if (input != null) await p.WriteToInputAsync(input);

        await p.WaitForExitAsync();

        return p.ExitCode == 0;
      }
    }
  }
}
