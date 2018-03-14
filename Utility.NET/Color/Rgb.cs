// ┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  FILE:       ColorSpaceRgb.cs
// ┃  PROJECT:    Utility.NET
// ┃  SOLUTION:   Utility.NET
// ┃  CREATED:    2015-12-25 @ 11:35 PM
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
	public class Rgb : ColorSpace
	{
		#region Constants
		public const string FriendlyName  = "RGB";
		public const int    PropertyCount = 3;
		#endregion

		#region Properties
		public                 double        R { get; set; }
		public                 double        G { get; set; }
		public                 double        B { get; set; }
		public static readonly Range<double> RangeR = new Range<double>(0.0, 255.0);
		public static readonly Range<double> RangeG = new Range<double>(0.0, 255.0);
		public static readonly Range<double> RangeB = new Range<double>(0.0, 255.0);
		#endregion

		#region Indexers
		public override double this[int index]
		{
			get
			{
				switch (index)
				{
					case 0:
						return R;
					case 1:
						return G;
					case 2:
						return B;
					default:
						return Double.NaN;
				}
			}
			set
			{
				switch (index)
				{
					case 0:
						R = value;
						break;
					case 1:
						G = value;
						break;
					case 2:
						B = value;
						break;
					default:
						throw new IndexOutOfRangeException("No property exists for this ColorSpace at the specified index");
				}
			}
		}
		#endregion

		#region Constructors
		public Rgb() : this(0, 0, 0) { }

		public Rgb(double r, double g, double b)
		{
			ValidateRange(r, RangeR, "R");
			ValidateRange(g, RangeG, "G");
			ValidateRange(b, RangeB, "B");
			R = r;
			G = g;
			B = b;
		}
		#endregion

		#region ColorSpace Methods
		protected override void FromRgb(Rgb color)
		{
			R = color.R;
			G = color.G;
			B = color.B;
		}

		protected override Rgb ToRgb()
		{
			return this;
		}

		public static Rgb Random()
		{
			return Random<Rgb>(
				new Range<double>(RangeR.Minimum, RangeR.Maximum, 0.255),
				new Range<double>(RangeG.Minimum, RangeG.Maximum, 0.255),
				new Range<double>(RangeB.Minimum, RangeB.Maximum, 0.255));
		}
		#endregion

		#region Interface Implementation (IEquatable<T>, IVirtuallyEquatable<T>)
		public override bool Equals(ColorSpace other)
		{
			var otherColor = other as Rgb;
			if (otherColor != null)
				return R.Equals(otherColor.R) && G.Equals(otherColor.G) && B.Equals(otherColor.B);
			return false;
		}

		public override bool VirtuallyEquals(ColorSpace other)
		{
			var otherColor = other as Rgb;
			if (otherColor != null)
			{
				return R.ApproximatelyEquals(otherColor.R) && G.ApproximatelyEquals(otherColor.G) &&
					B.ApproximatelyEquals(otherColor.B);
			}

			return false;
		}
		#endregion

		#region Interface Implementation (IXNode<T>)
		public override void FromXNode(XElement element)
		{
			if (element.Name != "ColorSpace")
				throw new XmlException("XElement name must be \"ColorSpace\"");
			if (element.Attribute("Type").Value != "RGB")
				throw new XmlException("Expected ColorSpace type RGB");

			R = Double.Parse(element.Attribute("R").Value);
			G = Double.Parse(element.Attribute("G").Value);
			B = Double.Parse(element.Attribute("B").Value);
		}

		public override XElement ToXNode()
		{
			var result = new XElement("ColorSpace");
			result.Add(new XAttribute("Type", "RGB"));
			result.Add(new XAttribute("R",    R));
			result.Add(new XAttribute("G",    G));
			result.Add(new XAttribute("B",    B));
			return result;
		}
		#endregion

		#region Method Overrides (System.Object)
		public override bool Equals(object obj)
		{
			var other = obj as Rgb;
			return other != null && Equals(other);
		}

		public override int GetHashCode()
		{
			return R.GetHashCode() + G.GetHashCode() + B.GetHashCode();
		}

		public override string ToString()
		{
			return $"RGB [R={R:F2}, G={G:F2}, B={B:F2}]";
		}
		#endregion
	}
}