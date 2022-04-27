// ┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  FILE:       LineGraph.cs
// ┃  PROJECT:    Utility.WPF
// ┃  SOLUTION:   Utility.NET
// ┃  CREATED:    2017-03-20 @ 7:01 PM
// ┣━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  AUTHOR:     Jonathan Ruisi
// ┃  EMAIL:      JonathanRuisi@gmail.com
// ┣━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  GIT REPO:   https://github.com/jonathanruisi/Utility.NET
// ┃  LICENSE:    https://github.com/jonathanruisi/Utility.NET/blob/master/LICENSE
// ┗━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;

namespace JLR.Utility.WPF.Elements
{
	[ContentProperty("Data")]
	public class LineGraph : FrameworkElement
	{
		#region Fields
		private List<FormattedText> _xAxisValueTextList, _yAxisValueTextList;
		private FormattedText       _titleText,          _xAxisLabelText, _yAxisLabelText;
		private Point               _plotTopLeft;
		private double              _plotBottomSpace;
		#endregion

		#region Properties (Graph)
		[Category("LineGraph")]
		public ObservableCollection<Point> Data
		{
			get { return (ObservableCollection<Point>)GetValue(DataProperty); }
			set { SetValue(DataProperty, value); }
		}

		public static readonly DependencyProperty DataProperty = DependencyProperty.Register(
			"Data",
			typeof(ObservableCollection<Point>),
			typeof(LineGraph),
			new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, OnDataPropertyChanged));

		public double XMin { get { return (double)GetValue(XMinProperty); } set { SetValue(XMinProperty, value); } }

