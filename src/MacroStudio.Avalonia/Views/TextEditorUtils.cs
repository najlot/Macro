using Avalonia.Media;
using AvaloniaEdit;
using AvaloniaEdit.Highlighting;

namespace MacroStudio.Avalonia.Views;

internal static class TextEditorUtils
{
	private static readonly (string Name, string Color)[] HighlightColors =
	[
		("Comment", "#57A64A"),
		("String", "#D69D85"),
		("StringInterpolation", "#FFD68F"),
		("Char", "#D69D85"),
		("Preprocessor", "#9B9B9B"),
		("Punctuation", "#EEF4FF"),
		("ValueTypeKeywords", "#4FC1FF"),
		("ReferenceTypeKeywords", "#569CD6"),
		("MethodCall", "#DCDCAA"),
		("NumberLiteral", "#B5CEA8"),
		("ThisOrBaseReference", "#9CDCFE"),
		("NullOrValueKeywords", "#569CD6"),
		("Keywords", "#4FC1FF"),
		("GotoKeywords", "#4FC1FF"),
		("ContextKeywords", "#4FC1FF"),
		("ExceptionKeywords", "#4FC1FF"),
		("CheckedKeyword", "#569CD6"),
		("UnsafeKeywords", "#569CD6"),
		("OperatorKeywords", "#D69D85"),
		("ParameterModifiers", "#569CD6"),
		("Modifiers", "#569CD6"),
		("Visibility", "#569CD6"),
		("NamespaceKeywords", "#569CD6"),
		("GetSetAddRemove", "#569CD6"),
		("TrueFalse", "#4FC1FF"),
		("TypeKeywords", "#569CD6"),
		("SemanticKeywords", "#569CD6")
	];

	public static void ApplyDarkTheme(TextEditor editor)
	{
		editor.Background = new SolidColorBrush(Color.Parse("#0B1220"));
		editor.Foreground = new SolidColorBrush(Color.Parse("#EEF4FF"));
		editor.LineNumbersForeground = new SolidColorBrush(Color.Parse("#6F86A8"));

		var highlighting = HighlightingManager.Instance.GetDefinition("C#");
		if (highlighting is null)
		{
			return;
		}

		foreach (var (name, color) in HighlightColors)
		{
			var rule = highlighting.NamedHighlightingColors.FirstOrDefault(highlightingColor => highlightingColor.Name == name);
			if (rule is null)
			{
				continue;
			}

			rule.Foreground = new SimpleHighlightingBrush(Color.Parse(color));
		}

		editor.SyntaxHighlighting = highlighting;
	}
}