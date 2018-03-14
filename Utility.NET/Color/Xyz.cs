// ┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  FILE:       Xyz.cs
// ┃  PROJECT:    Utility.NET
// ┃  SOLUTION:   Utility.NET
// ┃  CREATED:    2015-12-25 @ 11:40 PM
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
	public sealed class Xyz : ColorSpace
	{
		#region Constants
		public const   string FriendlyName  = "XYZ";
		public const   int    PropertyCount = 3;
		internal const double Epsilon       = 0.008856; // Intent = 216/24389
		internal const double Kappa         = 903.3;    // Intent = 24389/27
		#endregion

		#region Properties
		public static          double[]      WhiteReference { get; set; }
		public                 double        X              { get; set; }
		public                 double        Y              { get; set; }
		public                 double        Z              { get; set; }
		public static readonly Range<double> RangeX = new Range<double>(0.0, 100.0);
		public static readonly Range<double> RangeY = new Range<double>(0.0, 100.0);
		public static readonly Range<double> RangeZ = new Range<double>(0.0, 100.0);
		#endregion

		#region Indexers
		public override double this[int index]
		{
			get
			{
				switch (index)
				{
					case 0:
						return X;
					case 1:
						return Y;
					case 2:
						return Z;
					default:
						return Double.NaN;
				}
			}
			set
			{
				switch (index)
				{
					case 0:
						X = value;
						break;
					case 1:
						Y = value;
						break;
					case 2:
						Z = value;
						break;
					default:
						throw new IndexOutOfRangeException("No property exists for this ColorSpace at the specified index");
				}
			}
		}
		#endregion

		#region Constructors
		static Xyz()
		{
			WhiteReference = IlluminantTristimulus.D65;
		}

		public Xyz() : this(0, 0, 0) { }

		public Xyz(double x, double y, double z)
		{
			ValidateRange(x, RangeX, "X");
			ValidateRange(y, RangeY, "Y");
			ValidateRange(z, RangeZ, "Z");
			X = x;
			Y = y;
			Z = z;
		}
		#endregion

		#region ColorSpace Methods
		protected override void FromRgb(Rgb color)
		{
			var r = color.R / 255.0;
			var g = color.G / 255.0;
			var b = color.B / 255.0;

			r = (r > 0.04045 ? System.Math.Pow((r + 0.055) / 1.055, 2.4) : r / 12.92) * 100.0;
			g = (g > 0.04045 ? System.Math.Pow((g + 0.055) / 1.055, 2.4) : g / 12.92) * 100.0;
			b = (b > 0.04045 ? System.Math.Pow((b + 0.055) / 1.055, 2.4) : b / 12.92) * 100.0;

			// TODO: Hard-coded to use sRGB. Update to allow multiple working spaces
			X = r * RgbXyzMatrix.SRgb[0, 0] + g * RgbXyzMatrix.SRgb[0, 1] + b * RgbXyzMatrix.SRgb[0, 2];
			Y = r * RgbXyzMatrix.SRgb[1, 0] + g * RgbXyzMatrix.SRgb[1, 1] + b * RgbXyzMatrix.SRgb[1, 2];
			Z = r * RgbXyzMatrix.SRgb[2, 0] + g * RgbXyzMatrix.SRgb[2, 1] + b * RgbXyzMatrix.SRgb[2, 2];
		}

		protected override Rgb ToRgb()
		{
			var x = X / 100.0;
			var y = Y / 100.0;
			var z = Z / 100.0;

			// TODO: Hard-coded to use sRGB. Update to allow multiple working spaces
			var r = x * XyzRgbMatrix.SRgb[0, 0] + y * XyzRgbMatrix.SRgb[0, 1] + z * XyzRgbMatrix.SRgb[0, 2];
			var g = x * XyzRgbMatrix.SRgb[1, 0] + y * XyzRgbMatrix.SRgb[1, 1] + z * XyzRgbMatrix.SRgb[1, 2];
			var b = x * XyzRgbMatrix.SRgb[2, 0] + y * XyzRgbMatrix.SRgb[2, 1] + z * XyzRgbMatrix.SRgb[2, 2];

			r = r > 0.0031308 ? 1.055 * System.Math.Pow(r, 1 / 2.4) - 0.055 : 12.92 * r;
			g = g > 0.0031308 ? 1.055 * System.Math.Pow(g, 1 / 2.4) - 0.055 : 12.92 * g;
			b = b > 0.0031308 ? 1.055 * System.Math.Pow(b, 1 / 2.4) - 0.055 : 12.92 * b;

			return new Rgb { R = ScaleAndTrim(r), G = ScaleAndTrim(g), B = ScaleAndTrim(b) };
		}

		public static Xyz Random()
		{
			return Random<Xyz>(
				new Range<double>(RangeX.Minimum, RangeX.Maximum, 0.1),
				new Range<double>(RangeY.Minimum, RangeY.Maximum, 0.1),
				new Range<double>(RangeZ.Minimum, RangeZ.Maximum, 0.1));
		}
		#endregion

		#region Private Methods
		private static double ScaleAndTrim(double value)
		{
			var result = 255.0 * value;
			if (result < 0) return 0;
			if (result > 255) return 255;
			return result;
		}
		#endregion

		#region Internal Classes
		internal static class RgbXyzMatrix
		{
			internal static readonly double[,] SRgb =
				{ { 0.4124564, 0.3575761, 0.1804375 }, { 0.2126729, 0.7151522, 0.0721750 }, { 0.0193339, 0.1191920, 0.9503041 } };
		}

		internal static class XyzRgbMatrix
		{
			internal static readonly double[,] SRgb =
			{
				{ 3.2404542, -1.5371385, -0.4985314 }, { -0.9692660, 1.8760108, 0.0415560 }, { 0.0556434, -0.2040259, 1.0572252 }
			};
		}

		internal static class IlluminantTristimulus
		{
			internal static readonly double[] A   = { 109.850, 100.0, 35.585 };
			internal static readonly double[] B   = { 99.072, 100.0, 85.223 };
			internal static readonly double[] C   = { 98.074, 100.0, 118.232 };
			internal static readonly double[] D50 = { 96.422, 100.0, 82.521 };
			internal static readonly double[] D55 = { 95.682, 100.0, 92.149 };
			internal static readonly double[] D65 = { 95.047, 100.0, 108.883 };
			internal static readonly double[] D75 = { 94.972, 100.0, 122.638 };
			internal static readonly double[] E   = { 100.0, 100.0, 100.0 };
			internal static readonly double[] F2  = { 99.186, 100.0, 67.393 };
			internal static readonly double[] F7  = { 95.041, 100.0, 108.747 };
			internal static readonly double[] F11 = { 100.962, 100.0, 64.350 };
		}
		#endregion

		#region Interface Implementation (IEquatable<T>, IVirtuallyEquatable<T>)
		public override bool Equals(ColorSpace other)
		{
			var otherColor = other as Xyz;
			if (otherColor != null)
				return X.Equals(otherColor.X) && Y.Equals(otherColor.Y) && Z.Equals(otherColor.Z);
			return false;
		}

		public override bool VirtuallyEquals(ColorSpace other)
		{
			var otherColor = other as Xyz;
			if (otherColor != null)
			{
				return X.ApproximatelyEquals(otherColor.X) && Y.ApproximatelyEquals(otherColor.Y) &&
					Z.ApproximatelyEquals(otherColor.Z);
			}

			return false;
		}
		#endregion

		#region Interface Implementation (IXNode<T>)
		public override void FromXNode(XElement element)
		{
			if (element.Name != "ColorSpace")
				throw new XmlException("XElement name must be \"ColorSpace\"");
			if (element.Attribute("Type").Value != "XYZ")
				throw new XmlException("Expected ColorSpace type XYZ");

			X = Double.Parse(element.Attribute("X").Value);
			Y = Double.Parse(element.Attribute("Y").Value);
			Z = Double.Parse(element.Attribute("Z").Value);
		}

		public override XElement ToXNode()
		{
			var result = new XElement("ColorSpace");
			result.Add(new XAttribute("Type", "XYZ"));
			result.Add(new XAttribute("X",    X));
			result.Add(new XAttribute("Y",    Y));
			result.Add(new XAttribute("Z",    Z));
			return result;
		}
		#endregion

		#region Method Overrides (System.Object)
		public override bool Equals(object obj)
		{
			var other = obj as Xyz;
			return other != null && Equals(other);
		}

		public override int GetHashCode()
		{
			return X.GetHashCode() + Y.GetHashCode() + Z.GetHashCode();
		}

		public override string ToString()
		{
			return String.Format("{3} [X={0:F2}, Y={1:F2}, Z={2:F2}]", X, Y, Z, FriendlyName);
		}
		#endregion
	}
}