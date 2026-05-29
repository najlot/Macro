using MacroStudio.MVVM.ViewModel;

namespace MacroStudio.App.ViewModels;

public class GlobalMethodsViewModel : AbstractViewModel, ITabItem
{
	private const string AllCategories = "All categories";

	private static readonly BuiltInFunctionReference[] AllFunctions =
	[
		new(
			"GetClipboardText",
			"Clipboard",
			"Gets the current text content from the clipboard.",
			["string GetClipboardText();"],
			["clipboard", "paste", "text"]),
		new(
			"SetClipboardText",
			"Clipboard",
			"Sends text to the clipboard so other apps can paste it.",
			["void SetClipboardText(string text);"],
			["clipboard", "copy", "text"]),
		new(
			"GetScreenshot",
			"Screen & Images",
			"Captures a screenshot of the desktop as a platform-specific image object.",
			["object GetScreenshot();"],
			["screen", "screenshot", "desktop", "capture", "image"]),
		new(
			"GetBitmap",
			"Screen & Images",
			"Loads an image from a file path.",
			["object GetBitmap(string path);"],
			["image", "bitmap", "file", "load"]),
		new(
			"GetResourceBitmap",
			"Screen & Images",
			"Loads an image from the macro's embedded resources.",
			["object GetResourceBitmap(string name);"],
			["image", "bitmap", "resource", "asset"]),
		new(
			"SaveBitmap",
			"Screen & Images",
			"Saves an image object to a file path.",
			["void SaveBitmap(string path, object bitmap);"],
			["image", "bitmap", "save", "file", "export"]),
		new(
			"HasBitmap",
			"Image Search",
			"Checks whether a smaller image appears inside a larger image.",
			[
				"bool HasBitmap(object smallBmp, object bigBmp, int startX = 0, int startY = 0);",
				"bool HasBitmap(object smallBmp, object bigBmp, double tolerance, int startX = 0, int startY = 0);"
			],
			["image", "bitmap", "find", "search", "match", "tolerance"]),
		new(
			"SearchBitmap",
			"Image Search",
			"Finds the rectangle where a smaller image appears inside a larger image.",
			[
				"object SearchBitmap(object smallBmp, object bigBmp, int startX = 0, int startY = 0);",
				"object SearchBitmap(object smallBmp, object bigBmp, double tolerance, int startX = 0, int startY = 0);"
			],
			["image", "bitmap", "find", "search", "rectangle", "tolerance"]),
		new(
			"Simulate",
			"Input",
			"Simulates mouse or keyboard input with optional press and release control.",
			[
				"void Simulate(int key, int x, int y, int waitTime);",
				"void Simulate(int key, int x, int y, bool down, int waitTime);"
			],
			["input", "mouse", "keyboard", "click", "press", "release"]),
		new(
			"Wait",
			"Timing",
			"Pauses the macro for the specified number of milliseconds.",
			["void Wait(int milliseconds);"],
			["pause", "delay", "sleep", "milliseconds"]),
		new(
			"WaitMiliseconds",
			"Timing",
			"Alias for waiting in milliseconds. The function name keeps the existing spelling used by the macro API.",
			["void WaitMiliseconds(int milliseconds);"],
			["pause", "delay", "sleep", "milliseconds", "waitmilliseconds"]),
		new(
			"WaitSeconds",
			"Timing",
			"Pauses the macro for the specified number of seconds.",
			["void WaitSeconds(int seconds);"],
			["pause", "delay", "sleep", "seconds"]),
		new(
			"WaitMinutes",
			"Timing",
			"Pauses the macro for the specified number of minutes.",
			["void WaitMinutes(int minutes);"],
			["pause", "delay", "sleep", "minutes"]),
		new(
			"WaitHours",
			"Timing",
			"Pauses the macro for the specified number of hours.",
			["void WaitHours(int hours);"],
			["pause", "delay", "sleep", "hours"]),
		new(
			"SetCursorPosition",
			"Mouse",
			"Moves the cursor instantly to a screen position.",
			["void SetCursorPosition(int x, int y);"],
			["mouse", "cursor", "position", "move", "pointer"]),
		new(
			"MoveCursorTo",
			"Mouse",
			"Moves the cursor smoothly to a screen position over a duration.",
			["void MoveCursorTo(int x, int y, int ms);"],
			["mouse", "cursor", "position", "move", "pointer", "animate"]),
		new(
			"MouseLeftDown",
			"Mouse",
			"Presses and holds the left mouse button.",
			["void MouseLeftDown();"],
			["mouse", "left", "down", "press", "hold"]),
		new(
			"MouseLeftUp",
			"Mouse",
			"Releases the left mouse button.",
			["void MouseLeftUp();"],
			["mouse", "left", "up", "release"]),
		new(
			"MouseMiddleDown",
			"Mouse",
			"Presses and holds the middle mouse button.",
			["void MouseMiddleDown();"],
			["mouse", "middle", "down", "press", "hold"]),
		new(
			"MouseMiddleUp",
			"Mouse",
			"Releases the middle mouse button.",
			["void MouseMiddleUp();"],
			["mouse", "middle", "up", "release"]),
		new(
			"MouseRightDown",
			"Mouse",
			"Presses and holds the right mouse button.",
			["void MouseRightDown();"],
			["mouse", "right", "down", "press", "hold"]),
		new(
			"MouseRightUp",
			"Mouse",
			"Releases the right mouse button.",
			["void MouseRightUp();"],
			["mouse", "right", "up", "release"]),
		new(
			"PressKeyboardKey",
			"Keyboard",
			"Presses and holds a keyboard key.",
			["void PressKeyboardKey(byte key);"],
			["keyboard", "key", "press", "hold", "down"]),
		new(
			"ReleaseKeyboardKey",
			"Keyboard",
			"Releases a keyboard key.",
			["void ReleaseKeyboardKey(byte key);"],
			["keyboard", "key", "release", "up"])
	];

