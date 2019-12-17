using System;
using System.Linq;

using Windows.System.Threading;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

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

			foreach (var name in Enum.GetNames(typeof(MediaSlider.SnapIntervals)))
			{
				ComboBoxC1.Items?.Add(name);
			}

			ComboBoxC1.SelectedItem = Enum.GetName(typeof(MediaSlider.SnapIntervals), Slider.SnapToNearest);
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
							CoreDispatcherPriority.Normal, () => { Slider.DecreasePosition(1, MediaSlider.SnapIntervals.Frame); });
					}
					else
					{
						await Dispatcher.RunAsync(
							CoreDispatcherPriority.Normal, () => { Slider.IncreasePosition(1, MediaSlider.SnapIntervals.Frame); });
					}
				}, period);
		}

		private void ButtonB1A_Click(object sender, RoutedEventArgs e)
		{
			Slider.Markers.Add(new MediaSlider.MediaSliderMarker(TextBoxMarkerName.Text, Slider.Position));
		}

		private void ButtonB1B_Click(object sender, RoutedEventArgs e)
		{
			var selection = Slider.Markers.FirstOrDefault(marker => marker.Name == TextBoxMarkerName.Text);
			Slider.SelectedMarker = selection;
		}

		private void ButtonB1C_Click(object sender, RoutedEventArgs e)
		{
			foreach (var marker in Slider.Markers)
			{
				if (marker.Name != TextBoxMarkerName.Text)
					continue;

				Slider.Markers.Remove(marker);
				break;
			}
		}

		private void ButtonB2A_Click(object sender, RoutedEventArgs e)
		{
			if (Slider.SelectionStart == null || Slider.SelectionEnd == null)
				return;

			Slider.Clips.Add(new MediaSlider.MediaSliderClip(TextBoxClipName.Text,
			                                                 (decimal) Slider.SelectionStart,
			                                                 (decimal) Slider.SelectionEnd));
		}

		private void ButtonB2B_Click(object sender, RoutedEventArgs e)
		{
			var selection = Slider.Clips.First(clip => clip.Name == TextBoxClipName.Text);
			Slider.SelectedClip = selection;
		}

		private void ButtonB2C_Click(object sender, RoutedEventArgs e)
		{
			foreach (var clip in Slider.Clips)
			{
				if (clip.Name != TextBoxClipName.Text)
					continue;

				Slider.Clips.Remove(clip);
				break;
			}
		}

		private void ComboBoxC1_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (ComboBoxC1.SelectedItem == null)
				return;

			var selection = Enum.Parse<MediaSlider.SnapIntervals>((string) ComboBoxC1.SelectedItem);
			Slider.SnapToNearest = selection;
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
