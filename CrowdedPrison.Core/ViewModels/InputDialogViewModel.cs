using System.Windows.Input;
using Prism.Commands;

namespace CrowdedPrison.Core.ViewModels
{
  public class InputDialogViewModel : BaseDialogViewModel<string>
  {
    private string val;
    private string message;

    public ICommand OkCommand { get; }
    public ICommand CancelCommand { get; }

    public string Value
    {
      get => val;
      set => SetProperty(ref val, value);
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