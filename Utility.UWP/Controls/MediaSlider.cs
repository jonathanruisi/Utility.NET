using System;
using System.Collections.Generic;

using Windows.Devices.Input;
using Windows.Foundation;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

using JLR.Utility.NET;
using JLR.Utility.NET.Math;

using Microsoft.Graphics.Canvas.Brushes;
using Microsoft.Graphics.Canvas.UI;
using Microsoft.Graphics.Canvas.UI.Xaml;

namespace JLR.Utility.UWP.Controls
{
	[TemplatePart(Name = "PART_TickBar", Type        = typeof(CanvasControl))]
	[TemplatePart(Name = "PART_Position", Type       = typeof(TransportElement))]
	[TemplatePart(Name = "PART_SelectionStart", Type = typeof(TransportElement))]
	[TemplatePart(Name = "PART_SelectionEnd", Type   = typeof(TransportElement))]
	[TemplatePart(Name = "PART_ZoomStart", Type      = typeof(TransportElement))]
	[TemplatePart(Name = "PART_ZoomEnd", Type        = typeof(TransportElement))]
	[TemplatePart(Name = "PART_ZoomThumb", Type      = typeof(TransportElement))]
	public sealed class MediaSlider : Control
	{
		#region Constants
		private const int     DecimalPrecision    = 10;
		private const decimal MinimumVisibleRange = 1.0M;
		private const int     SecondsPerMinute    = 60;
		private const int     SecondsPerHour      = 3600;
		private const int     SecondsPerDay       = 86400;
		private const int     MinutesPerHour      = 60;
		private const int     HoursPerDay         = 24;
		#endregion

		#region Fields
		private TransportElement
			_positionElement,
			_selectionStartElement,
			_selectionEndElement,
			_zoomStartElement,
			_zoomEndElement,
			_zoomThumbElement;

		private ICanvasBrush
			_selectionHighlightBrush,
			_originTickBrush,
			_majorTickBrush,
			_minorTickBrush;

		private bool
			_isLeftMouseDown,
			_isBoundaryUpdateInProgress;

		private readonly HashSet<decimal>
			_majorTicks,
			_minorTicks;

		private CoreCursor _previousCursor;

		private readonly CoreCursor _primaryCursor =
			new CoreCursor(CoreCursorType.UpArrow, 0);

		private CanvasControl _tickCanvas;
		private Panel         _mainPanel, _zoomPanel;
		private Rect          _selectionRect;
		private double        _prevMousePosX;

		private readonly LinkedList<(int major, int minor, int minorPerMajor)>     _intervals;
		private          LinkedListNode<(int major, int minor, int minorPerMajor)> _currentInterval;
		#endregion

		#region Properties
		public TimeSpan Duration
		{
			get => TimeSpan.FromSeconds(decimal.ToDouble(End - Start));
			set => End = Start + (decimal)value.TotalSeconds;
		}

		public TimeSpan VisibleDuration
		{
			get => TimeSpan.FromSeconds(decimal.ToDouble(ZoomEnd - ZoomStart));
			set
			{
				var newDuration = (decimal)value.TotalSeconds;
				if(newDuration < MinimumVisibleRange)
					newDuration = MinimumVisibleRange;
				else if(newDuration >= End - Start)
				{
					SetVisibleWindow(Start, End);
					return;
				}

				var currentDuration = ZoomEnd - ZoomStart;
				var center = ZoomStart + currentDuration / 2;

				var start = center - newDuration / 2;
				if(start <= Start)
				{
					SetVisibleWindow(Start, newDuration);
					return;
				}

				var end = center + newDuration / 2;
				if(end >= End)
				{
					SetVisibleWindow(End - newDuration, End);
					return;
				}

				SetVisibleWindow(start, end);
			}
		}

		private decimal MajorTickInterval => _currentInterval.Value.major;

		private decimal MinorTickInterval => (decimal)_currentInterval.Value.minor /
											 _currentInterval.Value.minorPerMajor *
											 _currentInterval.Value.major;
		#endregion

		#region Dependency Properties
		//goto:#Behavior goto:#Alignment goto:#Sizing goto:#Z_Index goto:#Brushes goto:#Templates
		// tag:#General
		public decimal Start
		{
			get => (decimal) GetValue(StartProperty);
			set => SetValue(StartProperty, value);
		}

		public static readonly DependencyProperty StartProperty =
			DependencyProperty.Register("Start",
										typeof(decimal),
										typeof(MediaSlider),
										new PropertyMetadata(0.0M, OnStartChanged));

		public decimal End
		{
			get => (decimal) GetValue(EndProperty);
			set => SetValue(EndProperty, value);
		}

		public static readonly DependencyProperty EndProperty =
			DependencyProperty.Register("End",
										typeof(decimal),
										typeof(MediaSlider),
										new PropertyMetadata(10.0M, OnEndChanged));

		public decimal? SelectionStart
		{
			get => (decimal?) GetValue(SelectionStartProperty);
			set => SetValue(SelectionStartProperty, value);
		}

		public static readonly DependencyProperty SelectionStartProperty =
			DependencyProperty.Register("SelectionStart",
										typeof(decimal?),
										typeof(MediaSlider),
										new PropertyMetadata(null, OnSelectionStartChanged));

		public decimal? SelectionEnd
		{
			get => (decimal?) GetValue(SelectionEndProperty);
			set => SetValue(SelectionEndProperty, value);
		}

		public static readonly DependencyProperty SelectionEndProperty =
			DependencyProperty.Register("SelectionEnd",
										typeof(decimal?),
										typeof(MediaSlider),
										new PropertyMetadata(null, OnSelectionEndChanged));

		public decimal ZoomStart
		{
			get => (decimal) GetValue(ZoomStartProperty);
			set => SetValue(ZoomStartProperty, value);
		}

		public static readonly DependencyProperty ZoomStartProperty =
			DependencyProperty.Register("ZoomStart",
										typeof(decimal),
										typeof(MediaSlider),
										new PropertyMetadata(0.0M, OnZoomStartChanged));

		public decimal ZoomEnd
		{
			get => (decimal) GetValue(ZoomEndProperty);
			set => SetValue(ZoomEndProperty, value);
		}

		public static readonly DependencyProperty ZoomEndProperty =
			DependencyProperty.Register("ZoomEnd",
										typeof(decimal),
										typeof(MediaSlider),
										new PropertyMetadata(10.0M, OnZoomEndChanged));

		public decimal Position
		{
			get => (decimal) GetValue(PositionProperty);
			set => SetValue(PositionProperty, value);
		}

		public static readonly DependencyProperty PositionProperty =
			DependencyProperty.Register("Position",
										typeof(decimal),
										typeof(MediaSlider),
										new PropertyMetadata(0.0M, OnPositionChanged));

		public int FramesPerSecond
		{
			get => (int) GetValue(FramesPerSecondProperty);
			set => SetValue(FramesPerSecondProperty, value);
		}

		public static readonly DependencyProperty FramesPerSecondProperty =
			DependencyProperty.Register("FramesPerSecond",
										typeof(int),
										typeof(MediaSlider),
										new PropertyMetadata(30, OnFramesPerSecondChanged));

		// tag:#Behavior
		public FollowMode PositionFollowMode
		{
			get => (FollowMode) GetValue(PositionFollowModeProperty);
			set => SetValue(PositionFollowModeProperty, value);
		}

		public static readonly DependencyProperty PositionFollowModeProperty =
			DependencyProperty.Register("PositionFollowMode",
										typeof(FollowMode),
										typeof(MediaSlider),
										new PropertyMetadata(FollowMode.NoFollow));

		public double MinorTickClutterThreshold
		{
			get => (double) GetValue(MinorTickClutterThresholdProperty);
			set => SetValue(MinorTickClutterThresholdProperty, value);
		}

		public static readonly DependencyProperty MinorTickClutterThresholdProperty =
			DependencyProperty.Register("MinorTickClutterThreshold",
										typeof(double),
										typeof(MediaSlider),
										new PropertyMetadata(5.0));

		public double MajorTickClutterThreshold
		{
			get => (double) GetValue(MajorTickClutterThresholdProperty);
			set => SetValue(MajorTickClutterThresholdProperty, value);
		}

		public static readonly DependencyProperty MajorTickClutterThresholdProperty =
			DependencyProperty.Register("MajorTickClutterThreshold",
										typeof(double),
										typeof(MediaSlider),
										new PropertyMetadata(50.0));

		// tag:#Alignment
		public Position TickAlignment
		{
			get => (Position) GetValue(TickAlignmentProperty);
			set => SetValue(TickAlignmentProperty, value);
		}

