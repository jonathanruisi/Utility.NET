// ┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  FILE:       Lch.cs
// ┃  PROJECT:    Utility.NET
// ┃  SOLUTION:   Utility.NET
// ┃  CREATED:    2015-12-25 @ 11:34 PM
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
	public sealed class Lch : ColorSpace
	{
		#region Constants
		public const string FriendlyName  = "CIELCh";
		public const int    PropertyCount = 3;
		#endregion

		#region Properties
		public                 double        L { get; set; }
		public                 double        C { get; set; }
		public                 double        H { get; set; }
		public static readonly Range<double> RangeL = new Range<double>(0.0, 100.0);
		public static readonly Range<double> RangeC = new Range<double>(0.0, 100.0);
		public static readonly Range<double> RangeH = new Range<double>(0.0, 360.0);
		#endregion

		#region Indexers
		public override double this[int index]
		{
			get
			{
				switch (index)
				{
					case 0:
						return L;
					case 1:
						return C;
					case 2:
						return H;
					default:
						return Double.NaN;
				}
			}
			set
			{
				switch (index)
				{
					case 0:
						L = value;
						break;
					case 1:
						C = value;
						break;
					case 2:
						H = value.Equals(360.0) ? 0.0 : value;
						break;
					default:
						throw new IndexOutOfRangeException("No property exists for this ColorSpace at the specified index");
				}
			}
		}
		#endregion

		#region Constructors
		public Lch(Rgb color)
		{
			FromRgb(color);
		}

		public Lch() : this(0, 0, 0) { }

		public Lch(double l, double c, double h)
		{
			ValidateRange(l, RangeL, "L");
			ValidateRange(c, RangeC, "C");
			ValidateRange(h, RangeH, "H");
			if (h.Equals(360.0)) h = 0;
			L = l;
			C = c;
			H = h;
		}
		#endregion

		#region ColorSpace Methods
		protected override void FromRgb(Rgb color)
		{
			var lab  = new Lab(color);
			var hDeg = System.Math.Atan2(lab.B, lab.A);

			if (hDeg > 0)
			{
				hDeg = hDeg / System.Math.PI * 180.0;
			}
			else
			{
				hDeg = 360.0 - System.Math.Abs(hDeg) / System.Math.PI * 180.0;
			}

			if (hDeg < 0)
				hDeg += 360.0;
			else if (hDeg >= 360)
				hDeg -= 360.0;

			L = lab.L;
			C = System.Math.Sqrt(lab.A * lab.A + lab.B * lab.B);
			H = hDeg;
		}

		protected override Rgb ToRgb()
		{
			var hRad = H * System.Math.PI / 180.0;
			var lab  = new Lab { L = L, A = System.Math.Cos(hRad) * C, B = System.Math.Sin(hRad) * C };
			return lab.ToColorSpace<Rgb>();
		}

		public static Lch Random()
		{
			return Random<Lch>(
				new Range<double>(RangeL.Minimum, RangeL.Maximum, 0.1),
				new Range<double>(RangeC.Minimum, RangeC.Maximum, 0.1),
				new Range<double>(RangeH.Minimum, RangeH.Maximum, 0.36));
		}
		#endregion

		#region Interface Implementation (IEquatable<T>, IVirtuallyEquatable<T>)
		public override bool Equals(ColorSpace other)
		{
			var otherColor = other as Lch;
			if (otherColor != null)
				return L.Equals(otherColor.L) && C.Equals(otherColor.C) && H.Equals(otherColor.H);
			return false;
		}

		public override bool VirtuallyEquals(ColorSpace other)
		{
			var otherColor = other as Lch;
			if (otherColor != null)
			{
				return L.ApproximatelyEquals(otherColor.L) && C.ApproximatelyEquals(otherColor.C) &&
					H.ApproximatelyEquals(otherColor.H);
			}

			return false;
		}
		#endregion

		#region Interface Implementation (IXNode<T>)
		public override void FromXNode(XElement element)
		{
			if (element.Name != "ColorSpace")
				throw new XmlException("XElement name must be \"ColorSpace\"");
			if (element.Attribute("Type").Value != "LCH")
				throw new XmlException("Expected ColorSpace type LCH");

			L = Double.Parse(element.Attribute("L").Value);
			C = Double.Parse(element.Attribute("C").Value);
			H = Double.Parse(element.Attribute("H").Value);
		}

		public override XElement ToXNode()
		{
			var result = new XElement("ColorSpace");
			result.Add(new XAttribute("Type", "LCH"));
			result.Add(new XAttribute("L",    L));
			result.Add(new XAttribute("C",    C));
			result.Add(new XAttribute("H",    H));
			return result;
		}
		#endregion

		#region Method Overrides (System.Object)
		public override bool Equals(object obj)
		{
			var other = obj as Lch;
			return other != null && Equals(other);
		}

		public override int GetHashCode()
		{
			return L.GetHashCode() + C.GetHashCode() + H.GetHashCode();
		}

		public override string ToString()
		{
			return String.Format("{3} [L={0:F2}%, C={1:F2}%, h={2:F2}°]", L, C, H, FriendlyName);
		}
		#endregion
	}
}