	public string Title => "Built-in Functions";

	public bool IsSelected
	{
		get => field;
		set => Set(ref field, value);
	}

	public string SearchText
	{
		get => field;
		set
		{
			if (Set(ref field, value))
			{
				UpdateFilteredFunctions();
			}
		}
	} = string.Empty;

	public IReadOnlyList<string> Categories { get; }

	public string SelectedCategory
	{
		get => field;
		set
		{
			var category = string.IsNullOrWhiteSpace(value) ? AllCategories : value;
			if (Set(ref field, category))
			{
				UpdateFilteredFunctions();
			}
		}
	} = AllCategories;

	public IReadOnlyList<BuiltInFunctionReference> FilteredFunctions
	{
		get => field;
		private set
		{
			if (Set(ref field, value))
			{
				RaisePropertyChanged(nameof(HasResults));
				RaisePropertyChanged(nameof(HasNoResults));
				RaisePropertyChanged(nameof(ResultSummary));
				RaisePropertyChanged(nameof(EmptyStateMessage));
			}
		}
	} = [];

	public BuiltInFunctionReference? SelectedFunction
	{
		get => field;
		set
		{
			if (Set(ref field, value))
			{
				RaisePropertyChanged(nameof(HasSelectedFunction));
				RaisePropertyChanged(nameof(SelectedFunctionName));
				RaisePropertyChanged(nameof(SelectedFunctionCategory));
				RaisePropertyChanged(nameof(SelectedFunctionSummary));
				RaisePropertyChanged(nameof(SelectedFunctionSignatures));
				RaisePropertyChanged(nameof(SelectedFunctionKeywords));
				RaisePropertyChanged(nameof(SignatureSectionTitle));
			}
		}
	}

	public bool HasResults => FilteredFunctions.Count > 0;
	public bool HasNoResults => !HasResults;
	public bool HasSelectedFunction => SelectedFunction is not null;

