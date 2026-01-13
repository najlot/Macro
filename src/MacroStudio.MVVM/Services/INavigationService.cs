using MacroStudio.MVVM.ViewModel;
using System.Threading.Tasks;

namespace MacroStudio.MVVM.Services;

public interface INavigationService
{
	Task NavigateBack();

	Task NavigateForward(AbstractViewModel newViewModel);
}