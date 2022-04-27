// ┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  FILE:       ColorSetCreator.xaml.cs
// ┃  PROJECT:    Utility.WPF
// ┃  SOLUTION:   Utility.NET
// ┃  CREATED:    2015-12-25 @ 1:15 AM
// ┣━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  AUTHOR:     Jonathan Ruisi
// ┃  EMAIL:      JonathanRuisi@gmail.com
// ┣━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  GIT REPO:   https://github.com/jonathanruisi/Utility.NET
// ┃  LICENSE:    https://github.com/jonathanruisi/Utility.NET/blob/master/LICENSE
// ┗━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;

using JLR.Utility.NETFramework;
using JLR.Utility.NETFramework.Color;

namespace JLR.Utility.WPF.Controls
{
	public partial class ColorSetCreator : UserControl
	{
		#region Fields
		private bool    _updatingSliders,    _updatingColor,        _isRandomMode;
		private Binding _randomColorBinding, _currentColor1Binding, _currentColor2Binding;
		#endregion

		#region Dependency Properties
		public static readonly DependencyProperty ColorsProperty = DependencyProperty.Register(
			"Colors",
			typeof(ObservableCollection<ColorSpace>),
			typeof(ColorSetCreator));

		public ObservableCollection<ColorSpace> Colors
		{
			get { return (ObservableCollection<ColorSpace>)GetValue(ColorsProperty); }
			set { SetValue(ColorsProperty, value); }
		}

		public static readonly DependencyProperty CurrentColorProperty = DependencyProperty.Register(
			"CurrentColor",
			typeof(ColorSpace),
			typeof(ColorSetCreator),
			new FrameworkPropertyMetadata(ColorSpace.OpaqueBlack, OnDependencyPropertyChanged));

		public ColorSpace CurrentColor
		{
			get { return (ColorSpace)GetValue(CurrentColorProperty); }
			set { SetValue(CurrentColorProperty, value); }
		}

		public static readonly DependencyProperty MidgroundProperty = DependencyProperty.Register(
			"Midground",
			typeof(Brush),
			typeof(ColorSetCreator),
			new FrameworkPropertyMetadata(new SolidColorBrush(Color.FromRgb(32, 32, 32))));

		public Brush Midground
		{
			get { return (Brush)GetValue(MidgroundProperty); }
			set { SetValue(MidgroundProperty, value); }
		}

		public static readonly DependencyProperty AccentProperty = DependencyProperty.Register(
			"Accent",
			typeof(Brush),
			typeof(ColorSetCreator),
			new FrameworkPropertyMetadata(new SolidColorBrush(System.Windows.Media.Colors.LightGray)));

		public Brush Accent { get { return (Brush)GetValue(AccentProperty); } set { SetValue(AccentProperty, value); } }

		public static readonly DependencyProperty IsColorChooserOnlyProperty = DependencyProperty.Register(
			"IsColorChooserOnly",
			typeof(bool),
			typeof(ColorSetCreator),
			new FrameworkPropertyMetadata(false, OnDependencyPropertyChanged));

		public bool IsColorChooserOnly
		{
			get { return (bool)GetValue(IsColorChooserOnlyProperty); }
			set { SetValue(IsColorChooserOnlyProperty, value); }
		}
		#endregion

		#region Dependency Properties (Private)
		private static readonly DependencyProperty CurrentColor1Property = DependencyProperty.Register(
			"CurrentColor1",
			typeof(ColorSpace),
			typeof(ColorSetCreator),
			new FrameworkPropertyMetadata(ColorSpace.OpaqueBlack, OnDependencyPropertyChanged));

		private ColorSpace CurrentColor1
		{
			get { return (ColorSpace)GetValue(CurrentColor1Property); }
			set { SetValue(CurrentColor1Property, value); }
		}

		private static readonly DependencyProperty CurrentColor2Property = DependencyProperty.Register(
			"CurrentColor2",
			typeof(ColorSpace),
			typeof(ColorSetCreator),
			new FrameworkPropertyMetadata(ColorSpace.OpaqueBlack));

		private ColorSpace CurrentColor2
		{
			get { return (ColorSpace)GetValue(CurrentColor2Property); }
			set { SetValue(CurrentColor2Property, value); }
		}

		private static readonly DependencyProperty RandomColorProperty = DependencyProperty.Register(
			"RandomColor",
			typeof(ColorSpace),
			typeof(ColorSetCreator),
			new FrameworkPropertyMetadata(ColorSpace.OpaqueBlack, OnDependencyPropertyChanged));

		private ColorSpace RandomColor
		{
			get { return (ColorSpace)GetValue(RandomColorProperty); }
			set { SetValue(RandomColorProperty, value); }
		}
		#endregion

		#region Constructor
		public ColorSetCreator()
		{
			InitializeComponent();
			Colors           = new ObservableCollection<ColorSpace>();
			_updatingSliders = false;
			_updatingColor   = false;
			_isRandomMode    = false;
		}
		#endregion

		#region Property Changed Callbacks
		private static void OnDependencyPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var colorSetCreator = d as ColorSetCreator;
			if (colorSetCreator == null) return;
			if (e.Property == IsColorChooserOnlyProperty)
			{
				if ((bool)e.NewValue)
				{
					colorSetCreator.StackPanelColorSetTools.Visibility = Visibility.Collapsed;
					colorSetCreator.ListBoxColorSet.Visibility         = Visibility.Collapsed;
					colorSetCreator.RightSideGridSplitter.Visibility   = Visibility.Collapsed;
					colorSetCreator.ListBoxColorSetRow.Height          = new GridLength(0.0, GridUnitType.Pixel);
					colorSetCreator.ListBoxColorSetRow.MinHeight       = 0;
				}
				else
				{
					colorSetCreator.StackPanelColorSetTools.Visibility = Visibility.Visible;
					colorSetCreator.ListBoxColorSet.Visibility         = Visibility.Visible;
					colorSetCreator.RightSideGridSplitter.Visibility   = Visibility.Visible;
					colorSetCreator.ListBoxColorSetRow.Height          = new GridLength(1.5, GridUnitType.Star);
					colorSetCreator.ListBoxColorSetRow.MinHeight       = 75;
				}
			}
			else if ((e.Property == CurrentColor1Property || e.Property == RandomColorProperty) && e.NewValue != null &&
				!colorSetCreator._updatingColor)
			{
				colorSetCreator._updatingColor = true;
				colorSetCreator.CurrentColor   = e.NewValue as ColorSpace;
				colorSetCreator._updatingColor = false;
			}
			else if (e.Property == CurrentColorProperty && e.NewValue != null && !colorSetCreator._updatingColor)
			{
				colorSetCreator._updatingColor           = true;
				colorSetCreator.CheckBoxRandom.IsChecked = false;
				colorSetCreator.CurrentColor1            = e.NewValue as ColorSpace;
				colorSetCreator.CurrentColor2            = e.NewValue as ColorSpace;
				colorSetCreator.SynchronizeCurrentColorValues();
				colorSetCreator._updatingColor = false;
			}
		}
		#endregion

