using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Brushes;
using Microsoft.Graphics.Canvas.Geometry;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;

namespace JLR.Utility.WinUI.Controls
{
    public class MarkerStyleGroup
    {
        public MarkerStyle Style { get; set; }
        public MarkerStyle SelectedStyle { get; set; }
        public TimespanMarkerStyle TimespanMarkerStyle { get; set; }
        public TimespanMarkerStyle SelectedTimespanMarkerStyle { get; set; }
        public bool AreResourcesCreated { get; internal set; }
    }

    public sealed class MarkerStyle
    {
        public Geometry Geometry { get; set; }
        public Brush Fill { get; set; }

        public Brush Stroke { get; set; }
        public double StrokeThickness { get; set; }
        public CanvasStrokeStyle StrokeStyle { get; set; }

        public Brush TailStroke { get; set; }
        public double TailStrokeThickness { get; set; }
        public CanvasStrokeStyle TailStrokeStyle { get; set; }

        internal CanvasGeometry CanvasGeometry { get; set; }
        internal ICanvasBrush FillBrush { get; set; }
        internal ICanvasBrush StrokeBrush { get; set; }
        internal ICanvasBrush TailStrokeBrush { get; set; }
        internal CanvasRenderTarget RenderTarget { get; set; }
    }

    public sealed class TimespanMarkerStyle
    {
        public Brush SpanFill { get; set; }
        public Brush SpanLabel { get; set; }

        public Brush SpanStroke { get; set; }
        public double SpanStrokeThickness { get; set; }
        public CanvasStrokeStyle SpanStrokeStyle { get; set; }

        public Brush SpanStartTailStroke { get; set; }
        public double SpanStartTailStrokeThickness { get; set; }
        public CanvasStrokeStyle SpanStartTailStrokeStyle { get; set; }

        public Brush SpanEndTailStroke { get; set; }
        public double SpanEndTailStrokeThickness { get; set; }
        public CanvasStrokeStyle SpanEndTailStrokeStyle { get; set; }

        internal ICanvasBrush SpanFillBrush { get; set; }
        internal ICanvasBrush SpanLabelBrush { get; set; }
        internal ICanvasBrush SpanStrokeBrush { get; set; }
        internal ICanvasBrush SpanStartTailStrokeBrush { get; set; }
        internal ICanvasBrush SpanEndTailStrokeBrush { get; set; }
    }
}