// ┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  FILE:       GlyphPixelGrid.cs
// ┃  PROJECT:    Utility.WPF
// ┃  SOLUTION:   Utility.NET
// ┃  CREATED:    2015-12-25 @ 1:22 AM
// ┣━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  AUTHOR:     Jonathan Ruisi
// ┃  EMAIL:      JonathanRuisi@gmail.com
// ┣━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  GIT REPO:   https://github.com/jonathanruisi/Utility.NET
// ┃  LICENSE:    https://github.com/jonathanruisi/Utility.NET/blob/master/LICENSE
// ┗━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

using System;
using System.Windows;
using System.Windows.Media;

using JLR.Utility.NET.Color;

namespace JLR.Utility.WPF.Elements
{
	public class GlyphPixelGrid : PixelGrid
	{
		#region Properties (Glyph Metrics)
		public int AscentHeight
		{
			get { return (int)GetValue(AscentHeightProperty); }
			set { SetValue(AscentHeightProperty, value); }
		}

		public static readonly DependencyProperty AscentHeightProperty = DependencyProperty.Register(
			"AscentHeight",
			typeof(int),
			typeof(GlyphPixelGrid),
			new FrameworkPropertyMetadata(
				0,
				FrameworkPropertyMetadataOptions.AffectsRender,
				OnDependencyPropertyChanged,
				CoerceHorizontalGuide));

		public int DescentHeight
		{
			get { return (int)GetValue(DescentHeightProperty); }
			set { SetValue(DescentHeightProperty, value); }
		}

		public static readonly DependencyProperty DescentHeightProperty = DependencyProperty.Register(
			"DescentHeight",
			typeof(int),
			typeof(GlyphPixelGrid),
			new FrameworkPropertyMetadata(
				0,
				FrameworkPropertyMetadataOptions.AffectsRender,
				OnDependencyPropertyChanged,
				CoerceHorizontalGuide));

		public int UppercaseHeight
		{
			get { return (int)GetValue(UppercaseHeightProperty); }
			set { SetValue(UppercaseHeightProperty, value); }
		}

		public static readonly DependencyProperty UppercaseHeightProperty = DependencyProperty.Register(
			"UppercaseHeight",
			typeof(int),
			typeof(GlyphPixelGrid),
			new FrameworkPropertyMetadata(
				0,
				FrameworkPropertyMetadataOptions.AffectsRender,
				OnDependencyPropertyChanged,
				CoerceHorizontalGuide));

		public int LowercaseHeight
		{
			get { return (int)GetValue(LowercaseHeightProperty); }
			set { SetValue(LowercaseHeightProperty, value); }
		}

		public static readonly DependencyProperty LowercaseHeightProperty = DependencyProperty.Register(
			"LowercaseHeight",
			typeof(int),
			typeof(GlyphPixelGrid),
			new FrameworkPropertyMetadata(
				0,
				FrameworkPropertyMetadataOptions.AffectsRender,
				OnDependencyPropertyChanged,
				CoerceHorizontalGuide));

		public int LeftBearingWidth
		{
			get { return (int)GetValue(LeftBearingWidthProperty); }
			set { SetValue(LeftBearingWidthProperty, value); }
		}

		public static readonly DependencyProperty LeftBearingWidthProperty = DependencyProperty.Register(
			"LeftBearingWidth",
			typeof(int),
			typeof(GlyphPixelGrid),
			new FrameworkPropertyMetadata(
				0,
				FrameworkPropertyMetadataOptions.AffectsRender,
				OnDependencyPropertyChanged,
				CoerceVerticalGuide));

		public int RightBearingWidth
		{
			get { return (int)GetValue(RightBearingWidthProperty); }
			set { SetValue(RightBearingWidthProperty, value); }
		}

		public static readonly DependencyProperty RightBearingWidthProperty = DependencyProperty.Register(
			"RightBearingWidth",
			typeof(int),
			typeof(GlyphPixelGrid),
			new FrameworkPropertyMetadata(
				0,
				FrameworkPropertyMetadataOptions.AffectsRender,
				OnDependencyPropertyChanged,
				CoerceVerticalGuide));
		#endregion

