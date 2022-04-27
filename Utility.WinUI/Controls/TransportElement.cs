using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

namespace JLR.Utility.WinUI.Controls
{
    [TemplateVisualState(Name = "Normal",              GroupName = "MouseStates")]
    [TemplateVisualState(Name = "MouseOver",           GroupName = "MouseStates")]
    [TemplateVisualState(Name = "LeftButtonPressed",   GroupName = "MouseStates")]
    [TemplateVisualState(Name = "MiddleButtonPressed", GroupName = "MouseStates")]
    [TemplateVisualState(Name = "RightButtonPressed",  GroupName = "MouseStates")]
    public sealed class TransportElement : Control
    {
        #region Fields
        private InputCursor _primaryCursor, _dragCursor;
        #endregion

        #region Properties
        public bool IsMouseCaptured
        {
            get => (bool)GetValue(IsMouseCapturedProperty);
            private set => SetValue(IsMouseCapturedProperty, value);
        }

        public static readonly DependencyProperty IsMouseCapturedProperty =
            DependencyProperty.Register("IsMouseCaptured",
                                        typeof(bool),
                                        typeof(TransportElement),
                                        new PropertyMetadata(false));

        public bool IsMouseOver
        {
            get => (bool)GetValue(IsMouseOverProperty);
            private set => SetValue(IsMouseOverProperty, value);
        }

        public static readonly DependencyProperty IsMouseOverProperty =
            DependencyProperty.Register("IsMouseOver",
                                        typeof(bool),
                                        typeof(TransportElement),
                                        new PropertyMetadata(false));

        public InputSystemCursorShape MouseOverCursorShape
        {
            get => (InputSystemCursorShape)GetValue(MouseOverCursorShapeProperty);
            set => SetValue(MouseOverCursorShapeProperty, value);
        }

        public static readonly DependencyProperty MouseOverCursorShapeProperty =
            DependencyProperty.Register("MouseOverCursorShape",
                                        typeof(InputSystemCursorShape),
                                        typeof(TransportElement),
                                        new PropertyMetadata(InputSystemCursorShape.Arrow, OnCursorShapeChanged));

        public InputSystemCursorShape MouseDragCursorShape
        {
            get => (InputSystemCursorShape)GetValue(MouseDragCursorShapeProperty);
            set => SetValue(MouseDragCursorShapeProperty, value);
        }

        public static readonly DependencyProperty MouseDragCursorShapeProperty =
            DependencyProperty.Register("MouseDragCursorShape",
                                        typeof(InputSystemCursorShape),
                                        typeof(TransportElement),
                                        new PropertyMetadata(InputSystemCursorShape.Arrow, OnCursorShapeChanged));

        private static void OnCursorShapeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is TransportElement element))
                return;

            if (e.Property == MouseDragCursorShapeProperty)
            {
                element._dragCursor = InputSystemCursor.Create((InputSystemCursorShape)e.NewValue);
                if (element.IsMouseCaptured)
                    element.ProtectedCursor = element._dragCursor;
            }
            else if (e.Property == MouseOverCursorShapeProperty)
            {
                element._primaryCursor = InputSystemCursor.Create((InputSystemCursorShape)e.NewValue);
                if (element.IsMouseOver)
                    element.ProtectedCursor = element._primaryCursor;
            }
        }
        #endregion

        #region Constructor
        public TransportElement()
        {
            DefaultStyleKey = typeof(TransportElement);
            Loaded += TransportElement_Loaded;
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
            if (IsMouseCaptured)
                return;

            ProtectedCursor = _primaryCursor;
            VisualStateManager.GoToState(this, "MouseOver", false);
        }

        protected override void OnPointerExited(PointerRoutedEventArgs e)
        {
            base.OnPointerExited(e);

            IsMouseOver = false;

            if (IsMouseCaptured)
                return;

            VisualStateManager.GoToState(this, "Normal", false);
        }

        protected override void OnPointerPressed(PointerRoutedEventArgs e)
        {
            base.OnPointerPressed(e);

            var point = e.GetCurrentPoint(this);
            if (point.PointerDeviceType != PointerDeviceType.Mouse)
            {
                e.Handled = true;
                return;
            }

            if (point.Properties.IsLeftButtonPressed)
            {
                IsMouseCaptured = CapturePointer(e.Pointer);
                ProtectedCursor = _dragCursor;
                VisualStateManager.GoToState(this, "LeftButtonPressed", false);
            }
            else if (point.Properties.IsRightButtonPressed && !IsMouseCaptured)
            {
                VisualStateManager.GoToState(this, "RightButtonPressed", false);
            }
            else if (point.Properties.IsMiddleButtonPressed && !IsMouseCaptured)
            {
                VisualStateManager.GoToState(this, "MiddleButtonPressed", false);
            }
        }

        protected override void OnPointerReleased(PointerRoutedEventArgs e)
        {
            base.OnPointerReleased(e);

            var point = e.GetCurrentPoint(this);
            if (point.PointerDeviceType != PointerDeviceType.Mouse)
            {
                e.Handled = true;
                return;
            }

            ReleasePointerCapture(e.Pointer);
            ProtectedCursor = _primaryCursor;
            VisualStateManager.GoToState(this, IsMouseOver ? "MouseOver" : "Normal", false);
        }

        protected override void OnPointerCaptureLost(PointerRoutedEventArgs e)
        {
            base.OnPointerCaptureLost(e);

            IsMouseCaptured = false;
            VisualStateManager.GoToState(this, IsMouseOver ? "MouseOver" : "Normal", false);
        }

        protected override void OnPointerCanceled(PointerRoutedEventArgs e)
        {
            base.OnPointerCanceled(e);

            IsMouseCaptured = false;
            IsMouseOver = false;

            VisualStateManager.GoToState(this, "Normal", false);
        }
        #endregion
    }
}