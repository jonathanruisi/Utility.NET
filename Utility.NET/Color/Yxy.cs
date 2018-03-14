// ┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  FILE:       Yxy.cs
// ┃  PROJECT:    Utility.NET
// ┃  SOLUTION:   Utility.NET
// ┃  CREATED:    2015-12-25 @ 11:26 PM
// ┣━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  AUTHOR:     Jonathan Ruisi
// ┃  EMAIL:      JonathanRuisi@gmail.com
// ┣━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  GIT REPO:   https://github.com/jonathanruisi/Utility.NET
// ┃  LICENSE:    https://github.com/jonathanruisi/Utility.NET/blob/master/LICENSE
// ┗━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

using System;
using System.Xml;
using System.Xml.Linq;

using JLR.Utility.NET.Math;

namespace JLR.Utility.NET.Color
{
	public sealed class Yxy : ColorSpace
	{
		#region Constants
		public const string FriendlyName  = "Yxy";
		public const int    PropertyCount = 3;
		#endregion

		#region Properties
		public                 double        Y1 { get; set; }
		public                 double        X  { get; set; }
		public                 double        Y2 { get; set; }
		public static readonly Range<double> RangeY1 = new Range<double>(0.0, 100.0);
		public static readonly Range<double> RangeX  = new Range<double>(0.0, 1.0);
		public static readonly Range<double> RangeY2 = new Range<double>(0.0, 1.0);
		#endregion

		#region Indexers
		public override double this[int index]
		{
			get
			{
				switch (index)
				{
					case 0:
						return Y1;
					case 1:
						return X;
					case 2:
						return Y2;
					default:
						return Double.NaN;
				}
			}
			set
			{
				switch (index)
				{
					case 0:
						Y1 = value;
						break;
					case 1:
						X = value;
						break;
					case 2:
						Y2 = value;
						break;
					default:
						throw new IndexOutOfRangeException("No property exists for this ColorSpace at the specified index");
				}
			}
		}
		#endregion

		#region Constructors
		public Yxy(Rgb color)
		{
			FromRgb(color);
		}

		public Yxy() : this(0, 0, 0) { }

		public Yxy(double y1, double x, double y2)
		{
			ValidateRange(y1, RangeY1, "Y1");
			ValidateRange(x,  RangeX,  "X");
			ValidateRange(y2, RangeY2, "Y2");
			Y1 = y1;
			X  = x;
			Y2 = y2;
		}
		#endregion

		#region ColorSpace Methods
		protected override void FromRgb(Rgb color)
		{
			var xyz      = color.ToColorSpace<Xyz>();
			var dividend = xyz.X + xyz.Y + xyz.Z;
			Y1 = xyz.Y;
			X  = dividend.ApproximatelyEquals(0) ? 0 : xyz.X / dividend;
			Y2 = dividend.ApproximatelyEquals(0) ? 0 : xyz.Y / dividend;
		}

		protected override Rgb ToRgb()
		{
			Xyz xyz;
			if (Y2.Equals(0))
			{
				xyz = new Xyz(0, 0, 0);
			}
			else
			{
				xyz = new Xyz { X = X * (Y1 / Y2), Y = Y1, Z = (1.0 - X - Y2) * (Y1 / Y2) };
			}

			return xyz.ToColorSpace<Rgb>();
		}

		public static Yxy Random()
		{
			return Random<Yxy>(
				new Range<double>(RangeY1.Minimum, RangeY1.Maximum, 0.1),
				new Range<double>(RangeX.Minimum,  RangeX.Maximum,  0.001),
				new Range<double>(RangeY2.Minimum, RangeY2.Maximum, 0.001));
		}
		#endregion

		#region Interface Implementation (IEquatable<T>, IVirtuallyEquatable<T>)
		public override bool Equals(ColorSpace other)
		{
			var otherColor = other as Yxy;
			if (otherColor != null)
				return Y1.Equals(otherColor.Y1) && X.Equals(otherColor.X) && Y2.Equals(otherColor.Y2);
			return false;
		}

		public override bool VirtuallyEquals(ColorSpace other)
		{
			var otherColor = other as Yxy;
			if (otherColor != null)
			{
				return Y1.ApproximatelyEquals(otherColor.Y1) && X.ApproximatelyEquals(otherColor.X) &&
					Y2.ApproximatelyEquals(otherColor.Y2);
			}

			return false;
		}
		#endregion

		#region Interface Implementation (IXNode<T>)
		public override void FromXNode(XElement element)
		{
			if (element.Name != "ColorSpace")
				throw new XmlException("XElement name must be \"ColorSpace\"");
			if (element.Attribute("Type").Value != "YXY")
				throw new XmlException("Expected ColorSpace type YXY");

			Y1 = Double.Parse(element.Attribute("Y1").Value);
			X  = Double.Parse(element.Attribute("X").Value);
			Y2 = Double.Parse(element.Attribute("Y2").Value);
		}

		public override XElement ToXNode()
		{
			var result = new XElement("ColorSpace");
			result.Add(new XAttribute("Type", "YXY"));
			result.Add(new XAttribute("Y1",   Y1));
			result.Add(new XAttribute("X",    X));
			result.Add(new XAttribute("Y2",   Y2));
			return result;
		}
		#endregion

		#region Method Overrides (System.Object)
		public override bool Equals(object obj)
		{
			var other = obj as Yxy;
			return other != null && Equals(other);
		}

		public override int GetHashCode()
		{
			return Y1.GetHashCode() + X.GetHashCode() + Y2.GetHashCode();
		}

		public override string ToString()
		{
			return String.Format("{3} [Y={0:F2}%, x={1:F2}, y={2:F2}]", Y1, X, Y2, FriendlyName);
		}
		#endregion
	}
}