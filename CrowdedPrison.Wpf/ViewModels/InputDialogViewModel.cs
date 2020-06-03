using System.Windows.Input;
using Prism.Commands;

namespace CrowdedPrison.Wpf.ViewModels
{
  internal class InputDialogViewModel : BaseDialogViewModel<string>
  {
    private string value;
    private string message;

    public ICommand OkCommand { get; }
    public ICommand CancelCommand { get; }

    public string Value
    {
      get => value;
      set => base.SetProperty(ref value, value);
    }

    public string Message
    {
      get => message;
      set => SetProperty(ref message, value);
    }

    public InputDialogViewModel()
    {
      OkCommand = new DelegateCommand(Ok);
      CancelCommand = new DelegateCommand(Cancel);
    }

    private void Ok()
    {
      SetResult(Value);
    }

    private void Cancel()
    {
      SetResult(null);
    }
  }
}