		#region Private Methods
		private void SynchronizeCurrentColorValues()
		{
			var selectedValue = ComboBoxColorSpace.SelectedValue as string;
			if (selectedValue != null)
			{
				if (selectedValue != ColorSpace.GetFriendlyName(CurrentColor1))
					ComboBoxColorSpace.SelectedValue = ColorSpace.GetFriendlyName(CurrentColor1);
				else
				{
					_updatingSliders = true;
					UpdateSliderValues();
				}
			}
		}

		private void UpdateSliderValues()
		{
			SliderA1.Value = CurrentColor1[0];
			SliderA2.Value = CurrentColor2[0];
			SliderB1.Value = CurrentColor1[1];
			SliderB2.Value = CurrentColor2[1];
			SliderC1.Value = CurrentColor1[2];
			SliderC2.Value = CurrentColor2[2];
			if ((CurrentColor1 is Cmyk && CurrentColor2 is Cmyk) || (CurrentColor1 is Rgba && CurrentColor2 is Rgba))
			{
				SliderD1.Value = CurrentColor1[3];
				SliderD2.Value = CurrentColor2[3];
			}

			_updatingSliders = false;
		}

		private void ConfigureSlider(ref Slider slider,
									 double minimum,
									 double maximum,
									 double smallChange,
									 double largeChange,
									 double tickFrequency)
		{
			slider.Minimum       = minimum;
			slider.Maximum       = maximum;
			slider.SmallChange   = smallChange;
			slider.LargeChange   = largeChange;
			slider.TickFrequency = tickFrequency;
		}

		private void ConfigureBinding(ref Slider source,
									  ref TextBox destination,
									  string formatString,
									  IValueConverter converter = null)
		{
			var binding = new Binding("Value")
			{
				Source       = source,
				Mode         = BindingMode.TwoWay,
				Converter    = converter,
				StringFormat = formatString
			};
			destination.SetBinding(TextBox.TextProperty, binding);
		}

		private void ConvertToCmy()
		{
			CurrentColor1         = CurrentColor1.ToColorSpace<Cmy>();
			CurrentColor2         = CurrentColor2.ToColorSpace<Cmy>();
			_updatingSliders      = true;
			TextBlockLabelA1.Text = "C";
			TextBlockLabelA2.Text = "C";
			TextBlockLabelB1.Text = "M";
			TextBlockLabelB2.Text = "M";
			TextBlockLabelC1.Text = "Y";
			TextBlockLabelC2.Text = "Y";
			ConfigureSlider(ref SliderA1, 0.0, 1.0, 0.01, 0.1, 0.1);
			ConfigureSlider(ref SliderA2, 0.0, 1.0, 0.01, 0.1, 0.1);
			ConfigureSlider(ref SliderB1, 0.0, 1.0, 0.01, 0.1, 0.1);
			ConfigureSlider(ref SliderB2, 0.0, 1.0, 0.01, 0.1, 0.1);
			ConfigureSlider(ref SliderC1, 0.0, 1.0, 0.01, 0.1, 0.1);
			ConfigureSlider(ref SliderC2, 0.0, 1.0, 0.01, 0.1, 0.1);
			ConfigureBinding(ref SliderA1, ref TextBoxValueA1, "{0:F1}%", new PercentageConverter());
			ConfigureBinding(ref SliderA2, ref TextBoxValueA2, "{0:F1}%", new PercentageConverter());
			ConfigureBinding(ref SliderB1, ref TextBoxValueB1, "{0:F1}%", new PercentageConverter());
			ConfigureBinding(ref SliderB2, ref TextBoxValueB2, "{0:F1}%", new PercentageConverter());
			ConfigureBinding(ref SliderC1, ref TextBoxValueC1, "{0:F1}%", new PercentageConverter());
			ConfigureBinding(ref SliderC2, ref TextBoxValueC2, "{0:F1}%", new PercentageConverter());
			UpdateSliderValues();
		}

		private void ConvertToCmyk()
		{
			CurrentColor1         = CurrentColor1.ToColorSpace<Cmyk>();
			CurrentColor2         = CurrentColor2.ToColorSpace<Cmyk>();
			_updatingSliders      = true;
			TextBlockLabelA1.Text = "C";
			TextBlockLabelA2.Text = "C";
			TextBlockLabelB1.Text = "M";
			TextBlockLabelB2.Text = "M";
			TextBlockLabelC1.Text = "Y";
			TextBlockLabelC2.Text = "Y";
			TextBlockLabelD1.Text = "K";
			TextBlockLabelD2.Text = "K";
			ConfigureSlider(ref SliderA1, 0.0, 1.0, 0.01, 0.1, 0.1);
			ConfigureSlider(ref SliderA2, 0.0, 1.0, 0.01, 0.1, 0.1);
			ConfigureSlider(ref SliderB1, 0.0, 1.0, 0.01, 0.1, 0.1);
			ConfigureSlider(ref SliderB2, 0.0, 1.0, 0.01, 0.1, 0.1);
			ConfigureSlider(ref SliderC1, 0.0, 1.0, 0.01, 0.1, 0.1);
			ConfigureSlider(ref SliderC2, 0.0, 1.0, 0.01, 0.1, 0.1);
			ConfigureSlider(ref SliderD1, 0.0, 1.0, 0.01, 0.1, 0.1);
			ConfigureSlider(ref SliderD2, 0.0, 1.0, 0.01, 0.1, 0.1);
			ConfigureBinding(ref SliderA1, ref TextBoxValueA1, "{0:F1}%", new PercentageConverter());
			ConfigureBinding(ref SliderA2, ref TextBoxValueA2, "{0:F1}%", new PercentageConverter());
			ConfigureBinding(ref SliderB1, ref TextBoxValueB1, "{0:F1}%", new PercentageConverter());
			ConfigureBinding(ref SliderB2, ref TextBoxValueB2, "{0:F1}%", new PercentageConverter());
			ConfigureBinding(ref SliderC1, ref TextBoxValueC1, "{0:F1}%", new PercentageConverter());
			ConfigureBinding(ref SliderC2, ref TextBoxValueC2, "{0:F1}%", new PercentageConverter());
			ConfigureBinding(ref SliderD1, ref TextBoxValueD1, "{0:F1}%", new PercentageConverter());
			ConfigureBinding(ref SliderD2, ref TextBoxValueD2, "{0:F1}%", new PercentageConverter());
			UpdateSliderValues();
		}

