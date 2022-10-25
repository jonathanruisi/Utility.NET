using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Brushes;
using Microsoft.Graphics.Canvas.Geometry;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.Graphics.Canvas.UI;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Microsoft.UI;
using Microsoft.UI.Input;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

using Windows.Foundation;
using Windows.UI.Text;

namespace JLR.Utility.WinUI.Controls
{
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

    /// <summary>
    /// Specifies which property is being modified by user interaction.
    /// </summary>
    public enum ValueDragType
    {
        None,
        Position,
        SelectionStart,
        SelectionEnd,
        Selection,
        ZoomStart,
        ZoomEnd,
        Zoom
    }

    /// <summary>
    /// Specifies the unit to which various timeline elements will snap.
    /// </summary>
    public enum SnapInterval
    {
        MinorTick,
        MajorTick,
        Frame,
        TenthSecond,
        QuarterSecond,
        HalfSecond,
        Second,
        Minute,
        Hour,
        Day
    }
    #endregion

    #region Interfaces
    /// <summary>
    /// Represents a type which indicates a point in time (or span of time)
    /// on a <see cref="MediaTimeline"/>.
    /// </summary>
    public interface ITimelineMarker
    {
        /// <summary>
        /// Friendly name for the marker
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Position (in seconds) of the start of the marker
        /// </summary>
        decimal Position { get; }

        /// <summary>
        /// Amount of time (in seconds) covered by the marker.
        /// This can be equal to zero.
        /// </summary>
        decimal Duration { get; }

        /// <summary>
        /// Identifies the category/track to which the marker belongs.
        /// </summary>
        int Group { get; }

        /// <summary>
        /// Gets the name of the <see cref="MarkerStyleGroup"/> which
        /// defines the appearance of this <see cref="ITimelineMarker"/>.
        /// </summary>
        string Style { get; }
    }
    #endregion

    [TemplatePart(Name = "PART_TimelineCanvas", Type = typeof(CanvasControl))]
    [TemplatePart(Name = "PART_Position",       Type = typeof(TransportElement))]
    [TemplatePart(Name = "PART_SelectionStart", Type = typeof(TransportElement))]
    [TemplatePart(Name = "PART_SelectionEnd",   Type = typeof(TransportElement))]
    [TemplatePart(Name = "PART_SelectionThumb", Type = typeof(TransportElement))]
    [TemplatePart(Name = "PART_ZoomStart",      Type = typeof(TransportElement))]
    [TemplatePart(Name = "PART_ZoomEnd",        Type = typeof(TransportElement))]
    [TemplatePart(Name = "PART_ZoomThumb",      Type = typeof(TransportElement))]
    public sealed class MediaTimeline : Control
    {
        #region Constants
        private const int DecimalPrecision = 8;
        public static readonly decimal MinimumVisibleRange = 1.0M;
        private const int SecondsPerMinute = 60;
        private const int SecondsPerHour = 3600;
        private const int SecondsPerDay = 86400;
        private const int MinutesPerHour = 60;
        private const int HoursPerDay = 24;
        #endregion

        #region Fields
        private bool _wasCtrlKeyPressed;
        private bool _isBoundaryUpdateInProgress;
        private bool _blockSelectionChangedEvent,
                     _blockZoomChangedEvent;
        private double _leftMouseStartX;
        private ValueDragType _lastUsedTransportControl;
        private Panel _mainPanel, _zoomPanel;
        private CanvasControl _timelineCanvas;

        private TransportElement
            _positionElement,
            _selectionStartElement,
            _selectionEndElement,
            _selectionThumbElement,
            _zoomStartElement,
            _zoomEndElement,
            _zoomThumbElement;

        private ICanvasBrush
            _tickAreaBackgroundBrush,
            _markerAreaBackgroundBrush,
            _trackAreaBackgroundBrush,
            _originTickBrush,
            _majorTickBrush,
            _minorTickBrush,
            _selectionHighlightBrush;

        private readonly List<(decimal start, decimal end)> _selections;
        private readonly LinkedList<(int major, int minor, int subdivisionCount)> _intervals;
        private LinkedListNode<(int major, int minor, int subdivisionCount)> _currentInterval;
        #endregion

