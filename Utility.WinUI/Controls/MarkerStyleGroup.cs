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
        public float StrokeThickness { get; set; }
        public CanvasStrokeStyle StrokeStyle { get; set; }

        public Brush TailStroke { get; set; }
        public float TailStrokeThickness { get; set; }
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
        public float SpanStrokeThickness { get; set; }
        public CanvasStrokeStyle SpanStrokeStyle { get; set; }

        public Brush SpanStartTailStroke { get; set; }
        public float SpanStartTailStrokeThickness { get; set; }
        public CanvasStrokeStyle SpanStartTailStrokeStyle { get; set; }

        public Brush SpanEndTailStroke { get; set; }
        public float SpanEndTailStrokeThickness { get; set; }
        public CanvasStrokeStyle SpanEndTailStrokeStyle { get; set; }

        internal ICanvasBrush SpanFillBrush { get; set; }
        internal ICanvasBrush SpanLabelBrush { get; set; }
        internal ICanvasBrush SpanStrokeBrush { get; set; }
        internal ICanvasBrush SpanStartTailStrokeBrush { get; set; }
        internal ICanvasBrush SpanEndTailStrokeBrush { get; set; }
    }

    /*public class MarkerStyleGroup : DependencyObject
    {
        public MarkerStyle Style
        {
            get => (MarkerStyle)GetValue(StyleProperty);
            set => SetValue(StyleProperty, value);
        }

        public static readonly DependencyProperty StyleProperty =
            DependencyProperty.Register("Style",
                                        typeof(MarkerStyle),
                                        typeof(MarkerStyleGroup),
                                        new PropertyMetadata(null));

        public MarkerStyle SelectedStyle
        {
            get => (MarkerStyle)GetValue(SelectedStyleProperty);
            set => SetValue(SelectedStyleProperty, value);
        }

        public static readonly DependencyProperty SelectedStyleProperty =
            DependencyProperty.Register("SelectedStyle",
                                        typeof(MarkerStyle),
                                        typeof(MarkerStyleGroup),
                                        new PropertyMetadata(null));

        public TimespanMarkerStyle TimespanMarkerStyle
        {
            get => (TimespanMarkerStyle)GetValue(TimespanMarkerStyleProperty);
            set => SetValue(TimespanMarkerStyleProperty, value);
        }

        public static readonly DependencyProperty TimespanMarkerStyleProperty =
            DependencyProperty.Register("TimespanMarkerStyle",
                                        typeof(TimespanMarkerStyle),
                                        typeof(MarkerStyleGroup),
                                        new PropertyMetadata(null));

        public TimespanMarkerStyle SelectedTimespanMarkerStyle
        {
            get => (TimespanMarkerStyle)GetValue(SelectedTimespanMarkerStyleProperty);
            set => SetValue(SelectedTimespanMarkerStyleProperty, value);
        }

        public static readonly DependencyProperty SelectedTimespanMarkerStyleProperty =
            DependencyProperty.Register("SelectedTimespanMarkerStyle",
                                        typeof(TimespanMarkerStyle),
                                        typeof(MarkerStyleGroup),
                                        new PropertyMetadata(null));

        public bool AreResourcesCreated { get; internal set; }
    }

    public sealed class MarkerStyle : DependencyObject
    {
        public Geometry Geometry
        {
            get => (Geometry)GetValue(GeometryProperty);
            set => SetValue(GeometryProperty, value);
        }

        public static readonly DependencyProperty GeometryProperty =
            DependencyProperty.Register("Geometry",
                                        typeof(Geometry),
                                        typeof(MarkerStyle),
                                        new PropertyMetadata(null));

        public Brush Fill
        {
            get => (Brush)GetValue(FillProperty);
            set => SetValue(FillProperty, value);
        }

        public static readonly DependencyProperty FillProperty =
            DependencyProperty.Register("Fill",
                                        typeof(Brush),
                                        typeof(MarkerStyle),
                                        new PropertyMetadata(null));

        public Brush Stroke
        {
            get => (Brush)GetValue(StrokeProperty);
            set => SetValue(StrokeProperty, value);
        }

        public static readonly DependencyProperty StrokeProperty =
            DependencyProperty.Register("Stroke",
                                        typeof(Brush),
                                        typeof(MarkerStyle),
                                        new PropertyMetadata(null));

        public float StrokeThickness
        {
            get => (float)GetValue(StrokeThicknessProperty);
            set => SetValue(StrokeThicknessProperty, value);
        }

        public static readonly DependencyProperty StrokeThicknessProperty =
            DependencyProperty.Register("StrokeThickness",
                                        typeof(float),
                                        typeof(MarkerStyle),
                                        new PropertyMetadata(null));

        public CanvasStrokeStyle StrokeStyle
        {
            get => (CanvasStrokeStyle)GetValue(StrokeStyleProperty);
            set => SetValue(StrokeStyleProperty, value);
        }

        public static readonly DependencyProperty StrokeStyleProperty =
            DependencyProperty.Register("StrokeStyle",
                                        typeof(CanvasStrokeStyle),
                                        typeof(MarkerStyle),
                                        new PropertyMetadata(null));

        public Brush TailStroke
        {
            get => (Brush)GetValue(TailStrokeProperty);
            set => SetValue(TailStrokeProperty, value);
        }

        public static readonly DependencyProperty TailStrokeProperty =
            DependencyProperty.Register("TailStroke",
                                        typeof(Brush),
                                        typeof(MarkerStyle),
                                        new PropertyMetadata(null));

        public float TailStrokeThickness
        {
            get => (float)GetValue(TailStrokeThicknessProperty);
            set => SetValue(TailStrokeThicknessProperty, value);
        }

        public static readonly DependencyProperty TailStrokeThicknessProperty =
            DependencyProperty.Register("TailStrokeThickness",
                                        typeof(float),
                                        typeof(MarkerStyle),
                                        new PropertyMetadata(null));

        public CanvasStrokeStyle TailStrokeStyle
        {
            get => (CanvasStrokeStyle)GetValue(TailStrokeStyleProperty);
            set => SetValue(TailStrokeStyleProperty, value);
        }

        public static readonly DependencyProperty TailStrokeStyleProperty =
            DependencyProperty.Register("TailStrokeStyle",
                                        typeof(CanvasStrokeStyle),
                                        typeof(MarkerStyle),
                                        new PropertyMetadata(null));

        internal CanvasGeometry CanvasGeometry { get; set; }
        internal ICanvasBrush FillBrush { get; set; }
        internal ICanvasBrush StrokeBrush { get; set; }
        internal ICanvasBrush TailStrokeBrush { get; set; }
        internal CanvasRenderTarget RenderTarget { get; set; }
    }

    public sealed class TimespanMarkerStyle : DependencyObject
    {
        public Brush SpanFill
        {
            get => (Brush)GetValue(SpanFillProperty);
            set => SetValue(SpanFillProperty, value);
        }

        public static readonly DependencyProperty SpanFillProperty =
            DependencyProperty.Register("SpanFill",
                                        typeof(Brush),
                                        typeof(MarkerStyle),
                                        new PropertyMetadata(null));

        public Brush SpanLabel
        {
            get => (Brush)GetValue(SpanLabelProperty);
            set => SetValue(SpanLabelProperty, value);
        }

        public static readonly DependencyProperty SpanLabelProperty =
            DependencyProperty.Register("SpanLabel",
                                        typeof(Brush),
                                        typeof(MarkerStyle),
                                        new PropertyMetadata(null));

        public Brush SpanStroke
        {
            get => (Brush)GetValue(SpanStrokeProperty);
            set => SetValue(SpanStrokeProperty, value);
        }

        public static readonly DependencyProperty SpanStrokeProperty =
            DependencyProperty.Register("SpanStroke",
                                        typeof(Brush),
                                        typeof(MarkerStyle),
                                        new PropertyMetadata(null));

        public float SpanStrokeThickness
        {
            get => (float)GetValue(SpanStrokeThicknessProperty);
            set => SetValue(SpanStrokeThicknessProperty, value);
        }

        public static readonly DependencyProperty SpanStrokeThicknessProperty =
            DependencyProperty.Register("SpanStrokeThickness",
                                        typeof(float),
                                        typeof(MarkerStyle),
                                        new PropertyMetadata(null));

        public CanvasStrokeStyle SpanStrokeStyle
        {
            get => (CanvasStrokeStyle)GetValue(SpanStrokeStyleProperty);
            set => SetValue(SpanStrokeStyleProperty, value);
        }

        public static readonly DependencyProperty SpanStrokeStyleProperty =
            DependencyProperty.Register("SpanStrokeStyle",
                                        typeof(CanvasStrokeStyle),
                                        typeof(MarkerStyle),
                                        new PropertyMetadata(null));

        public Brush SpanStartTailStroke
        {
            get => (Brush)GetValue(SpanStartTailStrokeProperty);
            set => SetValue(SpanStartTailStrokeProperty, value);
        }

        public static readonly DependencyProperty SpanStartTailStrokeProperty =
            DependencyProperty.Register("SpanStartTailStroke",
                                        typeof(Brush),
                                        typeof(MarkerStyle),
                                        new PropertyMetadata(null));

        public float SpanStartTailStrokeThickness
        {
            get => (float)GetValue(SpanStartTailStrokeThicknessProperty);
            set => SetValue(SpanStartTailStrokeThicknessProperty, value);
        }

        public static readonly DependencyProperty SpanStartTailStrokeThicknessProperty =
            DependencyProperty.Register("SpanStartTailStrokeThickness",
                                        typeof(float),
                                        typeof(MarkerStyle),
                                        new PropertyMetadata(null));

        public CanvasStrokeStyle SpanStartTailStrokeStyle
        {
            get => (CanvasStrokeStyle)GetValue(SpanStartTailStrokeStyleProperty);
            set => SetValue(SpanStartTailStrokeStyleProperty, value);
        }

        public static readonly DependencyProperty SpanStartTailStrokeStyleProperty =
            DependencyProperty.Register("SpanStartTailStrokeStyle",
                                        typeof(CanvasStrokeStyle),
                                        typeof(MarkerStyle),
                                        new PropertyMetadata(null));

        public Brush SpanEndTailStroke
        {
            get => (Brush)GetValue(SpanEndTailStrokeProperty);
            set => SetValue(SpanEndTailStrokeProperty, value);
        }

        public static readonly DependencyProperty SpanEndTailStrokeProperty =
            DependencyProperty.Register("SpanEndTailStroke",
                                        typeof(Brush),
                                        typeof(MarkerStyle),
                                        new PropertyMetadata(null));

        public float SpanEndTailStrokeThickness
        {
            get => (float)GetValue(SpanEndTailStrokeThicknessProperty);
            set => SetValue(SpanEndTailStrokeThicknessProperty, value);
        }

        public static readonly DependencyProperty SpanEndTailStrokeThicknessProperty =
            DependencyProperty.Register("SpanEndTailStrokeThickness",
                                        typeof(float),
                                        typeof(MarkerStyle),
                                        new PropertyMetadata(null));

        public CanvasStrokeStyle SpanEndTailStrokeStyle
        {
            get => (CanvasStrokeStyle)GetValue(SpanEndTailStrokeStyleProperty);
            set => SetValue(SpanEndTailStrokeStyleProperty, value);
        }

        public static readonly DependencyProperty SpanEndTailStrokeStyleProperty =
            DependencyProperty.Register("SpanEndTailStrokeStyle",
                                        typeof(CanvasStrokeStyle),
                                        typeof(MarkerStyle),
                                        new PropertyMetadata(null));

        internal ICanvasBrush SpanFillBrush { get; set; }
        internal ICanvasBrush SpanLabelBrush { get; set; }
        internal ICanvasBrush SpanStrokeBrush { get; set; }
        internal ICanvasBrush SpanStartTailStrokeBrush { get; set; }
        internal ICanvasBrush SpanEndTailStrokeBrush { get; set; }
    }*/
}