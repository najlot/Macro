using System;
using System.Drawing;
using System.Threading;

namespace MacroStudio.Core;

public partial class InputSimulator
{
	public static void SimulateLongClick(Func<bool> release)
	{
		Thread.Sleep(100);
		Mouse.LeftDown();

		while (!release())
		{
			Thread.Sleep(100);
		}

		Mouse.LeftUp();
	}

	public static void SimulateLongClick(int duration = 1000)
	{
		Thread.Sleep(100);
		Mouse.LeftDown();
		Thread.Sleep(duration);
		Mouse.LeftUp();
	}

	public static void SimulateClick()
	{
		Thread.Sleep(100);
		Mouse.LeftDown();
		Thread.Sleep(25);
		Mouse.LeftUp();
	}

	public static void SimulateRightClick()
	{
		Thread.Sleep(100);
		Mouse.RightDown();
		Thread.Sleep(25);
		Mouse.RightUp();
	}

	public static void SimulateClick(Rectangle rectangle)
	{
		var point = new Point(rectangle.X, rectangle.Y);
		SimulateClick(point, rectangle.Width / 2, rectangle.Height / 2);
	}

	public static void SimulateClick(Point point, int addX = 0, int addY = 0)
	{
		Mouse.SetCursorPosition(point.X + addX, point.Y + addY);
		SimulateClick();
	}
}
