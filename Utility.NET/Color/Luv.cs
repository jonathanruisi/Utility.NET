// ┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  FILE:       Luv.cs
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
	public sealed class Luv : ColorSpace
	{
		#region Constants
		public const string FriendlyName  = "CIELuv";
		public const int    PropertyCount = 3;
		#endregion

		#region Properties
		public                 double        L { get; set; }
		public                 double        U { get; set; }
		public                 double        V { get; set; }
		public static readonly DiscreteRange<double> RangeL = new DiscreteRange<double>(0.0,    100.0);
		public static readonly DiscreteRange<double> RangeU = new DiscreteRange<double>(-134.0, 224.0);
		public static readonly DiscreteRange<double> RangeV = new DiscreteRange<double>(-140.0, 122.0);
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
						return U;
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
						L = value;
						break;
					case 1:
						U = value;
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
		public Luv(Rgb color)
		{
			FromRgb(color);
		}

		public Luv() : this(0, 0, 0) { }

		public Luv(double l, double u, double v)
		{
			ValidateRange(l, RangeL, "L");
			ValidateRange(u, RangeU, "U");
			ValidateRange(v, RangeV, "V");
			L = l;
			U = u;
			V = v;
		}
		#endregion

		#region ColorSpace Methods
		protected override void FromRgb(Rgb color)
		{
			var xyz = color.ToColorSpace<Xyz>();
			var y   = xyz.Y / Xyz.WhiteReference[1];
			L = y > Xyz.Epsilon ? 116.0 * System.Math.Pow(y, 1.0 / 3.0) - 16.0 : Xyz.Kappa * y;

			var targetDenominator    = xyz.X + 15.0 * xyz.Y + 3.0 * xyz.Z;
			var referenceDenominator = Xyz.WhiteReference[0] + 15.0 * Xyz.WhiteReference[1] + 3.0 * Xyz.WhiteReference[2];
			var xTarget = targetDenominator.Equals(0)
				? 0
				: 4.0 * xyz.X / targetDenominator - 4.0 * Xyz.WhiteReference[0] / referenceDenominator;
			var yTarget = targetDenominator.Equals(0)
				? 0
				: 9.0 * xyz.Y / targetDenominator - 9.0 * Xyz.WhiteReference[1] / referenceDenominator;
			U = 13.0 * L * xTarget;
			V = 13.0 * L * yTarget;
		}

		protected override Rgb ToRgb()
		{
			var referenceDenominator = Xyz.WhiteReference[0] + 15.0 * Xyz.WhiteReference[1] + 3.0 * Xyz.WhiteReference[2];
			var u1                   = 4.0 * Xyz.WhiteReference[0] / referenceDenominator;
			var v1                   = 9.0 * Xyz.WhiteReference[1] / referenceDenominator;
			var a                    = 1.0 / 3.0 * (52.0 * L / (U + 13.0 * L * u1) - 1.0);
			var y                    = L > Xyz.Kappa * Xyz.Epsilon ? System.Math.Pow((L + 16.0) / 116.0, 3) : L / Xyz.Kappa;
			var b                    = -5.0 * y;
			var d                    = y * (39.0 * L / (V + 13.0 * L * v1) - 5.0);
			var x                    = (d - b) / (a - -1.0 / 3.0);
			var z                    = x * a + b;
			var xyz                  = new Xyz { X = 100 * x, Y = 100 * y, Z = 100 * z };
			return xyz.ToColorSpace<Rgb>();
		}

		public static Luv Random()
		{
			return Random<Luv>(
				new DiscreteRange<double>(RangeL.Minimum, RangeL.Maximum, 0.1),
				new DiscreteRange<double>(RangeU.Minimum, RangeU.Maximum, 0.358),
				new DiscreteRange<double>(RangeV.Minimum, RangeV.Maximum, 0.262));
		}
		#endregion

		#region Interface Implementation (IEquatable<T>, IVirtuallyEquatable<T>)
		public override bool Equals(ColorSpace other)
		{
			var otherColor = other as Luv;
			if (otherColor != null)
				return L.Equals(otherColor.L) && U.Equals(otherColor.U) && U.Equals(otherColor.U);
			return false;
		}

		public override bool VirtuallyEquals(ColorSpace other)
		{
			var otherColor = other as Luv;
			if (otherColor != null)
			{
				return L.ApproximatelyEquals(otherColor.L) && U.ApproximatelyEquals(otherColor.U) &&
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
			if (element.Attribute("Type").Value != "LUV")
				throw new XmlException("Expected ColorSpace type LUV");

			L = Double.Parse(element.Attribute("L").Value);
			U = Double.Parse(element.Attribute("U").Value);
			V = Double.Parse(element.Attribute("V").Value);
		}

		public override XElement ToXNode()
		{
			var result = new XElement("ColorSpace");
			result.Add(new XAttribute("Type", "LUV"));
			result.Add(new XAttribute("L",    L));
			result.Add(new XAttribute("U",    U));
			result.Add(new XAttribute("V",    V));
			return result;
		}
		#endregion

		#region Method Overrides (System.Object)
		public override bool Equals(object obj)
		{
			var other = obj as Luv;
			return other != null && Equals(other);
		}

		public override int GetHashCode()
		{
			return L.GetHashCode() + U.GetHashCode() + U.GetHashCode();
		}

		public override string ToString()
		{
			return String.Format("{3} [L={0:F2}%, u={1:F2}, v={2:F2}]", L, U, V, FriendlyName);
		}
		#endregion
	}
}