﻿// ┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
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
using System.Runtime.CompilerServices;
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
	[TemplatePart(Name        = "PART_SelectionHighlight", Type  = typeof(ContentPresenter))]
	[TemplatePart(Name        = "PART_ZoomStart", Type           = typeof(ContentPresenter))]
	[TemplatePart(Name        = "PART_ZoomEnd", Type             = typeof(ContentPresenter))]
	[TemplatePart(Name        = "PART_ZoomThumb", Type           = typeof(ContentPresenter))]
	[TemplateVisualState(Name = "Normal", GroupName              = "MouseStates")]
	[TemplateVisualState(Name = "MouseOver", GroupName           = "MouseStates")]
	[TemplateVisualState(Name = "MouseLeftButtonDown", GroupName = "MouseStates")]
	public class MediaSlider : Control
	{
		#region Constants
		private const decimal MinVisibleRange = 1;
		#endregion

		#region Fields
		private TickBarAdvanced _tickBar;
		private Panel _mainPanel, _zoomPanel;
		private ContentPresenter _positionElement;
		private ContentPresenter _selStartElement, _selEndElement, _selHighlightElement;
		private ContentPresenter _zoomStartElement, _zoomEndElement, _zoomThumbElement;
		private LinkedList<int> _fpsDivisors;
		private LinkedListNode<int> _currentFpsDivisor;
		private decimal _snap;
		private bool _isSelectionChanging, _isZoomChanging;
		#endregion

		#region Properties
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
			new FrameworkPropertyMetadata(1, OnFramesPerSecondPropertyChanged),
			IsPositiveInteger);
		#endregion

		#region Range
		public decimal Minimum { get => (decimal)GetValue(MinimumProperty); set => SetValue(MinimumProperty, value); }

		public static readonly DependencyProperty MinimumProperty = DependencyProperty.Register(
			"Minimum",
			typeof(decimal),
			typeof(MediaSlider),
			new FrameworkPropertyMetadata(0.0M, OnMinimumPropertyChanged));

		public decimal Maximum { get => (decimal)GetValue(MaximumProperty); set => SetValue(MaximumProperty, value); }

		public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register(
			"Maximum",
			typeof(decimal),
			typeof(MediaSlider),
			new FrameworkPropertyMetadata(0.0M, OnMaximumPropertyChanged, CoerceMaximum));

		public decimal Position { get => (decimal)GetValue(PositionProperty); set => SetValue(PositionProperty, value); }

		public static readonly DependencyProperty PositionProperty = DependencyProperty.Register(
			"Position",
			typeof(decimal),
			typeof(MediaSlider),
			new FrameworkPropertyMetadata(0.0M, OnPositionPropertyChanged, CoercePosition));

		public decimal? SelectionStart
		{
			get => (decimal?)GetValue(SelectionStartProperty);
			set => SetValue(SelectionStartProperty, value);
		}

		public static readonly DependencyProperty SelectionStartProperty = DependencyProperty.Register(
			"SelectionStart",
			typeof(decimal?),
			typeof(MediaSlider),
			new FrameworkPropertyMetadata(null, OnSelectionStartPropertyChanged, CoerceSelection));

		public decimal? SelectionEnd
		{
			get => (decimal?)GetValue(SelectionEndProperty);
			set => SetValue(SelectionEndProperty, value);
		}

		public static readonly DependencyProperty SelectionEndProperty = DependencyProperty.Register(
			"SelectionEnd",
			typeof(decimal?),
			typeof(MediaSlider),
			new FrameworkPropertyMetadata(null, OnSelectionEndPropertyChanged, CoerceSelection));

		public decimal ZoomStart { get => (decimal)GetValue(ZoomStartProperty); set => SetValue(ZoomStartProperty, value); }

		public static readonly DependencyProperty ZoomStartProperty = DependencyProperty.Register(
			"ZoomStart",
			typeof(decimal),
			typeof(MediaSlider),
			new FrameworkPropertyMetadata(0.0M, OnZoomStartPropertyChanged, CoerceZoomStart));

		public decimal ZoomEnd { get => (decimal)GetValue(ZoomEndProperty); set => SetValue(ZoomEndProperty, value); }

		public static readonly DependencyProperty ZoomEndProperty = DependencyProperty.Register(
			"ZoomEnd",
			typeof(decimal),
			typeof(MediaSlider),
			new FrameworkPropertyMetadata(0.0M, OnZoomEndPropertyChanged, CoerceZoomEnd));
		#endregion

		#region Alignment
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

		public Position PositionElementAlignment
		{
			get => (Position)GetValue(PositionElementAlignmentProperty);
			set => SetValue(PositionElementAlignmentProperty, value);
		}

		public static readonly DependencyProperty PositionElementAlignmentProperty = DependencyProperty.Register(
			"PositionElementAlignment",
			typeof(JLR.Utility.NET.Position),
			typeof(MediaSlider),
			new FrameworkPropertyMetadata(JLR.Utility.NET.Position.Top, OnPositionElementPropertyChanged));

		public Position SelectionElementAlignment
		{
			get => (Position)GetValue(SelectionElementAlignmentProperty);
			set => SetValue(SelectionElementAlignmentProperty, value);
		}

		public static readonly DependencyProperty SelectionElementAlignmentProperty = DependencyProperty.Register(
			"SelectionElementAlignment",
			typeof(JLR.Utility.NET.Position),
			typeof(MediaSlider),
			new FrameworkPropertyMetadata(JLR.Utility.NET.Position.Top, OnSelectionElementPropertyChanged));

		public Position SelectionHighlightElementAlignment
		{
			get => (Position)GetValue(SelectionHighlightElementAlignmentProperty);
			set => SetValue(SelectionHighlightElementAlignmentProperty, value);
		}

		public static readonly DependencyProperty SelectionHighlightElementAlignmentProperty = DependencyProperty.Register(
			"SelectionHighlightElementAlignment",
			typeof(JLR.Utility.NET.Position),
			typeof(MediaSlider),
			new FrameworkPropertyMetadata(JLR.Utility.NET.Position.Middle, OnSelectionElementPropertyChanged));
		#endregion

		#region Sizing
		public GridLength InnerPaddingSize
		{
			get => (GridLength)GetValue(InnerPaddingSizeProperty);
			set => SetValue(InnerPaddingSizeProperty, value);
		}

		public static readonly DependencyProperty InnerPaddingSizeProperty = DependencyProperty.Register(
			"InnerPaddingSize",
			typeof(GridLength),
			typeof(MediaSlider),
			new FrameworkPropertyMetadata(new GridLength(1, GridUnitType.Star)),
			IsGridLengthValid);

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

		public double PositionElementRelativeSize
		{
			get => (double)GetValue(PositionElementRelativeSizeProperty);
			set => SetValue(PositionElementRelativeSizeProperty, value);
		}

		public static readonly DependencyProperty PositionElementRelativeSizeProperty = DependencyProperty.Register(
			"PositionElementRelativeSize",
			typeof(double),
			typeof(MediaSlider),
			new FrameworkPropertyMetadata(1.0, OnPositionElementPropertyChanged));

		public double SelectionElementRelativeSize
		{
			get => (double)GetValue(SelectionElementRelativeSizeProperty);
			set => SetValue(SelectionElementRelativeSizeProperty, value);
		}

		public static readonly DependencyProperty SelectionElementRelativeSizeProperty = DependencyProperty.Register(
			"SelectionElementRelativeSize",
			typeof(double),
			typeof(MediaSlider),
			new FrameworkPropertyMetadata(1.0, OnSelectionElementPropertyChanged));

		public double SelectionHighlightElementRelativeSize
		{
			get => (double)GetValue(SelectionHighlightElementRelativeSizeProperty);
			set => SetValue(SelectionHighlightElementRelativeSizeProperty, value);
		}

		public static readonly DependencyProperty SelectionHighlightElementRelativeSizeProperty = DependencyProperty.Register(
			"SelectionHighlightElementRelativeSize",
			typeof(double),
			typeof(MediaSlider),
			new FrameworkPropertyMetadata(1.0, OnSelectionElementPropertyChanged));

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

		#region Z-Index
		public int OriginTickZIndex
		{
			get => (int)GetValue(OriginTickZIndexProperty);
			set => SetValue(OriginTickZIndexProperty, value);
		}

		public static readonly DependencyProperty OriginTickZIndexProperty = DependencyProperty.Register(
			"OriginTickZIndex",
			typeof(int),
			typeof(MediaSlider),
			new FrameworkPropertyMetadata(0));

		public int MajorTickZIndex
		{
			get => (int)GetValue(MajorTickZIndexProperty);
			set => SetValue(MajorTickZIndexProperty, value);
		}

		public static readonly DependencyProperty MajorTickZIndexProperty = DependencyProperty.Register(
			"MajorTickZIndex",
			typeof(int),
			typeof(MediaSlider),
			new FrameworkPropertyMetadata(0));

		public int MinorTickZIndex
		{
			get => (int)GetValue(MinorTickZIndexProperty);
			set => SetValue(MinorTickZIndexProperty, value);
		}

		public static readonly DependencyProperty MinorTickZIndexProperty = DependencyProperty.Register(
			"MinorTickZIndex",
			typeof(int),
			typeof(MediaSlider),
			new FrameworkPropertyMetadata(0));
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

		#region Templates
		public DataTemplate PositionElementTemplate
		{
			get => (DataTemplate)GetValue(PositionElementTemplateProperty);
			set => SetValue(PositionElementTemplateProperty, value);
		}

		public static readonly DependencyProperty PositionElementTemplateProperty = DependencyProperty.Register(
			"PositionElementTemplate",
			typeof(DataTemplate),
			typeof(MediaSlider),
			new FrameworkPropertyMetadata(null, OnPositionElementPropertyChanged));

		public DataTemplate SelectionStartElementTemplate
		{
			get => (DataTemplate)GetValue(SelectionStartElementTemplateProperty);
			set => SetValue(SelectionStartElementTemplateProperty, value);
		}

		public static readonly DependencyProperty SelectionStartElementTemplateProperty = DependencyProperty.Register(
			"SelectionStartElementTemplate",
			typeof(DataTemplate),
			typeof(MediaSlider),
			new FrameworkPropertyMetadata(null, OnSelectionElementPropertyChanged));

		public DataTemplate SelectionEndElementTemplate
		{
			get => (DataTemplate)GetValue(SelectionEndElementTemplateProperty);
			set => SetValue(SelectionEndElementTemplateProperty, value);
		}

		public static readonly DependencyProperty SelectionEndElementTemplateProperty = DependencyProperty.Register(
			"SelectionEndElementTemplate",
			typeof(DataTemplate),
			typeof(MediaSlider),
			new FrameworkPropertyMetadata(null, OnSelectionElementPropertyChanged));

		public DataTemplate SelectionHighlightElementTemplate
		{
			get => (DataTemplate)GetValue(SelectionHighlightElementTemplateProperty);
			set => SetValue(SelectionHighlightElementTemplateProperty, value);
		}

		public static readonly DependencyProperty SelectionHighlightElementTemplateProperty = DependencyProperty.Register(
			"SelectionHighlightElementTemplate",
			typeof(DataTemplate),
			typeof(MediaSlider),
			new FrameworkPropertyMetadata(null, OnSelectionElementPropertyChanged));

		public DataTemplate ZoomStartElementTemplate
		{
			get => (DataTemplate)GetValue(ZoomStartElementTemplateProperty);
			set => SetValue(ZoomStartElementTemplateProperty, value);
		}

		public static readonly DependencyProperty ZoomStartElementTemplateProperty = DependencyProperty.Register(
			"ZoomStartElementTemplate",
			typeof(DataTemplate),
			typeof(MediaSlider),
			new FrameworkPropertyMetadata(null, OnZoomElementPropertyChanged));

		public DataTemplate ZoomEndElementTemplate
		{
			get => (DataTemplate)GetValue(ZoomEndElementTemplateProperty);
			set => SetValue(ZoomEndElementTemplateProperty, value);
		}

		public static readonly DependencyProperty ZoomEndElementTemplateProperty = DependencyProperty.Register(
			"ZoomEndElementTemplate",
			typeof(DataTemplate),
			typeof(MediaSlider),
			new FrameworkPropertyMetadata(null, OnZoomElementPropertyChanged));

		public DataTemplate ZoomThumbElementTemplate
		{
			get => (DataTemplate)GetValue(ZoomThumbElementTemplateProperty);
			set => SetValue(ZoomThumbElementTemplateProperty, value);
		}

		public static readonly DependencyProperty ZoomThumbElementTemplateProperty = DependencyProperty.Register(
			"ZoomThumbElementTemplate",
			typeof(DataTemplate),
			typeof(MediaSlider),
			new FrameworkPropertyMetadata(null, OnZoomElementPropertyChanged));
		#endregion
		#endregion

		#region Events
		#endregion

		#region Constructors
		static MediaSlider()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(MediaSlider), new FrameworkPropertyMetadata(typeof(MediaSlider)));
		}
		#endregion

		#region Public Methods
		#endregion

		#region Property Changed Callbacks
		private static void OnFramesPerSecondPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (!(d is MediaSlider slider) || !slider.IsLoaded)
				return;

			slider.InitializeSnap();
		}

		private static void OnMinimumPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (!(d is MediaSlider slider))
				return;

			slider.CoerceValue(MaximumProperty);
			slider.CoerceValue(PositionProperty);
			slider.CoerceValue(SelectionStartProperty);
			slider.CoerceValue(SelectionEndProperty);
			slider.CoerceValue(ZoomStartProperty);
			slider.CoerceValue(ZoomEndProperty);
			slider.UpdatePositionElementLayout();
			slider.UpdateSelectionElementLayout();
			slider.UpdateZoomElementLayout();
		}

		private static void OnMaximumPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (!(d is MediaSlider slider))
				return;

			slider.CoerceValue(PositionProperty);
			slider.CoerceValue(SelectionStartProperty);
			slider.CoerceValue(SelectionEndProperty);
			slider.CoerceValue(ZoomStartProperty);
			slider.CoerceValue(ZoomEndProperty);
			slider.UpdatePositionElementLayout();
			slider.UpdateSelectionElementLayout();
			slider.UpdateZoomElementLayout();
		}

		private static void OnPositionPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (!(d is MediaSlider slider))
				return;

			slider.UpdatePositionElementLayout();
		}

		private static void OnSelectionStartPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (!(d is MediaSlider slider))
				return;

			if (slider._isSelectionChanging)
			{
				slider._isSelectionChanging = false;
				return;
			}

			switch (e.NewValue)
			{
				case decimal newStart when newStart > slider.SelectionEnd:
					slider._isSelectionChanging = true;
					slider.SetCurrentValue(SelectionEndProperty, newStart);
					break;
				case null:
					slider._isSelectionChanging = true;
					slider.SetCurrentValue(SelectionEndProperty, null);
					break;
			}

			slider.UpdateSelectionElementLayout();
		}

		private static void OnSelectionEndPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (!(d is MediaSlider slider))
				return;

			if (slider._isSelectionChanging)
			{
				slider._isSelectionChanging = false;
				return;
			}

			switch (e.NewValue)
			{
				case decimal newEnd when newEnd < slider.SelectionStart:
					slider._isSelectionChanging = true;
					slider.SetCurrentValue(SelectionStartProperty, newEnd);
					break;
				case null:
					slider._isSelectionChanging = true;
					slider.SetCurrentValue(SelectionStartProperty, null);
					break;
			}

			slider.UpdateSelectionElementLayout();
		}

		private static void OnZoomStartPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (!(d is MediaSlider slider))
				return;

			if (slider._isZoomChanging)
			{
				slider._isZoomChanging = false;
				return;
			}

			slider.UpdatePositionElementLayout();
			slider.UpdateSelectionElementLayout();
			slider.UpdateZoomElementLayout();
		}

		private static void OnZoomEndPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (!(d is MediaSlider slider))
				return;

			if (slider._isZoomChanging)
			{
				slider._isZoomChanging = false;
				return;
			}

			slider.UpdatePositionElementLayout();
			slider.UpdateSelectionElementLayout();
			slider.UpdateZoomElementLayout();
		}

		private static void OnPositionElementPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (!(d is MediaSlider slider))
				return;

			slider.UpdatePositionElementLayout();
		}

		private static void OnSelectionElementPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (!(d is MediaSlider slider))
				return;

			slider.UpdateSelectionElementLayout();
		}

		private static void OnZoomElementPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (!(d is MediaSlider slider))
				return;

			slider.UpdateZoomElementLayout();
		}
		#endregion

		#region Coerce Value Callbacks
		private static object CoerceMaximum(DependencyObject d, object value)
		{
			var slider = (MediaSlider)d;
			return (decimal)value < slider.Minimum ? slider.Minimum : value;
		}

		private static object CoercePosition(DependencyObject d, object value)
		{
			var slider   = (MediaSlider)d;
			var position = (decimal)value;
			if (position < slider.Minimum)
				return slider.Minimum;
			return position > slider.Maximum ? slider.Maximum : value;
		}

		private static object CoerceSelection(DependencyObject d, object value)
		{
			if (value == null)
				return null;

			var slider    = (MediaSlider)d;
			var selection = (decimal)value;
			if (selection < slider.Minimum)
				return slider.Minimum;
			return selection > slider.Maximum ? slider.Maximum : value;
		}

		private static object CoerceZoomStart(DependencyObject d, object value)
		{
			var slider    = (MediaSlider)d;
			var zoomStart = (decimal)value;
			if (zoomStart < slider.Minimum)
				return slider.Minimum;

			var offset = slider.ZoomEnd - Math.Min(slider.Maximum - slider.Minimum, MinVisibleRange);
			if (zoomStart > offset)
				return offset > slider.Minimum ? offset : slider.Minimum;
			return value;
		}

		private static object CoerceZoomEnd(DependencyObject d, object value)
		{
			var slider  = (MediaSlider)d;
			var zoomEnd = (decimal)value;
			if (zoomEnd > slider.Maximum)
				return slider.Maximum;

			var offset = slider.ZoomStart + Math.Min(slider.Maximum - slider.Minimum, MinVisibleRange);
			if (zoomEnd < offset)
				return offset < slider.Maximum ? offset : slider.Maximum;
			return value;
		}
		#endregion

		#region Validate Value Callbacks
		private static bool IsGridLengthValid(object value)
		{
			return value is GridLength length && !length.IsAuto;
		}

		private static bool IsPositiveInteger(object value)
		{
			return value is int i && i > 0;
		}
		#endregion

		#region Layout and Render Overrides
		/// <inheritdoc />
		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			if (GetTemplateChild("PART_TickBar") is TickBarAdvanced tickBar)
			{
				_tickBar = tickBar;
				if (VisualTreeHelper.GetParent(tickBar) is Panel parent)
				{
					_mainPanel                     =  parent;
					_mainPanel.SizeChanged         += _mainPanel_SizeChanged;
					_mainPanel.MouseLeftButtonDown += _mainPanel_MouseLeftButtonDown;
				}

				_tickBar.SmallestTickGapChanged += _tickBar_SmallestTickGapChanged;
			}

			if (GetTemplateChild("PART_Position") is ContentPresenter position)
			{
				_positionElement                     =  position;
				_positionElement.MouseEnter          += _positionElement_MouseEnter;
				_positionElement.MouseLeave          += _positionElement_MouseLeave;
				_positionElement.MouseLeftButtonDown += _positionElement_MouseLeftButtonDown;
				_positionElement.MouseLeftButtonUp   += _positionElement_MouseLeftButtonUp;
				_positionElement.MouseMove           += _positionElement_MouseMove;
				_positionElement.SizeChanged         += _namedElement_SizeChanged;
			}

			if (GetTemplateChild("PART_SelectionStart") is ContentPresenter selectionStart)
			{
				_selStartElement                     =  selectionStart;
				_selStartElement.MouseEnter          += _selStartElement_MouseEnter;
				_selStartElement.MouseLeave          += _selStartElement_MouseLeave;
				_selStartElement.MouseLeftButtonDown += _selStartElement_MouseLeftButtonDown;
				_selStartElement.MouseLeftButtonUp   += _selStartElement_MouseLeftButtonUp;
				_selStartElement.MouseMove           += _selStartElement_MouseMove;
				_selStartElement.SizeChanged         += _namedElement_SizeChanged;
			}

			if (GetTemplateChild("PART_SelectionEnd") is ContentPresenter selectionEnd)
			{
				_selEndElement                     =  selectionEnd;
				_selEndElement.MouseEnter          += _selEndElement_MouseEnter;
				_selEndElement.MouseLeave          += _selEndElement_MouseLeave;
				_selEndElement.MouseLeftButtonDown += _selEndElement_MouseLeftButtonDown;
				_selEndElement.MouseLeftButtonUp   += _selEndElement_MouseLeftButtonUp;
				_selEndElement.MouseMove           += _selEndElement_MouseMove;
				_selEndElement.SizeChanged         += _namedElement_SizeChanged;
			}

			if (GetTemplateChild("PART_SelectionHighlight") is ContentPresenter selectionHighlight)
			{
				_selHighlightElement = selectionHighlight;
			}

			if (GetTemplateChild("PART_ZoomStart") is ContentPresenter zoomStart)
			{
				_zoomStartElement                     =  zoomStart;
				_zoomStartElement.MouseEnter          += _zoomStartElement_MouseEnter;
				_zoomStartElement.MouseLeave          += _zoomStartElement_MouseLeave;
				_zoomStartElement.MouseLeftButtonDown += _zoomStartElement_MouseLeftButtonDown;
				_zoomStartElement.MouseLeftButtonUp   += _zoomStartElement_MouseLeftButtonUp;
				_zoomStartElement.MouseMove           += _zoomStartElement_MouseMove;
				_zoomStartElement.SizeChanged         += _namedElement_SizeChanged;
			}

			if (GetTemplateChild("PART_ZoomEnd") is ContentPresenter zoomEnd)
			{
				_zoomEndElement                     =  zoomEnd;
				_zoomEndElement.MouseEnter          += _zoomEndElement_MouseEnter;
				_zoomEndElement.MouseLeave          += _zoomEndElement_MouseLeave;
				_zoomEndElement.MouseLeftButtonDown += _zoomEndElement_MouseLeftButtonDown;
				_zoomEndElement.MouseLeftButtonUp   += _zoomEndElement_MouseLeftButtonUp;
				_zoomEndElement.MouseMove           += _zoomEndElement_MouseMove;
				_zoomEndElement.SizeChanged         += _namedElement_SizeChanged;
			}

			if (GetTemplateChild("PART_ZoomThumb") is ContentPresenter zoomThumb)
			{
				_zoomThumbElement = zoomThumb;
				if (VisualTreeHelper.GetParent(zoomThumb) is Panel parent)
				{
					_zoomPanel             =  parent;
					_zoomPanel.SizeChanged += _zoomPanel_SizeChanged;
				}

				_zoomThumbElement.MouseEnter          += _zoomThumbElement_MouseEnter;
				_zoomThumbElement.MouseLeave          += _zoomThumbElement_MouseLeave;
				_zoomThumbElement.MouseLeftButtonDown += _zoomThumbElement_MouseLeftButtonDown;
				_zoomThumbElement.MouseLeftButtonUp   += _zoomThumbElement_MouseLeftButtonUp;
				_zoomThumbElement.MouseMove           += _zoomThumbElement_MouseMove;
			}

			Loaded += MediaSlider_Loaded;
		}
		#endregion

		#region Event Handlers
		#region General
		private void MediaSlider_Loaded(object sender, RoutedEventArgs e)
		{
			AdjustPaddingToFit();
			InitializeSnap();
		}

		private void _mainPanel_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			UpdatePositionElementLayout();
			UpdateSelectionElementLayout();
		}

		private void _zoomPanel_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			UpdateZoomElementLayout();
		}

		private void _namedElement_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			AdjustPaddingToFit();
		}

		private void _mainPanel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) { }

		private void _tickBar_SmallestTickGapChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			
		}
		#endregion

		#region Position Element
		private void _positionElement_MouseEnter(object sender, MouseEventArgs e) { }
		private void _positionElement_MouseLeave(object sender, MouseEventArgs e) { }
		private void _positionElement_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) { }
		private void _positionElement_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) { }
		private void _positionElement_MouseMove(object sender, MouseEventArgs e) { }
		#endregion

		#region Selection Start Element
		private void _selStartElement_MouseEnter(object sender, MouseEventArgs e) { }
		private void _selStartElement_MouseLeave(object sender, MouseEventArgs e) { }
		private void _selStartElement_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) { }
		private void _selStartElement_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) { }
		private void _selStartElement_MouseMove(object sender, MouseEventArgs e) { }
		#endregion

		#region Selection End Element
		private void _selEndElement_MouseEnter(object sender, MouseEventArgs e) { }
		private void _selEndElement_MouseLeave(object sender, MouseEventArgs e) { }
		private void _selEndElement_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) { }
		private void _selEndElement_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) { }
		private void _selEndElement_MouseMove(object sender, MouseEventArgs e) { }
		#endregion

		#region Zoom Start Element
		private void _zoomStartElement_MouseEnter(object sender, MouseEventArgs e) { }
		private void _zoomStartElement_MouseLeave(object sender, MouseEventArgs e) { }
		private void _zoomStartElement_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) { }
		private void _zoomStartElement_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) { }
		private void _zoomStartElement_MouseMove(object sender, MouseEventArgs e) { }
		#endregion

		#region Zoom End Element
		private void _zoomEndElement_MouseEnter(object sender, MouseEventArgs e) { }
		private void _zoomEndElement_MouseLeave(object sender, MouseEventArgs e) { }
		private void _zoomEndElement_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) { }
		private void _zoomEndElement_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) { }
		private void _zoomEndElement_MouseMove(object sender, MouseEventArgs e) { }
		#endregion

		#region Zoom Thumb Element
		private void _zoomThumbElement_MouseEnter(object sender, MouseEventArgs e) { }
		private void _zoomThumbElement_MouseLeave(object sender, MouseEventArgs e) { }
		private void _zoomThumbElement_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) { }
		private void _zoomThumbElement_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) { }
		private void _zoomThumbElement_MouseMove(object sender, MouseEventArgs e) { }
		#endregion
		#endregion

		#region Private Methods
		private void UpdatePositionElementLayout()
		{
			if (_mainPanel == null || _positionElement == null)
				return;

			// Hide position element if the current position is not within the current zoom window
			if (Position < ZoomStart || Position > ZoomEnd || ZoomStart == ZoomEnd)
			{
				_positionElement.Visibility = Visibility.Hidden;
				return;
			}

			// Resize position element
			var newHeight = PositionElementRelativeSize * _mainPanel.ActualHeight;
			_positionElement.Height = newHeight;

			// Position the position element on the primary axis
			_positionElement.Visibility = Visibility.Visible;
			Canvas.SetLeft(
				_positionElement,
				decimal.ToDouble(
					(Position - ZoomStart) * ((decimal)_mainPanel.ActualWidth / (ZoomEnd - ZoomStart)) -
					(decimal)_positionElement.ActualWidth / 2));

			// Position the position element on the secondary axis
			switch (PositionElementAlignment)
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

		private void UpdateSelectionElementLayout()
		{
			if (_mainPanel == null)
				return;

			var newRangeTop    = 0.0;
			var newRangeHeight = SelectionElementRelativeSize * _mainPanel.ActualHeight;
			if (_selStartElement != null && _selEndElement != null)
			{
				// Resize the selection elements
				_selStartElement.Height = newRangeHeight;
				_selEndElement.Height   = newRangeHeight;

				// Position the selection start element on the primary axis
				if (SelectionStart >= ZoomStart && SelectionStart <= ZoomEnd && ZoomStart != ZoomEnd && SelectionEnd != null)
				{
					_selStartElement.Visibility = Visibility.Visible;
					Canvas.SetLeft(
						_selStartElement,
						decimal.ToDouble(
							((decimal)SelectionStart - ZoomStart) * ((decimal)_mainPanel.ActualWidth / (ZoomEnd - ZoomStart)) -
							(decimal)_selStartElement.ActualWidth));
				}
				else
				{
					_selStartElement.Visibility = Visibility.Hidden;
				}

				// Position the selection end element on the primary axis
				if (SelectionEnd >= ZoomStart && SelectionEnd <= ZoomEnd && ZoomStart != ZoomEnd && SelectionStart != null)
				{
					_selEndElement.Visibility = Visibility.Visible;
					Canvas.SetLeft(
						_selEndElement,
						decimal.ToDouble(
							((decimal)SelectionEnd - ZoomStart) * ((decimal)_mainPanel.ActualWidth / (ZoomEnd - ZoomStart))));
				}
				else
				{
					_selEndElement.Visibility = Visibility.Hidden;
				}

				// Position the selection elements on the secondary axis
				switch (SelectionElementAlignment)
				{
					case JLR.Utility.NET.Position.Top:
					case JLR.Utility.NET.Position.Left:
						newRangeTop = 0;
						Canvas.SetTop(_selStartElement, newRangeTop);
						Canvas.SetTop(_selEndElement,   newRangeTop);
						break;
					case JLR.Utility.NET.Position.Middle:
					case JLR.Utility.NET.Position.Center:
						newRangeTop = (_mainPanel.ActualHeight - newRangeHeight) / 2;
						Canvas.SetTop(_selStartElement, newRangeTop);
						Canvas.SetTop(_selEndElement,   newRangeTop);
						break;
					case JLR.Utility.NET.Position.Bottom:
					case JLR.Utility.NET.Position.Right:
						newRangeTop = _mainPanel.ActualHeight - newRangeHeight;
						Canvas.SetTop(_selStartElement, newRangeTop);
						Canvas.SetTop(_selEndElement,   newRangeTop);
						break;
				}
			}

			if (_selHighlightElement != null)
			{
				if (SelectionEnd - SelectionStart != 0 && SelectionStart < ZoomEnd && SelectionEnd > ZoomStart)
				{
					_selHighlightElement.Visibility = Visibility.Visible;

					// Resize the selection highlight element
					var newHighlightHeight = SelectionHighlightElementRelativeSize * newRangeHeight;
					_selHighlightElement.Height = newHighlightHeight;

					// Position the selection highlight element on the primary axis
					var adjustedStart = SelectionStart >= ZoomStart ? (decimal)SelectionStart : ZoomStart;
					var adjustedEnd   = SelectionEnd <= ZoomEnd ? (decimal)SelectionEnd : ZoomEnd;
					Canvas.SetLeft(
						_selHighlightElement,
						decimal.ToDouble((adjustedStart - ZoomStart) * ((decimal)_mainPanel.ActualWidth / (ZoomEnd - ZoomStart))));
					_selHighlightElement.Width = decimal.ToDouble(
						Math.Abs(adjustedEnd - adjustedStart) * (decimal)_mainPanel.ActualWidth / (ZoomEnd - ZoomStart));

					// Position the selection highlight element on the secondary axis
					switch (SelectionHighlightElementAlignment)
					{
						case JLR.Utility.NET.Position.Top:
						case JLR.Utility.NET.Position.Left:
							Canvas.SetTop(_selHighlightElement, newRangeTop);
							break;
						case JLR.Utility.NET.Position.Middle:
						case JLR.Utility.NET.Position.Center:
							Canvas.SetTop(_selHighlightElement, newRangeTop + (newRangeHeight - newHighlightHeight) / 2);
							break;
						case JLR.Utility.NET.Position.Bottom:
						case JLR.Utility.NET.Position.Right:
							Canvas.SetTop(_selHighlightElement, newRangeTop + (newRangeHeight - newHighlightHeight));
							break;
					}
				}
				else
				{
					_selHighlightElement.Visibility = Visibility.Hidden;
				}
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
						(ZoomStart - Minimum) * ((decimal)_zoomPanel.ActualWidth / (Maximum - Minimum)) -
						(decimal)_zoomStartElement.ActualWidth));
			}

			// Position and resize the zoom end element
			if (_zoomEndElement != null)
			{
				_zoomEndElement.Height = _zoomPanel.ActualHeight;
				Canvas.SetTop(_zoomEndElement, 0);
				Canvas.SetLeft(
					_zoomEndElement,
					decimal.ToDouble((ZoomEnd - Minimum) * ((decimal)_zoomPanel.ActualWidth / (Maximum - Minimum))));
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
						decimal.ToDouble((ZoomStart - Minimum) * ((decimal)_zoomPanel.ActualWidth / (Maximum - Minimum))));
					_zoomThumbElement.Width = decimal.ToDouble(
						Math.Abs(ZoomEnd - ZoomStart) * (decimal)_zoomPanel.ActualWidth / (Maximum - Minimum));
				}
				else
				{
					_zoomThumbElement.Visibility = Visibility.Hidden;
				}
			}
		}

		private void AdjustPaddingToFit()
		{
			if (!IsLoaded)
				return;

			var newMargin = new Thickness(0, 0, 0, 0);
			if (_mainPanel != null)
			{
				var mainSpaceNeededLeft = _positionElement?.ActualWidth / 2 ?? 0;
				if (_selStartElement != null)
					mainSpaceNeededLeft = Math.Max(mainSpaceNeededLeft, _selStartElement.ActualWidth) - Padding.Left;

				var mainSpaceNeededRight = _positionElement?.ActualWidth / 2 ?? 0;
				if (_selEndElement != null)
					mainSpaceNeededRight = Math.Max(mainSpaceNeededRight, _selEndElement.ActualWidth) - Padding.Right;

				if (mainSpaceNeededLeft > 0)
					newMargin.Left = mainSpaceNeededLeft;
				if (mainSpaceNeededRight > 0)
					newMargin.Right = mainSpaceNeededRight;
				_mainPanel.SetCurrentValue(MarginProperty, newMargin);
			}

			if (_zoomPanel == null)
				return;

			var zoomSpaceNeededLeft  = _zoomStartElement?.ActualWidth - Padding.Left ?? 0;
			var zoomSpaceNeededRight = _zoomEndElement?.ActualWidth - Padding.Right ?? 0;

			newMargin = new Thickness(0, 0, 0, 0);
			if (zoomSpaceNeededLeft > 0)
				newMargin.Left = zoomSpaceNeededLeft;
			if (zoomSpaceNeededRight > 0)
				newMargin.Right = zoomSpaceNeededRight;
			_zoomPanel.SetCurrentValue(MarginProperty, newMargin);
		}

		private void InitializeSnap()
		{
			_fpsDivisors = new LinkedList<int>();
			foreach (var divisor in MathHelper.Divisors((ulong)FramesPerSecond, true))
			{
				_fpsDivisors.AddLast((int)divisor);
			}

			_currentFpsDivisor = _fpsDivisors.First;
			_snap              = (decimal)_currentFpsDivisor.Value / FramesPerSecond;
			_tickBar?.SetTickFrequencies(1, _snap);
		}
		#endregion
	}
}