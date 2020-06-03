using Prism.Commands;
using System;
using System.Windows.Input;

namespace CrowdedPrison.Core.ViewModels
{
  [Flags]
  public enum Buttons
  {
    None = 0,
    Ok = 1 << 0,
    Cancel = 1 << 1,
    Yes = 1 << 2,
    No = 1 << 3
  }

  public class MessageDialogViewModel : BaseDialogViewModel<Buttons>
  {
    private string message;
    private Buttons buttons;

    public string Message
    {
      get => message;
      set => SetProperty(ref message, value);
    }

    public Buttons Buttons
    {
      get => buttons;
      set 
      {
        SetProperty(ref buttons, value);
        RaisePropertyChanged(nameof(IsOkButtonVisible));
        RaisePropertyChanged(nameof(IsCancelButtonVisible));
        RaisePropertyChanged(nameof(IsYesButtonVisible));
        RaisePropertyChanged(nameof(IsNoButtonVisible));
      }

    }

    public bool IsOkButtonVisible => Buttons.HasFlag(Buttons.Ok);
    public bool IsCancelButtonVisible => Buttons.HasFlag(Buttons.Cancel);
    public bool IsYesButtonVisible => Buttons.HasFlag(Buttons.Yes);
    public bool IsNoButtonVisible => Buttons.HasFlag(Buttons.No);

    public ICommand ButtonPressCommand { get; }

    public MessageDialogViewModel()
    {
      ButtonPressCommand = new DelegateCommand<Buttons?>(b => SetResult(b ?? default));
    }
  }
}
