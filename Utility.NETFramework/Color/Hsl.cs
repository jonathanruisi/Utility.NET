// ┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  FILE:       Hsl.cs
// ┃  PROJECT:    Utility.NET
// ┃  SOLUTION:   Utility.NET
// ┃  CREATED:    2015-12-25 @ 11:31 PM
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

using JLR.Utility.NETFramework.Math;

namespace JLR.Utility.NETFramework.Color
{
	public sealed class Hsl : ColorSpace
	{
		#region Constants
		public const string FriendlyName  = "HSL";
		public const int    PropertyCount = 3;
		#endregion

		#region Properties
		public                 double        H { get; set; }
		public                 double        S { get; set; }
		public                 double        L { get; set; }
		public static readonly DiscreteRange<double> RangeH = new DiscreteRange<double>(0.0, 360.0);
		public static readonly DiscreteRange<double> RangeS = new DiscreteRange<double>(0.0, 1.0);
		public static readonly DiscreteRange<double> RangeL = new DiscreteRange<double>(0.0, 1.0);
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
						return L;
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
						L = value;
						break;
					default:
						throw new IndexOutOfRangeException("No property exists for this ColorSpace at the specified index");
				}
			}
		}
		#endregion

		#region Constructors
		public Hsl(Rgb color)
		{
			FromRgb(color);
		}

		public Hsl() : this(0, 0, 0) { }

		public Hsl(double h, double s, double l)
		{
			ValidateRange(h, RangeH, "H");
			ValidateRange(s, RangeS, "S");
			ValidateRange(l, RangeL, "L");
			if (h.Equals(360.0)) h = 0;
			H = h;
			S = s;
			L = l;
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
			L = (cMax + cMin) / 2.0;

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
				S = delta / (1.0 - System.Math.Abs(2.0 * L - 1.0));
			}
		}

		protected override Rgb ToRgb()
		{
			double r = 0.0, g = 0.0, b = 0.0;
			var    d = S * (1.0 - System.Math.Abs(2.0 * L - 1.0));
			var    m = L - 1.0 / 2.0 * d;
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

		public static Hsl Random()
		{
			return Random<Hsl>(
				new DiscreteRange<double>(RangeH.Minimum, RangeH.Maximum, 0.036),
				new DiscreteRange<double>(RangeS.Minimum, RangeS.Maximum, 0.001),
				new DiscreteRange<double>(RangeL.Minimum, RangeL.Maximum, 0.001));
		}
		#endregion

		#region Interface Implementation (IEquatable<T>, IVirtuallyEquatable<T>)
		public override bool Equals(ColorSpace other)
		{
			var otherColor = other as Hsl;
			if (otherColor != null)
				return H.Equals(otherColor.H) && S.Equals(otherColor.S) && L.Equals(otherColor.L);
			return false;
		}

		public override bool VirtuallyEquals(ColorSpace other)
		{
			var otherColor = other as Hsl;
			if (otherColor != null)
			{
				return H.ApproximatelyEquals(otherColor.H) && S.ApproximatelyEquals(otherColor.S) &&
					L.ApproximatelyEquals(otherColor.L);
			}

			return false;
		}
		#endregion

		#region Interface Implementation (IXNode<T>)
		public override void FromXNode(XElement element)
		{
			if (element.Name != "ColorSpace")
				throw new XmlException("XElement name must be \"ColorSpace\"");
			if (element.Attribute("Type").Value != "HSL")
				throw new XmlException("Expected ColorSpace type HSL");

			H = Double.Parse(element.Attribute("H").Value);
			S = Double.Parse(element.Attribute("S").Value);
			L = Double.Parse(element.Attribute("L").Value);
		}

		public override XElement ToXNode()
		{
			var result = new XElement("ColorSpace");
			result.Add(new XAttribute("Type", "HSL"));
			result.Add(new XAttribute("H",    H));
			result.Add(new XAttribute("S",    S));
			result.Add(new XAttribute("L",    L));
			return result;
		}
		#endregion

		#region Method Overrides (System.Object)
		public override bool Equals(object obj)
		{
			var other = obj as Hsl;
			return other != null && Equals(other);
		}

		public override int GetHashCode()
		{
			return H.GetHashCode() + S.GetHashCode() + L.GetHashCode();
		}

		public override string ToString()
		{
			return String.Format("{3} [H={0:F2}°, S={1:F2}%, L={2:F2}%]", H, S * 100, L * 100, FriendlyName);
		}
		#endregion
	}
}