using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

using Windows.Foundation;

namespace JLR.Utility.WinUI.Graphics;

public readonly struct Polygon
{
    public readonly Vector2[] Vertices;

    public Polygon(params Vector2[] vertices)
    {
        Vertices = new Vector2[vertices.Length];

        int i = 0;
        foreach (var vertex in vertices)
        {
            Vertices[i++] = vertex;
        }
    }

    public Rect BoundingBox
    {
        get
        {
            var xmin = Vertices.Min(v => v.X);
            var xmax = Vertices.Max(v => v.X);
            var ymin = Vertices.Min(v => v.Y);
            var ymax = Vertices.Max(v => v.Y);
            return new Rect(xmin, ymin, xmax - xmin, ymax - ymin);
        }
    }

    public bool Contains(Point p)
    {
        return Contains(new Vector2((float)p.X, (float)p.Y));
    }

    public bool Contains(Vector2 p)
    {
        // Quick bounding box check
        if (!BoundingBox.Contains(new Point(p.X, p.Y)))
        {
            return false;
        }

        bool isInside = false;
        for (int i = 0, j = Vertices.Length - 1; i < Vertices.Length; j = i, i++)
        {
            // If point lies on an edge or a vertex, treat it as inside
            if (IsPointOnSegment(p, Vertices[i], Vertices[j]))
                return true;

            // Ray-casting algorithm
            bool intersects = ((Vertices[i].Y > p.Y) != (Vertices[j].Y > p.Y)) &&
                              (p.X < (Vertices[j].X - Vertices[i].X) * (p.Y - Vertices[i].Y) / (Vertices[j].Y - Vertices[i].Y) + Vertices[i].X);
            if (intersects)
                isInside = !isInside;
        }
        return isInside;

        static bool IsPointOnSegment(in Vector2 p, in Vector2 a, in Vector2 b)
        {
            // Check for collinearity using cross product
            var crossProduct = (b.X - a.X) * (p.Y - a.Y) - (b.Y - a.Y) * (p.X - a.X);
            if (MathF.Abs(crossProduct) > float.Epsilon)
            {
                return false;
            }

            // Check if the point is within the bounding box of the segment
            var ap = p - a;
            var ab = b - a;
            var dotProduct = Vector2.Dot(ap, ab);
            if (dotProduct < -float.Epsilon)
                return false;

            var abLengthSquared = Vector2.Dot(ab, ab);
            if (dotProduct > abLengthSquared + float.Epsilon)
                return false;

            return true;
        }
    }
}