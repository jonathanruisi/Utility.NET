using Windows.Devices.Input;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace JLR.Utility.UWP.Controls
{
	[TemplateVisualState(Name = "Normal", GroupName              = "MouseStates")]
	[TemplateVisualState(Name = "MouseOver", GroupName           = "MouseStates")]
	[TemplateVisualState(Name = "LeftButtonPressed", GroupName   = "MouseStates")]
	[TemplateVisualState(Name = "MiddleButtonPressed", GroupName = "MouseStates")]
	[TemplateVisualState(Name = "RightButtonPressed", GroupName  = "MouseStates")]
	public sealed class TransportElement : Control
	{
		#region Fields
		private CoreCursor _previousCursor, _primaryCursor, _dragCursor;
		private bool       _isMouseCaptured;
		#endregion

		#region Properties
		public bool IsMouseOver
		{
			get => (bool) GetValue(IsMouseOverProperty);
			private set => SetValue(IsMouseOverProperty, value);
		}

		public static readonly DependencyProperty IsMouseOverProperty =
			DependencyProperty.Register("IsMouseOver",
			                            typeof(bool),
			                            typeof(TransportElement),
			                            new PropertyMetadata(false));

		public CoreCursorType MouseOverCursorType
		{
			get => (CoreCursorType) GetValue(MouseOverCursorTypeProperty);
			set => SetValue(MouseOverCursorTypeProperty, value);
		}

		public static readonly DependencyProperty MouseOverCursorTypeProperty =
			DependencyProperty.Register("MouseOverCursorType",
			                            typeof(CoreCursorType),
			                            typeof(TransportElement),
			                            new PropertyMetadata(CoreCursorType.Arrow, OnCursorTypeChanged));

		public CoreCursorType MouseDragCursorType
		{
			get => (CoreCursorType) GetValue(MouseDragCursorTypeProperty);
			set => SetValue(MouseDragCursorTypeProperty, value);
		}

		public static readonly DependencyProperty MouseDragCursorTypeProperty =
			DependencyProperty.Register("MouseDragCursorType",
			                            typeof(CoreCursorType),
			                            typeof(TransportElement),
			                            new PropertyMetadata(CoreCursorType.Arrow, OnCursorTypeChanged));

		private static void OnCursorTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (!(d is TransportElement element))
				return;

			if (e.Property == MouseDragCursorTypeProperty)
			{
				element._dragCursor = new CoreCursor((CoreCursorType) e.NewValue, 1);
				if (element._isMouseCaptured)
					Window.Current.CoreWindow.PointerCursor = element._dragCursor;
			}
			else if (e.Property == MouseOverCursorTypeProperty)
			{
				element._primaryCursor = new CoreCursor((CoreCursorType) e.NewValue, 0);
				if (element.IsMouseOver)
					Window.Current.CoreWindow.PointerCursor = element._primaryCursor;
			}
		}
		#endregion

		#region Constructor
		public TransportElement()
		{
			DefaultStyleKey =  typeof(TransportElement);
			Loaded          += TransportElement_Loaded;
			_primaryCursor  =  new CoreCursor(MouseOverCursorType, 0);
			_dragCursor     =  new CoreCursor(MouseDragCursorType, 1);
		}
		#endregion

		#region Event Handlers
		private void TransportElement_Loaded(object sender, RoutedEventArgs e)
		{
			VisualStateManager.GoToState(this, "Normal", false);
		}
		#endregion

		#region Method Overrides (Control)
		protected override void OnPointerEntered(PointerRoutedEventArgs e)
		{
			base.OnPointerEntered(e);

			IsMouseOver = true;
			if (_isMouseCaptured)
				return;

			_previousCursor                         = Window.Current.CoreWindow.PointerCursor;
			Window.Current.CoreWindow.PointerCursor = _primaryCursor;
			VisualStateManager.GoToState(this, "MouseOver", false);
		}

		protected override void OnPointerExited(PointerRoutedEventArgs e)
		{
			base.OnPointerExited(e);

			IsMouseOver = false;

			if (_isMouseCaptured)
				return;

			Window.Current.CoreWindow.PointerCursor = _previousCursor;
			VisualStateManager.GoToState(this, "Normal", false);
		}

		protected override void OnPointerPressed(PointerRoutedEventArgs e)
		{
			base.OnPointerPressed(e);

			var point = e.GetCurrentPoint(this);
			if (point.PointerDevice.PointerDeviceType != PointerDeviceType.Mouse)
			{
				e.Handled = true;
				return;
			}

			if (point.Properties.IsLeftButtonPressed)
			{
				_isMouseCaptured = CapturePointer(e.Pointer);
				Window.Current.CoreWindow.PointerCursor = _dragCursor;
				VisualStateManager.GoToState(this, "LeftButtonPressed", false);
			}
			else if (point.Properties.IsRightButtonPressed && !_isMouseCaptured)
			{
				VisualStateManager.GoToState(this, "RightButtonPressed", false);
			}
			else if (point.Properties.IsMiddleButtonPressed && !_isMouseCaptured)
			{
				VisualStateManager.GoToState(this, "MiddleButtonPressed", false);
			}
		}

		protected override void OnPointerReleased(PointerRoutedEventArgs e)
		{
			base.OnPointerReleased(e);

			var point = e.GetCurrentPoint(this);
			if (point.PointerDevice.PointerDeviceType != PointerDeviceType.Mouse)
			{
				e.Handled = true;
				return;
			}

			ReleasePointerCapture(e.Pointer);

			Window.Current.CoreWindow.PointerCursor = IsMouseOver ? _primaryCursor : _previousCursor;

			VisualStateManager.GoToState(this, IsMouseOver ? "MouseOver" : "Normal", false);
		}

		protected override void OnPointerCaptureLost(PointerRoutedEventArgs e)
		{
			base.OnPointerCaptureLost(e);

			_isMouseCaptured = false;
		}

		protected override void OnPointerCanceled(PointerRoutedEventArgs e)
		{
			base.OnPointerCanceled(e);

			_isMouseCaptured = false;
			IsMouseOver      = false;

			Window.Current.CoreWindow.PointerCursor = _previousCursor;
			VisualStateManager.GoToState(this, "Normal", false);
		}
		#endregion
	}
}