		private void ConvertToHsl()
		{
			CurrentColor1         = CurrentColor1.ToColorSpace<Hsl>();
			CurrentColor2         = CurrentColor2.ToColorSpace<Hsl>();
			_updatingSliders      = true;
			TextBlockLabelA1.Text = "H";
			TextBlockLabelA2.Text = "H";
			TextBlockLabelB1.Text = "S";
			TextBlockLabelB2.Text = "S";
			TextBlockLabelC1.Text = "L";
			TextBlockLabelC2.Text = "L";
			ConfigureSlider(ref SliderA1, 0.0, 360.0, 1.0,  10.0, 10.0);
			ConfigureSlider(ref SliderA2, 0.0, 360.0, 1.0,  10.0, 10.0);
			ConfigureSlider(ref SliderB1, 0.0, 1.0,   0.01, 0.1,  0.1);
			ConfigureSlider(ref SliderB2, 0.0, 1.0,   0.01, 0.1,  0.1);
			ConfigureSlider(ref SliderC1, 0.0, 1.0,   0.01, 0.1,  0.1);
			ConfigureSlider(ref SliderC2, 0.0, 1.0,   0.01, 0.1,  0.1);
			ConfigureBinding(ref SliderA1, ref TextBoxValueA1, "{0:F1}°", new DegreeConverter());
			ConfigureBinding(ref SliderA2, ref TextBoxValueA2, "{0:F1}°", new DegreeConverter());
			ConfigureBinding(ref SliderB1, ref TextBoxValueB1, "{0:F1}%", new PercentageConverter());
			ConfigureBinding(ref SliderB2, ref TextBoxValueB2, "{0:F1}%", new PercentageConverter());
			ConfigureBinding(ref SliderC1, ref TextBoxValueC1, "{0:F1}%", new PercentageConverter());
			ConfigureBinding(ref SliderC2, ref TextBoxValueC2, "{0:F1}%", new PercentageConverter());
			UpdateSliderValues();
		}

		private void ConvertToHsv()
		{
			CurrentColor1         = CurrentColor1.ToColorSpace<Hsv>();
			CurrentColor2         = CurrentColor2.ToColorSpace<Hsv>();
			_updatingSliders      = true;
			TextBlockLabelA1.Text = "H";
			TextBlockLabelA2.Text = "H";
			TextBlockLabelB1.Text = "S";
			TextBlockLabelB2.Text = "S";
			TextBlockLabelC1.Text = "V";
			TextBlockLabelC2.Text = "V";
			ConfigureSlider(ref SliderA1, 0.0, 360.0, 1.0,  10.0, 10.0);
			ConfigureSlider(ref SliderA2, 0.0, 360.0, 1.0,  10.0, 10.0);
			ConfigureSlider(ref SliderB1, 0.0, 1.0,   0.01, 0.1,  0.1);
			ConfigureSlider(ref SliderB2, 0.0, 1.0,   0.01, 0.1,  0.1);
			ConfigureSlider(ref SliderC1, 0.0, 1.0,   0.01, 0.1,  0.1);
			ConfigureSlider(ref SliderC2, 0.0, 1.0,   0.01, 0.1,  0.1);
			ConfigureBinding(ref SliderA1, ref TextBoxValueA1, "{0:F1}°", new DegreeConverter());
			ConfigureBinding(ref SliderA2, ref TextBoxValueA2, "{0:F1}°", new DegreeConverter());
			ConfigureBinding(ref SliderB1, ref TextBoxValueB1, "{0:F1}%", new PercentageConverter());
			ConfigureBinding(ref SliderB2, ref TextBoxValueB2, "{0:F1}%", new PercentageConverter());
			ConfigureBinding(ref SliderC1, ref TextBoxValueC1, "{0:F1}%", new PercentageConverter());
			ConfigureBinding(ref SliderC2, ref TextBoxValueC2, "{0:F1}%", new PercentageConverter());
			UpdateSliderValues();
		}

		private void ConvertToHunterLab()
		{
			CurrentColor1         = CurrentColor1.ToColorSpace<HunterLab>();
			CurrentColor2         = CurrentColor2.ToColorSpace<HunterLab>();
			_updatingSliders      = true;
			TextBlockLabelA1.Text = "L";
			TextBlockLabelA2.Text = "L";
			TextBlockLabelB1.Text = "a";
			TextBlockLabelB2.Text = "a";
			TextBlockLabelC1.Text = "b";
			TextBlockLabelC2.Text = "b";
			ConfigureSlider(ref SliderA1, 0.0,    100.0, 1.0, 10.0, 10.0);
			ConfigureSlider(ref SliderA2, 0.0,    100.0, 1.0, 10.0, 10.0);
			ConfigureSlider(ref SliderB1, -128.0, 128.0, 1.0, 8.0,  8.0);
			ConfigureSlider(ref SliderB2, -128.0, 128.0, 1.0, 8.0,  8.0);
			ConfigureSlider(ref SliderC1, -128.0, 128.0, 1.0, 8.0,  8.0);
			ConfigureSlider(ref SliderC2, -128.0, 128.0, 1.0, 8.0,  8.0);
			ConfigureBinding(ref SliderA1, ref TextBoxValueA1, "{0:F1}%");
			ConfigureBinding(ref SliderA2, ref TextBoxValueA2, "{0:F1}%");
			ConfigureBinding(ref SliderB1, ref TextBoxValueB1, "{0:F1}");
			ConfigureBinding(ref SliderB2, ref TextBoxValueB2, "{0:F1}");
			ConfigureBinding(ref SliderC1, ref TextBoxValueC1, "{0:F1}");
			ConfigureBinding(ref SliderC2, ref TextBoxValueC2, "{0:F1}");
			UpdateSliderValues();
		}

