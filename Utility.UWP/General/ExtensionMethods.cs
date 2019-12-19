using System;
using System.Linq;

using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Brushes;
using Microsoft.Toolkit.Uwp.UI.Media;

namespace JLR.Utility.UWP
{
	public static class ExtensionMethods
	{
		#region Brush
		public static ICanvasBrush CreateCanvasBrush(this Brush brush,
		                                             ICanvasResourceCreator resourceCreator)
		{
			switch (brush)
			{
				case SolidColorBrush solidColorBrush:
					return new CanvasSolidColorBrush(resourceCreator, new Color()
					{
						A = (byte) (solidColorBrush.Opacity * 255),
						R = solidColorBrush.Color.R,
						G = solidColorBrush.Color.G,
						B = solidColorBrush.Color.B
					});

				case LinearGradientBrush linearGradientBrush:
				{
					var gradientStops = linearGradientBrush
					                   .GradientStops.Select(gradientStop => new CanvasGradientStop
					                    {
						                    Position = (float) gradientStop.Offset,
						                    Color    = gradientStop.Color
					                    }).ToArray();
					return new CanvasLinearGradientBrush(resourceCreator, gradientStops);
				}

				case RadialGradientBrush radialGradientBrush:
				{
					var gradientStops = radialGradientBrush
					                   .GradientStops.Select(gradientStop => new CanvasGradientStop
					                    {
						                    Position = (float) gradientStop.Offset,
						                    Color    = gradientStop.Color
					                    }).ToArray();
					return new CanvasRadialGradientBrush(resourceCreator, gradientStops);
				}

				default:
					throw new InvalidOperationException(
						"This type of brush cannot be converted to ICanvasBrush");
			}
		}
		#endregion
	}
}