		#region Properties (Guide Brushes)
		public Brush AscentOverlayBrush
		{
			get { return (Brush)GetValue(AscentOverlayBrushProperty); }
			set { SetValue(AscentOverlayBrushProperty, value); }
		}

		public static readonly DependencyProperty AscentOverlayBrushProperty = DependencyProperty.Register(
			"AscentOverlayBrush",
			typeof(Brush),
			typeof(PixelGrid),
			new FrameworkPropertyMetadata(
				new SolidColorBrush(new Rgba(0, 0, 0, 48).ToSystemWindowsMediaColor()),
				FrameworkPropertyMetadataOptions.AffectsRender,
				OnDependencyPropertyChanged));

		public Brush DescentOverlayBrush
		{
			get { return (Brush)GetValue(DescentOverlayBrushProperty); }
			set { SetValue(DescentOverlayBrushProperty, value); }
		}

		public static readonly DependencyProperty DescentOverlayBrushProperty = DependencyProperty.Register(
			"DescentOverlayBrush",
			typeof(Brush),
			typeof(PixelGrid),
			new FrameworkPropertyMetadata(
				new SolidColorBrush(new Rgba(0, 0, 0, 48).ToSystemWindowsMediaColor()),
				FrameworkPropertyMetadataOptions.AffectsRender,
				OnDependencyPropertyChanged));

		public Brush BearingOverlayBrush
		{
			get { return (Brush)GetValue(BearingOverlayBrushProperty); }
			set { SetValue(BearingOverlayBrushProperty, value); }
		}

		public static readonly DependencyProperty BearingOverlayBrushProperty = DependencyProperty.Register(
			"BearingOverlayBrush",
			typeof(Brush),
			typeof(PixelGrid),
			new FrameworkPropertyMetadata(
				new SolidColorBrush(new Rgba(0, 0, 0, 48).ToSystemWindowsMediaColor()),
				FrameworkPropertyMetadataOptions.AffectsRender,
				OnDependencyPropertyChanged));

		public Brush BaselineGuideBrush
		{
			get { return (Brush)GetValue(BaselineGuideBrushProperty); }
			set { SetValue(BaselineGuideBrushProperty, value); }
		}

		public static readonly DependencyProperty BaselineGuideBrushProperty = DependencyProperty.Register(
			"BaselineGuideBrush",
			typeof(Brush),
			typeof(PixelGrid),
			new FrameworkPropertyMetadata(
				Brushes.Blue,
				FrameworkPropertyMetadataOptions.AffectsRender,
				OnDependencyPropertyChanged));

		public Brush UppercaseGuideBrush
		{
			get { return (Brush)GetValue(UppercaseGuideBrushProperty); }
			set { SetValue(UppercaseGuideBrushProperty, value); }
		}

		public static readonly DependencyProperty UppercaseGuideBrushProperty = DependencyProperty.Register(
			"UppercaseGuideBrush",
			typeof(Brush),
			typeof(PixelGrid),
			new FrameworkPropertyMetadata(
				Brushes.LimeGreen,
				FrameworkPropertyMetadataOptions.AffectsRender,
				OnDependencyPropertyChanged));

		public Brush LowercaseGuideBrush
		{
			get { return (Brush)GetValue(LowercaseGuideBrushProperty); }
			set { SetValue(LowercaseGuideBrushProperty, value); }
		}

		public static readonly DependencyProperty LowercaseGuideBrushProperty = DependencyProperty.Register(
			"LowercaseGuideBrush",
			typeof(Brush),
			typeof(PixelGrid),
			new FrameworkPropertyMetadata(
				Brushes.DeepPink,
				FrameworkPropertyMetadataOptions.AffectsRender,
				OnDependencyPropertyChanged));

		public Brush BearingGuideBrush
		{
			get { return (Brush)GetValue(BearingGuideBrushProperty); }
			set { SetValue(BearingGuideBrushProperty, value); }
		}

