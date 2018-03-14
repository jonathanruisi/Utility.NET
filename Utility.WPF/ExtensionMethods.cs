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

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

using JLR.Utility.NET.Color;

namespace JLR.Utility.WPF
{
	public static class ExtensionMethods
	{
		#region System.Windows.DependencyObject
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
		#endregion

		#region System.Windows.Thickness
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
		#endregion

		#region System.Windows.Controls.ListBox
		public static IEnumerable<int> SelectedIndices(this ListBox listBox, ListSortDirection direction)
		{
			return direction == ListSortDirection.Ascending
				? from index in from item in listBox.SelectedItems.Cast<object>() select listBox.Items.IndexOf(item)
				  orderby index ascending
				  select index
				: from index in from item in listBox.SelectedItems.Cast<object>() select listBox.Items.IndexOf(item)
				  orderby index descending
				  select index;
		}
		#endregion

		#region System.Windows.Controls.Slider
		public static void SnapToNearestTick(this Slider slider)
		{
			var previousTick       = (int)((slider.Value - slider.Minimum) / slider.TickFrequency) * slider.TickFrequency;
			var distanceToPrevious = slider.Value - previousTick;
			var distanceToNext     = previousTick + slider.TickFrequency - slider.Value;
			slider.Value = distanceToPrevious < distanceToNext ? previousTick : previousTick + slider.TickFrequency;
		}
		#endregion

		#region JLR.Utility.NET.Color
		public static System.Windows.Media.Color ToSystemWindowsMediaColor(this ColorSpace colorSpace)
		{
			if (colorSpace is Rgba rgba)
				return Color.FromArgb((byte)rgba.A, (byte)rgba.R, (byte)rgba.G, (byte)rgba.B);
			else
			{
				var rgb = colorSpace.ToColorSpace<Rgb>();
				return Color.FromRgb((byte)rgb.R, (byte)rgb.G, (byte)rgb.B);
			}
		}

		public static ColorSpace ToColorSpaceRgba(this System.Windows.Media.Color color)
		{
			return new Rgba(color.R, color.G, color.B, color.A);
		}
		#endregion
	}
}