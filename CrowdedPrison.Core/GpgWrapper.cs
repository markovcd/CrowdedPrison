using System;
using System.Threading.Tasks;

namespace CrowdedPrison.Core
{
  public class GpgWrapper
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

    public Task<string> DecryptAsync(string text)
    {
      return InOutOperationAsync(text, DecryptFileAsync);
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
      return RunCommandAsync($"--output {outputFileName} --encrypt --recipient {name} {inputFileName}");
    }

    public Task<bool> DecryptFileAsync(string inputFileName, string outputFileName)
    {
      return RunCommandAsync($"--output {outputFileName} --decrypt {inputFileName}");
    }

    public Task<bool> ImportKeyFromFileAsync(string fileName)
    {
      return RunCommandAsync($"--import \"{fileName}\"");
    }

    public Task<bool> ExportKeyToFileAsync(string fileName, string name)
    {
      return RunCommandAsync($"--armor --output \"{fileName}\" --export {name}");
    }

    public Task<bool> GenerateKeyAsync(string name)
    {
      return RunCommandAsync($"--batch --quick-generate-key {name}");
    }

    public async Task<bool> InOperationAsync(string data, Func<string, Task<bool>> operation)
    {
      var tempFileName = fileSystem.GetTempFilePath();
      try
      {
        fileSystem.WriteAllText(tempFileName, data);
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

        return fileSystem.ReadAllText(tempFileName);
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
        fileSystem.WriteAllText(inTempFileName, data);
        if (!await operation(inTempFileName, outTempFileName)) return null;
        return fileSystem.ReadAllText(outTempFileName);
      }
      finally
      {
        fileSystem.DeleteFile(inTempFileName);
        fileSystem.DeleteFile(outTempFileName);
      }
    }

    public async Task<bool> RunCommandAsync(string command)
    {
      var p = processFactory();
      using (p as IDisposable)
      {
        p.Start(GpgPath, command);

        await p.WaitForExitAsync();

        return p.ExitCode == 0;
      }
    }
  }
}
