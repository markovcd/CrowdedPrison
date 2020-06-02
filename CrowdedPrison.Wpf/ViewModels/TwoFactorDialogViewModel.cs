using System.Windows.Input;
using Prism.Commands;

namespace CrowdedPrison.Wpf.ViewModels
{
  public class TwoFactorDialogViewModel : BaseDialogViewModel<string>
  {
    private string code;

    public ICommand OkCommand { get; }
    public ICommand CancelCommand { get; }

    public string Code
    {
      get => code;
      set => SetProperty(ref code, value);
    }

    public TwoFactorDialogViewModel()
    {
      OkCommand = new DelegateCommand(Ok);
      CancelCommand = new DelegateCommand(Cancel);
    }

    private void Ok()
    {
      SetResult(Code);
    }

    private void Cancel()
    {
      SetResult(null);
    }
  }
}