	public string ResultSummary
	{
		get
		{
			if (HasNoResults)
			{
				return "No built-in functions match the current filters.";
			}

			var label = FilteredFunctions.Count == 1 ? "function" : "functions";
			if (string.IsNullOrWhiteSpace(SearchText) && SelectedCategory == AllCategories)
			{
				return $"Showing all {FilteredFunctions.Count} built-in {label}.";
			}

			return $"Showing {FilteredFunctions.Count} matching built-in {label}.";
		}
	}

	public string EmptyStateMessage => "Try another function name, keyword, or category.";

	public string SelectedFunctionName => SelectedFunction?.Name ?? "Select a built-in function";
	public string SelectedFunctionCategory => SelectedFunction?.Category ?? string.Empty;
	public string SelectedFunctionSummary => SelectedFunction?.Summary ?? "Search by name, action, or keyword to find the helper you need.";
	public IReadOnlyList<string> SelectedFunctionSignatures => SelectedFunction?.Signatures ?? [];
	public IReadOnlyList<string> SelectedFunctionKeywords => SelectedFunction?.Keywords ?? [];
	public string SignatureSectionTitle => SelectedFunctionSignatures.Count == 1 ? "Signature" : "Signatures";

	public GlobalMethodsViewModel()
	{
		Categories = [AllCategories, .. AllFunctions.Select(function => function.Category).Distinct()];
		UpdateFilteredFunctions();
	}

	private void UpdateFilteredFunctions()
	{
		var searchTerms = SearchText
			.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

		var filteredFunctions = AllFunctions
			.Where(function => MatchesCategory(function) && MatchesSearch(function, searchTerms))
			.OrderBy(function => GetMatchRank(function, searchTerms))
			.ThenBy(function => function.Category)
			.ThenBy(function => function.Name)
			.ToArray();

		FilteredFunctions = filteredFunctions;

		if (filteredFunctions.Length == 0)
		{
			SelectedFunction = null;
			return;
		}

		if (SelectedFunction is null || !filteredFunctions.Contains(SelectedFunction))
		{
			SelectedFunction = filteredFunctions[0];
		}
	}

	private bool MatchesCategory(BuiltInFunctionReference function)
	{
		return SelectedCategory == AllCategories || function.Category == SelectedCategory;
	}

	private static bool MatchesSearch(BuiltInFunctionReference function, string[] searchTerms)
	{
		if (searchTerms.Length == 0)
		{
			return true;
		}

		return searchTerms.All(term => function.SearchHaystack.Contains(term, StringComparison.OrdinalIgnoreCase));
	}

	private static int GetMatchRank(BuiltInFunctionReference function, string[] searchTerms)
	{
		if (searchTerms.Length == 0)
		{
			return 0;
		}

		var firstTerm = searchTerms[0];
		if (function.Name.StartsWith(firstTerm, StringComparison.OrdinalIgnoreCase))
		{
			return 0;
		}

		if (function.Name.Contains(firstTerm, StringComparison.OrdinalIgnoreCase))
		{
			return 1;
		}

		if (function.Keywords.Any(keyword => keyword.Contains(firstTerm, StringComparison.OrdinalIgnoreCase)))
		{
			return 2;
		}

		if (function.Signatures.Any(signature => signature.Contains(firstTerm, StringComparison.OrdinalIgnoreCase)))
		{
			return 3;
		}

		if (function.Summary.Contains(firstTerm, StringComparison.OrdinalIgnoreCase))
		{
			return 4;
		}

		return 5;
	}

	public sealed class BuiltInFunctionReference(
		string name,
		string category,
		string summary,
		IReadOnlyList<string> signatures,
		IReadOnlyList<string> keywords)
	{
		public string Name { get; } = name;
		public string Category { get; } = category;
		public string Summary { get; } = summary;
		public IReadOnlyList<string> Signatures { get; } = signatures;
		public IReadOnlyList<string> Keywords { get; } = keywords;
		public string PrimarySignature { get; } = signatures[0];
		public string SearchHaystack { get; } = string.Join('\n', [name, category, summary, .. signatures, .. keywords]);
	}
}