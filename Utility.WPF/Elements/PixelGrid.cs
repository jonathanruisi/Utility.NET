// ┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  FILE:       PixelGrid.cs
// ┃  PROJECT:    Utility.WPF
// ┃  SOLUTION:   Utility.NET
// ┃  CREATED:    2015-12-25 @ 1:10 AM
// ┣━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  AUTHOR:     Jonathan Ruisi
// ┃  EMAIL:      JonathanRuisi@gmail.com
// ┣━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  GIT REPO:   https://github.com/jonathanruisi/Utility.NET
// ┃  LICENSE:    https://github.com/jonathanruisi/Utility.NET/blob/master/LICENSE
// ┗━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

using JLR.Utility.NETFramework.Collections;
using JLR.Utility.NETFramework.Color;

namespace JLR.Utility.WPF.Elements
{
	public class PixelGrid : FrameworkElement
	{
		#region Fields
		private   PixelMap _pixelMap;
		protected double   PixelWidth,        PixelHeight;
		private   bool     _isLeftButtonDown, _isRightButtonDown;
		#endregion

		#region Properties
		public PixelMap GridData { get { return _pixelMap; } set { SetGridData(value); } }

		public bool IsEditable
		{
			get { return (bool)GetValue(IsEditableProperty); }
			set { SetValue(IsEditableProperty, value); }
		}

		public static readonly DependencyProperty IsEditableProperty = DependencyProperty.Register(
			"IsEditable",
			typeof(bool),
			typeof(PixelGrid),
			new FrameworkPropertyMetadata(true, OnDependencyPropertyChanged));

		public int PixelGridWidth
		{
			get { return (int)GetValue(PixelGridWidthProperty); }
			set { SetValue(PixelGridWidthProperty, value); }
		}

		public static readonly DependencyProperty PixelGridWidthProperty = DependencyProperty.Register(
			"PixelGridWidth",
			typeof(int),
			typeof(PixelGrid),
			new FrameworkPropertyMetadata(
				6,
				FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender,
				OnDependencyPropertyChanged),
			ValidateDimension);

		public int PixelGridHeight
		{
			get { return (int)GetValue(PixelGridHeightProperty); }
			set { SetValue(PixelGridHeightProperty, value); }
		}

		public static readonly DependencyProperty PixelGridHeightProperty = DependencyProperty.Register(
			"PixelGridHeight",
			typeof(int),
			typeof(PixelGrid),
			new FrameworkPropertyMetadata(
				6,
				FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender,
				OnDependencyPropertyChanged),
			ValidateDimension);

		public Size MinimumPixelSize
		{
			get { return (Size)GetValue(MinimumPixelSizeProperty); }
			set { SetValue(MinimumPixelSizeProperty, value); }
		}

		public static readonly DependencyProperty MinimumPixelSizeProperty = DependencyProperty.Register(
			"MinimumPixelSize",
			typeof(Size),
			typeof(PixelGrid),
			new FrameworkPropertyMetadata(new Size(2, 2), FrameworkPropertyMetadataOptions.AffectsMeasure));

		public double PixelBorderThickness
		{
			get { return (double)GetValue(PixelBorderThicknessProperty); }
			set { SetValue(PixelBorderThicknessProperty, value); }
		}

		public static readonly DependencyProperty PixelBorderThicknessProperty = DependencyProperty.Register(
			"PixelBorderThickness",
			typeof(double),
			typeof(PixelGrid),
			new FrameworkPropertyMetadata(
				1.0,
				FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender,
				OnDependencyPropertyChanged));

		public Brush PixelBorderBrush
		{
			get { return (Brush)GetValue(PixelBorderBrushProperty); }
			set { SetValue(PixelBorderBrushProperty, value); }
		}

		public static readonly DependencyProperty PixelBorderBrushProperty = DependencyProperty.Register(
			"PixelBorderBrush",
			typeof(Brush),
			typeof(PixelGrid),
			new FrameworkPropertyMetadata(
				Brushes.Black,
				FrameworkPropertyMetadataOptions.AffectsRender,
				OnDependencyPropertyChanged));

		public Brush SetPixelBrush
		{
			get { return (Brush)GetValue(SetPixelBrushProperty); }
			set { SetValue(SetPixelBrushProperty, value); }
		}