        #region Constructor
        public MediaTimeline()
        {
            DefaultStyleKey = typeof(MediaTimeline);
            _intervals = new LinkedList<(int major, int minor, int subdivisionCount)>();
            _selections = new List<(decimal start, decimal end)>();

            MarkerStyleGroups = new Dictionary<string, MarkerStyleGroup>();

            Markers = new ObservableCollection<ITimelineMarker>();
            Markers.CollectionChanged += Markers_CollectionChanged;

            Loaded += MediaTimeline_Loaded;
            Unloaded += MediaTimeline_Unloaded;
            PointerWheelChanged += MediaTimeline_PointerWheelChanged;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the total duration of the timeline.
        /// </summary>
        public TimeSpan Duration
        {
            get => TimeSpan.FromSeconds(decimal.ToDouble(End - Start));
            set
            {
                var newEnd = decimal.Round(Start + (decimal)value.TotalSeconds, DecimalPrecision);
                if (ZoomEnd > newEnd)
                    ZoomEnd = newEnd;
                End = newEnd;
            }
        }

        /// <summary>
        /// Gets or sets the duration of the current zoom region.
        /// </summary>
        /// <remarks>
        /// Unless necessary, the current time around which the zoom region is centered will not change.
        /// </remarks>
        public TimeSpan VisibleDuration
        {
            get => TimeSpan.FromSeconds(decimal.ToDouble(ZoomEnd - ZoomStart));
            set
            {
                var newDuration = decimal.Round((decimal)value.TotalSeconds, DecimalPrecision);

                if (newDuration < MinimumVisibleRange)
                    newDuration = MinimumVisibleRange;
                else if (newDuration >= End - Start)
                {
                    SetVisibleWindow(Start, End);
                    return;
                }

                var center = decimal.Round(ZoomStart + ((ZoomEnd - ZoomStart) / 2), DecimalPrecision);
                var start = decimal.Round(center - (newDuration / 2), DecimalPrecision);

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

        /// <summary>
        /// Gets the duration of one frame.
        /// </summary>
        public TimeSpan FrameDuration => TimeSpan.FromSeconds(1D / FramesPerSecond);

        /// <summary>
        /// Gets the read-only list of selected time intervals.
        /// </summary>
        /// <remarks>
        /// Use the <see cref="AddSelection"/>, <see cref="AddCurrentSelection"/>,
        /// <see cref="RemoveSelection"/>, and <see cref="RemoveCurrentSelection"/>
        /// methods to modify this list.
        /// </remarks>
        public ReadOnlyCollection<(decimal start, decimal end)> Selections
        {
            get => new(_selections);
        }

        public Dictionary<string, MarkerStyleGroup> MarkerStyleGroups { get; }
        #endregion

        #region Dependency Properties
        #region General

        /// <summary>
        /// Gets or sets the number of frames per second.
        /// This should match the FPS of the media
        /// represented in the timeline.
        /// </summary>
        /// <remarks>
        /// For images or other static content, it may
        /// be appropriate to set this value to match
        /// the display's refresh rate.
        /// </remarks>
        public int FramesPerSecond
        {
            get => (int)GetValue(FramesPerSecondProperty);
            set => SetValue(FramesPerSecondProperty, value);
        }

        public static readonly DependencyProperty FramesPerSecondProperty =
            DependencyProperty.Register("FramesPerSecond",
                                        typeof(int),
                                        typeof(MediaTimeline),
                                        new PropertyMetadata(30, OnFramesPerSecondChanged));

        /// <summary>
        /// Gets or sets the current position on the timeline,
        /// in seconds.
        /// </summary>
        public decimal Position
        {
            get => (decimal)GetValue(PositionProperty);
            set => SetValue(PositionProperty, value);
        }

        public static readonly DependencyProperty PositionProperty =
            DependencyProperty.Register("Position",
                                        typeof(decimal),
                                        typeof(MediaTimeline),
                                        new PropertyMetadata(0.0M, OnPositionChanged));

        /// <summary>
        /// Gets or sets the start of the timeline, in seconds.
        /// This is usually always <b>zero</b>, but can be
        /// changed for situations where the timeline must
        /// start at a non-zero value.
        /// </summary>
        public decimal Start
        {
            get => (decimal)GetValue(StartProperty);
            set => SetValue(StartProperty, value);
        }

        public static readonly DependencyProperty StartProperty =
            DependencyProperty.Register("Start",
                                        typeof(decimal),
                                        typeof(MediaTimeline),
                                        new PropertyMetadata(0.0M, OnStartChanged));

        /// <summary>
        /// Gets or sets the end of the timeline, in seconds.
        /// If <see cref="Start"/> = 0, this value will be
        /// equal to <see cref="Duration"/>.
        /// </summary>
        public decimal End
        {
            get => (decimal)GetValue(EndProperty);
            set => SetValue(EndProperty, value);
        }

        public static readonly DependencyProperty EndProperty =
            DependencyProperty.Register("End",
                                        typeof(decimal),
                                        typeof(MediaTimeline),
                                        new PropertyMetadata(10.0M, OnEndChanged));

        /// <summary>
        /// Gets or sets the start of the visible portion
        /// of the timeline, in seconds.
        /// </summary>
        public decimal ZoomStart
        {
            get => (decimal)GetValue(ZoomStartProperty);
            set => SetValue(ZoomStartProperty, value);
        }

        public static readonly DependencyProperty ZoomStartProperty =
            DependencyProperty.Register("ZoomStart",
                                        typeof(decimal),
                                        typeof(MediaTimeline),
                                        new PropertyMetadata(0.0M, OnZoomStartChanged));

        /// <summary>
        /// Gets or sets the end of the visible portion
        /// of the timeline, in seconds.
        /// </summary>
        public decimal ZoomEnd
        {
            get => (decimal)GetValue(ZoomEndProperty);
            set => SetValue(ZoomEndProperty, value);
        }

        public static readonly DependencyProperty ZoomEndProperty =
            DependencyProperty.Register("ZoomEnd",
                                        typeof(decimal),
                                        typeof(MediaTimeline),
                                        new PropertyMetadata(10.0M, OnZoomEndChanged));

        /// <summary>
        /// Gets or sets the start of the active
        /// selection range, in seconds.
        /// </summary>
        public decimal SelectionStart
        {
            get => (decimal)GetValue(SelectionStartProperty);
            set => SetValue(SelectionStartProperty, value);
        }

        public static readonly DependencyProperty SelectionStartProperty =
            DependencyProperty.Register("SelectionStart",
                                        typeof(decimal),
                                        typeof(MediaTimeline),
                                        new PropertyMetadata(0.0M, OnSelectionStartChanged));

        /// <summary>
        /// Gets or sets the end of the active
        /// selection range, in seconds.
        /// </summary>
        public decimal SelectionEnd
        {
            get => (decimal)GetValue(SelectionEndProperty);
            set => SetValue(SelectionEndProperty, value);
        }

        public static readonly DependencyProperty SelectionEndProperty =
            DependencyProperty.Register("SelectionEnd",
                                        typeof(decimal),
                                        typeof(MediaTimeline),
                                        new PropertyMetadata(0.0M, OnSelectionEndChanged));

        /// <summary>
        /// Gets a list of all markers in the timeline.
        /// </summary>
        /// <remarks>
        /// This collection is observable, and is capable
        /// of two-way binding to UI elements.
        /// </remarks>
        public ObservableCollection<ITimelineMarker> Markers
        {
            get => (ObservableCollection<ITimelineMarker>)GetValue(MarkersProperty);
            private set => SetValue(MarkersProperty, value);
        }

        public static readonly DependencyProperty MarkersProperty =
            DependencyProperty.Register("Markers",
                                        typeof(ObservableCollection<ITimelineMarker>),
                                        typeof(MediaTimeline),
                                        new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets a reference to the currently selected timeline marker.
        /// </summary>
        public ITimelineMarker SelectedMarker
        {
            get => (ITimelineMarker)GetValue(SelectedMarkerProperty);
            set => SetValue(SelectedMarkerProperty, value);
        }

        public static readonly DependencyProperty SelectedMarkerProperty =
            DependencyProperty.Register("SelectedMarker",
                                        typeof(ITimelineMarker),
                                        typeof(MediaTimeline),
                                        new PropertyMetadata(null, OnSelectedMarkerChanged));

        /// <summary>
        /// Gets the number of different tracks present in
        /// <see cref="Markers"/>.
        /// </summary>
        public int TrackCount
        {
            get => (int)GetValue(TrackCountProperty);
            private set => SetValue(TrackCountProperty, value);
        }

        public static readonly DependencyProperty TrackCountProperty =
            DependencyProperty.Register("TrackCount",
                                        typeof(int),
                                        typeof(MediaTimeline),
                                        new PropertyMetadata(0));
        #endregion

        #region Behavior
        /// <summary>
        /// Gets or sets a value indicating whether or not
        /// the <see cref="Position"/> can be adjusted
        /// by dragging the position thumb, or by clicking
        /// a specific time on the timeline.
        /// <para>
        /// If <c>true</c>, the position can be interactively
        /// controlled by the user in the UI.
        /// If <c>false</c>, <see cref="Position"/> can only
        /// be set directly in code, or by data binding.
        /// </para>
        /// </summary>
        public bool IsPositionAdjustmentEnabled
        {
            get => (bool)GetValue(IsPositionAdjustmentEnabledProperty);
            set => SetValue(IsPositionAdjustmentEnabledProperty, value);
        }

        public static readonly DependencyProperty IsPositionAdjustmentEnabledProperty =
            DependencyProperty.Register("IsPositionAdjustmentEnabled",
                                        typeof(bool),
                                        typeof(MediaTimeline),
                                        new PropertyMetadata(true, OnIsAdjustmentEnabledChanged));

        /// <summary>
        /// Gets or sets a value indicating whether or not
        /// the active selection range can be adjusted by
        /// dragging the selection start/end thumbs or the
        /// selection highlight area between the thumbs.
        /// <para>
        /// If <c>true</c>, the selection can be interactively
        /// controlled by the user in the UI.
        /// If <c>false</c>, the active selection range can only
        /// be set directly in code, or by data binding.
        /// </para>
        /// </summary>
        public bool IsSelectionAdjustmentEnabled
        {
            get => (bool)GetValue(IsSelectionAdjustmentEnabledProperty);
            set => SetValue(IsSelectionAdjustmentEnabledProperty, value);
        }

        public static readonly DependencyProperty IsSelectionAdjustmentEnabledProperty =
            DependencyProperty.Register("IsSelectionAdjustmentEnabled",
                                        typeof(bool),
                                        typeof(MediaTimeline),
                                        new PropertyMetadata(true, OnIsAdjustmentEnabledChanged));

        public bool IsSelectionEnabled
        {
            get => (bool)GetValue(IsSelectionEnabledProperty);
            set => SetValue(IsSelectionEnabledProperty, value);
        }

        public static readonly DependencyProperty IsSelectionEnabledProperty =
            DependencyProperty.Register("IsSelectionEnabled",
                                        typeof(bool),
                                        typeof(MediaTimeline),
                                        new PropertyMetadata(false, OnIsSelectionEnabledChanged));

        public bool IsZoomAdjustmentEnabled
        {
            get => (bool)GetValue(IsZoomAdjustmentEnabledProperty);
            set => SetValue(IsZoomAdjustmentEnabledProperty, value);
        }

        public static readonly DependencyProperty IsZoomAdjustmentEnabledProperty =
            DependencyProperty.Register("IsZoomAdjustmentEnabled",
                                        typeof(bool),
                                        typeof(MediaTimeline),
                                        new PropertyMetadata(true, OnIsAdjustmentEnabledChanged));

        public FollowMode PositionFollowMode
        {
            get => (FollowMode)GetValue(PositionFollowModeProperty);
            set => SetValue(PositionFollowModeProperty, value);
        }

        public static readonly DependencyProperty PositionFollowModeProperty =
            DependencyProperty.Register("PositionFollowMode",
                                        typeof(FollowMode),
                                        typeof(MediaTimeline),
                                        new PropertyMetadata(FollowMode.NoFollow));

        public SnapInterval SnapToNearest
        {
            get => (SnapInterval)GetValue(SnapToNearestProperty);
            set => SetValue(SnapToNearestProperty, value);
        }

        public static readonly DependencyProperty SnapToNearestProperty =
            DependencyProperty.Register("SnapToNearest",
                                        typeof(SnapInterval),
                                        typeof(MediaTimeline),
                                        new PropertyMetadata(SnapInterval.MinorTick));

        public double MajorTickClutterThreshold
        {
            get => (double)GetValue(MajorTickClutterThresholdProperty);
            set => SetValue(MajorTickClutterThresholdProperty, value);
        }

        public static readonly DependencyProperty MajorTickClutterThresholdProperty =
            DependencyProperty.Register("MajorTickClutterThreshold",
                                        typeof(double),
                                        typeof(MediaTimeline),
                                        new PropertyMetadata(5.0, OnTimelineCanvasRenderPropertyChanged));

        public double MinorTickClutterThreshold
        {
            get => (double)GetValue(MinorTickClutterThresholdProperty);
            set => SetValue(MinorTickClutterThresholdProperty, value);
        }

        public static readonly DependencyProperty MinorTickClutterThresholdProperty =
            DependencyProperty.Register("MinorTickClutterThreshold",
                                        typeof(double),
                                        typeof(MediaTimeline),
                                        new PropertyMetadata(5.0, OnTimelineCanvasRenderPropertyChanged));

        public InputSystemCursorShape CursorShape
        {
            get => (InputSystemCursorShape)GetValue(CursorShapeProperty);
            set => SetValue(CursorShapeProperty, value);
        }

        public static readonly DependencyProperty CursorShapeProperty =
            DependencyProperty.Register("CursorShape",
                                        typeof(InputSystemCursorShape),
                                        typeof(MediaTimeline),
                                        new PropertyMetadata(InputSystemCursorShape.Arrow, OnCursorShapePropertyChanged));

        #region Alignment
        public VerticalAlignment TickAlignment
        {
            get => (VerticalAlignment)GetValue(TickAlignmentProperty);
            set => SetValue(TickAlignmentProperty, value);
        }

        public static readonly DependencyProperty TickAlignmentProperty =
            DependencyProperty.Register("TickAlignment",
                                        typeof(VerticalAlignment),
                                        typeof(MediaTimeline),
                                        new PropertyMetadata(VerticalAlignment.Bottom, OnTimelineCanvasRenderPropertyChanged));

        public VerticalAlignment PositionElementAlignment
        {
            get => (VerticalAlignment)GetValue(PositionElementAlignmentProperty);
            set => SetValue(PositionElementAlignmentProperty, value);
        }

        public static readonly DependencyProperty PositionElementAlignmentProperty =
            DependencyProperty.Register("PositionElementAlignment",
                                        typeof(VerticalAlignment),
                                        typeof(MediaTimeline),
                                        new PropertyMetadata(VerticalAlignment.Top, OnTransportElementChanged));

        public VerticalAlignment SelectionInOutElementsAlignment
        {
            get => (VerticalAlignment)GetValue(SelectionInOutElementsAlignmentProperty);
            set => SetValue(SelectionInOutElementsAlignmentProperty, value);
        }

        public static readonly DependencyProperty SelectionInOutElementsAlignmentProperty =
            DependencyProperty.Register("SelectionInOutElementsAlignment",
                                        typeof(VerticalAlignment),
                                        typeof(MediaTimeline),
                                        new PropertyMetadata(VerticalAlignment.Top, OnTransportElementChanged));

        public VerticalAlignment SelectionThumbElementAlignment
        {
            get => (VerticalAlignment)GetValue(SelectionThumbElementAlignmentProperty);
            set => SetValue(SelectionThumbElementAlignmentProperty, value);
        }

        public static readonly DependencyProperty SelectionThumbElementAlignmentProperty =
            DependencyProperty.Register("SelectionThumbElementAlignment",
                                        typeof(VerticalAlignment),
                                        typeof(MediaTimeline),
                                        new PropertyMetadata(VerticalAlignment.Top, OnTransportElementChanged));

        public VerticalAlignment SelectionHighlightAlignment
        {
            get => (VerticalAlignment)GetValue(SelectionHighlightAlignmentProperty);
            set => SetValue(SelectionHighlightAlignmentProperty, value);
        }

        public static readonly DependencyProperty SelectionHighlightAlignmentProperty =
            DependencyProperty.Register("SelectionHighlightAlignment",
                                        typeof(VerticalAlignment),
                                        typeof(MediaTimeline),
                                        new PropertyMetadata(VerticalAlignment.Center, OnTimelineCanvasRenderPropertyChanged));
        #endregion

        #region Sizing
        public double SpaceRequiredToShowAllTracks
        {
            get => TrackAreaRect.Height;
        }

        public GridLength ZoomBarHeight
        {
            get => (GridLength)GetValue(ZoomBarHeightProperty);
            set => SetValue(ZoomBarHeightProperty, value);
        }

        public static readonly DependencyProperty ZoomBarHeightProperty =
            DependencyProperty.Register("ZoomBarHeight",
                                        typeof(GridLength),
                                        typeof(MediaTimeline),
                                        new PropertyMetadata(new GridLength(0.5, GridUnitType.Star)));

        public double MarkerBarHeight
        {
            get => (double)GetValue(MarkerBarHeightProperty);
            set => SetValue(MarkerBarHeightProperty, value);
        }

        public static readonly DependencyProperty MarkerBarHeightProperty =
            DependencyProperty.Register("MarkerBarHeight",
                                        typeof(double),
                                        typeof(MediaTimeline),
                                        new PropertyMetadata(25.0, OnTimelineCanvasRenderPropertyChanged));

        public double TrackHeight
        {
            get => (double)GetValue(TrackHeightProperty);
            set => SetValue(TrackHeightProperty, value);
        }

        public static readonly DependencyProperty TrackHeightProperty =
            DependencyProperty.Register("TrackHeight",
                                        typeof(double),
                                        typeof(MediaTimeline),
                                        new PropertyMetadata(15.0, OnTimelineCanvasRenderPropertyChanged));

        public double TrackSpacing
        {
            get => (double)GetValue(TrackSpacingProperty);
            set => SetValue(TrackSpacingProperty, value);
        }

        public static readonly DependencyProperty TrackSpacingProperty =
            DependencyProperty.Register("TrackSpacing",
                                        typeof(double),
                                        typeof(MediaTimeline),
                                        new PropertyMetadata(2.0, OnTimelineCanvasRenderPropertyChanged));

        public double PositionElementRelativeHeight
        {
            get => (double)GetValue(PositionElementRelativeHeightProperty);
            set => SetValue(PositionElementRelativeHeightProperty, value);
        }

        public static readonly DependencyProperty PositionElementRelativeHeightProperty =
            DependencyProperty.Register("PositionElementRelativeHeight",
                                        typeof(double),
                                        typeof(MediaTimeline),
                                        new PropertyMetadata(1.0, OnTransportElementChanged));

        public double SelectionInOutElementsRelativeHeight
        {
            get => (double)GetValue(SelectionInOutElementsRelativeHeightProperty);
            set => SetValue(SelectionInOutElementsRelativeHeightProperty, value);
        }

        public static readonly DependencyProperty SelectionInOutElementsRelativeHeightProperty =
            DependencyProperty.Register("SelectionInOutElementsRelativeHeight",
                                        typeof(double),
                                        typeof(MediaTimeline),
                                        new PropertyMetadata(1.0, OnTransportElementChanged));

        public double SelectionThumbElementRelativeHeight
        {
            get => (double)GetValue(SelectionThumbElementRelativeHeightProperty);
            set => SetValue(SelectionThumbElementRelativeHeightProperty, value);
        }

        public static readonly DependencyProperty SelectionThumbElementRelativeHeightProperty =
            DependencyProperty.Register("SelectionThumbElementRelativeHeight",
                                        typeof(double),
                                        typeof(MediaTimeline),
                                        new PropertyMetadata(1.0, OnTransportElementChanged));

        public double SelectionHighlightRelativeHeight
        {
            get => (double)GetValue(SelectionHighlightRelativeHeightProperty);
            set => SetValue(SelectionHighlightRelativeHeightProperty, value);
        }

        public static readonly DependencyProperty SelectionHighlightRelativeHeightProperty =
            DependencyProperty.Register("SelectionHighlightRelativeHeight",
                                        typeof(double),
                                        typeof(MediaTimeline),
                                        new PropertyMetadata(1.0, OnTimelineCanvasRenderPropertyChanged));

        public double OriginTickRelativeHeight
        {
            get => (double)GetValue(OriginTickRelativeHeightProperty);
            set => SetValue(OriginTickRelativeHeightProperty, value);
        }

        public static readonly DependencyProperty OriginTickRelativeHeightProperty =
            DependencyProperty.Register("OriginTickRelativeHeight",
                                        typeof(double),
                                        typeof(MediaTimeline),
                                        new PropertyMetadata(1.0, OnTimelineCanvasRenderPropertyChanged));

        public double MajorTickRelativeHeight
        {
            get => (double)GetValue(MajorTickRelativeHeightProperty);
            set => SetValue(MajorTickRelativeHeightProperty, value);
        }

        public static readonly DependencyProperty MajorTickRelativeHeightProperty =
            DependencyProperty.Register("MajorTickRelativeHeight",
                                        typeof(double),
                                        typeof(MediaTimeline),
                                        new PropertyMetadata(1.0, OnTimelineCanvasRenderPropertyChanged));

        public double MinorTickRelativeHeight
        {
            get => (double)GetValue(MinorTickRelativeHeightProperty);
            set => SetValue(MinorTickRelativeHeightProperty, value);
        }

        public static readonly DependencyProperty MinorTickRelativeHeightProperty =
            DependencyProperty.Register("MinorTickRelativeHeight",
                                        typeof(double),
                                        typeof(MediaTimeline),
                                        new PropertyMetadata(1.0, OnTimelineCanvasRenderPropertyChanged));

        public double OriginTickThickness
        {
            get => (double)GetValue(OriginTickThicknessProperty);
            set => SetValue(OriginTickThicknessProperty, value);
        }

        public static readonly DependencyProperty OriginTickThicknessProperty =
            DependencyProperty.Register("OriginTickThickness",
                                        typeof(double),
                                        typeof(MediaTimeline),
                                        new PropertyMetadata(1.0, OnTimelineCanvasRenderPropertyChanged));

        public double MajorTickThickness
        {
            get => (double)GetValue(MajorTickThicknessProperty);
            set => SetValue(MajorTickThicknessProperty, value);
        }

        public static readonly DependencyProperty MajorTickThicknessProperty =
            DependencyProperty.Register("MajorTickThickness",
                                        typeof(double),
                                        typeof(MediaTimeline),
                                        new PropertyMetadata(1.0, OnTimelineCanvasRenderPropertyChanged));

        public double MinorTickThickness
        {
            get => (double)GetValue(MinorTickThicknessProperty);
            set => SetValue(MinorTickThicknessProperty, value);
        }

        public static readonly DependencyProperty MinorTickThicknessProperty =
            DependencyProperty.Register("MinorTickThickness",
                                        typeof(double),
                                        typeof(MediaTimeline),
                                        new PropertyMetadata(1.0, OnTimelineCanvasRenderPropertyChanged));
        #endregion

        #region Z-Index
        public int OriginTickZIndex
        {
            get => (int)GetValue(OriginTickZIndexProperty);
            set => SetValue(OriginTickZIndexProperty, value);
        }

        public static readonly DependencyProperty OriginTickZIndexProperty =
            DependencyProperty.Register("OriginTickZIndex",
                                        typeof(int),
                                        typeof(MediaTimeline),
                                        new PropertyMetadata(0, OnTimelineCanvasRenderPropertyChanged));

        public int MajorTickZIndex
        {
            get => (int)GetValue(MajorTickZIndexProperty);
            set => SetValue(MajorTickZIndexProperty, value);
        }

        public static readonly DependencyProperty MajorTickZIndexProperty =
            DependencyProperty.Register("MajorTickZIndex",
                                        typeof(int),
                                        typeof(MediaTimeline),
                                        new PropertyMetadata(0, OnTimelineCanvasRenderPropertyChanged));

        public int MinorTickZIndex
        {
            get => (int)GetValue(MinorTickZIndexProperty);
            set => SetValue(MinorTickZIndexProperty, value);
        }

        public static readonly DependencyProperty MinorTickZIndexProperty =
            DependencyProperty.Register("MinorTickZIndex",
                                        typeof(int),
                                        typeof(MediaTimeline),
                                        new PropertyMetadata(0, OnTimelineCanvasRenderPropertyChanged));
        #endregion

        #region Alternate Font
        public FontFamily AlternateFontFamily
        {
            get => (FontFamily)GetValue(AlternateFontFamilyProperty);
            set => SetValue(AlternateFontFamilyProperty, value);
        }

        public static readonly DependencyProperty AlternateFontFamilyProperty =
            DependencyProperty.Register("AlternateFontFamily",
                                        typeof(FontFamily),
                                        typeof(MediaTimeline),
                                        new PropertyMetadata(null, OnTimelineCanvasRenderPropertyChanged));

        public double AlternateFontSize
        {
            get => (double)GetValue(AlternateFontSizeProperty);
            set => SetValue(AlternateFontSizeProperty, value);
        }

        public static readonly DependencyProperty AlternateFontSizeProperty =
            DependencyProperty.Register("AlternateFontSize",
                                        typeof(double),
                                        typeof(MediaTimeline),
                                        new PropertyMetadata(0.0, OnTimelineCanvasRenderPropertyChanged));

        public FontStretch AlternateFontStretch
        {
            get => (FontStretch)GetValue(AlternateFontStretchProperty);
            set => SetValue(AlternateFontStretchProperty, value);
        }

        public static readonly DependencyProperty AlternateFontStretchProperty =
            DependencyProperty.Register("AlternateFontStretch",
                                        typeof(FontStretch),
                                        typeof(MediaTimeline),
                                        new PropertyMetadata(FontStretch.Normal, OnTimelineCanvasRenderPropertyChanged));

        public FontStyle AlternateFontStyle
        {
            get => (FontStyle)GetValue(AlternateFontStyleProperty);
            set => SetValue(AlternateFontStyleProperty, value);
        }

        public static readonly DependencyProperty AlternateFontStyleProperty =
            DependencyProperty.Register("AlternateFontStyle",
                                        typeof(FontStyle),
                                        typeof(MediaTimeline),
                                        new PropertyMetadata(FontStyle.Normal, OnTimelineCanvasRenderPropertyChanged));

        public FontWeight AlternateFontWeight
        {
            get => (FontWeight)GetValue(AlternateFontWeightProperty);
            set => SetValue(AlternateFontWeightProperty, value);
        }

        public static readonly DependencyProperty AlternateFontWeightProperty =
            DependencyProperty.Register("AlternateFontWeight",
                                        typeof(FontWeight),
                                        typeof(MediaTimeline),
                                        new PropertyMetadata(FontWeights.Normal, OnTimelineCanvasRenderPropertyChanged));
        #endregion

        #region Brushes
        public Brush TickAreaBackground
        {
            get => (Brush)GetValue(TickAreaBackgroundProperty);
            set => SetValue(TickAreaBackgroundProperty, value);
        }

        public static readonly DependencyProperty TickAreaBackgroundProperty =
            DependencyProperty.Register("TickAreaBackground",
                                        typeof(Brush),
                                        typeof(MediaTimeline),
                                        new PropertyMetadata(null, OnTimelineCanvasBrushChanged));

        public Brush MarkerAreaBackground
        {
            get => (Brush)GetValue(MarkerAreaBackgroundProperty);
            set => SetValue(MarkerAreaBackgroundProperty, value);
        }

        public static readonly DependencyProperty MarkerAreaBackgroundProperty =
            DependencyProperty.Register("MarkerAreaBackground",
                                        typeof(Brush),
                                        typeof(MediaTimeline),
                                        new PropertyMetadata(null, OnTimelineCanvasBrushChanged));

        public Brush TrackAreaBackground
        {
            get => (Brush)GetValue(TrackAreaBackgroundProperty);
            set => SetValue(TrackAreaBackgroundProperty, value);
        }

        public static readonly DependencyProperty TrackAreaBackgroundProperty =
            DependencyProperty.Register("TrackAreaBackground",
                                        typeof(Brush),
                                        typeof(MediaTimeline),
                                        new PropertyMetadata(null, OnTimelineCanvasBrushChanged));

        public Brush ZoomAreaBackground
        {
            get => (Brush)GetValue(ZoomAreaBackgroundProperty);
            set => SetValue(ZoomAreaBackgroundProperty, value);
        }

        public static readonly DependencyProperty ZoomAreaBackgroundProperty =
            DependencyProperty.Register("ZoomAreaBackground",
                                        typeof(Brush),
                                        typeof(MediaTimeline),
                                        new PropertyMetadata(null));

        public Brush SelectionHighlightBrush
        {
            get => (Brush)GetValue(SelectionHighlightBrushProperty);
            set => SetValue(SelectionHighlightBrushProperty, value);
        }

        public static readonly DependencyProperty SelectionHighlightBrushProperty =
            DependencyProperty.Register("SelectionHighlightBrush",
                                        typeof(Brush),
                                        typeof(MediaTimeline),
                                        new PropertyMetadata(null, OnTimelineCanvasBrushChanged));

        public Brush OriginTickBrush
        {
            get => (Brush)GetValue(OriginTickBrushProperty);
            set => SetValue(OriginTickBrushProperty, value);
        }

        public static readonly DependencyProperty OriginTickBrushProperty =
            DependencyProperty.Register("OriginTickBrush",
                                        typeof(Brush),
                                        typeof(MediaTimeline),
                                        new PropertyMetadata(null, OnTimelineCanvasBrushChanged));

        public Brush MajorTickBrush
        {
            get => (Brush)GetValue(MajorTickBrushProperty);
            set => SetValue(MajorTickBrushProperty, value);
        }

        public static readonly DependencyProperty MajorTickBrushProperty =
            DependencyProperty.Register("MajorTickBrush",
                                        typeof(Brush),
                                        typeof(MediaTimeline),
                                        new PropertyMetadata(null, OnTimelineCanvasBrushChanged));

        public Brush MinorTickBrush
        {
            get => (Brush)GetValue(MinorTickBrushProperty);
            set => SetValue(MinorTickBrushProperty, value);
        }

        public static readonly DependencyProperty MinorTickBrushProperty =
            DependencyProperty.Register("MinorTickBrush",
                                        typeof(Brush),
                                        typeof(MediaTimeline),
                                        new PropertyMetadata(null, OnTimelineCanvasBrushChanged));
        #endregion
        #endregion

        #region Template
        public ControlTemplate PositionElementTemplate
        {
            get => (ControlTemplate)GetValue(PositionElementTemplateProperty);
            set => SetValue(PositionElementTemplateProperty, value);
        }

        public static readonly DependencyProperty PositionElementTemplateProperty =
            DependencyProperty.Register("PositionElementTemplate",
                                        typeof(ControlTemplate),
                                        typeof(MediaTimeline),
                                        new PropertyMetadata(null, OnTransportElementChanged));

        public ControlTemplate SelectionStartElementTemplate
        {
            get => (ControlTemplate)GetValue(SelectionStartElementTemplateProperty);
            set => SetValue(SelectionStartElementTemplateProperty, value);
        }

        public static readonly DependencyProperty SelectionStartElementTemplateProperty =
            DependencyProperty.Register("SelectionStartElementTemplate",
                                        typeof(ControlTemplate),
                                        typeof(MediaTimeline),
                                        new PropertyMetadata(null, OnTransportElementChanged));

        public ControlTemplate SelectionEndElementTemplate
        {
            get => (ControlTemplate)GetValue(SelectionEndElementTemplateProperty);
            set => SetValue(SelectionEndElementTemplateProperty, value);
        }

        public static readonly DependencyProperty SelectionEndElementTemplateProperty =
            DependencyProperty.Register("SelectionEndElementTemplate",
                                        typeof(ControlTemplate),
                                        typeof(MediaTimeline),
                                        new PropertyMetadata(null, OnTransportElementChanged));

        public ControlTemplate SelectionThumbElementTemplate
        {
            get => (ControlTemplate)GetValue(SelectionThumbElementTemplateProperty);
            set => SetValue(SelectionThumbElementTemplateProperty, value);
        }

        public static readonly DependencyProperty SelectionThumbElementTemplateProperty =
            DependencyProperty.Register("SelectionThumbElementTemplate",
                                        typeof(ControlTemplate),
                                        typeof(MediaTimeline),
                                        new PropertyMetadata(null, OnTransportElementChanged));

        public ControlTemplate ZoomStartElementTemplate
        {
            get => (ControlTemplate)GetValue(ZoomStartElementTemplateProperty);
            set => SetValue(ZoomStartElementTemplateProperty, value);
        }

        public static readonly DependencyProperty ZoomStartElementTemplateProperty =
            DependencyProperty.Register("ZoomStartElementTemplate",
                                        typeof(ControlTemplate),
                                        typeof(MediaTimeline),
                                        new PropertyMetadata(null, OnTransportElementChanged));

        public ControlTemplate ZoomEndElementTemplate
        {
            get => (ControlTemplate)GetValue(ZoomEndElementTemplateProperty);
            set => SetValue(ZoomEndElementTemplateProperty, value);
        }

        public static readonly DependencyProperty ZoomEndElementTemplateProperty =
            DependencyProperty.Register("ZoomEndElementTemplate",
                                        typeof(ControlTemplate),
                                        typeof(MediaTimeline),
                                        new PropertyMetadata(null, OnTransportElementChanged));

        public ControlTemplate ZoomThumbElementTemplate
        {
            get => (ControlTemplate)GetValue(ZoomThumbElementTemplateProperty);
            set => SetValue(ZoomThumbElementTemplateProperty, value);
        }

        public static readonly DependencyProperty ZoomThumbElementTemplateProperty =
            DependencyProperty.Register("ZoomThumbElementTemplate",
                                        typeof(ControlTemplate),
                                        typeof(MediaTimeline),
                                        new PropertyMetadata(null, OnTransportElementChanged));
        #endregion
        #endregion

        #region Events
        #region General
        public event EventHandler<(decimal start, decimal end)> DurationChanged;
        public event EventHandler<(decimal start, decimal end, bool isEnabled)> SelectionChanged;
        public event EventHandler<(decimal start, decimal end)> ZoomChanged;
        public event EventHandler<decimal> PositionChanged;
        public event EventHandler<int> FramesPerSecondChanged;
        public event EventHandler<ITimelineMarker> SelectedMarkerChanged;
        public event NotifyCollectionChangedEventHandler SelectionListChanged;
        public event EventHandler<int> TrackCountChanged;

        private void RaiseDurationChanged()
        {
            var handler = DurationChanged;
            handler?.Invoke(this, (Start, End));
        }

        private void RaiseSelectionChanged()
        {
            if (_blockSelectionChangedEvent)
                return;

            var handler = SelectionChanged;
            handler?.Invoke(this, (SelectionStart, SelectionEnd, IsSelectionEnabled));
        }

        private void RaiseZoomChanged()
        {
            if (_blockZoomChangedEvent)
                return;

            var handler = ZoomChanged;
            handler?.Invoke(this, (ZoomStart, ZoomEnd));
        }

        private void RaisePositionChanged()
        {
            var handler = PositionChanged;
            handler?.Invoke(this, Position);
        }

        private void RaiseFramesPerSecondChanged()
        {
            var handler = FramesPerSecondChanged;
            handler?.Invoke(this, FramesPerSecond);
        }

        private void RaiseSelectedMarkerChanged()
        {
            var handler = SelectedMarkerChanged;
            handler?.Invoke(this, SelectedMarker);
        }

        /// <summary>
        /// Raises the <see cref="SelectionListChanged"/> event.
        /// </summary>
        /// <param name="oldItems">List of added selections.</param>
        /// <param name="newItems">List of removed/replaced selections.</param>
        private void RaiseSelectionListChanged(IList<(decimal start, decimal end)> oldItems,
                                               IList<(decimal start, decimal end)> newItems)
        {
            var handler = SelectionListChanged;
            handler?.Invoke(this, new NotifyCollectionChangedEventArgs(
                NotifyCollectionChangedAction.Replace, newItems, oldItems));
        }

        /// <summary>
        /// Raises the <see cref="SelectionListChanged"/> event.
        /// </summary>
        /// <param name="action">
        /// <see cref="NotifyCollectionChangedAction.Add"/>,
        /// <see cref="NotifyCollectionChangedAction.Remove"/>,
        /// and <see cref="NotifyCollectionChangedAction.Reset"/> only.
        /// </param>
        /// <param name="changedItems">List of added or removed (possibly cleared) selections.</param>
        private void RaiseSelectionListChanged(NotifyCollectionChangedAction action,
                                               IList<(decimal start, decimal end)> changedItems = null)
        {
            var handler = SelectionListChanged;
            handler?.Invoke(this, new NotifyCollectionChangedEventArgs(action, changedItems));
        }

        private void RaiseTrackCountChanged()
        {
            var handler = TrackCountChanged;
            handler?.Invoke(this, TrackCount);
        }
        #endregion

        #region Input
        public event EventHandler<ValueDragType> TimelineValueDragStarted;
        public event EventHandler<ValueDragType> TimelineValueDragCompleted;

        private void RaiseDragStarted(ValueDragType dragType)
        {
            var handler = TimelineValueDragStarted;
            handler?.Invoke(this, dragType);
        }

        private void RaiseDragCompleted(ValueDragType dragType)
        {
            var handler = TimelineValueDragCompleted;
            handler?.Invoke(this, dragType);
        }
        #endregion
        #endregion

        #region Public Methods
        /// <summary>
        /// Adds the currently selected timespan to the selections list.
        /// </summary>
        public void AddCurrentSelection()
        {
            if (!IsSelectionEnabled || SelectionEnd - SelectionStart == 0)
                return;

            AddSelection(SelectionStart, SelectionEnd);
        }

        /// <summary>
        /// Removes the currently selected timespan from the selections list.
        /// </summary>
        public void RemoveCurrentSelection()
        {
            if (!IsSelectionEnabled || SelectionEnd - SelectionStart == 0)
                return;

            RemoveSelection(SelectionStart, SelectionEnd);
        }

        /// <summary>
        /// Adds a specified timespan to the selections list.
        /// </summary>
        /// <param name="start">The start time.</param>
        /// <param name="end">The end time.</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public void AddSelection(decimal start, decimal end)
        {
            if (start < Start || start > End)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(start),
                    $"The specified value must be between {Start:#.###} and {End:#.###}");
            }

            if (end < Start || end > End)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(end),
                    $"The specified value must be between {Start:#.###} and {End:#.###}");
            }

            if (start >= end)
            {
                throw new ArgumentException($"The specified value must be < {end:#.###}", nameof(start));
            }

            // Specified selection is a subset of an existing selection - do nothing.
            if (Selections.Any(x => x.start <= start && x.end >= end))
                return;

            var addedSelections = new List<(decimal start, decimal end)>();
            var removedSelections = new List<(decimal start, decimal end)>();

            // An existing selection overlaps with the specified start value.
            // Adjust the specified start value to include the existing selection,
            // then remove the existing selection.
            var overlap = Selections.Where(x => x.start < start && x.end >= start);
            if (overlap.Any())
            {
                var selection = overlap.First();
                start = selection.start;
                removedSelections.Add(selection);
                _selections.Remove(selection);
            }

            // An existing selection overlaps with the specified end value.
            // Adjust the specified end value to include the existing selection,
            // then remove the existing selection.
            overlap = Selections.Where(x => x.start <= end && x.end > end);
            if (overlap.Any())
            {
                var selection = overlap.First();
                end = selection.end;
                removedSelections.Add(selection);
                _selections.Remove(selection);
            }

            // Remove any existing selections which fall entirely within the specified selection.
            int count = 0;
            foreach (var selection in Selections.Where(x => x.start >= start && x.end <= end))
            {
                removedSelections.Add(selection);
                count++;
            }

            for (var i = removedSelections.Count - count; i < removedSelections.Count; i++)
            {
                _selections.Remove(removedSelections[i]);
            }

            // Add the adjusted selection to the selections list
            var newSelection = (start, end);
            addedSelections.Add(newSelection);
            _selections.Add(newSelection);

            // If the added selection falls anywhere in the current zoom range,
            // redraw the timeline canvas
            if (start < ZoomEnd && end > ZoomStart)
            {
                _timelineCanvas?.Invalidate();
            }

            if (removedSelections.Count > 0)
                RaiseSelectionListChanged(removedSelections, addedSelections);
            else
                RaiseSelectionListChanged(NotifyCollectionChangedAction.Add, addedSelections);
        }

