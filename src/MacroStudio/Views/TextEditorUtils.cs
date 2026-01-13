using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit;
using System.Windows.Media;

namespace MacroStudio.Views;

public static class TextEditorUtils
{
	public static void ApplyDarkTheme(TextEditor editor)
	{
		editor.Background = new SolidColorBrush(Color.FromRgb(61, 61, 76));
		editor.Foreground = new SolidColorBrush(Colors.WhiteSmoke);
		editor.LineNumbersForeground = new SolidColorBrush(Colors.Gray);

		var highlighting = HighlightingManager.Instance.GetDefinition("C#");
		if (highlighting == null)
		{
			return;
		}

		void SetColor(string name, string hexColor)
		{
			var rule = highlighting.NamedHighlightingColors.First(x => x.Name == name);
			rule.Foreground = new SimpleHighlightingBrush((Color)ColorConverter.ConvertFromString(hexColor));
		}

		SetColor("Comment", "#FF57A64A");
		SetColor("String", "#FFD69D85");
		SetColor("StringInterpolation", "#FFFFD68F");
		SetColor("Char", "#FFD69D85");
		SetColor("Preprocessor", "#FF9B9B9B");
		SetColor("Punctuation", "White");
		SetColor("ValueTypeKeywords", "#FF00A0FF");
		SetColor("ReferenceTypeKeywords", "#FF559CD6");
		SetColor("MethodCall", "#FFDCDCAA");
		SetColor("NumberLiteral", "#FFB5CEA8");
		SetColor("ThisOrBaseReference", "#FF3A6A9B");
		SetColor("NullOrValueKeywords", "#FF559CD6");
		SetColor("Keywords", "#FF00A0FF");
		SetColor("GotoKeywords", "#FF00A0FF");
		SetColor("ContextKeywords", "#FF00A0FF");
		SetColor("ExceptionKeywords", "#FF00A0FF");
		SetColor("CheckedKeyword", "#FF559CD6");
		SetColor("UnsafeKeywords", "#FF559CD6");
		SetColor("OperatorKeywords", "#FFD69D85");
		SetColor("ParameterModifiers", "#FF559CD6");
		SetColor("Modifiers", "#FF559CD6");
		SetColor("Visibility", "#FF559CD6");
		SetColor("NamespaceKeywords", "#FF559CD6");
		SetColor("GetSetAddRemove", "#FF559CD6");
		SetColor("TrueFalse", "#FF00A0FF");
		SetColor("TypeKeywords", "#FF559CD6");
		SetColor("SemanticKeywords", "#FF559CD6");

		editor.SyntaxHighlighting = highlighting;
	}
}
