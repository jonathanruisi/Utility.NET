// ┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  FILE:       MediaSlider.cs
// ┃  PROJECT:    Utility.WPF
// ┃  SOLUTION:   Utility.NET
// ┃  CREATED:    2018-03-13 @ 1:00 AM
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
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

using JLR.Utility.NET;
using JLR.Utility.NET.Math;
using JLR.Utility.WPF.Elements;

namespace JLR.Utility.WPF.Controls
{
	[TemplatePart(Name        = "PART_TickBar", Type             = typeof(TickBarAdvanced))]
	[TemplatePart(Name        = "PART_Position", Type            = typeof(ContentPresenter))]
	[TemplatePart(Name        = "PART_SelectionStart", Type      = typeof(ContentPresenter))]
	[TemplatePart(Name        = "PART_SelectionEnd", Type        = typeof(ContentPresenter))]
	[TemplatePart(Name        = "PART_SelectionRange", Type      = typeof(ContentPresenter))]
	[TemplatePart(Name        = "PART_VisibleRangeStart", Type   = typeof(ContentPresenter))]
	[TemplatePart(Name        = "PART_VisibleRangeEnd", Type     = typeof(ContentPresenter))]
	[TemplatePart(Name        = "PART_VisibleRange", Type        = typeof(ContentPresenter))]
	[TemplateVisualState(Name = "Normal", GroupName              = "MouseStates")]
	[TemplateVisualState(Name = "MouseOver", GroupName           = "MouseStates")]
	[TemplateVisualState(Name = "MouseLeftButtonDown", GroupName = "MouseStates")]
	public class MediaSlider : Control
	{
		#region Constants
		private const decimal MinVisibleRange = 1;
		#endregion

		#region Fields
		private ContentPresenter _positionElement;
		private ContentPresenter _visRngStartElement, _visRngEndElement, _visRngElement;
		private ContentPresenter _selStartElement, _selEndElement, _selRngElement;
		private Panel _mainPanel, _zoomPanel;
		private TickBarAdvanced _tickBar;
		private bool _isMouseLeftButtonDown;
		private bool _isSelRngChanging, _isVisRngChanging, _isVisRngDragging;
		private double _prevMouseCoord;
		private LinkedList<int> _fpsDivisors;
		private LinkedListNode<int> _currentFpsDivisor;
		private Range<decimal>? _selectionRangeOld;
		private Range<decimal> _visibleRangeOld;
		private (decimal temporal, double visual) _snapDistance;
		#endregion

		#region Properties
		/// <summary>
		/// Gets or sets the current selection range.
		/// This value wraps the <see cref="SelectionStart"/> and <see cref="SelectionEnd"/> properties.
		/// Avoid setting the selection range via this property if the
		/// individual selection range bounds are dependent on each other.
		/// </summary>
		public Range<decimal>? SelectionRange
		{
			get
			{
				if (SelectionStart == null || SelectionEnd == null)
					return null;
				return new Range<decimal>((decimal)SelectionStart, (decimal)SelectionEnd);
			}
			set
			{
				if (value.HasValue)
				{
					if (value.Value.Minimum <= SelectionEnd)
					{
						_isSelRngChanging = true;
						SelectionStart    = value.Value.Minimum;
						SelectionEnd      = value.Value.Maximum;
					}
					else if (value.Value.Maximum >= SelectionStart)
					{
						_isSelRngChanging = true;
						SelectionEnd      = value.Value.Maximum;
						SelectionStart    = value.Value.Minimum;
					}
				}
				else
				{
					SelectionStart = null; // SelectionEnd will be set to null by the property changed callback
				}
			}
		}

		/// <summary>
		/// Gets or sets the visible range.
		/// This value wraps the <see cref="VisibleRangeStart"/> and <see cref="VisibleRangeEnd"/> properties.
		/// Avoid setting the visual range via this property if the
		/// individual visual range bounds are dependent on each other.
		/// </summary>
		public Range<decimal> VisibleRange
		{
			get => new Range<decimal>(VisibleRangeStart, VisibleRangeEnd);
			set
			{
				if (value.Minimum <= VisibleRangeEnd)
				{
					_isVisRngChanging = true;
					VisibleRangeStart = value.Minimum;
					VisibleRangeEnd   = value.Maximum;
				}
				else if (value.Maximum >= VisibleRangeStart)
				{
					_isVisRngChanging = true;
					VisibleRangeEnd   = value.Maximum;
					VisibleRangeStart = value.Minimum;
				}
			}
		}

		/// <summary>
		/// Gets or sets the zoom factor.
		/// This is a value between 0 and 1 that represents the
		/// visual range as a fraction of the total media duration.
		/// </summary>
		public decimal ZoomFactor
		{
			get => VisibleRange.Magnitude() / (Maximum - Minimum);
			set
			{
				if (value < 0 || value > 1)
					throw new ArgumentOutOfRangeException(nameof(value), $@"{nameof(value)} must be between 0 and 1");
				var changeLimit = (MinVisibleRange - VisibleRangeEnd + VisibleRangeStart) / 2;
				var change      = Math.Max((value * (Maximum - Minimum) - VisibleRange.Magnitude()) / 2, changeLimit);
				VisibleRange = new Range<decimal>(VisibleRangeStart - change, VisibleRangeEnd + change);
			}
		}
		#endregion

		#region Dependency Properties
		#region General
		/// <summary>
		/// Gets or sets the minimum acceptable amount of space (in pixels) between tick marks.
		/// This value is used to automatically adjust the density of tick marks (visual snap distance)
		/// based on the current size of the control and the current visible range.
		/// When zooming in for example, tick marks will be added until every frame/sample is represented.
		/// Likewise, tick marks will be removed when zooming out.
		/// </summary>
		public double TickDensityThreshold
		{
			get => (double)GetValue(TickDensityThresholdProperty);
			set => SetValue(TickDensityThresholdProperty, value);
		}

		public static readonly DependencyProperty TickDensityThresholdProperty = DependencyProperty.Register(
			"TickDensityThreshold",
			typeof(double),
			typeof(MediaSlider),
			new FrameworkPropertyMetadata(3.0));
		#endregion

		#region Media
		/// <summary>
		/// Gets or sets the number of frames (or samples) into which each second of media is divided.
		/// This property does not currently support drop-frame timecode,
		/// therefore it is recommended to set this value to the nearest whole number of frames.
		/// For example, a video file with an actual framerate of 29.97fps would set this value to 30.
		/// TODO: This control is currently time-based, and does not support accurate frame/sample selection - add this capability!
		/// </summary>
		public int FramesPerSecond
		{
			get => (int)GetValue(FramesPerSecondProperty);
			set => SetValue(FramesPerSecondProperty, value);
		}

		public static readonly DependencyProperty FramesPerSecondProperty = DependencyProperty.Register(
			"FramesPerSecond",
			typeof(int),
			typeof(MediaSlider),
			new FrameworkPropertyMetadata(0, OnMediaPropertyChanged));
		#endregion

		#region Range
		/// <summary>
		/// Gets or sets the <see cref="Minimum"/> possible <see cref="Position"/> in the <see cref="MediaSlider"/>.
		/// </summary>
		public decimal Minimum { get => (decimal)GetValue(MinimumProperty); set => SetValue(MinimumProperty, value); }

		public static readonly DependencyProperty MinimumProperty = DependencyProperty.Register(
			"Minimum",
			typeof(decimal),
			typeof(MediaSlider),
			new FrameworkPropertyMetadata(0.0M, OnRangePropertyChanged));

		/// <summary>
		/// Gets or sets the <see cref="Maximum"/> possible <see cref="Position"/> in the <see cref="MediaSlider"/>.
		/// </summary>
		public decimal Maximum { get => (decimal)GetValue(MaximumProperty); set => SetValue(MaximumProperty, value); }

		public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register(
			"Maximum",
			typeof(decimal),
			typeof(MediaSlider),
			new FrameworkPropertyMetadata(0.0M, OnRangePropertyChanged, CoerceMaximum));

		/// <summary>
		/// Gets or sets the current <see cref="Position"/> of the playhead within the <see cref="MediaSlider"/>.
		/// </summary>
		public decimal Position { get => (decimal)GetValue(PositionProperty); set => SetValue(PositionProperty, value); }

		public static readonly DependencyProperty PositionProperty = DependencyProperty.Register(
			"Position",
			typeof(decimal),
			typeof(MediaSlider),
			new FrameworkPropertyMetadata(0.0M, OnRangePropertyChanged, CoercePosition));

		/// <summary>
		/// Gets or sets the lower bound of the <see cref="SelectionRange"/>.
		/// Setting this value to <code>null</code> disables the selection and hides the relevant visual elements.
		/// </summary>
		public decimal? SelectionStart
		{
			get => (decimal?)GetValue(SelectionStartProperty);
			set => SetValue(SelectionStartProperty, value);
		}

		public static readonly DependencyProperty SelectionStartProperty = DependencyProperty.Register(
			"SelectionStart",
			typeof(decimal?),
			typeof(MediaSlider),
			new FrameworkPropertyMetadata(null, OnRangePropertyChanged, CoerceSelectionStart));

		/// <summary>
		/// Gets or sets the upper bound of the <see cref="SelectionRange"/>.
		/// Setting this value to <code>null</code> disables the selection and hides the relevant visual elements.
		/// </summary>
		public decimal? SelectionEnd
		{
			get => (decimal?)GetValue(SelectionEndProperty);
			set => SetValue(SelectionEndProperty, value);
		}

		public static readonly DependencyProperty SelectionEndProperty = DependencyProperty.Register(
			"SelectionEnd",
			typeof(decimal?),
			typeof(MediaSlider),
			new FrameworkPropertyMetadata(null, OnRangePropertyChanged, CoerceSelectionEnd));

		/// <summary>
		/// Gets or sets the lower bound of the <see cref="VisibleRange"/> (zoom).
		/// </summary>
		public decimal VisibleRangeStart
		{
			get => (decimal)GetValue(VisibleRangeStartProperty);
			set => SetValue(VisibleRangeStartProperty, value);
		}

		public static readonly DependencyProperty VisibleRangeStartProperty = DependencyProperty.Register(
			"VisibleRangeStart",
			typeof(decimal),
			typeof(MediaSlider),
			new FrameworkPropertyMetadata(0.0M, OnRangePropertyChanged, CoerceVisibleRangeStart));

