using System.Threading.Tasks;
using CrowdedPrison.Core.ViewModels;

namespace CrowdedPrison.Core.Services
{
  public interface IDialogService
  {
    Task<TResult> ShowDialogAsync<TResult>(BaseDialogViewModel<TResult> viewModel);
  }
}