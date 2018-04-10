// ┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  FILE:       TickBarAdvanced.cs
// ┃  PROJECT:    Utility.WPF
// ┃  SOLUTION:   Utility.NET
// ┃  CREATED:    2018-03-13 @ 12:56 AM
// ┣━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  AUTHOR:     Jonathan Ruisi
// ┃  EMAIL:      JonathanRuisi@gmail.com
// ┣━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  GIT REPO:   https://github.com/jonathanruisi/Utility.NET
// ┃  LICENSE:    https://github.com/jonathanruisi/Utility.NET/blob/master/LICENSE
// ┗━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

using JLR.Utility.NET;

namespace JLR.Utility.WPF.Elements
{
	public class TickBarAdvanced : FrameworkElement
	{
		#region Fields
		private Pen _originTickPen, _majorTickPen, _minorTickPen;
		private bool _ignoreTickValuePropertyChange;
		#endregion

		#region Properties
		#region General
		public Orientation Orientation
		{
			get => (Orientation)GetValue(OrientationProperty);
			set => SetValue(OrientationProperty, value);
		}

		public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register(
			"Orientation",
			typeof(Orientation),
			typeof(TickBarAdvanced),
			new FrameworkPropertyMetadata(Orientation.Horizontal, FrameworkPropertyMetadataOptions.AffectsRender));

		public Position TickAlignment
		{
			get => (Position)GetValue(TickAlignmentProperty);
			set => SetValue(TickAlignmentProperty, value);
		}

		public static readonly DependencyProperty TickAlignmentProperty = DependencyProperty.Register(
			"TickAlignment",
			typeof(Position),
			typeof(TickBarAdvanced),
			new FrameworkPropertyMetadata(Position.Center, FrameworkPropertyMetadataOptions.AffectsRender));

		public bool IsDirectionReversed
		{
			get => (bool)GetValue(IsDirectionReversedProperty);
			set => SetValue(IsDirectionReversedProperty, value);
		}

		public static readonly DependencyProperty IsDirectionReversedProperty = DependencyProperty.Register(
			"IsDirectionReversed",
			typeof(bool),
			typeof(TickBarAdvanced),
			new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));

		public bool IsShiftBoundaryTicks
		{
			get => (bool)GetValue(IsShiftBoundaryTicksProperty);
			set => SetValue(IsShiftBoundaryTicksProperty, value);
		}