		/// <summary>
		/// Gets or sets the upper bound of the <see cref="VisibleRange"/> (zoom).
		/// </summary>
		public decimal VisibleRangeEnd
		{
			get => (decimal)GetValue(VisibleRangeEndProperty);
			set => SetValue(VisibleRangeEndProperty, value);
		}

		public static readonly DependencyProperty VisibleRangeEndProperty = DependencyProperty.Register(
			"VisibleRangeEnd",
			typeof(decimal),
			typeof(MediaSlider),
			new FrameworkPropertyMetadata(0.0M, OnRangePropertyChanged, CoerceVisibleRangeEnd));
		#endregion

		#region Alignment and Sizing
		/// <summary>
		/// Gets or sets the vertical alignment of the tick marks. Possible values include
		/// <see cref="NET.Position.Top"/>, <see cref="NET.Position.Middle"/>, and <see cref="NET.Position.Bottom"/>.
		/// </summary>
		public Position TickAlignment
		{
			get => (Position)GetValue(TickAlignmentProperty);
			set => SetValue(TickAlignmentProperty, value);
		}

		public static readonly DependencyProperty TickAlignmentProperty = DependencyProperty.Register(
			"TickAlignment",
			typeof(JLR.Utility.NET.Position),
			typeof(MediaSlider),
			new FrameworkPropertyMetadata(JLR.Utility.NET.Position.Bottom));

		/// <summary>
		/// Gets or sets the vertical alignment of the playhead.
		/// A seperate <see cref="DataTemplate"/> can be defined for each possible <see cref="NET.Position"/>:
		/// (<see cref="NET.Position.Top"/>, <see cref="NET.Position.Middle"/>, <see cref="NET.Position.Bottom"/>).
		/// </summary>
		public Position PositionAlignment
		{
			get => (Position)GetValue(PositionAlignmentProperty);
			set => SetValue(PositionAlignmentProperty, value);
		}

		public static readonly DependencyProperty PositionAlignmentProperty = DependencyProperty.Register(
			"PositionAlignment",
			typeof(Position),
			typeof(MediaSlider),
			new FrameworkPropertyMetadata(JLR.Utility.NET.Position.Top, OnAlignmentAndSizingPropertyChanged));

		/// <summary>
		/// Gets or sets the vertical alignment of the selection range sliders. Possible values include
		/// <see cref="NET.Position.Top"/>, <see cref="NET.Position.Middle"/>, and <see cref="NET.Position.Bottom"/>.
		/// </summary>
		public Position SelectionRangeAlignment
		{
			get => (Position)GetValue(SelectionRangeAlignmentProperty);
			set => SetValue(SelectionRangeAlignmentProperty, value);
		}

		public static readonly DependencyProperty SelectionRangeAlignmentProperty = DependencyProperty.Register(
			"SelectionRangeAlignment",
			typeof(Position),
			typeof(MediaSlider),
			new FrameworkPropertyMetadata(JLR.Utility.NET.Position.Top, OnAlignmentAndSizingPropertyChanged));

		/// <summary>
		/// Gets or sets the vertical alignment of the highlight element between the selection range sliders.
		/// This value is relative to the selection range sliders, and possible values include
		/// <see cref="NET.Position.Top"/>, <see cref="NET.Position.Middle"/>, and <see cref="NET.Position.Bottom"/>.
		/// </summary>
		public Position SelectionRangeHighlightAlignment
		{
			get => (Position)GetValue(SelectionRangeHighlightAlignmentProperty);
			set => SetValue(SelectionRangeHighlightAlignmentProperty, value);
		}

		public static readonly DependencyProperty SelectionRangeHighlightAlignmentProperty = DependencyProperty.Register(
			"SelectionRangeHighlightAlignment",
			typeof(Position),
			typeof(MediaSlider),
			new FrameworkPropertyMetadata(JLR.Utility.NET.Position.Middle, OnAlignmentAndSizingPropertyChanged));

		/// <summary>
		/// Gets or sets the <see cref="GridLength"/> (calculated height) of the zoom bar,
		/// which defines the <see cref="VisibleRange"/> of this control.
		/// A <see cref="GridUnitType.Star"/> value is relative to the size of the main slider area.
		/// </summary>
		public GridLength ZoomBarSize
		{
			get => (GridLength)GetValue(ZoomBarSizeProperty);
			set => SetValue(ZoomBarSizeProperty, value);
		}

		public static readonly DependencyProperty ZoomBarSizeProperty = DependencyProperty.Register(
			"ZoomBarSize",
			typeof(GridLength),
			typeof(MediaSlider),
			new FrameworkPropertyMetadata(new GridLength(1, GridUnitType.Star)),
			IsGridLengthValid);

		/// <summary>
		/// Gets or sets the <see cref="GridLength"/> (calculated height)
		/// of the space between the main slider area and the zoom bar.
		/// A <see cref="GridUnitType.Star"/> value is relative to the size of the main slider area.
		/// </summary>
		public GridLength InnerGapSize
		{
			get => (GridLength)GetValue(InnerGapSizeProperty);
			set => SetValue(InnerGapSizeProperty, value);
		}

		public static readonly DependencyProperty InnerGapSizeProperty = DependencyProperty.Register(
			"InnerGapSize",
			typeof(GridLength),
			typeof(MediaSlider),
			new FrameworkPropertyMetadata(new GridLength(1, GridUnitType.Star)),
			IsGridLengthValid);

		/// <summary>
		/// Gets or sets the height of the position element relative to the height of the main slider area.
		/// </summary>
		public double PositionRelativeSize
		{
			get => (double)GetValue(PositionRelativeSizeProperty);
			set => SetValue(PositionRelativeSizeProperty, value);
		}

		public static readonly DependencyProperty PositionRelativeSizeProperty = DependencyProperty.Register(
			"PositionRelativeSize",
			typeof(double),
			typeof(MediaSlider),
			new FrameworkPropertyMetadata(1.0, OnAlignmentAndSizingPropertyChanged));

		/// <summary>
		/// Gets or sets the height of the selection range elements relative to the height of the main slider area.
		/// </summary>
		public double SelectionRangeRelativeSize
		{
			get => (double)GetValue(SelectionRangeRelativeSizeProperty);
			set => SetValue(SelectionRangeRelativeSizeProperty, value);
		}

		public static readonly DependencyProperty SelectionRangeRelativeSizeProperty = DependencyProperty.Register(
			"SelectionRangeRelativeSize",
			typeof(double),
			typeof(MediaSlider),
			new FrameworkPropertyMetadata(1.0, OnAlignmentAndSizingPropertyChanged));

		/// <summary>
		/// Gets or sets the height of the selection range highlight element
		/// relative to the height of the selection range elements.
		/// </summary>
		public double SelectionRangeHighlightRelativeSize
		{
			get => (double)GetValue(SelectionRangeHighlightRelativeSizeProperty);
			set => SetValue(SelectionRangeHighlightRelativeSizeProperty, value);
		}

		public static readonly DependencyProperty SelectionRangeHighlightRelativeSizeProperty = DependencyProperty.Register(
			"SelectionRangeHighlightRelativeSize",
			typeof(double),
			typeof(MediaSlider),
			new FrameworkPropertyMetadata(1.0, OnAlignmentAndSizingPropertyChanged));

		/// <summary>
		/// Gets or sets the height of the origin (<see cref="Position"/>=0) tick mark
		/// relative to the height of the main slider area.
		/// </summary>
		public double OriginTickRelativeSize
		{
			get => (double)GetValue(OriginTickRelativeSizeProperty);
			set => SetValue(OriginTickRelativeSizeProperty, value);
		}

		public static readonly DependencyProperty OriginTickRelativeSizeProperty = DependencyProperty.Register(
			"OriginTickRelativeSize",
			typeof(double),
			typeof(MediaSlider),
			new FrameworkPropertyMetadata(1.0));

		/// <summary>
		/// Gets or sets the height of all major (1 second interval or greater) tick marks
		/// relative to the height of the main slider area.
		/// </summary>
		public double MajorTickRelativeSize
		{
			get => (double)GetValue(MajorTickRelativeSizeProperty);
			set => SetValue(MajorTickRelativeSizeProperty, value);
		}

		public static readonly DependencyProperty MajorTickRelativeSizeProperty = DependencyProperty.Register(
			"MajorTickRelativeSize",
			typeof(double),
			typeof(MediaSlider),
			new FrameworkPropertyMetadata(1.0));

		/// <summary>
		/// Gets or sets the height of all minor (frame/sample) tick marks
		/// relative to the height of the main slider area.
		/// </summary>
		public double MinorTickRelativeSize
		{
			get => (double)GetValue(MinorTickRelativeSizeProperty);
			set => SetValue(MinorTickRelativeSizeProperty, value);
		}

		public static readonly DependencyProperty MinorTickRelativeSizeProperty = DependencyProperty.Register(
			"MinorTickRelativeSize",
			typeof(double),
			typeof(MediaSlider),
			new FrameworkPropertyMetadata(1.0));

		/// <summary>
		/// Gets or sets the thickness of the origin (<see cref="Position"/>=0) tick mark.
		/// </summary>
		public double OriginTickThickness
		{
			get => (double)GetValue(OriginTickThicknessProperty);
			set => SetValue(OriginTickThicknessProperty, value);
		}

		public static readonly DependencyProperty OriginTickThicknessProperty = DependencyProperty.Register(
			"OriginTickThickness",
			typeof(double),
			typeof(MediaSlider),
			new FrameworkPropertyMetadata(1.0));

		/// <summary>
		/// Gets or sets the thickness of all major (1 second interval or greater) tick marks.
		/// </summary>
		public double MajorTickThickness
		{
			get => (double)GetValue(MajorTickThicknessProperty);
			set => SetValue(MajorTickThicknessProperty, value);
		}

		public static readonly DependencyProperty MajorTickThicknessProperty = DependencyProperty.Register(
			"MajorTickThickness",
			typeof(double),
			typeof(MediaSlider),
			new FrameworkPropertyMetadata(1.0));