		private void ConvertToLab()
		{
			CurrentColor1         = CurrentColor1.ToColorSpace<Lab>();
			CurrentColor2         = CurrentColor2.ToColorSpace<Lab>();
			_updatingSliders      = true;
			TextBlockLabelA1.Text = "L";
			TextBlockLabelA2.Text = "L";
			TextBlockLabelB1.Text = "a";
			TextBlockLabelB2.Text = "a";
			TextBlockLabelC1.Text = "b";
			TextBlockLabelC2.Text = "b";
			ConfigureSlider(ref SliderA1, 0.0,    100.0, 1.0, 10.0, 10.0);
			ConfigureSlider(ref SliderA2, 0.0,    100.0, 1.0, 10.0, 10.0);
			ConfigureSlider(ref SliderB1, -128.0, 128.0, 1.0, 8.0,  8.0);
			ConfigureSlider(ref SliderB2, -128.0, 128.0, 1.0, 8.0,  8.0);
			ConfigureSlider(ref SliderC1, -128.0, 128.0, 1.0, 8.0,  8.0);
			ConfigureSlider(ref SliderC2, -128.0, 128.0, 1.0, 8.0,  8.0);
			ConfigureBinding(ref SliderA1, ref TextBoxValueA1, "{0:F1}%");
			ConfigureBinding(ref SliderA2, ref TextBoxValueA2, "{0:F1}%");
			ConfigureBinding(ref SliderB1, ref TextBoxValueB1, "{0:F1}");
			ConfigureBinding(ref SliderB2, ref TextBoxValueB2, "{0:F1}");
			ConfigureBinding(ref SliderC1, ref TextBoxValueC1, "{0:F1}");
			ConfigureBinding(ref SliderC2, ref TextBoxValueC2, "{0:F1}");
			UpdateSliderValues();
		}

		private void ConvertToLch()
		{
			CurrentColor1         = CurrentColor1.ToColorSpace<Lch>();
			CurrentColor2         = CurrentColor2.ToColorSpace<Lch>();
			_updatingSliders      = true;
			TextBlockLabelA1.Text = "L";
			TextBlockLabelA2.Text = "L";
			TextBlockLabelB1.Text = "C";
			TextBlockLabelB2.Text = "C";
			TextBlockLabelC1.Text = "h";
			TextBlockLabelC2.Text = "h";
			ConfigureSlider(ref SliderA1, 0.0, 100.0, 1.0, 10.0, 10.0);
			ConfigureSlider(ref SliderA2, 0.0, 100.0, 1.0, 10.0, 10.0);
			ConfigureSlider(ref SliderB1, 0.0, 100.0, 1.0, 10.0, 10.0);
			ConfigureSlider(ref SliderB2, 0.0, 100.0, 1.0, 10.0, 10.0);
			ConfigureSlider(ref SliderC1, 0.0, 360.0, 1.0, 10.0, 10.0);
			ConfigureSlider(ref SliderC2, 0.0, 360.0, 1.0, 10.0, 10.0);
			ConfigureBinding(ref SliderA1, ref TextBoxValueA1, "{0:F1}%");
			ConfigureBinding(ref SliderA2, ref TextBoxValueA2, "{0:F1}%");
			ConfigureBinding(ref SliderB1, ref TextBoxValueB1, "{0:F1}%");
			ConfigureBinding(ref SliderB2, ref TextBoxValueB2, "{0:F1}%");
			ConfigureBinding(ref SliderC1, ref TextBoxValueC1, "{0:F1}°", new DegreeConverter());
			ConfigureBinding(ref SliderC2, ref TextBoxValueC2, "{0:F1}°", new DegreeConverter());
			UpdateSliderValues();
		}

		private void ConvertToLuv()
		{
			CurrentColor1         = CurrentColor1.ToColorSpace<Luv>();
			CurrentColor2         = CurrentColor2.ToColorSpace<Luv>();
			_updatingSliders      = true;
			TextBlockLabelA1.Text = "L";
			TextBlockLabelA2.Text = "L";
			TextBlockLabelB1.Text = "u";
			TextBlockLabelB2.Text = "u";
			TextBlockLabelC1.Text = "v";
			TextBlockLabelC2.Text = "v";
			ConfigureSlider(ref SliderA1, 0.0,    100.0, 1.0, 10.0, 10.0);
			ConfigureSlider(ref SliderA2, 0.0,    100.0, 1.0, 10.0, 10.0);
			ConfigureSlider(ref SliderB1, -134.0, 224.0, 1.0, 10.0, 10.0);
			ConfigureSlider(ref SliderB2, -134.0, 224.0, 1.0, 10.0, 10.0);
			ConfigureSlider(ref SliderC1, -140.0, 122.0, 1.0, 10.0, 10.0);
			ConfigureSlider(ref SliderC2, -140.0, 122.0, 1.0, 10.0, 10.0);
			ConfigureBinding(ref SliderA1, ref TextBoxValueA1, "{0:F1}%");
			ConfigureBinding(ref SliderA2, ref TextBoxValueA2, "{0:F1}%");
			ConfigureBinding(ref SliderB1, ref TextBoxValueB1, "{0:F1}");
			ConfigureBinding(ref SliderB2, ref TextBoxValueB2, "{0:F1}");
			ConfigureBinding(ref SliderC1, ref TextBoxValueC1, "{0:F1}");
			ConfigureBinding(ref SliderC2, ref TextBoxValueC2, "{0:F1}");
			UpdateSliderValues();
		}

