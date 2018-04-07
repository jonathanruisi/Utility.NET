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
using System.Linq;
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
		#endregion

		#region Properties
		#region General
		/// <summary>
		/// Gets or sets the primary drawing axis as either
		/// <see cref="System.Windows.Controls.Orientation.Horizontal"/> or
		/// <see cref="System.Windows.Controls.Orientation.Vertical"/>.
		/// </summary>
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

		/// <summary>
		/// Gets or sets the alignment of the tick marks.
		/// When <see cref="Orientation"/> = <see cref="System.Windows.Controls.Orientation.Horizontal"/>,
		/// possible values include
		/// <see cref="Position.Left"/>, <see cref="Position.Center"/>, and <see cref="Position.Right"/>.
		/// When <see cref="Orientation"/> = <see cref="System.Windows.Controls.Orientation.Vertical"/>,
		/// possible values include
		/// <see cref="Position.Top"/>, <see cref="Position.Middle"/>, and <see cref="Position.Bottom"/>.
		/// </summary>
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

		/// <summary>
		/// Gets or sets whether or not the primary axis is reversed, thereby drawing the tick marks in reverse order.
		/// For <see cref="Orientation"/> = <see cref="System.Windows.Controls.Orientation.Horizontal"/>,
		/// tick marks are positioned right-to-left, whereas
		/// for <see cref="Orientation"/> = <see cref="System.Windows.Controls.Orientation.Vertical"/>,
		/// tick marks are positioned bottom-to-top.
		/// </summary>
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

		/// <summary>
		/// Gets or sets whether or not tick marks equal to <see cref="Minimum"/> or <see cref="Maximum"/>
		/// are shifted inwards so that their full thickness is visible.
		/// If set to <code>true</code>, tick marks occuring at the boundaries will be shifted inwards
		/// (visually) by 1/2 their thickness, so they can be rendered at their full thickness.
		/// If set to <code>false</code>, no tick marks will be adjusted in this way, and may appear
		/// to be cut off at boundary values.
		/// </summary>
		public bool IsBoundaryTickInwardShift
		{
			get => (bool)GetValue(IsBoundaryTickInwardShiftProperty);
			set => SetValue(IsBoundaryTickInwardShiftProperty, value);
		}

		public static readonly DependencyProperty IsBoundaryTickInwardShiftProperty = DependencyProperty.Register(
			"IsBoundaryTickInwardShift",
			typeof(bool),
			typeof(TickBarAdvanced),
			new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender));
		#endregion

		#region Range
		/// <summary>
		/// Gets or sets the <see cref="Minimum"/> value above which tick marks are visible.
		/// </summary>
		public decimal Minimum { get => (decimal)GetValue(MinimumProperty); set => SetValue(MinimumProperty, value); }

		public static readonly DependencyProperty MinimumProperty = DependencyProperty.Register(
			"Minimum",
			typeof(decimal),
			typeof(TickBarAdvanced),
			new FrameworkPropertyMetadata(0.0M, FrameworkPropertyMetadataOptions.AffectsRender, OnDependencyPropertyChanged));

		/// <summary>
		/// Gets or sets the <see cref="Maximum"/> value below which tick marks are visible.
		/// </summary>
		public decimal Maximum { get => (decimal)GetValue(MaximumProperty); set => SetValue(MaximumProperty, value); }

		public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register(
			"Maximum",
			typeof(decimal),
			typeof(TickBarAdvanced),
			new FrameworkPropertyMetadata(10.0M, FrameworkPropertyMetadataOptions.AffectsRender, OnDependencyPropertyChanged));
		#endregion

		#region Tick Positioning
		/// <summary>
		/// Gets or sets the collection of values that each correspond to the position of a major tick mark.
		/// Setting this property directly overrides <see cref="MajorTickFrequency"/>.
		/// </summary>
		public List<decimal> MajorTicks
		{
			get => (List<decimal>)GetValue(MajorTicksProperty);
			set => SetValue(MajorTicksProperty, value);
		}

		public static readonly DependencyProperty MajorTicksProperty = DependencyProperty.Register(
			"MajorTicks",
			typeof(List<decimal>),
			typeof(TickBarAdvanced),
			new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

		/// <summary>
		/// Gets or sets the collection of values that each correspond to the position of a minor tick mark.
		/// Setting this property directly overrides <see cref="MinorTickFrequency"/>.
		/// </summary>
		public List<decimal> MinorTicks
		{
			get => (List<decimal>)GetValue(MinorTicksProperty);
			set => SetValue(MinorTicksProperty, value);
		}

		public static readonly DependencyProperty MinorTicksProperty = DependencyProperty.Register(
			"MinorTicks",
			typeof(List<decimal>),
			typeof(TickBarAdvanced),
			new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

		/// <summary>
		/// Gets or sets the interval between major tick marks.
		/// Setting this property directly clears (and re-creates) the <see cref="MajorTicks"/> collection.
		/// </summary>
		public decimal MajorTickFrequency
		{
			get => (decimal)GetValue(MajorTickFrequencyProperty);
			set => SetValue(MajorTickFrequencyProperty, value);
		}

		public static readonly DependencyProperty MajorTickFrequencyProperty = DependencyProperty.Register(
			"MajorTickFrequency",
			typeof(decimal),
			typeof(TickBarAdvanced),
			new FrameworkPropertyMetadata(1.0M, FrameworkPropertyMetadataOptions.AffectsRender, OnDependencyPropertyChanged));

		/// <summary>
		/// Gets or sets the interval between minor tick marks.
		/// Setting this property directly clears (and re-creates) the <see cref="MinorTicks"/> collection.
		/// </summary>
		public decimal MinorTickFrequency
		{
			get => (decimal)GetValue(MinorTickFrequencyProperty);
			set => SetValue(MinorTickFrequencyProperty, value);
		}

		public static readonly DependencyProperty MinorTickFrequencyProperty = DependencyProperty.Register(
			"MinorTickFrequency",
			typeof(decimal),
			typeof(TickBarAdvanced),
			new FrameworkPropertyMetadata(0.25M, FrameworkPropertyMetadataOptions.AffectsRender, OnDependencyPropertyChanged));
		#endregion

		#region Tick Size
		/// <summary>
		/// Gets or sets the length of the origin tick mark relative to
		/// the height (or width) of the control's secondary axis.
		/// </summary>
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

		/// <summary>
		/// Gets or sets the length of all major tick marks relative to
		/// the height (or width) of the control's secondary axis.
		/// </summary>
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

		/// <summary>
		/// Gets or sets the length of all minor tick marks relative to
		/// the height (or width) of the control's secondary axis.
		/// </summary>
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

		/// <summary>
		/// Gets or sets the thickness of the origin tick mark.
		/// </summary>
		public double OriginTickThickness
		{
			get => (double)GetValue(OriginTickThicknessProperty);
			set => SetValue(OriginTickThicknessProperty, value);
		}

		public static readonly DependencyProperty OriginTickThicknessProperty = DependencyProperty.Register(
			"OriginTickThickness",
			typeof(double),
			typeof(TickBarAdvanced),
			new FrameworkPropertyMetadata(2.0, FrameworkPropertyMetadataOptions.AffectsRender, OnDependencyPropertyChanged));

		/// <summary>
		/// Gets or sets the thickness of all major tick marks.
		/// </summary>
		public double MajorTickThickness
		{
			get => (double)GetValue(MajorTickThicknessProperty);
			set => SetValue(MajorTickThicknessProperty, value);
		}

		public static readonly DependencyProperty MajorTickThicknessProperty = DependencyProperty.Register(
			"MajorTickThickness",
			typeof(double),
			typeof(TickBarAdvanced),
			new FrameworkPropertyMetadata(1.0, FrameworkPropertyMetadataOptions.AffectsRender, OnDependencyPropertyChanged));

		/// <summary>
		/// Gets or sets the thickness of all minor tick marks.
		/// </summary>
		public double MinorTickThickness
		{
			get => (double)GetValue(MinorTickThicknessProperty);
			set => SetValue(MinorTickThicknessProperty, value);
		}

		public static readonly DependencyProperty MinorTickThicknessProperty = DependencyProperty.Register(
			"MinorTickThickness",
			typeof(double),
			typeof(TickBarAdvanced),
			new FrameworkPropertyMetadata(1.0, FrameworkPropertyMetadataOptions.AffectsRender, OnDependencyPropertyChanged));
		#endregion

		#region Brush
		/// <summary>
		/// Gets or sets a brush that describes the background of this control.
		/// </summary>
		public Brush Background { get => (Brush)GetValue(BackgroundProperty); set => SetValue(BackgroundProperty, value); }

		public static readonly DependencyProperty BackgroundProperty = DependencyProperty.Register(
			"Background",
			typeof(Brush),
			typeof(TickBarAdvanced),
			new FrameworkPropertyMetadata(Brushes.Transparent, FrameworkPropertyMetadataOptions.AffectsRender));

		/// <summary>
		/// Gets or sets a brush that describes the color of the origin tick mark.
		/// </summary>
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
				OnDependencyPropertyChanged));

		/// <summary>
		/// Gets or sets a brush that describes the color of all major tick marks.
		/// </summary>
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
				OnDependencyPropertyChanged));

		/// <summary>
		/// Gets or sets a brush that describes the color of all minor tick marks.
		/// </summary>
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
				OnDependencyPropertyChanged));
		#endregion
		#endregion

		#region Internal Properties
		/// <summary>
		/// Calculates the density of tick marks currently visible as a fraction of the available drawing space.
		/// <see cref="TickDensity"/>=0 indicates that no tick marks are visible,
		/// whereas <see cref="TickDensity"/>=1 indicates that the entire primary drawing axis
		/// is saturated with tick marks, and no free space is visible.
		/// </summary>
		internal decimal TickDensity
		{
			get
			{
				var result =
					MajorTicks.Where(tick => tick > Minimum && tick < Maximum && tick != 0).Sum(tick => (decimal)MajorTickThickness) +
					MinorTicks.Where(tick => tick > Minimum && tick < Maximum && tick != 0).Sum(tick => (decimal)MinorTickThickness);
				if (Minimum <= 0 && Maximum >= 0)
					result += (decimal)OriginTickThickness;
				switch (Orientation)
				{
					case Orientation.Horizontal when ActualWidth > 0:
						return result / (decimal)ActualWidth;
					case Orientation.Vertical when ActualHeight > 0:
						return result / (decimal)ActualHeight;
					default:
						return 0;
				}
			}
		}

		internal double PrimaryAxisLength => Orientation == Orientation.Horizontal ? ActualWidth : ActualHeight;
		internal double SecondaryAxisLength => Orientation == Orientation.Horizontal ? ActualHeight : ActualWidth;
		#endregion

		#region Constructors
		static TickBarAdvanced()
		{
			DefaultStyleKeyProperty.OverrideMetadata(
				typeof(TickBarAdvanced),
				new FrameworkPropertyMetadata(typeof(TickBarAdvanced)));
		}

		public TickBarAdvanced()
		{
			Initialized += TickBarAdvanced_Initialized;
		}
		#endregion

		#region Internal Methods
		internal double GetTickRenderPosition(decimal value)
		{
			var position = (double)(value - Minimum) * PrimaryAxisLength / (double)(Maximum - Minimum);
			if (IsDirectionReversed)
				position = PrimaryAxisLength - position;
			return position;
		}
		#endregion

		#region Private Methods
		private void UpdatePens()
		{
			_originTickPen = new Pen(OriginTickBrush, OriginTickThickness);
			_originTickPen.Freeze();
			_majorTickPen = new Pen(MajorTickBrush, MajorTickThickness);
			_majorTickPen.Freeze();
			_minorTickPen = new Pen(MinorTickBrush, MinorTickThickness);
			_minorTickPen.Freeze();
		}

		private void UpdateTicks()
		{
			if (MajorTickFrequency > 0)
			{
				MajorTicks = new List<decimal>();
				var firstTick = MajorTickFrequency * (int)(Minimum / MajorTickFrequency);
				if (Minimum >= 0 && Math.Abs(firstTick - Minimum) > 0)
					firstTick = MajorTickFrequency * ((int)(Minimum / MajorTickFrequency) + 1);
				for (var i = firstTick; i <= Maximum; i += MajorTickFrequency)
				{
					MajorTicks.Add(i);
				}
			}

			if (MinorTickFrequency > 0)
			{
				MinorTicks = new List<decimal>();
				var firstTick = MinorTickFrequency * (int)(Minimum / MinorTickFrequency);
				if (Minimum >= 0 && Math.Abs(firstTick - Minimum) > 0)
					firstTick = MinorTickFrequency * ((int)(Minimum / MinorTickFrequency) + 1);
				for (var i = firstTick; i <= Maximum; i += MinorTickFrequency)
				{
					MinorTicks.Add(i);
				}
			}

			MajorTicks.Sort();
			MinorTicks.Sort();
		}
		#endregion

		#region Property Changed Callbacks, Coerce Value Callbacks, Validate Value Callbacks
		private static void OnDependencyPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (!(d is TickBarAdvanced tickBar)) return;

			if (e.Property == OriginTickBrushProperty || e.Property == MajorTickBrushProperty ||
				e.Property == MinorTickBrushProperty || e.Property == OriginTickThicknessProperty ||
				e.Property == MajorTickThicknessProperty || e.Property == MinorTickThicknessProperty)
			{
				tickBar.UpdatePens();
			}
			else if (e.Property == MinimumProperty || e.Property == MaximumProperty ||
				e.Property == MajorTickFrequencyProperty || e.Property == MinorTickFrequencyProperty)
			{
				if (e.Property == MajorTickFrequencyProperty && (decimal)e.NewValue == 0)
					tickBar.MajorTicks = new List<decimal>();
				else if (e.Property == MinorTickFrequencyProperty && (decimal)e.NewValue == 0)
					tickBar.MinorTicks = new List<decimal>();
				tickBar.UpdateTicks();
			}
		}
		#endregion

		#region Layout and Render Methods
		///<inheritdoc cref="UIElement.OnRender"/>
		protected override void OnRender(DrawingContext drawingContext)
		{
			if (Math.Abs(ActualWidth) < double.Epsilon || Math.Abs(ActualHeight) < double.Epsilon || Maximum - Minimum == 0)
				return;

			// Calculate known pixel coordinates based on orientation and placement
			var overlapThreshold = (Maximum - Minimum) / (decimal)PrimaryAxisLength;
			var secAxisOrigin    = CalculateSecondaryAxisLineCoordinates(OriginTickRelativeSize);
			var secAxisMajor     = CalculateSecondaryAxisLineCoordinates(MajorTickRelativeSize);
			var secAxisMinor     = CalculateSecondaryAxisLineCoordinates(MinorTickRelativeSize);

			// Draw background
			drawingContext.DrawRectangle(Background, null, new Rect(new Point(0, 0), new Size(ActualWidth, ActualHeight)));

			// Draw unique major and minor ticks (ticks that are at least overlapThreshold apart in value)
			int i = 0, j = 0;
			while (i < MajorTicks.Count && j < MinorTicks.Count)
			{
				if (MinorTicks[j] < MajorTicks[i])
				{
					if (Math.Abs(MajorTicks[i] - MinorTicks[j]) > overlapThreshold && MinorTicks[j] != 0)
						DrawTick(GetTickRenderPosition(MinorTicks[j]), secAxisMinor, ref _minorTickPen);
					j++;
				}
				else if (MinorTicks[j] >= MajorTicks[i])
				{
					if (MajorTicks[i] != 0)
						DrawTick(GetTickRenderPosition(MajorTicks[i]), secAxisMajor, ref _majorTickPen);
					if (Math.Abs(MinorTicks[j] - MajorTicks[i]) <= overlapThreshold)
						j++;
					i++;
				}
			}

			// Draw remaining major ticks
			while (i < MajorTicks.Count)
			{
				if (MajorTicks[i] != 0)
					DrawTick(GetTickRenderPosition(MajorTicks[i]), secAxisMajor, ref _majorTickPen);
				i++;
			}

			// Draw remaining minor ticks
			while (j < MinorTicks.Count)
			{
				if (MinorTicks[j] != 0)
					DrawTick(GetTickRenderPosition(MinorTicks[j]), secAxisMinor, ref _minorTickPen);
				j++;
			}

			// Draw origin tick
			if (Minimum <= 0 && Maximum >= 0)
			{
				DrawTick(GetTickRenderPosition(0), secAxisOrigin, ref _originTickPen);
			}

			// Local function which calculates the start and end coordinates (secondary axis) for a line.
			// When Orientation==Horizontal, the secondary axis is the Y axis.
			// When Orientation==Vertical, the secondary axis is the X axis.
			(double start, double end) CalculateSecondaryAxisLineCoordinates(double relativeSize)
			{
				switch (TickAlignment)
				{
					case Position.Top:
					case Position.Left:
						return (0, relativeSize * SecondaryAxisLength);
					case Position.Middle:
					case Position.Center:
						var startCoord = (SecondaryAxisLength - (relativeSize * SecondaryAxisLength)) / 2;
						return (startCoord, SecondaryAxisLength - startCoord);
					case Position.Bottom:
					case Position.Right:
						return (SecondaryAxisLength, SecondaryAxisLength - relativeSize * SecondaryAxisLength);
					default:
						return (0, 0);
				}
			}

			// Local funtion which draws each tick given its screen coordinates and type-specific pen
			void DrawTick(double position, (double start, double end) secAxisCoords, ref Pen pen)
			{
				// For any ticks that are equal to the max or min values,
				// visually shift the tick inwards by half of its thickness.
				if (IsBoundaryTickInwardShift)
				{
					if (Math.Abs(position) < double.Epsilon)
						position = pen.Thickness / 2;
					else if (Math.Abs(position - PrimaryAxisLength) < double.Epsilon)
						position = PrimaryAxisLength - pen.Thickness / 2;
				}

				if (Orientation == Orientation.Horizontal)
					drawingContext.DrawLine(pen, new Point(position, secAxisCoords.start), new Point(position, secAxisCoords.end));
				else
					drawingContext.DrawLine(pen, new Point(secAxisCoords.start, position), new Point(secAxisCoords.end, position));
			}
		}
		#endregion

		#region Event Handlers
		private void TickBarAdvanced_Initialized(object sender, EventArgs e)
		{
			UpdatePens();
			UpdateTicks();
		}
		#endregion
	}
}