		public static readonly DependencyProperty SetPixelBrushProperty = DependencyProperty.Register(
			"SetPixelBrush",
			typeof(Brush),
			typeof(PixelGrid),
			new FrameworkPropertyMetadata(
				Brushes.Black,
				FrameworkPropertyMetadataOptions.AffectsRender,
				OnDependencyPropertyChanged));

		public Brush ClearPixelBrush
		{
			get { return (Brush)GetValue(ClearPixelBrushProperty); }
			set { SetValue(ClearPixelBrushProperty, value); }
		}

		public static readonly DependencyProperty ClearPixelBrushProperty = DependencyProperty.Register(
			"ClearPixelBrush",
			typeof(Brush),
			typeof(PixelGrid),
			new FrameworkPropertyMetadata(
				Brushes.White,
				FrameworkPropertyMetadataOptions.AffectsRender,
				OnDependencyPropertyChanged));

		public Brush MouseOverlayBrush
		{
			get { return (Brush)GetValue(MouseOverlayBrushProperty); }
			set { SetValue(MouseOverlayBrushProperty, value); }
		}

		public static readonly DependencyProperty MouseOverlayBrushProperty = DependencyProperty.Register(
			"MouseOverlayBrush",
			typeof(Brush),
			typeof(PixelGrid),
			new FrameworkPropertyMetadata(
				new SolidColorBrush(new Rgba(32, 64, 128, 128).ToSystemWindowsMediaColor()),
				FrameworkPropertyMetadataOptions.AffectsRender,
				OnDependencyPropertyChanged));
		#endregion

		#region Protected Properties
		protected Point MouseOverPixelCoordinates
		{
			get { return (Point)GetValue(MouseOverPixelCoordinatesProperty); }
			set { SetValue(MouseOverPixelCoordinatesProperty, value); }
		}

		protected static readonly DependencyProperty MouseOverPixelCoordinatesProperty = DependencyProperty.Register(
			"MouseOverPixelCoordinates",
			typeof(Point),
			typeof(PixelGrid),
			new FrameworkPropertyMetadata(
				new Point(-1, -1),
				FrameworkPropertyMetadataOptions.AffectsRender,
				OnDependencyPropertyChanged));
		#endregion

		#region Events
		public event RoutedEventHandler GridUpdated
		{
			add { AddHandler(GridUpdatedEvent, value); }
			remove { RemoveHandler(GridUpdatedEvent, value); }
		}

		public static readonly RoutedEvent GridUpdatedEvent = EventManager.RegisterRoutedEvent(
			"GridUpdated",
			RoutingStrategy.Bubble,
			typeof(RoutedEventHandler),
			typeof(PixelGrid));
		#endregion

