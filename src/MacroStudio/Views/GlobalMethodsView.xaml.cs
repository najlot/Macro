using System;
using System.Windows;
using System.Windows.Controls;

namespace MacroStudio.Views;

public partial class GlobalMethodsView : UserControl
{
	public GlobalMethodsView()
	{
		InitializeComponent();
		TextEditorUtils.ApplyDarkTheme(Code);
		Loaded += OnLoaded;
		IsVisibleChanged += OnIsVisibleChanged;
	}

	private void OnLoaded(object sender, RoutedEventArgs e)
	{
		FocusSearchBox();
		UpdateSearchResult();
	}

	private void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		if (IsVisible)
		{
			FocusSearchBox();
		}
	}

	private void SearchBox_OnTextChanged(object sender, TextChangedEventArgs e)
	{
		UpdateSearchResult();
	}

	private void UpdateSearchResult()
	{
		if (Code.Document is null)
		{
			return;
		}

		var searchText = SearchBox.Text;
		if (string.IsNullOrWhiteSpace(searchText))
		{
			Code.Select(0, 0);
			return;
		}

		var offset = Code.Document.Text.IndexOf(searchText, StringComparison.OrdinalIgnoreCase);
		if (offset < 0)
		{
			Code.Select(0, 0);
			return;
		}

		Code.Select(offset, searchText.Length);
		Code.CaretOffset = offset;
		var line = Code.Document.GetLineByOffset(offset);
		Code.ScrollTo(line.LineNumber, 0);
		Code.TextArea.Caret.BringCaretToView();
	}

	private void FocusSearchBox()
	{
		SearchBox.Dispatcher.BeginInvoke(SearchBox.Focus);
	}
}
