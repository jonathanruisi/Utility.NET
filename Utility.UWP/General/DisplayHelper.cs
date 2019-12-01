using System.Drawing;

using Windows.Graphics.Display;

namespace JLR.Utility.UWP
{
	public static class DisplayHelper
	{
		public static Windows.Foundation.Size GetCurrentDisplaySize()
		{
			var dispInfo = DisplayInformation.GetForCurrentView();
			var size = new Windows.Foundation.Size(
				dispInfo.ScreenWidthInRawPixels * dispInfo.RawPixelsPerViewPixel,
				dispInfo.ScreenHeightInRawPixels * dispInfo.RawPixelsPerViewPixel);
			return size;
		}
	}
}