		#region Constructors
		static PixelGrid()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(PixelGrid), new FrameworkPropertyMetadata(typeof(PixelGrid)));
		}

		public PixelGrid()
		{
			MouseEnter         += PixelGrid_MouseEnter;
			MouseLeave         += PixelGrid_MouseLeave;
			MouseMove          += PixelGrid_MouseMove;
			MouseDown          += PixelGrid_MouseDown;
			MouseUp            += PixelGrid_MouseUp;
			_pixelMap          =  new PixelMap(PixelGridWidth, PixelGridHeight);
			_isLeftButtonDown  =  false;
			_isRightButtonDown =  false;
		}
		#endregion

		#region Public Methods
		public void ChangePixel(int row, int column, bool state)
		{
			_pixelMap[row, column] = state;
			InvalidateVisual();
			if (!_isLeftButtonDown && !_isRightButtonDown)
				RaiseEvent(new RoutedEventArgs(GridUpdatedEvent, this));
		}

		public void ChangeAllPixels(bool state)
		{
			for (var i = 0; i < PixelGridHeight; i++)
				_pixelMap.SetRow(new BitArray64(PixelGridWidth, state), i);
			InvalidateVisual();
			RaiseEvent(new RoutedEventArgs(GridUpdatedEvent, this));
		}

		public void SetGridData(PixelMap gridData, bool resizeToSourceGrid = true)
		{
			if (gridData.Width != PixelGridWidth || gridData.Height != PixelGridHeight)
			{
				if (resizeToSourceGrid)
				{
					PixelGridWidth  = gridData.Width;
					PixelGridHeight = gridData.Height;
				}
				else
				{
					gridData.Resize(PixelGridWidth, PixelGridHeight);
				}
			}

			_pixelMap = gridData.Clone() as PixelMap;
			InvalidateVisual();
			RaiseEvent(new RoutedEventArgs(GridUpdatedEvent, this));
		}

		public void Transform(int top, int bottom, int left, int right)
		{
			_pixelMap.Transform(top, bottom, left, right);
			InvalidateVisual();
			RaiseEvent(new RoutedEventArgs(GridUpdatedEvent, this));
		}
		#endregion

		#region Property Changed Callbacks, Coerce Value Callbacks, Validate Value Callbacks
		private static void OnDependencyPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var pixelGrid = d as PixelGrid;
			if (pixelGrid == null) return;

			if (e.Property == PixelGridWidthProperty)
			{
				pixelGrid._pixelMap.Resize((int)e.NewValue, pixelGrid.PixelGridHeight);
			}
			else if (e.Property == PixelGridHeightProperty)
			{
				pixelGrid._pixelMap.Resize(pixelGrid.PixelGridWidth, (int)e.NewValue);
			}
			else if (e.Property == IsEditableProperty)
			{
				if ((bool)e.NewValue)
				{
					pixelGrid.MouseEnter += pixelGrid.PixelGrid_MouseEnter;
					pixelGrid.MouseLeave += pixelGrid.PixelGrid_MouseLeave;
					pixelGrid.MouseMove  += pixelGrid.PixelGrid_MouseMove;
					pixelGrid.MouseDown  += pixelGrid.PixelGrid_MouseDown;
					pixelGrid.MouseUp    += pixelGrid.PixelGrid_MouseUp;
				}
				else
				{
					pixelGrid.MouseEnter -= pixelGrid.PixelGrid_MouseEnter;
					pixelGrid.MouseLeave -= pixelGrid.PixelGrid_MouseLeave;
					pixelGrid.MouseMove  -= pixelGrid.PixelGrid_MouseMove;
					pixelGrid.MouseDown  -= pixelGrid.PixelGrid_MouseDown;
					pixelGrid.MouseUp    -= pixelGrid.PixelGrid_MouseUp;
				}
			}
			else if (e.Property == MouseOverPixelCoordinatesProperty)
			{
				if ((int)((Point)e.NewValue).X >= 0 && (int)((Point)e.NewValue).Y >= 0)
				{
					if (pixelGrid._isLeftButtonDown)
						pixelGrid._pixelMap[(int)((Point)e.NewValue).Y, (int)((Point)e.NewValue).X] = true;
					else if (pixelGrid._isRightButtonDown)
						pixelGrid._pixelMap[(int)((Point)e.NewValue).Y, (int)((Point)e.NewValue).X] = false;
				}
			}
		}

		private static bool ValidateDimension(object value)
		{
			var dimension = (int)value;
			return dimension > 0 && (dimension % 1).Equals(0);
		}
		#endregion

		#region Layout Methods
		protected override Size MeasureOverride(Size availableSize)
		{
			var minimumWidth  = PixelGridWidth * MinimumPixelSize.Width + PixelBorderThickness * (PixelGridWidth - 1);
			var minimumHeight = PixelGridHeight * MinimumPixelSize.Height + PixelBorderThickness * (PixelGridHeight - 1);
			return new Size(minimumWidth, minimumHeight);
		}

		protected override Size ArrangeOverride(Size finalSize)
		{
			return finalSize;
		}
		#endregion

		#region Render Methods
		protected override void OnRender(DrawingContext drawingContext)
		{
			double currentX, currentY;
			var    pixelBorderPen = new Pen(PixelBorderBrush, PixelBorderThickness);

			// Calculate actual pixel size
			PixelWidth  = (ActualWidth - PixelBorderThickness * (PixelGridWidth - 1)) / PixelGridWidth;
			PixelHeight = (ActualHeight - PixelBorderThickness * (PixelGridHeight - 1)) / PixelGridHeight;

			// Draw horizontal pixel borders
			for (var i = 1; i < PixelGridHeight; i++)
			{
				currentY = i * PixelHeight + i * PixelBorderThickness - PixelBorderThickness / 2.0;
				drawingContext.DrawLine(pixelBorderPen, new Point(0, currentY), new Point(ActualWidth, currentY));
			}

			// Draw vertical pixel borders
			for (var i = 1; i < PixelGridWidth; i++)
			{
				currentX = i * PixelWidth + i * PixelBorderThickness - PixelBorderThickness / 2.0;
				drawingContext.DrawLine(pixelBorderPen, new Point(currentX, 0), new Point(currentX, ActualHeight));
			}

			// Draw pixel rectangles
			for (var y = 0; y < PixelGridHeight; y++)
			{
				for (var x = 0; x < PixelGridWidth; x++)
				{
					currentX = x * PixelWidth + x * PixelBorderThickness;
					currentY = y * PixelHeight + y * PixelBorderThickness;
					drawingContext.DrawRectangle(
						_pixelMap[y, x] ? SetPixelBrush : ClearPixelBrush,
						null,
						new Rect(currentX, currentY, PixelWidth, PixelHeight));
					if (!_isLeftButtonDown && !_isRightButtonDown && MouseOverPixelCoordinates.X.Equals(x) &&
						MouseOverPixelCoordinates.Y.Equals(y))
					{
						drawingContext.DrawRectangle(MouseOverlayBrush, null, new Rect(currentX, currentY, PixelWidth, PixelHeight));
					}
				}
			}
		}
		#endregion

		#region Event Handlers
		private void PixelGrid_MouseEnter(object sender, MouseEventArgs e)
		{
			if (e.LeftButton == MouseButtonState.Pressed)
				_isLeftButtonDown = true;
			if (e.RightButton == MouseButtonState.Pressed)
				_isRightButtonDown = true;
			e.Handled = true;
		}

		private void PixelGrid_MouseLeave(object sender, MouseEventArgs e)
		{
			if (_isLeftButtonDown || _isRightButtonDown)
				RaiseEvent(new RoutedEventArgs(GridUpdatedEvent, this));
			_isLeftButtonDown         = false;
			_isRightButtonDown        = false;
			MouseOverPixelCoordinates = new Point(-1, -1);
			e.Handled                 = true;
		}

		private void PixelGrid_MouseDown(object sender, MouseButtonEventArgs e)
		{
			if (e.ChangedButton == MouseButton.Left && (int)MouseOverPixelCoordinates.X >= 0 &&
				(int)MouseOverPixelCoordinates.Y >= 0)
			{
				_isLeftButtonDown = true;
				if (e.ClickCount == 1)
					ChangePixel((int)MouseOverPixelCoordinates.Y, (int)MouseOverPixelCoordinates.X, true);
				else if (e.ClickCount == 2)
					ChangeAllPixels(true);
			}

			if (e.ChangedButton == MouseButton.Right && (int)MouseOverPixelCoordinates.X >= 0 &&
				(int)MouseOverPixelCoordinates.Y >= 0)
			{
				_isRightButtonDown = true;
				if (e.ClickCount == 1)
					ChangePixel((int)MouseOverPixelCoordinates.Y, (int)MouseOverPixelCoordinates.X, false);
				else if (e.ClickCount == 2)
					ChangeAllPixels(false);
			}

			e.Handled = true;
		}

		private void PixelGrid_MouseUp(object sender, MouseButtonEventArgs e)
		{
			if (e.ChangedButton == MouseButton.Left)
			{
				_isLeftButtonDown = false;
				RaiseEvent(new RoutedEventArgs(GridUpdatedEvent, this));
				InvalidateVisual();
			}

			if (e.ChangedButton == MouseButton.Right)
			{
				_isRightButtonDown = false;
				RaiseEvent(new RoutedEventArgs(GridUpdatedEvent, this));
				InvalidateVisual();
			}

			e.Handled = true;
		}

		private void PixelGrid_MouseMove(object sender, MouseEventArgs e)
		{
			var mousePosition = e.GetPosition(this);
			var coordinates   = new Point(-1, -1);
			for (var x = 0; x < PixelGridWidth; x++)
			{
				var lowerBound = x * PixelWidth + x * PixelBorderThickness;
				var upperBound = (x + 1) * PixelWidth + x * PixelBorderThickness;
				if (mousePosition.X >= lowerBound && mousePosition.X <= upperBound)
					coordinates.X = x;
			}

			for (var y = 0; y < PixelGridHeight; y++)
			{
				var lowerBound = y * PixelHeight + y * PixelBorderThickness;
				var upperBound = (y + 1) * PixelHeight + y * PixelBorderThickness;
				if (mousePosition.Y >= lowerBound && mousePosition.Y <= upperBound)
					coordinates.Y = y;
			}

			MouseOverPixelCoordinates = coordinates;
			e.Handled                 = true;
		}
		#endregion
	}
}