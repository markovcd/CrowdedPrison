using System.Windows.Controls;
using System.Windows.Input;
using Prism.Commands;

namespace CrowdedPrison.Core.ViewModels
{
  public class PasswordDialogViewModel : BaseDialogViewModel<string>
  {
    private string message;

    public ICommand OkCommand { get; }
    public ICommand CancelCommand { get; }

    public string Message
    {
      get => message;
      set => SetProperty(ref message, value);
    }

    public PasswordDialogViewModel()
    {
      OkCommand = new DelegateCommand<PasswordBox>(Ok);
      CancelCommand = new DelegateCommand(Cancel);
    }

    private void Ok(PasswordBox passwordBox)
    {
      SetResult(passwordBox.Password);
    }

    private void Cancel()
    {
      SetResult(null);
    }
  }
}