		public static readonly DependencyProperty BearingGuideBrushProperty = DependencyProperty.Register(
			"BearingGuideBrush",
			typeof(Brush),
			typeof(PixelGrid),
			new FrameworkPropertyMetadata(
				Brushes.DeepSkyBlue,
				FrameworkPropertyMetadataOptions.AffectsRender,
				OnDependencyPropertyChanged));
		#endregion

		#region Properties (Guide Thickness)
		public double BaselineGuideThickness
		{
			get { return (double)GetValue(BaselineGuideThicknessProperty); }
			set { SetValue(BaselineGuideThicknessProperty, value); }
		}

		public static readonly DependencyProperty BaselineGuideThicknessProperty = DependencyProperty.Register(
			"BaselineGuideThickness",
			typeof(double),
			typeof(PixelGrid),
			new FrameworkPropertyMetadata(5.0, FrameworkPropertyMetadataOptions.AffectsRender, OnDependencyPropertyChanged));

		public double UppercaseGuideThickness
		{
			get { return (double)GetValue(UppercaseGuideThicknessProperty); }
			set { SetValue(UppercaseGuideThicknessProperty, value); }
		}

		public static readonly DependencyProperty UppercaseGuideThicknessProperty = DependencyProperty.Register(
			"UppercaseGuideThickness",
			typeof(double),
			typeof(PixelGrid),
			new FrameworkPropertyMetadata(3.0, FrameworkPropertyMetadataOptions.AffectsRender, OnDependencyPropertyChanged));

		public double LowercaseGuideThickness
		{
			get { return (double)GetValue(LowercaseGuideThicknessProperty); }
			set { SetValue(LowercaseGuideThicknessProperty, value); }
		}

		public static readonly DependencyProperty LowercaseGuideThicknessProperty = DependencyProperty.Register(
			"LowercaseGuideThickness",
			typeof(double),
			typeof(PixelGrid),
			new FrameworkPropertyMetadata(3.0, FrameworkPropertyMetadataOptions.AffectsRender, OnDependencyPropertyChanged));

		public double BearingGuideThickness
		{
			get { return (double)GetValue(BearingGuideThicknessProperty); }
			set { SetValue(BearingGuideThicknessProperty, value); }
		}

		public static readonly DependencyProperty BearingGuideThicknessProperty = DependencyProperty.Register(
			"BearingGuideThickness",
			typeof(double),
			typeof(PixelGrid),
			new FrameworkPropertyMetadata(3.0, FrameworkPropertyMetadataOptions.AffectsRender, OnDependencyPropertyChanged));
		#endregion

		#region Properties (Guide Visibility)
		public bool ExtendGuideLines
		{
			get { return (bool)GetValue(ExtendGuideLinesProperty); }
			set { SetValue(ExtendGuideLinesProperty, value); }
		}

		public static readonly DependencyProperty ExtendGuideLinesProperty = DependencyProperty.Register(
			"ExtendGuideLines",
			typeof(bool),
			typeof(GlyphPixelGrid),
			new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender, OnDependencyPropertyChanged));

		public bool IsAscentOverlayVisible
		{
			get { return (bool)GetValue(IsAscentOverlayVisibleProperty); }
			set { SetValue(IsAscentOverlayVisibleProperty, value); }
		}