		public static readonly DependencyProperty XMinProperty = DependencyProperty.Register(
			"XMin",
			typeof(double),
			typeof(LineGraph),
			new FrameworkPropertyMetadata(
				0.0,
				FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

		public double XMax { get { return (double)GetValue(XMaxProperty); } set { SetValue(XMaxProperty, value); } }

		public static readonly DependencyProperty XMaxProperty = DependencyProperty.Register(
			"XMax",
			typeof(double),
			typeof(LineGraph),
			new FrameworkPropertyMetadata(
				0.0,
				FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

		public double YMin { get { return (double)GetValue(YMinProperty); } set { SetValue(YMinProperty, value); } }

		public static readonly DependencyProperty YMinProperty = DependencyProperty.Register(
			"YMin",
			typeof(double),
			typeof(LineGraph),
			new FrameworkPropertyMetadata(
				0.0,
				FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

		public double YMax { get { return (double)GetValue(YMaxProperty); } set { SetValue(YMaxProperty, value); } }

		public static readonly DependencyProperty YMaxProperty = DependencyProperty.Register(
			"YMax",
			typeof(double),
			typeof(LineGraph),
			new FrameworkPropertyMetadata(
				0.0,
				FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

		public double XMajorGrid
		{
			get { return (double)GetValue(XMajorGridProperty); }
			set { SetValue(XMajorGridProperty, value); }
		}

		public static readonly DependencyProperty XMajorGridProperty = DependencyProperty.Register(
			"XMajorGrid",
			typeof(double),
			typeof(LineGraph),
			new FrameworkPropertyMetadata(
				0.0,
				FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

		public double XMinorGrid
		{
			get { return (double)GetValue(XMinorGridProperty); }
			set { SetValue(XMinorGridProperty, value); }
		}

		public static readonly DependencyProperty XMinorGridProperty = DependencyProperty.Register(
			"XMinorGrid",
			typeof(double),
			typeof(LineGraph),
			new FrameworkPropertyMetadata(
				0.0,
				FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

		public double YMajorGrid
		{
			get { return (double)GetValue(YMajorGridProperty); }
			set { SetValue(YMajorGridProperty, value); }
		}

		public static readonly DependencyProperty YMajorGridProperty = DependencyProperty.Register(
			"YMajorGrid",
			typeof(double),
			typeof(LineGraph),
			new FrameworkPropertyMetadata(
				0.0,
				FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

		public double YMinorGrid
		{
			get { return (double)GetValue(YMinorGridProperty); }
			set { SetValue(YMinorGridProperty, value); }
		}

		public static readonly DependencyProperty YMinorGridProperty = DependencyProperty.Register(
			"YMinorGrid",
			typeof(double),
			typeof(LineGraph),
			new FrameworkPropertyMetadata(
				0.0,
				FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

		public bool XAutoScale
		{
			get { return (bool)GetValue(XAutoScaleProperty); }
			set { SetValue(XAutoScaleProperty, value); }
		}

		public static readonly DependencyProperty XAutoScaleProperty = DependencyProperty.Register(
			"XAutoScale",
			typeof(bool),
			typeof(LineGraph),
			new FrameworkPropertyMetadata(
				false,
				FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

		public bool YAutoScale
		{
			get { return (bool)GetValue(YAutoScaleProperty); }
			set { SetValue(YAutoScaleProperty, value); }
		}

		public static readonly DependencyProperty YAutoScaleProperty = DependencyProperty.Register(
			"YAutoScale",
			typeof(bool),
			typeof(LineGraph),
			new FrameworkPropertyMetadata(
				false,
				FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));
		#endregion

		#region Properties (Labels)
		public string Title { get { return (string)GetValue(TitleProperty); } set { SetValue(TitleProperty, value); } }

		public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
			"Title",
			typeof(string),
			typeof(LineGraph),
			new FrameworkPropertyMetadata(
				null,
				FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

		public string XAxisLabel
		{
			get { return (string)GetValue(XAxisLabelProperty); }
			set { SetValue(XAxisLabelProperty, value); }
		}

		public static readonly DependencyProperty XAxisLabelProperty = DependencyProperty.Register(
			"XAxisLabel",
			typeof(string),
			typeof(LineGraph),
			new FrameworkPropertyMetadata(
				null,
				FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

		public string YAxisLabel
		{
			get { return (string)GetValue(YAxisLabelProperty); }
			set { SetValue(YAxisLabelProperty, value); }
		}

		public static readonly DependencyProperty YAxisLabelProperty = DependencyProperty.Register(
			"YAxisLabel",
			typeof(string),
			typeof(LineGraph),
			new FrameworkPropertyMetadata(
				null,
				FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

		public double LabelSpacing
		{
			get { return (double)GetValue(LabelSpacingProperty); }
			set { SetValue(LabelSpacingProperty, value); }
		}

		public static readonly DependencyProperty LabelSpacingProperty = DependencyProperty.Register(
			"LabelSpacing",
			typeof(double),
			typeof(LineGraph),
			new FrameworkPropertyMetadata(
				2.0,
				FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

		public int XAxisValueLabelPrecision
		{
			get { return (int)GetValue(XAxisValueLabelPrecisionProperty); }
			set { SetValue(XAxisValueLabelPrecisionProperty, value); }
		}

		public static readonly DependencyProperty XAxisValueLabelPrecisionProperty = DependencyProperty.Register(
			"XAxisValueLabelPrecision",
			typeof(int),
			typeof(LineGraph),
			new FrameworkPropertyMetadata(
				0,
				FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

		public int YAxisValueLabelPrecision
		{
			get { return (int)GetValue(YAxisValueLabelPrecisionProperty); }
			set { SetValue(YAxisValueLabelPrecisionProperty, value); }
		}

		public static readonly DependencyProperty YAxisValueLabelPrecisionProperty = DependencyProperty.Register(
			"YAxisValueLabelPrecision",
			typeof(int),
			typeof(LineGraph),
			new FrameworkPropertyMetadata(
				0,
				FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));
		#endregion

		#region Properties (Thickness)
		public double XAxisThickness
		{
			get { return (double)GetValue(XAxisThicknessProperty); }
			set { SetValue(XAxisThicknessProperty, value); }
		}

		public static readonly DependencyProperty XAxisThicknessProperty = DependencyProperty.Register(
			"XAxisThickness",
			typeof(double),
			typeof(LineGraph),
			new FrameworkPropertyMetadata(
				1.0,
				FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

		public double YAxisThickness
		{
			get { return (double)GetValue(YAxisThicknessProperty); }
			set { SetValue(YAxisThicknessProperty, value); }
		}

		public static readonly DependencyProperty YAxisThicknessProperty = DependencyProperty.Register(
			"YAxisThickness",
			typeof(double),
			typeof(LineGraph),
			new FrameworkPropertyMetadata(
				1.0,
				FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

		public double MajorGridThickness
		{
			get { return (double)GetValue(MajorGridThicknessProperty); }
			set { SetValue(MajorGridThicknessProperty, value); }
		}

		public static readonly DependencyProperty MajorGridThicknessProperty = DependencyProperty.Register(
			"MajorGridThickness",
			typeof(double),
			typeof(LineGraph),
			new FrameworkPropertyMetadata(
				1.0,
				FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

		public double MinorGridThickness
		{
			get { return (double)GetValue(MinorGridThicknessProperty); }
			set { SetValue(MinorGridThicknessProperty, value); }
		}

		public static readonly DependencyProperty MinorGridThicknessProperty = DependencyProperty.Register(
			"MinorGridThickness",
			typeof(double),
			typeof(LineGraph),
			new FrameworkPropertyMetadata(
				0.5,
				FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

		public double PlotThickness
		{
			get { return (double)GetValue(PlotThicknessProperty); }
			set { SetValue(PlotThicknessProperty, value); }
		}

		public static readonly DependencyProperty PlotThicknessProperty = DependencyProperty.Register(
			"PlotThickness",
			typeof(double),
			typeof(LineGraph),
			new FrameworkPropertyMetadata(2.0, FrameworkPropertyMetadataOptions.AffectsRender));
		#endregion

		#region Properties (Brushes)
		public Brush XAxisBrush
		{
			get { return (Brush)GetValue(XAxisBrushProperty); }
			set { SetValue(XAxisBrushProperty, value); }
		}

		public static readonly DependencyProperty XAxisBrushProperty = DependencyProperty.Register(
			"XAxisBrush",
			typeof(Brush),
			typeof(LineGraph),
			new FrameworkPropertyMetadata(Brushes.Black, FrameworkPropertyMetadataOptions.AffectsRender));

		public Brush YAxisBrush
		{
			get { return (Brush)GetValue(YAxisBrushProperty); }
			set { SetValue(YAxisBrushProperty, value); }
		}

		public static readonly DependencyProperty YAxisBrushProperty = DependencyProperty.Register(
			"YAxisBrush",
			typeof(Brush),
			typeof(LineGraph),
			new FrameworkPropertyMetadata(Brushes.Black, FrameworkPropertyMetadataOptions.AffectsRender));

		public Brush MajorGridBrush
		{
			get { return (Brush)GetValue(MajorGridBrushProperty); }
			set { SetValue(MajorGridBrushProperty, value); }
		}

		public static readonly DependencyProperty MajorGridBrushProperty = DependencyProperty.Register(
			"MajorGridBrush",
			typeof(Brush),
			typeof(LineGraph),
			new FrameworkPropertyMetadata(Brushes.Black, FrameworkPropertyMetadataOptions.AffectsRender));

		public Brush MinorGridBrush
		{
			get { return (Brush)GetValue(MinorGridBrushProperty); }
			set { SetValue(MinorGridBrushProperty, value); }
		}

		public static readonly DependencyProperty MinorGridBrushProperty = DependencyProperty.Register(
			"MinorGridBrush",
			typeof(Brush),
			typeof(LineGraph),
			new FrameworkPropertyMetadata(Brushes.Gray, FrameworkPropertyMetadataOptions.AffectsRender));

		public Brush PlotBrush
		{
			get { return (Brush)GetValue(PlotBrushProperty); }
			set { SetValue(PlotBrushProperty, value); }
		}

		public static readonly DependencyProperty PlotBrushProperty = DependencyProperty.Register(
			"PlotBrush",
			typeof(Brush),
			typeof(LineGraph),
			new FrameworkPropertyMetadata(Brushes.Blue, FrameworkPropertyMetadataOptions.AffectsRender));

		public Brush Background
		{
			get { return (Brush)GetValue(BackgroundProperty); }
			set { SetValue(BackgroundProperty, value); }
		}

		public static readonly DependencyProperty BackgroundProperty = DependencyProperty.Register(
			"Background",
			typeof(Brush),
			typeof(LineGraph),
			new FrameworkPropertyMetadata(Brushes.Transparent, FrameworkPropertyMetadataOptions.AffectsRender));

		public Brush TitleBrush
		{
			get { return (Brush)GetValue(TitleBrushProperty); }
			set { SetValue(TitleBrushProperty, value); }
		}

		public static readonly DependencyProperty TitleBrushProperty = DependencyProperty.Register(
			"TitleBrush",
			typeof(Brush),
			typeof(LineGraph),
			new FrameworkPropertyMetadata(Brushes.Black, FrameworkPropertyMetadataOptions.AffectsRender));

		public Brush AxisLabelBrush
		{
			get { return (Brush)GetValue(AxisLabelBrushProperty); }
			set { SetValue(AxisLabelBrushProperty, value); }
		}

		public static readonly DependencyProperty AxisLabelBrushProperty = DependencyProperty.Register(
			"AxisLabelBrush",
			typeof(Brush),
			typeof(LineGraph),
			new FrameworkPropertyMetadata(Brushes.Black, FrameworkPropertyMetadataOptions.AffectsRender));

		public Brush AxisValueBrush
		{
			get { return (Brush)GetValue(AxisValueBrushProperty); }
			set { SetValue(AxisValueBrushProperty, value); }
		}

		public static readonly DependencyProperty AxisValueBrushProperty = DependencyProperty.Register(
			"AxisValueBrush",
			typeof(Brush),
			typeof(LineGraph),
			new FrameworkPropertyMetadata(Brushes.Black, FrameworkPropertyMetadataOptions.AffectsRender));
		#endregion

		#region Properties (Fonts)
		public FontFamily TitleFontFamily
		{
			get { return (FontFamily)GetValue(TitleFontFamilyProperty); }
			set { SetValue(TitleFontFamilyProperty, value); }
		}

		public static readonly DependencyProperty TitleFontFamilyProperty = DependencyProperty.Register(
			"TitleFontFamily",
			typeof(FontFamily),
			typeof(LineGraph),
			new FrameworkPropertyMetadata(
				null,
				FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

		public FontWeight TitleFontWeight
		{
			get { return (FontWeight)GetValue(TitleFontWeightProperty); }
			set { SetValue(TitleFontWeightProperty, value); }
		}

		public static readonly DependencyProperty TitleFontWeightProperty = DependencyProperty.Register(
			"TitleFontWeight",
			typeof(FontWeight),
			typeof(LineGraph),
			new FrameworkPropertyMetadata(
				FontWeights.Bold,
				FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

		public double TitleFontSize
		{
			get { return (double)GetValue(TitleFontSizeProperty); }
			set { SetValue(TitleFontSizeProperty, value); }
		}

		public static readonly DependencyProperty TitleFontSizeProperty = DependencyProperty.Register(
			"TitleFontSize",
			typeof(double),
			typeof(LineGraph),
			new FrameworkPropertyMetadata(
				18.0,
				FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

		public FontFamily AxisLabelFontFamily
		{
			get { return (FontFamily)GetValue(AxisLabelFontFamilyProperty); }
			set { SetValue(AxisLabelFontFamilyProperty, value); }
		}

		public static readonly DependencyProperty AxisLabelFontFamilyProperty = DependencyProperty.Register(
			"AxisLabelFontFamily",
			typeof(FontFamily),
			typeof(LineGraph),
			new FrameworkPropertyMetadata(
				null,
				FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

		public FontWeight AxisLabelFontWeight
		{
			get { return (FontWeight)GetValue(AxisLabelFontWeightProperty); }
			set { SetValue(AxisLabelFontWeightProperty, value); }
		}

		public static readonly DependencyProperty AxisLabelFontWeightProperty = DependencyProperty.Register(
			"AxisLabelFontWeight",
			typeof(FontWeight),
			typeof(LineGraph),
			new FrameworkPropertyMetadata(
				FontWeights.Bold,
				FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

		public double AxisLabelFontSize
		{
			get { return (double)GetValue(AxisLabelFontSizeProperty); }
			set { SetValue(AxisLabelFontSizeProperty, value); }
		}

		public static readonly DependencyProperty AxisLabelFontSizeProperty = DependencyProperty.Register(
			"AxisLabelFontSize",
			typeof(double),
			typeof(LineGraph),
			new FrameworkPropertyMetadata(
				14.0,
				FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

		public FontFamily AxisValueFontFamily
		{
			get { return (FontFamily)GetValue(AxisValueFontFamilyProperty); }
			set { SetValue(AxisValueFontFamilyProperty, value); }
		}

		public static readonly DependencyProperty AxisValueFontFamilyProperty = DependencyProperty.Register(
			"AxisValueFontFamily",
			typeof(FontFamily),
			typeof(LineGraph),
			new FrameworkPropertyMetadata(
				null,
				FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

		public FontWeight AxisValueFontWeight
		{
			get { return (FontWeight)GetValue(AxisValueFontWeightProperty); }
			set { SetValue(AxisValueFontWeightProperty, value); }
		}

		public static readonly DependencyProperty AxisValueFontWeightProperty = DependencyProperty.Register(
			"AxisValueFontWeight",
			typeof(FontWeight),
			typeof(LineGraph),
			new FrameworkPropertyMetadata(
				FontWeights.Regular,
				FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

		public double AxisValueFontSize
		{
			get { return (double)GetValue(AxisValueFontSizeProperty); }
			set { SetValue(AxisValueFontSizeProperty, value); }
		}

		public static readonly DependencyProperty AxisValueFontSizeProperty = DependencyProperty.Register(
			"AxisValueFontSize",
			typeof(double),
			typeof(LineGraph),
			new FrameworkPropertyMetadata(
				10.0,
				FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));
		#endregion

		#region Constructors
		static LineGraph()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(LineGraph), new FrameworkPropertyMetadata(typeof(LineGraph)));
		}

		public LineGraph()
		{
			_xAxisValueTextList = new List<FormattedText>();
			_yAxisValueTextList = new List<FormattedText>();
			SetCurrentValue(DataProperty,                new ObservableCollection<Point>());
			SetCurrentValue(TitleProperty,               string.Empty);
			SetCurrentValue(XAxisLabelProperty,          string.Empty);
			SetCurrentValue(YAxisLabelProperty,          string.Empty);
			SetCurrentValue(TitleFontFamilyProperty,     new FontFamily("Arial"));
			SetCurrentValue(AxisLabelFontFamilyProperty, new FontFamily("Arial"));
			SetCurrentValue(AxisValueFontFamilyProperty, new FontFamily("Arial"));
		}
		#endregion

		#region Property Changed Callbacks, Coerce Value Callbacks, Validate Value Callbacks, Change Notification Callbacks
		private static void OnDataPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var lineGraph = d as LineGraph;
			if (lineGraph == null) return;

			var oldList = e.OldValue as INotifyCollectionChanged;
			var newList = e.NewValue as INotifyCollectionChanged;

			if (oldList != null)
				oldList.CollectionChanged -= lineGraph.OnDataCollectionChanged;
			if (newList != null)
				newList.CollectionChanged += lineGraph.OnDataCollectionChanged;
		}

		private void OnDataCollectionChanged(object source, NotifyCollectionChangedEventArgs e)
		{
			InvalidateVisual();
		}
		#endregion

		#region Public Methods
		#endregion

		#region Private Methods
		private void UpdateTitleText()
		{
#pragma warning disable CS0618 // Type or member is obsolete
			_titleText = new FormattedText(
				Title,
				CultureInfo.CurrentCulture,
				FlowDirection.LeftToRight,
				new Typeface(TitleFontFamily, FontStyles.Normal, TitleFontWeight, FontStretches.Normal),
				TitleFontSize,
				TitleBrush);
#pragma warning restore CS0618 // Type or member is obsolete
		}

		private void UpdateAxisLabelText()
		{
#pragma warning disable CS0618 // Type or member is obsolete
			_xAxisLabelText = new FormattedText(
				XAxisLabel,
				CultureInfo.CurrentCulture,
				FlowDirection.LeftToRight,
				new Typeface(AxisLabelFontFamily, FontStyles.Normal, AxisLabelFontWeight, FontStretches.Normal),
				AxisLabelFontSize,
				AxisLabelBrush);
#pragma warning restore CS0618 // Type or member is obsolete

#pragma warning disable CS0618 // Type or member is obsolete
			_yAxisLabelText = new FormattedText(
				YAxisLabel,
				CultureInfo.CurrentCulture,
				FlowDirection.LeftToRight,
				new Typeface(AxisLabelFontFamily, FontStyles.Normal, AxisLabelFontWeight, FontStretches.Normal),
				AxisLabelFontSize,
				AxisLabelBrush);
#pragma warning restore CS0618 // Type or member is obsolete
		}

		private void UpdateAxisValueText()
		{
			_xAxisValueTextList.Clear();
			var formatStr = "F" + XAxisValueLabelPrecision;
			for (var i = XMin; i < XMax; i += XMajorGrid)
			{
#pragma warning disable CS0618 // Type or member is obsolete
				_xAxisValueTextList.Add(
					new FormattedText(
						i.ToString(formatStr),
						CultureInfo.CurrentCulture,
						FlowDirection.LeftToRight,
						new Typeface(AxisValueFontFamily, FontStyles.Normal, AxisValueFontWeight, FontStretches.Normal),
						AxisValueFontSize,
						AxisValueBrush));
#pragma warning restore CS0618 // Type or member is obsolete
			}

			_yAxisValueTextList.Clear();
			formatStr = "F" + YAxisValueLabelPrecision;
			for (var i = YMin; i < YMax; i += YMajorGrid)
			{
#pragma warning disable CS0618 // Type or member is obsolete
				_yAxisValueTextList.Add(
					new FormattedText(
						i.ToString(formatStr),
						CultureInfo.CurrentCulture,
						FlowDirection.LeftToRight,
						new Typeface(AxisValueFontFamily, FontStyles.Normal, AxisValueFontWeight, FontStretches.Normal),
						AxisValueFontSize,
						AxisValueBrush));
#pragma warning restore CS0618 // Type or member is obsolete
			}
		}
		#endregion

		#region Layout & Render Methods
		/// <inheritdoc />
		protected override Size MeasureOverride(Size availableSize)
		{
			// Auto-scale axes if appropriate
			if (XAutoScale)
				XMax = Data.Select(point => point.X).Concat(new[] { 0.0 }).Max();
			if (YAutoScale)
				YMax = Data.Select(point => point.Y).Concat(new[] { 0.0 }).Max();

			// Update FormattedText objects
			UpdateTitleText();
			UpdateAxisLabelText();
			UpdateAxisValueText();

			// Determine space required for formatted text strings
			double tallestXAxisLabel = 0.0, widestYAxisLabel = 0.0;
			foreach (FormattedText ft in _xAxisValueTextList)
			{
				if (ft.Height > tallestXAxisLabel)
					tallestXAxisLabel = ft.Height;
			}

			foreach (FormattedText ft in _yAxisValueTextList)
			{
				if (ft.Width > widestYAxisLabel)
					widestYAxisLabel = ft.Width;
			}

			// Determine the rectangle in which the plot will be drawn
			_plotTopLeft = new Point(
				widestYAxisLabel + LabelSpacing + _yAxisLabelText.Height + LabelSpacing,
				_titleText.Height + LabelSpacing);
			_plotBottomSpace = tallestXAxisLabel + LabelSpacing + _xAxisLabelText.Height + LabelSpacing;

			// Determine space required for gridlines
			var xMajorGridCount = (int)((XMax - XMin) / XMajorGrid) - 1;
			var yMajorGridCount = (int)((YMax - YMin) / YMajorGrid) - 1;
			var xMinorGridCount = (int)((XMax - XMin) / XMinorGrid) - 1;
			var yMinorGridCount = (int)((YMax - YMin) / YMinorGrid) - 1;
			if (XMinorGrid > 0.0 && (int)XMajorGrid % (int)XMinorGrid == 0)
				xMinorGridCount -= xMajorGridCount;
			if (YMinorGrid > 0.0 && (int)YMajorGrid % (int)YMinorGrid == 0)
				yMinorGridCount -= yMajorGridCount;

			// Calculate minimum required space
			var minWidth  = xMajorGridCount * MajorGridThickness + xMinorGridCount * MinorGridThickness + _plotTopLeft.X;
			var minHeight = yMajorGridCount * MajorGridThickness + yMinorGridCount * MinorGridThickness + _plotTopLeft.Y;
			return new Size(minWidth, minHeight);
		}

		/// <inheritdoc />
		protected override Size ArrangeOverride(Size finalSize)
		{
			return finalSize;
		}

		/// <inheritdoc />
		protected override void OnRender(DrawingContext drawingContext)
		{
			// TODO:  Currently only renders properly for a graph with the origin in the bottom-left corner (no negative values)!

			var xAxisPen     = new Pen(XAxisBrush,     XAxisThickness);
			var yAxisPen     = new Pen(YAxisBrush,     YAxisThickness);
			var majorGridPen = new Pen(MajorGridBrush, MajorGridThickness);
			var minorGridPen = new Pen(MinorGridBrush, MinorGridThickness);
			var plotPen      = new Pen(PlotBrush,      PlotThickness);

			// Calculate values necessary for drawing
			var plotRect = new Rect(
				_plotTopLeft,
				new Size(ActualWidth - _plotTopLeft.X, ActualHeight - _plotTopLeft.Y - _plotBottomSpace));
			var xMajorGridCount  = (XMax - XMin) / XMajorGrid;
			var xMinorGridCount  = (XMax - XMin) / XMinorGrid;
			var yMajorGridCount  = (YMax - YMin) / YMajorGrid;
			var yMinorGridCount  = (YMax - YMin) / YMinorGrid;
			var xMajorGridScalar = plotRect.Width / xMajorGridCount;
			var xMinorGridScalar = plotRect.Width / xMinorGridCount;
			var yMajorGridScalar = plotRect.Height / yMajorGridCount;
			var yMinorGridScalar = plotRect.Height / yMinorGridCount;

			// Draw background
			drawingContext.DrawRectangle(Background, null, new Rect(new Point(0.0, 0.0), new Size(ActualWidth, ActualHeight)));

			// Draw title
			drawingContext.DrawText(_titleText, new Point(plotRect.Left + (plotRect.Width / 2) - (_titleText.Width / 2), 0.0));

			// Draw X-Axis label
			drawingContext.DrawText(
				_xAxisLabelText,
				new Point(
					plotRect.Left + (plotRect.Width / 2) - (_xAxisLabelText.Width / 2),
					ActualHeight - _xAxisLabelText.Height));

			// Draw Y-Axis label
			var location  = new Point(0.0, plotRect.Top + ((plotRect.Bottom - plotRect.Top) / 2) + (_yAxisLabelText.Width / 2));
			var transform = new RotateTransform { Angle = -90 };
			drawingContext.PushTransform(transform);
			drawingContext.DrawText(_yAxisLabelText, new Point(-location.Y, location.X));
			drawingContext.Pop();

			// Draw axes
			drawingContext.DrawLine(xAxisPen, plotRect.BottomLeft, new Point(ActualWidth, plotRect.Bottom));
			drawingContext.DrawLine(yAxisPen, plotRect.TopLeft,    plotRect.BottomLeft);

			// Draw X minor gridlines
			for (var i = 1; i < xMinorGridCount; i++)
			{
				drawingContext.DrawLine(
					minorGridPen,
					new Point(plotRect.Left + i * xMinorGridScalar, plotRect.Top),
					new Point(plotRect.Left + i * xMinorGridScalar, plotRect.Bottom));
			}

			// Draw Y minor gridlines
			for (var i = 1; i < yMinorGridCount; i++)
			{
				drawingContext.DrawLine(
					minorGridPen,
					new Point(plotRect.Left, plotRect.Bottom - i * yMinorGridScalar),
					new Point(ActualWidth,   plotRect.Bottom - i * yMinorGridScalar));
			}

			// Draw X-Axis major gridlines
			for (var i = 1; i < xMajorGridCount; i++)
			{
				drawingContext.DrawLine(
					majorGridPen,
					new Point(plotRect.Left + i * xMajorGridScalar, plotRect.Top),
					new Point(plotRect.Left + i * xMajorGridScalar, plotRect.Bottom));
			}

			// Draw X-Axis value labels
			for (var i = 0; i < xMajorGridCount; i++)
			{
				var x = plotRect.Left + i * xMajorGridScalar;
				drawingContext.DrawText(
					_xAxisValueTextList[i],
					new Point(x - (_xAxisValueTextList[i].Width / 2), plotRect.Bottom + LabelSpacing));
			}

			// Draw Y-Axis major gridlines
			for (var i = 1; i < yMajorGridCount; i++)
			{
				drawingContext.DrawLine(
					majorGridPen,
					new Point(plotRect.Left, plotRect.Bottom - i * yMajorGridScalar),
					new Point(ActualWidth,   plotRect.Bottom - i * yMajorGridScalar));
			}

			// Draw Y-Axis value labels
			for (var i = 0; i < yMajorGridCount; i++)
			{
				var y = plotRect.Bottom - i * yMajorGridScalar;
				drawingContext.DrawText(
					_yAxisValueTextList[i],
					new Point(plotRect.Left - LabelSpacing - _yAxisValueTextList[i].Width, y - (_yAxisValueTextList[i].Height / 2)));
			}

			// Plot data
			for (var i = 0; i < Data.Count; i++)
			{
				if (i > 0)
					drawingContext.DrawLine(
						plotPen,
						new Point(
							plotRect.Left + ((plotRect.Width * Data[i - 1].X) / XMax),
							plotRect.Bottom - ((plotRect.Height * Data[i - 1].Y) / YMax)),
						new Point(
							plotRect.Left + ((plotRect.Width * Data[i].X) / XMax),
							plotRect.Bottom - ((plotRect.Height * Data[i].Y) / YMax)));
			}
		}
		#endregion
	}
}