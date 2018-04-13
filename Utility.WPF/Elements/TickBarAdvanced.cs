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
using System.Windows.Data;
using System.Windows.Media;

using JLR.Utility.NET;

namespace JLR.Utility.WPF.Elements
{
	#region Enumerated Types
	[Flags]
	public enum TickTypes
	{
		Origin = 1,
		Major = 2,
		Minor = 4
	};
	#endregion

	public class TickBarAdvanced : FrameworkElement
	{
		#region Fields
		private Pen _originTickPen, _majorTickPen, _minorTickPen;
		private bool _ignoreTickValuePropertyChange;
		private double _smallestTickGap;
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

		public int OriginTickZIndex
		{
			get => (int)GetValue(OriginTickZIndexProperty);
			set => SetValue(OriginTickZIndexProperty, value);
		}

		public static readonly DependencyProperty OriginTickZIndexProperty = DependencyProperty.Register(
			"OriginTickZIndex",
			typeof(int),
			typeof(TickBarAdvanced),
			new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsRender));

		public int MajorTickZIndex
		{
			get => (int)GetValue(MajorTickZIndexProperty);
			set => SetValue(MajorTickZIndexProperty, value);
		}

		public static readonly DependencyProperty MajorTickZIndexProperty = DependencyProperty.Register(
			"MajorTickZIndex",
			typeof(int),
			typeof(TickBarAdvanced),
			new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsRender));

		public int MinorTickZIndex
		{
			get => (int)GetValue(MinorTickZIndexProperty);
			set => SetValue(MinorTickZIndexProperty, value);
		}

		public static readonly DependencyProperty MinorTickZIndexProperty = DependencyProperty.Register(
			"MinorTickZIndex",
			typeof(int),
			typeof(TickBarAdvanced),
			new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsRender));
		#endregion

		#region Tick Positioning
		[TypeConverter(typeof(TickListConverter))]
		public List<(decimal value, TickTypes type)> Ticks
		{
			get => (List<(decimal value, TickTypes type)>)GetValue(TicksProperty);
			set => SetValue(TicksProperty, value);
		}

		public static readonly DependencyProperty TicksProperty = DependencyProperty.Register(
			"Ticks",
			typeof(List<(decimal value, TickTypes type)>),
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
			new FrameworkPropertyMetadata(0M, FrameworkPropertyMetadataOptions.AffectsRender, OnTickValuePropertyChanged));

		public decimal MinorTickFrequency
		{
			get => (decimal)GetValue(MinorTickFrequencyProperty);
			set => SetValue(MinorTickFrequencyProperty, value);
		}

		public static readonly DependencyProperty MinorTickFrequencyProperty = DependencyProperty.Register(
			"MinorTickFrequency",
			typeof(decimal),
			typeof(TickBarAdvanced),
			new FrameworkPropertyMetadata(0M, FrameworkPropertyMetadataOptions.AffectsRender, OnTickValuePropertyChanged));
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

		#region Constructor
		static TickBarAdvanced()
		{
			DefaultStyleKeyProperty.OverrideMetadata(
				typeof(TickBarAdvanced),
				new FrameworkPropertyMetadata(typeof(TickBarAdvanced)));
		}
		#endregion

		#region Internal Properties, Events, and Methods
		internal event RoutedPropertyChangedEventHandler<double> SmallestTickGapChanged
		{
			add => AddHandler(SmallestTickGapChangedEvent, value);
			remove => RemoveHandler(SmallestTickGapChangedEvent, value);
		}

		internal static readonly RoutedEvent SmallestTickGapChangedEvent = EventManager.RegisterRoutedEvent(
			"SmallestTickGapChanged",
			RoutingStrategy.Direct,
			typeof(RoutedPropertyChangedEventHandler<double>),
			typeof(TickBarAdvanced));

		internal double CalculateTickRenderCoordinate(decimal value)
		{
			var primaryAxisLength = Orientation == Orientation.Horizontal ? ActualWidth : ActualHeight;
			var position          = (double)(value - Minimum) * primaryAxisLength / (double)(Maximum - Minimum);
			if (IsDirectionReversed && Orientation == Orientation.Horizontal ||
				!IsDirectionReversed && Orientation == Orientation.Vertical)
				position = primaryAxisLength - position;
			return position;
		}
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

		private List<(decimal value, TickTypes type)> GenerateTicks()
		{
			var ticks = new List<(decimal value, TickTypes type)>();

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
						ticks.Add((adjustedMinor, tickFlags));
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
						ticks.Add((adjustedMajor, tickFlags));
						major         += MajorTickFrequency;
						adjustedMajor =  decimal.Round(major, DecimalPrecision, MidpointRounding.ToEven);
					}
				}
			}
			else if (MajorTickFrequency > 0) // Only major ticks needed
			{
				while (adjustedMajor <= Maximum)
				{
					ticks.Add((adjustedMajor, adjustedMajor == 0 ? TickTypes.Origin | TickTypes.Major : TickTypes.Major));
					major         += MajorTickFrequency;
					adjustedMajor =  decimal.Round(major, DecimalPrecision, MidpointRounding.ToEven);
				}
			}
			else if (MinorTickFrequency > 0) // Only minor ticks needed
			{
				while (adjustedMinor <= Maximum)
				{
					ticks.Add((adjustedMinor, adjustedMinor == 0 ? TickTypes.Origin | TickTypes.Minor : TickTypes.Minor));
					minor         += MinorTickFrequency;
					adjustedMinor =  decimal.Round(minor, DecimalPrecision, MidpointRounding.ToEven);
				}
			}

			return ticks;
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
				((List<(decimal, TickTypes)>)e.NewValue).Sort();
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
			var secAxisOrigin       = CalculateSecondaryAxisCoordinates(OriginTickRelativeSize);
			var secAxisMajor        = CalculateSecondaryAxisCoordinates(MajorTickRelativeSize);
			var secAxisMinor        = CalculateSecondaryAxisCoordinates(MinorTickRelativeSize);

			// Determine draw order for different tick types
			var drawOrder = new SortedList<int, TickTypes> { { OriginTickZIndex, TickTypes.Origin } };
			if (drawOrder.ContainsKey(MajorTickZIndex))
				drawOrder[MajorTickZIndex] |= TickTypes.Major;
			else
				drawOrder.Add(MajorTickZIndex, TickTypes.Major);
			if (drawOrder.ContainsKey(MinorTickZIndex))
				drawOrder[MinorTickZIndex] |= TickTypes.Minor;
			else
				drawOrder.Add(MinorTickZIndex, TickTypes.Minor);

			// Draw background
			drawingContext.DrawRectangle(Background, null, new Rect(new Point(0, 0), new Size(ActualWidth, ActualHeight)));

			// Draw ticks
			var smallestGap = primaryAxisLength;
			(double position, double thickness) prevTick = (0, 0);
			for (var i = 0; i < Ticks.Count; i++)
			{
				var thickest = 0.0;
				var position = CalculateTickRenderCoordinate(Ticks[i].value);
				if (position < 0 || position > primaryAxisLength)
					continue;

				foreach (var z in drawOrder)
				{
					switch (Ticks[i].type & z.Value)
					{
						case TickTypes.Minor:
							DrawTick(secAxisMinor, ref _minorTickPen);
							thickest = Math.Max(thickest, MinorTickThickness);
							break;
						case TickTypes.Major:
						case TickTypes.Major | TickTypes.Minor:
							DrawTick(secAxisMajor, ref _majorTickPen);
							thickest = Math.Max(thickest, MajorTickThickness);
							break;
						case TickTypes.Origin:
						case TickTypes.Origin | TickTypes.Major:
						case TickTypes.Origin | TickTypes.Minor:
						case TickTypes.Origin | TickTypes.Major | TickTypes.Minor:
							DrawTick(secAxisOrigin, ref _originTickPen);
							thickest = Math.Max(thickest, OriginTickThickness);
							break;
					}
				}

				if (i > 0)
				{
					var gap = position > prevTick.position
						? position - prevTick.position - thickest / 2 - prevTick.thickness / 2
						: prevTick.position - position - thickest / 2 - prevTick.thickness / 2;
					smallestGap = Math.Min(gap, smallestGap);
				}

				prevTick = (position, thickest);

				void DrawTick(Range<double> secAxisCoords, ref Pen pen)
				{
					// For any ticks that are equal to the max or min values,
					// visually shift the tick inwards by half of its thickness.
					if (IsShiftBoundaryTicks)
					{
						if (position < pen.Thickness / 2)
							position = pen.Thickness / 2;
						else if (primaryAxisLength - position < pen.Thickness / 2)
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

			if (Math.Abs(smallestGap - _smallestTickGap) > double.Epsilon)
			{
				RaiseEvent(new RoutedPropertyChangedEventArgs<double>(_smallestTickGap, smallestGap, SmallestTickGapChangedEvent));
				_smallestTickGap = smallestGap;
			}

			Range<double> CalculateSecondaryAxisCoordinates(double relativeSize)
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

	public class TickListConverter : TypeConverter, IValueConverter
	{
		/// <inheritdoc />
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
		{
			return sourceType == typeof(string);
		}

		/// <inheritdoc />
		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
		{
			return destinationType == typeof(string);
		}

		/// <inheritdoc />
		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			if (!(value is string strValue))
				return null;

			var result      = new List<(decimal value, TickTypes type)>();
			var tickStrings = strValue.Split(',', ' ');
			foreach (var tickString in tickStrings)
			{
				var paramStrings = tickString.Split(':');
				if (!decimal.TryParse(paramStrings[0], out var number))
					continue;

				TickTypes tickType = 0;
				if (paramStrings.Length >= 2)
				{
					foreach (var ch in paramStrings[1])
					{
						switch (ch)
						{
							case 'O':
								tickType |= TickTypes.Origin;
								break;
							case 'M':
								tickType |= TickTypes.Major;
								break;
							case 'm':
								tickType |= TickTypes.Minor;
								break;
						}
					}
				}

				result.Add((number, tickType == 0 ? TickTypes.Major : tickType));
			}

			return result;
		}

		/// <inheritdoc />
		public override object ConvertTo(ITypeDescriptorContext context,
										 CultureInfo culture,
										 object value,
										 Type destinationType)
		{
			if (!(value is List<(decimal value, TickTypes type)> ticks))
				return null;

			var str = new StringBuilder();
			foreach (var tick in ticks)
			{
				str.Append(tick.value);
				str.Append(':');
				if (tick.type.HasFlag(TickTypes.Origin))
					str.Append('O');
				if (tick.type.HasFlag(TickTypes.Major))
					str.Append('M');
				if (tick.type.HasFlag(TickTypes.Minor))
					str.Append('m');
				str.Append(", ");
			}

			if (ticks.Count > 1)
				str.Remove(str.Length - 2, 2);
			return str.ToString();
		}

		#region IValueConverter Implementation
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return ConvertTo(null, culture, value, targetType);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return ConvertFrom(value);
		}
		#endregion
	}
}