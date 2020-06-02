using System.Threading.Tasks;
using CrowdedPrison.Wpf.ViewModels;

namespace CrowdedPrison.Wpf.Services
{
  internal interface IDialogService
  {
    Task<TResult> ShowDialogAsync<TResult>(BaseDialogViewModel<TResult> viewModel);
  }
}