﻿using System;
using System.Linq;

using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Brushes;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;

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
    }
}