using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;

namespace MacroStudio.Avalonia.Views;

internal sealed class MessageDialogWindow : Window
{
	public MessageDialogWindow(string title, string message)
	{
		Title = title;
		Width = 520;
		CanResize = false;
		WindowStartupLocation = WindowStartupLocation.CenterOwner;

		Content = new Border
		{
			Padding = new Thickness(20),
			Background = new SolidColorBrush(Color.Parse("#101927")),
			Child = new StackPanel
			{
				Spacing = 16,
				Children =
				{
					new TextBlock
					{
						Text = message,
						TextWrapping = TextWrapping.Wrap,
						MaxWidth = 460
					},
					new Button
					{
						Content = "OK",
						HorizontalAlignment = HorizontalAlignment.Right,
						MinWidth = 90
					}
				}
			}
		};

		var button = ((StackPanel)((Border)Content!).Child!).Children.OfType<Button>().First();
		button.Click += (_, _) => Close();
	}
}