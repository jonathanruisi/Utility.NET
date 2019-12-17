using System.Collections.ObjectModel;

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

using JLR.Utility.NET;

using Microsoft.Graphics.Canvas.Geometry;

namespace JLR.Utility.UWP.Controls
{
	public sealed partial class MediaSlider
	{
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

		public double InPointLineThickness
		{
			get => (double) GetValue(InPointLineThicknessProperty);
			set => SetValue(InPointLineThicknessProperty, value);
		}

		public static readonly DependencyProperty InPointLineThicknessProperty =
			DependencyProperty.Register("InPointLineThickness",
			                            typeof(double),
			                            typeof(MediaSlider),
			                            new PropertyMetadata(1.0, OnTickCanvasRenderPropertyChanged));

		public double OutPointLineThickness
		{
			get => (double) GetValue(OutPointLineThicknessProperty);
			set => SetValue(OutPointLineThicknessProperty, value);
		}

		public static readonly DependencyProperty OutPointLineThicknessProperty =
			DependencyProperty.Register("OutPointLineThickness",
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

		#region Brushes
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

		public Brush ClipMarkerBrush
		{
			get => (Brush) GetValue(ClipMarkerBrushProperty);
			set => SetValue(ClipMarkerBrushProperty, value);
		}

		public static readonly DependencyProperty ClipMarkerBrushProperty =
			DependencyProperty.Register("ClipMarkerBrush",
			                            typeof(Brush),
			                            typeof(MediaSlider),
			                            new PropertyMetadata(null, OnMarkerCanvasBrushChanged));

		public Brush InPointBrush
		{
			get => (Brush) GetValue(InPointBrushProperty);
			set => SetValue(InPointBrushProperty, value);
		}

		public static readonly DependencyProperty InPointBrushProperty =
			DependencyProperty.Register("InPointBrush",
			                            typeof(Brush),
			                            typeof(MediaSlider),
			                            new PropertyMetadata(null, OnTickCanvasBrushChanged));

		public Brush OutPointBrush
		{
			get => (Brush) GetValue(OutPointBrushProperty);
			set => SetValue(OutPointBrushProperty, value);
		}

		public static readonly DependencyProperty OutPointBrushProperty =
			DependencyProperty.Register("OutPointBrush",
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

		public CanvasStrokeStyle InPointLineStyle
		{
			get => (CanvasStrokeStyle) GetValue(InPointLineStyleProperty);
			set => SetValue(InPointLineStyleProperty, value);
		}

		public static readonly DependencyProperty InPointLineStyleProperty =
			DependencyProperty.Register("InPointLineStyle",
			                            typeof(CanvasStrokeStyle),
			                            typeof(MediaSlider),
			                            new PropertyMetadata(null, OnTickCanvasRenderPropertyChanged));

		public CanvasStrokeStyle OutPointLineStyle
		{
			get => (CanvasStrokeStyle) GetValue(OutPointLineStyleProperty);
			set => SetValue(OutPointLineStyleProperty, value);
		}

		public static readonly DependencyProperty OutPointLineStyleProperty =
			DependencyProperty.Register("OutPointLineStyle",
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
			slider._markerCanvas?.Invalidate();
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
			slider._markerCanvas?.Invalidate();
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
			slider._markerCanvas?.Invalidate();
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
			slider._markerCanvas?.Invalidate();
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

		private static void OnMarkerCanvasBrushChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (!(d is MediaSlider slider))
				return;

			//@@warn TODO: Need to reload resources needed for rendering
		}

		private static void OnSelectedMarkerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (!(d is MediaSlider slider))
				return;

			slider._tickCanvas.Invalidate();
			slider._markerCanvas.Invalidate();
		}
		#endregion
	}
}