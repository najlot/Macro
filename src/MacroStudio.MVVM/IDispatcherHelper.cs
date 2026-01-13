using System;
using System.Threading.Tasks;

namespace MacroStudio.MVVM;

public interface IDispatcherHelper
{
	void BeginInvokeOnMainThread(Action action);
	Task BeginInvokeOnMainThread(Func<Task> action);
}