		private void ConvertToRgb()
		{
			CurrentColor1         = CurrentColor1.ToColorSpace<Rgb>();
			CurrentColor2         = CurrentColor2.ToColorSpace<Rgb>();
			_updatingSliders      = true;
			TextBlockLabelA1.Text = "R";
			TextBlockLabelA2.Text = "R";
			TextBlockLabelB1.Text = "G";
			TextBlockLabelB2.Text = "G";
			TextBlockLabelC1.Text = "B";
			TextBlockLabelC2.Text = "B";
			ConfigureSlider(ref SliderA1, 0.0, 255.0, 1.0, 16.0, 16.0);
			ConfigureSlider(ref SliderA2, 0.0, 255.0, 1.0, 16.0, 16.0);
			ConfigureSlider(ref SliderB1, 0.0, 255.0, 1.0, 16.0, 16.0);
			ConfigureSlider(ref SliderB2, 0.0, 255.0, 1.0, 16.0, 16.0);
			ConfigureSlider(ref SliderC1, 0.0, 255.0, 1.0, 16.0, 16.0);
			ConfigureSlider(ref SliderC2, 0.0, 255.0, 1.0, 16.0, 16.0);
			ConfigureBinding(ref SliderA1, ref TextBoxValueA1, "{0:F1}");
			ConfigureBinding(ref SliderA2, ref TextBoxValueA2, "{0:F1}");
			ConfigureBinding(ref SliderB1, ref TextBoxValueB1, "{0:F1}");
			ConfigureBinding(ref SliderB2, ref TextBoxValueB2, "{0:F1}");
			ConfigureBinding(ref SliderC1, ref TextBoxValueC1, "{0:F1}");
			ConfigureBinding(ref SliderC2, ref TextBoxValueC2, "{0:F1}");
			UpdateSliderValues();
		}

		private void ConvertToRgba()
		{
			CurrentColor1         = CurrentColor1.ToColorSpace<Rgba>();
			CurrentColor2         = CurrentColor2.ToColorSpace<Rgba>();
			_updatingSliders      = true;
			TextBlockLabelA1.Text = "R";
			TextBlockLabelA2.Text = "R";
			TextBlockLabelB1.Text = "G";
			TextBlockLabelB2.Text = "G";
			TextBlockLabelC1.Text = "B";
			TextBlockLabelC2.Text = "B";
			TextBlockLabelD1.Text = "A";
			TextBlockLabelD2.Text = "A";
			ConfigureSlider(ref SliderA1, 0.0, 255.0, 1.0, 16.0, 16.0);
			ConfigureSlider(ref SliderA2, 0.0, 255.0, 1.0, 16.0, 16.0);
			ConfigureSlider(ref SliderB1, 0.0, 255.0, 1.0, 16.0, 16.0);
			ConfigureSlider(ref SliderB2, 0.0, 255.0, 1.0, 16.0, 16.0);
			ConfigureSlider(ref SliderC1, 0.0, 255.0, 1.0, 16.0, 16.0);
			ConfigureSlider(ref SliderC2, 0.0, 255.0, 1.0, 16.0, 16.0);
			ConfigureSlider(ref SliderD1, 0.0, 255.0, 1.0, 16.0, 16.0);
			ConfigureSlider(ref SliderD2, 0.0, 255.0, 1.0, 16.0, 16.0);
			ConfigureBinding(ref SliderA1, ref TextBoxValueA1, "{0:F1}");
			ConfigureBinding(ref SliderA2, ref TextBoxValueA2, "{0:F1}");
			ConfigureBinding(ref SliderB1, ref TextBoxValueB1, "{0:F1}");
			ConfigureBinding(ref SliderB2, ref TextBoxValueB2, "{0:F1}");
			ConfigureBinding(ref SliderC1, ref TextBoxValueC1, "{0:F1}");
			ConfigureBinding(ref SliderC2, ref TextBoxValueC2, "{0:F1}");
			ConfigureBinding(ref SliderD1, ref TextBoxValueD1, "{0:F1}");
			ConfigureBinding(ref SliderD2, ref TextBoxValueD2, "{0:F1}");
			UpdateSliderValues();
		}

		private void ConvertToXyz()
		{
			CurrentColor1         = CurrentColor1.ToColorSpace<Xyz>();
			CurrentColor2         = CurrentColor2.ToColorSpace<Xyz>();
			_updatingSliders      = true;
			TextBlockLabelA1.Text = "X";
			TextBlockLabelA2.Text = "X";
			TextBlockLabelB1.Text = "Y";
			TextBlockLabelB2.Text = "Y";
			TextBlockLabelC1.Text = "Z";
			TextBlockLabelC2.Text = "Z";
			ConfigureSlider(ref SliderA1, 0.0, 100.0, 1.0, 10.0, 10.0);
			ConfigureSlider(ref SliderA2, 0.0, 100.0, 1.0, 10.0, 10.0);
			ConfigureSlider(ref SliderB1, 0.0, 100.0, 1.0, 10.0, 10.0);
			ConfigureSlider(ref SliderB2, 0.0, 100.0, 1.0, 10.0, 10.0);
			ConfigureSlider(ref SliderC1, 0.0, 100.0, 1.0, 10.0, 10.0);
			ConfigureSlider(ref SliderC2, 0.0, 100.0, 1.0, 10.0, 10.0);
			ConfigureBinding(ref SliderA1, ref TextBoxValueA1, "{0:F1}");
			ConfigureBinding(ref SliderA2, ref TextBoxValueA2, "{0:F1}");
			ConfigureBinding(ref SliderB1, ref TextBoxValueB1, "{0:F1}");
			ConfigureBinding(ref SliderB2, ref TextBoxValueB2, "{0:F1}");
			ConfigureBinding(ref SliderC1, ref TextBoxValueC1, "{0:F1}");
			ConfigureBinding(ref SliderC2, ref TextBoxValueC2, "{0:F1}");
			UpdateSliderValues();
		}