        /// <summary>
        /// Removes a specified timespan from the selections list.
        /// </summary>
        /// <param name="start">The start time.</param>
        /// <param name="end">The end time.</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public void RemoveSelection(decimal start, decimal end)
        {
            if (start < Start || start > End)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(start),
                    $"The specified value must be between {Start:#.###} and {End:#.###}");
            }

            if (end < Start || end > End)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(end),
                    $"The specified value must be between {Start:#.###} and {End:#.###}");
            }

            if (start >= end)
            {
                throw new ArgumentException($"The specified value must be < {end:#.###}", nameof(start));
            }

            // Specified selection does not overlap with any existing selection - do nothing.
            if (!Selections.Any(x => x.start <= end && x.end >= start))
                return;

            var addedSelections = new List<(decimal start, decimal end)>();
            var removedSelections = new List<(decimal start, decimal end)>();

            // Remove any existing selections which fall entirely within the specified selection.
            int count = 0;
            foreach (var selection in Selections.Where(x => x.start >= start && x.end <= end))
            {
                removedSelections.Add(selection);
                count++;
            }

            for (var i = removedSelections.Count - count; i < removedSelections.Count; i++)
            {
                _selections.Remove(removedSelections[i]);
            }

            // Specified selection is a subset of an existing selection.
            // Remove the existing selection and create new selections
            // representing the remaining sections of the original.
            var overlap = Selections.Where(x => x.start <= start && x.end >= end);
            if (overlap.Any())
            {
                var selection = overlap.First();
                if (selection.start != start)
                {
                    var newLeft = (selection.start, start);
                    addedSelections.Add(newLeft);
                    _selections.Add(newLeft);
                }

                if (selection.end != end)
                {
                    var newRight = (end, selection.end);
                    addedSelections.Add(newRight);
                    _selections.Add(newRight);
                }

                removedSelections.Add(selection);
                _selections.Remove(selection);
            }

            // An existing selection overlaps with the specified start value.
            // Trim the existing selection back to the specified start value.
            overlap = Selections.Where(x => x.start < start && x.end > start);
            if (overlap.Any())
            {
                var selection = overlap.First();
                var newSelection = (selection.start, start);
                removedSelections.Add(selection);
                _selections.Remove(selection);
                addedSelections.Add(newSelection);
                _selections.Add(newSelection);
            }

            // An existing selection overlaps with the specified end value.
            // Trim the existing selection forward to the specified end value.
            overlap = Selections.Where(x => x.start < end && x.end > end);
            if (overlap.Any())
            {
                var selection = overlap.First();
                var newSelection = (end, selection.end);
                removedSelections.Add(selection);
                _selections.Remove(selection);
                addedSelections.Add(newSelection);
                _selections.Add(newSelection);
            }

            // If the removed selection falls anywhere in the current zoom range,
            // redraw the timeline canvas
            if (start < ZoomEnd && end > ZoomStart)
            {
                _timelineCanvas?.Invalidate();
            }

            if (addedSelections.Count > 0)
                RaiseSelectionListChanged(removedSelections, addedSelections);
            else
                RaiseSelectionListChanged(NotifyCollectionChangedAction.Remove, removedSelections);
        }

        /// <summary>
        /// Removes all selections from the selections list.
        /// </summary>
        public void ClearSelections()
        {
            var redrawNeeded = _selections.Any(x => x.start < ZoomEnd && x.end > ZoomStart);
            _selections.Clear();

            if (redrawNeeded)
                _timelineCanvas?.Invalidate();

            RaiseSelectionListChanged(NotifyCollectionChangedAction.Reset);
        }

        /// <summary>
        /// Zoom out to the full duration of the timeline.
        /// </summary>
        public void ZoomOutFull()
        {
            SetVisibleWindow(Start, End);
        }

        /// <summary>
        /// Centers the current zoom region around a specified time.
        /// </summary>
        /// <param name="position">A position on the timeline.</param>
        /// <exception cref="ArgumentOutOfRangeException"/>
        public void CenterVisibleWindow(decimal position)
        {
            if (!IsValidVisibleRange)
                return;

            if (position < Start || position > End)
                throw new ArgumentOutOfRangeException(
                    nameof(position),
                    $"The specified value must be between {Start:#.###} and {End:#.###}");

            var amount = position - (ZoomStart + ((ZoomEnd - ZoomStart) / 2));
            OffsetVisibleWindow(amount);
        }

        /// <summary>
        /// Shifts the current zoom region by a specific amount of time.
        /// </summary>
        /// <param name="offset">The offset by which the zoom region is shifted.</param>
        public void OffsetVisibleWindow(decimal offset)
        {
            if (!IsValidVisibleRange)
                return;

            var currentWindow = ZoomEnd - ZoomStart;

            if (offset < 0 && ZoomStart > Start)
            {
                _blockZoomChangedEvent = true;
                ZoomStart = Math.Max(ZoomStart + offset, Start);
                _blockZoomChangedEvent = false;
                ZoomEnd = ZoomStart + currentWindow;
            }
            else if (offset > 0 && ZoomEnd < End)
            {
                _blockZoomChangedEvent = true;
                ZoomEnd = Math.Min(ZoomEnd + offset, End);
                _blockZoomChangedEvent = false;
                ZoomStart = ZoomEnd - currentWindow;
            }
        }

        /// <summary>
        /// Sets the current zoom region to a specified time range.
        /// </summary>
        /// <param name="start">Point in time where the visible zoom region starts.</param>
        /// <param name="end">Point in time where the visible zoom region ends.</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public void SetVisibleWindow(decimal start, decimal end)
        {
            if (start < Start || start > End)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(start),
                    $"The specified value must be between {Start:#.###} and {End:#.###}");
            }

            if (end < Start || end > End)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(end),
                    $"The specified value must be between {Start:#.###} and {End:#.###}");
            }

            if (start >= end)
            {
                throw new ArgumentException($"The specified value must be < {end:#.###}", nameof(start));
            }

            if (Math.Abs(end - start) < MinimumVisibleRange)
            {
                throw new ArgumentOutOfRangeException(
                    $"The visible window defined by {nameof(start)} and {nameof(end)} " +
                    $"must be greater than or equal to {MinimumVisibleRange}");
            }

            if (ZoomStart > start && ZoomEnd < end ||
                ZoomStart < start && ZoomEnd > end ||
                ZoomEnd >= start + MinimumVisibleRange)
            {
                _blockZoomChangedEvent = true;
                ZoomStart = start;
                _blockZoomChangedEvent = false;
                ZoomEnd = end;
            }
            else
            {
                _blockZoomChangedEvent = true;
                ZoomEnd = end;
                _blockZoomChangedEvent = false;
                ZoomStart = start;
            }
        }

        /// <summary>
        /// Returns a point in time closest to the specified time, constrained by a specified time interval.
        /// </summary>
        /// <param name="relativeTo">A position on the timeline.</param>
        /// <param name="mustBeVisible">
        /// If <c>true</c>, the returned value will fall within the current zoom region even if the specified value does not.
        /// If <c>false</c>, the current zoom region is ignored.
        /// </param>
        /// <param name="snapInterval">Constrains the set of possible return values.</param>
        /// <returns></returns>
        public decimal GetNearestSnapValue(decimal relativeTo, bool mustBeVisible, SnapInterval snapInterval)
        {
            decimal newValue;

            if (mustBeVisible)
            {
                if (!IsValidVisibleRange)
                    throw new ArgumentException(
                        $"{nameof(mustBeVisible)} = true, but the current zoom region is invalid.");

                if (relativeTo < ZoomStart)
                    relativeTo = ZoomStart;
                else if (relativeTo > ZoomEnd)
                    relativeTo = ZoomEnd;
            }

            var value = Math.Abs(relativeTo);
            var interval = GetSnapIntervalValue(snapInterval);
            var offsetToward = value % interval;
            var offsetAway = interval - offsetToward;

            if (offsetToward == 0)
                return decimal.Round(relativeTo, DecimalPrecision);

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

        public void SetSelectionFromMarker(ITimelineMarker marker)
        {
            if (marker == null)
                throw new ArgumentNullException(nameof(marker));

            _blockSelectionChangedEvent = true;
            IsSelectionEnabled = true;
            SelectionStart = marker.Position;
            _blockSelectionChangedEvent = false;
            SelectionEnd = marker.Position + marker.Duration;
        }

        public T GetClosestMarkerBeforeCurrentPosition<T>(decimal minDistance = 0.0M)
            where T : class, ITimelineMarker
        {
            var markers = from marker in Markers.OfType<T>()
                          where Position > marker.Position + minDistance
                          orderby Position - marker.Position
                          select marker;

            return markers.Any() ? markers.First() : null;
        }

        public T GetClosestMarkerAfterCurrentPosition<T>(decimal minDistance = 0.0M)
            where T : class, ITimelineMarker
        {
            var markers = from marker in Markers.OfType<T>()
                          where Position < marker.Position - minDistance
                          orderby marker.Position - Position
                          select marker;

            return markers.Any() ? markers.First() : null;
        }

        public void Reset()
        {
            Position = 0;
            SelectionStart = 0;
            SelectionEnd = 0;
            Markers.Clear();
            Start = 0;
            End = MinimumVisibleRange;
            ClearSelections();
            ZoomOutFull();

            IsPositionAdjustmentEnabled = false;
            IsZoomAdjustmentEnabled = false;
            IsSelectionAdjustmentEnabled = false;
            IsSelectionEnabled = false;
            PositionFollowMode = FollowMode.Advance;
            SnapToNearest = SnapInterval.Frame;
        }
        #endregion

        #region Dependency Property Callbacks
        #region General
        private static void OnStartChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not MediaTimeline timeline || timeline._isBoundaryUpdateInProgress)
                return;

            timeline._isBoundaryUpdateInProgress = true;
            timeline.AdjustEnd();
            if (timeline.AdjustSelectionStart() || timeline.AdjustSelectionEnd())
            {
                if (timeline.IsSelectionEnabled)
                    timeline.RaiseSelectionChanged();
            }
            if (timeline.AdjustZoomStart() || timeline.AdjustZoomEnd())
                timeline.RaiseZoomChanged();
            if (timeline.AdjustPosition())
                timeline.RaisePositionChanged();
            timeline._isBoundaryUpdateInProgress = false;

            timeline._timelineCanvas?.Invalidate();
            timeline.UpdateSelectionElementLayout();
            timeline.UpdateZoomElementLayout();
            timeline.UpdatePositionElementLayout();
            timeline.RaiseDurationChanged();
        }

        private static void OnEndChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not MediaTimeline timeline || timeline._isBoundaryUpdateInProgress)
                return;

            timeline._isBoundaryUpdateInProgress = true;
            timeline.AdjustStart();
            if (timeline.AdjustSelectionStart() || timeline.AdjustSelectionEnd())
            {
                if (timeline.IsSelectionEnabled)
                    timeline.RaiseSelectionChanged();
            }
            if (timeline.AdjustZoomStart() || timeline.AdjustZoomEnd())
                timeline.RaiseZoomChanged();
            if (timeline.AdjustPosition())
                timeline.RaisePositionChanged();
            timeline._isBoundaryUpdateInProgress = false;

            timeline._timelineCanvas?.Invalidate();
            timeline.UpdateSelectionElementLayout();
            timeline.UpdateZoomElementLayout();
            timeline.UpdatePositionElementLayout();
            timeline.RaiseDurationChanged();
        }

        private static void OnSelectionStartChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not MediaTimeline timeline || timeline._isBoundaryUpdateInProgress)
                return;

            timeline._isBoundaryUpdateInProgress = true;
            timeline.AdjustSelectionStart();
            timeline._isBoundaryUpdateInProgress = false;

            if (timeline.IsSelectionEnabled)
            {
                timeline.UpdateSelectionElementLayout();
                timeline.RaiseSelectionChanged();
            }
        }

        private static void OnSelectionEndChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not MediaTimeline timeline || timeline._isBoundaryUpdateInProgress)
                return;

            timeline._isBoundaryUpdateInProgress = true;
            timeline.AdjustSelectionEnd();
            timeline._isBoundaryUpdateInProgress = false;

            if (timeline.IsSelectionEnabled)
            {
                timeline.UpdateSelectionElementLayout();
                timeline.RaiseSelectionChanged();
            }
        }

        private static void OnZoomStartChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not MediaTimeline timeline || timeline._isBoundaryUpdateInProgress)
                return;

            timeline._isBoundaryUpdateInProgress = true;
            timeline.AdjustZoomStart();
            timeline._isBoundaryUpdateInProgress = false;

            timeline._timelineCanvas?.Invalidate();
            timeline.UpdateSelectionElementLayout();
            timeline.UpdateZoomElementLayout();
            timeline.UpdatePositionElementLayout();
            timeline.RaiseZoomChanged();
        }

        private static void OnZoomEndChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not MediaTimeline timeline || timeline._isBoundaryUpdateInProgress)
                return;

            timeline._isBoundaryUpdateInProgress = true;
            timeline.AdjustZoomEnd();
            timeline._isBoundaryUpdateInProgress = false;

            timeline._timelineCanvas?.Invalidate();
            timeline.UpdateSelectionElementLayout();
            timeline.UpdateZoomElementLayout();
            timeline.UpdatePositionElementLayout();
            timeline.RaiseZoomChanged();
        }

        private static void OnPositionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not MediaTimeline timeline || timeline._isBoundaryUpdateInProgress)
                return;

            timeline._isBoundaryUpdateInProgress = true;
            timeline.AdjustPosition();
            timeline._isBoundaryUpdateInProgress = false;

            timeline.UpdatePositionElementLayout();
            timeline.RaisePositionChanged();
            timeline.FollowPosition();
        }

        private static void OnFramesPerSecondChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not MediaTimeline timeline || !timeline.IsLoaded)
                return;

            timeline.InitializeTimescale();
            timeline._timelineCanvas?.Invalidate();
            timeline.RaiseFramesPerSecondChanged();
        }

        private static void OnSelectedMarkerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not MediaTimeline timeline)
                return;

            timeline._timelineCanvas?.Invalidate();
            timeline.RaiseSelectedMarkerChanged();
        }
        #endregion

        #region Behavior
        private static void OnIsAdjustmentEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not MediaTimeline timeline)
                return;

            if (e.Property == IsPositionAdjustmentEnabledProperty && timeline._positionElement != null)
            {
                timeline._positionElement.IsEnabled = (bool)e.NewValue;
            }
            else if (e.Property == IsSelectionAdjustmentEnabledProperty)
            {
                if (timeline._selectionStartElement != null)
                    timeline._selectionStartElement.IsEnabled = (bool)e.NewValue;
                if (timeline._selectionEndElement != null)
                    timeline._selectionEndElement.IsEnabled = (bool)e.NewValue;
            }
            else if (e.Property == IsZoomAdjustmentEnabledProperty)
            {
                if (timeline._zoomStartElement != null)
                    timeline._zoomStartElement.IsEnabled = (bool)e.NewValue;
                if (timeline._zoomEndElement != null)
                    timeline._zoomEndElement.IsEnabled = (bool)e.NewValue;
                if (timeline._zoomThumbElement != null)
                    timeline._zoomThumbElement.IsEnabled = (bool)e.NewValue;
            }
        }

        private static void OnIsSelectionEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not MediaTimeline timeline)
                return;

            timeline.UpdateSelectionElementLayout();
            timeline.RaiseSelectionChanged();
        }

        private static void OnCursorShapePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not MediaTimeline timeline || !timeline.IsLoaded)
                return;

            timeline.ProtectedCursor = InputSystemCursor.Create(timeline.CursorShape);
        }
        #endregion

        #region Template
        private static void OnTransportElementChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not MediaTimeline timeline)
                return;

            if (e.Property == PositionElementAlignmentProperty ||
                e.Property == PositionElementRelativeHeightProperty ||
                e.Property == PositionElementTemplateProperty)
            {
                timeline.UpdatePositionElementLayout();
            }
            else if (e.Property == SelectionInOutElementsAlignmentProperty ||
                     e.Property == SelectionInOutElementsRelativeHeightProperty ||
                     e.Property == SelectionThumbElementAlignmentProperty ||
                     e.Property == SelectionThumbElementRelativeHeightProperty ||
                     e.Property == SelectionStartElementTemplateProperty ||
                     e.Property == SelectionEndElementTemplateProperty ||
                     e.Property == SelectionThumbElementTemplateProperty)
            {
                timeline.UpdateSelectionElementLayout();
            }
            else if (e.Property == ZoomStartElementTemplateProperty ||
                     e.Property == ZoomEndElementTemplateProperty ||
                     e.Property == ZoomThumbElementTemplateProperty)
            {
                timeline.UpdateZoomElementLayout();
            }

            if (e.Property == PositionElementTemplateProperty ||
                e.Property == SelectionStartElementTemplateProperty ||
                e.Property == SelectionEndElementTemplateProperty ||
                e.Property == ZoomStartElementTemplateProperty ||
                e.Property == ZoomEndElementTemplateProperty)
            {
                timeline.AdjustPadding();
            }
        }
        #endregion

        #region Render Properties
        private static void OnTimelineCanvasRenderPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not MediaTimeline timeline)
                return;

            timeline._timelineCanvas?.Invalidate();
        }

        private static void OnTimelineCanvasBrushChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // TODO: Implement asynchronous resource loading for Win2D
        }
        #endregion
        #endregion

        #region Layout Method Overrides
        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (GetTemplateChild("PART_TimelineCanvas") is CanvasControl canvas)
            {
                _timelineCanvas                  = canvas;
                _timelineCanvas.Draw            += CanvasControl_Draw;
                _timelineCanvas.CreateResources += CanvasControl_CreateResources;

                if (VisualTreeHelper.GetParent(canvas) is Panel parent)
                {
                    _mainPanel                     = parent;
                    _mainPanel.SizeChanged        += MainPanel_SizeChanged;
                    _mainPanel.PointerPressed     += MainPanel_PointerPressed;
                    _mainPanel.PointerCaptureLost += MainPanel_PointerCaptureLost;
                }
            }

            if (GetTemplateChild("PART_Position") is TransportElement position)
            {
                _positionElement                     = position;
                _positionElement.SizeChanged        += TransportElement_SizeChanged;
                _positionElement.PointerEntered     += MainPanelElement_PointerEntered;
                _positionElement.PointerPressed     += TransportElement_PointerPressed;
                _positionElement.PointerCaptureLost += TransportElement_PointerCaptureLost;
                _positionElement.PointerMoved       += MainPanelElement_PointerMoved;
            }

            if (GetTemplateChild("PART_SelectionStart") is TransportElement selectionStart)
            {
                _selectionStartElement                     = selectionStart;
                _selectionStartElement.SizeChanged        += TransportElement_SizeChanged;
                _selectionStartElement.PointerEntered     += MainPanelElement_PointerEntered;
                _selectionStartElement.PointerPressed     += TransportElement_PointerPressed;
                _selectionStartElement.PointerCaptureLost += TransportElement_PointerCaptureLost;
                _selectionStartElement.PointerMoved       += MainPanelElement_PointerMoved;
            }

            if (GetTemplateChild("PART_SelectionEnd") is TransportElement selectionEnd)
            {
                _selectionEndElement                     = selectionEnd;
                _selectionEndElement.SizeChanged        += TransportElement_SizeChanged;
                _selectionEndElement.PointerEntered     += MainPanelElement_PointerEntered;
                _selectionEndElement.PointerPressed     += TransportElement_PointerPressed;
                _selectionEndElement.PointerCaptureLost += TransportElement_PointerCaptureLost;
                _selectionEndElement.PointerMoved       += MainPanelElement_PointerMoved;
            }

            if (GetTemplateChild("PART_SelectionThumb") is TransportElement selectionThumb)
            {
                _selectionThumbElement                     = selectionThumb;
                _selectionThumbElement.PointerPressed     += TransportElement_PointerPressed;
                _selectionThumbElement.PointerCaptureLost += TransportElement_PointerCaptureLost;
                _selectionThumbElement.PointerMoved       += MainPanelElement_PointerMoved;
            }

            if (GetTemplateChild("PART_ZoomStart") is TransportElement zoomStart)
            {
                _zoomStartElement                     = zoomStart;
                _zoomStartElement.SizeChanged        += TransportElement_SizeChanged;
                _zoomStartElement.PointerPressed     += TransportElement_PointerPressed;
                _zoomStartElement.PointerCaptureLost += TransportElement_PointerCaptureLost;
                _zoomStartElement.PointerMoved       += ZoomPanelElement_PointerMoved;
            }

            if (GetTemplateChild("PART_ZoomEnd") is TransportElement zoomEnd)
            {
                _zoomEndElement                     = zoomEnd;
                _zoomEndElement.SizeChanged        += TransportElement_SizeChanged;
                _zoomEndElement.PointerPressed     += TransportElement_PointerPressed;
                _zoomEndElement.PointerCaptureLost += TransportElement_PointerCaptureLost;
                _zoomEndElement.PointerMoved       += ZoomPanelElement_PointerMoved;
            }

            if (GetTemplateChild("PART_ZoomThumb") is TransportElement zoomThumb)
            {
                _zoomThumbElement = zoomThumb;
                if (VisualTreeHelper.GetParent(zoomThumb) is Panel parent)
                {
                    _zoomPanel                      = parent;
                    _zoomPanel.SizeChanged         += ZoomPanel_SizeChanged;
                    _zoomPanel.PointerPressed      += ZoomPanel_PointerPressed;
                    _zoomPanel.PointerCaptureLost  += ZoomPanel_PointerCaptureLost;
                }

                _zoomThumbElement.PointerPressed     += TransportElement_PointerPressed;
                _zoomThumbElement.PointerCaptureLost += TransportElement_PointerCaptureLost;
                _zoomThumbElement.PointerMoved       += ZoomPanelElement_PointerMoved;
            }
        }
        #endregion

        #region Event Handlers
        #region General
        private void MediaTimeline_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeTimescale();
            AdjustPadding();
            UpdatePositionElementLayout();
            UpdateSelectionElementLayout();
            UpdateZoomElementLayout();

            ProtectedCursor = InputSystemCursor.Create(CursorShape);
        }

        private void MediaTimeline_Unloaded(object sender, RoutedEventArgs e)
        {
            // CanvasControl needs to properly dispose of resources to avoid memory leaks
            _timelineCanvas?.RemoveFromVisualTree();
            _timelineCanvas = null;
        }

        private void MainPanel_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_timelineCanvas != null)
            {
                _timelineCanvas.Width = e.NewSize.Width;
                _timelineCanvas.Height = e.NewSize.Height;
            }

            UpdatePositionElementLayout();
            UpdateSelectionElementLayout();
        }

        private void ZoomPanel_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateZoomElementLayout();
        }

        private void TransportElement_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (Math.Abs(e.PreviousSize.Width - e.NewSize.Width) > double.Epsilon)
                AdjustPadding();
        }
        #endregion

        #region Input
        #region Control
        private void MediaTimeline_PointerWheelChanged(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            var delta = e.GetCurrentPoint(this).Properties.MouseWheelDelta / 120;

            if (IsCtrlKeyPressed)
            {
                var visibleDurationIncrement = Duration / 50;
                VisibleDuration -= visibleDurationIncrement * delta;

                if (IsShiftKeyPressed && _lastUsedTransportControl != ValueDragType.None)
                {
                    // Zoom on and center last used timeline control (if CTRL is pressed)
                    if (_lastUsedTransportControl == ValueDragType.Position)
                        CenterVisibleWindow(Position);
                    else if (_lastUsedTransportControl == ValueDragType.SelectionStart && IsSelectionEnabled)
                        CenterVisibleWindow(SelectionStart);
                    else if (_lastUsedTransportControl == ValueDragType.SelectionEnd && IsSelectionEnabled)
                        CenterVisibleWindow(SelectionEnd);
                    else if (_lastUsedTransportControl == ValueDragType.Selection && IsSelectionEnabled)
                        CenterVisibleWindow(SelectionStart + ((SelectionEnd - SelectionStart) / 2M));
                }
                else
                {
                    // Zoom on center of current visible timeline segment
                    CenterVisibleWindow(ZoomStart + ((ZoomEnd - ZoomStart) / 2M));
                }
            }
            else
            {
                // Scroll current visible timeline segment forward or backward in time
                if (IsShiftKeyPressed)
                    OffsetVisibleWindow(delta * GetSnapIntervalValue(SnapInterval.MinorTick));
                else
                    OffsetVisibleWindow(delta * GetSnapIntervalValue(SnapInterval.MajorTick));
            }
        }
        #endregion

        #region Panel
        private void MainPanel_PointerPressed(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (!IsValidVisibleRange || (!IsPositionAdjustmentEnabled && !IsSelectionAdjustmentEnabled))
                return;

            var point = e.GetCurrentPoint(_mainPanel);
            var pos = ConvertScreenCoordinateToPosition(point.Position.X);
            var snapPos = GetNearestSnapValue(pos, true, SnapToNearest);
            var halfSnap = GetSnapIntervalValue(SnapInterval.MinorTick) / 2;

            if (point.Properties.PointerUpdateKind != PointerUpdateKind.LeftButtonPressed)
                return;

            _wasCtrlKeyPressed = IsCtrlKeyPressed;

            var closestMarkers = from marker in Markers
                                 where (marker.Duration == 0 &&
                                        marker.Position >= pos - halfSnap &&
                                        marker.Position <= pos + halfSnap) ||
                                       (marker.Duration > 0 &&
                                        marker.Position <= pos &&
                                        marker.Position + marker.Duration >= pos)
                                 orderby Math.Min(Math.Abs(marker.Position - pos), Math.Abs(marker.Position + marker.Duration - pos))
                                 select marker;

            if (MarkerAreaRect.Contains(point.Position))
            {
                var closestMarker = closestMarkers.Where(x => x.Duration == 0M).FirstOrDefault();

                if (_wasCtrlKeyPressed && closestMarker != null)
                    SetSelectionFromMarker(closestMarker);
                else
                    SelectedMarker = closestMarker;
            }
            else if (TrackAreaRect.Contains(point.Position))
            {
                // Determine which track was clicked
                int track = 0;
                var trackTopCoord = TrackAreaRect.Top;
                for (; track < TrackCount; track++)
                {
                    if (point.Position.Y >= trackTopCoord &&
                        point.Position.Y <= trackTopCoord + TrackHeight)
                        break;

                    trackTopCoord += TrackHeight + TrackSpacing;
                }

                var closestMarker = closestMarkers.Where(x => x.Duration > 0 && x.Group == track).FirstOrDefault();

                if (_wasCtrlKeyPressed && closestMarker != null)
                    SetSelectionFromMarker(closestMarker);
                else
                    SelectedMarker = closestMarker;
            }
            else if (TickAreaRect.Contains(point.Position))
            {
                if (_wasCtrlKeyPressed && IsSelectionAdjustmentEnabled)
                {
                    _lastUsedTransportControl = ValueDragType.Selection;
                    _blockSelectionChangedEvent = true;
                    IsSelectionEnabled = true;
                    SelectionStart = snapPos;
                    _blockSelectionChangedEvent = false;
                    SelectionEnd = snapPos;
                }
                else if (!_wasCtrlKeyPressed && IsPositionAdjustmentEnabled)
                {
                    _lastUsedTransportControl = ValueDragType.Position;
                    _mainPanel.CapturePointer(e.Pointer);
                    RaiseDragStarted(ValueDragType.Position);
                    var closestMarker = closestMarkers.FirstOrDefault();

                    if (closestMarker != null && Math.Abs(closestMarker.Position - pos) < Math.Abs(snapPos - pos))
                        Position = closestMarker.Position;
                    else
                        Position = snapPos;
                }
            }
        }

        private void MainPanel_PointerCaptureLost(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (e.GetCurrentPoint(_mainPanel).Properties.PointerUpdateKind ==
                PointerUpdateKind.LeftButtonReleased && !_wasCtrlKeyPressed)
            {
                RaiseDragCompleted(ValueDragType.Position);
            }

            _wasCtrlKeyPressed = false;
        }

        private void ZoomPanel_PointerPressed(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (!IsValidVisibleRange || !IsZoomAdjustmentEnabled)
                return;

            _zoomPanel.CapturePointer(e.Pointer);
        }

        private void ZoomPanel_PointerCaptureLost(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            RaiseDragCompleted(ValueDragType.Zoom);
        }
        #endregion

        #region TransportElement
        private void MainPanelElement_PointerEntered(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            // Prevent this event from bubbling up to the parent Panel
            // (Let the TransportElement handle cursor changes)
            e.Handled = true;
        }

        private void TransportElement_PointerPressed(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            var point = e.GetCurrentPoint(sender as TransportElement);

            if (point.Properties.PointerUpdateKind == PointerUpdateKind.LeftButtonPressed)
            {
                _leftMouseStartX = point.Position.X;

                if (sender.Equals(_positionElement))
                {
                    _lastUsedTransportControl = ValueDragType.Position;
                    RaiseDragStarted(ValueDragType.Position);
                }
                else if (sender.Equals(_selectionStartElement))
                {
                    _wasCtrlKeyPressed = IsCtrlKeyPressed;
                    if (_wasCtrlKeyPressed)
                        IsSelectionEnabled = false;
                    else
                    {
                        _lastUsedTransportControl = ValueDragType.SelectionStart;
                        RaiseDragStarted(ValueDragType.SelectionStart);
                    }
                }
                else if (sender.Equals(_selectionEndElement))
                {
                    _wasCtrlKeyPressed = IsCtrlKeyPressed;
                    if (_wasCtrlKeyPressed)
                        IsSelectionEnabled = false;
                    else
                    {
                        _lastUsedTransportControl = ValueDragType.SelectionEnd;
                        RaiseDragStarted(ValueDragType.SelectionEnd);
                    }
                }
                else if (sender.Equals(_selectionThumbElement))
                {
                    _wasCtrlKeyPressed = IsCtrlKeyPressed;
                    if (_wasCtrlKeyPressed)
                        IsSelectionEnabled = false;
                    else
                    {
                        _lastUsedTransportControl = ValueDragType.Selection;
                        RaiseDragStarted(ValueDragType.Selection);
                    }
                }
                else if (sender.Equals(_zoomStartElement))
                    RaiseDragStarted(ValueDragType.ZoomStart);
                else if (sender.Equals(_zoomEndElement))
                    RaiseDragStarted(ValueDragType.ZoomEnd);
                else if (sender.Equals(_zoomThumbElement))
                    RaiseDragStarted(ValueDragType.Zoom);
            }

            // Prevent this event from bubbling up to the parent Panel
            e.Handled = true;
        }

        private void TransportElement_PointerCaptureLost(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (sender.Equals(_positionElement))
                RaiseDragCompleted(ValueDragType.Position);
            else if (sender.Equals(_selectionStartElement) && !_wasCtrlKeyPressed)
                RaiseDragCompleted(ValueDragType.SelectionStart);
            else if (sender.Equals(_selectionEndElement) && !_wasCtrlKeyPressed)
                RaiseDragCompleted(ValueDragType.SelectionEnd);
            else if (sender.Equals(_selectionThumbElement) && !_wasCtrlKeyPressed)
                RaiseDragCompleted(ValueDragType.Selection);
            else if (sender.Equals(_zoomStartElement))
                RaiseDragCompleted(ValueDragType.ZoomStart);
            else if (sender.Equals(_zoomEndElement))
                RaiseDragCompleted(ValueDragType.ZoomEnd);
            else if (sender.Equals(_zoomThumbElement))
                RaiseDragCompleted(ValueDragType.Zoom);

            // Prevent this event from bubbling up to the parent Panel
            e.Handled = true;
            _wasCtrlKeyPressed = false;
        }

        private void MainPanelElement_PointerMoved(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            var point = e.GetCurrentPoint(sender as TransportElement);
            if (!point.Properties.IsLeftButtonPressed)
                return;

            // Get pointer position.
            // Only continue if the position has changed by more than half of the snap distance
            var snapInterval = GetSnapIntervalValue(SnapToNearest);
            var snapPixels = ConvertTimeIntervalToPixels(snapInterval);
            var pos = point.Position.X - _leftMouseStartX;
            var delta = Math.Abs(pos);

            if (pos == 0 || delta < snapPixels / 2)
                return;

            // Calculate the number of snap intervals by which the value will be adjusted
            var newValue = snapInterval * (int)(delta / snapPixels);

            if (sender.Equals(_positionElement))
            {
                if (pos < 0)
                {
                    newValue = Position - newValue >= ZoomStart
                        ? GetNearestSnapValue(Position - newValue, true, SnapToNearest)
                        : GetNearestSnapValue(ZoomStart, true, SnapToNearest);
                }
                else if (pos > 0)
                {
                    newValue = Position + newValue <= ZoomEnd
                        ? GetNearestSnapValue(Position + newValue, true, SnapToNearest)
                        : GetNearestSnapValue(ZoomEnd, true, SnapToNearest);
                }

                AdjustNewPositionForNearbyMarkerSnap();
                if (IsSelectionEnabled)
                {
                    if (Math.Abs(newValue - SelectionStart) <= GetSnapIntervalValue(SnapInterval.MinorTick))
                        newValue = SelectionStart;
                    if (Math.Abs(newValue - SelectionEnd) <= GetSnapIntervalValue(SnapInterval.MinorTick))
                        newValue = SelectionEnd;
                }

                if (newValue != Position)
                    Position = newValue;
            }
            else if (sender.Equals(_selectionStartElement))
            {
                if (pos < 0)
                {
                    newValue = SelectionStart - newValue >= ZoomStart
                        ? GetNearestSnapValue(SelectionStart - newValue, true, SnapToNearest)
                        : GetNearestSnapValue(ZoomStart, true, SnapToNearest);
                }
                else if (pos > 0)
                {
                    newValue = SelectionStart + newValue <= ZoomEnd
                        ? GetNearestSnapValue(SelectionStart + newValue, true, SnapToNearest)
                        : GetNearestSnapValue(ZoomEnd, true, SnapToNearest);
                }

                AdjustNewPositionForNearbyMarkerSnap();
                if (Math.Abs(newValue - Position) <= GetSnapIntervalValue(SnapInterval.MinorTick))
                    newValue = Position;
                if (newValue != SelectionStart)
                    SelectionStart = newValue;
            }
            else if (sender.Equals(_selectionEndElement))
            {
                if (pos < 0)
                {
                    newValue = SelectionEnd - newValue >= ZoomStart
                        ? GetNearestSnapValue(SelectionEnd - newValue, true, SnapToNearest)
                        : GetNearestSnapValue(ZoomStart, true, SnapToNearest);
                }
                else if (pos > 0)
                {
                    newValue = SelectionEnd + newValue <= ZoomEnd
                        ? GetNearestSnapValue(SelectionEnd + newValue, true, SnapToNearest)
                        : GetNearestSnapValue(ZoomEnd, true, SnapToNearest);
                }

                AdjustNewPositionForNearbyMarkerSnap();
                if (Math.Abs(newValue - Position) <= GetSnapIntervalValue(SnapInterval.MinorTick))
                    newValue = Position;
                if (newValue != SelectionEnd)
                    SelectionEnd = newValue;
            }
            else if (sender.Equals(_selectionThumbElement))
            {
                if (pos < 0)
                {
                    var newStart = SelectionStart - newValue >= ZoomStart
                        ? GetNearestSnapValue(SelectionStart - newValue, true, SnapToNearest)
                        : GetNearestSnapValue(ZoomStart, true, SnapToNearest);
                    var newEnd = SelectionEnd - (SelectionStart - newStart);

                    if (newStart != SelectionStart && newEnd != SelectionEnd)
                    {
                        _blockSelectionChangedEvent = true;
                        SelectionStart = newStart;
                        _blockSelectionChangedEvent = false;
                        SelectionEnd = newEnd;
                    }
                }
                else if (pos > 0)
                {
                    var newEnd = SelectionEnd + newValue <= ZoomEnd
                        ? GetNearestSnapValue(SelectionEnd + newValue, true, SnapToNearest)
                        : GetNearestSnapValue(ZoomEnd, true, SnapToNearest);
                    var newStart = SelectionStart + (newEnd - SelectionEnd);

                    if (newStart != SelectionStart && newEnd != SelectionEnd)
                    {
                        _blockSelectionChangedEvent = true;
                        SelectionStart = newStart;
                        _blockSelectionChangedEvent = false;
                        SelectionEnd = newEnd;
                    }
                }
            }

            // Local function that adjusts position
            // if a nearby marker is within snapping range
            void AdjustNewPositionForNearbyMarkerSnap()
            {
                var snapToMarker =
                    Markers.Where(x => Math.Abs(newValue - x.Position) <= GetSnapIntervalValue(SnapInterval.MinorTick) ||
                                       Math.Abs(newValue - (x.Position + x.Duration)) <= GetSnapIntervalValue(SnapInterval.MinorTick))
                           .OrderBy(x => Math.Min(Math.Abs(newValue - x.Position), Math.Abs(newValue - (x.Position + x.Duration))))
                           .FirstOrDefault();

                if (snapToMarker != null)
                {
                    if (snapToMarker.Duration > 0 &&
                        Math.Abs(newValue - (snapToMarker.Position + snapToMarker.Duration)) < Math.Abs(newValue - snapToMarker.Position))
                        newValue = snapToMarker.Position + snapToMarker.Duration;
                    else newValue = snapToMarker.Position;
                }
            }
        }

        private void ZoomPanelElement_PointerMoved(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            var point = e.GetCurrentPoint(sender as TransportElement);
            if (!point.Properties.IsLeftButtonPressed)
                return;

            // Get pointer position
            // Only continue if the position has changed by more than 0.1 pixels horizontally
            var pos = point.Position.X - _leftMouseStartX;
            if (Math.Abs(pos) < 0.1)
                return;

            // Calculate the adjustment value
            var delta = (decimal)pos * (End - Start) / (decimal)_zoomPanel.ActualWidth;

            if (sender.Equals(_zoomStartElement) &&
                (pos < 0 && ZoomStart > Start || pos > 0 && ZoomStart < End))
            {
                ZoomStart += delta;
            }
            else if (sender.Equals(_zoomEndElement) &&
                (pos < 0 && ZoomEnd > Start || pos > 0 && ZoomEnd < End))
            {
                ZoomEnd += delta;
            }
            else if (sender.Equals(_zoomThumbElement))
            {
                OffsetVisibleWindow(delta);
            }
        }
        #endregion
        #endregion

        #region Collection
        private void Markers_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var prevTrackCount = TrackCount;
            TrackCount = Markers.Where(x => x.Duration > 0).Select(x => x.Group).Distinct().Count();

            if (TrackCount != prevTrackCount)
            {
                _timelineCanvas?.Invalidate();
                UpdatePositionElementLayout();
                UpdateSelectionElementLayout();
                RaiseTrackCountChanged();
                return;
            }

            if (e.NewItems?.Count > 0)
            {
                if (e.NewItems.Cast<ITimelineMarker>().Any(x => (x.Position >= ZoomStart &&
                                                                 x.Position <= ZoomEnd) ||
                                                                (x.Position + x.Duration >= ZoomStart &&
                                                                 x.Position + x.Duration <= ZoomEnd)))
                {
                    _timelineCanvas?.Invalidate();
                    return;
                }
            }

            if (e.OldItems?.Count > 0)
            {
                if (e.OldItems.Cast<ITimelineMarker>().Any(x => (x.Position >= ZoomStart &&
                                                                 x.Position <= ZoomEnd) ||
                                                                (x.Position + x.Duration >= ZoomStart &&
                                                                 x.Position + x.Duration <= ZoomEnd)))
                {
                    _timelineCanvas?.Invalidate();
                }
            }
        }
        #endregion

        #region Rendering
        private void CanvasControl_CreateResources(CanvasControl sender, CanvasCreateResourcesEventArgs args)
        {
            _tickAreaBackgroundBrush     = TickAreaBackground?.CreateCanvasBrush(sender.Device);
            _markerAreaBackgroundBrush   = MarkerAreaBackground?.CreateCanvasBrush(sender.Device);
            _trackAreaBackgroundBrush    = TrackAreaBackground?.CreateCanvasBrush(sender.Device);
            _originTickBrush             = OriginTickBrush?.CreateCanvasBrush(sender.Device);
            _majorTickBrush              = MajorTickBrush?.CreateCanvasBrush(sender.Device);
            _minorTickBrush              = MinorTickBrush?.CreateCanvasBrush(sender.Device);
            _selectionHighlightBrush     = SelectionHighlightBrush?.CreateCanvasBrush(sender.Device);

            // Marker resources
            foreach (var groupEntry in MarkerStyleGroups)
            {
                if (groupEntry.Value is not MarkerStyleGroup group || group.AreResourcesCreated)
                    continue;

                if (group.Style is not null)
                {
                    group.Style.CanvasGeometry = group.Style.Geometry?.CreateCanvasGeometry(sender.Device);
                    group.Style.FillBrush = group.Style.Fill?.CreateCanvasBrush(sender.Device);
                    group.Style.StrokeBrush = group.Style.Stroke?.CreateCanvasBrush(sender.Device);
                    group.Style.TailStrokeBrush = group.Style.TailStroke?.CreateCanvasBrush(sender.Device);

                    if (group.Style.CanvasGeometry != null)
                    {
                        var bounds = group.Style.CanvasGeometry.ComputeBounds();
                        group.Style.RenderTarget = new CanvasRenderTarget(sender.Device,
                            (float)bounds.Width, (float)bounds.Height, sender.Dpi);
                        using CanvasDrawingSession ds = group.Style.RenderTarget.CreateDrawingSession();

                        ds.Clear(Colors.Transparent);
                        if (group.Style.FillBrush != null)
                            ds.FillGeometry(group.Style.CanvasGeometry, group.Style.FillBrush);
                        if (group.Style.StrokeBrush != null && group.Style.StrokeThickness > 0)
                            ds.DrawGeometry(group.Style.CanvasGeometry, group.Style.StrokeBrush);
                    }
                }

                if (group.SelectedStyle is not null)
                {
                    group.SelectedStyle.CanvasGeometry = group.SelectedStyle.Geometry != null
                        ? (group.SelectedStyle.Geometry?.CreateCanvasGeometry(sender.Device))
                        : group.Style.CanvasGeometry;
                    group.SelectedStyle.FillBrush = group.SelectedStyle.Fill?.CreateCanvasBrush(sender.Device);
                    group.SelectedStyle.StrokeBrush = group.SelectedStyle.Stroke?.CreateCanvasBrush(sender.Device);
                    group.SelectedStyle.TailStrokeBrush = group.SelectedStyle.TailStroke?.CreateCanvasBrush(sender.Device);

                    if (group.SelectedStyle.CanvasGeometry != null)
                    {
                        var bounds = group.SelectedStyle.CanvasGeometry.ComputeBounds();
                        group.SelectedStyle.RenderTarget = new CanvasRenderTarget(sender.Device,
                            (float)bounds.Width, (float)bounds.Height, sender.Dpi);
                        using CanvasDrawingSession ds = group.SelectedStyle.RenderTarget.CreateDrawingSession();

                        ds.Clear(Colors.Transparent);
                        if (group.SelectedStyle.FillBrush != null)
                            ds.FillGeometry(group.SelectedStyle.CanvasGeometry, group.SelectedStyle.FillBrush);
                        if (group.SelectedStyle.StrokeBrush != null && group.SelectedStyle.StrokeThickness > 0)
                            ds.DrawGeometry(group.SelectedStyle.CanvasGeometry, group.SelectedStyle.StrokeBrush);
                    }
                }

                if (group.TimespanMarkerStyle is not null)
                {
                    group.TimespanMarkerStyle.SpanFillBrush =
                        group.TimespanMarkerStyle.SpanFill?.CreateCanvasBrush(sender.Device);
                    group.TimespanMarkerStyle.SpanLabelBrush =
                        group.TimespanMarkerStyle.SpanLabel?.CreateCanvasBrush(sender.Device);
                    group.TimespanMarkerStyle.SpanStrokeBrush =
                        group.TimespanMarkerStyle.SpanStroke?.CreateCanvasBrush(sender.Device);
                    group.TimespanMarkerStyle.SpanStartTailStrokeBrush =
                        group.TimespanMarkerStyle.SpanStartTailStroke?.CreateCanvasBrush(sender.Device);
                    group.TimespanMarkerStyle.SpanEndTailStrokeBrush =
                        group.TimespanMarkerStyle.SpanEndTailStroke?.CreateCanvasBrush(sender.Device);
                }

                if (group.SelectedTimespanMarkerStyle is not null)
                {
                    group.SelectedTimespanMarkerStyle.SpanFillBrush =
                        group.SelectedTimespanMarkerStyle.SpanFill?.CreateCanvasBrush(sender.Device);
                    group.SelectedTimespanMarkerStyle.SpanLabelBrush =
                        group.SelectedTimespanMarkerStyle.SpanLabel?.CreateCanvasBrush(sender.Device);
                    group.SelectedTimespanMarkerStyle.SpanStrokeBrush =
                        group.SelectedTimespanMarkerStyle.SpanStroke?.CreateCanvasBrush(sender.Device);
                    group.SelectedTimespanMarkerStyle.SpanStartTailStrokeBrush =
                        group.SelectedTimespanMarkerStyle.SpanStartTailStroke?.CreateCanvasBrush(sender.Device);
                    group.SelectedTimespanMarkerStyle.SpanEndTailStrokeBrush =
                        group.SelectedTimespanMarkerStyle.SpanEndTailStroke?.CreateCanvasBrush(sender.Device);
                }

                group.AreResourcesCreated = group.Style != null ||
                                            group.TimespanMarkerStyle != null;
            }
        }

        private void CanvasControl_Draw(CanvasControl sender, CanvasDrawEventArgs args)
        {
            // Do not proceed with render if the CanvasControl's size is zero
            if (Math.Abs(sender.ActualWidth) < double.Epsilon ||
                Math.Abs(sender.ActualHeight) < double.Epsilon)
                return;

            var markerAreaRect = MarkerAreaRect;
            var trackAreaRect = TrackAreaRect;
            var tickAreaRect = TickAreaRect;

            // Draw backgrounds
            args.DrawingSession.FillRectangle(markerAreaRect, _markerAreaBackgroundBrush);
            args.DrawingSession.FillRectangle(tickAreaRect, _tickAreaBackgroundBrush);
            args.DrawingSession.FillRectangle(trackAreaRect, _trackAreaBackgroundBrush);

            // Stop rendering if the current visible range is invalid
            if (!IsValidVisibleRange)
                return;

            // Calculate vertical axis render coordinates for each tick type
            var verticalCoordsOrigin = CalculateVerticalRenderCoordinates(
                OriginTickRelativeHeight, TickAlignment, ref tickAreaRect);
            var verticalCoordsMajor = CalculateVerticalRenderCoordinates(
                MajorTickRelativeHeight, TickAlignment, ref tickAreaRect);
            var verticalCoordsMinor = CalculateVerticalRenderCoordinates(
                MinorTickRelativeHeight, TickAlignment, ref tickAreaRect);

            // Optimize tick spacing
            _currentInterval = _intervals.First;
            while ((MinorTickSpacing < MinorTickClutterThreshold ||
                    MajorTickSpacing < MajorTickClutterThreshold) &&
                   _currentInterval.Next != null)
            {
                _currentInterval = _currentInterval.Next;
            }

            // Create major and minor tick lists
            var majorTicks = new HashSet<decimal>();
            var minorTicks = new HashSet<decimal>();

            decimal major = 0, minor = 0;
            var minorInterval = GetSnapIntervalValue(SnapInterval.MinorTick);
            var majorInterval = GetSnapIntervalValue(SnapInterval.MajorTick);

            if (majorInterval > 0)
            {
                major = majorInterval * (int)(ZoomStart / majorInterval);
                if (ZoomStart >= 0 && Math.Abs(major - ZoomStart) > 0)
                    major = majorInterval * ((int)(ZoomStart / majorInterval) + 1);
            }

            if (minorInterval > 0)
            {
                minor = minorInterval * (int)(ZoomStart / minorInterval);
                if (ZoomStart >= 0 && Math.Abs(minor - ZoomStart) > 0)
                    minor = minorInterval * ((int)(ZoomStart / minorInterval) + 1);
            }

            var adjMajor = decimal.Round(major, DecimalPrecision);
            var adjMinor = decimal.Round(minor, DecimalPrecision);

            if (majorInterval > 0 && minorInterval > 0) // Both major and minor ticks will be drawn
            {
                while (adjMajor <= ZoomEnd || adjMinor <= ZoomEnd)
                {
                    if (adjMinor < adjMajor)
                        AddMinorTick();
                    else
                        AddMajorTick();
                }
            }
            else if (majorInterval > 0) // Only major ticks will be drawn
            {
                while (adjMajor <= ZoomEnd)
                {
                    AddMajorTick();
                }
            }
            else if (minorInterval > 0) // Only minor ticks will be drawn
            {
                while (adjMinor <= ZoomEnd)
                {
                    AddMinorTick();
                }
            }

            // If overlapping major and minor ticks share the same Z-index,
            // major ticks take precedence.
            if (MajorTickZIndex == MinorTickZIndex)
                minorTicks.ExceptWith(majorTicks);

            // Determine the tick draw order based on Z-index and tick type
            var drawOrder = new SortedSet<KeyValuePair<TickType, int>>(new TickTypeComparer())
            {
                new KeyValuePair<TickType, int>(TickType.Origin, OriginTickZIndex),
                new KeyValuePair<TickType, int>(TickType.Major, MajorTickZIndex),
                new KeyValuePair<TickType, int>(TickType.Minor, MinorTickZIndex)
            };

            // Draw ticks
            foreach (var type in drawOrder)
            {
                switch (type.Key)
                {
                    case TickType.Origin:
                    {
                        using (args.DrawingSession.CreateLayer(1.0f))
                        {
                            DrawTick(CalculateHorizontalRenderCoordinates(0.0M, 0.0M, ref tickAreaRect).x,
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
                                DrawTick(CalculateHorizontalRenderCoordinates(tick, tick, ref tickAreaRect).x,
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
                                DrawTick(CalculateHorizontalRenderCoordinates(tick, tick, ref tickAreaRect).x,
                                         verticalCoordsMinor,
                                         _minorTickBrush,
                                         MinorTickThickness);
                            }
                        }

                        break;
                    }
                }
            }

            // Draw selection highlights
            using (args.DrawingSession.CreateLayer(1.0f))
            {
                foreach (var (start, end) in _selections.Where(x => x.start <= ZoomEnd && x.end >= ZoomStart))
                {
                    var adjustedStart = start >= ZoomStart ? start : ZoomStart;
                    var adjustedEnd = end <= ZoomEnd ? end : ZoomEnd;
                    var (x, width) = CalculateHorizontalRenderCoordinates(adjustedStart,
                                                                          adjustedEnd,
                                                                          ref tickAreaRect);
                    var (y, height) = CalculateVerticalRenderCoordinates(SelectionHighlightRelativeHeight,
                                                                         SelectionHighlightAlignment,
                                                                         ref tickAreaRect);

                    args.DrawingSession.FillRectangle(
                        new Rect(x, y, width, height),
                        _selectionHighlightBrush);
                }
            }

            // Draw active track segments
            using (args.DrawingSession.CreateLayer(1.0f))
            {
                // Calculate top coordinate for each track
                var trackTopCoords = new List<double>();
                if (TrackCount > 0)
                    trackTopCoords.Add(trackAreaRect.Top);
                for (var i = 1; i < TrackCount; i++)
                {
                    trackTopCoords.Add(trackTopCoords[i - 1] + TrackHeight + TrackSpacing);
                }

                // Find visible active track segments
                var visibleSegments = from clip in Markers
                                      where clip.Duration > 0 &&
                                            clip.Position < ZoomEnd &&
                                            clip.Position + clip.Duration > ZoomStart
                                      select clip;

                foreach (var segment in visibleSegments)
                {
                    // Lookup marker style
                    var style = segment == SelectedMarker
                        ? MarkerStyleGroups[segment.Style].SelectedTimespanMarkerStyle
                        : MarkerStyleGroups[segment.Style].TimespanMarkerStyle;

                    // Calculate rectangle where this segment will be drawn
                    var (x, width) = CalculateHorizontalRenderCoordinates(
                        segment.Position > ZoomStart ? segment.Position : ZoomStart,
                        segment.Position + segment.Duration < ZoomEnd
                            ? segment.Position + segment.Duration
                            : ZoomEnd,
                        ref trackAreaRect);

                    var segmentRect = new Rect(x, trackTopCoords[segment.Group], width, TrackHeight);

                    // Draw segment rectangle
                    args.DrawingSession.FillRectangle(segmentRect, style.SpanFillBrush);

                    if (style.SpanStrokeThickness > 0)
                        args.DrawingSession.DrawRectangle(segmentRect,
                            style.SpanStrokeBrush, (float)style.SpanStrokeThickness, style.SpanStrokeStyle);

                    // Draw marker line(s)
                    if (segment.Position >= ZoomStart &&
                        segment.Position <= ZoomEnd &&
                        style.SpanStartTailStrokeThickness > 0)
                    {
                        args.DrawingSession.DrawLine((float)segmentRect.Left, (float)tickAreaRect.Top,
                                                     (float)segmentRect.Left, (float)segmentRect.Top,
                                                     style.SpanEndTailStrokeBrush,
                                                     (float)style.SpanStartTailStrokeThickness,
                                                     style.SpanStartTailStrokeStyle);
                    }

                    if (segment == SelectedMarker &&
                        segment.Position + segment.Duration >= ZoomStart &&
                        segment.Position + segment.Duration <= ZoomEnd &&
                        style.SpanEndTailStrokeThickness > 0)
                    {
                        args.DrawingSession.DrawLine((float)segmentRect.Right, (float)tickAreaRect.Top,
                                                     (float)segmentRect.Right, (float)segmentRect.Top,
                                                     style.SpanEndTailStrokeBrush,
                                                     (float)style.SpanEndTailStrokeThickness,
                                                     style.SpanEndTailStrokeStyle);
                    }

                    // Generate text label for this segment
                    var textFormat = new CanvasTextFormat
                    {
                        FontFamily = FontFamily.Source,
                        FontSize = (float)FontSize,
                        FontStretch = FontStretch,
                        FontStyle = FontStyle,
                        FontWeight = FontWeight,
                        Direction = CanvasTextDirection.LeftToRightThenTopToBottom,
                        LastLineWrapping = false,
                        LineSpacingMode = CanvasLineSpacingMode.Default,
                        LineSpacing = 0,
                        LineSpacingBaseline = 0,
                        HorizontalAlignment = CanvasHorizontalAlignment.Center,
                        VerticalAlignment = CanvasVerticalAlignment.Top,
                        WordWrapping = CanvasWordWrapping.EmergencyBreak
                    };

                    var textLayout = new CanvasTextLayout(sender.Device,
                                                          segment.Name,
                                                          textFormat,
                                                          (float)segmentRect.Width,
                                                          (float)segmentRect.Height);

                    textLayout.LineSpacingBaseline = (float)textLayout.DrawBounds.Height;
                    textLayout.LineSpacing = textLayout.LineSpacingBaseline + 1;

                    // Try to draw segment text using primary font
                    if (textLayout.DrawBounds.Height < segmentRect.Height &&
                        textLayout.DrawBounds.Width < segmentRect.Width)
                    {
                        args.DrawingSession.DrawTextLayout(
                            textLayout,
                            (float)x,
                            (float)(segmentRect.Bottom - (segmentRect.Height / 2) - (textLayout.LayoutBounds.Height / 2)),
                            style.SpanLabelBrush);
                    }
                    else if (AlternateFontFamily != null && AlternateFontSize > 0)
                    {
                        var textFormatAlt = new CanvasTextFormat
                        {
                            FontFamily = AlternateFontFamily.Source,
                            FontSize = (float)AlternateFontSize,
                            FontStretch = AlternateFontStretch,
                            FontStyle = AlternateFontStyle,
                            FontWeight = AlternateFontWeight,
                            Direction = CanvasTextDirection.LeftToRightThenTopToBottom,
                            LastLineWrapping = false,
                            LineSpacingMode = CanvasLineSpacingMode.Default,
                            LineSpacing = 0,
                            LineSpacingBaseline = 0,
                            HorizontalAlignment = CanvasHorizontalAlignment.Center,
                            VerticalAlignment = CanvasVerticalAlignment.Top,
                            WordWrapping = CanvasWordWrapping.EmergencyBreak
                        };

                        var textLayoutAlt = new CanvasTextLayout(sender.Device,
                                                                 segment.Name,
                                                                 textFormatAlt,
                                                                 (float)segmentRect.Width,
                                                                 (float)segmentRect.Height);

                        textLayoutAlt.LineSpacingBaseline = (float)textLayoutAlt.DrawBounds.Height;
                        textLayoutAlt.LineSpacing = textLayoutAlt.LineSpacingBaseline + 1;

                        // Try to draw segment text using altername font
                        if (textLayoutAlt.DrawBounds.Height < segmentRect.Height &&
                            textLayoutAlt.DrawBounds.Width < segmentRect.Width)
                        {
                            args.DrawingSession.DrawTextLayout(
                                textLayoutAlt,
                                (float)x,
                                (float)(segmentRect.Bottom - (segmentRect.Height / 2) - (textLayoutAlt.LayoutBounds.Height / 2)),
                                style.SpanLabelBrush);
                        }
                    }
                }
            }

            // Draw markers
            using (args.DrawingSession.CreateLayer(1.0f))
            {
                foreach (var marker in Markers.Where(x => x.Duration == 0))
                {
                    if (marker.Position < ZoomStart || marker.Position > ZoomEnd)
                        continue;

                    var style = marker == SelectedMarker
                        ? MarkerStyleGroups[marker.Style].SelectedStyle
                        : MarkerStyleGroups[marker.Style].Style;

                    var x = (float)CalculateHorizontalRenderCoordinates(marker.Position, marker.Position, ref tickAreaRect).x;
                    var y = (float)(markerAreaRect.Top + (markerAreaRect.Height - style.RenderTarget.Bounds.Height));

                    args.DrawingSession.DrawImage(style.RenderTarget, x, y);
                }
            }

            // Local function to add a major tick to its list and advance to the next
            void AddMajorTick()
            {
                majorTicks.Add(adjMajor);
                major += majorInterval;
                adjMajor = decimal.Round(major, DecimalPrecision);
            }

            // Local function to add a minor tick to its list and advance to the next
            void AddMinorTick()
            {
                minorTicks.Add(adjMinor);
                minor += minorInterval;
                adjMinor = decimal.Round(minor, DecimalPrecision);
            }

            // Local function to draw a tick
            void DrawTick(double position,
                          (double y, double height) verticalCoords,
                          ICanvasBrush brush,
                          double thickness)
            {
                var adjPos = position - (thickness / 2);
                args.DrawingSession.FillRectangle(
                    new Rect(adjPos, verticalCoords.y, thickness, verticalCoords.height),
                    brush);
            }
        }
        #endregion
        #endregion

        #region Private Properties
        private bool IsValidVisibleRange => ZoomEnd - ZoomStart >= MinimumVisibleRange;

        private Rect TrackAreaRect
        {
            get
            {
                var trackBarHeight = TrackCount > 0 ? (TrackHeight * TrackCount) + ((TrackCount - 1) * TrackSpacing) : 0;
                return new Rect(0, _mainPanel.ActualHeight - trackBarHeight, _mainPanel.ActualWidth, trackBarHeight);
            }
        }

        private Rect MarkerAreaRect => new(0, 0, _mainPanel.ActualWidth, MarkerBarHeight);

        private Rect TickAreaRect => new(new Point(0, MarkerAreaRect.Bottom),
                                         new Point(_mainPanel.ActualWidth, TrackAreaRect.Top));

        private double MinorTickSpacing =>
            ConvertTimeIntervalToPixels(
                GetSnapIntervalValue(
                    SnapInterval.MinorTick)) - MinorTickThickness;

        private double MajorTickSpacing =>
            ConvertTimeIntervalToPixels(
                GetSnapIntervalValue(
                    SnapInterval.MajorTick)) - MajorTickThickness;

        private static bool IsCtrlKeyPressed =>
            PInvoke.User32.GetKeyState((int)PInvoke.User32.VirtualKey.VK_CONTROL) < 0;

        private static bool IsShiftKeyPressed =>
            PInvoke.User32.GetKeyState((int)PInvoke.User32.VirtualKey.VK_SHIFT) < 0;
        #endregion

        #region Private Methods
        private void UpdatePositionElementLayout()
        {
            if (_mainPanel == null || _positionElement == null || !IsLoaded)
                return;

            // Hide position element if the current position
            // is not within the current visible range,
            // or if the current visible range is invalid.
            if (Position < ZoomStart || Position > ZoomEnd || !IsValidVisibleRange)
            {
                _positionElement.Visibility = Visibility.Collapsed;
                return;
            }

            var tickAreaRect = TickAreaRect;
            var (x, _) = CalculateHorizontalRenderCoordinates(Position, Position, ref tickAreaRect);
            var (y, height) = CalculateVerticalRenderCoordinates(PositionElementRelativeHeight,
                                                                 PositionElementAlignment,
                                                                 ref tickAreaRect);

            Canvas.SetLeft(_positionElement, x - (_positionElement.ActualWidth / 2));
            Canvas.SetTop(_positionElement, y);
            _positionElement.Height = height;
            _positionElement.Visibility = Visibility.Visible;
        }

        private void UpdateSelectionElementLayout()
        {
            if (_mainPanel == null || !IsLoaded)
                return;

            // Hide all selection-related elements if selection is disabled,
            // or if the current visible range is invalid.
            if (!IsSelectionEnabled || !IsValidVisibleRange)
            {
                _selectionStartElement.Visibility = Visibility.Collapsed;
                _selectionEndElement.Visibility = Visibility.Collapsed;
                _selectionThumbElement.Visibility = Visibility.Collapsed;
                return;
            }

            var tickAreaRect = TickAreaRect;

            if (_selectionStartElement != null && _selectionEndElement != null)
            {
                // Position the selection start element on the horizontal axis
                if (SelectionStart >= ZoomStart && SelectionStart <= ZoomEnd)
                {
                    var (x, _) = CalculateHorizontalRenderCoordinates(SelectionStart, SelectionStart, ref tickAreaRect);
                    Canvas.SetLeft(_selectionStartElement, x - _selectionStartElement.ActualWidth);
                    _selectionStartElement.Visibility = Visibility.Visible;
                }
                else
                {
                    _selectionStartElement.Visibility = Visibility.Collapsed;
                }

                // Position the selection end element on the horizontal axis
                if (SelectionEnd >= ZoomStart && SelectionEnd <= ZoomEnd)
                {
                    var (x, _) = CalculateHorizontalRenderCoordinates(SelectionEnd, SelectionEnd, ref tickAreaRect);
                    Canvas.SetLeft(_selectionEndElement, x);
                    _selectionEndElement.Visibility = Visibility.Visible;
                }
                else
                {
                    _selectionEndElement.Visibility = Visibility.Collapsed;
                }

                // Position the selection start and end elements on the vertical axis
                var (y, height) = CalculateVerticalRenderCoordinates(SelectionInOutElementsRelativeHeight,
                                                                     SelectionInOutElementsAlignment,
                                                                     ref tickAreaRect);

                Canvas.SetTop(_selectionStartElement, y);
                Canvas.SetTop(_selectionEndElement, y);
                _selectionStartElement.Height = height;
                _selectionEndElement.Height = height;
            }

            // Update the selection thumb
            if (_selectionThumbElement != null &&
                SelectionEnd - SelectionStart != 0 &&
                SelectionStart < ZoomEnd &&
                SelectionEnd > ZoomStart)
            {
                var adjustedStart = SelectionStart >= ZoomStart ? SelectionStart : ZoomStart;
                var adjustedEnd = SelectionEnd <= ZoomEnd ? SelectionEnd : ZoomEnd;
                var (x, width) = CalculateHorizontalRenderCoordinates(adjustedStart, adjustedEnd, ref tickAreaRect);
                var (y, height) = CalculateVerticalRenderCoordinates(SelectionThumbElementRelativeHeight,
                                                                     SelectionThumbElementAlignment,
                                                                     ref tickAreaRect);

                Canvas.SetLeft(_selectionThumbElement, x);
                Canvas.SetTop(_selectionThumbElement, y);
                _selectionThumbElement.Width = width;
                _selectionThumbElement.Height = height;
                _selectionThumbElement.Visibility = Visibility.Visible;
            }
            else
            {
                _selectionThumbElement.Visibility = Visibility.Collapsed;
            }
        }

        private void UpdateZoomElementLayout()
        {
            if (_zoomPanel == null || !IsLoaded)
                return;

            // Hide all zoom-related elements if the current visible range is invalid.
            // This situation cannot be caused by direct modification of zoom values.
            // However, if the timeline has a total duration which is less than
            // the minimum allowable visible range, this situation will occur.
            if (!IsValidVisibleRange)
            {
                _zoomStartElement.Visibility = Visibility.Collapsed;
                _zoomEndElement.Visibility = Visibility.Collapsed;
                _zoomThumbElement.Visibility = Visibility.Collapsed;
                return;
            }

            // Position and resize the zoom start element
            if (_zoomStartElement != null)
            {
                _zoomStartElement.Height = _zoomPanel.ActualHeight;
                Canvas.SetTop(_zoomStartElement, 0);
                Canvas.SetLeft(
                    _zoomStartElement,
                    decimal.ToDouble(
                        ((ZoomStart - Start) *
                        ((decimal)_zoomPanel.ActualWidth / (End - Start))) -
                        (decimal)_zoomStartElement.ActualWidth));
                _zoomStartElement.Visibility = Visibility.Visible;
            }

            // Position and resize the zoom end element
            if (_zoomEndElement != null)
            {
                _zoomEndElement.Height = _zoomPanel.ActualHeight;
                Canvas.SetTop(_zoomEndElement, 0);
                Canvas.SetLeft(
                    _zoomEndElement,
                    decimal.ToDouble(
                        (ZoomEnd - Start) *
                        ((decimal)_zoomPanel.ActualWidth / (End - Start))));
                _zoomEndElement.Visibility = Visibility.Visible;
            }

            // Position and resize the zoom thumb element
            if (_zoomThumbElement != null)
            {
                if (ZoomEnd - ZoomStart != 0)
                {
                    Canvas.SetTop(_zoomThumbElement, 0);
                    Canvas.SetLeft(
                        _zoomThumbElement,
                        decimal.ToDouble(
                            (ZoomStart - Start) *
                            ((decimal)_zoomPanel.ActualWidth / (End - Start))));
                    _zoomThumbElement.Width = decimal.ToDouble(
                        Math.Abs(ZoomEnd - ZoomStart) *
                        (decimal)_zoomPanel.ActualWidth /
                        (End - Start));
                    _zoomThumbElement.Height = _zoomPanel.ActualHeight;
                    _zoomThumbElement.Visibility = Visibility.Visible;
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

            var left = _positionElement?.ActualWidth / 2 ?? 0;
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
                        OffsetVisibleWindow(decimal.Negate(currentWindow));
                    break;

                case FollowMode.Scroll:
                    CenterVisibleWindow(Position);
                    break;

                default:
                    break;
            }
        }

        private (double x, double width) CalculateHorizontalRenderCoordinates(decimal start, decimal end, ref Rect bounds)
        {
            var x = decimal.ToDouble((start - ZoomStart) * ((decimal)bounds.Width / (ZoomEnd - ZoomStart)));
            var w = decimal.ToDouble(Math.Abs(end - start) * (decimal)bounds.Width / (ZoomEnd - ZoomStart));
            return (x, w);
        }

        private static (double y, double height) CalculateVerticalRenderCoordinates(double relativeHeight,
                                                                             VerticalAlignment alignment,
                                                                             ref Rect bounds)
        {
            var elementHeight = relativeHeight * bounds.Height;

            return alignment switch
            {
                VerticalAlignment.Center => (bounds.Top + ((bounds.Height - elementHeight) / 2), elementHeight),
                VerticalAlignment.Bottom => (bounds.Bottom - elementHeight, elementHeight),
                _ => (bounds.Top, elementHeight)
            };
        }

        private double ConvertTimeIntervalToPixels(decimal timeInterval)
        {
            return IsValidVisibleRange
                ? decimal.ToDouble((decimal)_timelineCanvas.ActualWidth * timeInterval / (ZoomEnd - ZoomStart))
                : 0;
        }

        private decimal ConvertScreenCoordinateToPosition(double coordinate)
        {
            if (!IsValidVisibleRange)
                return 0;

            if (coordinate < 0)
                coordinate = 0;

            var positiveWidth = ConvertTimeIntervalToPixels(ZoomEnd); // Number of pixels for which Position >= 0;
            var zeroCoordinate = _timelineCanvas.ActualWidth - positiveWidth;

            if (coordinate >= zeroCoordinate)
            {
                return decimal.Round(
                    (decimal)(coordinate - zeroCoordinate) * ZoomEnd / (decimal)positiveWidth,
                    DecimalPrecision);
            }

            return decimal.Round(
                (decimal)(zeroCoordinate - coordinate) * ZoomStart / (decimal)zeroCoordinate,
                DecimalPrecision);
        }

        private decimal GetSnapIntervalValue(SnapInterval interval)
        {
            return interval switch
            {
                SnapInterval.Frame => 1.0M / FramesPerSecond,
                SnapInterval.TenthSecond => 0.1M,
                SnapInterval.QuarterSecond => 0.25M,
                SnapInterval.HalfSecond => 0.5M,
                SnapInterval.Second => 1,
                SnapInterval.Minute => SecondsPerMinute,
                SnapInterval.Hour => SecondsPerHour,
                SnapInterval.Day => SecondsPerDay,
                SnapInterval.MinorTick => (decimal)_currentInterval.Value.minor /
                                                   _currentInterval.Value.subdivisionCount *
                                                   _currentInterval.Value.major,
                SnapInterval.MajorTick => _currentInterval.Value.major,
                _ => throw new ArgumentOutOfRangeException(nameof(interval), interval, null)
            };
        }

        private void InitializeTimescale()
        {
            _intervals.Clear();

            // Add intervals for seconds subdivided by frames
            foreach (var divisor in NET.Math.Prime.Divisors((ulong)FramesPerSecond))
            {
                _intervals.AddLast((1, (int)divisor, FramesPerSecond));
            }

            // Add intervals for minutes subdivided by seconds
            foreach (var divisor in NET.Math.Prime.Divisors(SecondsPerMinute))
            {
                _intervals.AddLast((SecondsPerMinute, (int)divisor, SecondsPerMinute));
            }

            // Add intervals for hours subdivided by minutes
            foreach (var divisor in NET.Math.Prime.Divisors(MinutesPerHour))
            {
                _intervals.AddLast((SecondsPerHour, (int)divisor, MinutesPerHour));
            }

            // Add intervals for days subdivided by hours
            foreach (var divisor in NET.Math.Prime.Divisors(HoursPerDay))
            {
                _intervals.AddLast((SecondsPerDay, (int)divisor, HoursPerDay));
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
            var result = false;

            if (SelectionStart < Start)
            {
                SelectionStart = Start;
                result = true;
            }
            else if (SelectionStart > End)
            {
                SelectionStart = End;
                result = true;
            }

            if (SelectionStart > SelectionEnd)
            {
                SelectionEnd = SelectionStart;
                result = true;
            }

            return result;
        }

        private bool AdjustSelectionEnd()
        {
            var result = false;

            if (SelectionEnd < Start)
            {
                SelectionEnd = Start;
                result = true;
            }
            else if (SelectionEnd > End)
            {
                SelectionEnd = End;
                result = true;
            }

            if (SelectionEnd < SelectionStart)
            {
                SelectionStart = SelectionEnd;
                result = true;
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
                        if (y.Key is TickType.Major or TickType.Minor)
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
                        if (y.Key is TickType.Origin or TickType.Major)
                            result = -1;
                        break;
                    }
                }

                return result;
            }
        }
        #endregion
    }
}