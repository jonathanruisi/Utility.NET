﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System.Threading;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

using JLR.Utility.UWP.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace UWPTestApp
{
	public sealed partial class MainPage : Page
	{
		private ThreadPoolTimer _timer;
		private bool _isPlayDirectionReversed;

		public MainPage()
		{
			InitializeComponent();
		}

		#region Event Handlers (Page)
		private void Page_Loaded(object sender, RoutedEventArgs e)
		{
			TextBoxA1.Text = Slider.Start.ToString("0.###");
			TextBoxA2.Text = Slider.End.ToString("0.###");
			TextBoxA3.Text = Slider.FramesPerSecond.ToString("N0");
			TextBoxB1.Text = Slider.Position.ToString("0.###");
			TextBoxC1.Text = Slider.MinorTickThickness.ToString("0.###");
			TextBoxC2.Text = Slider.MajorTickThickness.ToString("0.###");

			switch (Slider.PositionFollowMode)
			{
				case FollowMode.Advance:
					RadioButtonA1A.IsChecked = true;
					break;

				case FollowMode.Scroll:
					RadioButtonA1B.IsChecked = true;
					break;

				default:
					RadioButtonA1A.IsChecked = false;
					RadioButtonA1B.IsChecked = false;
					break;
			}

			Slider.DurationChanged += Slider_DurationChanged;
			Slider.PositionChanged += Slider_PositionChanged;
		}
		#endregion

		#region Event Handlers (Slider)
		private void Slider_DurationChanged(object sender, (decimal start, decimal end) e)
		{
			TextBoxA1.Text = e.start.ToString("0.###");
			TextBoxA2.Text = e.end.ToString("0.###");
		}

		private void Slider_PositionChanged(object sender, decimal e)
		{
			TextBoxB1.Text = e.ToString("0.###");
		}
		#endregion

		#region Event Handlers (Button)
		private void RadioButtonA1A_Checked(object sender, RoutedEventArgs e)
		{
			Slider.PositionFollowMode = FollowMode.Advance;
		}

		private void RadioButtonA1B_Checked(object sender, RoutedEventArgs e)
		{
			Slider.PositionFollowMode = FollowMode.Scroll;
		}

		private void RadioButtonA2A_Checked(object sender, RoutedEventArgs e)
		{
			_isPlayDirectionReversed = true;
		}

		private void RadioButtonA2B_Checked(object sender, RoutedEventArgs e)
		{
			_isPlayDirectionReversed = false;
		}

		private void ButtonA3_Click(object sender, RoutedEventArgs e)
		{
			if (!(bool) ButtonA3.IsChecked)
			{
				_timer?.Cancel();
				return;
			}

			var period = TimeSpan.FromSeconds(1 / (double) Slider.FramesPerSecond);
			_timer = ThreadPoolTimer.CreatePeriodicTimer(
				async source =>
				{
					if (_isPlayDirectionReversed)
					{
						await Dispatcher.RunAsync(
							CoreDispatcherPriority.Normal, () => { Slider.DecreasePosition(0, 1); });
					}
					else
					{
						await Dispatcher.RunAsync(
							CoreDispatcherPriority.Normal, () => { Slider.IncreasePosition(0, 1); });
					}
				}, period);
		}

		private void ButtonB1A_Click(object sender, RoutedEventArgs e)
		{
			Slider.Markers.Add(Slider.Position);
		}

		private void ButtonB1B_Click(object sender, RoutedEventArgs e)
		{
			Slider.Markers.Remove(Slider.Position);
		}

		private void ButtonB2A_Click(object sender, RoutedEventArgs e)
		{
			if (Slider.SelectionStart == null || Slider.SelectionEnd == null)
				return;

			Slider.Clips.Add(((decimal) Slider.SelectionStart, (decimal) Slider.SelectionEnd));
		}

		private void ButtonB2B_Click(object sender, RoutedEventArgs e)
		{
			if(Slider.SelectionStart == null || Slider.SelectionEnd == null)
				return;

			Slider.Clips.Remove(((decimal)Slider.SelectionStart, (decimal)Slider.SelectionEnd));
		}
		#endregion

		#region Event Handlers (TextBox)
		private void TextBoxA1_LostFocus(object sender, RoutedEventArgs e)
		{
			if (decimal.TryParse(TextBoxA1.Text, out var value))
			{
				Slider.Start = value;
			}
		}

		private void TextBoxA2_LostFocus(object sender, RoutedEventArgs e)
		{
			if (decimal.TryParse(TextBoxA2.Text, out var value))
			{
				Slider.End = value;
			}
		}

		private void TextBoxA3_LostFocus(object sender, RoutedEventArgs e)
		{
			if (int.TryParse(TextBoxA3.Text, out var value))
			{
				Slider.FramesPerSecond = value;
			}
		}

		private void TextBoxB1_LostFocus(object sender, RoutedEventArgs e)
		{
			if (decimal.TryParse(TextBoxB1.Text, out var value))
			{
				Slider.Position = value;
			}
		}

		private void TextBoxC1_LostFocus(object sender, RoutedEventArgs e)
		{
			if (double.TryParse(TextBoxC1.Text, out var value))
			{
				Slider.MinorTickThickness = value;
			}
		}

		private void TextBoxC2_LostFocus(object sender, RoutedEventArgs e)
		{
			if (double.TryParse(TextBoxC2.Text, out var value))
			{
				Slider.MajorTickThickness = value;
			}
		}
		#endregion
	}
}