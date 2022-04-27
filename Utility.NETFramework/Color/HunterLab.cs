// ┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  FILE:       HunterLab.cs
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
	public sealed class HunterLab : ColorSpace
	{
		#region Constants
		public const string FriendlyName  = "HunterLab";
		public const int    PropertyCount = 3;
		#endregion

		#region Properties
		public                 double        L { get; set; }
		public                 double        A { get; set; }
		public                 double        B { get; set; }
		public static readonly DiscreteRange<double> RangeL = new DiscreteRange<double>(0.0,    100.0);
		public static readonly DiscreteRange<double> RangeA = new DiscreteRange<double>(-128.0, 128.0);
		public static readonly DiscreteRange<double> RangeB = new DiscreteRange<double>(-128.0, 128.0);
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
		public HunterLab(Rgb color)
		{
			FromRgb(color);
		}

		public HunterLab() : this(0, 0, 0) { }

		public HunterLab(double l, double a, double b)
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
			L = 10.0 * System.Math.Sqrt(xyz.Y);
			A = !xyz.Y.Equals(0) ? 17.5 * ((1.02 * xyz.X - xyz.Y) / System.Math.Sqrt(xyz.Y)) : 0;
			B = !xyz.Y.Equals(0) ? 7.0 * ((xyz.Y - 0.847 * xyz.Z) / System.Math.Sqrt(xyz.Y)) : 0;
		}

		protected override Rgb ToRgb()
		{
			var x = A / 17.5 * (L / 10.0);
			var y = L / 10.0 * (L / 10.0);
			var z = B / 7.0 * (L / 10.0);

			var xyz = new Xyz { X = (x + y) / 1.02, Y = y, Z = -(z - y) / 0.847 };
			return xyz.ToColorSpace<Rgb>();
		}

		public static HunterLab Random()
		{
			return Random<HunterLab>(
				new DiscreteRange<double>(RangeL.Minimum, RangeL.Maximum, 0.1),
				new DiscreteRange<double>(RangeA.Minimum, RangeA.Maximum, 0.256),
				new DiscreteRange<double>(RangeB.Minimum, RangeB.Maximum, 0.256));
		}
		#endregion

		#region Interface Implementation (IEquatable<T>, IVirtuallyEquatable<T>)
		public override bool Equals(ColorSpace other)
		{
			var otherColor = other as HunterLab;
			if (otherColor != null)
				return L.Equals(otherColor.L) && A.Equals(otherColor.A) && B.Equals(otherColor.B);
			return false;
		}

		public override bool VirtuallyEquals(ColorSpace other)
		{
			var otherColor = other as HunterLab;
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
			if (element.Attribute("Type").Value != "HunterLAB")
				throw new XmlException("Expected ColorSpace type HunterLAB");

			L = Double.Parse(element.Attribute("L").Value);
			A = Double.Parse(element.Attribute("A").Value);
			B = Double.Parse(element.Attribute("B").Value);
		}

		public override XElement ToXNode()
		{
			var result = new XElement("ColorSpace");
			result.Add(new XAttribute("Type", "HunterLAB"));
			result.Add(new XAttribute("L",    L));
			result.Add(new XAttribute("A",    A));
			result.Add(new XAttribute("B",    B));
			return result;
		}
		#endregion

		#region Method Overrides (System.Object)
		public override bool Equals(object obj)
		{
			var other = obj as HunterLab;
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