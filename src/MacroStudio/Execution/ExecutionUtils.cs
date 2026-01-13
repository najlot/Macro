using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.Scripting.Hosting;
using System.Diagnostics;

namespace MacroStudio.Execution;

public static class ExecutionUtils
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

		// Run first compilation
		Task.Run(async () =>
		{
			var globals = new ScriptGlobals();
			_initialScript.Compile();
			await _initialScript.CreateDelegate()(globals);
		}).ContinueWith(task => Console.WriteLine(task.Exception), TaskContinuationOptions.OnlyOnFaulted);
	}

	public static ScriptRunner<object>? GetRunner(string code)
	{
		if (_initialScript is not { } scr)
		{
            _initialScript = CSharpScript.Create(string.Empty, GetOptions(), typeof(ScriptGlobals), GetLoader());
            scr = _initialScript;
        }

		var script = scr.ContinueWith(code);
		script.Compile();
		return script.CreateDelegate();
	}

	public static InteractiveAssemblyLoader GetLoader()
	{
		if (_loader == null)
		{
			_loader = new InteractiveAssemblyLoader();

			foreach (var reference in References)
			{
				_loader.RegisterDependency(reference);
			}
		}

		return _loader;
	}

	public static ScriptOptions GetOptions()
	{
		_options ??= ScriptOptions
			.Default
			.WithOptimizationLevel(Microsoft.CodeAnalysis.OptimizationLevel.Release)
			.WithReferences(References)
			.AddImports(Imports);

		return _options;
	}
}
