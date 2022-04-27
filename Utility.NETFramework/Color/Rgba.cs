// ┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  FILE:       Rgba.cs
// ┃  PROJECT:    Utility.NET
// ┃  SOLUTION:   Utility.NET
// ┃  CREATED:    2015-12-25 @ 11:39 PM
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
	public sealed class Rgba : Rgb
	{
		#region Constants
		public new const string FriendlyName  = "RGBA";
		public new const int    PropertyCount = 4;
		#endregion

		#region Properties
		public                 double        A { get; set; }
		public static readonly DiscreteRange<double> RangeA = new DiscreteRange<double>(0.0, 255.0);
		#endregion

		#region Indexers
		public override double this[int index]
		{
			get { return index == 3 ? A : base[index]; }
			set
			{
				if (index == 3) A = value;
				else base[index]  = value;
			}
		}
		#endregion

		#region Constructors
		public Rgba() : this(0, 0, 0, 255) { }

		public Rgba(double r, double g, double b, double a) : base(r, g, b)
		{
			ValidateRange(a, RangeB, "A");
			A = a;
		}
		#endregion

		#region ColorSpace Methods
		protected override void FromRgb(Rgb color)
		{
			base.FromRgb(color);
			var rgba = color as Rgba;
			if (rgba != null)
				A = rgba.A;
		}

		public new static Rgba Random()
		{
			return Random<Rgba>(
				new DiscreteRange<double>(RangeR.Minimum, RangeR.Maximum, 0.255),
				new DiscreteRange<double>(RangeG.Minimum, RangeG.Maximum, 0.255),
				new DiscreteRange<double>(RangeB.Minimum, RangeB.Maximum, 0.255),
				new DiscreteRange<double>(RangeA.Minimum, RangeA.Maximum, 0.255));
		}
		#endregion

		#region Interface Implementation (IEquatable<T>, IVirtuallyEquatable<T>)
		public override bool Equals(ColorSpace other)
		{
			var otherColor = other as Rgba;
			if (otherColor != null)
				return base.Equals(otherColor) && A.Equals(otherColor.A);
			return false;
		}

		public override bool VirtuallyEquals(ColorSpace other)
		{
			var otherColor = other as Rgba;
			if (otherColor != null)
			{
				return base.VirtuallyEquals(otherColor) && A.ApproximatelyEquals(otherColor.A);
			}

			return false;
		}
		#endregion

		#region Interface Implementation (IXNode<T>)
		public override void FromXNode(XElement element)
		{
			if (element.Name != "ColorSpace")
				throw new XmlException("XElement name must be \"ColorSpace\"");
			if (element.Attribute("Type").Value != "RGBA")
				throw new XmlException("Expected ColorSpace type RGBA");

			R = Double.Parse(element.Attribute("R").Value);
			G = Double.Parse(element.Attribute("G").Value);
			B = Double.Parse(element.Attribute("B").Value);
			A = Double.Parse(element.Attribute("A").Value);
		}

		public override XElement ToXNode()
		{
			var result = new XElement("ColorSpace");
			result.Add(new XAttribute("Type", "RGBA"));
			result.Add(new XAttribute("R",    R));
			result.Add(new XAttribute("G",    G));
			result.Add(new XAttribute("B",    B));
			result.Add(new XAttribute("A",    A));
			return result;
		}
		#endregion

		#region Method Overrides (System.Object)
		public override int GetHashCode()
		{
			return base.GetHashCode() + A.GetHashCode();
		}

		public override string ToString()
		{
			return $"RGBA [R={R:F2}, G={G:F2}, B={B:F2}, A={A:F2}]";
		}
		#endregion
	}
}