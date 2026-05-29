using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.Scripting.Hosting;

namespace MacroStudio.Backend.Windows.Execution;

internal static class ExecutionUtils
{
	private static Script<object>? _initialScript;
	private static InteractiveAssemblyLoader? _loader;
	private static ScriptOptions? _options;

	public static readonly System.Reflection.Assembly[] References =
	[
		typeof(object).Assembly,
		typeof(System.Drawing.Rectangle).Assembly,
		typeof(System.IO.FileInfo).Assembly,
		typeof(System.Linq.IQueryable).Assembly,
		typeof(System.Dynamic.DynamicObject).Assembly,
		typeof(System.Text.RegularExpressions.Regex).Assembly,
		typeof(Process).Assembly
	];

	public static readonly string[] Imports =
	[
		"System",
		"System.IO",
		"System.Linq",
		"System.Text",
		"System.Drawing",
		"System.Dynamic",
		"System.Diagnostics",
		"System.Collections.Generic",
		"System.Text.RegularExpressions"
	];

	public static void Initialize()
	{
		_initialScript = CSharpScript.Create(string.Empty, GetOptions(), typeof(ScriptGlobals), GetLoader());

		Task.Run(async () =>
		{
			var globals = new ScriptGlobals();
			_initialScript.Compile();
			await _initialScript.CreateDelegate()(globals);
		}).ContinueWith(task => Console.WriteLine(task.Exception), TaskContinuationOptions.OnlyOnFaulted);
	}

	public static ScriptRunner<object>? GetRunner(string code)
	{
		if (_initialScript is not { } script)
		{
			_initialScript = CSharpScript.Create(string.Empty, GetOptions(), typeof(ScriptGlobals), GetLoader());
			script = _initialScript;
		}

		var continuation = script.ContinueWith(code);
		continuation.Compile();
		return continuation.CreateDelegate();
	}

	private static InteractiveAssemblyLoader GetLoader()
	{
		if (_loader is not null)
		{
			return _loader;
		}

		_loader = new InteractiveAssemblyLoader();
		foreach (var reference in References)
		{
			_loader.RegisterDependency(reference);
		}

		return _loader;
	}

	private static ScriptOptions GetOptions()
	{
		_options ??= ScriptOptions.Default
			.WithOptimizationLevel(Microsoft.CodeAnalysis.OptimizationLevel.Release)
			.WithReferences(References)
			.AddImports(Imports);

		return _options;
	}
}