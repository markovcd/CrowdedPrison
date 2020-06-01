using System.Threading.Tasks;
using MaterialDesignThemes.Wpf;

namespace CrowdedPrison.Wpf
{
  internal partial class Shell 
  {
    private TaskCompletionSource<object> tcs;

    public bool IsDialogShown { get; set; }

    public Shell()
    {
      InitializeComponent();
    }

    public async Task ShowDialogAsync(object content)
    {
      DialogContent.Content = content;
      tcs = new TaskCompletionSource<object>();
      DialogHost.IsOpen = true;
      await tcs.Task;
    }

    public void HideDialogAsync()
    {
      DialogHost.IsOpen = false;
      DialogContent.Content = null;
    }

    private void OnDialogOpened(object sender, DialogOpenedEventArgs eventArgs)
    {
      tcs?.TrySetResult(null);
    }
  }
}