		public static readonly DependencyProperty TickAlignmentProperty =
			DependencyProperty.Register("TickAlignment",
										typeof(Position),
										typeof(MediaSlider),
										new PropertyMetadata(NET.Position.Bottom, OnTickCanvasRenderPropertyChanged));

		public Position PositionElementAlignment
		{
			get => (Position) GetValue(PositionElementAlignmentProperty);
			set => SetValue(PositionElementAlignmentProperty, value);
		}

		public static readonly DependencyProperty PositionElementAlignmentProperty =
			DependencyProperty.Register("PositionElementAlignment",
										typeof(Position),
										typeof(MediaSlider),
										new PropertyMetadata(NET.Position.Top, OnTransportElementChanged));

		public Position SelectionElementAlignment
		{
			get => (Position) GetValue(SelectionElementAlignmentProperty);
			set => SetValue(SelectionElementAlignmentProperty, value);
		}

		public static readonly DependencyProperty SelectionElementAlignmentProperty =
			DependencyProperty.Register("SelectionElementAlignment",
										typeof(Position),
										typeof(MediaSlider),
										new PropertyMetadata(NET.Position.Top, OnTransportElementChanged));

		public Position SelectionHighlightAlignment
		{
			get => (Position) GetValue(SelectionHighlightAlignmentProperty);
			set => SetValue(SelectionHighlightAlignmentProperty, value);
		}

		public static readonly DependencyProperty SelectionHighlightAlignmentProperty =
			DependencyProperty.Register("SelectionHighlightAlignment",
										typeof(Position),
										typeof(MediaSlider),
										new PropertyMetadata(NET.Position.Middle, OnTickCanvasRenderPropertyChanged));

		// tag:#Sizing
		public GridLength ZoomBarSize
		{
			get => (GridLength) GetValue(ZoomBarSizeProperty);
			set => SetValue(ZoomBarSizeProperty, value);
		}

		public static readonly DependencyProperty ZoomBarSizeProperty =
			DependencyProperty.Register("ZoomBarSize",
										typeof(GridLength),
										typeof(MediaSlider),
										new PropertyMetadata(new GridLength(0.5, GridUnitType.Star)));

		public double PositionElementRelativeSize
		{
			get => (double) GetValue(PositionElementRelativeSizeProperty);
			set => SetValue(PositionElementRelativeSizeProperty, value);
		}

		public static readonly DependencyProperty PositionElementRelativeSizeProperty =
			DependencyProperty.Register("PositionElementRelativeSize",
										typeof(double),
										typeof(MediaSlider),
										new PropertyMetadata(1.0, OnTransportElementChanged));

		public double SelectionElementRelativeSize
		{
			get => (double) GetValue(SelectionElementRelativeSizeProperty);
			set => SetValue(SelectionElementRelativeSizeProperty, value);
		}

		public static readonly DependencyProperty SelectionElementRelativeSizeProperty =
			DependencyProperty.Register("SelectionElementRelativeSize",
										typeof(double),
										typeof(MediaSlider),
										new PropertyMetadata(1.0, OnTransportElementChanged));

		public double SelectionHighlightRelativeSize
		{
			get => (double) GetValue(SelectionHighlightRelativeSizeProperty);
			set => SetValue(SelectionHighlightRelativeSizeProperty, value);
		}

		public static readonly DependencyProperty SelectionHighlightRelativeSizeProperty =
			DependencyProperty.Register("SelectionHighlightRelativeSize",
										typeof(double),
										typeof(MediaSlider),
										new PropertyMetadata(1.0, OnTickCanvasRenderPropertyChanged));

		public double OriginTickRelativeSize
		{
			get => (double) GetValue(OriginTickRelativeSizeProperty);
			set => SetValue(OriginTickRelativeSizeProperty, value);
		}

		public static readonly DependencyProperty OriginTickRelativeSizeProperty =
			DependencyProperty.Register("OriginTickRelativeSize",
										typeof(double),
										typeof(MediaSlider),
										new PropertyMetadata(1.0, OnTickCanvasRenderPropertyChanged));

		public double MajorTickRelativeSize
		{
			get => (double) GetValue(MajorTickRelativeSizeProperty);
			set => SetValue(MajorTickRelativeSizeProperty, value);
		}

		public static readonly DependencyProperty MajorTickRelativeSizeProperty =
			DependencyProperty.Register("MajorTickRelativeSize",
										typeof(double),
										typeof(MediaSlider),
										new PropertyMetadata(1.0, OnTickCanvasRenderPropertyChanged));

		public double MinorTickRelativeSize
		{
			get => (double) GetValue(MinorTickRelativeSizeProperty);
			set => SetValue(MinorTickRelativeSizeProperty, value);
		}

		public static readonly DependencyProperty MinorTickRelativeSizeProperty =
			DependencyProperty.Register("MinorTickRelativeSize",
										typeof(double),
										typeof(MediaSlider),
										new PropertyMetadata(1.0, OnTickCanvasRenderPropertyChanged));

		public double OriginTickThickness
		{
			get => (double) GetValue(OriginTickThicknessProperty);
			set => SetValue(OriginTickThicknessProperty, value);
		}

		public static readonly DependencyProperty OriginTickThicknessProperty =
			DependencyProperty.Register("OriginTickThickness",
										typeof(double),
										typeof(MediaSlider),
										new PropertyMetadata(1.0, OnTickCanvasRenderPropertyChanged));

		public double MajorTickThickness
		{
			get => (double) GetValue(MajorTickThicknessProperty);
			set => SetValue(MajorTickThicknessProperty, value);
		}

		public static readonly DependencyProperty MajorTickThicknessProperty =
			DependencyProperty.Register("MajorTickThickness",
										typeof(double),
										typeof(MediaSlider),
										new PropertyMetadata(1.0, OnTickCanvasRenderPropertyChanged));

		public double MinorTickThickness
		{
			get => (double) GetValue(MinorTickThicknessProperty);
			set => SetValue(MinorTickThicknessProperty, value);
		}

		public static readonly DependencyProperty MinorTickThicknessProperty =
			DependencyProperty.Register("MinorTickThickness",
										typeof(double),
										typeof(MediaSlider),
										new PropertyMetadata(1.0, OnTickCanvasRenderPropertyChanged));
		
		// tag:#Z_Index
		public int OriginTickZIndex
		{
			get => (int) GetValue(OriginTickZIndexProperty);
			set => SetValue(OriginTickZIndexProperty, value);
		}

		public static readonly DependencyProperty OriginTickZIndexProperty =
			DependencyProperty.Register("OriginTickZIndex",
										typeof(int),
										typeof(MediaSlider),
										new PropertyMetadata(0, OnTickCanvasRenderPropertyChanged));

		public int MajorTickZIndex
		{
			get => (int) GetValue(MajorTickZIndexProperty);
			set => SetValue(MajorTickZIndexProperty, value);
		}

		public static readonly DependencyProperty MajorTickZIndexProperty =
			DependencyProperty.Register("MajorTickZIndex",
										typeof(int),
										typeof(MediaSlider),
										new PropertyMetadata(0, OnTickCanvasRenderPropertyChanged));

		public int MinorTickZIndex
		{
			get => (int) GetValue(MinorTickZIndexProperty);
			set => SetValue(MinorTickZIndexProperty, value);
		}

		public static readonly DependencyProperty MinorTickZIndexProperty =
			DependencyProperty.Register("MinorTickZIndex",
										typeof(int),
										typeof(MediaSlider),
										new PropertyMetadata(0, OnTickCanvasRenderPropertyChanged));
		
		// tag:#Brushes
		public Brush SelectionHighlightBrush
		{
			get => (Brush) GetValue(SelectionHighlightBrushProperty);
			set => SetValue(SelectionHighlightBrushProperty, value);
		}

		public static readonly DependencyProperty SelectionHighlightBrushProperty =
			DependencyProperty.Register("SelectionHighlightBrush",
										typeof(Brush),
										typeof(MediaSlider),
										new PropertyMetadata(null, OnTickCanvasBrushChanged));

		public Brush OriginTickBrush
		{
			get => (Brush) GetValue(OriginTickBrushProperty);
			set => SetValue(OriginTickBrushProperty, value);
		}

		public static readonly DependencyProperty OriginTickBrushProperty =
			DependencyProperty.Register("OriginTickBrush",
										typeof(Brush),
										typeof(MediaSlider),
										new PropertyMetadata(null, OnTickCanvasBrushChanged));

		public Brush MajorTickBrush
		{
			get => (Brush) GetValue(MajorTickBrushProperty);
			set => SetValue(MajorTickBrushProperty, value);
		}