		private void ConvertToYxy()
		{
			CurrentColor1         = CurrentColor1.ToColorSpace<Yxy>();
			CurrentColor2         = CurrentColor2.ToColorSpace<Yxy>();
			_updatingSliders      = true;
			TextBlockLabelA1.Text = "Y";
			TextBlockLabelA2.Text = "Y";
			TextBlockLabelB1.Text = "x";
			TextBlockLabelB2.Text = "x";
			TextBlockLabelC1.Text = "y";
			TextBlockLabelC2.Text = "y";
			ConfigureSlider(ref SliderA1, 0.0, 100.0, 1.0,  10.0, 10.0);
			ConfigureSlider(ref SliderA2, 0.0, 100.0, 1.0,  10.0, 10.0);
			ConfigureSlider(ref SliderB1, 0.0, 1.0,   0.01, 0.1,  0.1);
			ConfigureSlider(ref SliderB2, 0.0, 1.0,   0.01, 0.1,  0.1);
			ConfigureSlider(ref SliderC1, 0.0, 1.0,   0.01, 0.1,  0.1);
			ConfigureSlider(ref SliderC2, 0.0, 1.0,   0.01, 0.1,  0.1);
			ConfigureBinding(ref SliderA1, ref TextBoxValueA1, "{0:F1}%");
			ConfigureBinding(ref SliderA2, ref TextBoxValueA2, "{0:F1}%");
			ConfigureBinding(ref SliderB1, ref TextBoxValueB1, "{0:F3}");
			ConfigureBinding(ref SliderB2, ref TextBoxValueB2, "{0:F3}");
			ConfigureBinding(ref SliderC1, ref TextBoxValueC1, "{0:F3}");
			ConfigureBinding(ref SliderC2, ref TextBoxValueC2, "{0:F3}");
			UpdateSliderValues();
		}

		private void Randomize<T>() where T : ColorSpace, new()
		{
			var useLargeIncrement = CheckBoxIncrement.IsChecked == true;
			if (typeof(T) == typeof(Cmyk) || typeof(T) == typeof(Rgba))
			{
				RandomColor = ColorSpace.Random<T>(
					new DiscreteRange<double>(
						SliderA1.Value,
						SliderA2.Value,
						useLargeIncrement ? SliderA1.TickFrequency : SliderA1.SmallChange),
					new DiscreteRange<double>(
						SliderB1.Value,
						SliderB2.Value,
						useLargeIncrement ? SliderB1.TickFrequency : SliderB1.SmallChange),
					new DiscreteRange<double>(
						SliderC1.Value,
						SliderC2.Value,
						useLargeIncrement ? SliderC1.TickFrequency : SliderC1.SmallChange),
					new DiscreteRange<double>(
						SliderD1.Value,
						SliderD2.Value,
						useLargeIncrement ? SliderD1.TickFrequency : SliderD1.SmallChange));
			}
			else
			{
				RandomColor = ColorSpace.Random<T>(
					new DiscreteRange<double>(
						SliderA1.Value,
						SliderA2.Value,
						useLargeIncrement ? SliderA1.TickFrequency : SliderA1.SmallChange),
					new DiscreteRange<double>(
						SliderB1.Value,
						SliderB2.Value,
						useLargeIncrement ? SliderB1.TickFrequency : SliderB1.SmallChange),
					new DiscreteRange<double>(
						SliderC1.Value,
						SliderC2.Value,
						useLargeIncrement ? SliderC1.TickFrequency : SliderC1.SmallChange));
			}
		}
		#endregion

		#region Event Handlers
		private void UserControl_Loaded(object sender, RoutedEventArgs e)
		{
			// Initialize controls
			CheckBoxRandom.IsChecked = false;
			ComboBoxColorSpace.Items.Add(Cmy.FriendlyName);
			ComboBoxColorSpace.Items.Add(Cmyk.FriendlyName);
			ComboBoxColorSpace.Items.Add(Hsl.FriendlyName);
			ComboBoxColorSpace.Items.Add(Hsv.FriendlyName);
			ComboBoxColorSpace.Items.Add(HunterLab.FriendlyName);
			ComboBoxColorSpace.Items.Add(Lab.FriendlyName);
			ComboBoxColorSpace.Items.Add(Lch.FriendlyName);
			ComboBoxColorSpace.Items.Add(Luv.FriendlyName);
			ComboBoxColorSpace.Items.Add(Rgb.FriendlyName);
			ComboBoxColorSpace.Items.Add(Rgba.FriendlyName);
			ComboBoxColorSpace.Items.Add(Xyz.FriendlyName);
			ComboBoxColorSpace.Items.Add(Yxy.FriendlyName);
			ComboBoxColorSpace.SelectedValue = Rgba.FriendlyName;
			ListBoxColorSet.ItemsSource      = Colors;

			// Configure bindings (enable / disable)
			var binding =
				new Binding("SelectedItems.Count") { Source = ListBoxColorSet, Converter = new PositiveIntegerToBoolConverter() };
			ButtonRemove.SetBinding(IsEnabledProperty, binding);

			// Configure bindings (Random = true)
			binding = new Binding("IsChecked") { Source = CheckBoxRandom, Converter = new BooleanToVisibilityConverter() };
			TextBlockLabelA2.SetBinding(VisibilityProperty, binding);
			TextBlockLabelAMin.SetBinding(VisibilityProperty, binding);
			TextBlockLabelAMax.SetBinding(VisibilityProperty, binding);
			TextBoxValueA2.SetBinding(VisibilityProperty, binding);
			SliderA2.SetBinding(VisibilityProperty, binding);
			TextBlockLabelB2.SetBinding(VisibilityProperty, binding);
			TextBlockLabelBMin.SetBinding(VisibilityProperty, binding);
			TextBlockLabelBMax.SetBinding(VisibilityProperty, binding);
			TextBoxValueB2.SetBinding(VisibilityProperty, binding);
			SliderB2.SetBinding(VisibilityProperty, binding);
			TextBlockLabelC2.SetBinding(VisibilityProperty, binding);
			TextBlockLabelCMin.SetBinding(VisibilityProperty, binding);
			TextBlockLabelCMax.SetBinding(VisibilityProperty, binding);
			TextBoxValueC2.SetBinding(VisibilityProperty, binding);
			SliderC2.SetBinding(VisibilityProperty, binding);
			CheckBoxIncrement.SetBinding(VisibilityProperty, binding);
			ButtonRandomize.SetBinding(VisibilityProperty, binding);

			// Configure bindings (ColorSpace = CMYK)
			binding = new Binding("SelectedValue")
			{
				Source    = ComboBoxColorSpace,
				Converter = new ColorSpaceToVisibilityConverter()
			};
			TextBlockLabelD1.SetBinding(VisibilityProperty, binding);
			TextBoxValueD1.SetBinding(VisibilityProperty, binding);
			SliderD1.SetBinding(VisibilityProperty, binding);

			// Configure bindings (Random = true && ColorSpace = CMYK)
			var bindings = new[]
			{
				new Binding("SelectedValue") { Source = ComboBoxColorSpace }, new Binding("IsChecked") { Source = CheckBoxRandom }
			};
			var multiBinding = new MultiBinding() { Converter = new ColorSpaceToVisibilityMultiConverter() };
			foreach (var bindingPart in bindings)
			{
				multiBinding.Bindings.Add(bindingPart);
			}

			TextBlockLabelD2.SetBinding(VisibilityProperty, multiBinding);
			TextBlockLabelDMin.SetBinding(VisibilityProperty, multiBinding);
			TextBlockLabelDMax.SetBinding(VisibilityProperty, multiBinding);
			TextBoxValueD2.SetBinding(VisibilityProperty, multiBinding);
			SliderD2.SetBinding(VisibilityProperty, multiBinding);

			// Configure bindings (Current color rectangles)
			_currentColor1Binding =
				new Binding("CurrentColor1") { Source = this, Converter = new ColorSpaceToSolidColorBrushConverter() };
			RectangleCurrentColor1.SetBinding(Shape.FillProperty, _currentColor1Binding);

			_currentColor2Binding =
				new Binding("CurrentColor2") { Source = this, Converter = new ColorSpaceToSolidColorBrushConverter() };
			RectangleCurrentColor2.SetBinding(Shape.FillProperty, _currentColor2Binding);

			_randomColorBinding =
				new Binding("RandomColor") { Source = this, Converter = new ColorSpaceToSolidColorBrushConverter() };
			RectangleRandomColor1.SetBinding(Shape.FillProperty, _currentColor1Binding);
			RectangleRandomColor2.SetBinding(Shape.FillProperty, _currentColor2Binding);
		}