		/// <summary>
		/// Gets or sets the thickness of all minor (frame/sample) tick marks.
		/// </summary>
		public double MinorTickThickness
		{
			get => (double)GetValue(MinorTickThicknessProperty);
			set => SetValue(MinorTickThicknessProperty, value);
		}

		public static readonly DependencyProperty MinorTickThicknessProperty = DependencyProperty.Register(
			"MinorTickThickness",
			typeof(double),
			typeof(MediaSlider),
			new FrameworkPropertyMetadata(1.0));
		#endregion

		#region Brushes
		/// <summary>
		/// Gets or sets a brush that describes the background of the main slider area.
		/// </summary>
		public Brush TickBarBackground
		{
			get => (Brush)GetValue(TickBarBackgroundProperty);
			set => SetValue(TickBarBackgroundProperty, value);
		}

		public static readonly DependencyProperty TickBarBackgroundProperty = DependencyProperty.Register(
			"TickBarBackground",
			typeof(Brush),
			typeof(MediaSlider),
			new FrameworkPropertyMetadata(null));

		/// <summary>
		/// Gets or sets a brush that describes the background of the zoom area.
		/// </summary>
		public Brush ZoomBarBackground
		{
			get => (Brush)GetValue(ZoomBarBackgroundProperty);
			set => SetValue(ZoomBarBackgroundProperty, value);
		}

		public static readonly DependencyProperty ZoomBarBackgroundProperty = DependencyProperty.Register(
			"ZoomBarBackground",
			typeof(Brush),
			typeof(MediaSlider),
			new FrameworkPropertyMetadata(null));

		/// <summary>
		/// Gets or sets a brush that describes the color of the origin (<see cref="Position"/>=0) tick mark.
		/// </summary>
		public Brush OriginTickBrush
		{
			get => (Brush)GetValue(OriginTickBrushProperty);
			set => SetValue(OriginTickBrushProperty, value);
		}

		public static readonly DependencyProperty OriginTickBrushProperty = DependencyProperty.Register(
			"OriginTickBrush",
			typeof(Brush),
			typeof(MediaSlider),
			new FrameworkPropertyMetadata(null));

		/// <summary>
		/// Gets or sets a brush that describes the color of all major (1 second interval or greater) tick marks.
		/// </summary>
		public Brush MajorTickBrush
		{
			get => (Brush)GetValue(MajorTickBrushProperty);
			set => SetValue(MajorTickBrushProperty, value);
		}

		public static readonly DependencyProperty MajorTickBrushProperty = DependencyProperty.Register(
			"MajorTickBrush",
			typeof(Brush),
			typeof(MediaSlider),
			new FrameworkPropertyMetadata(null));

		/// <summary>
		/// Gets or sets a brush that describes the color of all minor (frame/sample) tick marks.
		/// </summary>
		public Brush MinorTickBrush
		{
			get => (Brush)GetValue(MinorTickBrushProperty);
			set => SetValue(MinorTickBrushProperty, value);
		}

		public static readonly DependencyProperty MinorTickBrushProperty = DependencyProperty.Register(
			"MinorTickBrush",
			typeof(Brush),
			typeof(MediaSlider),
			new FrameworkPropertyMetadata(null));
		#endregion

		#region Templates
		public DataTemplate PositionTemplate
		{
			get => (DataTemplate)GetValue(PositionTemplateProperty);
			set => SetValue(PositionTemplateProperty, value);
		}

		public static readonly DependencyProperty PositionTemplateProperty = DependencyProperty.Register(
			"PositionTemplate",
			typeof(DataTemplate),
			typeof(MediaSlider),
			new FrameworkPropertyMetadata(null));

		public DataTemplate SelectionStartTemplate
		{
			get => (DataTemplate)GetValue(SelectionStartTemplateProperty);
			set => SetValue(SelectionStartTemplateProperty, value);
		}

		public static readonly DependencyProperty SelectionStartTemplateProperty = DependencyProperty.Register(
			"SelectionStartTemplate",
			typeof(DataTemplate),
			typeof(MediaSlider),
			new FrameworkPropertyMetadata(null));

		public DataTemplate SelectionEndTemplate
		{
			get => (DataTemplate)GetValue(SelectionEndTemplateProperty);
			set => SetValue(SelectionEndTemplateProperty, value);
		}

		public static readonly DependencyProperty SelectionEndTemplateProperty = DependencyProperty.Register(
			"SelectionEndTemplate",
			typeof(DataTemplate),
			typeof(MediaSlider),
			new FrameworkPropertyMetadata(null));

		public DataTemplate SelectionRangeTemplate
		{
			get => (DataTemplate)GetValue(SelectionRangeTemplateProperty);
			set => SetValue(SelectionRangeTemplateProperty, value);
		}

		public static readonly DependencyProperty SelectionRangeTemplateProperty = DependencyProperty.Register(
			"SelectionRangeTemplate",
			typeof(DataTemplate),
			typeof(MediaSlider),
			new FrameworkPropertyMetadata(null));

		public DataTemplate VisibleRangeStartTemplate
		{
			get => (DataTemplate)GetValue(VisibleRangeStartTemplateProperty);
			set => SetValue(VisibleRangeStartTemplateProperty, value);
		}

		public static readonly DependencyProperty VisibleRangeStartTemplateProperty = DependencyProperty.Register(
			"VisibleRangeStartTemplate",
			typeof(DataTemplate),
			typeof(MediaSlider),
			new FrameworkPropertyMetadata(null));

		public DataTemplate VisibleRangeEndTemplate
		{
			get => (DataTemplate)GetValue(VisibleRangeEndTemplateProperty);
			set => SetValue(VisibleRangeEndTemplateProperty, value);
		}

		public static readonly DependencyProperty VisibleRangeEndTemplateProperty = DependencyProperty.Register(
			"VisibleRangeEndTemplate",
			typeof(DataTemplate),
			typeof(MediaSlider),
			new FrameworkPropertyMetadata(null));

		public DataTemplate VisibleRangeTemplate
		{
			get => (DataTemplate)GetValue(VisibleRangeTemplateProperty);
			set => SetValue(VisibleRangeTemplateProperty, value);
		}

		public static readonly DependencyProperty VisibleRangeTemplateProperty = DependencyProperty.Register(
			"VisibleRangeTemplate",
			typeof(DataTemplate),
			typeof(MediaSlider),
			new FrameworkPropertyMetadata(null));
		#endregion
		#endregion

		#region Events
		/// <summary>
		/// Occurs when the playhead element receives logical focus and mouse capture.
		/// </summary>
		public event RoutedEventHandler PositionDragStarted
		{
			add => AddHandler(PositionDragStartedEvent, value);
			remove => RemoveHandler(PositionDragStartedEvent, value);
		}

		public static readonly RoutedEvent PositionDragStartedEvent = EventManager.RegisterRoutedEvent(
			"PositionDragStarted",
			RoutingStrategy.Bubble,
			typeof(RoutedEventHandler),
			typeof(MediaSlider));

		/// <summary>
		/// Occurs when the playhead element loses mouse capture.
		/// </summary>
		public event RoutedEventHandler PositionDragCompleted
		{
			add => AddHandler(PositionDragCompletedEvent, value);
			remove => RemoveHandler(PositionDragCompletedEvent, value);
		}

		public static readonly RoutedEvent PositionDragCompletedEvent = EventManager.RegisterRoutedEvent(
			"PositionDragCompleted",
			RoutingStrategy.Bubble,
			typeof(RoutedEventHandler),
			typeof(MediaSlider));

		/// <summary>
		/// Occurs when the <see cref="Position"/> changes.
		/// </summary>
		public event RoutedPropertyChangedEventHandler<decimal> PositionChanged
		{
			add => AddHandler(PositionChangedEvent, value);
			remove => RemoveHandler(PositionChangedEvent, value);
		}

		public static readonly RoutedEvent PositionChangedEvent = EventManager.RegisterRoutedEvent(
			"PositionChanged",
			RoutingStrategy.Bubble,
			typeof(RoutedPropertyChangedEventHandler<decimal>),
			typeof(MediaSlider));

		/// <summary>
		/// Occurs when either of the selection elements receive logical focus and mouse capture.
		/// </summary>
		public event RoutedEventHandler SelectionRangeDragStarted
		{
			add => AddHandler(SelectionRangeDragStartedEvent, value);
			remove => RemoveHandler(SelectionRangeDragStartedEvent, value);
		}

		public static readonly RoutedEvent SelectionRangeDragStartedEvent = EventManager.RegisterRoutedEvent(
			"SelectionRangeDragStarted",
			RoutingStrategy.Bubble,
			typeof(RoutedEventHandler),
			typeof(MediaSlider));

		/// <summary>
		/// Occurs when either of the selection elements lose mouse capture.
		/// </summary>
		public event RoutedEventHandler SelectionRangeDragCompleted
		{
			add => AddHandler(SelectionRangeDragCompletedEvent, value);
			remove => RemoveHandler(SelectionRangeDragCompletedEvent, value);
		}

		public static readonly RoutedEvent SelectionRangeDragCompletedEvent = EventManager.RegisterRoutedEvent(
			"SelectionRangeDragCompleted",
			RoutingStrategy.Bubble,
			typeof(RoutedEventHandler),
			typeof(MediaSlider));

		/// <summary>
		/// Occurs when the selection range changes.
		/// </summary>
		public event RoutedPropertyChangedEventHandler<Range<decimal>?> SelectionRangeChanged
		{
			add => AddHandler(SelectionRangeChangedEvent, value);
			remove => RemoveHandler(SelectionRangeChangedEvent, value);
		}

		public static readonly RoutedEvent SelectionRangeChangedEvent = EventManager.RegisterRoutedEvent(
			"SelectionRangeChanged",
			RoutingStrategy.Bubble,
			typeof(RoutedPropertyChangedEventHandler<Range<decimal>?>),
			typeof(MediaSlider));

		/// <summary>
		/// Occurs when any of the visible range elements receive logical focus and mouse capture.
		/// </summary>
		public event RoutedEventHandler VisibleRangeDragStarted
		{
			add => AddHandler(VisibleRangeDragStartedEvent, value);
			remove => RemoveHandler(VisibleRangeDragStartedEvent, value);
		}