		public static readonly DependencyProperty MajorTickBrushProperty =
			DependencyProperty.Register("MajorTickBrush",
										typeof(Brush),
										typeof(MediaSlider),
										new PropertyMetadata(null, OnTickCanvasBrushChanged));

		public Brush MinorTickBrush
		{
			get => (Brush) GetValue(MinorTickBrushProperty);
			set => SetValue(MinorTickBrushProperty, value);
		}

		public static readonly DependencyProperty MinorTickBrushProperty =
			DependencyProperty.Register("MinorTickBrush",
										typeof(Brush),
										typeof(MediaSlider),
										new PropertyMetadata(null, OnTickCanvasBrushChanged));
		
		// tag:#Templates
		public ControlTemplate PositionElementTemplate
		{
			get => (ControlTemplate) GetValue(PositionElementTemplateProperty);
			set => SetValue(PositionElementTemplateProperty, value);
		}

		public static readonly DependencyProperty PositionElementTemplateProperty =
			DependencyProperty.Register("PositionElementTemplate",
										typeof(ControlTemplate),
										typeof(MediaSlider),
										new PropertyMetadata(null, OnTransportElementChanged));

		public ControlTemplate SelectionStartElementTemplate
		{
			get => (ControlTemplate) GetValue(SelectionStartElementTemplateProperty);
			set => SetValue(SelectionStartElementTemplateProperty, value);
		}

		public static readonly DependencyProperty SelectionStartElementTemplateProperty =
			DependencyProperty.Register("SelectionStartElementTemplate",
										typeof(ControlTemplate),
										typeof(MediaSlider),
										new PropertyMetadata(null, OnTransportElementChanged));

		public ControlTemplate SelectionEndElementTemplate
		{
			get => (ControlTemplate) GetValue(SelectionEndElementTemplateProperty);
			set => SetValue(SelectionEndElementTemplateProperty, value);
		}

		public static readonly DependencyProperty SelectionEndElementTemplateProperty =
			DependencyProperty.Register("SelectionEndElementTemplate",
										typeof(ControlTemplate),
										typeof(MediaSlider),
										new PropertyMetadata(null, OnTransportElementChanged));

		public ControlTemplate ZoomStartElementTemplate
		{
			get => (ControlTemplate) GetValue(ZoomStartElementTemplateProperty);
			set => SetValue(ZoomStartElementTemplateProperty, value);
		}

		public static readonly DependencyProperty ZoomStartElementTemplateProperty =
			DependencyProperty.Register("ZoomStartElementTemplate",
										typeof(ControlTemplate),
										typeof(MediaSlider),
										new PropertyMetadata(null, OnTransportElementChanged));

		public ControlTemplate ZoomEndElementTemplate
		{
			get => (ControlTemplate) GetValue(ZoomEndElementTemplateProperty);
			set => SetValue(ZoomEndElementTemplateProperty, value);
		}

		public static readonly DependencyProperty ZoomEndElementTemplateProperty =
			DependencyProperty.Register("ZoomEndElementTemplate",
										typeof(ControlTemplate),
										typeof(MediaSlider),
										new PropertyMetadata(null, OnTransportElementChanged));

		public ControlTemplate ZoomThumbElementTemplate
		{
			get => (ControlTemplate) GetValue(ZoomThumbElementTemplateProperty);
			set => SetValue(ZoomThumbElementTemplateProperty, value);
		}

		public static readonly DependencyProperty ZoomThumbElementTemplateProperty =
			DependencyProperty.Register("ZoomThumbElementTemplate",
										typeof(ControlTemplate),
										typeof(MediaSlider),
										new PropertyMetadata(null, OnTransportElementChanged));
		#endregion

		#region Events
		public event EventHandler<decimal> PositionChanged;

		private void RaisePositionChanged()
		{
			var handler = PositionChanged;
			handler?.Invoke(this, Position);
		}

		public event EventHandler<decimal> PositionDragStarted;

		private void RaisePositionDragStarted()
		{
			var handler = PositionDragStarted;
			handler?.Invoke(this, Position);
		}

		public event EventHandler<decimal> PositionDragCompleted;

		private void RaisePositionDragCompleted()
		{
			var handler = PositionDragCompleted;
			handler?.Invoke(this, Position);
		}

		public event EventHandler<(decimal start, decimal end)> DurationChanged;

		private void RaiseDurationChanged()
		{
			var handler = DurationChanged;
			handler?.Invoke(this, (Start, End));
		}

		public event EventHandler<(decimal start, decimal end)?> SelectionChanged;

		private void RaiseSelectionChanged()
		{
			(decimal, decimal)? selection;
			if (SelectionStart == null || SelectionEnd == null)
				selection = null;
			else
				selection = ((decimal) SelectionStart, (decimal) SelectionEnd);

			var handler = SelectionChanged;
			handler?.Invoke(this, selection);
		}

		public event EventHandler<(decimal start, decimal end)> ZoomChanged;

		private void RaiseZoomChanged()
		{
			var handler = ZoomChanged;
			handler?.Invoke(this, (ZoomStart, ZoomEnd));
		}
		#endregion

		#region Constructor
		public MediaSlider()
		{
			DefaultStyleKey = typeof(MediaSlider);
			_majorTicks     = new HashSet<decimal>();
			_minorTicks     = new HashSet<decimal>();
			_intervals      = new LinkedList<(int major, int minor, int minorPerMajor)>();
		}
		#endregion

		#region Public Methods
		public void CenterVisibleWindow(decimal position)
		{
			if (position < Start || position > End)
				return;

			var amount = position - (ZoomStart + (ZoomEnd - ZoomStart) / 2);
			OffsetVisibleWindow(amount);
		}

		public void OffsetVisibleWindow(decimal offset)
		{
			var currentWindow = ZoomEnd - ZoomStart;

			if (offset < 0 && ZoomStart > Start)
			{
				ZoomStart = Math.Max(ZoomStart + offset, Start);
				ZoomEnd   = ZoomStart + currentWindow;
			}
			else if (offset > 0 && ZoomEnd < End)
			{
				ZoomEnd   = Math.Min(ZoomEnd + offset, End);
				ZoomStart = ZoomEnd - currentWindow;
			}
		}

		public void SetVisibleWindow(decimal start, decimal end)
		{
			if (start < Start)
				throw new ArgumentException("This value must be >= Start", nameof(start));
			if (end > End)
				throw new ArgumentException("This value must be <= End", nameof(end));

			if (ZoomStart > start && ZoomEnd < end ||
				ZoomStart < start && ZoomEnd > end ||
				ZoomEnd >= start + MinimumVisibleRange)
			{
				ZoomStart = start;
				ZoomEnd   = end;
			}
			else
			{
				ZoomEnd   = end;
				ZoomStart = start;
			}
		}

		public void IncreasePosition(int majorIntervals, int minorIntervals)
		{
			var amount = majorIntervals * MajorTickInterval + minorIntervals * MinorTickInterval;
			Position = amount >= End
				? End
				: decimal.Round(Position + amount, DecimalPrecision);
		}

		public void DecreasePosition(int majorIntervals, int minorIntervals)
		{
			var amount = majorIntervals * MajorTickInterval + minorIntervals * MinorTickInterval;
			Position = amount <= Start
				? Start
				: decimal.Round(Position - amount, DecimalPrecision);
		}

		public decimal GetNearestTick(decimal relativeTo, bool preferVisible)
		{
			var value = Math.Abs(relativeTo);

			var offsetMajorToward = decimal.Round(value % MajorTickInterval, DecimalPrecision);
			var offsetMinorToward = decimal.Round(value % MinorTickInterval, DecimalPrecision);

			if (offsetMajorToward == 0 || offsetMinorToward == 0)
				return relativeTo;

			var offsetMajorAway = decimal.Round(MajorTickInterval - offsetMajorToward, DecimalPrecision);
			var offsetMinorAway = decimal.Round(MinorTickInterval - offsetMinorToward, DecimalPrecision);

			var offsetToward = Math.Min(offsetMajorToward, offsetMinorToward);
			var offsetAway   = Math.Min(offsetMajorAway, offsetMinorAway);

			if (!preferVisible)
			{
				return decimal.Round(offsetToward < offsetAway
					                     ? relativeTo > 0
						                     ? relativeTo - offsetToward
						                     : relativeTo + offsetToward
					                     : relativeTo > 0
						                     ? relativeTo + offsetAway
						                     : relativeTo - offsetAway,
				                     DecimalPrecision);
			}

			if (relativeTo >= 0)
			{
				if (relativeTo - offsetToward < ZoomStart)
					return decimal.Round(relativeTo + offsetAway, DecimalPrecision);
				if (relativeTo + offsetAway > ZoomEnd)
					return decimal.Round(relativeTo - offsetToward, DecimalPrecision);
				return decimal.Round(offsetToward < offsetAway
					                     ? relativeTo - offsetToward
					                     : relativeTo + offsetAway
				                   , DecimalPrecision);
			}

			if (relativeTo + offsetToward > ZoomEnd)
				return decimal.Round(relativeTo - offsetAway, DecimalPrecision);
			if (relativeTo - offsetAway < ZoomStart)
				return decimal.Round(relativeTo + offsetToward, DecimalPrecision);
			return decimal.Round(offsetToward < offsetAway
				                     ? relativeTo + offsetToward
				                     : relativeTo - offsetAway,
			                     DecimalPrecision);

		}
		#endregion

