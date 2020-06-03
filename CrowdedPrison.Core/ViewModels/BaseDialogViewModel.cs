using System;
using Prism.Mvvm;

namespace CrowdedPrison.Core.ViewModels
{
  public abstract class BaseDialogViewModel<TResult> : BindableBase
  {
    protected virtual void SetResult(TResult result)
    {
      RequestCloseCallback?.Invoke(result);
    }

    public Action<TResult> RequestCloseCallback { get; set; }

  }
}