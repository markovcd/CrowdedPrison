using System;
using Prism.Mvvm;

namespace CrowdedPrison.Wpf.ViewModels
{
  internal class BaseDialogViewModel<TResult> : BindableBase
  {
    protected virtual void SetResult(TResult result)
    {
      RequestCloseCallback?.Invoke(result);
    }

    internal Action<TResult> RequestCloseCallback { get; set; }

  }
}