		#region Dependency Property Callbacks
		private static void OnStartChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (!(d is MediaSlider slider) || slider._isBoundaryUpdateInProgress)
				return;

			slider._isBoundaryUpdateInProgress = true;
			slider.AdjustEnd();
			if (slider.AdjustSelectionStart() || slider.AdjustSelectionEnd())
				slider.RaiseSelectionChanged();
			if (slider.AdjustZoomStart() || slider.AdjustZoomEnd())
				slider.RaiseZoomChanged();
			if (slider.AdjustPosition())
				slider.RaisePositionChanged();
			slider._isBoundaryUpdateInProgress = false;

			slider._tickCanvas?.Invalidate();
			slider.UpdateSelectionElementLayout();
			slider.UpdateZoomElementLayout();
			slider.UpdatePositionElementLayout();
			slider.RaiseDurationChanged();
		}

		private static void OnEndChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (!(d is MediaSlider slider) || slider._isBoundaryUpdateInProgress)
				return;

			slider._isBoundaryUpdateInProgress = true;
			slider.AdjustStart();
			if (slider.AdjustSelectionStart() || slider.AdjustSelectionEnd())
				slider.RaiseSelectionChanged();
			if (slider.AdjustZoomStart() || slider.AdjustZoomEnd())
				slider.RaiseZoomChanged();
			if (slider.AdjustPosition())
				slider.RaisePositionChanged();
			slider._isBoundaryUpdateInProgress = false;

