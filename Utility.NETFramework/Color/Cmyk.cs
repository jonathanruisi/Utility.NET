// ┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  FILE:       Cmyk.cs
// ┃  PROJECT:    Utility.NET
// ┃  SOLUTION:   Utility.NET
// ┃  CREATED:    2015-12-25 @ 11:30 PM
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
	public sealed class Cmyk : ColorSpace
	{
		#region Constants
		public const string FriendlyName  = "CMYK";
		public const int    PropertyCount = 4;
		#endregion

		#region Properties
		public                 double        C { get; set; }
		public                 double        M { get; set; }
		public                 double        Y { get; set; }
		public                 double        K { get; set; }
		public static readonly DiscreteRange<double> RangeC = new DiscreteRange<double>(0.0, 1.0);
		public static readonly DiscreteRange<double> RangeM = new DiscreteRange<double>(0.0, 1.0);
		public static readonly DiscreteRange<double> RangeY = new DiscreteRange<double>(0.0, 1.0);
		public static readonly DiscreteRange<double> RangeK = new DiscreteRange<double>(0.0, 1.0);
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
					case 3:
						return K;
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
					case 3:
						K = value;
						break;
					default:
						throw new IndexOutOfRangeException("No property exists for this ColorSpace at the specified index");
				}
			}
		}
		#endregion

		#region Constructors
		public Cmyk(Rgb color)
		{
			FromRgb(color);
		}

		public Cmyk() : this(0, 0, 0, 0) { }

		public Cmyk(double c, double m, double y, double k)
		{
			ValidateRange(c, RangeC, "C");
			ValidateRange(m, RangeM, "M");
			ValidateRange(y, RangeY, "Y");
			ValidateRange(k, RangeK, "K");
			C = c;
			M = m;
			Y = y;
			K = k;
		}
		#endregion

		#region ColorSpace Methods
		protected override void FromRgb(Rgb color)
		{
			var cmy = new Cmy(color);
			var k   = 1.0;

			if (cmy.C < k)
				k = cmy.C;
			if (cmy.M < k)
				k = cmy.M;
			if (cmy.Y < k)
				k = cmy.Y;
			K = k;

			if (k.ApproximatelyEquals(1.0))
			{
				C = 0;
				M = 0;
				Y = 0;
			}
			else
			{
				C = (cmy.C - k) / (1 - k);
				M = (cmy.M - k) / (1 - k);
				Y = (cmy.Y - k) / (1 - k);
			}
		}

		protected override Rgb ToRgb()
		{
			var cmy = new Cmy { C = C * (1 - K) + K, M = M * (1 - K) + K, Y = Y * (1 - K) + K };
			return cmy.ToColorSpace<Rgb>();
		}

		public static Cmyk Random()
		{
			return Random<Cmyk>(
				new DiscreteRange<double>(RangeC.Minimum, RangeC.Maximum, 0.001),
				new DiscreteRange<double>(RangeM.Minimum, RangeM.Maximum, 0.001),
				new DiscreteRange<double>(RangeY.Minimum, RangeY.Maximum, 0.001),
				new DiscreteRange<double>(RangeK.Minimum, RangeK.Maximum, 0.001));
		}
		#endregion

		#region Interface Implementation (IEquatable<T>, IVirtuallyEquatable<T>)
		public override bool Equals(ColorSpace other)
		{
			var otherColor = other as Cmyk;
			if (otherColor != null)
				return C.Equals(otherColor.C) && M.Equals(otherColor.M) && Y.Equals(otherColor.Y) && K.Equals(otherColor.K);
			return false;
		}

		public override bool VirtuallyEquals(ColorSpace other)
		{
			var otherColor = other as Cmyk;
			if (otherColor != null)
			{
				return C.ApproximatelyEquals(otherColor.C) && M.ApproximatelyEquals(otherColor.M) &&
					Y.ApproximatelyEquals(otherColor.Y) && K.ApproximatelyEquals(otherColor.K);
			}

			return false;
		}
		#endregion

		#region Interface Implementation (IXNode<T>)
		public override void FromXNode(XElement element)
		{
			if (element.Name != "ColorSpace")
				throw new XmlException("XElement name must be \"ColorSpace\"");
			if (element.Attribute("Type").Value != "CMYK")
				throw new XmlException("Expected ColorSpace type CMYK");

			C = Double.Parse(element.Attribute("C").Value);
			M = Double.Parse(element.Attribute("M").Value);
			Y = Double.Parse(element.Attribute("Y").Value);
			K = Double.Parse(element.Attribute("K").Value);
		}

		public override XElement ToXNode()
		{
			var result = new XElement("ColorSpace");
			result.Add(new XAttribute("Type", "CMYK"));
			result.Add(new XAttribute("C",    C));
			result.Add(new XAttribute("M",    M));
			result.Add(new XAttribute("Y",    Y));
			result.Add(new XAttribute("K",    K));
			return result;
		}
		#endregion

		#region Method Overrides (System.Object)
		public override bool Equals(object obj)
		{
			var other = obj as Cmyk;
			return other != null && Equals(other);
		}

		public override int GetHashCode()
		{
			return C.GetHashCode() + M.GetHashCode() + Y.GetHashCode() + K.GetHashCode();
		}

		public override string ToString()
		{
			return String.Format(
				"{4} [C={0:F2}%, M={1:F2}%, Y={2:F2}%, K={3:F2}%]",
				C * 100,
				M * 100,
				Y * 100,
				K * 100,
				FriendlyName);
		}
		#endregion
	}
}