		public static readonly DependencyProperty IsShiftBoundaryTicksProperty = DependencyProperty.Register(
			"IsShiftBoundaryTicks",
			typeof(bool),
			typeof(TickBarAdvanced),
			new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender));
		#endregion

		#region Range
		public decimal Minimum { get => (decimal)GetValue(MinimumProperty); set => SetValue(MinimumProperty, value); }

		public static readonly DependencyProperty MinimumProperty = DependencyProperty.Register(
			"Minimum",
			typeof(decimal),
			typeof(TickBarAdvanced),
			new FrameworkPropertyMetadata(0.0M, FrameworkPropertyMetadataOptions.AffectsRender, OnRangePropertyChanged));

		public decimal Maximum { get => (decimal)GetValue(MaximumProperty); set => SetValue(MaximumProperty, value); }

		public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register(
			"Maximum",
			typeof(decimal),
			typeof(TickBarAdvanced),
			new FrameworkPropertyMetadata(
				10.0M,
				FrameworkPropertyMetadataOptions.AffectsRender,
				OnRangePropertyChanged,
				CoerceMaximum));

		public int DecimalPrecision
		{
			get => (int)GetValue(DecimalPrecisionProperty);
			set => SetValue(DecimalPrecisionProperty, value);
		}

		public static readonly DependencyProperty DecimalPrecisionProperty = DependencyProperty.Register(
			"DecimalPrecision",
			typeof(int),
			typeof(TickBarAdvanced),
			new FrameworkPropertyMetadata(6, OnRangePropertyChanged));

		public int ZIndexOrigin { get => (int)GetValue(ZIndexOriginProperty); set => SetValue(ZIndexOriginProperty, value); }

		public static readonly DependencyProperty ZIndexOriginProperty = DependencyProperty.Register(
			"ZIndexOrigin",
			typeof(int),
			typeof(TickBarAdvanced),
			new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsRender));

		public int ZIndexMajor { get => (int)GetValue(ZIndexMajorProperty); set => SetValue(ZIndexMajorProperty, value); }

		public static readonly DependencyProperty ZIndexMajorProperty = DependencyProperty.Register(
			"ZIndexMajor",
			typeof(int),
			typeof(TickBarAdvanced),
			new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsRender));

		public int ZIndexMinor { get => (int)GetValue(ZIndexMinorProperty); set => SetValue(ZIndexMinorProperty, value); }

		public static readonly DependencyProperty ZIndexMinorProperty = DependencyProperty.Register(
			"ZIndexMinor",
			typeof(int),
			typeof(TickBarAdvanced),
			new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsRender));
		#endregion

		#region Tick Positioning
		[TypeConverter(typeof(TickHashSetConverter))]
		public Dictionary<decimal,Tick> Ticks { get => (Dictionary<decimal,Tick>)GetValue(TicksProperty); set => SetValue(TicksProperty, value); }

		public static readonly DependencyProperty TicksProperty = DependencyProperty.Register(
			"Ticks",
			typeof(Dictionary<decimal,Tick>),
			typeof(TickBarAdvanced),
			new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, OnTickValuePropertyChanged));

		public decimal MajorTickFrequency
		{
			get => (decimal)GetValue(MajorTickFrequencyProperty);
			set => SetValue(MajorTickFrequencyProperty, value);
		}

		public static readonly DependencyProperty MajorTickFrequencyProperty = DependencyProperty.Register(
			"MajorTickFrequency",
			typeof(decimal),
			typeof(TickBarAdvanced),
			new FrameworkPropertyMetadata(1.0M, FrameworkPropertyMetadataOptions.AffectsRender, OnTickValuePropertyChanged));

		public decimal MinorTickFrequency
		{
			get => (decimal)GetValue(MinorTickFrequencyProperty);
			set => SetValue(MinorTickFrequencyProperty, value);
		}

		public static readonly DependencyProperty MinorTickFrequencyProperty = DependencyProperty.Register(
			"MinorTickFrequency",
			typeof(decimal),
			typeof(TickBarAdvanced),
			new FrameworkPropertyMetadata(0.25M, FrameworkPropertyMetadataOptions.AffectsRender, OnTickValuePropertyChanged));
		#endregion

		#region Tick Size
		public double OriginTickRelativeSize
		{
			get => (double)GetValue(OriginTickRelativeSizeProperty);
			set => SetValue(OriginTickRelativeSizeProperty, value);
		}

		public static readonly DependencyProperty OriginTickRelativeSizeProperty = DependencyProperty.Register(
			"OriginTickRelativeSize",
			typeof(double),
			typeof(TickBarAdvanced),
			new FrameworkPropertyMetadata(1.0, FrameworkPropertyMetadataOptions.AffectsRender));

		public double MajorTickRelativeSize
		{
			get => (double)GetValue(MajorTickRelativeSizeProperty);
			set => SetValue(MajorTickRelativeSizeProperty, value);
		}

		public static readonly DependencyProperty MajorTickRelativeSizeProperty = DependencyProperty.Register(
			"MajorTickRelativeSize",
			typeof(double),
			typeof(TickBarAdvanced),
			new FrameworkPropertyMetadata(1.0, FrameworkPropertyMetadataOptions.AffectsRender));

		public double MinorTickRelativeSize
		{
			get => (double)GetValue(MinorTickRelativeSizeProperty);
			set => SetValue(MinorTickRelativeSizeProperty, value);
		}

		public static readonly DependencyProperty MinorTickRelativeSizeProperty = DependencyProperty.Register(
			"MinorTickRelativeSize",
			typeof(double),
			typeof(TickBarAdvanced),
			new FrameworkPropertyMetadata(0.5, FrameworkPropertyMetadataOptions.AffectsRender));

		public double OriginTickThickness
		{
			get => (double)GetValue(OriginTickThicknessProperty);
			set => SetValue(OriginTickThicknessProperty, value);
		}

		public static readonly DependencyProperty OriginTickThicknessProperty = DependencyProperty.Register(
			"OriginTickThickness",
			typeof(double),
			typeof(TickBarAdvanced),
			new FrameworkPropertyMetadata(2.0, FrameworkPropertyMetadataOptions.AffectsRender, OnAppearancePropertyChanged));

		public double MajorTickThickness
		{
			get => (double)GetValue(MajorTickThicknessProperty);
			set => SetValue(MajorTickThicknessProperty, value);
		}

		public static readonly DependencyProperty MajorTickThicknessProperty = DependencyProperty.Register(
			"MajorTickThickness",
			typeof(double),
			typeof(TickBarAdvanced),
			new FrameworkPropertyMetadata(1.0, FrameworkPropertyMetadataOptions.AffectsRender, OnAppearancePropertyChanged));

		public double MinorTickThickness
		{
			get => (double)GetValue(MinorTickThicknessProperty);
			set => SetValue(MinorTickThicknessProperty, value);
		}

		public static readonly DependencyProperty MinorTickThicknessProperty = DependencyProperty.Register(
			"MinorTickThickness",
			typeof(double),
			typeof(TickBarAdvanced),
			new FrameworkPropertyMetadata(1.0, FrameworkPropertyMetadataOptions.AffectsRender, OnAppearancePropertyChanged));
		#endregion

		#region Brush
		public Brush Background { get => (Brush)GetValue(BackgroundProperty); set => SetValue(BackgroundProperty, value); }

		public static readonly DependencyProperty BackgroundProperty = DependencyProperty.Register(
			"Background",
			typeof(Brush),
			typeof(TickBarAdvanced),
			new FrameworkPropertyMetadata(Brushes.Transparent, FrameworkPropertyMetadataOptions.AffectsRender));

		public Brush OriginTickBrush
		{
			get => (Brush)GetValue(OriginTickBrushProperty);
			set => SetValue(OriginTickBrushProperty, value);
		}

		public static readonly DependencyProperty OriginTickBrushProperty = DependencyProperty.Register(
			"OriginTickBrush",
			typeof(Brush),
			typeof(TickBarAdvanced),
			new FrameworkPropertyMetadata(
				Brushes.Black,
				FrameworkPropertyMetadataOptions.AffectsRender,
				OnAppearancePropertyChanged));

		public Brush MajorTickBrush
		{
			get => (Brush)GetValue(MajorTickBrushProperty);
			set => SetValue(MajorTickBrushProperty, value);
		}

		public static readonly DependencyProperty MajorTickBrushProperty = DependencyProperty.Register(
			"MajorTickBrush",
			typeof(Brush),
			typeof(TickBarAdvanced),
			new FrameworkPropertyMetadata(
				Brushes.Black,
				FrameworkPropertyMetadataOptions.AffectsRender,
				OnAppearancePropertyChanged));

		public Brush MinorTickBrush
		{
			get => (Brush)GetValue(MinorTickBrushProperty);
			set => SetValue(MinorTickBrushProperty, value);
		}

		public static readonly DependencyProperty MinorTickBrushProperty = DependencyProperty.Register(
			"MinorTickBrush",
			typeof(Brush),
			typeof(TickBarAdvanced),
			new FrameworkPropertyMetadata(
				Brushes.DimGray,
				FrameworkPropertyMetadataOptions.AffectsRender,
				OnAppearancePropertyChanged));
		#endregion
		#endregion

		#region Internal Properties
		#endregion

		#region Constructor
		static TickBarAdvanced()
		{
			DefaultStyleKeyProperty.OverrideMetadata(
				typeof(TickBarAdvanced),
				new FrameworkPropertyMetadata(typeof(TickBarAdvanced)));
		}
		#endregion

		#region Internal Methods
		#endregion

		#region Private Methods
		private void UpdateOriginTickPen()
		{
			_originTickPen = new Pen(OriginTickBrush, OriginTickThickness);
			_originTickPen.Freeze();
		}

		private void UpdateMajorTickPen()
		{
			_majorTickPen = new Pen(MajorTickBrush, MajorTickThickness);
			_majorTickPen.Freeze();
		}

		private void UpdateMinorTickPen()
		{
			_minorTickPen = new Pen(MinorTickBrush, MinorTickThickness);
			_minorTickPen.Freeze();
		}

		private Dictionary<decimal,Tick> GenerateTicks()
		{
			var ticks = new Dictionary<decimal, Tick>();

			decimal major = 0;
			if (MajorTickFrequency > 0)
			{
				major = MajorTickFrequency * (int)(Minimum / MajorTickFrequency);
				if (Minimum >= 0 && Math.Abs(major - Minimum) > 0)
					major = MajorTickFrequency * ((int)(Minimum / MajorTickFrequency) + 1);
			}

			decimal minor = 0;
			if (MinorTickFrequency > 0)
			{
				minor = MinorTickFrequency * (int)(Minimum / MinorTickFrequency);
				if (Minimum >= 0 && Math.Abs(minor - Minimum) > 0)
					minor = MinorTickFrequency * ((int)(Minimum / MinorTickFrequency) + 1);
			}

			var adjustedMinor = decimal.Round(minor, DecimalPrecision, MidpointRounding.ToEven);
			var adjustedMajor = decimal.Round(major, DecimalPrecision, MidpointRounding.ToEven);
			if (MajorTickFrequency > 0 && MinorTickFrequency > 0) // Both major and minor ticks needed
			{
				while (adjustedMinor <= Maximum && adjustedMajor <= Maximum)
				{
					var tickFlags = adjustedMinor == 0 && adjustedMajor == 0 ? TickTypes.Origin : 0;

					if (adjustedMinor < adjustedMajor)
					{
						tickFlags |= TickTypes.Minor;
						ticks.Add(adjustedMinor, new Tick(tickFlags, 0));
						minor         += MinorTickFrequency;
						adjustedMinor =  decimal.Round(minor, DecimalPrecision, MidpointRounding.ToEven);
					}
					else
					{

						if (adjustedMinor == adjustedMajor)
						{
							tickFlags     |= TickTypes.Minor;
							minor         += MinorTickFrequency;
							adjustedMinor =  decimal.Round(minor, DecimalPrecision, MidpointRounding.ToEven);
						}

						tickFlags |= TickTypes.Major;
						ticks.Add(adjustedMajor, new Tick(tickFlags, 0));
						major         += MajorTickFrequency;
						adjustedMajor =  decimal.Round(major, DecimalPrecision, MidpointRounding.ToEven);
					}
				}
			}
			else if (MajorTickFrequency > 0) // Only major ticks needed
			{
				while (adjustedMajor <= Maximum)
				{
					ticks.Add(adjustedMajor, new Tick(adjustedMajor == 0 ? TickTypes.Origin | TickTypes.Major : TickTypes.Major, 0));
					major         += MajorTickFrequency;
					adjustedMajor =  decimal.Round(major, DecimalPrecision, MidpointRounding.ToEven);
				}
			}
			else if (MinorTickFrequency > 0) // Only minor ticks needed
			{
				while (adjustedMinor <= Maximum)
				{
					ticks.Add(adjustedMinor, new Tick(adjustedMinor == 0 ? TickTypes.Origin | TickTypes.Minor : TickTypes.Minor, 0));
					minor         += MinorTickFrequency;
					adjustedMinor =  decimal.Round(minor, DecimalPrecision, MidpointRounding.ToEven);
				}
			}

			return ticks;
		}

		private double GetTickRenderCoordinate(decimal value)
		{
			var primaryAxisLength = Orientation == Orientation.Horizontal ? ActualWidth : ActualHeight;
			var position          = (double)(value - Minimum) * primaryAxisLength / (double)(Maximum - Minimum);
			if (IsDirectionReversed)
				position = primaryAxisLength - position;
			return position;
		}
		#endregion

		#region Property Changed Callbacks, Coerce Value Callbacks, Validate Value Callbacks
		private static void OnRangePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (!(d is TickBarAdvanced tickBar)) return;

			if (e.Property == MinimumProperty)
				tickBar.CoerceValue(MaximumProperty);
			if (tickBar.MajorTickFrequency != 0 || tickBar.MinorTickFrequency != 0)
			{
				tickBar._ignoreTickValuePropertyChange = true;
				tickBar.SetCurrentValue(TicksProperty, tickBar.GenerateTicks());
				tickBar._ignoreTickValuePropertyChange = false;
			}
		}

		private static void OnTickValuePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (!(d is TickBarAdvanced tickBar)) return;

			if (e.Property == TicksProperty && !tickBar._ignoreTickValuePropertyChange)
			{
				tickBar._ignoreTickValuePropertyChange = true;
				tickBar.SetCurrentValue(MajorTickFrequencyProperty, 0M);
				tickBar.SetCurrentValue(MinorTickFrequencyProperty, 0M);
				tickBar._ignoreTickValuePropertyChange = false;
			}
			else if ((e.Property == MajorTickFrequencyProperty || e.Property == MinorTickFrequencyProperty) &&
				!tickBar._ignoreTickValuePropertyChange)
			{
				tickBar._ignoreTickValuePropertyChange = true;
				tickBar.SetCurrentValue(TicksProperty, tickBar.GenerateTicks());
				tickBar._ignoreTickValuePropertyChange = false;
			}
		}

		private static void OnAppearancePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (!(d is TickBarAdvanced tickBar)) return;

			if (e.Property == OriginTickBrushProperty || e.Property == OriginTickThicknessProperty)
				tickBar.UpdateOriginTickPen();
			else if (e.Property == MajorTickBrushProperty || e.Property == MajorTickThicknessProperty)
				tickBar.UpdateMajorTickPen();
			else if (e.Property == MinorTickBrushProperty || e.Property == MinorTickThicknessProperty)
				tickBar.UpdateMinorTickPen();
		}

		private static object CoerceMaximum(DependencyObject d, object value)
		{
			var tickBar = (TickBarAdvanced)d;
			return (decimal)value < tickBar.Minimum ? tickBar.Minimum : value;
		}
		#endregion

		#region Method Overrides (System.Windows.FrameworkElement)
		/// <inheritdoc />
		protected override void OnRender(DrawingContext drawingContext)
		{
			if (Math.Abs(ActualWidth) < double.Epsilon || Math.Abs(ActualHeight) < double.Epsilon)
				return;

			// Calculate render coordinates
			var primaryAxisLength   = Orientation == Orientation.Horizontal ? ActualWidth : ActualHeight;
			var secondaryAxisLength = Orientation == Orientation.Horizontal ? ActualHeight : ActualWidth;
			var secAxisOrigin       = CalculateSecondaryAxisLineCoordinates(OriginTickRelativeSize);
			var secAxisMajor        = CalculateSecondaryAxisLineCoordinates(MajorTickRelativeSize);
			var secAxisMinor        = CalculateSecondaryAxisLineCoordinates(MinorTickRelativeSize);

			// Draw background
			drawingContext.DrawRectangle(Background, null, new Rect(new Point(0, 0), new Size(ActualWidth, ActualHeight)));

			// Draw ticks
			foreach (var tick in Ticks)
			{
				var position = GetTickRenderCoordinate(tick.Key);
				switch (tick.Value.TickType)
				{
					case TickTypes.Origin:
						DrawTick(ref position, ref secAxisOrigin, ref _originTickPen);
						break;
					case TickTypes.Major:
						DrawTick(ref position, ref secAxisMajor, ref _majorTickPen);
						break;
					case TickTypes.Minor:
						DrawTick(ref position, ref secAxisMinor, ref _minorTickPen);
						break;
					default:
						if (tick.Value.TickType.HasFlag(TickTypes.Origin))
							DrawTick(ref position, ref secAxisOrigin, ref _originTickPen);
						else if (tick.Value.TickType.HasFlag(TickTypes.Major))
							DrawTick(ref position, ref secAxisMajor, ref _majorTickPen);
						else if (tick.Value.TickType.HasFlag(TickTypes.Minor))
							DrawTick(ref position, ref secAxisMinor, ref _minorTickPen);
						break;
				}
			}

			Range<double> CalculateSecondaryAxisLineCoordinates(double relativeSize)
			{
				switch (TickAlignment)
				{
					case Position.Top:
					case Position.Left:
						return new Range<double>(0, relativeSize * secondaryAxisLength);
					case Position.Middle:
					case Position.Center:
						var startCoord = (secondaryAxisLength - (relativeSize * secondaryAxisLength)) / 2;
						return new Range<double>(startCoord, secondaryAxisLength - startCoord);
					case Position.Bottom:
					case Position.Right:
						return new Range<double>(secondaryAxisLength, secondaryAxisLength - relativeSize * secondaryAxisLength);
					default:
						return new Range<double>(0, 0);
				}
			}

			void DrawTick(ref double position, ref Range<double> secAxisCoords, ref Pen pen)
			{
				// For any ticks that are equal to the max or min values,
				// visually shift the tick inwards by half of its thickness.
				if (IsShiftBoundaryTicks)
				{
					if (Math.Abs(position) < double.Epsilon)
						position = pen.Thickness / 2;
					else if (Math.Abs(position - primaryAxisLength) < double.Epsilon)
						position = primaryAxisLength - pen.Thickness / 2;
				}

				if (Orientation == Orientation.Horizontal)
					drawingContext.DrawLine(
						pen,
						new Point(position, secAxisCoords.Minimum),
						new Point(position, secAxisCoords.Maximum));
				else
					drawingContext.DrawLine(
						pen,
						new Point(secAxisCoords.Minimum, position),
						new Point(secAxisCoords.Maximum, position));
			}
		}

		/// <inheritdoc />
		protected override void OnInitialized(EventArgs e)
		{
			UpdateOriginTickPen();
			UpdateMajorTickPen();
			UpdateMinorTickPen();
			if (Ticks == null)
				SetCurrentValue(TicksProperty, GenerateTicks());
			base.OnInitialized(e);
		}
		#endregion
	}
}