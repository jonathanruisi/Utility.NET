using System.Collections.Generic;

using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

using Microsoft.Graphics.Canvas.Brushes;
using Microsoft.Graphics.Canvas.UI.Xaml;

namespace JLR.Utility.UWP.Controls
{
	[TemplatePart(Name = "PART_MarkerBar", Type      = typeof(CanvasControl))]
	[TemplatePart(Name = "PART_TickBar", Type        = typeof(CanvasControl))]
	[TemplatePart(Name = "PART_Position", Type       = typeof(TransportElement))]
	[TemplatePart(Name = "PART_SelectionStart", Type = typeof(TransportElement))]
	[TemplatePart(Name = "PART_SelectionEnd", Type   = typeof(TransportElement))]
	[TemplatePart(Name = "PART_ZoomStart", Type      = typeof(TransportElement))]
	[TemplatePart(Name = "PART_ZoomEnd", Type        = typeof(TransportElement))]
	[TemplatePart(Name = "PART_ZoomThumb", Type      = typeof(TransportElement))]
	public sealed partial class MediaSlider : Control
	{
		#region Constants
		private const int     DecimalPrecision    = 8;
		private const decimal MinimumVisibleRange = 1.0M;
		private const int     SecondsPerMinute    = 60;
		private const int     SecondsPerHour      = 3600;
		private const int     SecondsPerDay       = 86400;
		private const int     MinutesPerHour      = 60;
		private const int     HoursPerDay         = 24;
		#endregion

		#region Fields
		private TransportElement
			_positionElement,
			_selectionStartElement,
			_selectionEndElement,
			_zoomStartElement,
			_zoomEndElement,
			_zoomThumbElement;

		private ICanvasBrush
			_selectionHighlightBrush,
			_originTickBrush,
			_majorTickBrush,
			_minorTickBrush,
			_markerBrush,
			_clipMarkerBrush,
			_inPointBrush,
			_outPointBrush,
			_selectedMarkerBrush,
			_selectedClipBrush;

		private bool
			_isLeftMouseDown,
			_isBoundaryUpdateInProgress;

		private CoreCursor _previousCursor;

		private readonly CoreCursor _primaryCursor =
			new CoreCursor(CoreCursorType.UpArrow, 0);

		private CanvasControl _tickCanvas, _markerCanvas;
		private Panel         _mainPanel,  _zoomPanel;
		private Rect          _selectionRect;
		private double        _prevMousePosX;

		private readonly LinkedList<(int major, int minor, int minorPerMajor)>     _intervals;
		private          LinkedListNode<(int major, int minor, int minorPerMajor)> _currentInterval;
		#endregion
	}
}