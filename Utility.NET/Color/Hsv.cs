// ┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  FILE:       Hsv.cs
// ┃  PROJECT:    Utility.NET
// ┃  SOLUTION:   Utility.NET
// ┃  CREATED:    2015-12-25 @ 11:32 PM
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
	public sealed class Hsv : ColorSpace
	{
		#region Constants
		public const string FriendlyName  = "HSV";
		public const int    PropertyCount = 3;
		#endregion

		#region Properties
		public                 double        H { get; set; }
		public                 double        S { get; set; }
		public                 double        V { get; set; }
		public static readonly Range<double> RangeH = new Range<double>(0.0, 360.0);
		public static readonly Range<double> RangeS = new Range<double>(0.0, 1.0);
		public static readonly Range<double> RangeV = new Range<double>(0.0, 1.0);
		#endregion

		#region Indexers
		public override double this[int index]
		{
			get
			{
				switch (index)
				{
					case 0:
						return H;
					case 1:
						return S;
					case 2:
						return V;
					default:
						return Double.NaN;
				}
			}
			set
			{
				switch (index)
				{
					case 0:
						H = value.Equals(360.0) ? 0 : value;
						break;
					case 1:
						S = value;
						break;
					case 2:
						V = value;
						break;
					default:
						throw new IndexOutOfRangeException("No property exists for this ColorSpace at the specified index");
				}
			}
		}
		#endregion

		#region Constructors
		public Hsv(Rgb color)
		{
			FromRgb(color);
		}

		public Hsv() : this(0, 0, 0) { }

		public Hsv(double h, double s, double v)
		{
			ValidateRange(h, RangeH, "H");
			ValidateRange(s, RangeS, "S");
			ValidateRange(v, RangeV, "V");
			if (h.Equals(360.0)) h = 0;
			H = h;
			S = s;
			V = v;
		}
		#endregion

		#region ColorSpace Methods
		protected override void FromRgb(Rgb color)
		{
			var r1 = color.R / 255.0;
			var g1 = color.G / 255.0;
			var b1 = color.B / 255.0;

			var cMin = r1;
			var cMax = r1;
			if (g1 > cMax)
				cMax = g1;
			if (b1 > cMax)
				cMax = b1;
			if (g1 < cMin)
				cMin = g1;
			if (b1 < cMin)
				cMin = b1;
			var delta = cMax - cMin;

			H = 0;
			S = 0;
			V = cMax;

			if (!color.R.Equals(color.G) || !color.G.Equals(color.B))
			{
				if (r1.Equals(cMax))
					H = (g1 - b1) / delta;
				else if (g1.Equals(cMax))
					H = 2.0 + (b1 - r1) / delta;
				else if (b1.Equals(cMax))
					H = 4.0 + (r1 - g1) / delta;

				H *= 60.0;
				if (H < 0) H += 360.0;
			}

			if (!delta.Equals(0))
			{
				S = delta / cMax;
			}
		}

		protected override Rgb ToRgb()
		{
			double r = 0.0, g = 0.0, b = 0.0;
			var    d = V * S;
			var    m = V - d;
			var    x = d * (1.0 - System.Math.Abs(H / 60.0 % 2.0 - 1.0));

			if (H >= 0 && H < 60)
			{
				r = d + m;
				g = x + m;
				b = m;
			}
			else if (H >= 60 && H < 120)
			{
				r = x + m;
				g = d + m;
				b = m;
			}
			else if (H >= 120 && H < 180)
			{
				r = m;
				g = d + m;
				b = x + m;
			}
			else if (H >= 180 && H < 240)
			{
				r = m;
				g = x + m;
				b = d + m;
			}
			else if (H >= 240 && H < 300)
			{
				r = x + m;
				g = m;
				b = d + m;
			}
			else if (H >= 300 && H < 360)
			{
				r = d + m;
				g = m;
				b = x + m;
			}

			if (r < 0) r = 0;
			if (g < 0) g = 0;
			if (b < 0) b = 0;
			return new Rgb(255.0 * r, 255.0 * g, 255.0 * b);
		}

		public static Hsv Random()
		{
			return Random<Hsv>(
				new Range<double>(RangeH.Minimum, RangeH.Maximum, 0.036),
				new Range<double>(RangeS.Minimum, RangeS.Maximum, 0.001),
				new Range<double>(RangeV.Minimum, RangeV.Maximum, 0.001));
		}
		#endregion

		#region Interface Implementation (IEquatable<T>, IVirtuallyEquatable<T>)
		public override bool Equals(ColorSpace other)
		{
			var otherColor = other as Hsv;
			if (otherColor != null)
				return H.Equals(otherColor.H) && S.Equals(otherColor.S) && V.Equals(otherColor.V);
			return false;
		}

		public override bool VirtuallyEquals(ColorSpace other)
		{
			var otherColor = other as Hsv;
			if (otherColor != null)
			{
				return H.ApproximatelyEquals(otherColor.H) && S.ApproximatelyEquals(otherColor.S) &&
					V.ApproximatelyEquals(otherColor.V);
			}

			return false;
		}
		#endregion

		#region Interface Implementation (IXNode<T>)
		public override void FromXNode(XElement element)
		{
			if (element.Name != "ColorSpace")
				throw new XmlException("XElement name must be \"ColorSpace\"");
			if (element.Attribute("Type").Value != "HSV")
				throw new XmlException("Expected ColorSpace type HSV");

			H = Double.Parse(element.Attribute("H").Value);
			S = Double.Parse(element.Attribute("S").Value);
			V = Double.Parse(element.Attribute("V").Value);
		}

		public override XElement ToXNode()
		{
			var result = new XElement("ColorSpace");
			result.Add(new XAttribute("Type", "HSV"));
			result.Add(new XAttribute("H",    H));
			result.Add(new XAttribute("S",    S));
			result.Add(new XAttribute("V",    V));
			return result;
		}
		#endregion

		#region Method Overrides (System.Object)
		public override bool Equals(object obj)
		{
			var other = obj as Hsv;
			return other != null && Equals(other);
		}

		public override int GetHashCode()
		{
			return H.GetHashCode() + S.GetHashCode() + V.GetHashCode();
		}

		public override string ToString()
		{
			return String.Format("{3} [H={0:F2}°, S={1:F2}%, V={2:F2}%]", H, S * 100, V * 100, FriendlyName);
		}
		#endregion
	}
}