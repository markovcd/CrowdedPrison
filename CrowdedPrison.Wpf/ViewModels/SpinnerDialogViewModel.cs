namespace CrowdedPrison.Wpf.ViewModels
{
  internal class SpinnerDialogViewModel : BaseDialogViewModel<object>
  {
    private string message;

    public string Message
    {
      get => message;
      set => SetProperty(ref message, value);
    }

    public void Close()
    {
      SetResult(null);
    }
  }
}