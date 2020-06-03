using CrowdedPrison.Encryption;
using Prism.Commands;
using System;
using System.ComponentModel;
using System.Windows.Input;

namespace CrowdedPrison.Core.ViewModels
{
  public class DownloadGpgDialogViewModel : BaseDialogViewModel<string>
  {
    private readonly IGpgDownloader downloader;
    private bool isDownloading;

    public ICommand DownloadCommand { get; }

    public bool IsDownloading
    {
      get => isDownloading;
      set => SetProperty(ref isDownloading, value);
    }

    public DownloadGpgDialogViewModel(IGpgDownloader downloader)
    {
      this.downloader = downloader;
      DownloadCommand = new DelegateCommand(Download, () => !IsDownloading);
    }

    private async void Download()
    {
      IsDownloading = true;

      try
      {
        var path = await downloader.EnsureGpgExistsAsync();
        SetResult(path);
      }     
      catch (TimeoutException)
      {
        SetResult(null);
      }
      catch (OperationCanceledException)
      {
        SetResult(null);
      }
      catch (Win32Exception)
      {
        SetResult(null);
      }
      finally
      {
        IsDownloading = false;
      }
    }
  }
}
