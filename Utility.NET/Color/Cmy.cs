// ┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  FILE:       Cmy.cs
// ┃  PROJECT:    Utility.NET
// ┃  SOLUTION:   Utility.NET
// ┃  CREATED:    2015-12-25 @ 11:29 PM
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
	public sealed class Cmy : ColorSpace
	{
		#region Constants
		public const string FriendlyName  = "CMY";
		public const int    PropertyCount = 3;
		#endregion

		#region Properties
		public                 double        C { get; set; }
		public                 double        M { get; set; }
		public                 double        Y { get; set; }
		public static readonly Range<double> RangeC = new Range<double>(0.0, 1.0);
		public static readonly Range<double> RangeM = new Range<double>(0.0, 1.0);
		public static readonly Range<double> RangeY = new Range<double>(0.0, 1.0);
		#endregion

		#region Indexers
		public override double this[int index]
		{
			get
			{
				switch (index)
				{
					case 0:
						return C;
					case 1:
						return M;
					case 2:
						return Y;
					default:
						return Double.NaN;
				}
			}
			set
			{
				switch (index)
				{
					case 0:
						C = value;
						break;
					case 1:
						M = value;
						break;
					case 2:
						Y = value;
						break;
					default:
						throw new IndexOutOfRangeException("No property exists for this ColorSpace at the specified index");
				}
			}
		}
		#endregion

		#region Constructors
		public Cmy(Rgb color)
		{
			FromRgb(color);
		}

		public Cmy() : this(0, 0, 0) { }

		public Cmy(double c, double m, double y)
		{
			ValidateRange(c, RangeC, "C");
			ValidateRange(m, RangeM, "M");
			ValidateRange(y, RangeY, "Y");
			C = c;
			M = m;
			Y = y;
		}
		#endregion

		#region ColorSpace Methods
		protected override void FromRgb(Rgb color)
		{
			C = 1 - color.R / 255.0;
			M = 1 - color.G / 255.0;
			Y = 1 - color.B / 255.0;
		}

		protected override Rgb ToRgb()
		{
			return new Rgb { R = (1 - C) * 255.0, G = (1 - M) * 255.0, B = (1 - Y) * 255.0 };
		}

		public static Cmy Random()
		{
			return Random<Cmy>(
				new Range<double>(RangeC.Minimum, RangeC.Maximum, 0.001),
				new Range<double>(RangeM.Minimum, RangeM.Maximum, 0.001),
				new Range<double>(RangeY.Minimum, RangeY.Maximum, 0.001));
		}
		#endregion

		#region Interface Implementation (IEquatable<T>, IVirtuallyEquatable<T>)
		public override bool Equals(ColorSpace other)
		{
			var otherColor = other as Cmy;
			if (otherColor != null)
				return C.Equals(otherColor.C) && M.Equals(otherColor.M) && Y.Equals(otherColor.Y);
			return false;
		}

		public override bool VirtuallyEquals(ColorSpace other)
		{
			var otherColor = other as Cmy;
			if (otherColor != null)
			{
				return C.ApproximatelyEquals(otherColor.C) && M.ApproximatelyEquals(otherColor.M) &&
					Y.ApproximatelyEquals(otherColor.Y);
			}

			return false;
		}
		#endregion

		#region Interface Implementation (IXNode<T>)
		public override void FromXNode(XElement element)
		{
			if (element.Name != "ColorSpace")
				throw new XmlException("XElement name must be \"ColorSpace\"");
			if (element.Attribute("Type").Value != "CMY")
				throw new XmlException("Expected ColorSpace type CMY");

			C = Double.Parse(element.Attribute("C").Value);
			M = Double.Parse(element.Attribute("M").Value);
			Y = Double.Parse(element.Attribute("Y").Value);
		}

		public override XElement ToXNode()
		{
			var result = new XElement("ColorSpace");
			result.Add(new XAttribute("Type", "CMY"));
			result.Add(new XAttribute("C",    C));
			result.Add(new XAttribute("M",    M));
			result.Add(new XAttribute("Y",    Y));
			return result;
		}
		#endregion

		#region Method Overrides (System.Object)
		public override bool Equals(object obj)
		{
			var other = obj as Cmy;
			return other != null && Equals(other);
		}

		public override int GetHashCode()
		{
			return C.GetHashCode() + M.GetHashCode() + Y.GetHashCode();
		}

		public override string ToString()
		{
			return String.Format("{3} [C={0:F2}%, M={1:F2}%, Y={2:F2}%]", C * 100, M * 100, Y * 100, FriendlyName);
		}
		#endregion
	}
}