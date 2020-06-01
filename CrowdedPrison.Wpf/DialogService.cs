using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using CrowdedPrison.Wpf.ViewModels;

namespace CrowdedPrison.Wpf
{
  public interface IDialogService
  {
    Task<TResult> ShowDialogAsync<TResult>(BaseDialogViewModel<TResult> viewModel);
  }

  internal class DialogService : IDialogService
  {
    private static readonly IDictionary<Type, Type> Registrations = new Dictionary<Type, Type>();

    private readonly Shell shell;

    public DialogService(Shell shell)
    {
      this.shell = shell;
    }

    public static void Register<TViewModel, TView>()
    {
      Registrations.Add(typeof(TViewModel), typeof(TView));
    }

    public async Task<TResult> ShowDialogAsync<TResult>(BaseDialogViewModel<TResult> viewModel)
    {
      if (shell.IsDialogShown) throw new InvalidOperationException("Other dialog is already shown.");

      var tcs = new TaskCompletionSource<TResult>();
      var view = GetView(viewModel.GetType());
      view.DataContext = viewModel;

      viewModel.RequestCloseCallback = r =>
      {
        shell.HideDialogAsync();
        tcs.TrySetResult(r);
      };

      try
      {
        shell.IsDialogShown = true;

        await shell.ShowDialogAsync(view);

        return await tcs.Task;
      }
      finally
      {
        shell.IsDialogShown = false;
        viewModel.RequestCloseCallback = null;
      }
    }

    private static FrameworkElement GetView(Type viewModelType)
    {
      return Activator.CreateInstance(Registrations[viewModelType]) as FrameworkElement;
    }
  }
}