		public static readonly DependencyProperty IsAscentOverlayVisibleProperty = DependencyProperty.Register(
			"IsAscentOverlayVisible",
			typeof(bool),
			typeof(GlyphPixelGrid),
			new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender, OnDependencyPropertyChanged));

		public bool IsDescentOverlayVisible
		{
			get { return (bool)GetValue(IsDescentOverlayVisibleProperty); }
			set { SetValue(IsDescentOverlayVisibleProperty, value); }
		}

		public static readonly DependencyProperty IsDescentOverlayVisibleProperty = DependencyProperty.Register(
			"IsDescentOverlayVisible",
			typeof(bool),
			typeof(GlyphPixelGrid),
			new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender, OnDependencyPropertyChanged));

		public bool IsBearingOverlayVisible
		{
			get { return (bool)GetValue(IsBearingOverlayVisibleProperty); }
			set { SetValue(IsBearingOverlayVisibleProperty, value); }
		}

		public static readonly DependencyProperty IsBearingOverlayVisibleProperty = DependencyProperty.Register(
			"IsBearingOverlayVisible",
			typeof(bool),
			typeof(GlyphPixelGrid),
			new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender, OnDependencyPropertyChanged));

		public bool IsBaselineGuideVisible
		{
			get { return (bool)GetValue(IsBaselineGuideVisibleProperty); }
			set { SetValue(IsBaselineGuideVisibleProperty, value); }
		}

		public static readonly DependencyProperty IsBaselineGuideVisibleProperty = DependencyProperty.Register(
			"IsBaselineGuideVisible",
			typeof(bool),
			typeof(GlyphPixelGrid),
			new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender, OnDependencyPropertyChanged));

		public bool IsUppercaseGuideVisible
		{
			get { return (bool)GetValue(IsUppercaseGuideVisibleProperty); }
			set { SetValue(IsUppercaseGuideVisibleProperty, value); }
		}

		public static readonly DependencyProperty IsUppercaseGuideVisibleProperty = DependencyProperty.Register(
			"IsUppercaseGuideVisible",
			typeof(bool),
			typeof(GlyphPixelGrid),
			new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender, OnDependencyPropertyChanged));

		public bool IsLowercaseGuideVisible
		{
			get { return (bool)GetValue(IsLowercaseGuideVisibleProperty); }
			set { SetValue(IsLowercaseGuideVisibleProperty, value); }
		}

		public static readonly DependencyProperty IsLowercaseGuideVisibleProperty = DependencyProperty.Register(
			"IsLowercaseGuideVisible",
			typeof(bool),
			typeof(GlyphPixelGrid),
			new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender, OnDependencyPropertyChanged));

		public bool IsBearingGuideVisible
		{
			get { return (bool)GetValue(IsBearingGuideVisibleProperty); }
			set { SetValue(IsBearingGuideVisibleProperty, value); }
		}

		public static readonly DependencyProperty IsBearingGuideVisibleProperty = DependencyProperty.Register(
			"IsBearingGuideVisible",
			typeof(bool),
			typeof(GlyphPixelGrid),
			new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender, OnDependencyPropertyChanged));
		#endregion

		#region Constructors
		static GlyphPixelGrid()
		{
			DefaultStyleKeyProperty.OverrideMetadata(
				typeof(GlyphPixelGrid),
				new FrameworkPropertyMetadata(typeof(GlyphPixelGrid)));
			PixelGridWidthProperty.OverrideMetadata(
				typeof(GlyphPixelGrid),
				new FrameworkPropertyMetadata(OnDependencyPropertyChanged));
			PixelGridHeightProperty.OverrideMetadata(
				typeof(GlyphPixelGrid),
				new FrameworkPropertyMetadata(OnDependencyPropertyChanged));
		}

		public GlyphPixelGrid()
		{
			Loaded += GlyphPixelGrid_Loaded;
		}
		#endregion

		#region Public Methods
		public void AdjustGuidesToFit()
		{
			var ascenderDescenderHeight =
				PixelGridHeight < 8 ? 0 : (int)(Math.Round(PixelGridHeight / 8.0, MidpointRounding.ToEven) / 2);
			var bearingWidth = PixelGridWidth < 8 ? 0 : (int)(Math.Round(PixelGridWidth / 8.0, MidpointRounding.ToEven) / 2);
			var boxHeight    = PixelGridHeight - ascenderDescenderHeight * 2;

			AscentHeight      = ascenderDescenderHeight;
			DescentHeight     = ascenderDescenderHeight;
			LeftBearingWidth  = bearingWidth;
			RightBearingWidth = bearingWidth;
			UppercaseHeight   = boxHeight;
			LowercaseHeight =
				boxHeight < 8 ? UppercaseHeight : (int)Math.Round(boxHeight * 0.66, MidpointRounding.AwayFromZero);
		}
		#endregion

		#region Property Changed Callbacks, Coerce Value Callbacks, Validate Value Callbacks
		private static void OnDependencyPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var pixelGrid = d as GlyphPixelGrid;
			if (pixelGrid == null) return;

			if (e.Property == PixelGridWidthProperty || e.Property == PixelGridHeightProperty)
			{
				pixelGrid.AdjustGuidesToFit();
			}
			else if (e.Property == LowercaseHeightProperty)
			{
				if (pixelGrid.LowercaseHeight > pixelGrid.UppercaseHeight)
					pixelGrid.UppercaseHeight = pixelGrid.LowercaseHeight;
			}
			else if (e.Property == UppercaseHeightProperty)
			{
				if (pixelGrid.UppercaseHeight > pixelGrid.PixelGridHeight - pixelGrid.AscentHeight - pixelGrid.DescentHeight)
					pixelGrid.AscentHeight = pixelGrid.PixelGridHeight - pixelGrid.UppercaseHeight - pixelGrid.DescentHeight;
				if (pixelGrid.UppercaseHeight < pixelGrid.LowercaseHeight)
					pixelGrid.LowercaseHeight = pixelGrid.UppercaseHeight;
			}
			else if (e.Property == LeftBearingWidthProperty)
			{
				if (pixelGrid.RightBearingWidth > pixelGrid.PixelGridWidth - pixelGrid.LeftBearingWidth)
					pixelGrid.RightBearingWidth = pixelGrid.PixelGridWidth - pixelGrid.LeftBearingWidth;
			}
			else if (e.Property == RightBearingWidthProperty)
			{
				if (pixelGrid.LeftBearingWidth > pixelGrid.PixelGridWidth - pixelGrid.RightBearingWidth)
					pixelGrid.LeftBearingWidth = pixelGrid.PixelGridWidth - pixelGrid.RightBearingWidth;
			}
			else if (e.Property == AscentHeightProperty)
			{
				if (pixelGrid.AscentHeight > pixelGrid.PixelGridHeight - pixelGrid.UppercaseHeight - pixelGrid.DescentHeight)
					pixelGrid.UppercaseHeight = pixelGrid.PixelGridHeight - pixelGrid.AscentHeight - pixelGrid.DescentHeight;
				if (pixelGrid.AscentHeight > pixelGrid.PixelGridHeight - pixelGrid.DescentHeight)
					pixelGrid.DescentHeight = pixelGrid.PixelGridHeight - pixelGrid.AscentHeight;
			}
			else if (e.Property == DescentHeightProperty)
			{
				if (pixelGrid.DescentHeight > pixelGrid.PixelGridHeight - pixelGrid.AscentHeight - pixelGrid.UppercaseHeight)
					pixelGrid.UppercaseHeight = pixelGrid.PixelGridHeight - pixelGrid.AscentHeight - pixelGrid.DescentHeight;
				if (pixelGrid.DescentHeight > pixelGrid.PixelGridHeight - pixelGrid.AscentHeight)
					pixelGrid.AscentHeight = pixelGrid.PixelGridHeight - pixelGrid.DescentHeight;
			}
		}

		private static object CoerceHorizontalGuide(DependencyObject d, object value)
		{
			var pixelGrid = d as GlyphPixelGrid;
			var current   = (int)value;
			if (pixelGrid == null || current < 0 || current > pixelGrid.PixelGridHeight)
				return DependencyProperty.UnsetValue;
			return current;
		}

		private static object CoerceVerticalGuide(DependencyObject d, object value)
		{
			var pixelGrid = d as GlyphPixelGrid;
			var current   = (int)value;
			if (pixelGrid == null || current < 0 || current > pixelGrid.PixelGridWidth)
				return DependencyProperty.UnsetValue;
			return current;
		}
		#endregion

		#region Event Handlers
		private void GlyphPixelGrid_Loaded(object sender, RoutedEventArgs e)
		{
			AdjustGuidesToFit();
		}
		#endregion

		#region Render Methods
		protected override void OnRender(DrawingContext drawingContext)
		{
			base.OnRender(drawingContext);

			// Define pens
			var baselineGuidePen = new Pen(BaselineGuideBrush, BaselineGuideThickness)
			{
				StartLineCap = PenLineCap.Round,
				EndLineCap   = PenLineCap.Round,
				DashCap      = PenLineCap.Flat
			};
			var lowercaseGuidePen = new Pen(LowercaseGuideBrush, LowercaseGuideThickness)
			{
				StartLineCap = PenLineCap.Round,
				EndLineCap   = PenLineCap.Round,
				DashCap      = PenLineCap.Flat
			};
			var uppercaseGuidePen = new Pen(UppercaseGuideBrush, UppercaseGuideThickness)
			{
				StartLineCap = PenLineCap.Round,
				EndLineCap   = PenLineCap.Round,
				DashCap      = PenLineCap.Flat
			};
			var bearingGuidePen = new Pen(BearingGuideBrush, BearingGuideThickness)
			{
				StartLineCap = PenLineCap.Round,
				EndLineCap   = PenLineCap.Round,
				DashCap      = PenLineCap.Round
			};

			// Calculate guide coordinates
			var baselineY = (PixelGridHeight - DescentHeight) * PixelHeight +
				(PixelGridHeight - DescentHeight) * PixelBorderThickness - PixelBorderThickness / 2.0;
			var lowercaseY = (PixelGridHeight - DescentHeight - LowercaseHeight) * PixelHeight +
				(PixelGridHeight - DescentHeight - LowercaseHeight) * PixelBorderThickness - PixelBorderThickness / 2.0;
			var uppercaseY = (PixelGridHeight - DescentHeight - UppercaseHeight) * PixelHeight +
				(PixelGridHeight - DescentHeight - UppercaseHeight) * PixelBorderThickness - PixelBorderThickness / 2.0;
			var bearingXMin = LeftBearingWidth == 0
				? 0
				: LeftBearingWidth * PixelWidth + (LeftBearingWidth + PixelBorderThickness) - PixelBorderThickness / 2.0;
			var bearingXMax = RightBearingWidth == 0
				? PixelGridWidth * PixelWidth + (PixelGridWidth - 1) * PixelBorderThickness
				: (PixelGridWidth - RightBearingWidth) * PixelWidth + (PixelGridWidth - RightBearingWidth + PixelBorderThickness) -
				PixelBorderThickness / 2.0;

			// Adjust pen strokes for overlapped guide lines
			var uTripleConstant = ActualWidth / PixelGridWidth / 6.0 * (1.0 / uppercaseGuidePen.Thickness);
			var lTripleConstant = ActualWidth / PixelGridWidth / 6.0 * (1.0 / lowercaseGuidePen.Thickness);
			var uDoubleConstant = ActualWidth / PixelGridWidth / 4.0 * (1.0 / uppercaseGuidePen.Thickness);
			var lDoubleConstant = ActualWidth / PixelGridWidth / 4.0 * (1.0 / lowercaseGuidePen.Thickness);

			if (bearingXMin.Equals(bearingXMax))
				bearingGuidePen.DashStyle = DashStyles.Dash;

			if (baselineY.Equals(lowercaseY) && baselineY.Equals(uppercaseY))
			{
				uppercaseGuidePen.DashStyle = new DashStyle(
					new[] { uTripleConstant * 2, uTripleConstant * 4 },
					uTripleConstant * 3);
				lowercaseGuidePen.DashStyle = new DashStyle(
					new[] { lTripleConstant * 2, lTripleConstant * 4 },
					lTripleConstant * 5);
			}
			else if (baselineY.Equals(lowercaseY))
			{
				lowercaseGuidePen.DashStyle = new DashStyle(
					new[] { lDoubleConstant * 2, lDoubleConstant * 2 },
					lDoubleConstant * 3);
			}
			else if (lowercaseY.Equals(uppercaseY))
			{
				uppercaseGuidePen.DashStyle = new DashStyle(new[] { uDoubleConstant * 2, uDoubleConstant * 2 }, uDoubleConstant);
				lowercaseGuidePen.DashStyle = new DashStyle(
					new[] { lDoubleConstant * 2, lDoubleConstant * 2 },
					lDoubleConstant * 3);
			}

			// Draw ascent overlay
			double currentX, currentY;
			if (IsAscentOverlayVisible)
			{
				for (var y = 0; y < AscentHeight; y++)
				{
					for (var x = 0; x < PixelGridWidth; x++)
					{
						currentX = x * PixelWidth + x * PixelBorderThickness;
						currentY = y * PixelHeight + y * PixelBorderThickness;
						if (!Point.Equals(MouseOverPixelCoordinates, new Point(x, y)))
						{
							drawingContext.DrawRectangle(AscentOverlayBrush, null, new Rect(currentX, currentY, PixelWidth, PixelHeight));
						}
					}
				}
			}

			// Draw descent overlay
			if (IsDescentOverlayVisible)
			{
				for (var y = 0; y < DescentHeight; y++)
				{
					for (var x = 0; x < PixelGridWidth; x++)
					{
						currentX = x * PixelWidth + x * PixelBorderThickness;
						currentY = (PixelGridHeight - DescentHeight + y) * PixelHeight +
							(PixelGridHeight - DescentHeight + y) * PixelBorderThickness;
						if (!Point.Equals(MouseOverPixelCoordinates, new Point(x, y)))
						{
							drawingContext.DrawRectangle(DescentOverlayBrush, null, new Rect(currentX, currentY, PixelWidth, PixelHeight));
						}
					}
				}
			}

			// Draw bearing overlay
			if (IsBearingOverlayVisible)
			{
				// Left bearing
				for (var y = 0; y < PixelGridHeight; y++)
				{
					for (var x = 0; x < LeftBearingWidth; x++)
					{
						currentX = x * PixelWidth + x * PixelBorderThickness;
						currentY = y * PixelHeight + y * PixelBorderThickness;
						if (!Point.Equals(MouseOverPixelCoordinates, new Point(x, y)))
						{
							drawingContext.DrawRectangle(BearingOverlayBrush, null, new Rect(currentX, currentY, PixelWidth, PixelHeight));
						}
					}
				}

				// Right bearing
				for (var y = 0; y < PixelGridHeight; y++)
				{
					for (var x = 0; x < RightBearingWidth; x++)
					{
						currentX = (PixelGridWidth - RightBearingWidth + x) * PixelWidth +
							(PixelGridWidth - RightBearingWidth + x) * PixelBorderThickness;
						currentY = y * PixelHeight + y * PixelBorderThickness;
						if (!Point.Equals(MouseOverPixelCoordinates, new Point(x, y)))
						{
							drawingContext.DrawRectangle(BearingOverlayBrush, null, new Rect(currentX, currentY, PixelWidth, PixelHeight));
						}
					}
				}
			}

			// Draw guide lines
			if (IsBearingGuideVisible && BearingGuideThickness > 0)
			{
				drawingContext.DrawLine(
					bearingGuidePen,
					new Point(bearingXMin, ExtendGuideLines ? 0 : uppercaseY),
					new Point(bearingXMin, ExtendGuideLines ? ActualHeight : baselineY));
				drawingContext.DrawLine(
					bearingGuidePen,
					new Point(bearingXMax, ExtendGuideLines ? 0 : uppercaseY),
					new Point(bearingXMax, ExtendGuideLines ? ActualHeight : baselineY));
			}

			if (IsBaselineGuideVisible && BaselineGuideThickness > 0)
			{
				drawingContext.DrawLine(baselineGuidePen, new Point(0, baselineY), new Point(ActualWidth, baselineY));
			}

			if (IsLowercaseGuideVisible && LowercaseGuideThickness > 0)
			{
				drawingContext.DrawLine(
					lowercaseGuidePen,
					new Point(ExtendGuideLines ? 0 : bearingXMin,           lowercaseY),
					new Point(ExtendGuideLines ? ActualWidth : bearingXMax, lowercaseY));
			}

			if (IsUppercaseGuideVisible && UppercaseGuideThickness > 0)
			{
				drawingContext.DrawLine(
					uppercaseGuidePen,
					new Point(ExtendGuideLines ? 0 : bearingXMin,           uppercaseY),
					new Point(ExtendGuideLines ? ActualWidth : bearingXMax, uppercaseY));
			}
		}
		#endregion
	}
}