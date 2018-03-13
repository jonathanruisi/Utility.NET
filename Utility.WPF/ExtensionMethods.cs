// ┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  FILE:       ExtensionMethods.cs
// ┃  PROJECT:    Utility.WPF
// ┃  SOLUTION:   Utility.NET
// ┃  CREATED:    2018-03-13 @ 12:45 AM
// ┣━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  AUTHOR:     Jonathan Ruisi
// ┃  EMAIL:      JonathanRuisi@gmail.com
// ┣━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  GIT REPO:   https://github.com/jonathanruisi/Utility.NET
// ┃  LICENSE:    https://github.com/jonathanruisi/Utility.NET/blob/master/LICENSE
// ┗━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

using System.Windows;
using System.Windows.Media;

namespace JLR.Utility.WPF
{
	public static class ExtensionMethods
	{
		public static T FindVisualChild<T>(this DependencyObject depObj) where T : DependencyObject
		{
			if (depObj != null)
			{
				for (var i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
				{
					DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
					if (child is T)
					{
						return (T)child;
					}

					var childItem = FindVisualChild<T>(child);
					if (childItem != null)
						return childItem;
				}
			}

			return null;
		}

		public static Thickness Add(this Thickness thickness, Thickness value)
		{
			return new Thickness(
				thickness.Left + value.Left,
				thickness.Top + value.Top,
				thickness.Right + value.Right,
				thickness.Bottom + value.Bottom);
		}

		public static Thickness Subtract(this Thickness thickness, Thickness value)
		{
			return new Thickness(
				thickness.Left - value.Left,
				thickness.Top - value.Top,
				thickness.Right - value.Right,
				thickness.Bottom - value.Bottom);
		}

		public static Thickness Scale(this Thickness thickness, double value)
		{
			return new Thickness(
				thickness.Left * value,
				thickness.Top * value,
				thickness.Right * value,
				thickness.Bottom * value);
		}
	}
}