		public static readonly RoutedEvent VisibleRangeDragStartedEvent = EventManager.RegisterRoutedEvent(
			"VisibleRangeDragStarted",
			RoutingStrategy.Bubble,
			typeof(RoutedEventHandler),
			typeof(MediaSlider));

		/// <summary>
		/// Occurs when any of the visible range elements lose mouse capture.
		/// </summary>
		public event RoutedEventHandler VisibleRangeDragCompleted
		{
			add => AddHandler(VisibleRangeDragCompletedEvent, value);
			remove => RemoveHandler(VisibleRangeDragCompletedEvent, value);
		}

		public static readonly RoutedEvent VisibleRangeDragCompletedEvent = EventManager.RegisterRoutedEvent(
			"VisibleRangeDragCompleted",
			RoutingStrategy.Bubble,
			typeof(RoutedEventHandler),
			typeof(MediaSlider));

		/// <summary>
		/// Occurs when the visible range changes.
		/// </summary>
		public event RoutedPropertyChangedEventHandler<Range<decimal>> VisibleRangeChanged
		{
			add => AddHandler(VisibleRangeChangedEvent, value);
			remove => RemoveHandler(VisibleRangeChangedEvent, value);
		}

		public static readonly RoutedEvent VisibleRangeChangedEvent = EventManager.RegisterRoutedEvent(
			"VisibleRangeChanged",
			RoutingStrategy.Bubble,
			typeof(RoutedPropertyChangedEventHandler<Range<decimal>>),
			typeof(MediaSlider));
		#endregion