			slider._tickCanvas?.Invalidate();
			slider.UpdateSelectionElementLayout();
			slider.UpdateZoomElementLayout();
			slider.UpdatePositionElementLayout();
			slider.RaiseDurationChanged();
		}

		private static void OnSelectionStartChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (!(d is MediaSlider slider) || slider._isBoundaryUpdateInProgress)
				return;

			slider._isBoundaryUpdateInProgress = true;
			slider.AdjustSelectionStart();
			slider._isBoundaryUpdateInProgress = false;

			slider._tickCanvas?.Invalidate();
			slider.UpdateSelectionElementLayout();
			slider.RaiseSelectionChanged();
		}

		private static void OnSelectionEndChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (!(d is MediaSlider slider) || slider._isBoundaryUpdateInProgress)
				return;

			slider._isBoundaryUpdateInProgress = true;
			slider.AdjustSelectionEnd();
			slider._isBoundaryUpdateInProgress = false;

			slider._tickCanvas?.Invalidate();
			slider.UpdateSelectionElementLayout();
			slider.RaiseSelectionChanged();
		}

		private static void OnZoomStartChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (!(d is MediaSlider slider) || slider._isBoundaryUpdateInProgress)
				return;

			slider._isBoundaryUpdateInProgress = true;
			slider.AdjustZoomStart();
			slider._isBoundaryUpdateInProgress = false;

			slider._tickCanvas?.Invalidate();
			slider.UpdateSelectionElementLayout();
			slider.UpdateZoomElementLayout();
			slider.UpdatePositionElementLayout();
			slider.RaiseZoomChanged();
		}

		private static void OnZoomEndChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (!(d is MediaSlider slider) || slider._isBoundaryUpdateInProgress)
				return;

			slider._isBoundaryUpdateInProgress = true;
			slider.AdjustZoomEnd();
			slider._isBoundaryUpdateInProgress = false;

			slider._tickCanvas?.Invalidate();
			slider.UpdateSelectionElementLayout();
			slider.UpdateZoomElementLayout();
			slider.UpdatePositionElementLayout();
			slider.RaiseZoomChanged();
		}

		private static void OnPositionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (!(d is MediaSlider slider) || slider._isBoundaryUpdateInProgress)
				return;

			slider._isBoundaryUpdateInProgress = true;
			slider.AdjustPosition();
			slider._isBoundaryUpdateInProgress = false;

			slider.UpdatePositionElementLayout();
			slider.RaisePositionChanged();
			slider.FollowPosition();
		}

		private static void OnFramesPerSecondChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (!(d is MediaSlider slider) || !slider.IsLoaded)
				return;

			slider.InitializeTimescale();
			slider._tickCanvas.Invalidate();
		}

		private static void OnTransportElementChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (!(d is MediaSlider slider))
				return;

			if (e.Property == PositionElementAlignmentProperty ||
				e.Property == PositionElementRelativeSizeProperty ||
				e.Property == PositionElementTemplateProperty)
			{
				slider.UpdatePositionElementLayout();
			}
			else if (e.Property == SelectionElementAlignmentProperty ||
					 e.Property == SelectionElementRelativeSizeProperty ||
					 e.Property == SelectionStartElementTemplateProperty ||
					 e.Property == SelectionEndElementTemplateProperty)
			{
				slider.UpdateSelectionElementLayout();
			}
			else if (e.Property == ZoomStartElementTemplateProperty ||
					 e.Property == ZoomEndElementTemplateProperty ||
					 e.Property == ZoomThumbElementTemplateProperty)
			{
				slider.UpdateZoomElementLayout();
			}

			if (e.Property == PositionElementTemplateProperty ||
				e.Property == SelectionStartElementTemplateProperty ||
				e.Property == SelectionEndElementTemplateProperty ||
				e.Property == ZoomStartElementTemplateProperty ||
				e.Property == ZoomEndElementTemplateProperty)
			{
				slider.AdjustPadding();
			}
		}

		private static void OnTickCanvasRenderPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (!(d is MediaSlider slider) || slider._tickCanvas == null)
				return;

			slider._tickCanvas.Invalidate();
		}

		private static void OnTickCanvasBrushChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (!(d is MediaSlider slider))
				return;

			//@@warn TODO: Need to reload resources needed for rendering
		}
		#endregion

		#region Layout Method Overrides
		protected override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			if (GetTemplateChild("PART_TickBar") is CanvasControl tickBar)
			{
				if (VisualTreeHelper.GetParent(tickBar) is Panel parent)
				{
					_mainPanel                =  parent;
					_mainPanel.SizeChanged    += MainPanel_SizeChanged;
					_mainPanel.PointerEntered += MainPanel_PointerEntered;
					_mainPanel.PointerExited  += MainPanel_PointerExited;
					_mainPanel.PointerPressed += MainPanel_PointerPressed;
				}

				_tickCanvas                 =  tickBar;
				_tickCanvas.Draw            += DrawTickBar;
				_tickCanvas.CreateResources += CreateTickBarResources;
			}

			if (GetTemplateChild("PART_Position") is TransportElement position)
			{
				_positionElement                 =  position;
				_positionElement.PointerPressed  += TransportElement_PointerPressed;
				_positionElement.PointerReleased += TransportElement_PointerReleased;
				_positionElement.PointerMoved    += PositionElement_PointerMoved;
				_positionElement.SizeChanged     += TransportElement_SizeChanged;
			}

			if (GetTemplateChild("PART_SelectionStart") is TransportElement selectionStart)
			{
				_selectionStartElement                 =  selectionStart;
				_selectionStartElement.PointerPressed  += TransportElement_PointerPressed;
				_selectionStartElement.PointerReleased += TransportElement_PointerReleased;
				_selectionStartElement.PointerMoved    += SelectionStartElement_PointerMoved;
				_selectionStartElement.SizeChanged     += TransportElement_SizeChanged;
			}

			if (GetTemplateChild("PART_SelectionEnd") is TransportElement selectionEnd)
			{
				_selectionEndElement                 =  selectionEnd;
				_selectionEndElement.PointerPressed  += TransportElement_PointerPressed;
				_selectionEndElement.PointerReleased += TransportElement_PointerReleased;
				_selectionEndElement.PointerMoved    += SelectionEndElement_PointerMoved;
				_selectionEndElement.SizeChanged     += TransportElement_SizeChanged;
			}

			if (GetTemplateChild("PART_ZoomStart") is TransportElement zoomStart)
			{
				_zoomStartElement                 =  zoomStart;
				_zoomStartElement.PointerPressed  += TransportElement_PointerPressed;
				_zoomStartElement.PointerReleased += TransportElement_PointerReleased;
				_zoomStartElement.PointerMoved    += ZoomStartElement_PointerMoved;
				_zoomStartElement.DoubleTapped    += ZoomStartElement_DoubleTapped;
				_zoomStartElement.SizeChanged     += TransportElement_SizeChanged;
			}

			if (GetTemplateChild("PART_ZoomEnd") is TransportElement zoomEnd)
			{
				_zoomEndElement                 =  zoomEnd;
				_zoomEndElement.PointerPressed  += TransportElement_PointerPressed;
				_zoomEndElement.PointerReleased += TransportElement_PointerReleased;
				_zoomEndElement.PointerMoved    += ZoomEndElement_PointerMoved;
				_zoomEndElement.DoubleTapped    += ZoomEndElement_DoubleTapped;
				_zoomEndElement.SizeChanged     += TransportElement_SizeChanged;
			}

			if (GetTemplateChild("PART_ZoomThumb") is TransportElement zoomThumb)
			{
				_zoomThumbElement = zoomThumb;
				if (VisualTreeHelper.GetParent(zoomThumb) is Panel parent)
				{
					_zoomPanel              =  parent;
					_zoomPanel.SizeChanged  += ZoomPanel_SizeChanged;
					_zoomPanel.DoubleTapped += ZoomPanel_DoubleTapped;
				}

				_zoomThumbElement.PointerPressed  += TransportElement_PointerPressed;
				_zoomThumbElement.PointerReleased += TransportElement_PointerReleased;
				_zoomThumbElement.PointerMoved    += ZoomThumbElement_PointerMoved;
				_zoomThumbElement.DoubleTapped    += ZoomThumbElement_DoubleTapped;
			}

			Loaded   += MediaSlider_Loaded;
			Unloaded += MediaSlider_Unloaded;
		}
		#endregion

		#region Event Handlers (General)
		private void MediaSlider_Loaded(object sender, RoutedEventArgs e)
		{
			InitializeTimescale();
			AdjustPadding();
		}

		private void MediaSlider_Unloaded(object sender, RoutedEventArgs e)
		{
			// CanvasControl needs to properly dispose of resources to avoid memory leaks
			_tickCanvas.RemoveFromVisualTree();
			_tickCanvas = null;
		}

		private void MainPanel_PointerEntered(object sender, PointerRoutedEventArgs e)
		{
			if (_positionElement.IsMouseOver || _selectionStartElement.IsMouseOver || _selectionEndElement.IsMouseOver)
				return;

			_previousCursor                         = Window.Current.CoreWindow.PointerCursor;
			Window.Current.CoreWindow.PointerCursor = _primaryCursor;
		}

		private void MainPanel_PointerExited(object sender, PointerRoutedEventArgs e)
		{
			Window.Current.CoreWindow.PointerCursor = _previousCursor;
		}

		private void MainPanel_PointerPressed(object sender, PointerRoutedEventArgs e)
		{
			if (_positionElement.IsMouseOver ||
			    _selectionStartElement.IsMouseOver ||
			    _selectionEndElement.IsMouseOver ||
			    _isLeftMouseDown)
				return;

			var point = e.GetCurrentPoint(_mainPanel);
			if (point.PointerDevice.PointerDeviceType != PointerDeviceType.Mouse)
			{
				e.Handled = true;
				return;
			}

			var pos = ConvertScreenCoordinateToPosition(point.Position.X);
			var closest = GetNearestTick(pos, true);

			if (point.Properties.IsLeftButtonPressed)
				Position = closest;
			else if (point.Properties.IsRightButtonPressed)
			{
				if (Window.Current.CoreWindow.GetKeyState(VirtualKey.Control).HasFlag(CoreVirtualKeyStates.Down))
					SelectionEnd = closest;
				else
					SelectionStart = closest;
			}
		}

		private void MainPanel_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			_tickCanvas.Width  = e.NewSize.Width;
			_tickCanvas.Height = e.NewSize.Height;
			UpdatePositionElementLayout();
			UpdateSelectionElementLayout();
		}

		private void ZoomPanel_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			UpdateZoomElementLayout();
		}

		private void ZoomPanel_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
		{
			e.Handled = true;

			if (e.PointerDeviceType != PointerDeviceType.Mouse)
				return;

			CenterVisibleWindow(Position);
		}
		#endregion

		#region Event Handlers (Transport Elements)
		private void TransportElement_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			if (Math.Abs(e.PreviousSize.Width - e.NewSize.Width) > double.Epsilon)
				AdjustPadding();
		}

		private void TransportElement_PointerPressed(object sender, PointerRoutedEventArgs e)
		{
			if (!(sender is TransportElement element) || _isLeftMouseDown)
				return;

			var point = e.GetCurrentPoint(element);
			if (point.PointerDevice.PointerDeviceType != PointerDeviceType.Mouse)
			{
				e.Handled = true;
				return;
			}

			if (point.Properties.IsLeftButtonPressed)
			{
				_prevMousePosX   = point.Position.X;
				_isLeftMouseDown = true;

				if (element == _positionElement)
					RaisePositionDragStarted();
			}
			else if (point.Properties.IsRightButtonPressed)
			{
				// Set selection start to position (or selection end if CTRL is pressed)
				if (element == _positionElement)
				{
					if (Window.Current.CoreWindow.GetKeyState(VirtualKey.Control).HasFlag(CoreVirtualKeyStates.Down))
						SelectionEnd = Position;
					else
						SelectionStart = Position;
				}

				// Set position to selection start
				else if (element == _selectionStartElement && SelectionStart != null)
				{
					Position = (decimal) SelectionStart;
				}

				// Set position to selection end
				else if (element == _selectionEndElement && SelectionEnd != null)
				{
					Position = (decimal) SelectionEnd;
				}

				// Zoom to selection
				else if (element == _zoomThumbElement && SelectionStart != null && SelectionEnd != null)
				{
					SetVisibleWindow((decimal) SelectionStart, (decimal) SelectionEnd);
				}
			}
		}

		private void TransportElement_PointerReleased(object sender, PointerRoutedEventArgs e)
		{
			if (!(sender is TransportElement element))
				return;

			_isLeftMouseDown = false;

			if (element == _positionElement)
				RaisePositionDragCompleted();
		}

		private void PositionElement_PointerMoved(object sender, PointerRoutedEventArgs e)
		{
			if (!_isLeftMouseDown)
				return;

			// Get mouse position, continuing only if the position has changed
			// by more than half of the snap distance
			var snapPixels = ConvertTimeIntervalToPixels(MinorTickInterval);
			var pos        = e.GetCurrentPoint(_positionElement).Position.X - _prevMousePosX;
			var delta      = Math.Abs(pos);

			if (delta < snapPixels / 2)
				return;

			var newValue = MinorTickInterval * (int) (delta / snapPixels);
			if (pos < 0)
			{
				if (Position - newValue >= ZoomStart)
					Position -= newValue;
				else
					Position = GetNearestTick(ZoomStart, true);

			}
			else if (pos > 0)
			{
				if (Position + newValue <= ZoomEnd)
					Position += newValue;
				else
					Position = GetNearestTick(ZoomEnd, true);
			}
		}

		private void SelectionStartElement_PointerMoved(object sender, PointerRoutedEventArgs e)
		{
			if (!_isLeftMouseDown)
				return;

			// Get mouse position, continuing only if the position has changed
			// by more than half of the snap distance
			var snapPixels = ConvertTimeIntervalToPixels(MinorTickInterval);
			var pos        = e.GetCurrentPoint(_selectionStartElement).Position.X - _prevMousePosX;
			var delta      = Math.Abs(pos);

			if (delta < snapPixels / 2)
				return;

			var newValue = MinorTickInterval * (int) (delta / snapPixels);
			if (pos < 0)
			{
				if (SelectionStart - newValue >= ZoomStart)
					SelectionStart -= newValue;
				else
					Position = GetNearestTick(ZoomStart, true);
			}
			else if (pos > 0)
			{
				if (SelectionStart + newValue <= ZoomEnd)
					SelectionStart += newValue;
				else
					Position = GetNearestTick(ZoomEnd, true);
			}
		}

		private void SelectionEndElement_PointerMoved(object sender, PointerRoutedEventArgs e)
		{
			if (!_isLeftMouseDown)
				return;

			// Get mouse position, continuing only if the position has changed
			// by more than half of the snap distance
			var snapPixels = ConvertTimeIntervalToPixels(MinorTickInterval);
			var pos        = e.GetCurrentPoint(_selectionEndElement).Position.X - _prevMousePosX;
			var delta      = Math.Abs(pos);

			if (delta < snapPixels / 2)
				return;

			var newValue = MinorTickInterval * (int) (delta / snapPixels);
			if (pos < 0)
			{
				if (SelectionEnd - newValue >= ZoomStart)
					SelectionEnd -= newValue;
				else
					Position = GetNearestTick(ZoomStart, true);
			}
			else if (pos > 0)
			{
				if (SelectionEnd + newValue <= ZoomEnd)
					SelectionEnd += newValue;
				else
					Position = GetNearestTick(ZoomEnd, true);
			}
		}

		private void ZoomStartElement_PointerMoved(object sender, PointerRoutedEventArgs e)
		{
			if (!_isLeftMouseDown)
				return;

			// Get mouse position, continuing only if the position has changed
			// by more than 0.1 pixels horizontally
			var pos = e.GetCurrentPoint(_zoomStartElement).Position.X - _prevMousePosX;
			if (Math.Abs(pos) < 0.1)
				return;

			var delta = (decimal) pos * (End - Start) / (decimal) _zoomPanel.ActualWidth;
			if (pos < 0 && ZoomStart > Start || pos > 0 && ZoomStart < End)
			{
				ZoomStart += delta;
			}
		}

		private void ZoomEndElement_PointerMoved(object sender, PointerRoutedEventArgs e)
		{
			if (!_isLeftMouseDown)
				return;

			// Get mouse position, continuing only if the position has changed
			// by more than 0.1 pixels horizontally
			var pos = e.GetCurrentPoint(_zoomEndElement).Position.X - _prevMousePosX;
			if (Math.Abs(pos) < 0.1)
				return;

			var delta = (decimal) pos * (End - Start) / (decimal) _zoomPanel.ActualWidth;
			if (pos < 0 && ZoomEnd > Start || pos > 0 && ZoomEnd < End)
			{
				ZoomEnd += delta;
			}
		}

		private void ZoomThumbElement_PointerMoved(object sender, PointerRoutedEventArgs e)
		{
			if (!_isLeftMouseDown)
				return;

			// Get mouse position, continuing only if the position has changed
			// by more than 0.1 pixels horizontally
			var pos = e.GetCurrentPoint(_zoomThumbElement).Position.X - _prevMousePosX;
			if (Math.Abs(pos) < 0.1)
				return;

			var delta = (decimal) pos * (End - Start) / (decimal) _zoomPanel.ActualWidth;
			OffsetVisibleWindow(delta);
		}

		private void ZoomStartElement_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
		{
			if (!(sender is TransportElement))
				return;

			e.Handled = true;

			if (e.PointerDeviceType != PointerDeviceType.Mouse)
				return;

			if (ZoomStart != Start)
			{
				ZoomStart = Start;
			}
		}

		private void ZoomEndElement_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
		{
			if (!(sender is TransportElement))
				return;

			e.Handled = true;

			if (e.PointerDeviceType != PointerDeviceType.Mouse)
				return;

			if (ZoomEnd != End)
			{
				ZoomEnd = End;
			}
		}

		private void ZoomThumbElement_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
		{
			if (!(sender is TransportElement))
				return;

			e.Handled = true;

			if (e.PointerDeviceType != PointerDeviceType.Mouse)
				return;

			if (ZoomStart == Start && ZoomEnd == End)
			{
				VisibleDuration = TimeSpan.FromSeconds(decimal.ToDouble(MinimumVisibleRange));
				CenterVisibleWindow(Position);
			}
			else
			{
				ZoomStart = Start;
				ZoomEnd   = End;
			}
		}
		#endregion

		#region Event Handlers (Rendering)
		private void CreateTickBarResources(CanvasControl sender, CanvasCreateResourcesEventArgs args)
		{
			_selectionHighlightBrush = SelectionHighlightBrush?.CreateCanvasBrush(sender.Device);
			_originTickBrush         = OriginTickBrush?.CreateCanvasBrush(sender.Device);
			_majorTickBrush          = MajorTickBrush?.CreateCanvasBrush(sender.Device);
			_minorTickBrush          = MinorTickBrush?.CreateCanvasBrush(sender.Device);
		}

		private void DrawTickBar(CanvasControl sender, CanvasDrawEventArgs args)
		{
			// Don't render if the CanvasControl's size is zero
			if (Math.Abs(ActualWidth) < double.Epsilon || Math.Abs(ActualHeight) < double.Epsilon)
				return;

			// Calculate vertical axis render coordinates
			var verticalCoordsOrigin = CalculateVerticalAxisCoordinates(OriginTickRelativeSize);
			var verticalCoordsMajor  = CalculateVerticalAxisCoordinates(MajorTickRelativeSize);
			var verticalCoordsMinor  = CalculateVerticalAxisCoordinates(MinorTickRelativeSize);

			// Find optimal timescale for readable tick spacing
			_currentInterval = _intervals.First;
			var minorSpaceBetween = ConvertTimeIntervalToPixels(MinorTickInterval) - MinorTickThickness;
			var majorSpaceBetween = ConvertTimeIntervalToPixels(MajorTickInterval) - MajorTickThickness;
			while ((minorSpaceBetween < MinorTickClutterThreshold ||
					majorSpaceBetween < MajorTickClutterThreshold) &&
				   _currentInterval.Next != null)
			{
				_currentInterval  = _currentInterval.Next;
				minorSpaceBetween = ConvertTimeIntervalToPixels(MinorTickInterval) - MinorTickThickness;
				majorSpaceBetween = ConvertTimeIntervalToPixels(MajorTickInterval) - MajorTickThickness;
			}

			// Update tick lists
			_majorTicks.Clear();
			_minorTicks.Clear();

			decimal major = 0;
			if (MajorTickInterval > 0)
			{
				major = MajorTickInterval * (int) (ZoomStart / MajorTickInterval);
				if (ZoomStart >= 0 && Math.Abs(major - ZoomStart) > 0)
					major = MajorTickInterval * ((int) (ZoomStart / MajorTickInterval) + 1);
			}

			decimal minor = 0;
			if (MinorTickInterval > 0)
			{
				minor = MinorTickInterval * (int) (ZoomStart / MinorTickInterval);
				if (ZoomStart >= 0 && Math.Abs(minor - ZoomStart) > 0)
					minor = MinorTickInterval * ((int) (ZoomStart / MinorTickInterval) + 1);
			}

			var majorAdj = decimal.Round(major, DecimalPrecision, MidpointRounding.ToEven);
			var minorAdj = decimal.Round(minor, DecimalPrecision, MidpointRounding.ToEven);
			if (MajorTickInterval > 0 && MinorTickInterval > 0) // Both major and minor ticks are needed
			{
				while (majorAdj <= ZoomEnd || minorAdj <= ZoomEnd)
				{
					if (minorAdj < majorAdj)
						AddMinorTick();
					else
						AddMajorTick();
				}
			}
			else if (MajorTickInterval > 0) // Only major ticks are needed
			{
				while (majorAdj <= ZoomEnd)
				{
					AddMajorTick();
				}
			}
			else if (MinorTickInterval > 0) // Only minor ticks are needed
			{
				while (minorAdj <= ZoomEnd)
				{
					AddMinorTick();
				}
			}

			// If major and minor ticks sharing the same Z-index overlap, major ticks take precedence.
			if (MajorTickZIndex == MinorTickZIndex)
				_minorTicks.ExceptWith(_majorTicks);

			// Determine tick type draw order based on Z-index
			var drawOrder = new SortedSet<KeyValuePair<TickType, int>>(new TickTypeComparer())
			{
				new KeyValuePair<TickType, int>(TickType.Origin, OriginTickZIndex),
				new KeyValuePair<TickType, int>(TickType.Major, MajorTickZIndex),
				new KeyValuePair<TickType, int>(TickType.Minor, MinorTickZIndex)
			};

			// Draw selection highlight rectangle
			if (_selectionRect != Rect.Empty)
			{
				using (args.DrawingSession.CreateLayer(1.0f))
				{
					args.DrawingSession.FillRectangle(_selectionRect, _selectionHighlightBrush);
				}
			}

			// Draw Ticks
			foreach (var type in drawOrder)
			{
				switch (type.Key)
				{
					case TickType.Origin:
					{
						DrawTick(CalculateHorizontalAxisCoordinate(0.0M),
								 verticalCoordsOrigin,
								 _originTickBrush,
								 OriginTickThickness);
						break;
					}

					case TickType.Major:
					{
						using (args.DrawingSession.CreateLayer(1.0f))
						{
							foreach (var tick in _majorTicks)
							{
								DrawTick(CalculateHorizontalAxisCoordinate(tick),
										 verticalCoordsMajor,
										 _majorTickBrush,
										 MajorTickThickness);
							}
						}

						break;
					}

					case TickType.Minor:
					{
						using (args.DrawingSession.CreateLayer(1.0f))
						{
							foreach (var tick in _minorTicks)
							{
								DrawTick(CalculateHorizontalAxisCoordinate(tick),
										 verticalCoordsMinor,
										 _minorTickBrush,
										 MinorTickThickness);
							}
						}

						break;
					}
				}
			}

			// Local function to add a major tick to its list and advance to the next
			void AddMajorTick()
			{
				_majorTicks.Add(majorAdj);
				major    += MajorTickInterval;
				majorAdj =  decimal.Round(major, DecimalPrecision, MidpointRounding.ToEven);
			}

			// Local function to add a minor tick to its list and advance to the next
			void AddMinorTick()
			{
				_minorTicks.Add(minorAdj);
				minor    += MinorTickInterval;
				minorAdj =  decimal.Round(minor, DecimalPrecision, MidpointRounding.ToEven);
			}

			// Local function to calculate horizontal axis render coordinate
			double CalculateHorizontalAxisCoordinate(decimal value)
			{
				return decimal.ToDouble(value - ZoomStart) *
					   _tickCanvas.ActualWidth /
					   decimal.ToDouble(ZoomEnd - ZoomStart);
			}

			// Local function to calculate vertical axis render coordinates
			(double start, double end) CalculateVerticalAxisCoordinates(double relativeSize)
			{
				switch (TickAlignment)
				{
					case NET.Position.Top:
						return (0, relativeSize * _tickCanvas.ActualHeight);
					case NET.Position.Middle:
						var pos = (_tickCanvas.ActualHeight - relativeSize * _tickCanvas.ActualHeight) / 2;
						return (pos, _tickCanvas.ActualHeight - pos);
					case NET.Position.Bottom:
						return (_tickCanvas.ActualHeight,
								_tickCanvas.ActualHeight - relativeSize * _tickCanvas.ActualHeight);
					default:
						return (0, 0);
				}
			}

			// Local function to draw a tick
			void DrawTick(double position,
						  (double start, double end) verticalCoords,
						  ICanvasBrush brush,
						  double thickness)
			{
				var posAdj = position - thickness / 2;
				args.DrawingSession.FillRectangle(new Rect(new Point(posAdj, verticalCoords.start),
														   new Point(posAdj + thickness, verticalCoords.end)),
												  brush);
			}
		}
		#endregion

		#region Private Methods
		private void UpdatePositionElementLayout()
		{
			if (_mainPanel == null || _positionElement == null)
				return;

			// Hide position element if the current position is not within the zoom range
			if (Position < ZoomStart || Position > ZoomEnd || ZoomStart == ZoomEnd)
			{
				_positionElement.Visibility = Visibility.Collapsed;
				return;
			}

			// Resize position element
			var height = PositionElementRelativeSize * _mainPanel.ActualHeight;
			_positionElement.Height = height;

			// Position the position element on the horizontal axis
			_positionElement.Visibility = Visibility.Visible;
			Canvas.SetLeft(
				_positionElement,
				decimal.ToDouble(
					(Position - ZoomStart) *
					((decimal) _mainPanel.ActualWidth / (ZoomEnd - ZoomStart)) -
					(decimal) _positionElement.ActualWidth / 2));

			// Position the position element on the vertical axis
			var top = PositionElementAlignment switch
			{
				NET.Position.Middle => ((_mainPanel.ActualHeight - height) / 2),
				NET.Position.Bottom => (_mainPanel.ActualHeight - height),
				_                   => 0.0
			};

			Canvas.SetTop(_positionElement, top);
		}

		private void UpdateSelectionElementLayout()
		{
			if (_mainPanel == null)
				return;

			var thumbTop    = 0.0;
			var thumbHeight = SelectionElementRelativeSize * _mainPanel.ActualHeight;

			if (_selectionStartElement != null && _selectionEndElement != null)
			{
				// Resize the selection elements
				_selectionStartElement.Height = thumbHeight;
				_selectionEndElement.Height   = thumbHeight;

				// Position the selection start element on the horizontal axis
				if (SelectionStart >= ZoomStart &&
					SelectionStart <= ZoomEnd &&
					ZoomStart != ZoomEnd &&
					SelectionEnd != null)
				{
					_selectionStartElement.Visibility = Visibility.Visible;
					Canvas.SetLeft(
						_selectionStartElement,
						decimal.ToDouble(
							((decimal) SelectionStart - ZoomStart) *
							((decimal) _mainPanel.ActualWidth / (ZoomEnd - ZoomStart)) -
							(decimal) _selectionStartElement.ActualWidth));
				}
				else
				{
					_selectionStartElement.Visibility = Visibility.Collapsed;
				}

				// Position the selection end element on the horizontal axis
				if (SelectionEnd >= ZoomStart &&
					SelectionEnd <= ZoomEnd &&
					ZoomStart != ZoomEnd &&
					SelectionStart != null)
				{
					_selectionEndElement.Visibility = Visibility.Visible;
					Canvas.SetLeft(
						_selectionEndElement,
						decimal.ToDouble(
							((decimal) SelectionEnd - ZoomStart) *
							((decimal) _mainPanel.ActualWidth / (ZoomEnd - ZoomStart))));
				}
				else
				{
					_selectionEndElement.Visibility = Visibility.Collapsed;
				}

				// Position the selection elements on the vertical axis
				thumbTop = SelectionElementAlignment switch
				{
					NET.Position.Middle => ((_mainPanel.ActualHeight - thumbHeight) / 2),
					NET.Position.Bottom => (_mainPanel.ActualHeight - thumbHeight),
					_                   => 0.0
				};

				Canvas.SetTop(_selectionStartElement, thumbTop);
				Canvas.SetTop(_selectionEndElement, thumbTop);
			}

			// Update selection highlight rectangle
			if (SelectionEnd - SelectionStart != 0 &&
				SelectionStart < ZoomEnd &&
				SelectionEnd > ZoomStart)
			{
				// Calculate selection highlight rectangle height
				var rectHeight = SelectionHighlightRelativeSize * thumbHeight;

				// Calculate selection highlight rectangle width and horizontal start coordinate
				var adjustedStart = SelectionStart >= ZoomStart ? (decimal) SelectionStart : ZoomStart;
				var adjustedEnd   = SelectionEnd <= ZoomEnd ? (decimal) SelectionEnd : ZoomEnd;
				var rectLeft = decimal.ToDouble((adjustedStart - ZoomStart) *
												((decimal) _mainPanel.ActualWidth / (ZoomEnd - ZoomStart)));
				var rectWidth = decimal.ToDouble(Math.Abs(adjustedEnd - adjustedStart) *
												 (decimal) _mainPanel.ActualWidth /
												 (ZoomEnd - ZoomStart));

				// Calculate selection highlight rectangle vertical start coordinate
				var rectTop = SelectionHighlightAlignment switch
				{
					NET.Position.Top    => thumbTop,
					NET.Position.Middle => (thumbTop + (thumbHeight - rectHeight) / 2),
					NET.Position.Bottom => (thumbTop + (thumbHeight - rectHeight)),
					_                   => 0.0
				};

				_selectionRect = new Rect(rectLeft, rectTop, rectWidth, rectHeight);
			}
			else
			{
				_selectionRect = Rect.Empty;
			}
		}

		private void UpdateZoomElementLayout()
		{
			if (_zoomPanel == null)
				return;

			// Position and resize the zoom start element
			if (_zoomStartElement != null)
			{
				_zoomStartElement.Height = _zoomPanel.ActualHeight;
				Canvas.SetTop(_zoomStartElement, 0);
				Canvas.SetLeft(
					_zoomStartElement,
					decimal.ToDouble(
						(ZoomStart - Start) *
						((decimal) _zoomPanel.ActualWidth / (End - Start)) -
						(decimal) _zoomStartElement.ActualWidth));
			}

			// Position and resize the zoom end element
			if (_zoomEndElement != null)
			{
				_zoomEndElement.Height = _zoomPanel.ActualHeight;
				Canvas.SetTop(_zoomStartElement, 0);
				Canvas.SetLeft(
					_zoomEndElement,
					decimal.ToDouble((ZoomEnd - Start) *
									 ((decimal) _zoomPanel.ActualWidth / (End - Start))));
			}

			// Position and resize the zoom thumb element
			if (_zoomThumbElement != null)
			{
				if (ZoomEnd - ZoomStart != 0)
				{
					_zoomThumbElement.Visibility = Visibility.Visible;
					_zoomThumbElement.Height     = _zoomPanel.ActualHeight;
					Canvas.SetTop(_zoomThumbElement, 0);
					Canvas.SetLeft(
						_zoomThumbElement,
						decimal.ToDouble(
							(ZoomStart - Start) *
							((decimal) _zoomPanel.ActualWidth / (End - Start))));
					_zoomThumbElement.Width = decimal.ToDouble(
						Math.Abs(ZoomEnd - ZoomStart) *
						(decimal) _zoomPanel.ActualWidth / (End - Start));
				}
				else
				{
					_zoomThumbElement.Visibility = Visibility.Collapsed;
				}
			}
		}

		private void AdjustPadding()
		{
			if (!IsLoaded)
				return;

			var left  = _positionElement?.ActualWidth / 2 ?? 0;
			var right = left;

			if (_selectionStartElement != null)
				left = Math.Max(left, _selectionStartElement.ActualWidth) -
					   Padding.Left - (_mainPanel?.Margin.Left ?? 0);

			if (_selectionEndElement != null)
				right = Math.Max(right, _selectionEndElement.ActualWidth) -
						Padding.Right - (_mainPanel?.Margin.Right ?? 0);

			if (_zoomStartElement != null)
				left = Math.Max(left, _zoomStartElement.ActualWidth) -
					   Padding.Left - (_zoomPanel?.Margin.Left ?? 0);

			if (_zoomEndElement != null)
				right = Math.Max(right, _zoomEndElement.ActualWidth) -
						Padding.Right - (_zoomPanel?.Margin.Right ?? 0);


			if (_mainPanel != null)
			{
				var margin = new Thickness(left > 0 ? left : 0,
										   _mainPanel.Margin.Top,
										   right > 0 ? right : 0,
										   _mainPanel.Margin.Bottom);
				_mainPanel.SetValue(MarginProperty, margin);
			}

			if (_zoomPanel != null)
			{
				var margin = new Thickness(left > 0 ? left : 0,
										   _zoomPanel.Margin.Top,
										   right > 0 ? right : 0,
										   _zoomPanel.Margin.Bottom);
				_zoomPanel.SetValue(MarginProperty, margin);
			}
		}

		private void FollowPosition()
		{
			switch (PositionFollowMode)
			{
				case FollowMode.Advance:
					var currentWindow = ZoomEnd - ZoomStart;
					if (Position > ZoomEnd)
						OffsetVisibleWindow(currentWindow);
					else if (Position < ZoomStart)
						OffsetVisibleWindow(-currentWindow);
					break;

				case FollowMode.Scroll:
					CenterVisibleWindow(Position);
					break;
			}
		}

		private double ConvertTimeIntervalToPixels(decimal duration)
		{
			return decimal.ToDouble((decimal) _tickCanvas.ActualWidth * duration / (ZoomEnd - ZoomStart));
		}

		private decimal ConvertScreenCoordinateToPosition(double coordinate)
		{
			var positiveWidth  = ConvertTimeIntervalToPixels(ZoomEnd); // Number of pixels for which Position >= 0
			var zeroCoordinate = _tickCanvas.ActualWidth - positiveWidth;

			if (coordinate >= zeroCoordinate)
			{
				return Math.Round((decimal) (coordinate - zeroCoordinate) * ZoomEnd / (decimal) positiveWidth,
				                  DecimalPrecision,
				                  MidpointRounding.ToEven);
			}

			return Math.Round(
				(decimal) (zeroCoordinate - coordinate) * ZoomStart /
				(decimal) (_tickCanvas.ActualWidth - positiveWidth),
				DecimalPrecision,
				MidpointRounding.ToEven);
		}

		private void InitializeTimescale()
		{
			_intervals.Clear();

			// Add intervals for seconds subdivided by frames
			foreach (var divisor in MathHelper.Divisors((ulong) FramesPerSecond))
			{
				_intervals.AddLast((1, (int) divisor, FramesPerSecond));
			}

			// Add intervals for minutes subdivided by seconds
			foreach (var divisor in MathHelper.Divisors(60))
			{
				_intervals.AddLast((SecondsPerMinute, (int) divisor, SecondsPerMinute));
			}

			// Add intervals for hours subdivided by minutes
			foreach (var divisor in MathHelper.Divisors(60))
			{
				_intervals.AddLast((SecondsPerHour, (int) divisor, MinutesPerHour));
			}

			// Add intervals for days subdivided by hours
			foreach (var divisor in MathHelper.Divisors(60))
			{
				_intervals.AddLast((SecondsPerDay, (int) divisor, HoursPerDay));
			}

			_currentInterval = _intervals.First;
		}

		private void AdjustStart()
		{
			if (Start <= End)
				return;

			Start = End;
		}

		private void AdjustEnd()
		{
			if (End >= Start)
				return;

			End = Start;
		}

		private bool AdjustSelectionStart()
		{
			if (SelectionStart == null)
			{
				if (SelectionEnd == null)
					return false;

				SelectionEnd = null;
				return true;
			}

			var result = false;

			if (SelectionStart < Start)
			{
				SelectionStart = Start;
				result         = true;
			}
			else if (SelectionStart > End)
			{
				SelectionStart = End;
				result         = true;
			}

			if (SelectionEnd == null || SelectionStart > SelectionEnd)
			{
				SelectionEnd = SelectionStart;
				result       = true;
			}

			return result;
		}

		private bool AdjustSelectionEnd()
		{
			if (SelectionEnd == null)
			{
				if (SelectionStart == null)
					return false;

				SelectionStart = null;
				return true;
			}

			var result = false;

			if (SelectionEnd < Start)
			{
				SelectionEnd = Start;
				result       = true;
			}
			else if (SelectionEnd > End)
			{
				SelectionEnd = End;
				result       = true;
			}

			if (SelectionStart == null || SelectionEnd < SelectionStart)
			{
				SelectionStart = SelectionEnd;
				result         = true;
			}

			return result;
		}

		private bool AdjustZoomStart()
		{
			if (ZoomStart < Start)
			{
				ZoomStart = Start;
				return true;
			}

			var offset = ZoomEnd - Math.Min(End - Start, MinimumVisibleRange);
			if (ZoomStart > offset)
			{
				ZoomStart = offset > Start ? offset : Start;
				return true;
			}

			return false;
		}

		private bool AdjustZoomEnd()
		{
			if (ZoomEnd > End)
			{
				ZoomEnd = End;
				return true;
			}

			var offset = ZoomStart + Math.Min(End - Start, MinimumVisibleRange);
			if (ZoomEnd < offset)
			{
				ZoomEnd = offset < End ? offset : End;
				return true;
			}

			return false;
		}

		private bool AdjustPosition()
		{
			if (Position < Start)
			{
				Position = Start;
				return true;
			}

			if (Position > End)
			{
				Position = End;
				return true;
			}

			return false;
		}
		#endregion

		#region Nested Types
		private enum TickType
		{
			Origin,
			Major,
			Minor
		}

		private class TickTypeComparer : IComparer<KeyValuePair<TickType, int>>
		{
			public int Compare(KeyValuePair<TickType, int> x, KeyValuePair<TickType, int> y)
			{
				if (x.Value != y.Value)
					return x.Value.CompareTo(y.Value);

				var result = 0;
				switch (x.Key)
				{
					case TickType.Origin:
					{
						if (y.Key == TickType.Major || y.Key == TickType.Minor)
							result = 1;
						break;
					}

					case TickType.Major:
					{
						switch (y.Key)
						{
							case TickType.Origin:
								result = -1;
								break;
							case TickType.Minor:
								result = 1;
								break;
						}

						break;
					}

					case TickType.Minor:
					{
						if (y.Key == TickType.Origin || y.Key == TickType.Major)
							result = -1;
						break;
					}
				}

				return result;
			}
		}
		#endregion
	}

	#region Enumerated Types
	/// <summary>
	/// Specifies the way in which the visible window
	/// adjusts to keep an item visible
	/// (the position thumb, for example).
	/// </summary>
	public enum FollowMode
	{
		/// <summary>
		/// Visible window is never automatically adjusted.
		/// </summary>
		NoFollow,

		/// <summary>
		/// Advance the visible window by its current duration
		/// in the direction of change.
		/// </summary>
		Advance,

		/// <summary>
		/// Once the item reaches the center of the visible window,
		/// keep it centered by moving the window based on the
		/// amount and direction of change.
		/// </summary>
		Scroll
	}
	#endregion
}