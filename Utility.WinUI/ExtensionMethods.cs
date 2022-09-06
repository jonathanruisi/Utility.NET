using System;
using System.Linq;
using System.Numerics;

using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Brushes;
using Microsoft.Graphics.Canvas.Geometry;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;

using Windows.Foundation;
using Windows.Storage;

using WinRT.Interop;

namespace JLR.Utility.WinUI
{
    public static class ExtensionMethods
    {
        #region Microsoft.UI.Xaml.Window
        public static AppWindow GetAppWindowForCurrentWindow(this Window window)
        {
            var hWnd = WindowNative.GetWindowHandle(window);
            var winId = Win32Interop.GetWindowIdFromWindow(hWnd);
            return AppWindow.GetFromWindowId(winId);
        }
        #endregion

        #region Microsoft.UI.Xaml.Media.Brush
        public static ICanvasBrush CreateCanvasBrush(this Brush brush,
                                                     ICanvasResourceCreator resourceCreator)
        {
            switch (brush)
            {
                case SolidColorBrush solidColorBrush:
                    return new CanvasSolidColorBrush(resourceCreator, solidColorBrush.Color);

                case LinearGradientBrush linearGradientBrush:
                {
                    var gradientStops = linearGradientBrush.GradientStops.Select(
                                        gradientStop => new CanvasGradientStop
                                        {
                                            Position = (float)gradientStop.Offset,
                                            Color = gradientStop.Color
                                        }).ToArray();

                    return new CanvasLinearGradientBrush(resourceCreator, gradientStops);
                }

                case RadialGradientBrush radialGradientBrush:
                {
                    var gradientStops = radialGradientBrush.GradientStops.Select(
                                        gradientStop => new CanvasGradientStop
                                        {
                                            Position = (float)gradientStop.Offset,
                                            Color = gradientStop.Color
                                        }).ToArray();

                    return new CanvasRadialGradientBrush(resourceCreator, gradientStops);
                }

                default:
                    throw new InvalidOperationException(
                        "This type of brush cannot be converted to ICanvasBrush via this method");
            }
        }
        #endregion

        #region Microsoft.UI.Xaml.Media.Geometry
        public static CanvasGeometry CreateCanvasGeometry(this Geometry geometry,
                                                          ICanvasResourceCreator resourceCreator)
        {
            switch (geometry)
            {
                case EllipseGeometry shape:
                    return CanvasGeometry.CreateEllipse(resourceCreator,
                        (float)shape.Center.X, (float)shape.Center.Y, (float)shape.RadiusX, (float)shape.RadiusY);

                case PathGeometry shape:
                {
                    using var pathBuilder = new CanvasPathBuilder(resourceCreator);
                    foreach (var figure in shape.Figures)
                    {
                        pathBuilder.BeginFigure((float)figure.StartPoint.X, (float)figure.StartPoint.Y);

                        foreach (var segment in figure.Segments)
                        {
                            switch (segment)
                            {
                                case ArcSegment seg:
                                    pathBuilder.AddArc(new Vector2((float)seg.Point.X, (float)seg.Point.Y),
                                                       (float)seg.Size.Width, (float)seg.Size.Height,
                                                       (float)seg.RotationAngle,
                                                       seg.SweepDirection == SweepDirection.Clockwise ? CanvasSweepDirection.Clockwise : CanvasSweepDirection.CounterClockwise,
                                                       seg.IsLargeArc ? CanvasArcSize.Large : CanvasArcSize.Small);
                                    break;

                                case BezierSegment seg:
                                    pathBuilder.AddCubicBezier(new Vector2((float)seg.Point1.X, (float)seg.Point1.Y),
                                                               new Vector2((float)seg.Point2.X, (float)seg.Point2.Y),
                                                               new Vector2((float)seg.Point3.X, (float)seg.Point3.Y));
                                    break;

                                case LineSegment seg:
                                    pathBuilder.AddLine((float)seg.Point.X, (float)seg.Point.Y);
                                    break;

                                case PolyLineSegment seg:
                                    foreach (var p in seg.Points)
                                        pathBuilder.AddLine(new Vector2((float)p.X, (float)p.Y));
                                    break;

                                case QuadraticBezierSegment seg:
                                    pathBuilder.AddQuadraticBezier(new Vector2((float)seg.Point1.X, (float)seg.Point1.Y),
                                                                   new Vector2((float)seg.Point2.X, (float)seg.Point2.Y));
                                    break;

                                default:
                                    throw new InvalidOperationException(
                                        "This type of figure cannot be added to a CanvasGeometry using this method");
                            }
                        }

                        pathBuilder.EndFigure(figure.IsClosed ? CanvasFigureLoop.Closed : CanvasFigureLoop.Open);
                    }

                    return CanvasGeometry.CreatePath(pathBuilder);
                }

                case RectangleGeometry shape:
                    return CanvasGeometry.CreateRectangle(resourceCreator, shape.Rect);

                default:
                    throw new InvalidOperationException(
                        "This type of geometry cannot be converted to CanvasGeometry via this method");
            }
        }
        #endregion

        #region Windows.Foundation.Rect
        public static Point GetCenterPoint(this Rect rect)
        {
            return new Point(rect.X + (rect.Width / 2), rect.Y + (rect.Height / 2));
        }
        #endregion

        #region Windows.Storage.IStorageItem
        public static string GetFileExtension(this IStorageItem file)
        {
            if (file.Name.Contains('.'))
                return file.Name.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Last().ToLower();

            return string.Empty;
        }
        #endregion
    }
}