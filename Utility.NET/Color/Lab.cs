// ┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  FILE:       Lab.cs
// ┃  PROJECT:    Utility.NET
// ┃  SOLUTION:   Utility.NET
// ┃  CREATED:    2015-12-25 @ 11:33 PM
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
	public sealed class Lab : ColorSpace
	{
		#region Constants
		public const string FriendlyName  = "CIELab";
		public const int    PropertyCount = 3;
		#endregion

		#region Properties
		public                 double        L { get; set; }
		public                 double        A { get; set; }
		public                 double        B { get; set; }
		public static readonly Range<double> RangeL = new Range<double>(0.0,    100.0);
		public static readonly Range<double> RangeA = new Range<double>(-128.0, 128.0);
		public static readonly Range<double> RangeB = new Range<double>(-128.0, 128.0);
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
						return A;
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
						L = value;
						break;
					case 1:
						A = value;
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
		public Lab(Rgb color)
		{
			FromRgb(color);
		}

		public Lab() : this(0, 0, 0) { }

		public Lab(double l, double a, double b)
		{
			ValidateRange(l, RangeL, "L");
			ValidateRange(a, RangeA, "A");
			ValidateRange(b, RangeB, "B");
			L = l;
			A = a;
			B = b;
		}
		#endregion

		#region ColorSpace Methods
		protected override void FromRgb(Rgb color)
		{
			var xyz = color.ToColorSpace<Xyz>();
			var x   = xyz.X / Xyz.WhiteReference[0];
			var y   = xyz.Y / Xyz.WhiteReference[1];
			var z   = xyz.Z / Xyz.WhiteReference[2];

			x = x > Xyz.Epsilon ? CubicRoot(x) : (Xyz.Kappa * x + 16) / 116;
			y = y > Xyz.Epsilon ? CubicRoot(y) : (Xyz.Kappa * y + 16) / 116;
			z = z > Xyz.Epsilon ? CubicRoot(z) : (Xyz.Kappa * z + 16) / 116;

			L = System.Math.Max(0, 116 * y - 16);
			A = 500 * (x - y);
			B = 200 * (y - z);
		}

		protected override Rgb ToRgb()
		{
			var y      = (L + 16.0) / 116.0;
			var x      = A / 500.0 + y;
			var z      = y - B / 200.0;
			var xCubed = System.Math.Pow(x, 3);
			var zCubed = System.Math.Pow(z, 3);

			var xyz = new Xyz
			{
				X = Xyz.WhiteReference[0] * (xCubed > Xyz.Epsilon ? xCubed : (x - 16.0 / 116.0) / 7.787),
				Y = Xyz.WhiteReference[1] * (L > Xyz.Kappa * Xyz.Epsilon ? System.Math.Pow((L + 16.0) / 116.0, 3) : L / Xyz.Kappa),
				Z = Xyz.WhiteReference[2] * (zCubed > Xyz.Epsilon ? zCubed : (z - 16.0 / 116.0) / 7.787)
			};

			return xyz.ToColorSpace<Rgb>();
		}

		public static Lab Random()
		{
			return Random<Lab>(
				new Range<double>(RangeL.Minimum, RangeL.Maximum, 0.1),
				new Range<double>(RangeA.Minimum, RangeA.Maximum, 0.256),
				new Range<double>(RangeB.Minimum, RangeB.Maximum, 0.256));
		}
		#endregion

		#region Private Methods
		private static double CubicRoot(double value)
		{
			return System.Math.Pow(value, 1.0 / 3.0);
		}
		#endregion

		#region Interface Implementation (IEquatable<T>, IVirtuallyEquatable<T>)
		public override bool Equals(ColorSpace other)
		{
			var otherColor = other as Lab;
			if (otherColor != null)
				return L.Equals(otherColor.L) && A.Equals(otherColor.A) && B.Equals(otherColor.B);
			return false;
		}

		public override bool VirtuallyEquals(ColorSpace other)
		{
			var otherColor = other as Lab;
			if (otherColor != null)
			{
				return L.ApproximatelyEquals(otherColor.L) && A.ApproximatelyEquals(otherColor.A) &&
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
			if (element.Attribute("Type").Value != "LAB")
				throw new XmlException("Expected ColorSpace type LAB");

			L = Double.Parse(element.Attribute("L").Value);
			A = Double.Parse(element.Attribute("A").Value);
			B = Double.Parse(element.Attribute("B").Value);
		}

		public override XElement ToXNode()
		{
			var result = new XElement("ColorSpace");
			result.Add(new XAttribute("Type", "LAB"));
			result.Add(new XAttribute("L",    L));
			result.Add(new XAttribute("A",    A));
			result.Add(new XAttribute("B",    B));
			return result;
		}
		#endregion

		#region Method Overrides (System.Object)
		public override bool Equals(object obj)
		{
			var other = obj as Lab;
			return other != null && Equals(other);
		}

		public override int GetHashCode()
		{
			return L.GetHashCode() + A.GetHashCode() + B.GetHashCode();
		}

		public override string ToString()
		{
			return String.Format("{3} [L={0:F2}%, a={1:F2}, b={2:F2}]", L, A, B, FriendlyName);
		}
		#endregion
	}
}