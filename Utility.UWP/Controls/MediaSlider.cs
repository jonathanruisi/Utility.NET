﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

using Windows.Devices.Input;
using Windows.Foundation;
using Windows.System;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

using JLR.Utility.NET;
using JLR.Utility.NET.Math;

using Microsoft.Graphics.Canvas.Brushes;
using Microsoft.Graphics.Canvas.Geometry;
using Microsoft.Graphics.Canvas.Text;
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
		private const int     DecimalPrecision    = 8;
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
			_tickAreaBackgroundBrush,
			_markerAreaBackgroundBrush,
			_selectionHighlightBrush,
			_originTickBrush,
			_majorTickBrush,
			_minorTickBrush,
			_markerBrush,
			_clipSpanBrush,
			_clipInOutPointBrush,
			_clipLabelBrush,
			_selectedMarkerBrush,
			_selectedClipBrush;

		private bool
			_isLeftMouseDown,
			_isBoundaryUpdateInProgress;

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
			set => End = decimal.Round(Start + (decimal) value.TotalSeconds, DecimalPrecision);
		}

		public TimeSpan VisibleDuration
		{
			get => TimeSpan.FromSeconds(decimal.ToDouble(ZoomEnd - ZoomStart));
			set
			{
				var newDuration = decimal.Round((decimal) value.TotalSeconds, DecimalPrecision);
				if (newDuration < MinimumVisibleRange)
					newDuration = MinimumVisibleRange;
				else if (newDuration >= End - Start)
				{
					SetVisibleWindow(Start, End);
					return;
				}

				var center = decimal.Round(ZoomStart + ((ZoomEnd - ZoomStart) / 2), DecimalPrecision);
				var start  = decimal.Round(center - (newDuration / 2), DecimalPrecision);

				if (start <= Start)
				{
					SetVisibleWindow(Start, newDuration);
					return;
				}

				var end = decimal.Round(center + (newDuration / 2), DecimalPrecision);
				if (end >= End)
				{
					SetVisibleWindow(End - newDuration, End);
					return;
				}

				SetVisibleWindow(start, end);
			}
		}
		#endregion

		#region Dependency Properties
		#region General
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

		public ObservableCollection<MediaSliderMarker> Markers
		{
			get => (ObservableCollection<MediaSliderMarker>) GetValue(MarkersProperty);
			private set => SetValue(MarkersProperty, value);
		}

		public static readonly DependencyProperty MarkersProperty =
			DependencyProperty.Register("Markers",
										typeof(ObservableCollection<MediaSliderMarker>),
										typeof(MediaSlider),
										new PropertyMetadata(null));

		public MediaSliderMarker SelectedMarker
		{
			get => (MediaSliderMarker) GetValue(SelectedMarkerProperty);
			set => SetValue(SelectedMarkerProperty, value);
		}

		public static readonly DependencyProperty SelectedMarkerProperty =
			DependencyProperty.Register("SelectedMarker",
										typeof(MediaSliderMarker),
										typeof(MediaSlider),
										new PropertyMetadata(null, OnSelectedMarkerChanged));

		public ObservableCollection<MediaSliderClip> Clips
		{
			get => (ObservableCollection<MediaSliderClip>) GetValue(ClipsProperty);
			private set => SetValue(ClipsProperty, value);
		}

		public static readonly DependencyProperty ClipsProperty =
			DependencyProperty.Register("Clips",
										typeof(ObservableCollection<MediaSliderClip>),
										typeof(MediaSlider),
										new PropertyMetadata(null));

		public MediaSliderClip SelectedClip
		{
			get => (MediaSliderClip) GetValue(SelectedClipProperty);
			set => SetValue(SelectedClipProperty, value);
		}

		public static readonly DependencyProperty SelectedClipProperty =
			DependencyProperty.Register("SelectedClip",
										typeof(MediaSliderClip),
										typeof(MediaSlider),
										new PropertyMetadata(null, OnSelectedMarkerChanged));
		#endregion

		#region Behavior
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

		public SnapIntervals SnapToNearest
		{
			get => (SnapIntervals) GetValue(SnapToNearestProperty);
			set => SetValue(SnapToNearestProperty, value);
		}

		public static readonly DependencyProperty SnapToNearestProperty =
			DependencyProperty.Register("SnapToNearest",
										typeof(SnapIntervals),
										typeof(MediaSlider),
										new PropertyMetadata(SnapIntervals.MinorTick));

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
		#endregion

		#region Alignment
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
		#endregion

		#region Sizing
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

		public GridLength MarkerBarSize
		{
			get => (GridLength) GetValue(MarkerBarSizeProperty);
			set => SetValue(MarkerBarSizeProperty, value);
		}

		public static readonly DependencyProperty MarkerBarSizeProperty =
			DependencyProperty.Register("MarkerBarSize",
										typeof(GridLength),
										typeof(MediaSlider),
										new PropertyMetadata(new GridLength(0.20, GridUnitType.Star)));

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

		public double MarkerLineThickness
		{
			get => (double) GetValue(MarkerLineThicknessProperty);
			set => SetValue(MarkerLineThicknessProperty, value);
		}

		public static readonly DependencyProperty MarkerLineThicknessProperty =
			DependencyProperty.Register("MarkerLineThickness",
										typeof(double),
										typeof(MediaSlider),
										new PropertyMetadata(1.0, OnTickCanvasRenderPropertyChanged));

		public double ClipInOutPointLineThickness
		{
			get => (double) GetValue(ClipInOutPointLineThicknessProperty);
			set => SetValue(ClipInOutPointLineThicknessProperty, value);
		}

		public static readonly DependencyProperty ClipInOutPointLineThicknessProperty =
			DependencyProperty.Register("ClipInOutPointLineThickness",
										typeof(double),
										typeof(MediaSlider),
										new PropertyMetadata(1.0, OnTickCanvasRenderPropertyChanged));
		#endregion

		#region Z-Index
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
		#endregion

		#region Alternate Font
		public string AlternateFontFamily
		{
			get => (string) GetValue(AlternateFontFamilyProperty);
			set => SetValue(AlternateFontFamilyProperty, value);
		}

		public static readonly DependencyProperty AlternateFontFamilyProperty =
			DependencyProperty.Register("AlternateFontFamily",
										typeof(string),
										typeof(MediaSlider),
										new PropertyMetadata(null, OnTickCanvasRenderPropertyChanged));

		public double AlternateFontSize
		{
			get => (double) GetValue(AlternateFontSizeProperty);
			set => SetValue(AlternateFontSizeProperty, value);
		}

		public static readonly DependencyProperty AlternateFontSizeProperty =
			DependencyProperty.Register("AlternateFontSize",
										typeof(double),
										typeof(MediaSlider),
										new PropertyMetadata(0.0, OnTickCanvasRenderPropertyChanged));

		public FontStretch AlternateFontStretch
		{
			get => (FontStretch) GetValue(AlternateFontStretchProperty);
			set => SetValue(AlternateFontStretchProperty, value);
		}

		public static readonly DependencyProperty AlternateFontStretchProperty =
			DependencyProperty.Register("AlternateFontStretch",
										typeof(FontStretch),
										typeof(MediaSlider),
										new PropertyMetadata(FontStretch.Normal, OnTickCanvasRenderPropertyChanged));

		public FontStyle AlternateFontStyle
		{
			get => (FontStyle) GetValue(AlternateFontStyleProperty);
			set => SetValue(AlternateFontStyleProperty, value);
		}

		public static readonly DependencyProperty AlternateFontStyleProperty =
			DependencyProperty.Register("AlternateFontStyle",
										typeof(FontStyle),
										typeof(MediaSlider),
										new PropertyMetadata(FontStyle.Normal, OnTickCanvasRenderPropertyChanged));

		public FontWeight AlternateFontWeight
		{
			get => (FontWeight) GetValue(AlternateFontWeightProperty);
			set => SetValue(AlternateFontWeightProperty, value);
		}

		public static readonly DependencyProperty AlternateFontWeightProperty =
			DependencyProperty.Register("AlternateFontWeight",
										typeof(FontWeight),
										typeof(MediaSlider),
										new PropertyMetadata(FontWeights.Normal, OnTickCanvasRenderPropertyChanged));
		#endregion

		#region Brushes
		public Brush TickAreaBackground
		{
			get => (Brush) GetValue(TickAreaBackgroundProperty);
			set => SetValue(TickAreaBackgroundProperty, value);
		}

		public static readonly DependencyProperty TickAreaBackgroundProperty =
			DependencyProperty.Register("TickAreaBackground",
										typeof(Brush),
										typeof(MediaSlider),
										new PropertyMetadata(null, OnTickCanvasBrushChanged));

		public Brush MarkerAreaBackground
		{
			get => (Brush) GetValue(MarkerAreaBackgroundProperty);
			set => SetValue(MarkerAreaBackgroundProperty, value);
		}

		public static readonly DependencyProperty MarkerAreaBackgroundProperty =
			DependencyProperty.Register("MarkerAreaBackground",
										typeof(Brush),
										typeof(MediaSlider),
										new PropertyMetadata(null, OnTickCanvasBrushChanged));

		public Brush ZoomAreaBackground
		{
			get => (Brush) GetValue(ZoomAreaBackgroundProperty);
			set => SetValue(ZoomAreaBackgroundProperty, value);
		}

		public static readonly DependencyProperty ZoomAreaBackgroundProperty =
			DependencyProperty.Register("ZoomAreaBackground",
										typeof(Brush),
										typeof(MediaSlider),
										new PropertyMetadata(null));

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

		public Brush MarkerBrush
		{
			get => (Brush) GetValue(MarkerBrushProperty);
			set => SetValue(MarkerBrushProperty, value);
		}

		public static readonly DependencyProperty MarkerBrushProperty =
			DependencyProperty.Register("MarkerBrush",
										typeof(Brush),
										typeof(MediaSlider),
										new PropertyMetadata(null, OnTickCanvasBrushChanged));

		public Brush ClipSpanBrush
		{
			get => (Brush) GetValue(ClipSpanBrushProperty);
			set => SetValue(ClipSpanBrushProperty, value);
		}

		public static readonly DependencyProperty ClipSpanBrushProperty =
			DependencyProperty.Register("ClipSpanBrush",
										typeof(Brush),
										typeof(MediaSlider),
										new PropertyMetadata(null, OnTickCanvasBrushChanged));

		public Brush ClipInOutPointBrush
		{
			get => (Brush) GetValue(ClipInOutPointBrushProperty);
			set => SetValue(ClipInOutPointBrushProperty, value);
		}

		public static readonly DependencyProperty ClipInOutPointBrushProperty =
			DependencyProperty.Register("ClipInOutPointBrush",
										typeof(Brush),
										typeof(MediaSlider),
										new PropertyMetadata(null, OnTickCanvasBrushChanged));

		public Brush ClipLabelBrush
		{
			get => (Brush) GetValue(ClipLabelBrushProperty);
			set => SetValue(ClipLabelBrushProperty, value);
		}

		public static readonly DependencyProperty ClipLabelBrushProperty =
			DependencyProperty.Register("ClipLabelBrush",
										typeof(Brush),
										typeof(MediaSlider),
										new PropertyMetadata(null, OnTickCanvasBrushChanged));

		public Brush SelectedMarkerBrush
		{
			get => (Brush) GetValue(SelectedMarkerBrushProperty);
			set => SetValue(SelectedMarkerBrushProperty, value);
		}

		public static readonly DependencyProperty SelectedMarkerBrushProperty =
			DependencyProperty.Register("SelectedMarkerBrush",
										typeof(Brush),
										typeof(MediaSlider),
										new PropertyMetadata(null, OnTickCanvasBrushChanged));

		public Brush SelectedClipBrush
		{
			get => (Brush) GetValue(SelectedClipBrushProperty);
			set => SetValue(SelectedClipBrushProperty, value);
		}

		public static readonly DependencyProperty SelectedClipBrushProperty =
			DependencyProperty.Register("SelectedClipBrush",
										typeof(Brush),
										typeof(MediaSlider),
										new PropertyMetadata(null, OnTickCanvasBrushChanged));
		#endregion

		#region Line Styles
		public CanvasStrokeStyle MarkerLineStyle
		{
			get => (CanvasStrokeStyle) GetValue(MarkerLineStyleProperty);
			set => SetValue(MarkerLineStyleProperty, value);
		}

		public static readonly DependencyProperty MarkerLineStyleProperty =
			DependencyProperty.Register("MarkerLineStyle",
										typeof(CanvasStrokeStyle),
										typeof(MediaSlider),
										new PropertyMetadata(null, OnTickCanvasRenderPropertyChanged));

		public CanvasStrokeStyle ClipInOutPointLineStyle
		{
			get => (CanvasStrokeStyle) GetValue(ClipInOutPointLineStyleProperty);
			set => SetValue(ClipInOutPointLineStyleProperty, value);
		}

		public static readonly DependencyProperty ClipInOutPointLineStyleProperty =
			DependencyProperty.Register("ClipInOutPointLineStyle",
										typeof(CanvasStrokeStyle),
										typeof(MediaSlider),
										new PropertyMetadata(null, OnTickCanvasRenderPropertyChanged));
		#endregion

		#region Templates
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
			_intervals      = new LinkedList<(int major, int minor, int minorPerMajor)>();

			Markers = new ObservableCollection<MediaSliderMarker>();
			Clips   = new ObservableCollection<MediaSliderClip>();

			Markers.CollectionChanged += Markers_CollectionChanged;
			Clips.CollectionChanged   += Clips_CollectionChanged;
		}
		#endregion

		#region Public Methods
		public void CenterVisibleWindow(decimal position)
		{
			if (position < Start || position > End)
				return;

			var amount = position - (ZoomStart + ((ZoomEnd - ZoomStart) / 2));
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

		public void IncreasePosition(int numberOfIntervals, SnapIntervals snapInterval)
		{
			var interval = GetSnapIntervalValue(snapInterval);
			var amount   = numberOfIntervals * interval;
			Position = amount >= End
				? End
				: GetNearestSnapValue(Position + amount, false, snapInterval);
		}

		public void DecreasePosition(int numberOfIntervals, SnapIntervals snapInterval)
		{
			var interval = GetSnapIntervalValue(snapInterval);
			var amount   = numberOfIntervals * interval;
			Position = amount <= Start
				? Start
				: GetNearestSnapValue(Position - amount, false, snapInterval);
		}

		public decimal GetNearestSnapValue(decimal relativeTo, bool mustBeVisible, SnapIntervals snapInterval)
		{
			decimal newValue;
			var     value        = Math.Abs(relativeTo);
			var     interval     = GetSnapIntervalValue(snapInterval);
			var     offsetToward = value % interval;

			if (offsetToward == 0)
				return decimal.Round(relativeTo, DecimalPrecision);

			var offsetAway = interval - offsetToward;

			if (mustBeVisible)
			{
				if (relativeTo >= 0)
				{
					if (relativeTo - offsetToward < ZoomStart)
						newValue = relativeTo + offsetAway;
					else if (relativeTo + offsetAway > ZoomEnd)
						newValue = relativeTo - offsetToward;
					else
					{
						newValue = offsetToward < offsetAway
							? relativeTo - offsetToward
							: relativeTo + offsetAway;
					}
				}
				else
				{
					if (relativeTo + offsetToward > ZoomEnd)
						newValue = relativeTo - offsetAway;
					else if (relativeTo - offsetAway < ZoomStart)
						newValue = relativeTo + offsetToward;
					else
					{
						newValue = offsetToward < offsetAway
							? relativeTo + offsetToward
							: relativeTo - offsetAway;
					}
				}
			}
			else
			{
				newValue = offsetToward < offsetAway
					? relativeTo > 0
						? relativeTo - offsetToward
						: relativeTo + offsetToward
					: relativeTo > 0
						? relativeTo + offsetAway
						: relativeTo - offsetAway;
			}

			return decimal.Round(newValue, DecimalPrecision);
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

			// TODO: Need to reload resources needed for rendering
		}

		private static void OnSelectedMarkerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (!(d is MediaSlider slider))
				return;

			slider._tickCanvas.Invalidate();
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
				_tickCanvas.Draw            += TickCanvas_Draw;
				_tickCanvas.CreateResources += TickCanvas_CreateResources;
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
			// CanvasControls need to properly dispose of resources to avoid memory leaks
			_tickCanvas.RemoveFromVisualTree();
			_tickCanvas   = null;
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

			var pos     = ConvertScreenCoordinateToPosition(point.Position.X);
			var closest = GetNearestSnapValue(pos, true, SnapToNearest);

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
			var snapInterval = GetSnapIntervalValue(SnapToNearest);
			var snapPixels   = ConvertTimeIntervalToPixels(snapInterval);
			var pos          = e.GetCurrentPoint(_positionElement).Position.X - _prevMousePosX;
			var delta        = Math.Abs(pos);

			if (delta < snapPixels / 2)
				return;

			var newValue = snapInterval * (int) (delta / snapPixels);
			if (pos < 0)
			{
				newValue = Position - newValue >= ZoomStart
					? GetNearestSnapValue(Position - newValue, true, SnapToNearest)
					: GetNearestSnapValue(ZoomStart, true, SnapToNearest);

				if (newValue != Position)
					Position = newValue;
			}
			else if (pos > 0)
			{
				newValue = Position + newValue <= ZoomEnd
					? GetNearestSnapValue(Position + newValue, true, SnapToNearest)
					: GetNearestSnapValue(ZoomEnd, true, SnapToNearest);

				if (newValue != Position)
					Position = newValue;
			}
		}

		private void SelectionStartElement_PointerMoved(object sender, PointerRoutedEventArgs e)
		{
			if (!_isLeftMouseDown)
				return;

			// Get mouse position, continuing only if the position has changed
			// by more than half of the snap distance
			var snapInterval = GetSnapIntervalValue(SnapToNearest);
			var snapPixels   = ConvertTimeIntervalToPixels(snapInterval);
			var pos          = e.GetCurrentPoint(_selectionStartElement).Position.X - _prevMousePosX;
			var delta        = Math.Abs(pos);

			if (delta < snapPixels / 2)
				return;

			var newValue = snapInterval * (int) (delta / snapPixels);
			if (pos < 0 && SelectionStart != null)
			{
				newValue = SelectionStart - newValue >= ZoomStart
					? GetNearestSnapValue((decimal) SelectionStart - newValue, true, SnapToNearest)
					: GetNearestSnapValue(ZoomStart, true, SnapToNearest);

				if (newValue != SelectionStart)
					SelectionStart = newValue;
			}
			else if (pos > 0 && SelectionStart != null)
			{
				newValue = SelectionStart + newValue <= ZoomEnd
					? GetNearestSnapValue((decimal) SelectionStart + newValue, true, SnapToNearest)
					: GetNearestSnapValue(ZoomEnd, true, SnapToNearest);

				if (newValue != SelectionStart)
					SelectionStart = newValue;
			}
		}

		private void SelectionEndElement_PointerMoved(object sender, PointerRoutedEventArgs e)
		{
			if (!_isLeftMouseDown)
				return;

			// Get mouse position, continuing only if the position has changed
			// by more than half of the snap distance
			var snapInterval = GetSnapIntervalValue(SnapToNearest);
			var snapPixels   = ConvertTimeIntervalToPixels(snapInterval);
			var pos          = e.GetCurrentPoint(_selectionEndElement).Position.X - _prevMousePosX;
			var delta        = Math.Abs(pos);

			if (delta < snapPixels / 2)
				return;

			var newValue = snapInterval * (int) (delta / snapPixels);
			if (pos < 0 && SelectionEnd != null)
			{
				newValue = SelectionEnd - newValue >= ZoomStart
					? GetNearestSnapValue((decimal) SelectionEnd - newValue, true, SnapToNearest)
					: GetNearestSnapValue(ZoomStart, true, SnapToNearest);

				if (newValue != SelectionEnd)
					SelectionEnd = newValue;
			}
			else if (pos > 0 && SelectionEnd != null)
			{
				newValue = SelectionEnd + newValue <= ZoomEnd
					? GetNearestSnapValue((decimal) SelectionEnd + newValue, true, SnapToNearest)
					: GetNearestSnapValue(ZoomEnd, true, SnapToNearest);

				if (newValue != SelectionEnd)
					SelectionEnd = newValue;
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

			var delta = ((decimal) pos * (End - Start)) / (decimal) _zoomPanel.ActualWidth;
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

			var delta = ((decimal) pos * (End - Start)) / (decimal) _zoomPanel.ActualWidth;
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

			var delta = ((decimal) pos * (End - Start)) / (decimal) _zoomPanel.ActualWidth;
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

		#region Event Handlers (Collection)
		private void Markers_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (e.NewItems != null)
			{
				if (e.NewItems.Cast<MediaSliderMarker>().Any(marker => marker.Time >= ZoomStart &&
																	   marker.Time <= ZoomEnd))
				{
					_tickCanvas.Invalidate();
					return;
				}
			}

			if (e.OldItems != null)
			{
				if (e.OldItems.Cast<MediaSliderMarker>().Any(marker => marker.Time >= ZoomStart &&
																	   marker.Time <= ZoomEnd))
				{
					_tickCanvas.Invalidate();
				}
			}
		}

		private void Clips_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (e.NewItems != null)
			{
				if (e.NewItems.Cast<MediaSliderClip>().Any(
					clip => clip.StartTime >= ZoomStart && clip.StartTime <= ZoomEnd ||
							clip.EndTime >= ZoomStart && clip.EndTime <= ZoomEnd))
				{
					_tickCanvas.Invalidate();
					return;
				}
			}

			if (e.OldItems != null)
			{
				if (e.OldItems.Cast<MediaSliderClip>().Any(
					clip => clip.StartTime >= ZoomStart && clip.StartTime <= ZoomEnd ||
							clip.EndTime >= ZoomStart && clip.EndTime <= ZoomEnd))
				{
					_tickCanvas.Invalidate();
				}
			}
		}
		#endregion

		#region Event Handlers (Rendering)
		private void TickCanvas_CreateResources(CanvasControl sender, CanvasCreateResourcesEventArgs args)
		{
			_tickAreaBackgroundBrush   = TickAreaBackground?.CreateCanvasBrush(sender.Device);
			_markerAreaBackgroundBrush = MarkerAreaBackground?.CreateCanvasBrush(sender.Device);
			_selectionHighlightBrush   = SelectionHighlightBrush?.CreateCanvasBrush(sender.Device);
			_originTickBrush           = OriginTickBrush?.CreateCanvasBrush(sender.Device);
			_majorTickBrush            = MajorTickBrush?.CreateCanvasBrush(sender.Device);
			_minorTickBrush            = MinorTickBrush?.CreateCanvasBrush(sender.Device);
			_markerBrush               = MarkerBrush?.CreateCanvasBrush(sender.Device);
			_clipSpanBrush             = ClipSpanBrush?.CreateCanvasBrush(sender.Device);
			_clipInOutPointBrush       = ClipInOutPointBrush?.CreateCanvasBrush(sender.Device);
			_clipLabelBrush            = ClipLabelBrush?.CreateCanvasBrush(sender.Device);
			_selectedMarkerBrush       = SelectedMarkerBrush?.CreateCanvasBrush(sender.Device);
			_selectedClipBrush         = SelectedClipBrush?.CreateCanvasBrush(sender.Device);
		}

		private void TickCanvas_Draw(CanvasControl sender, CanvasDrawEventArgs args)
		{
			// Don't render if the CanvasControl's size is zero
			if (Math.Abs(sender.ActualWidth) < double.Epsilon || Math.Abs(sender.ActualHeight) < double.Epsilon)
				return;

			// Get current marker area and tick area rectangles
			var markerAreaRect = MarkerAreaRect;
			var tickAreaRect   = TickAreaRect;

			// Calculate vertical axis render coordinates for each tick type
			var verticalCoordsOrigin = CalculateVerticalAxisCoordinates(OriginTickRelativeSize);
			var verticalCoordsMajor  = CalculateVerticalAxisCoordinates(MajorTickRelativeSize);
			var verticalCoordsMinor  = CalculateVerticalAxisCoordinates(MinorTickRelativeSize);

			// Calculate vertical axis render coordinates for marker geometry
			var y0 = (float) markerAreaRect.Top;
			var y1 = (float) (markerAreaRect.Height * 0.25);
			var y2 = (float) (markerAreaRect.Height * 0.5);
			var y3 = (float) (markerAreaRect.Height * 0.667);
			var y4 = (float) (markerAreaRect.Height * 0.75);
			var y5 = (float) (markerAreaRect.Top + markerAreaRect.Height);

			// Create marker geometry
			var path = new CanvasPathBuilder(sender.Device);
			path.BeginFigure(5.0f, y0);
			path.AddLine(5.0f, y3);
			path.AddLine(0.0f, y5);
			path.AddLine(-5.0f, y3);
			path.AddLine(-5.0f, y0);
			path.EndFigure(CanvasFigureLoop.Closed);
			var markerGeometry = CanvasGeometry.CreatePath(path);

			// Create clip in-point geometry
			path = new CanvasPathBuilder(sender.Device);
			path.BeginFigure(2.0f, y0);
			path.AddLine(2.0f, y5);
			path.AddLine(-2.0f, y5);
			path.AddLine(-2.0f, y4);
			path.AddLine(-8.0f, y2);
			path.AddLine(-2.0f, y1);
			path.AddLine(-2.0f, y0);
			path.EndFigure(CanvasFigureLoop.Closed);
			var inPointGeometry = CanvasGeometry.CreatePath(path);

			// Create clip out-point geometry
			path = new CanvasPathBuilder(sender.Device);
			path.BeginFigure(-2.0f, y0);
			path.AddLine(-2.0f, y5);
			path.AddLine(2.0f, y5);
			path.AddLine(2.0f, y4);
			path.AddLine(8.0f, y2);
			path.AddLine(2.0f, y1);
			path.AddLine(2.0f, y0);
			path.EndFigure(CanvasFigureLoop.Closed);
			var outPointGeometry = CanvasGeometry.CreatePath(path);

			// Find optimal timescale for readable tick spacing
			_currentInterval = _intervals.First;
			var minorSpaceBetween =
				ConvertTimeIntervalToPixels(GetSnapIntervalValue(SnapIntervals.MinorTick)) - MinorTickThickness;
			var majorSpaceBetween =
				ConvertTimeIntervalToPixels(GetSnapIntervalValue(SnapIntervals.MajorTick)) - MajorTickThickness;
			while ((minorSpaceBetween < MinorTickClutterThreshold ||
					majorSpaceBetween < MajorTickClutterThreshold) &&
				   _currentInterval.Next != null)
			{
				_currentInterval = _currentInterval.Next;
				minorSpaceBetween = ConvertTimeIntervalToPixels(GetSnapIntervalValue(SnapIntervals.MinorTick)) -
									MinorTickThickness;
				majorSpaceBetween = ConvertTimeIntervalToPixels(GetSnapIntervalValue(SnapIntervals.MajorTick)) -
									MajorTickThickness;
			}

			// Create tick lists
			var majorTicks = new HashSet<decimal>();
			var minorTicks = new HashSet<decimal>();

			decimal major             = 0;
			var     minorTickInterval = GetSnapIntervalValue(SnapIntervals.MinorTick);
			var     majorTickInterval = GetSnapIntervalValue(SnapIntervals.MajorTick);
			if (majorTickInterval > 0)
			{
				major = majorTickInterval * (int) (ZoomStart / majorTickInterval);
				if (ZoomStart >= 0 && Math.Abs(major - ZoomStart) > 0)
					major = majorTickInterval * ((int) (ZoomStart / majorTickInterval) + 1);
			}

			decimal minor = 0;
			if (minorTickInterval > 0)
			{
				minor = minorTickInterval * (int) (ZoomStart / minorTickInterval);
				if (ZoomStart >= 0 && Math.Abs(minor - ZoomStart) > 0)
					minor = minorTickInterval * ((int) (ZoomStart / minorTickInterval) + 1);
			}

			var majorAdj = decimal.Round(major, DecimalPrecision);
			var minorAdj = decimal.Round(minor, DecimalPrecision);
			if (majorTickInterval > 0 && minorTickInterval > 0) // Both major and minor ticks are needed
			{
				while (majorAdj <= ZoomEnd || minorAdj <= ZoomEnd)
				{
					if (minorAdj < majorAdj)
						AddMinorTick();
					else
						AddMajorTick();
				}
			}
			else if (majorTickInterval > 0) // Only major ticks are needed
			{
				while (majorAdj <= ZoomEnd)
				{
					AddMajorTick();
				}
			}
			else if (minorTickInterval > 0) // Only minor ticks are needed
			{
				while (minorAdj <= ZoomEnd)
				{
					AddMinorTick();
				}
			}

			// If major and minor ticks sharing the same Z-index overlap, major ticks take precedence.
			if (MajorTickZIndex == MinorTickZIndex)
				minorTicks.ExceptWith(majorTicks);

			// Determine tick type draw order based on Z-index
			var drawOrder = new SortedSet<KeyValuePair<TickType, int>>(new TickTypeComparer())
			{
				new KeyValuePair<TickType, int>(TickType.Origin, OriginTickZIndex),
				new KeyValuePair<TickType, int>(TickType.Major, MajorTickZIndex),
				new KeyValuePair<TickType, int>(TickType.Minor, MinorTickZIndex)
			};

			// Draw background(s)
			args.DrawingSession.FillRectangle(tickAreaRect, _tickAreaBackgroundBrush);
			args.DrawingSession.FillRectangle(markerAreaRect, _markerAreaBackgroundBrush);

			// Draw selection highlight rectangle
			if (_selectionRect != Rect.Empty)
			{
				using (args.DrawingSession.CreateLayer(1.0f))
				{
					args.DrawingSession.FillRectangle(_selectionRect, _selectionHighlightBrush);
				}
			}

			// Draw ticks
			foreach (var type in drawOrder)
			{
				switch (type.Key)
				{
					case TickType.Origin:
					{
						using (args.DrawingSession.CreateLayer(1.0f))
						{
							DrawTick(CalculateHorizontalAxisCoordinate(0.0M),
									 verticalCoordsOrigin,
									 _originTickBrush,
									 OriginTickThickness);
						}

						break;
					}

					case TickType.Major:
					{
						using (args.DrawingSession.CreateLayer(1.0f))
						{
							foreach (var tick in majorTicks)
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
							foreach (var tick in minorTicks)
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

			// Draw clips
			using (args.DrawingSession.CreateLayer(1.0f))
			{
				foreach (var clip in Clips)
				{
					if (clip.EndTime - clip.StartTime == 0 || clip.StartTime > ZoomEnd || clip.EndTime < ZoomStart)
						continue;

					var spanBrush  = clip == SelectedClip ? _selectedClipBrush : _clipSpanBrush;
					var inOutBrush = clip == SelectedClip ? _selectedClipBrush : _clipInOutPointBrush;

					var x1 = (float) CalculateHorizontalAxisCoordinate(
						clip.StartTime <= ZoomStart ? ZoomStart : clip.StartTime);
					var x2 = (float) CalculateHorizontalAxisCoordinate(
						clip.EndTime >= ZoomEnd ? ZoomEnd : clip.EndTime);

					// Generate text label for this clip
					var textFormat = new CanvasTextFormat
					{
						FontFamily          = FontFamily.Source,
						FontSize            = (float) FontSize,
						FontStretch         = FontStretch,
						FontStyle           = FontStyle,
						FontWeight          = FontWeight,
						Direction           = CanvasTextDirection.LeftToRightThenTopToBottom,
						LastLineWrapping    = false,
						LineSpacingMode     = CanvasLineSpacingMode.Default,
						LineSpacing         = 0,
						LineSpacingBaseline = 0,
						HorizontalAlignment = CanvasHorizontalAlignment.Center,
						VerticalAlignment   = CanvasVerticalAlignment.Top,
						WordWrapping        = CanvasWordWrapping.EmergencyBreak
					};

					var textLayout = new CanvasTextLayout(sender.Device,
														  clip.Name,
														  textFormat,
														  x2 - x1,
														  (float) markerAreaRect.Height);

					textLayout.LineSpacingBaseline = (float) textLayout.DrawBounds.Height;
					textLayout.LineSpacing         = textLayout.LineSpacingBaseline + 1;

					// Generate alternate text label for this clip (if applicable)
					CanvasTextLayout textLayoutAlt = null;
					if (!string.IsNullOrEmpty(AlternateFontFamily) && AlternateFontSize > 0)
					{
						var textFormatAlt = new CanvasTextFormat
						{
							FontFamily          = AlternateFontFamily,
							FontSize            = (float) AlternateFontSize,
							FontStretch         = AlternateFontStretch,
							FontStyle           = AlternateFontStyle,
							FontWeight          = AlternateFontWeight,
							Direction           = CanvasTextDirection.LeftToRightThenTopToBottom,
							LastLineWrapping    = false,
							LineSpacingMode     = CanvasLineSpacingMode.Default,
							LineSpacing         = 0,
							LineSpacingBaseline = 0,
							HorizontalAlignment = CanvasHorizontalAlignment.Center,
							VerticalAlignment   = CanvasVerticalAlignment.Top,
							WordWrapping        = CanvasWordWrapping.EmergencyBreak
						};

						textLayoutAlt = new CanvasTextLayout(sender.Device,
															 clip.Name,
															 textFormatAlt,
															 x2 - x1,
															 (float) markerAreaRect.Height);

						textLayoutAlt.LineSpacingBaseline = (float) textLayoutAlt.DrawBounds.Height;
						textLayoutAlt.LineSpacing         = textLayoutAlt.LineSpacingBaseline + 1;
					}

					// Draw the span rectangle and clip label based on available space
					if (textLayout.DrawBounds.Height <= y3 && textLayout.DrawBounds.Width <= x2 - x1)
					{
						args.DrawingSession.FillRectangle(x1, y3, x2 - x1, y5 - y3, spanBrush);
						args.DrawingSession.DrawTextLayout
						(
							textLayout,
							x1,
							(y3 / 2) - (float) (textLayout.DrawBounds.Top + (textLayout.DrawBounds.Height / 2)),
							_clipLabelBrush
						);
					}
					else if (textLayoutAlt != null &&
							 textLayoutAlt.DrawBounds.Height <= y3 &&
							 textLayoutAlt.DrawBounds.Width <= x2 - x1)
					{
						args.DrawingSession.FillRectangle(x1, y3, x2 - x1, y5 - y3, spanBrush);
						args.DrawingSession.DrawTextLayout
						(
							textLayoutAlt,
							x1,
							(y3 / 2) - (float) (textLayoutAlt.DrawBounds.Top +
												(textLayoutAlt.DrawBounds.Height / 2)),
							_clipLabelBrush
						);
					}
					else if (textLayout.DrawBounds.Height <= y5 && textLayout.DrawBounds.Width <= x2 - x1)
					{
						var width = ((x2 - x1) / 2) - ((float) textLayout.DrawBounds.Width / 2) - 2;
						args.DrawingSession.FillRectangle(x1, y3, width, y5 - y3, spanBrush);
						args.DrawingSession.FillRectangle(x2 - width, y3, width, y5 - y3, spanBrush);
						args.DrawingSession.DrawTextLayout
						(
							textLayout,
							x1,
							(y5 / 2) - (float) (textLayout.DrawBounds.Top + (textLayout.DrawBounds.Height / 2)),
							_clipLabelBrush
						);
					}
					else if (textLayoutAlt != null &&
							 textLayoutAlt.DrawBounds.Height <= y5 &&
							 textLayoutAlt.DrawBounds.Width <= x2 - x1)
					{
						var width = ((x2 - x1) / 2) - ((float) textLayoutAlt.DrawBounds.Width / 2) - 2;
						args.DrawingSession.FillRectangle(x1, y3, width, y5 - y3, spanBrush);
						args.DrawingSession.FillRectangle(x2 - width, y3, width, y5 - y3, spanBrush);
						args.DrawingSession.DrawTextLayout
						(
							textLayoutAlt,
							x1,
							(y5 / 2) - (float) (textLayoutAlt.DrawBounds.Top +
												(textLayoutAlt.DrawBounds.Height / 2)),
							_clipLabelBrush
						);
					}
					else
					{
						args.DrawingSession.FillRectangle(x1, y3, x2 - x1, y5 - y3, spanBrush);
					}

					// Draw clip in-point
					if (clip.StartTime >= ZoomStart && clip.StartTime <= ZoomEnd)
					{
						args.DrawingSession.FillGeometry(inPointGeometry, x1, (float) markerAreaRect.Top, inOutBrush);
						args.DrawingSession.DrawLine(x1, (float) tickAreaRect.Top,
													 x1, (float) tickAreaRect.Bottom,
													 inOutBrush,
													 (float) ClipInOutPointLineThickness,
													 ClipInOutPointLineStyle);
					}

					// Draw clip out-point
					if (clip.EndTime >= ZoomStart && clip.EndTime <= ZoomEnd)
					{
						args.DrawingSession.FillGeometry(outPointGeometry, x2, (float) markerAreaRect.Top, inOutBrush);
						args.DrawingSession.DrawLine(x2, (float) tickAreaRect.Top,
													 x2, (float) tickAreaRect.Bottom,
													 inOutBrush,
													 (float) ClipInOutPointLineThickness,
													 ClipInOutPointLineStyle);
					}
				}
			}

			// Draw markers
			using (args.DrawingSession.CreateLayer(1.0f))
			{
				foreach (var marker in Markers)
				{
					if (marker.Time < ZoomStart || marker.Time > ZoomEnd)
						continue;

					var brush = marker == SelectedMarker ? _selectedMarkerBrush : _markerBrush;
					var x     = (float) CalculateHorizontalAxisCoordinate(marker.Time);

					args.DrawingSession.FillGeometry(markerGeometry, x, (float) markerAreaRect.Top, brush);
					args.DrawingSession.DrawLine(x, (float) tickAreaRect.Top,
												 x, (float) tickAreaRect.Bottom,
												 brush,
												 (float) MarkerLineThickness,
												 MarkerLineStyle);
				}
			}

			// Local function to add a major tick to its list and advance to the next
			void AddMajorTick()
			{
				majorTicks.Add(majorAdj);
				major    += majorTickInterval;
				majorAdj =  decimal.Round(major, DecimalPrecision);
			}

			// Local function to add a minor tick to its list and advance to the next
			void AddMinorTick()
			{
				minorTicks.Add(minorAdj);
				minor    += minorTickInterval;
				minorAdj =  decimal.Round(minor, DecimalPrecision);
			}

			// Local function to calculate horizontal axis render coordinate
			double CalculateHorizontalAxisCoordinate(decimal value)
			{
				return (decimal.ToDouble(value - ZoomStart) * tickAreaRect.Width) /
					   decimal.ToDouble(ZoomEnd - ZoomStart);
			}

			// Local function to calculate vertical axis render coordinates
			(double start, double end) CalculateVerticalAxisCoordinates(double relativeSize)
			{
				switch (TickAlignment)
				{
					case NET.Position.Top:
						return (tickAreaRect.Top, relativeSize * tickAreaRect.Height);
					case NET.Position.Middle:
						var offset = (tickAreaRect.Height - (relativeSize * tickAreaRect.Height)) / 2;
						return (tickAreaRect.Top + offset, tickAreaRect.Bottom - offset);
					case NET.Position.Bottom:
						return (tickAreaRect.Bottom,
								tickAreaRect.Bottom - (relativeSize * tickAreaRect.Height));
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
				var posAdj = position - (thickness / 2);
				args.DrawingSession.FillRectangle(new Rect(new Point(posAdj, verticalCoords.start),
														   new Point(posAdj + thickness, verticalCoords.end)),
												  brush);
			}
		}
		#endregion

		#region Private Properties
		private Rect TickAreaRect
		{
			get
			{
				var markerRect = MarkerAreaRect;
				return new Rect()
				{
					X      = markerRect.Left,
					Y      = markerRect.Bottom,
					Width  = markerRect.Width,
					Height = _mainPanel.ActualHeight - markerRect.Height
				};
			}
		}

		private Rect MarkerAreaRect =>
			new Rect(0, 0,
					 _mainPanel.ActualWidth,
					 MarkerBarSize.GridUnitType switch
					 {
						 GridUnitType.Pixel => MarkerBarSize.Value,
						 GridUnitType.Star  => MarkerBarSize.Value * ActualHeight,
						 _                  => 0
					 });
		#endregion

		#region Private Methods
		private void UpdatePositionElementLayout()
		{
			if (_mainPanel == null || _positionElement == null)
				return;

			// Get current tick area rectangle
			var tickAreaRect = TickAreaRect;

			// Hide position element if the current position is not within the zoom range
			if (Position < ZoomStart || Position > ZoomEnd || ZoomStart == ZoomEnd)
			{
				_positionElement.Visibility = Visibility.Collapsed;
				return;
			}

			// Resize position element
			var height = PositionElementRelativeSize * tickAreaRect.Height;
			_positionElement.Height = height;

			// Position the position element on the horizontal axis
			_positionElement.Visibility = Visibility.Visible;
			Canvas.SetLeft(
				_positionElement,
				decimal.ToDouble(
					((Position - ZoomStart) *
					 ((decimal) tickAreaRect.Width / (ZoomEnd - ZoomStart))) -
					((decimal) _positionElement.ActualWidth / 2)));

			// Position the position element on the vertical axis
			var top = PositionElementAlignment switch
			{
				NET.Position.Middle => tickAreaRect.Top + ((tickAreaRect.Height - height) / 2),
				NET.Position.Bottom => tickAreaRect.Bottom - height,
				_                   => tickAreaRect.Top
			};

			Canvas.SetTop(_positionElement, top);
		}

		private void UpdateSelectionElementLayout()
		{
			if (_mainPanel == null)
				return;

			// Get current tick area rectangle
			var tickAreaRect = TickAreaRect;

			var thumbTop = tickAreaRect.Top;
			var thumbHeight = SelectionElementRelativeSize * tickAreaRect.Height;

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
							(((decimal) SelectionStart - ZoomStart) *
							 ((decimal) tickAreaRect.Width / (ZoomEnd - ZoomStart))) -
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
							((decimal) tickAreaRect.Width / (ZoomEnd - ZoomStart))));
				}
				else
				{
					_selectionEndElement.Visibility = Visibility.Collapsed;
				}

				// Position the selection elements on the vertical axis
				thumbTop = SelectionElementAlignment switch
				{
					NET.Position.Middle => tickAreaRect.Top + ((_mainPanel.ActualHeight - thumbHeight) / 2),
					NET.Position.Bottom => tickAreaRect.Bottom - thumbHeight,
					_                   => tickAreaRect.Top
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
												((decimal) tickAreaRect.Width / (ZoomEnd - ZoomStart)));
				var rectWidth = decimal.ToDouble((Math.Abs(adjustedEnd - adjustedStart) *
												  (decimal) tickAreaRect.Width) /
												 (ZoomEnd - ZoomStart));

				// Calculate selection highlight rectangle vertical start coordinate
				var rectTop = SelectionHighlightAlignment switch
				{
					NET.Position.Middle => thumbTop + ((thumbHeight - rectHeight) / 2),
					NET.Position.Bottom => thumbTop + (thumbHeight - rectHeight),
					_                   => thumbTop
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
						((ZoomStart - Start) *
						 ((decimal) _zoomPanel.ActualWidth / (End - Start))) -
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
						(Math.Abs(ZoomEnd - ZoomStart) *
						 (decimal) _zoomPanel.ActualWidth) / (End - Start));
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
			{
				left = Math.Max(left, _selectionStartElement.ActualWidth) -
					   Padding.Left - (_mainPanel?.Margin.Left ?? 0);
			}

			if (_selectionEndElement != null)
			{
				right = Math.Max(right, _selectionEndElement.ActualWidth) -
						Padding.Right - (_mainPanel?.Margin.Right ?? 0);
			}

			if (_zoomStartElement != null)
			{
				left = Math.Max(left, _zoomStartElement.ActualWidth) -
					   Padding.Left - (_zoomPanel?.Margin.Left ?? 0);
			}

			if (_zoomEndElement != null)
			{
				right = Math.Max(right, _zoomEndElement.ActualWidth) -
						Padding.Right - (_zoomPanel?.Margin.Right ?? 0);
			}


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
						OffsetVisibleWindow(decimal.Negate(currentWindow));
					break;

				case FollowMode.Scroll:
					CenterVisibleWindow(Position);
					break;
			}
		}

		private double ConvertTimeIntervalToPixels(decimal duration)
		{
			return decimal.ToDouble(((decimal) _tickCanvas.ActualWidth * duration) / (ZoomEnd - ZoomStart));
		}

		private decimal ConvertScreenCoordinateToPosition(double coordinate)
		{
			var positiveWidth  = ConvertTimeIntervalToPixels(ZoomEnd); // Number of pixels for which Position >= 0
			var zeroCoordinate = _tickCanvas.ActualWidth - positiveWidth;

			if (coordinate >= zeroCoordinate)
			{
				return decimal.Round(((decimal) (coordinate - zeroCoordinate) * ZoomEnd) / (decimal) positiveWidth,
									 DecimalPrecision);
			}

			return decimal.Round(
				((decimal) (zeroCoordinate - coordinate) * ZoomStart) /
				(decimal) (_tickCanvas.ActualWidth - positiveWidth),
				DecimalPrecision);
		}

		private decimal GetSnapIntervalValue(SnapIntervals interval)
		{
			return interval switch
			{
				SnapIntervals.Frame  => ((decimal) 1 / FramesPerSecond),
				SnapIntervals.Second => 1,
				SnapIntervals.Minute => SecondsPerMinute,
				SnapIntervals.Hour   => SecondsPerHour,
				SnapIntervals.Day    => SecondsPerDay,
				SnapIntervals.MinorTick => (((decimal) _currentInterval.Value.minor /
											 _currentInterval.Value.minorPerMajor) * _currentInterval.Value.major),
				SnapIntervals.MajorTick => _currentInterval.Value.major,
				_                       => throw new ArgumentOutOfRangeException(nameof(interval), interval, null)
			};
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
			foreach (var divisor in MathHelper.Divisors(SecondsPerMinute))
			{
				_intervals.AddLast((SecondsPerMinute, (int) divisor, SecondsPerMinute));
			}

			// Add intervals for hours subdivided by minutes
			foreach (var divisor in MathHelper.Divisors(MinutesPerHour))
			{
				_intervals.AddLast((SecondsPerHour, (int) divisor, MinutesPerHour));
			}

			// Add intervals for days subdivided by hours
			foreach (var divisor in MathHelper.Divisors(HoursPerDay))
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

		#region Nested Types (Public)
		public class MediaSliderMarker
		{
			public string Name { get; }
			public decimal Time { get; }

			public MediaSliderMarker(string name, decimal time)
			{
				Name = name;
				Time = time;
			}
		}

		public class MediaSliderClip
		{
			public string Name { get; }
			public decimal StartTime { get; }
			public decimal EndTime { get; }

			public MediaSliderClip(string name, decimal startTime, decimal endTime)
			{
				Name      = name;
				StartTime = startTime;
				EndTime   = endTime;
			}
		}

		public enum SnapIntervals
		{
			Frame,
			Second,
			Minute,
			Hour,
			Day,
			MinorTick,
			MajorTick
		}
		#endregion

		#region Nested Types (Private)
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