		private void CheckBoxRandom_CheckedChanged(object sender, RoutedEventArgs e)
		{
			if (CheckBoxRandom.IsChecked == true)
			{
				_isRandomMode = true;
				RectangleRandomColor1.SetBinding(Shape.FillProperty, _randomColorBinding);
				RectangleRandomColor2.SetBinding(Shape.FillProperty, _randomColorBinding);
			}
			else
			{
				_isRandomMode               = false;
				CheckBoxIncrement.IsChecked = false;
				SliderA2.Value              = SliderA1.Value;
				SliderB2.Value              = SliderB1.Value;
				SliderC2.Value              = SliderC1.Value;
				SliderD2.Value              = SliderD1.Value;
				RandomColor                 = ColorSpace.OpaqueBlack;
				RectangleRandomColor1.SetBinding(Shape.FillProperty, _currentColor1Binding);
				RectangleRandomColor2.SetBinding(Shape.FillProperty, _currentColor2Binding);
			}

			e.Handled = true;
		}

		private void CheckBoxIncrement_CheckedChanged(object sender, RoutedEventArgs e)
		{
			if (CheckBoxIncrement.IsChecked == true)
			{
				SliderA1.IsSnapToTickEnabled = true;
				SliderA2.IsSnapToTickEnabled = true;
				SliderB1.IsSnapToTickEnabled = true;
				SliderB2.IsSnapToTickEnabled = true;
				SliderC1.IsSnapToTickEnabled = true;
				SliderC2.IsSnapToTickEnabled = true;
				SliderD1.IsSnapToTickEnabled = true;
				SliderD2.IsSnapToTickEnabled = true;
				SliderA1.SnapToNearestTick();
				SliderA2.SnapToNearestTick();
				SliderB1.SnapToNearestTick();
				SliderB2.SnapToNearestTick();
				SliderC1.SnapToNearestTick();
				SliderC2.SnapToNearestTick();
				SliderD1.SnapToNearestTick();
				SliderD2.SnapToNearestTick();
			}
			else if (CheckBoxIncrement.IsChecked == false)
			{
				SliderA1.IsSnapToTickEnabled = false;
				SliderA2.IsSnapToTickEnabled = false;
				SliderB1.IsSnapToTickEnabled = false;
				SliderB2.IsSnapToTickEnabled = false;
				SliderC1.IsSnapToTickEnabled = false;
				SliderC2.IsSnapToTickEnabled = false;
				SliderD1.IsSnapToTickEnabled = false;
				SliderD2.IsSnapToTickEnabled = false;
			}

			e.Handled = true;
		}

		private void ListBoxColorSet_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (IsColorChooserOnly) return;
			CheckBoxRandom.IsChecked = false;

			if (ListBoxColorSet.SelectedItems.Count == 1)
			{
				var selectedColor = ListBoxColorSet.SelectedValue as ColorSpace;
				if (selectedColor != null)
				{
					CurrentColor1 = selectedColor.Clone() as ColorSpace;
					CurrentColor2 = selectedColor.Clone() as ColorSpace;
					SynchronizeCurrentColorValues();
				}
			}
			else if (ListBoxColorSet.Items.Count >= 1 && ListBoxColorSet.SelectedIndex == -1)
			{
				ListBoxColorSet.SelectedIndex = 0;
				SynchronizeCurrentColorValues();
			}
			else
			{
				CurrentColor1 = ColorSpace.OpaqueBlack;
				CurrentColor2 = ColorSpace.OpaqueBlack;
				SynchronizeCurrentColorValues();
			}

