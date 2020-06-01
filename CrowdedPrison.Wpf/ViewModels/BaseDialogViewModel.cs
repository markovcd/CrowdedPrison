using System;
using Prism.Mvvm;

namespace CrowdedPrison.Wpf.ViewModels
{
  public class BaseDialogViewModel<TResult> : BindableBase
  {
    protected virtual void SetResult(TResult result)
    {
      RequestCloseCallback?.Invoke(result);
    }

    internal Action<TResult> RequestCloseCallback { get; set; }

  }
}