		#region Constructors
		static MediaSlider()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(MediaSlider), new FrameworkPropertyMetadata(typeof(MediaSlider)));
		}

		public MediaSlider()
		{
			_snapDistance          = (0, 0);
			_prevMouseCoord        = 0;
			_isMouseLeftButtonDown = false;
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Automatically adjusts various padding values to provide empty space needed to accomodate
		/// the playhead, selection range elements, and visible range elements
		/// when they are at their respective minimum and maximum positions.
		/// This method is called automatically by the control when needed,
		/// but has been made publicly accessible for situations when readjustment is necessary.
		/// TODO: This functionality should be handled by the layout system
		/// </summary>
		public void AdjustPaddingToFit()
		{
			var mainLeftSpaceNeeded  = Math.Max(_positionElement.ActualWidth / 2, _selStartElement.ActualWidth) - Padding.Left;
			var mainRightSpaceNeeded = Math.Max(_positionElement.ActualWidth / 2, _selEndElement.ActualWidth) - Padding.Right;
			var zoomLeftSpaceNeeded  = _visRngStartElement.ActualWidth - Padding.Left;
			var zoomRightSpaceNeeded = _visRngEndElement.ActualWidth - Padding.Right;

			var newMargin = new Thickness(0, 0, 0, 0);
			if (mainLeftSpaceNeeded > 0)
				newMargin.Left = mainLeftSpaceNeeded;
			if (mainRightSpaceNeeded > 0)
				newMargin.Right = mainRightSpaceNeeded;
			_mainPanel.Margin = newMargin;

			newMargin = new Thickness(0, 0, 0, 0);
			if (zoomLeftSpaceNeeded > 0)
				newMargin.Left = zoomLeftSpaceNeeded;
			if (zoomRightSpaceNeeded > 0)
				newMargin.Right = zoomRightSpaceNeeded;
			_zoomPanel.Margin = newMargin;
		}

		/// <summary>
		/// Moves the current visible range to the current playhead <see cref="Position"/>.
		/// </summary>
		/// <param name="isCentered">
		/// If <code>true</code>, the visible range will be centered around the playhead.
		/// If <code>false</code>, the visible range will start at the playhead.
		/// </param>
		public void MoveToPlayhead(bool isCentered)
		{
			var delta = Position - VisibleRangeStart;
			var range = VisibleRange.Magnitude();
			if (isCentered)
				delta -= range / 2;

			_isVisRngDragging = true;
			if (delta < 0 && VisibleRangeStart > Minimum)
			{
				_isVisRngChanging = true;
				VisibleRangeStart = Math.Max(VisibleRangeStart + delta, Minimum);
				VisibleRangeEnd   = VisibleRangeStart + range;
			}
			else if (delta > 0 && VisibleRangeEnd < Maximum)
			{
				_isVisRngChanging = true;
				VisibleRangeEnd   = Math.Min(VisibleRangeEnd + delta, Maximum);
				VisibleRangeStart = VisibleRangeEnd - range;
			}

			_isVisRngDragging = false;
		}

		/// <summary>
		/// Moves the playhead (<see cref="Position"/>) to the beginning or end of the current selection.
		/// If no selection is defined, this method does nothing.
		/// </summary>
		/// <param name="moveToEnd">
		/// If <code>true</code>, moves the playhead to the end of the selection.
		/// If <code>false</code>, moves the playhead to the beginning of the selection.
		/// </param>
		public void MovePlayheadToSelection(bool moveToEnd)
		{
			if (moveToEnd && SelectionEnd.HasValue)
				Position = SelectionEnd.Value;
			else if (!moveToEnd && SelectionStart.HasValue)
				Position = SelectionStart.Value;
		}
		#endregion

		#region Property Changed Callbacks, Coerce Value Callbacks, Validate Value Callbacks
		private static void OnRangePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (!(d is MediaSlider mediaSlider)) return;

			if (e.Property == MinimumProperty)
			{
				mediaSlider.CoerceValue(MaximumProperty);
				mediaSlider.CoerceValue(PositionProperty);
				mediaSlider.CoerceValue(SelectionStartProperty);
				mediaSlider.CoerceValue(SelectionEndProperty);
				mediaSlider.CoerceValue(VisibleRangeStartProperty);
				mediaSlider.CoerceValue(VisibleRangeEndProperty);
				mediaSlider.UpdatePositionElement();
				mediaSlider.UpdateSelectionRangeElements();
				mediaSlider.UpdateVisibleRangeElements();
			}
			else if (e.Property == MaximumProperty)
			{
				mediaSlider.CoerceValue(PositionProperty);
				mediaSlider.CoerceValue(SelectionStartProperty);
				mediaSlider.CoerceValue(SelectionEndProperty);
				mediaSlider.CoerceValue(VisibleRangeStartProperty);
				mediaSlider.CoerceValue(VisibleRangeEndProperty);
				mediaSlider.UpdatePositionElement();
				mediaSlider.UpdateSelectionRangeElements();
				mediaSlider.UpdateVisibleRangeElements();
			}
			else if (e.Property == PositionProperty)
			{
				mediaSlider.UpdatePositionElement();
				mediaSlider.RaiseEvent(
					new RoutedPropertyChangedEventArgs<decimal>((decimal)e.OldValue, (decimal)e.NewValue, PositionChangedEvent));
			}
			else if (e.Property == SelectionStartProperty)
			{
				Range<decimal>? newRange = null;

				if (e.NewValue is decimal newStart && mediaSlider.SelectionEnd != null)
				{
					if (newStart > mediaSlider.SelectionEnd)
					{
						mediaSlider._isSelRngChanging = true;
						mediaSlider.SelectionEnd      = newStart;
					}

					newRange = new Range<decimal>(newStart, (decimal)mediaSlider.SelectionEnd);
				}
				else if (e.NewValue == null)
				{
					mediaSlider._isSelRngChanging = true;
					mediaSlider.SelectionEnd      = null;
				}

				if (mediaSlider._isSelRngChanging)
				{
					mediaSlider._isSelRngChanging = false;
					return;
				}

				mediaSlider.UpdateSelectionRangeElements();
				mediaSlider.RaiseEvent(
					new RoutedPropertyChangedEventArgs<Range<decimal>?>(
						mediaSlider._selectionRangeOld,
						newRange,
						SelectionRangeChangedEvent));
				mediaSlider._selectionRangeOld = newRange;
			}
			else if (e.Property == SelectionEndProperty)
			{
				Range<decimal>? newRange = null;

				if (e.NewValue is decimal newEnd && mediaSlider.SelectionStart != null)
				{
					if (newEnd < mediaSlider.SelectionStart)
					{
						mediaSlider._isSelRngChanging = true;
						mediaSlider.SelectionStart    = newEnd;
					}

					newRange = new Range<decimal>((decimal)mediaSlider.SelectionStart, newEnd);
				}
				else if (e.NewValue == null)
				{
					mediaSlider._isSelRngChanging = true;
					mediaSlider.SelectionStart    = null;
				}

				if (mediaSlider._isSelRngChanging)
				{
					mediaSlider._isSelRngChanging = false;
					return;
				}

				mediaSlider.UpdateSelectionRangeElements();
				mediaSlider.RaiseEvent(
					new RoutedPropertyChangedEventArgs<Range<decimal>?>(
						mediaSlider._selectionRangeOld,
						newRange,
						SelectionRangeChangedEvent));
				mediaSlider._selectionRangeOld = newRange;
			}
			else if (e.Property == VisibleRangeStartProperty)
			{
				if (mediaSlider._isVisRngChanging)
				{
					mediaSlider._isVisRngChanging = false;
					return;
				}

				var newRange = new Range<decimal>((decimal)e.NewValue, mediaSlider.VisibleRangeEnd);
				mediaSlider.UpdatePositionElement();
				mediaSlider.UpdateSelectionRangeElements();
				mediaSlider.UpdateVisibleRangeElements();
				if (!mediaSlider._isVisRngDragging)
					mediaSlider.AdjustTickDensity(mediaSlider._visibleRangeOld.Magnitude().CompareTo(newRange.Magnitude()));
				mediaSlider.RaiseEvent(
					new RoutedPropertyChangedEventArgs<Range<decimal>>(
						mediaSlider._visibleRangeOld,
						newRange,
						VisibleRangeChangedEvent));
				mediaSlider._visibleRangeOld = newRange;
			}
			else if (e.Property == VisibleRangeEndProperty)
			{
				if (mediaSlider._isVisRngChanging)
				{
					mediaSlider._isVisRngChanging = false;
					return;
				}

				var newRange = new Range<decimal>(mediaSlider.VisibleRangeStart, (decimal)e.NewValue);
				mediaSlider.UpdatePositionElement();
				mediaSlider.UpdateSelectionRangeElements();
				mediaSlider.UpdateVisibleRangeElements();
				if (!mediaSlider._isVisRngDragging)
					mediaSlider.AdjustTickDensity(mediaSlider._visibleRangeOld.Magnitude().CompareTo(newRange.Magnitude()));
				mediaSlider.RaiseEvent(
					new RoutedPropertyChangedEventArgs<Range<decimal>>(
						mediaSlider._visibleRangeOld,
						newRange,
						VisibleRangeChangedEvent));
				mediaSlider._visibleRangeOld = newRange;
			}
		}

		private static void OnAlignmentAndSizingPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (!(d is MediaSlider mediaSlider) || mediaSlider._mainPanel == null) return;

			if (e.Property == PositionRelativeSizeProperty || e.Property == PositionAlignmentProperty)
			{
				mediaSlider.ArrangePositionElement();
			}
			else if (e.Property == SelectionRangeRelativeSizeProperty ||
				e.Property == SelectionRangeHighlightRelativeSizeProperty || e.Property == SelectionRangeAlignmentProperty ||
				e.Property == SelectionRangeHighlightAlignmentProperty)
			{
				mediaSlider.ArrangeSelectionElements();
			}
		}

		private static void OnMediaPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (!(d is MediaSlider mediaSlider)) return;

			if (e.Property == FramesPerSecondProperty)
			{
				mediaSlider.AdjustTickDensity(0, true);
			}
		}

		private static object CoerceMaximum(DependencyObject d, object value)
		{
			var mediaSlider = (MediaSlider)d;
			return (decimal)value < mediaSlider.Minimum ? mediaSlider.Minimum : value;
		}

		private static object CoercePosition(DependencyObject d, object value)
		{
			var mediaSlider = (MediaSlider)d;
			var position    = (decimal)value;

			if (position < mediaSlider.Minimum)
				return mediaSlider.Minimum;
			if (position > mediaSlider.Maximum)
				return mediaSlider.Maximum;
			return position;
		}

		private static object CoerceSelectionStart(DependencyObject d, object value)
		{
			if (value == null)
				return null;

			var mediaSlider    = (MediaSlider)d;
			var selectionStart = (decimal)value;

			if (selectionStart < mediaSlider.Minimum)
				return mediaSlider.Minimum;
			if (selectionStart > mediaSlider.Maximum)
				return mediaSlider.Maximum;
			return selectionStart;
		}

		private static object CoerceSelectionEnd(DependencyObject d, object value)
		{
			if (value == null)
				return null;

			var mediaSlider  = (MediaSlider)d;
			var selectionEnd = (decimal)value;

			if (selectionEnd < mediaSlider.Minimum)
				return mediaSlider.Minimum;
			if (selectionEnd > mediaSlider.Maximum)
				return mediaSlider.Maximum;
			return selectionEnd;
		}

		private static object CoerceVisibleRangeStart(DependencyObject d, object value)
		{
			var mediaSlider       = (MediaSlider)d;
			var visibleRangeStart = (decimal)value;

			if (visibleRangeStart < mediaSlider.Minimum)
				return mediaSlider.Minimum;

			var offset = mediaSlider.VisibleRangeEnd - Math.Min(mediaSlider.Maximum - mediaSlider.Minimum, MinVisibleRange);
			if (visibleRangeStart > offset)
				return offset > mediaSlider.Minimum ? offset : mediaSlider.Minimum;

			return visibleRangeStart;
		}

		private static object CoerceVisibleRangeEnd(DependencyObject d, object value)
		{
			var mediaSlider     = (MediaSlider)d;
			var visibleRangeEnd = (decimal)value;

			if (visibleRangeEnd > mediaSlider.Maximum)
				return mediaSlider.Maximum;

			var offset = mediaSlider.VisibleRangeStart + Math.Min(mediaSlider.Maximum - mediaSlider.Minimum, MinVisibleRange);
			if (visibleRangeEnd < offset)
				return offset < mediaSlider.Maximum ? offset : mediaSlider.Maximum;

			return visibleRangeEnd;
		}

		/// <summary>
		/// Checks that a GridLength value is explicitly set as either a pixel value or relative size
		/// </summary>
		/// <param name="value">The GridLength to be validated</param>
		/// <returns>True if valid, false if invalid</returns>
		private static bool IsGridLengthValid(object value)
		{
			return value is GridLength gridLength && !gridLength.IsAuto;
		}
		#endregion

		#region Layout and Render Overrides
		/// <inheritdoc cref="FrameworkElement.OnApplyTemplate"/>
		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			if (GetTemplateChild("PART_TickBar") is TickBarAdvanced tickBar)
			{
				_tickBar = tickBar;
				if (VisualTreeHelper.GetParent(tickBar) is Panel parent)
				{
					_mainPanel                     =  parent;
					_mainPanel.SizeChanged         += MediaSlider_MainPanel_SizeChanged;
					_mainPanel.MouseLeftButtonDown += MediaSlider_MainPanel_MouseLeftButtonDown;
				}
			}

			if (GetTemplateChild("PART_Position") is ContentPresenter position)
			{
				_positionElement                     =  position;
				_positionElement.MouseEnter          += Position_MouseEnter;
				_positionElement.MouseLeave          += Position_MouseLeave;
				_positionElement.MouseLeftButtonDown += Position_MouseLeftButtonDown;
				_positionElement.MouseLeftButtonUp   += Position_MouseLeftButtonUp;
				_positionElement.MouseMove           += Position_MouseMove;
			}

			if (GetTemplateChild("PART_SelectionStart") is ContentPresenter selectionStart)
			{
				_selStartElement                     =  selectionStart;
				_selStartElement.MouseEnter          += SelectionStart_MouseEnter;
				_selStartElement.MouseLeave          += SelectionStart_MouseLeave;
				_selStartElement.MouseLeftButtonDown += SelectionStart_MouseLeftButtonDown;
				_selStartElement.MouseLeftButtonUp   += SelectionStart_MouseLeftButtonUp;
				_selStartElement.MouseMove           += SelectionStart_MouseMove;
			}

			if (GetTemplateChild("PART_SelectionEnd") is ContentPresenter selectionEnd)
			{
				_selEndElement                     =  selectionEnd;
				_selEndElement.MouseEnter          += SelectionEnd_MouseEnter;
				_selEndElement.MouseLeave          += SelectionEnd_MouseLeave;
				_selEndElement.MouseLeftButtonDown += SelectionEnd_MouseLeftButtonDown;
				_selEndElement.MouseLeftButtonUp   += SelectionEnd_MouseLeftButtonUp;
				_selEndElement.MouseMove           += SelectionEnd_MouseMove;
			}

			if (GetTemplateChild("PART_SelectionRange") is ContentPresenter selectionRange)
			{
				_selRngElement = selectionRange;
			}

			if (GetTemplateChild("PART_VisibleRangeStart") is ContentPresenter visibleRangeStart)
			{
				_visRngStartElement                     =  visibleRangeStart;
				_visRngStartElement.MouseEnter          += VisibleRangeStart_MouseEnter;
				_visRngStartElement.MouseLeave          += VisibleRangeStart_MouseLeave;
				_visRngStartElement.MouseLeftButtonDown += VisibleRangeStart_MouseLeftButtonDown;
				_visRngStartElement.MouseLeftButtonUp   += VisibleRangeStart_MouseLeftButtonUp;
				_visRngStartElement.MouseMove           += VisibleRangeStart_MouseMove;
			}

			if (GetTemplateChild("PART_VisibleRangeEnd") is ContentPresenter visibleRangeEnd)
			{
				_visRngEndElement                     =  visibleRangeEnd;
				_visRngEndElement.MouseEnter          += VisibleRangeEnd_MouseEnter;
				_visRngEndElement.MouseLeave          += VisibleRangeEnd_MouseLeave;
				_visRngEndElement.MouseLeftButtonDown += VisibleRangeEnd_MouseLeftButtonDown;
				_visRngEndElement.MouseLeftButtonUp   += VisibleRangeEnd_MouseLeftButtonUp;
				_visRngEndElement.MouseMove           += VisibleRangeEnd_MouseMove;
			}

			if (GetTemplateChild("PART_VisibleRange") is ContentPresenter visibleRange)
			{
				_visRngElement = visibleRange;
				if (VisualTreeHelper.GetParent(visibleRange) is Panel parent)
				{
					_zoomPanel             =  parent;
					_zoomPanel.SizeChanged += MediaSlider_ZoomPanel_SizeChanged;
				}

				_visRngElement.MouseEnter          += VisibleRange_MouseEnter;
				_visRngElement.MouseLeave          += VisibleRange_MouseLeave;
				_visRngElement.MouseLeftButtonDown += VisibleRange_MouseLeftButtonDown;
				_visRngElement.MouseLeftButtonUp   += VisibleRange_MouseLeftButtonUp;
				_visRngElement.MouseMove           += VisibleRange_MouseMove;
			}

			// TODO: Investigate different ways to achieve the effect of the binding below. SetCurrentValue() perhaps?
			/*var binding = new Binding { Path = new PropertyPath("Minimum"), Source = this };
			SetBinding(VisibleRangeStartProperty, binding);
			binding = new Binding { Path = new PropertyPath("Maximum"), Source = this };
			SetBinding(VisibleRangeEndProperty, binding);*/

			Loaded += MediaSlider_Loaded;
		}
		#endregion

		#region Private Methods
		private void UpdatePositionElement()
		{
			if (_mainPanel == null || _positionElement == null || Math.Abs(_mainPanel.ActualWidth) < double.Epsilon)
				return;

			// Hide the playhead if the current position is not within the current visible range
			if (Position < VisibleRangeStart || Position > VisibleRangeEnd || VisibleRangeStart == VisibleRangeEnd)
			{
				_positionElement.Visibility = Visibility.Collapsed;
				return;
			}

			// Move the playhead to the current position
			_positionElement.Visibility = Visibility.Visible;
			Canvas.SetLeft(
				_positionElement,
				decimal.ToDouble(
					(Position - VisibleRangeStart) * ((decimal)_mainPanel.ActualWidth / (VisibleRangeEnd - VisibleRangeStart)) -
					(decimal)_positionElement.ActualWidth / 2));
		}

		private void UpdateSelectionRangeElements()
		{
			if (_mainPanel == null || _selStartElement == null || _selEndElement == null || _selRngElement == null)
				return;

			// Hide the selection range controls if the selection range feature is disabled
			if (SelectionStart == null || SelectionEnd == null)
			{
				_selStartElement.Visibility = Visibility.Collapsed;
				_selEndElement.Visibility   = Visibility.Collapsed;
				_selRngElement.Visibility   = Visibility.Collapsed;
				return;
			}

			// Move the selection range start control if it is enabled and within the current visible range
			if (SelectionStart >= VisibleRangeStart && SelectionStart <= VisibleRangeEnd && VisibleRangeStart != VisibleRangeEnd)
			{
				_selStartElement.Visibility = Visibility.Visible;
				Canvas.SetLeft(
					_selStartElement,
					decimal.ToDouble(
						((decimal)SelectionStart - VisibleRangeStart) *
						((decimal)_mainPanel.ActualWidth / (VisibleRangeEnd - VisibleRangeStart)) -
						(decimal)_selStartElement.ActualWidth));
			}
			else
			{
				_selStartElement.Visibility = Visibility.Collapsed;
			}

			// Move the selection range end control if it is enabled and within the current visible range
			if (SelectionEnd >= VisibleRangeStart && SelectionEnd <= VisibleRangeEnd && VisibleRangeStart != VisibleRangeEnd)
			{
				_selEndElement.Visibility = Visibility.Visible;
				Canvas.SetLeft(
					_selEndElement,
					decimal.ToDouble(
						((decimal)SelectionEnd - VisibleRangeStart) *
						((decimal)_mainPanel.ActualWidth / (VisibleRangeEnd - VisibleRangeStart))));
			}
			else
			{
				_selEndElement.Visibility = Visibility.Collapsed;
			}

			// Move the selection range highlight if selection range is enabled and within the current visible range
			if (SelectionEnd - SelectionStart != 0 && SelectionStart < VisibleRangeEnd && SelectionEnd > VisibleRangeStart)
			{
				if (VisibleRangeStart != VisibleRangeEnd)
				{
					_selRngElement.Visibility = Visibility.Visible;
					var adjustedSelectionStart = SelectionStart >= VisibleRangeStart ? (decimal)SelectionStart : VisibleRangeStart;
					var adjustedSelectionEnd   = SelectionEnd <= VisibleRangeEnd ? (decimal)SelectionEnd : VisibleRangeEnd;

					Canvas.SetLeft(
						_selRngElement,
						decimal.ToDouble(
							(adjustedSelectionStart - VisibleRangeStart) *
							((decimal)_mainPanel.ActualWidth / (VisibleRangeEnd - VisibleRangeStart))));
					_selRngElement.Width = decimal.ToDouble(
						Math.Abs(adjustedSelectionEnd - adjustedSelectionStart) * (decimal)_mainPanel.ActualWidth /
						(VisibleRangeEnd - VisibleRangeStart));
				}
				else
				{
					Canvas.SetLeft(_selRngElement, 0);
					_selRngElement.Width = _mainPanel.ActualWidth;
				}
			}
			else
			{
				_selRngElement.Visibility = Visibility.Collapsed;
			}
		}

		private void UpdateVisibleRangeElements()
		{
			if (_zoomPanel == null || _visRngStartElement == null || _visRngEndElement == null || _visRngElement == null)
				return;

			// Move the visible range start control
			Canvas.SetLeft(
				_visRngStartElement,
				decimal.ToDouble(
					(VisibleRangeStart - Minimum) * ((decimal)_zoomPanel.ActualWidth / (Maximum - Minimum)) -
					(decimal)_visRngStartElement.ActualWidth));

			// Move the visible range end control
			Canvas.SetLeft(
				_visRngEndElement,
				decimal.ToDouble((VisibleRangeEnd - Minimum) * ((decimal)_zoomPanel.ActualWidth / (Maximum - Minimum))));

			// Move the visible range highlight (collapsed if visible range = 0)
			if (VisibleRangeEnd - VisibleRangeStart != 0)
			{
				_visRngElement.Visibility = Visibility.Visible;
				Canvas.SetLeft(
					_visRngElement,
					decimal.ToDouble((VisibleRangeStart - Minimum) * ((decimal)_zoomPanel.ActualWidth / (Maximum - Minimum))));
				_visRngElement.Width = decimal.ToDouble(
					Math.Abs(VisibleRangeEnd - VisibleRangeStart) * (decimal)_zoomPanel.ActualWidth / (Maximum - Minimum));
			}
			else
			{
				_visRngElement.Visibility = Visibility.Collapsed;
			}
		}

		private void ArrangePositionElement()
		{
			var newHeight = PositionRelativeSize * _mainPanel.ActualHeight;
			_positionElement.Height = newHeight;

			switch (PositionAlignment)
			{
				case JLR.Utility.NET.Position.Top:
				case JLR.Utility.NET.Position.Left:
					Canvas.SetTop(_positionElement, 0);
					break;
				case JLR.Utility.NET.Position.Middle:
				case JLR.Utility.NET.Position.Center:
					Canvas.SetTop(_positionElement, (_mainPanel.ActualHeight - newHeight) / 2);
					break;
				case JLR.Utility.NET.Position.Bottom:
				case JLR.Utility.NET.Position.Right:
					Canvas.SetTop(_positionElement, _mainPanel.ActualHeight - newHeight);
					break;
			}
		}

		private void ArrangeSelectionElements()
		{
			var newRangeHeight     = SelectionRangeRelativeSize * _mainPanel.ActualHeight;
			var newHighlightHeight = SelectionRangeHighlightRelativeSize * newRangeHeight;
			_selStartElement.Height = newRangeHeight;
			_selEndElement.Height   = newRangeHeight;
			_selRngElement.Height   = newHighlightHeight;

			switch (SelectionRangeAlignment)
			{
				case JLR.Utility.NET.Position.Top:
				case JLR.Utility.NET.Position.Left:
					Canvas.SetTop(_selStartElement, 0);
					Canvas.SetTop(_selEndElement,   0);
					break;
				case JLR.Utility.NET.Position.Middle:
				case JLR.Utility.NET.Position.Center:
					Canvas.SetTop(_selStartElement, (_mainPanel.ActualHeight - newRangeHeight) / 2);
					Canvas.SetTop(_selEndElement,   (_mainPanel.ActualHeight - newRangeHeight) / 2);
					Canvas.SetTop(_selRngElement,   (_mainPanel.ActualHeight - newHighlightHeight) / 2);
					break;
				case JLR.Utility.NET.Position.Bottom:
				case JLR.Utility.NET.Position.Right:
					Canvas.SetTop(_selStartElement, _mainPanel.ActualHeight - newRangeHeight);
					Canvas.SetTop(_selEndElement,   _mainPanel.ActualHeight - newRangeHeight);
					Canvas.SetTop(_selRngElement,   _mainPanel.ActualHeight - newHighlightHeight);
					break;
				default:
					break;
			}

			switch (SelectionRangeHighlightAlignment)
			{
				case JLR.Utility.NET.Position.Top:
				case JLR.Utility.NET.Position.Left:
					Canvas.SetTop(_selRngElement, Canvas.GetTop(_selStartElement));
					break;
				case JLR.Utility.NET.Position.Middle:
				case JLR.Utility.NET.Position.Center:
					Canvas.SetTop(_selRngElement, Canvas.GetTop(_selStartElement) + (newRangeHeight - newHighlightHeight) / 2);
					break;
				case JLR.Utility.NET.Position.Bottom:
				case JLR.Utility.NET.Position.Right:
					Canvas.SetTop(_selRngElement, Canvas.GetTop(_selStartElement) + (newRangeHeight - newHighlightHeight));
					break;
			}
		}

		private void ArrangeVisibleRangeElements()
		{
			_visRngStartElement.Height = _zoomPanel.ActualHeight;
			_visRngEndElement.Height   = _zoomPanel.ActualHeight;
			_visRngElement.Height      = _zoomPanel.ActualHeight;
			Canvas.SetTop(_visRngStartElement, 0);
			Canvas.SetTop(_visRngEndElement,   0);
			Canvas.SetTop(_visRngElement,      0);
		}

		/// <summary>
		/// Adjusts the temporal and visual snap distance based on the visual range of the slider,
		/// amount of drawing space currently available, and the tick density threshold.
		/// This method directly modifies the major and minor tick frequencies,
		/// as well as the _snapDistance tuple.
		/// </summary>
		/// <param name="zoomChange">
		/// Specifies whether or not a change has occurred that has the effect of zooming in or out.
		/// A positive value indicates zooming in (more space between tick marks),
		/// a negative value indicates zooming out (less space between tick marks),
		/// and zero indicates no change in zoom (no change to the space between tick marks).
		/// </param>
		/// <param name="initialize">
		/// If true, the list of FPS divisors will be (re)initialized and the tick density will be reevaluated.
		/// </param>
		/// <remarks>
		/// This method does not use <see cref="TickBarAdvanced.TickDensity"/> in its calculation
		/// since in the case of <see cref="MediaSlider"/>, the space between individual major and minor
		/// tick marks is known. This is a much simpler (and less expensive) calculation.
		/// </remarks>
		private void AdjustTickDensity(int zoomChange, bool initialize = false)
		{
			if (_mainPanel == null || _tickBar == null || Math.Abs(_mainPanel.ActualWidth) < double.Epsilon)
			{
				_snapDistance = (0, 0);
				return;
			}

			if (_fpsDivisors == null)
				initialize = true;

			if (initialize)
			{
				_fpsDivisors = new LinkedList<int>();
				foreach (var divisor in MathHelper.Divisors((ulong)FramesPerSecond, true))
				{
					_fpsDivisors.AddLast((int)divisor);
				}

				_currentFpsDivisor     = _fpsDivisors.First;
				_snapDistance.temporal = (decimal)_currentFpsDivisor.Value / FramesPerSecond;
			}

			UpdateVisualSnapDistance();

			if (zoomChange < 0 || initialize)
			{
				while (TestTickDensity() < 0)
				{
					if (_currentFpsDivisor.Next != null)
					{
						_currentFpsDivisor     = _currentFpsDivisor.Next;
						_snapDistance.temporal = (decimal)_currentFpsDivisor.Value / FramesPerSecond;
						UpdateVisualSnapDistance();
					}
					else if (_snapDistance.temporal >= 1)
					{
						_snapDistance.temporal += 1;
						UpdateVisualSnapDistance();
					}
				}
			}
			else if (zoomChange > 0)
			{
				while (TestTickDensity() > TickDensityThreshold && _currentFpsDivisor != _fpsDivisors.First)
				{
					if (_snapDistance.temporal > 1)
					{
						_snapDistance.temporal -= 1;
						UpdateVisualSnapDistance();
					}
					else if (_currentFpsDivisor.Previous != null)
					{
						_currentFpsDivisor     = _currentFpsDivisor.Previous;
						_snapDistance.temporal = (decimal)_currentFpsDivisor.Value / FramesPerSecond;
						UpdateVisualSnapDistance();
					}
				}
			}
			else return;

			if (_snapDistance.temporal < 1)
			{
				_tickBar.MinorTickFrequency = _snapDistance.temporal;
				_tickBar.MajorTickFrequency = 1;
			}
			else
			{
				_tickBar.MinorTickFrequency = 0;
				_tickBar.MajorTickFrequency = _snapDistance.temporal;
			}

			#region Local Methods
			void UpdateVisualSnapDistance()
			{
				_snapDistance.visual = decimal.ToDouble(
					_snapDistance.temporal * (decimal)_mainPanel.ActualWidth / (VisibleRangeEnd - VisibleRangeStart));
			}

			double TestTickDensity()
			{
				var thickness = _snapDistance.temporal < 1 ? _tickBar.MinorTickThickness : _tickBar.MajorTickThickness;
				return _snapDistance.visual - (thickness * TickDensityThreshold);
			}
			#endregion
		}
		#endregion

		#region Event Handlers
		private void MediaSlider_Loaded(object sender, RoutedEventArgs e)
		{
			AdjustPaddingToFit();
			AdjustTickDensity(0, true);
		}

		private void MediaSlider_MainPanel_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			UpdatePositionElement();
			ArrangePositionElement();
			UpdateSelectionRangeElements();
			ArrangeSelectionElements();
			AdjustTickDensity(e.NewSize.Width.CompareTo(e.PreviousSize.Width));
		}

		private void MediaSlider_ZoomPanel_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			UpdateVisibleRangeElements();
			ArrangeVisibleRangeElements();
		}

		private void MediaSlider_MainPanel_MouseLeftButtonDown(object sender, MouseEventArgs e)
		{
			if (e.Source.Equals(_selStartElement) || e.Source.Equals(_selEndElement) || e.Source.Equals(_positionElement))
				return;

			var pos = e.GetPosition(_mainPanel);
			var closest = (from tick in _tickBar.MajorTickPositions.Concat(_tickBar.MinorTickPositions)
						   orderby Math.Abs(tick.position - pos.X)
						   select tick).First();
			RaiseEvent(new RoutedEventArgs(PositionDragStartedEvent, this));
			Position = closest.value;
			RaiseEvent(new RoutedEventArgs(PositionDragCompletedEvent, this));
		}

		private void Position_MouseEnter(object sender, MouseEventArgs e)
		{
			if (_isMouseLeftButtonDown) return;

			e.MouseDevice.OverrideCursor = Cursors.Hand;
			if (_positionElement.ContentTemplate.FindName("shape", _positionElement) is Shape position)
			{
				VisualStateManager.GoToElementState(position, "MouseOver", false);
			}
		}

		private void Position_MouseLeave(object sender, MouseEventArgs e)
		{
			e.MouseDevice.OverrideCursor = Cursors.Arrow;
			if (_positionElement.ContentTemplate.FindName("shape", _positionElement) is Shape position &&
				!_positionElement.IsMouseCaptureWithin)
			{
				VisualStateManager.GoToElementState(position, "Normal", false);
			}
		}

		private void Position_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (_isMouseLeftButtonDown) return;

			if (_positionElement.ContentTemplate.FindName("shape", _positionElement) is Shape position)
			{
				VisualStateManager.GoToElementState(position, "MouseLeftButtonDown", false);
			}

			_positionElement.CaptureMouse();
			_prevMouseCoord        = e.GetPosition(_positionElement).X;
			_isMouseLeftButtonDown = true;
			RaiseEvent(new RoutedEventArgs(PositionDragStartedEvent, this));
		}

		private void Position_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			if (_positionElement.ContentTemplate.FindName("shape", _positionElement) is Shape position)
			{
				VisualStateManager.GoToElementState(position, _positionElement.IsMouseOver ? "MouseOver" : "Normal", false);
			}

			_positionElement.ReleaseMouseCapture();
			_isMouseLeftButtonDown = false;
			RaiseEvent(new RoutedEventArgs(PositionDragCompletedEvent, this));
		}

		private void Position_MouseMove(object sender, MouseEventArgs e)
		{
			if (!_isMouseLeftButtonDown) return;

			// Get mouse position, but only continue if it has moved more than half of the visual snap distance
			var pos  = e.GetPosition(_positionElement).X - _prevMouseCoord;
			var dist = Math.Abs(pos);
			if (dist < _snapDistance.visual / 2)
				return;

			if (pos < 0)
			{
				if (Position - _snapDistance.temporal > VisibleRangeStart)
					Position -= _snapDistance.temporal * (int)(dist / _snapDistance.visual);
				else
					Position = VisibleRangeStart;
			}
			else if (pos > 0)
			{
				if (Position + _snapDistance.temporal < VisibleRangeEnd)
					Position += _snapDistance.temporal * (int)(dist / _snapDistance.visual);
				else
					Position = VisibleRangeEnd;
			}
		}

		private void SelectionStart_MouseEnter(object sender, MouseEventArgs e)
		{
			if (_isMouseLeftButtonDown) return;

			e.MouseDevice.OverrideCursor = Cursors.SizeWE;
			if (_selStartElement.ContentTemplate.FindName("shape", _selStartElement) is Shape selectionStart)
			{
				VisualStateManager.GoToElementState(selectionStart, "MouseOver", false);
			}
		}

		private void SelectionStart_MouseLeave(object sender, MouseEventArgs e)
		{
			e.MouseDevice.OverrideCursor = Cursors.Arrow;
			if (_selStartElement.ContentTemplate.FindName("shape", _selStartElement) is Shape selectionStart &&
				!_selStartElement.IsMouseCaptureWithin)
			{
				VisualStateManager.GoToElementState(selectionStart, "Normal", false);
			}
		}

		private void SelectionStart_MouseLeftButtonDown(object sender, MouseEventArgs e)
		{
			if (_isMouseLeftButtonDown) return;

			if (_selStartElement.ContentTemplate.FindName("shape", _selStartElement) is Shape selectionStart)
			{
				VisualStateManager.GoToElementState(selectionStart, "MouseLeftButtonDown", false);
			}

			_selStartElement.CaptureMouse();
			_prevMouseCoord        = e.GetPosition(_selStartElement).X;
			_isMouseLeftButtonDown = true;
			RaiseEvent(new RoutedEventArgs(SelectionRangeDragStartedEvent, this));
		}

		private void SelectionStart_MouseLeftButtonUp(object sender, MouseEventArgs e)
		{
			if (_selStartElement.ContentTemplate.FindName("shape", _selStartElement) is Shape selectionStart)
			{
				VisualStateManager.GoToElementState(selectionStart, _selStartElement.IsMouseOver ? "MouseOver" : "Normal", false);
			}

			_selStartElement.ReleaseMouseCapture();
			_isMouseLeftButtonDown = false;
			RaiseEvent(new RoutedEventArgs(SelectionRangeDragCompletedEvent, this));
		}

		private void SelectionStart_MouseMove(object sender, MouseEventArgs e)
		{
			if (!_isMouseLeftButtonDown) return;

			// Get mouse position, but only continue if it has moved more than half of the visual snap distance
			var pos  = e.GetPosition(_selStartElement).X - _prevMouseCoord;
			var dist = Math.Abs(pos);
			if (dist < _snapDistance.visual / 2)
				return;

			if (pos < 0)
			{
				if (SelectionStart - _snapDistance.temporal > VisibleRangeStart)
					SelectionStart -= _snapDistance.temporal * (int)(dist / _snapDistance.visual);
				else
					SelectionStart = VisibleRangeStart;
			}
			else if (pos > 0)
			{
				if (SelectionStart + _snapDistance.temporal < VisibleRangeEnd)
					SelectionStart += _snapDistance.temporal * (int)(dist / _snapDistance.visual);
				else
					SelectionStart = VisibleRangeEnd;
			}
		}

		private void SelectionEnd_MouseEnter(object sender, MouseEventArgs e)
		{
			if (_isMouseLeftButtonDown) return;

			e.MouseDevice.OverrideCursor = Cursors.SizeWE;
			if (_selEndElement.ContentTemplate.FindName("shape", _selEndElement) is Shape selectionEnd)
			{
				VisualStateManager.GoToElementState(selectionEnd, "MouseOver", false);
			}
		}

		private void SelectionEnd_MouseLeave(object sender, MouseEventArgs e)
		{
			e.MouseDevice.OverrideCursor = Cursors.Arrow;
			if (_selEndElement.ContentTemplate.FindName("shape", _selEndElement) is Shape selectionEnd &&
				!_selEndElement.IsMouseCaptureWithin)
			{
				VisualStateManager.GoToElementState(selectionEnd, "Normal", false);
			}
		}

		private void SelectionEnd_MouseLeftButtonDown(object sender, MouseEventArgs e)
		{
			if (_isMouseLeftButtonDown) return;

			if (_selEndElement.ContentTemplate.FindName("shape", _selEndElement) is Shape selectionEnd)
			{
				VisualStateManager.GoToElementState(selectionEnd, "MouseLeftButtonDown", false);
			}

			_selEndElement.CaptureMouse();
			_prevMouseCoord        = e.GetPosition(_selEndElement).X;
			_isMouseLeftButtonDown = true;
			RaiseEvent(new RoutedEventArgs(SelectionRangeDragStartedEvent, this));
		}

		private void SelectionEnd_MouseLeftButtonUp(object sender, MouseEventArgs e)
		{
			if (_selEndElement.ContentTemplate.FindName("shape", _selEndElement) is Shape selectionEnd)
			{
				VisualStateManager.GoToElementState(selectionEnd, _selEndElement.IsMouseOver ? "MouseOver" : "Normal", false);
			}

			_selEndElement.ReleaseMouseCapture();
			_isMouseLeftButtonDown = false;
			RaiseEvent(new RoutedEventArgs(SelectionRangeDragCompletedEvent, this));
		}

		private void SelectionEnd_MouseMove(object sender, MouseEventArgs e)
		{
			if (!_isMouseLeftButtonDown) return;

			// Get mouse position, but only continue if it has moved more than half of the visual snap distance
			var pos  = e.GetPosition(_selEndElement).X - _prevMouseCoord;
			var dist = Math.Abs(pos);
			if (dist < _snapDistance.visual / 2)
				return;

			if (pos < 0)
			{
				if (SelectionEnd - _snapDistance.temporal > VisibleRangeStart)
					SelectionEnd -= _snapDistance.temporal * (int)(dist / _snapDistance.visual);
				else
					SelectionEnd = VisibleRangeStart;
			}
			else if (pos > 0)
			{
				if (SelectionEnd + _snapDistance.temporal < VisibleRangeEnd)
					SelectionEnd += _snapDistance.temporal * (int)(dist / _snapDistance.visual);
				else
					SelectionEnd = VisibleRangeEnd;
			}
		}

		private void VisibleRangeStart_MouseEnter(object sender, MouseEventArgs e)
		{
			if (_isMouseLeftButtonDown) return;

			e.MouseDevice.OverrideCursor = Cursors.SizeWE;
			if (_visRngStartElement.ContentTemplate.FindName("shape", _visRngStartElement) is Shape visibleRangeStart)
			{
				VisualStateManager.GoToElementState(visibleRangeStart, "MouseOver", false);
			}
		}

		private void VisibleRangeStart_MouseLeave(object sender, MouseEventArgs e)
		{
			e.MouseDevice.OverrideCursor = Cursors.Arrow;
			if (_visRngStartElement.ContentTemplate.FindName("shape", _visRngStartElement) is Shape visibleRangeStart &&
				!_visRngStartElement.IsMouseCaptureWithin)
			{
				VisualStateManager.GoToElementState(visibleRangeStart, "Normal", false);
			}
		}

		private void VisibleRangeStart_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (_isMouseLeftButtonDown) return;

			if (_visRngStartElement.ContentTemplate.FindName("shape", _visRngStartElement) is Shape visibleRangeStart)
			{
				VisualStateManager.GoToElementState(visibleRangeStart, "MouseLeftButtonDown", false);
			}

			_visRngStartElement.CaptureMouse();
			_prevMouseCoord        = e.GetPosition(_visRngStartElement).X;
			_isMouseLeftButtonDown = true;
			RaiseEvent(new RoutedEventArgs(VisibleRangeDragStartedEvent, this));
		}

		private void VisibleRangeStart_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			if (_visRngStartElement.ContentTemplate.FindName("shape", _visRngStartElement) is Shape visibleRangeStart)
			{
				VisualStateManager.GoToElementState(
					visibleRangeStart,
					_visRngStartElement.IsMouseOver ? "MouseOver" : "Normal",
					false);
			}

			_visRngStartElement.ReleaseMouseCapture();
			_isMouseLeftButtonDown = false;
			RaiseEvent(new RoutedEventArgs(VisibleRangeDragCompletedEvent, this));
		}

		private void VisibleRangeStart_MouseMove(object sender, MouseEventArgs e)
		{
			if (!_isMouseLeftButtonDown || !_visRngStartElement.IsMouseCaptureWithin) return;

			// Get mouse position, but only continue if it has moved more than 0.1 pixels horizontally
			var pos = e.GetPosition(_visRngStartElement).X - _prevMouseCoord;
			if (Math.Abs(pos) < 0.1)
				return;

			var delta = (decimal)pos * (Maximum - Minimum) / (decimal)_zoomPanel.ActualWidth;
			if ((pos < 0 && VisibleRangeStart > Minimum) || (pos > 0 && VisibleRangeStart < Maximum))
			{
				VisibleRangeStart += delta;
			}
		}

		private void VisibleRangeEnd_MouseEnter(object sender, MouseEventArgs e)
		{
			if (_isMouseLeftButtonDown) return;

			e.MouseDevice.OverrideCursor = Cursors.SizeWE;
			if (_visRngEndElement.ContentTemplate.FindName("shape", _visRngEndElement) is Shape visibleRangeEnd)
			{
				VisualStateManager.GoToElementState(visibleRangeEnd, "MouseOver", false);
			}
		}

		private void VisibleRangeEnd_MouseLeave(object sender, MouseEventArgs e)
		{
			e.MouseDevice.OverrideCursor = Cursors.Arrow;
			if (_visRngEndElement.ContentTemplate.FindName("shape", _visRngEndElement) is Shape visibleRangeEnd &&
				!_visRngEndElement.IsMouseCaptureWithin)
			{
				VisualStateManager.GoToElementState(visibleRangeEnd, "Normal", false);
			}
		}

		private void VisibleRangeEnd_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (_isMouseLeftButtonDown) return;

			if (_visRngEndElement.ContentTemplate.FindName("shape", _visRngEndElement) is Shape visibleRangeEnd)
			{
				VisualStateManager.GoToElementState(visibleRangeEnd, "MouseLeftButtonDown", false);
			}

			_visRngEndElement.CaptureMouse();
			_prevMouseCoord        = e.GetPosition(_visRngEndElement).X;
			_isMouseLeftButtonDown = true;
			RaiseEvent(new RoutedEventArgs(VisibleRangeDragStartedEvent, this));
		}

		private void VisibleRangeEnd_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			if (_visRngEndElement.ContentTemplate.FindName("shape", _visRngEndElement) is Shape visibleRangeEnd)
			{
				VisualStateManager.GoToElementState(visibleRangeEnd, _visRngEndElement.IsMouseOver ? "MouseOver" : "Normal", false);
			}

			_visRngEndElement.ReleaseMouseCapture();
			_isMouseLeftButtonDown = false;
			RaiseEvent(new RoutedEventArgs(VisibleRangeDragCompletedEvent, this));
		}

		private void VisibleRangeEnd_MouseMove(object sender, MouseEventArgs e)
		{
			if (!_isMouseLeftButtonDown || !_visRngEndElement.IsMouseCaptureWithin) return;

			// Get mouse position, but only continue if it has moved more than 0.1 pixels horizontally
			var pos = e.GetPosition(_visRngEndElement).X - _prevMouseCoord;
			if (Math.Abs(pos) < 0.1)
				return;

			var delta = (decimal)pos * (Maximum - Minimum) / (decimal)_zoomPanel.ActualWidth;
			if ((pos < 0 && VisibleRangeEnd > Minimum) || (pos > 0 && VisibleRangeEnd < Maximum))
			{
				VisibleRangeEnd += delta;
			}
		}

		private void VisibleRange_MouseEnter(object sender, MouseEventArgs e)
		{
			if (_isMouseLeftButtonDown) return;

			e.MouseDevice.OverrideCursor = Cursors.ScrollWE;
			if (_visRngElement.ContentTemplate.FindName("shape", _visRngElement) is Shape visibleRange)
			{
				VisualStateManager.GoToElementState(visibleRange, "MouseOver", false);
			}
		}

		private void VisibleRange_MouseLeave(object sender, MouseEventArgs e)
		{
			e.MouseDevice.OverrideCursor = Cursors.Arrow;
			if (_visRngElement.ContentTemplate.FindName("shape", _visRngElement) is Shape visibleRange &&
				!_visRngElement.IsMouseCaptureWithin)
			{
				VisualStateManager.GoToElementState(visibleRange, "Normal", false);
			}
		}

		private void VisibleRange_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (_isMouseLeftButtonDown) return;

			if (_visRngElement.ContentTemplate.FindName("shape", _visRngElement) is Shape visibleRange)
			{
				VisualStateManager.GoToElementState(visibleRange, "MouseLeftButtonDown", false);
			}

			if (e.ClickCount >= 2)
			{
				_isVisRngChanging = true;
				VisibleRangeStart = Minimum;
				VisibleRangeEnd   = Maximum;
			}
			else
			{
				_visRngElement.CaptureMouse();
				_prevMouseCoord        = e.GetPosition(_visRngElement).X;
				_isMouseLeftButtonDown = true;
				RaiseEvent(new RoutedEventArgs(VisibleRangeDragStartedEvent, this));
			}
		}

		private void VisibleRange_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			if (_visRngElement.ContentTemplate.FindName("shape", _visRngElement) is Shape visibleRange)
			{
				VisualStateManager.GoToElementState(visibleRange, _visRngElement.IsMouseOver ? "MouseOver" : "Normal", false);
			}

			_visRngElement.ReleaseMouseCapture();
			_isVisRngDragging      = false;
			_isMouseLeftButtonDown = false;
			RaiseEvent(new RoutedEventArgs(VisibleRangeDragCompletedEvent, this));
		}

		private void VisibleRange_MouseMove(object sender, MouseEventArgs e)
		{
			if (!_isMouseLeftButtonDown || !_visRngElement.IsMouseCaptureWithin) return;

			// Get mouse position, but only continue if it has moved more than 0.1 pixels horizontally
			var pos = e.GetPosition(_visRngElement).X - _prevMouseCoord;
			if (Math.Abs(pos) < 0.1)
				return;

			_isVisRngDragging = true;
			var delta = (decimal)pos * (Maximum - Minimum) / (decimal)_zoomPanel.ActualWidth;
			var range = VisibleRange.Magnitude();

			if (pos < 0 && VisibleRangeStart > Minimum)
			{
				_isVisRngChanging = true;
				VisibleRangeStart = Math.Max(VisibleRangeStart + delta, Minimum);
				VisibleRangeEnd   = VisibleRangeStart + range;
			}
			else if (pos > 0 && VisibleRangeEnd < Maximum)
			{
				_isVisRngChanging = true;
				VisibleRangeEnd   = Math.Min(VisibleRangeEnd + delta, Maximum);
				VisibleRangeStart = VisibleRangeEnd - range;
			}
		}
		#endregion
	}
}