			e.Handled = true;
		}

		private void ComboBoxColorSpace_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			var selectedItemString = e.AddedItems[0] as string;

			if (!String.IsNullOrEmpty(selectedItemString))
			{
				if (selectedItemString == Cmy.FriendlyName)
					ConvertToCmy();
				else if (selectedItemString == Cmyk.FriendlyName)
					ConvertToCmyk();
				else if (selectedItemString == Hsl.FriendlyName)
					ConvertToHsl();
				else if (selectedItemString == Hsv.FriendlyName)
					ConvertToHsv();
				else if (selectedItemString == HunterLab.FriendlyName)
					ConvertToHunterLab();
				else if (selectedItemString == Lab.FriendlyName)
					ConvertToLab();
				else if (selectedItemString == Lch.FriendlyName)
					ConvertToLch();
				else if (selectedItemString == Luv.FriendlyName)
					ConvertToLuv();
				else if (selectedItemString == Rgb.FriendlyName)
					ConvertToRgb();
				else if (selectedItemString == Rgba.FriendlyName)
					ConvertToRgba();
				else if (selectedItemString == Xyz.FriendlyName)
					ConvertToXyz();
				else if (selectedItemString == Yxy.FriendlyName)
					ConvertToYxy();
			}

			e.Handled = true;
		}

		private void ButtonRandomize_Click(object sender, RoutedEventArgs e)
		{
			if (CurrentColor1.GetType() != CurrentColor2.GetType())
				throw new ApplicationException("Unexpected ColorSpace mismatch");

			if (CurrentColor1 is Cmy)
				Randomize<Cmy>();
			else if (CurrentColor1 is Cmyk)
				Randomize<Cmyk>();
			else if (CurrentColor1 is Hsl)
				Randomize<Hsl>();
			else if (CurrentColor1 is Hsv)
				Randomize<Hsv>();
			else if (CurrentColor1 is HunterLab)
				Randomize<HunterLab>();
			else if (CurrentColor1 is Lab)
				Randomize<Lab>();
			else if (CurrentColor1 is Lch)
				Randomize<Lch>();
			else if (CurrentColor1 is Luv)
				Randomize<Luv>();
			else if (CurrentColor1 is Rgb)
				Randomize<Rgb>();
			else if (CurrentColor1 is Rgba)
				Randomize<Rgba>();
			else if (CurrentColor1 is Xyz)
				Randomize<Xyz>();
			else if (CurrentColor1 is Yxy)
				Randomize<Yxy>();
			e.Handled = true;
		}

		private void ButtonAdd_Click(object sender, RoutedEventArgs e)
		{
			Colors.Add(_isRandomMode ? RandomColor : CurrentColor1);
			e.Handled = true;
		}

		private void ButtonRemove_Click(object sender, RoutedEventArgs e)
		{
			var colorsToRemove = (from object item in ListBoxColorSet.SelectedItems
								  select ListBoxColorSet.Items[ListBoxColorSet.Items.IndexOf(item)] as ColorSpace).ToList();

			foreach (var colorSpace in colorsToRemove)
			{
				Colors.Remove(colorSpace);
			}

			e.Handled = true;
		}

		private void ButtonMoveUp_Click(object sender, RoutedEventArgs e)
		{
			var selectedIndex = Colors.IndexOf(ListBoxColorSet.SelectedValue as ColorSpace);
			if (selectedIndex >= 1)
				Colors.Move(selectedIndex, selectedIndex - 1);
			e.Handled = true;
		}

		private void ButtonMoveDown_Click(object sender, RoutedEventArgs e)
		{
			var selectedIndex = Colors.IndexOf(ListBoxColorSet.SelectedValue as ColorSpace);
			if (selectedIndex < Colors.Count - 1)
				Colors.Move(selectedIndex, selectedIndex + 1);
			e.Handled = true;
		}

		private void SliderA1_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			if (!_updatingSliders)
			{
				CurrentColor1[0] = e.NewValue;
				CurrentColor1    = CurrentColor1.Clone() as ColorSpace;

				if (!_isRandomMode || (_isRandomMode && SliderA1.Value > SliderA2.Value))
				{
					SliderA2.Value = SliderA1.Value;
				}
			}

			e.Handled = true;
		}

		private void SliderA2_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			if (!_updatingSliders)
			{
				CurrentColor2[0] = e.NewValue;
				CurrentColor2    = CurrentColor2.Clone() as ColorSpace;

				if (_isRandomMode && SliderA2.Value < SliderA1.Value)
				{
					SliderA1.Value = SliderA2.Value;
				}
			}

			e.Handled = true;
		}

		private void SliderB1_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			if (!_updatingSliders)
			{
				CurrentColor1[1] = e.NewValue;
				CurrentColor1    = CurrentColor1.Clone() as ColorSpace;

				if (!_isRandomMode || (_isRandomMode && SliderB1.Value > SliderB2.Value))
				{
					SliderB2.Value = SliderB1.Value;
				}
			}

			e.Handled = true;
		}

		private void SliderB2_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			if (!_updatingSliders)
			{
				CurrentColor2[1] = e.NewValue;
				CurrentColor2    = CurrentColor2.Clone() as ColorSpace;

				if (_isRandomMode && SliderB2.Value < SliderB1.Value)
				{
					SliderB1.Value = SliderB2.Value;
				}
			}

			e.Handled = true;
		}

		private void SliderC1_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			if (!_updatingSliders)
			{
				CurrentColor1[2] = e.NewValue;
				CurrentColor1    = CurrentColor1.Clone() as ColorSpace;

				if (!_isRandomMode || (_isRandomMode && SliderC1.Value > SliderC2.Value))
				{
					SliderC2.Value = SliderC1.Value;
				}
			}

			e.Handled = true;
		}

		private void SliderC2_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			if (!_updatingSliders)
			{
				CurrentColor2[2] = e.NewValue;
				CurrentColor2    = CurrentColor2.Clone() as ColorSpace;

				if (_isRandomMode && SliderC2.Value < SliderC1.Value)
				{
					SliderC1.Value = SliderC2.Value;
				}
			}

			e.Handled = true;
		}

		private void SliderD1_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			if (!_updatingSliders && (CurrentColor2 is Cmyk || CurrentColor2 is Rgba))
			{
				CurrentColor1[3] = e.NewValue;
				CurrentColor1    = CurrentColor1.Clone() as ColorSpace;

				if (!_isRandomMode || (_isRandomMode && SliderD1.Value > SliderD2.Value))
				{
					SliderD2.Value = SliderD1.Value;
				}
			}

			e.Handled = true;
		}

		private void SliderD2_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			if (!_updatingSliders && (CurrentColor2 is Cmyk || CurrentColor2 is Rgba))
			{
				CurrentColor2[3] = e.NewValue;
				CurrentColor2    = CurrentColor2.Clone() as ColorSpace;

				if (_isRandomMode && SliderD2.Value < SliderD1.Value)
				{
					SliderD1.Value = SliderD2.Value;
				}
			}

			e.Handled = true;
		}
		#endregion
	}
}