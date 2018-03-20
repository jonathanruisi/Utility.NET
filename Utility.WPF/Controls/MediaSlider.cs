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
using System.Windows.Data;
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
		#region Fields
		private ContentPresenter                  _selectionStart,    _selectionEnd,    _selectionRange;
		private ContentPresenter                  _visibleRangeStart, _visibleRangeEnd, _visibleRange;
		private ContentPresenter                  _position;
		private Panel                             _mainPanel, _zoomPanel;
		private TickBarAdvanced                   _tickBar;
		private bool                              _isMouseLeftButtonDown;
		private double                            _prevMouseCoord;
		private (decimal temporal, double visual) _snapDistance;
		#endregion

		#region Properties
		public double MouseDist { get => (double)GetValue(MouseDistProperty); set => SetValue(MouseDistProperty, value); }

		public static readonly DependencyProperty MouseDistProperty = DependencyProperty.Register(
			"MouseDist",
			typeof(double),
			typeof(MediaSlider),
			new FrameworkPropertyMetadata(0.0));

		#region General
		#endregion

		#region Media
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
		public decimal Minimum { get => (decimal)GetValue(MinimumProperty); set => SetValue(MinimumProperty, value); }

		public static readonly DependencyProperty MinimumProperty = DependencyProperty.Register(
			"Minimum",
			typeof(decimal),
			typeof(MediaSlider),
			new FrameworkPropertyMetadata(0.0M, OnRangePropertyChanged));

		public decimal Maximum { get => (decimal)GetValue(MaximumProperty); set => SetValue(MaximumProperty, value); }

		public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register(
			"Maximum",
			typeof(decimal),
			typeof(MediaSlider),
			new FrameworkPropertyMetadata(0.0M, OnRangePropertyChanged, CoerceMaximum));

		public decimal Position { get => (decimal)GetValue(PositionProperty); set => SetValue(PositionProperty, value); }

		public static readonly DependencyProperty PositionProperty = DependencyProperty.Register(
			"Position",
			typeof(decimal),
			typeof(MediaSlider),
			new FrameworkPropertyMetadata(0.0M, OnRangePropertyChanged, CoercePosition));

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
		#endregion

		#region Alignment and Sizing
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
		#endregion

		#region Events
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

		public event RoutedEventHandler SelectionChanged
		{
			add => AddHandler(SelectionChangedEvent, value);
			remove => RemoveHandler(SelectionChangedEvent, value);
		}

		public static readonly RoutedEvent SelectionChangedEvent = EventManager.RegisterRoutedEvent(
			"SelectionChanged",
			RoutingStrategy.Bubble,
			typeof(RoutedEventHandler),
			typeof(MediaSlider));

		public event RoutedEventHandler VisibleRangeChanged
		{
			add => AddHandler(VisibleRangeChangedEvent, value);
			remove => RemoveHandler(VisibleRangeChangedEvent, value);
		}

		public static readonly RoutedEvent VisibleRangeChangedEvent = EventManager.RegisterRoutedEvent(
			"VisibleRangeChanged",
			RoutingStrategy.Bubble,
			typeof(RoutedEventHandler),
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
		public void AdjustPaddingToFit()
		{
			var mainLeftSpaceNeeded  = Math.Max(_position.ActualWidth / 2, _selectionStart.ActualWidth) - Padding.Left;
			var mainRightSpaceNeeded = Math.Max(_position.ActualWidth / 2, _selectionEnd.ActualWidth) - Padding.Right;
			var zoomLeftSpaceNeeded  = _visibleRangeStart.ActualWidth - Padding.Left;
			var zoomRightSpaceNeeded = _visibleRangeEnd.ActualWidth - Padding.Right;

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
				if (e.NewValue is decimal newValue && newValue > mediaSlider.SelectionEnd)
					mediaSlider.SelectionEnd = newValue;
				else if (e.NewValue == null)
					mediaSlider.SelectionEnd = null;

				mediaSlider.UpdateSelectionRangeElements();
				mediaSlider.RaiseEvent(new RoutedEventArgs(SelectionChangedEvent, d));
			}
			else if (e.Property == SelectionEndProperty)
			{
				if (e.NewValue is decimal newValue && newValue < mediaSlider.SelectionStart)
					mediaSlider.SelectionStart = newValue;
				else if (e.NewValue == null)
					mediaSlider.SelectionStart = null;

				mediaSlider.UpdateSelectionRangeElements();
				mediaSlider.RaiseEvent(new RoutedEventArgs(SelectionChangedEvent, d));
			}
			else if (e.Property == VisibleRangeStartProperty)
			{
				mediaSlider.UpdatePositionElement();
				mediaSlider.UpdateSelectionRangeElements();
				mediaSlider.UpdateVisibleRangeElements();
				mediaSlider.RaiseEvent(new RoutedEventArgs(VisibleRangeChangedEvent, d));
			}
			else if (e.Property == VisibleRangeEndProperty)
			{
				mediaSlider.UpdatePositionElement();
				mediaSlider.UpdateSelectionRangeElements();
				mediaSlider.UpdateVisibleRangeElements();
				mediaSlider.RaiseEvent(new RoutedEventArgs(VisibleRangeChangedEvent, d));
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

			if (e.Property == FramesPerSecondProperty) { }
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

			var offset = mediaSlider.VisibleRangeEnd - Math.Min(mediaSlider.Maximum - mediaSlider.Minimum, 1);
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

			var offset = mediaSlider.VisibleRangeStart + Math.Min(mediaSlider.Maximum - mediaSlider.Minimum, 1);
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
		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			if (GetTemplateChild("PART_TickBar") is TickBarAdvanced tickBar)
			{
				_tickBar = tickBar;
				var binding = new Binding
				{
					Path      = new PropertyPath("FramesPerSecond"),
					Source    = this,
					Converter = new NumericInverseConverter()
				};
				_tickBar.SetBinding(TickBarAdvanced.MinorTickFrequencyProperty, binding);

				if (VisualTreeHelper.GetParent(tickBar) is Panel parent)
				{
					_mainPanel                     =  parent;
					_mainPanel.SizeChanged         += MediaSlider_MainPanel_SizeChanged;
					_mainPanel.MouseLeftButtonDown += MediaSlider_MainPanel_MouseLeftButtonDown;
				}
			}

			if (GetTemplateChild("PART_Position") is ContentPresenter position)
			{
				_position                     =  position;
				_position.MouseEnter          += Position_MouseEnter;
				_position.MouseLeave          += Position_MouseLeave;
				_position.MouseLeftButtonDown += Position_MouseLeftButtonDown;
				_position.MouseLeftButtonUp   += Position_MouseLeftButtonUp;
				_position.MouseMove           += Position_MouseMove;
			}

			if (GetTemplateChild("PART_SelectionStart") is ContentPresenter selectionStart)
			{
				_selectionStart                     =  selectionStart;
				_selectionStart.MouseEnter          += SelectionStart_MouseEnter;
				_selectionStart.MouseLeave          += SelectionStart_MouseLeave;
				_selectionStart.MouseLeftButtonDown += SelectionStart_MouseLeftButtonDown;
				_selectionStart.MouseLeftButtonUp   += SelectionStart_MouseLeftButtonUp;
				_selectionStart.MouseMove           += SelectionStart_MouseMove;
			}

			if (GetTemplateChild("PART_SelectionEnd") is ContentPresenter selectionEnd)
			{
				_selectionEnd                     =  selectionEnd;
				_selectionEnd.MouseEnter          += SelectionEnd_MouseEnter;
				_selectionEnd.MouseLeave          += SelectionEnd_MouseLeave;
				_selectionEnd.MouseLeftButtonDown += SelectionEnd_MouseLeftButtonDown;
				_selectionEnd.MouseLeftButtonUp   += SelectionEnd_MouseLeftButtonUp;
				_selectionEnd.MouseMove           += SelectionEnd_MouseMove;
			}

			if (GetTemplateChild("PART_SelectionRange") is ContentPresenter selectionRange)
			{
				_selectionRange = selectionRange;
			}

			if (GetTemplateChild("PART_VisibleRangeStart") is ContentPresenter visibleRangeStart)
			{
				_visibleRangeStart                     =  visibleRangeStart;
				_visibleRangeStart.MouseEnter          += VisibleRangeStart_MouseEnter;
				_visibleRangeStart.MouseLeave          += VisibleRangeStart_MouseLeave;
				_visibleRangeStart.MouseLeftButtonDown += VisibleRangeStart_MouseLeftButtonDown;
				_visibleRangeStart.MouseLeftButtonUp   += VisibleRangeStart_MouseLeftButtonUp;
				_visibleRangeStart.MouseMove           += VisibleRangeStart_MouseMove;
			}

			if (GetTemplateChild("PART_VisibleRangeEnd") is ContentPresenter visibleRangeEnd)
			{
				_visibleRangeEnd                     =  visibleRangeEnd;
				_visibleRangeEnd.MouseEnter          += VisibleRangeEnd_MouseEnter;
				_visibleRangeEnd.MouseLeave          += VisibleRangeEnd_MouseLeave;
				_visibleRangeEnd.MouseLeftButtonDown += VisibleRangeEnd_MouseLeftButtonDown;
				_visibleRangeEnd.MouseLeftButtonUp   += VisibleRangeEnd_MouseLeftButtonUp;
				_visibleRangeEnd.MouseMove           += VisibleRangeEnd_MouseMove;
			}

			if (GetTemplateChild("PART_VisibleRange") is ContentPresenter visibleRange)
			{
				_visibleRange = visibleRange;
				if (VisualTreeHelper.GetParent(visibleRange) is Panel parent)
				{
					_zoomPanel             =  parent;
					_zoomPanel.SizeChanged += MediaSlider_ZoomPanel_SizeChanged;
				}

				_visibleRange.MouseEnter          += VisibleRange_MouseEnter;
				_visibleRange.MouseLeave          += VisibleRange_MouseLeave;
				_visibleRange.MouseLeftButtonDown += VisibleRange_MouseLeftButtonDown;
				_visibleRange.MouseLeftButtonUp   += VisibleRange_MouseLeftButtonUp;
				_visibleRange.MouseMove           += VisibleRange_MouseMove;
			}

			Loaded += MediaSlider_Loaded;
		}
		#endregion

		#region Private Methods
		private void UpdatePositionElement()
		{
			if (_mainPanel == null || _position == null || Math.Abs(_mainPanel.ActualWidth) < double.Epsilon)
				return;

			if (Position < VisibleRangeStart || Position > VisibleRangeEnd || VisibleRangeStart == VisibleRangeEnd)
			{
				_position.Visibility = Visibility.Collapsed;
				return;
			}

			_position.Visibility = Visibility.Visible;
			Canvas.SetLeft(
				_position,
				decimal.ToDouble(
					(Position - VisibleRangeStart) * ((decimal)_mainPanel.ActualWidth / (VisibleRangeEnd - VisibleRangeStart)) -
					(decimal)_position.ActualWidth / 2));
		}

		private void UpdateSelectionRangeElements()
		{
			if (_mainPanel == null || _selectionStart == null || _selectionEnd == null || _selectionRange == null)
				return;

			// Hide the selection range controls if the selection range feature is disabled
			if (SelectionStart == null || SelectionEnd == null)
			{
				_selectionStart.Visibility = Visibility.Collapsed;
				_selectionEnd.Visibility   = Visibility.Collapsed;
				_selectionRange.Visibility = Visibility.Collapsed;
				return;
			}

			// Move the selection range start control if it is enabled and within the current visible range
			if (SelectionStart >= VisibleRangeStart && SelectionStart <= VisibleRangeEnd && VisibleRangeStart != VisibleRangeEnd)
			{
				_selectionStart.Visibility = Visibility.Visible;
				Canvas.SetLeft(
					_selectionStart,
					decimal.ToDouble(
						((decimal)SelectionStart - VisibleRangeStart) *
						((decimal)_mainPanel.ActualWidth / (VisibleRangeEnd - VisibleRangeStart)) -
						(decimal)_selectionStart.ActualWidth));
			}
			else
			{
				_selectionStart.Visibility = Visibility.Collapsed;
			}

			// Move the selection range end control if it is enabled and within the current visible range
			if (SelectionEnd >= VisibleRangeStart && SelectionEnd <= VisibleRangeEnd && VisibleRangeStart != VisibleRangeEnd)
			{
				_selectionEnd.Visibility = Visibility.Visible;
				Canvas.SetLeft(
					_selectionEnd,
					decimal.ToDouble(
						((decimal)SelectionEnd - VisibleRangeStart) *
						((decimal)_mainPanel.ActualWidth / (VisibleRangeEnd - VisibleRangeStart))));
			}
			else
			{
				_selectionEnd.Visibility = Visibility.Collapsed;
			}

			// Move the selection range highlight if selection range is enabled and within the current visible range
			if (SelectionEnd - SelectionStart != 0 && SelectionStart < VisibleRangeEnd && SelectionEnd > VisibleRangeStart)
			{
				if (VisibleRangeStart != VisibleRangeEnd)
				{
					_selectionRange.Visibility = Visibility.Visible;
					var adjustedSelectionStart = SelectionStart >= VisibleRangeStart ? (decimal)SelectionStart : VisibleRangeStart;
					var adjustedSelectionEnd   = SelectionEnd <= VisibleRangeEnd ? (decimal)SelectionEnd : VisibleRangeEnd;

					Canvas.SetLeft(
						_selectionRange,
						decimal.ToDouble(
							(adjustedSelectionStart - VisibleRangeStart) *
							((decimal)_mainPanel.ActualWidth / (VisibleRangeEnd - VisibleRangeStart))));
					_selectionRange.Width = decimal.ToDouble(
						Math.Abs(adjustedSelectionEnd - adjustedSelectionStart) * (decimal)_mainPanel.ActualWidth /
						(VisibleRangeEnd - VisibleRangeStart));
				}
				else
				{
					Canvas.SetLeft(_selectionRange, 0);
					_selectionRange.Width = _mainPanel.ActualWidth;
				}
			}
			else
			{
				_selectionRange.Visibility = Visibility.Collapsed;
			}
		}

		private void UpdateVisibleRangeElements()
		{
			if (_zoomPanel == null || _visibleRangeStart == null || _visibleRangeEnd == null || _visibleRange == null)
				return;

			// Move the visible range start control
			Canvas.SetLeft(
				_visibleRangeStart,
				decimal.ToDouble(
					(VisibleRangeStart - Minimum) * ((decimal)_zoomPanel.ActualWidth / (Maximum - Minimum)) -
					(decimal)_visibleRangeStart.ActualWidth));

			// Move the visible range end control
			Canvas.SetLeft(
				_visibleRangeEnd,
				decimal.ToDouble((VisibleRangeEnd - Minimum) * ((decimal)_zoomPanel.ActualWidth / (Maximum - Minimum))));

			// Move the visible range highlight (collapsed if visible range = 0)
			if (VisibleRangeEnd - VisibleRangeStart != 0)
			{
				_visibleRange.Visibility = Visibility.Visible;
				Canvas.SetLeft(
					_visibleRange,
					decimal.ToDouble((VisibleRangeStart - Minimum) * ((decimal)_zoomPanel.ActualWidth / (Maximum - Minimum))));
				_visibleRange.Width = decimal.ToDouble(
					Math.Abs(VisibleRangeEnd - VisibleRangeStart) * (decimal)_zoomPanel.ActualWidth / (Maximum - Minimum));
			}
			else
			{
				_visibleRange.Visibility = Visibility.Collapsed;
			}
		}

		private void ArrangePositionElement()
		{
			var newHeight = PositionRelativeSize * _mainPanel.ActualHeight;
			_position.Height = newHeight;

			switch (PositionAlignment)
			{
				case JLR.Utility.NET.Position.Top:
				case JLR.Utility.NET.Position.Left:
					Canvas.SetTop(_position, 0);
					break;
				case JLR.Utility.NET.Position.Middle:
				case JLR.Utility.NET.Position.Center:
					Canvas.SetTop(_position, (_mainPanel.ActualHeight - newHeight) / 2);
					break;
				case JLR.Utility.NET.Position.Bottom:
				case JLR.Utility.NET.Position.Right:
					Canvas.SetTop(_position, _mainPanel.ActualHeight - newHeight);
					break;
			}
		}

		private void ArrangeSelectionElements()
		{
			var newRangeHeight     = SelectionRangeRelativeSize * _mainPanel.ActualHeight;
			var newHighlightHeight = SelectionRangeHighlightRelativeSize * newRangeHeight;
			_selectionStart.Height = newRangeHeight;
			_selectionEnd.Height   = newRangeHeight;
			_selectionRange.Height = newHighlightHeight;

			switch (SelectionRangeAlignment)
			{
				case JLR.Utility.NET.Position.Top:
				case JLR.Utility.NET.Position.Left:
					Canvas.SetTop(_selectionStart, 0);
					Canvas.SetTop(_selectionEnd,   0);
					break;
				case JLR.Utility.NET.Position.Middle:
				case JLR.Utility.NET.Position.Center:
					Canvas.SetTop(_selectionStart, (_mainPanel.ActualHeight - newRangeHeight) / 2);
					Canvas.SetTop(_selectionEnd,   (_mainPanel.ActualHeight - newRangeHeight) / 2);
					Canvas.SetTop(_selectionRange, (_mainPanel.ActualHeight - newHighlightHeight) / 2);
					break;
				case JLR.Utility.NET.Position.Bottom:
				case JLR.Utility.NET.Position.Right:
					Canvas.SetTop(_selectionStart, _mainPanel.ActualHeight - newRangeHeight);
					Canvas.SetTop(_selectionEnd,   _mainPanel.ActualHeight - newRangeHeight);
					Canvas.SetTop(_selectionRange, _mainPanel.ActualHeight - newHighlightHeight);
					break;
				default:
					break;
			}

			switch (SelectionRangeHighlightAlignment)
			{
				case JLR.Utility.NET.Position.Top:
				case JLR.Utility.NET.Position.Left:
					Canvas.SetTop(_selectionRange, Canvas.GetTop(_selectionStart));
					break;
				case JLR.Utility.NET.Position.Middle:
				case JLR.Utility.NET.Position.Center:
					Canvas.SetTop(_selectionRange, Canvas.GetTop(_selectionStart) + (newRangeHeight - newHighlightHeight) / 2);
					break;
				case JLR.Utility.NET.Position.Bottom:
				case JLR.Utility.NET.Position.Right:
					Canvas.SetTop(_selectionRange, Canvas.GetTop(_selectionStart) + (newRangeHeight - newHighlightHeight));
					break;
			}
		}

		private void ArrangeVisibleRangeElements()
		{
			_visibleRangeStart.Height = _zoomPanel.ActualHeight;
			_visibleRangeEnd.Height   = _zoomPanel.ActualHeight;
			_visibleRange.Height      = _zoomPanel.ActualHeight;
			Canvas.SetTop(_visibleRangeStart, 0);
			Canvas.SetTop(_visibleRangeEnd,   0);
			Canvas.SetTop(_visibleRange,      0);
		}

		private void UpdateSnapDistance()
		{
			if (_tickBar == null || _mainPanel == null)
			{
				_snapDistance = (0, 0);
				return;
			}

			_snapDistance.temporal = _tickBar.MinorTickFrequency;
			_snapDistance.visual = decimal.ToDouble(
				_snapDistance.temporal * (decimal)_mainPanel.ActualWidth / (VisibleRangeEnd - VisibleRangeStart));
		}
		#endregion

		#region Event Handlers
		private void MediaSlider_Loaded(object sender, RoutedEventArgs e)
		{
			AdjustPaddingToFit();
		}

		private void MediaSlider_MainPanel_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			UpdatePositionElement();
			ArrangePositionElement();
			UpdateSelectionRangeElements();
			ArrangeSelectionElements();
		}

		private void MediaSlider_ZoomPanel_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			UpdateVisibleRangeElements();
			ArrangeVisibleRangeElements();
		}

		private void MediaSlider_MainPanel_MouseLeftButtonDown(object sender, MouseEventArgs e)
		{
			if (e.Source.Equals(_selectionStart) || e.Source.Equals(_selectionEnd) || e.Source.Equals(_position))
				return;

			var pos = e.GetPosition(_mainPanel);
			var closest = (from tick in _tickBar.MajorTickPositions.Concat(_tickBar.MinorTickPositions)
						   orderby Math.Abs(tick.position - pos.X)
						   select tick).First();
			Position = closest.value;
		}

		private void Position_MouseEnter(object sender, MouseEventArgs e)
		{
			if (_isMouseLeftButtonDown) return;

			e.MouseDevice.OverrideCursor = Cursors.Hand;
			if (_position.ContentTemplate.FindName("shape", _position) is Shape position)
			{
				VisualStateManager.GoToElementState(position, "MouseOver", false);
			}
		}

		private void Position_MouseLeave(object sender, MouseEventArgs e)
		{
			e.MouseDevice.OverrideCursor = Cursors.Arrow;
			if (_position.ContentTemplate.FindName("shape", _position) is Shape position && !_position.IsMouseCaptureWithin)
			{
				VisualStateManager.GoToElementState(position, "Normal", false);
			}
		}

		private void Position_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (_isMouseLeftButtonDown) return;

			if (_position.ContentTemplate.FindName("shape", _position) is Shape position)
			{
				VisualStateManager.GoToElementState(position, "MouseLeftButtonDown", false);
			}

			_position.CaptureMouse();
			UpdateSnapDistance();
			_isMouseLeftButtonDown = true;
		}

		private void Position_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			if (_position.ContentTemplate.FindName("shape", _position) is Shape position)
			{
				VisualStateManager.GoToElementState(position, _position.IsMouseOver ? "MouseOver" : "Normal", false);
			}

			_position.ReleaseMouseCapture();
			_isMouseLeftButtonDown = false;
		}

		private void Position_MouseMove(object sender, MouseEventArgs e)
		{
			if (!_isMouseLeftButtonDown) return;

			var pos = e.GetPosition(_position);
			pos.X -= _position.ActualWidth / 2;
			var dist = Math.Abs(pos.X);

			if (pos.X < 0 && dist > _snapDistance.visual / 2)
			{
				if (Position - _snapDistance.temporal > VisibleRangeStart)
					Position -= _snapDistance.temporal * (int)(dist / _snapDistance.visual);
				else
					Position = VisibleRangeStart;
			}
			else if (pos.X > 0 && dist > _snapDistance.visual / 2)
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
			if (_selectionStart.ContentTemplate.FindName("shape", _selectionStart) is Shape selectionStart)
			{
				VisualStateManager.GoToElementState(selectionStart, "MouseOver", false);
			}
		}

		private void SelectionStart_MouseLeave(object sender, MouseEventArgs e)
		{
			e.MouseDevice.OverrideCursor = Cursors.Arrow;
			if (_selectionStart.ContentTemplate.FindName("shape", _selectionStart) is Shape selectionStart &&
				!_selectionStart.IsMouseCaptureWithin)
			{
				VisualStateManager.GoToElementState(selectionStart, "Normal", false);
			}
		}

		private void SelectionStart_MouseLeftButtonDown(object sender, MouseEventArgs e)
		{
			if (_isMouseLeftButtonDown) return;

			if (_selectionStart.ContentTemplate.FindName("shape", _selectionStart) is Shape selectionStart)
			{
				VisualStateManager.GoToElementState(selectionStart, "MouseLeftButtonDown", false);
			}

			_selectionStart.CaptureMouse();
			UpdateSnapDistance();
			_isMouseLeftButtonDown = true;
		}

		private void SelectionStart_MouseLeftButtonUp(object sender, MouseEventArgs e)
		{
			if (_selectionStart.ContentTemplate.FindName("shape", _selectionStart) is Shape selectionStart)
			{
				VisualStateManager.GoToElementState(selectionStart, _selectionStart.IsMouseOver ? "MouseOver" : "Normal", false);
			}

			_selectionStart.ReleaseMouseCapture();
			_isMouseLeftButtonDown = false;
		}

		private void SelectionStart_MouseMove(object sender, MouseEventArgs e)
		{
			if (!_isMouseLeftButtonDown) return;

			var pos = e.GetPosition(_selectionStart);
			pos.X -= _selectionStart.ActualWidth / 2;
			var dist = Math.Abs(pos.X);

			if (pos.X < 0 && dist > _snapDistance.visual / 2)
			{
				if (SelectionStart - _snapDistance.temporal > VisibleRangeStart)
					SelectionStart -= _snapDistance.temporal * (int)(dist / _snapDistance.visual);
				else
					SelectionStart = VisibleRangeStart;
			}
			else if (pos.X > 0 && dist > _snapDistance.visual / 2)
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
			if (_selectionEnd.ContentTemplate.FindName("shape", _selectionEnd) is Shape selectionEnd)
			{
				VisualStateManager.GoToElementState(selectionEnd, "MouseOver", false);
			}
		}

		private void SelectionEnd_MouseLeave(object sender, MouseEventArgs e)
		{
			e.MouseDevice.OverrideCursor = Cursors.Arrow;
			if (_selectionEnd.ContentTemplate.FindName("shape", _selectionEnd) is Shape selectionEnd &&
				!_selectionEnd.IsMouseCaptureWithin)
			{
				VisualStateManager.GoToElementState(selectionEnd, "Normal", false);
			}
		}

		private void SelectionEnd_MouseLeftButtonDown(object sender, MouseEventArgs e)
		{
			if (_isMouseLeftButtonDown) return;

			if (_selectionEnd.ContentTemplate.FindName("shape", _selectionEnd) is Shape selectionEnd)
			{
				VisualStateManager.GoToElementState(selectionEnd, "MouseLeftButtonDown", false);
			}

			_selectionEnd.CaptureMouse();
			UpdateSnapDistance();
			_isMouseLeftButtonDown = true;
		}

		private void SelectionEnd_MouseLeftButtonUp(object sender, MouseEventArgs e)
		{
			if (_selectionEnd.ContentTemplate.FindName("shape", _selectionEnd) is Shape selectionEnd)
			{
				VisualStateManager.GoToElementState(selectionEnd, _selectionEnd.IsMouseOver ? "MouseOver" : "Normal", false);
			}

			_selectionEnd.ReleaseMouseCapture();
			_isMouseLeftButtonDown = false;
		}

		private void SelectionEnd_MouseMove(object sender, MouseEventArgs e)
		{
			if (!_isMouseLeftButtonDown) return;

			var pos = e.GetPosition(_selectionEnd);
			pos.X -= _selectionEnd.ActualWidth / 2;
			var dist = Math.Abs(pos.X);

			if (pos.X < 0 && dist > _snapDistance.visual / 2)
			{
				if (SelectionEnd - _snapDistance.temporal > VisibleRangeStart)
					SelectionEnd -= _snapDistance.temporal * (int)(dist / _snapDistance.visual);
				else
					SelectionEnd = VisibleRangeStart;
			}
			else if (pos.X > 0 && dist > _snapDistance.visual / 2)
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
			if (_visibleRangeStart.ContentTemplate.FindName("shape", _visibleRangeStart) is Shape visibleRangeStart)
			{
				VisualStateManager.GoToElementState(visibleRangeStart, "MouseOver", false);
			}
		}

		private void VisibleRangeStart_MouseLeave(object sender, MouseEventArgs e)
		{
			e.MouseDevice.OverrideCursor = Cursors.Arrow;
			if (_visibleRangeStart.ContentTemplate.FindName("shape", _visibleRangeStart) is Shape visibleRangeStart &&
				!_visibleRangeStart.IsMouseCaptureWithin)
			{
				VisualStateManager.GoToElementState(visibleRangeStart, "Normal", false);
			}
		}

		private void VisibleRangeStart_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (_isMouseLeftButtonDown) return;

			if (_visibleRangeStart.ContentTemplate.FindName("shape", _visibleRangeStart) is Shape visibleRangeStart)
			{
				VisualStateManager.GoToElementState(visibleRangeStart, "MouseLeftButtonDown", false);
			}

			_visibleRangeStart.CaptureMouse();
			_isMouseLeftButtonDown = true;
		}

		private void VisibleRangeStart_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			if (_visibleRangeStart.ContentTemplate.FindName("shape", _visibleRangeStart) is Shape visibleRangeStart)
			{
				VisualStateManager.GoToElementState(
					visibleRangeStart,
					_visibleRangeStart.IsMouseOver ? "MouseOver" : "Normal",
					false);
			}

			_visibleRangeStart.ReleaseMouseCapture();
			_isMouseLeftButtonDown = false;
		}

		private void VisibleRangeStart_MouseMove(object sender, MouseEventArgs e)
		{
			if (!_isMouseLeftButtonDown || !_visibleRangeStart.IsMouseCaptureWithin) return;

			var pos   = e.GetPosition(_visibleRangeStart);
			var delta = (decimal)pos.X * (Maximum - Minimum) / (decimal)_zoomPanel.ActualWidth;


			if ((pos.X < 0 && VisibleRangeStart > Minimum) || (pos.X > 0 && VisibleRangeStart < Maximum))
			{
				VisibleRangeStart += delta;
			}
		}

		private void VisibleRangeEnd_MouseEnter(object sender, MouseEventArgs e)
		{
			if (_isMouseLeftButtonDown) return;

			e.MouseDevice.OverrideCursor = Cursors.SizeWE;
			if (_visibleRangeEnd.ContentTemplate.FindName("shape", _visibleRangeEnd) is Shape visibleRangeEnd)
			{
				VisualStateManager.GoToElementState(visibleRangeEnd, "MouseOver", false);
			}
		}

		private void VisibleRangeEnd_MouseLeave(object sender, MouseEventArgs e)
		{
			e.MouseDevice.OverrideCursor = Cursors.Arrow;
			if (_visibleRangeEnd.ContentTemplate.FindName("shape", _visibleRangeEnd) is Shape visibleRangeEnd &&
				!_visibleRangeEnd.IsMouseCaptureWithin)
			{
				VisualStateManager.GoToElementState(visibleRangeEnd, "Normal", false);
			}
		}

		private void VisibleRangeEnd_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (_isMouseLeftButtonDown) return;

			if (_visibleRangeEnd.ContentTemplate.FindName("shape", _visibleRangeEnd) is Shape visibleRangeEnd)
			{
				VisualStateManager.GoToElementState(visibleRangeEnd, "MouseLeftButtonDown", false);
			}

			_visibleRangeEnd.CaptureMouse();
			_isMouseLeftButtonDown = true;
		}

		private void VisibleRangeEnd_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			if (_visibleRangeEnd.ContentTemplate.FindName("shape", _visibleRangeEnd) is Shape visibleRangeEnd)
			{
				VisualStateManager.GoToElementState(visibleRangeEnd, _visibleRangeEnd.IsMouseOver ? "MouseOver" : "Normal", false);
			}

			_visibleRangeEnd.ReleaseMouseCapture();
			_isMouseLeftButtonDown = false;
		}

		private void VisibleRangeEnd_MouseMove(object sender, MouseEventArgs e)
		{
			if (!_isMouseLeftButtonDown || !_visibleRangeEnd.IsMouseCaptureWithin) return;

			var pos   = e.GetPosition(_visibleRangeEnd);
			var delta = (decimal)pos.X * (Maximum - Minimum) / (decimal)_zoomPanel.ActualWidth;


			if ((pos.X < 0 && VisibleRangeEnd > Minimum) || (pos.X > 0 && VisibleRangeEnd < Maximum))
			{
				VisibleRangeEnd += delta;
			}
		}

		private void VisibleRange_MouseEnter(object sender, MouseEventArgs e)
		{
			if (_isMouseLeftButtonDown) return;

			e.MouseDevice.OverrideCursor = Cursors.ScrollWE;
			if (_visibleRange.ContentTemplate.FindName("shape", _visibleRange) is Shape visibleRange)
			{
				VisualStateManager.GoToElementState(visibleRange, "MouseOver", false);
			}
		}

		private void VisibleRange_MouseLeave(object sender, MouseEventArgs e)
		{
			e.MouseDevice.OverrideCursor = Cursors.Arrow;
			if (_visibleRange.ContentTemplate.FindName("shape", _visibleRange) is Shape visibleRange &&
				!_visibleRange.IsMouseCaptureWithin)
			{
				VisualStateManager.GoToElementState(visibleRange, "Normal", false);
			}
		}

		private void VisibleRange_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (_isMouseLeftButtonDown) return;

			if (_visibleRange.ContentTemplate.FindName("shape", _visibleRange) is Shape visibleRange)
			{
				VisualStateManager.GoToElementState(visibleRange, "MouseLeftButtonDown", false);
			}

			_visibleRange.CaptureMouse();
			_prevMouseCoord = e.GetPosition(_visibleRange).X;
			if (e.ClickCount >= 2)
			{
				VisibleRangeStart = Minimum;
				VisibleRangeEnd   = Maximum;
			}

			_isMouseLeftButtonDown = true;
		}

		private void VisibleRange_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			if (_visibleRange.ContentTemplate.FindName("shape", _visibleRange) is Shape visibleRange)
			{
				VisualStateManager.GoToElementState(visibleRange, _visibleRange.IsMouseOver ? "MouseOver" : "Normal", false);
			}

			_visibleRange.ReleaseMouseCapture();
			_isMouseLeftButtonDown = false;
		}

		private void VisibleRange_MouseMove(object sender, MouseEventArgs e)
		{
			if (!_isMouseLeftButtonDown || !_visibleRange.IsMouseCaptureWithin) return;

			var delta = (decimal)(e.GetPosition(_visibleRange).X - _prevMouseCoord) * (Maximum - Minimum) /
				(decimal)_zoomPanel.ActualWidth;
			var range = VisibleRangeEnd - VisibleRangeStart;

			if (delta < 0 && VisibleRangeStart > Minimum)
			{
				VisibleRangeStart = Math.Max(VisibleRangeStart + delta, Minimum);
				VisibleRangeEnd   = VisibleRangeStart + range;
			}
			else if (delta > 0 && VisibleRangeEnd < Maximum)
			{
				VisibleRangeEnd   = Math.Min(VisibleRangeEnd + delta, Maximum);
				VisibleRangeStart = VisibleRangeEnd - range;
			}
		}
		#endregion
	}
}