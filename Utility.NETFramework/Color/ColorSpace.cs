// ┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  FILE:       ColorSpace.cs
// ┃  PROJECT:    Utility.NET
// ┃  SOLUTION:   Utility.NET
// ┃  CREATED:    2015-12-25 @ 11:27 PM
// ┣━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  AUTHOR:     Jonathan Ruisi
// ┃  EMAIL:      JonathanRuisi@gmail.com
// ┣━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  GIT REPO:   https://github.com/jonathanruisi/Utility.NET
// ┃  LICENSE:    https://github.com/jonathanruisi/Utility.NET/blob/master/LICENSE
// ┗━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

using JLR.Utility.NETFramework.Math;
using JLR.Utility.NETFramework.Xml;

namespace JLR.Utility.NETFramework.Color
{
	#region Enumerated Types
	/// <summary>
	/// Algorithms used to calculate the perceived difference between two colors (ΔE).
	/// </summary>
	public enum ComparisonMethod
	{
		/// <summary>
		/// The first formula used by CIE to calculate ΔE.
		/// This formula has been replaced by the CIE1994 and CIE2000 formulas.
		/// </summary>
		Cie1976,

		/// <summary>
		/// Extends the CIE1976 formula to address perceptual non-uniformities.
		/// This formula allows for application specific adjustments for graphic arts and textiles.
		/// This implementation targets graphic arts.
		/// </summary>
		Cie1994,

		/// <summary>
		/// Adds five corrections to the CIE1994 formula to further address perceptual uniformity issues.
		/// </summary>
		Cie2000,

		/// <summary>
		/// An alternative difference measure developed by the Colour Measurement Committee
		/// of the Society of Dyers and Colourists.
		/// </summary>
		Cmc1984
	}

	/// <summary>
	/// Basic color harmonies that can be generated from any given (dominant) color.
	/// </summary>
	public enum ColorHarmony
	{
		/// <summary>
		/// Color opposite to the dominant color
		/// </summary>
		Complementary,

		/// <summary>
		/// Two colors adjacent to the complementary color
		/// </summary>
		SplitComplementary,

		/// <summary>
		/// Two colors adjacent to the dominant color
		/// </summary>
		Analogous,

		/// <summary>
		/// Two colors evenly spaced from the dominant color
		/// </summary>
		Triadic,

		/// <summary>
		/// Four colors arranged in two complementary pairs.
		/// One of the four colors is dominant.
		/// </summary>
		Tetradic,

		/// <summary>
		/// Three colors evenly spaced from the dominant color
		/// </summary>
		Square
	}
	#endregion

	/// <summary>
	/// An abstract class that describes a color in a specific color space.
	/// </summary>
	public abstract class ColorSpace : IXNode<XElement>, IEquatable<ColorSpace>, IVirtuallyEquatable<ColorSpace>,
									   IFormattable, ICloneable
	{
		#region Constants
		private const double JustNoticeableDifference = 2.3;
		#endregion

		#region Fields
		private static readonly Random Rand;
		#endregion

		#region Peoperties
		public string NodeName => "ColorSpace";

		/// <summary>
		/// Creates a ColorSpace object with no color information (Opaque Black)
		/// </summary>
		public static ColorSpace OpaqueBlack => new Rgb(0, 0, 0);

		/// <summary>
		/// Creates a "Transparent Black" ColorSpace object
		/// </summary>
		public static ColorSpace Transparent => new Rgba(0, 0, 0, 0);

		/// <summary>
		/// Returns true if the current color is considered to be on the "warm" side of the color wheel.
		/// </summary>
		public bool IsWarm
		{
			get
			{
				var hsl = ToColorSpace<Hsl>();
				return hsl.H >= 0.0 && hsl.H < 180.0;
			}
		}

		/// <summary>
		/// Returns true if the current color is considered to be on the "cool" side of the color wheel.
		/// </summary>
		public bool IsCool
		{
			get
			{
				var hsl = ToColorSpace<Hsl>();
				return hsl.H >= 180.0 && hsl.H < 360.0;
			}
		}

		/// <summary>
		/// Returns true if the current color is fully saturated.
		/// </summary>
		public bool IsPureHue
		{
			get
			{
				var hsl = ToColorSpace<Hsl>();
				return hsl.S.ApproximatelyEquals(1.0) && hsl.L.ApproximatelyEquals(0.5);
			}
		}
		#endregion

		#region Indexers
		/// <summary>
		/// Provides direct access to a ColorSpace property.
		/// </summary>
		/// <param name="index">
		/// The index of the property to access. The index is equivalent to the
		/// position of that property in the name of the ColorSpace.
		/// For example, for the RGB ColorSpace, the "G" value would have an index of 1.
		/// </param>
		/// <returns>The value of the property at the specified index.</returns>
		public abstract double this[int index] { get; set; }
		#endregion

		#region Constructors
		static ColorSpace()
		{
			Rand = new Random();
		}
		#endregion

		#region Conversion Methods
		protected abstract void FromRgb(Rgb color);
		protected abstract Rgb ToRgb();

		/// <summary>
		/// Converts the current ColorSpace instance to another ColorSpace
		/// </summary>
		/// <typeparam name="T">The destination ColorSpace</typeparam>
		/// <returns>A new ColorSpace instance converted to the specified type</returns>
		public T ToColorSpace<T>() where T : ColorSpace, new()
		{
			if (typeof(T) == GetType())
				return (T)MemberwiseClone();
			var newColorSpace = new T();
			newColorSpace.FromRgb(ToRgb());
			return newColorSpace;
		}
		#endregion

		#region Creation Methods
		public static ColorSpace Parse(string str)
		{
			if (String.IsNullOrEmpty(str))
				return null;

			bool   ignoreScalars;
			string friendlyName;
			var    values = new List<double>();
			if (str[0] == '@')
			{
				var strs = str.Split(':');
				friendlyName = strs[0].TrimStart('@');
				for (var i = 1; i < strs.Length; i++)
				{
					values.Add(Double.Parse(strs[i]));
				}

				ignoreScalars = true;
			}
			else
			{
				friendlyName = Regex.Match(str, @"^([\S]{1,})").Value;
				var parameterList = Regex.Matches(str, @"[(+)?(\-)?\.0-9]+");

				foreach (var parameter in parameterList)
				{
					double value;
					if (Double.TryParse(((Match)parameter).Value, out value))
					{
						values.Add(value);
					}
				}

				ignoreScalars = false;
			}

			if (friendlyName == Cmy.FriendlyName && values.Count == Cmy.PropertyCount)
				return new Cmy(
					ignoreScalars ? values[0] : values[0] / 100.0,
					ignoreScalars ? values[1] : values[1] / 100.0,
					ignoreScalars ? values[2] : values[2] / 100.0);
			if (friendlyName == Cmyk.FriendlyName && values.Count == Cmyk.PropertyCount)
				return new Cmyk(
					ignoreScalars ? values[0] : values[0] / 100.0,
					ignoreScalars ? values[1] : values[1] / 100.0,
					ignoreScalars ? values[2] : values[2] / 100.0,
					ignoreScalars ? values[3] : values[3] / 100.0);
			if (friendlyName == Hsl.FriendlyName && values.Count == Hsl.PropertyCount)
				return new Hsl(
					values[0],
					ignoreScalars ? values[1] : values[1] / 100.0,
					ignoreScalars ? values[2] : values[2] / 100.0);
			if (friendlyName == Hsv.FriendlyName && values.Count == Hsv.PropertyCount)
				return new Hsv(
					values[0],
					ignoreScalars ? values[1] : values[1] / 100.0,
					ignoreScalars ? values[2] : values[2] / 100.0);
			if (friendlyName == HunterLab.FriendlyName && values.Count == HunterLab.PropertyCount)
				return new HunterLab(values[0], values[1], values[2]);
			if (friendlyName == Lab.FriendlyName && values.Count == Lab.PropertyCount)
				return new Lab(values[0], values[1], values[2]);
			if (friendlyName == Lch.FriendlyName && values.Count == Lch.PropertyCount)
				return new Lch(values[0], values[1], values[2]);
			if (friendlyName == Luv.FriendlyName && values.Count == Luv.PropertyCount)
				return new Luv(values[0], values[1], values[2]);
			if (friendlyName == Rgb.FriendlyName && values.Count == Rgb.PropertyCount)
				return new Rgb(values[0], values[1], values[2]);
			if (friendlyName == Rgba.FriendlyName && values.Count == Rgba.PropertyCount)
				return new Rgba(values[0], values[1], values[2], values[3]);
			if (friendlyName == Xyz.FriendlyName && values.Count == Xyz.PropertyCount)
				return new Xyz(values[0], values[1], values[2]);
			if (friendlyName == Yxy.FriendlyName && values.Count == Yxy.PropertyCount)
				return new Yxy(values[0], values[1], values[2]);
			return null;
		}

		public static ColorSpace FromXElement(XElement element)
		{
			if (element.Name != "ColorSpace")
				throw new XmlException("XElement name must be \"ColorSpace\"");

			switch (element.Attribute("Type").Value)
			{
				case "CMY":
				{
					var result = new Cmy();
					result.FromXNode(element);
					return result;
				}
				case "CMYK":
				{
					var result = new Cmyk();
					result.FromXNode(element);
					return result;
				}
				case "HSL":
				{
					var result = new Hsl();
					result.FromXNode(element);
					return result;
				}
				case "HSV":
				{
					var result = new Hsv();
					result.FromXNode(element);
					return result;
				}
				case "HunterLAB":
				{
					var result = new HunterLab();
					result.FromXNode(element);
					return result;
				}
				case "LAB":
				{
					var result = new Lab();
					result.FromXNode(element);
					return result;
				}
				case "LCH":
				{
					var result = new Lch();
					result.FromXNode(element);
					return result;
				}
				case "LUV":
				{
					var result = new Luv();
					result.FromXNode(element);
					return result;
				}
				case "RGB":
				{
					var result = new Rgb();
					result.FromXNode(element);
					return result;
				}
				case "RGBA":
				{
					var result = new Rgba();
					result.FromXNode(element);
					return result;
				}
				case "XYZ":
				{
					var result = new Xyz();
					result.FromXNode(element);
					return result;
				}
				case "YXY":
				{
					var result = new Yxy();
					result.FromXNode(element);
					return result;
				}
				default:
					throw new ArgumentException("Unrecognized ColorSpace type");
			}
		}

		public static T Random<T>(params DiscreteRange<double>[] values) where T : ColorSpace, new()
		{
			if (values == null)
				throw new ArgumentNullException(nameof(values), "The property list cannot be null");

			if (values.Length < 3 || values.Length > 4)
				throw new ArgumentException($"No ColorSpace types exist that have {values.Length} properties");

			if (values.Length == 4 && typeof(T) != typeof(Cmyk) && typeof(T) != typeof(Rgba))
				throw new ArgumentException("Only Cmyk has 4 color properties");

			var properties = new decimal[values.Length];
			for (var i = 0; i < values.Length; i++)
			{
				var range = new DiscreteRange<decimal>(
					(decimal)values[i].Minimum,
					(decimal)values[i].Maximum,
					(decimal)values[i].Increment);

				if (range.Minimum > range.Maximum)
					throw new ArgumentOutOfRangeException(
						$"Property #{i + 1}",
						"Specified minimum cannot be greater than specified maximum");

				if (range.Increment < 0)
					throw new ArgumentOutOfRangeException($"Property #{i + 1}", "Given increment cannot be negative");

				if (range.Minimum != range.Maximum && range.Increment == 0)
					throw new ArgumentException(
						"The specified increment cannot be zero for ranges where minimum ≠ maximum",
						$"Property #{i + 1}");

				properties[i] = range.Minimum == range.Maximum
					? range.Minimum
					: range.Minimum + Rand.Next((int)((range.Maximum - range.Minimum) / range.Increment) + 1) * range.Increment;
			}

			var result = new T
			{
				[0] = Decimal.ToDouble(properties[0]),
				[1] = Decimal.ToDouble(properties[1]),
				[2] = Decimal.ToDouble(properties[2])
			};
			if (result is Cmyk)
				result[3] = Decimal.ToDouble(properties[3]);
			else if (result is Rgba)
				result[3] = Decimal.ToDouble(properties[3]);
			return result;
		}
		#endregion

		#region Blending Methods
		#endregion

		#region Color Theory Methods
		/// <summary>
		/// Gets basic color chords based on the current (dominant) color.
		/// Useful for creating color schemes.
		/// </summary>
		/// <param name="harmony">The type of color chord to create.</param>
		/// <param name="includeThisColor">
		/// If true, the current color will be included in the resulting ColorSpace array.
		/// </param>
		/// <returns>An array of ColorSpace objects that make up the specified color chord.</returns>
		public ColorSpace[] GetHarmony(ColorHarmony harmony, bool includeThisColor = false)
		{
			var          resultSizeOffset = includeThisColor ? 1 : 0;
			ColorSpace[] result;

			switch (harmony)
			{
				case ColorHarmony.Complementary:
				{
					var i = 0;
					result = new ColorSpace[1 + resultSizeOffset];
					var hsl = ToColorSpace<Hsl>();
					hsl.H = AddDegrees(hsl.H, 180);
					if (includeThisColor)
						result[i++] = this;
					result[i] = hsl;
					break;
				}
				case ColorHarmony.SplitComplementary:
				{
					var i = 0;
					result = new ColorSpace[2 + resultSizeOffset];
					var hsl1 = ToColorSpace<Hsl>();
					var hsl2 = ToColorSpace<Hsl>();
					hsl1.H = AddDegrees(hsl1.H, 150);
					hsl2.H = SubtractDegrees(hsl2.H, 150);
					if (includeThisColor)
						result[i++] = this;
					result[i++] = hsl1;
					result[i]   = hsl2;
					break;
				}
				case ColorHarmony.Analogous:
				{
					var i = 0;
					result = new ColorSpace[2 + resultSizeOffset];
					var hsl1 = ToColorSpace<Hsl>();
					var hsl2 = ToColorSpace<Hsl>();
					hsl1.H = AddDegrees(hsl1.H, 30);
					hsl2.H = SubtractDegrees(hsl2.H, 30);
					if (includeThisColor)
						result[i++] = this;
					result[i++] = hsl1;
					result[i]   = hsl2;
					break;
				}
				case ColorHarmony.Triadic:
				{
					var i = 0;
					result = new ColorSpace[2 + resultSizeOffset];
					var hsl1 = ToColorSpace<Hsl>();
					var hsl2 = ToColorSpace<Hsl>();
					hsl1.H = AddDegrees(hsl1.H, 120);
					hsl2.H = SubtractDegrees(hsl2.H, 120);
					if (includeThisColor)
						result[i++] = this;
					result[i++] = hsl1;
					result[i]   = hsl2;
					break;
				}
				case ColorHarmony.Tetradic:
				{
					var i = 0;
					result = new ColorSpace[3 + resultSizeOffset];
					var hsl1 = ToColorSpace<Hsl>();
					hsl1.H = AddDegrees(hsl1.H, 30);
					var hsl2And3 = GetHarmony(ColorHarmony.SplitComplementary);
					if (includeThisColor)
						result[i++] = this;
					result[i++] = hsl1;
					result[i++] = hsl2And3[0];
					result[i]   = hsl2And3[1];
					break;
				}
				case ColorHarmony.Square:
				{
					var i = 0;
					result = new ColorSpace[3 + resultSizeOffset];
					var hsl1 = ToColorSpace<Hsl>();
					var hsl2 = ToColorSpace<Hsl>();
					var hsl3 = ToColorSpace<Hsl>();
					hsl1.H = AddDegrees(hsl1.H, 90);
					hsl2.H = AddDegrees(hsl2.H, 180);
					hsl3.H = AddDegrees(hsl3.H, 270);
					if (includeThisColor)
						result[i++] = this;
					result[i++] = hsl1;
					result[i++] = hsl2;
					result[i]   = hsl3;
					break;
				}
				default:
					throw new ArgumentException("Unrecognized color harmony", nameof(harmony));
			}

			return result;
		}

		/// <summary>
		/// Gets a specified number of incrementally tinted versions of the current color, toward pure white.
		/// </summary>
		/// <param name="numberOfTints">
		/// The number of tints to return.
		/// The higher the number of tints, the more subtle the tinting will be between successive tints.
		/// </param>
		/// <param name="includeThisColor">
		/// If true, the current color will be included in the resulting ColorSpace array.
		/// </param>
		/// <returns>An array of ColorSpace objects that are tints of the current color.</returns>
		public ColorSpace[] GetTints(int numberOfTints, bool includeThisColor = false)
		{
			var resultSizeOffset = includeThisColor ? 1 : 0;
			var result           = new ColorSpace[numberOfTints + resultSizeOffset];
			var hsv              = ToColorSpace<Hsv>();
			var increment        = hsv.S / (numberOfTints + 1);
			var s                = includeThisColor ? hsv.S : hsv.S - increment;
			for (var i = 0; i < numberOfTints + resultSizeOffset; i++)
			{
				result[i] =  new Hsv(hsv.H, s, hsv.V);
				s         -= increment;
			}

			return result;
		}

		/// <summary>
		/// Gets a specified number of incrementally shaded versions of the current color, toward pure black.
		/// </summary>
		/// <param name="numberOfShades">
		/// The number of shades to return.
		/// The higher the number of tints, the more subtle the shading will be between successive shades.
		/// </param>
		/// <param name="includeThisColor">
		/// If true, the current color will be included in the resulting ColorSpace array.
		/// </param>
		/// <returns>An array of ColorSpace objects that are shades of the current color.</returns>
		public ColorSpace[] GetShades(int numberOfShades, bool includeThisColor = false)
		{
			var resultSizeOffset = includeThisColor ? 1 : 0;
			var result           = new ColorSpace[numberOfShades + resultSizeOffset];
			var hsv              = ToColorSpace<Hsv>();
			var increment        = hsv.V / (numberOfShades + 1);
			var v                = includeThisColor ? hsv.V : hsv.V - increment;
			for (var i = 0; i < numberOfShades + resultSizeOffset; i++)
			{
				result[i] =  new Hsv(hsv.H, hsv.S, v);
				v         -= increment;
			}

			return result;
		}

		/// <summary>
		/// Gets a specified number of incrementally toned versions of the current color, toward gray.
		/// </summary>
		/// <param name="numberOfTones">
		/// The number of tones to return.
		/// The higher the number of tints, the more subtle the toning will be between successive tones.
		/// </param>
		/// <param name="includeThisColor">
		/// If true, the current color will be included in the resulting ColorSpace array.
		/// </param>
		/// <returns>An array of ColorSpace objects that are tones of the current color.</returns>
		public ColorSpace[] GetTones(int numberOfTones, bool includeThisColor = false)
		{
			var resultSizeOffset = includeThisColor ? 1 : 0;
			var result           = new ColorSpace[numberOfTones + resultSizeOffset];
			var hsl              = ToColorSpace<Hsl>();
			var increment        = hsl.S / (numberOfTones + 1);
			var s                = includeThisColor ? hsl.S : hsl.S - increment;
			for (var i = 0; i < numberOfTones + resultSizeOffset; i++)
			{
				result[i] =  new Hsl(hsl.H, s, hsl.L);
				s         -= increment;
			}

			return result;
		}

		/// <summary>
		/// Gets a color that is the same hue and saturation with a different lightness.
		/// The lightness is determined in order to provide maximum contrast within the current hue.
		/// </summary>
		/// <returns>A color with a contrasting lightness, but same hue and saturation</returns>
		public ColorSpace GetAutoDarkenOrLighten()
		{
			var hsl = ToColorSpace<Hsl>();
			if (hsl.L >= 0.5)
				hsl.L /= 2.0;
			else
				hsl.L += (1 - hsl.L) / 2.0;
			return hsl;
		}

		public static ColorSpace Add(params ColorSpace[] colors)
		{
			if (colors == null)
				throw new ArgumentNullException(nameof(colors));
			if (colors.Length < 2)
				throw new ArgumentException("Must specify at least two colors", nameof(colors));

			int r = 0, g = 0, b = 0, a = 0;
			foreach (var color in colors.Select(color => color.ToColorSpace<Rgba>()))
			{
				if (r + color.R <= 255)
					r += (int)color.R;
				if (g + color.G <= 255)
					g += (int)color.G;
				if (b + color.B <= 255)
					b += (int)color.B;
				if (a + color.A <= 255)
					a += (int)color.A;
			}

			return new Rgba(r, g, b, a);
		}
		#endregion

		#region Comparison Methods
		/// <summary>
		/// Determines if two colors are "close enough" to be perceived as the same color.
		/// </summary>
		/// <param name="other">A color to compare to this instance.</param>
		/// <returns>
		/// true if this color and the comparison color can be perceived as the same.
		/// false if this color and the comparison color appear different to the human eye.
		/// </returns>
		public bool PerceptuallyEquals(ColorSpace other)
		{
			return Compare(this, other) < JustNoticeableDifference;
		}

		/// <summary>
		/// Calculates the ΔE of this color compared to another color, using the specified comparison method.
		/// This value represents the perceptual difference between the two colors.
		/// </summary>
		/// <param name="other">The color to compare to this color</param>
		/// <param name="method">
		/// The algorith to use for the comparison.
		/// There is an efficiency/accuracy trade-off between the different methods.
		/// </param>
		/// <returns>
		/// ΔE value representing the perceived difference between this color and the comparison color.
		/// </returns>
		public double Compare(ColorSpace other, ComparisonMethod method = ComparisonMethod.Cie2000)
		{
			return Compare(this, other, method);
		}

		/// <summary>
		/// Calculates the ΔE of two colors, using the specified comparison method.
		/// This value represents the perceptual difference between the two colors.
		/// </summary>
		/// <param name="color1">The first color</param>
		/// <param name="color2">The second color</param>
		/// <param name="method">
		/// The algorith to use for the comparison.
		/// There is an efficiency/accuracy trade-off between the different methods.</param>
		/// <returns>ΔE value representing the perceived difference between the two colors.</returns>
		public static double Compare(ColorSpace color1, ColorSpace color2, ComparisonMethod method = ComparisonMethod.Cie2000)
		{
			if (color1 == null || color2 == null)
				throw new NullReferenceException("Neither color1 or color2 may be null");

			switch (method)
			{
				case ComparisonMethod.Cie1976:
					return CompareCie1976(color1, color2);
				case ComparisonMethod.Cie1994:
					return CompareCie1994(color1, color2);
				case ComparisonMethod.Cie2000:
					return CompareCie2000(color1, color2);
				case ComparisonMethod.Cmc1984:
					return CompareCmc1984(color1, color2);
				default:
					throw new ArgumentException("Unrecognized comparison method", nameof(method));
			}
		}

		private static double CompareCie1976(ColorSpace color1, ColorSpace color2)
		{
			var lab1 = color1.ToColorSpace<Lab>();
			var lab2 = color2.ToColorSpace<Lab>();
			var diff = (lab1.L - lab2.L) * (lab1.L - lab2.L) + (lab1.A - lab2.A) * (lab1.A - lab2.A) +
				(lab1.B - lab2.B) * (lab1.B - lab2.B);
			return System.Math.Sqrt(diff);
		}

		private static double CompareCie1994(ColorSpace color1, ColorSpace color2)
		{
			const double kl   = 1.0;
			const double k1   = 0.045;
			const double k2   = 0.015;
			const double sl   = 1.0;
			const double kc   = 1.0;
			const double kh   = 1.0;
			var          lab1 = color1.ToColorSpace<Lab>();
			var          lab2 = color2.ToColorSpace<Lab>();

			var deltaL = lab1.L - lab2.L;
			var deltaA = lab1.A - lab2.A;
			var deltaB = lab1.B - lab2.B;

			var c1     = System.Math.Sqrt(lab1.A * lab1.A + lab1.B * lab1.B);
			var c2     = System.Math.Sqrt(lab2.A * lab2.A + lab2.B * lab2.B);
			var deltaC = c1 - c2;

			var deltaH = deltaA * deltaA + deltaB * deltaB - deltaC * deltaC;
			deltaH = deltaH < 0 ? 0 : System.Math.Sqrt(deltaH);

			var sc = 1.0 + k1 * c1;
			var sh = 1.0 + k2 * c1;

			var deltaL1 = deltaL / (kl * sl);
			var deltaC1 = deltaC / (kc * sc);
			var deltaH1 = deltaH / (kh * sh);
			var i       = deltaL1 * deltaL1 + deltaC1 * deltaC1 + deltaH1 * deltaH1;
			return i < 0 ? 0 : System.Math.Sqrt(i);
		}

		private static double CompareCie2000(ColorSpace color1, ColorSpace color2)
		{
			const double kl = 1.0;
			const double kc = 1.0;
			const double kh = 1.0;

			var lab1 = color1.ToColorSpace<Lab>();
			var lab2 = color2.ToColorSpace<Lab>();

			var lBar         = (lab1.L + lab2.L) / 2.0;
			var c1           = System.Math.Sqrt(lab1.A * lab1.A + lab1.B * lab1.B);
			var c2           = System.Math.Sqrt(lab2.A * lab2.A + lab2.B * lab2.B);
			var cBar         = (c1 + c2) / 2.0;
			var cBarInPower7 = cBar * cBar * cBar;
			cBarInPower7 *= cBarInPower7 * cBar;
			var g         = 1 - System.Math.Sqrt(cBarInPower7 / (cBarInPower7 + 6103515625));
			var aPrime1   = lab1.A + lab1.A / 2.0 * g;
			var aPrime2   = lab2.A + lab2.A / 2.0 * g;
			var cPrime1   = System.Math.Sqrt(aPrime1 * aPrime1 + lab1.B * lab1.B);
			var cPrime2   = System.Math.Sqrt(aPrime2 * aPrime2 + lab2.B * lab2.B);
			var cBarPrime = (cPrime1 + cPrime2) / 2.0;
			var hPrime1   = System.Math.Atan2(lab1.B, aPrime1) % 360;
			var hPrime2   = System.Math.Atan2(lab2.B, aPrime2) % 360;
			var hBar      = System.Math.Abs(hPrime1 - hPrime2);

			double deltaHPrime;
			if (hBar <= 180)
			{
				deltaHPrime = hPrime2 - hPrime1;
			}
			else if (hBar > 180 && hPrime2 <= hPrime1)
			{
				deltaHPrime = hPrime2 - hPrime1 + 360.0;
			}
			else
			{
				deltaHPrime = hPrime2 - hPrime1 - 360.0;
			}

			var deltaLPrime = lab2.L - lab1.L;
			var deltaCPrime = cPrime2 - cPrime1;
			deltaHPrime = 2 * System.Math.Sqrt(cPrime1 * cPrime2) * System.Math.Sin(deltaHPrime / 2.0);
			var hBarPrime = hBar > 180 ? (hPrime1 + hPrime2 + 360) / 2.0 : (hPrime1 + hPrime2) / 2.0;

			var t = 1 - .17 * System.Math.Cos(hBarPrime - 30) + .24 * System.Math.Cos(2 * hBarPrime) +
				.32 * System.Math.Cos(3 * hBarPrime + 6) - .2 * System.Math.Cos(4 * hBarPrime - 63);

			var lBarMinus50Sqr = (lBar - 50) * (lBar - 50);
			var sl             = 1 + .015 * lBarMinus50Sqr / System.Math.Sqrt(20 + lBarMinus50Sqr);
			var sc             = 1 + .045 * cBarPrime;
			var sh             = 1 + .015 * cBarPrime * t;

			var cBarPrimeInPower7 = cBarPrime * cBarPrime * cBarPrime;
			cBarPrimeInPower7 *= cBarPrimeInPower7 * cBarPrime;
			var rt = -2 * System.Math.Sqrt(cBarPrimeInPower7 / (cBarPrimeInPower7 + 6103515625)) // 25 ^ 7
				* System.Math.Sin(60.0 * System.Math.Exp(-((hBarPrime - 275.0) / 25.0)));

			var deltaLPrimeDivklsl = deltaLPrime / (kl * sl);
			var deltaCPrimeDivkcsc = deltaCPrime / (kc * sc);
			var deltaHPrimeDivkhsh = deltaHPrime / (kh * sh);
			var deltaE = System.Math.Sqrt(
				deltaLPrimeDivklsl * deltaLPrimeDivklsl + deltaCPrimeDivkcsc * deltaCPrimeDivkcsc +
				deltaHPrimeDivkhsh * deltaHPrimeDivkhsh + rt * (deltaCPrime / (kc * kh)) * (deltaHPrime / (kh * sh)));

			return deltaE;
		}

		private static double CompareCmc1984(ColorSpace color1, ColorSpace color2)
		{
			const double defaultLightness = 2.0;
			const double defaultChroma    = 1.0;

			var lab1 = color1.ToColorSpace<Lab>();
			var lab2 = color2.ToColorSpace<Lab>();

			var deltaL = lab1.L - lab2.L;
			var h      = System.Math.Atan2(lab1.B, lab1.A);
			var c1     = System.Math.Sqrt(lab1.A * lab1.A + lab1.B * lab1.B);
			var c2     = System.Math.Sqrt(lab2.A * lab2.A + lab2.B * lab2.B);
			var c1A    = System.Math.Pow(c1, 4);
			var deltaC = c1 - c2;
			var deltaH = System.Math.Sqrt(
				(lab1.A - lab2.A) * (lab1.A - lab2.A) + (lab1.B - lab2.B) * (lab1.B - lab2.B) - deltaC * deltaC);
			var t = 164 <= h || h >= 345
				? 0.56 + System.Math.Abs(0.2 * System.Math.Cos(h + 168.0))
				: 0.36 + System.Math.Abs(0.4 * System.Math.Cos(h + 35.0));
			var f  = System.Math.Sqrt(c1A / (c1A + 1900.0));
			var sL = lab1.L < 16 ? 0.511 : 0.040975 * lab1.L / (1.0 + 0.01765 * lab1.L);
			var sC = 0.0638 * c1 / (1.0 + 0.0131 * c1) + 0.638;
			var sH = sC * (f * t + 1.0 - f);

			var diff = System.Math.Pow(deltaL / (defaultLightness * sL), 2) + System.Math.Pow(deltaC / (defaultChroma * sC), 2) +
				System.Math.Pow(deltaH / sH,                             2);
			return System.Math.Sqrt(diff);
		}
		#endregion

		#region Utility Methods
		/// <summary>
		/// Gets the "Friendly Name" of the current <see cref="ColorSpace"/> instance
		/// </summary>
		/// <param name="color">The <see cref="ColorSpace"/> object</param>
		/// <returns>Friendly name string</returns>
		public static string GetFriendlyName(ColorSpace color)
		{
			return color.GetType().GetField("FriendlyName").GetValue(color) as string;
		}

		/// <summary>
		/// Gets the number of color properties for the current <see cref="ColorSpace"/> instance
		/// </summary>
		/// <param name="color">The <see cref="ColorSpace"/> object</param>
		/// <returns>The number of color properties for the current instance</returns>
		public static int GetPropertyCount(ColorSpace color)
		{
			return (int)color.GetType().GetField("PropertyCount").GetValue(color);
		}

		private static Rgb NormalizeRgb(ColorSpace colorSpace)
		{
			var rgb = colorSpace.ToRgb();
			if (rgb.R < 0 || rgb.R.ApproximatelyEquals(0,          0.001)) rgb.R = 0;
			else if (rgb.R > 255 || rgb.R.ApproximatelyEquals(255, 0.001)) rgb.R = 255;
			else rgb.R                                                           = System.Math.Round(rgb.R);
			if (rgb.G < 0 || rgb.G.ApproximatelyEquals(0,          0.001)) rgb.G = 0;
			else if (rgb.G > 255 || rgb.G.ApproximatelyEquals(255, 0.001)) rgb.G = 255;
			else rgb.G                                                           = System.Math.Round(rgb.G);
			if (rgb.B < 0 || rgb.B.ApproximatelyEquals(0,          0.001)) rgb.B = 0;
			else if (rgb.B > 255 || rgb.B.ApproximatelyEquals(255, 0.001)) rgb.B = 255;
			else rgb.B                                                           = System.Math.Round(rgb.B);

			var rgba = rgb as Rgba;
			if (rgba != null)
			{
				if (rgba.A < 0 || rgba.A.ApproximatelyEquals(0,          0.001)) rgba.A = 0;
				else if (rgba.A > 255 || rgba.A.ApproximatelyEquals(255, 0.001)) rgba.A = 255;
				else rgba.A                                                             = System.Math.Round(rgba.A);
			}

			return rgb;
		}

		private static double AddDegrees(double value, double degreesToAdd)
		{
			if (value + degreesToAdd >= 360)
				return System.Math.Abs(value + degreesToAdd) % 360.0;
			return value + degreesToAdd;
		}

		private static double SubtractDegrees(double value, double degreesToSubtract)
		{
			if (value - degreesToSubtract < 0)
				return System.Math.Abs(value - degreesToSubtract) % 360;
			return value - degreesToSubtract;
		}

		protected static void ValidateRange(double value, DiscreteRange<double> range, string name)
		{
			if (value < range.Minimum || value > range.Maximum)
				throw new ArgumentOutOfRangeException(
					name,
					String.Format(
						"The specified value ({1}) must be in the range: {0} ≤ {1} ≤ {2}",
						range.Minimum,
						name,
						range.Maximum));
		}
		#endregion

		#region Interface Implementation (IEquatable<T>, IVirtuallyEquatable<T>, IXNode<T>, ICloneable)
		public abstract bool Equals(ColorSpace other);
		public abstract bool VirtuallyEquals(ColorSpace other);
		public abstract void FromXNode(XElement element);
		public abstract XElement ToXNode();

		public object Clone()
		{
			return MemberwiseClone();
		}
		#endregion

		#region Interface Implementation (IFormattable)
		/// <summary>
		/// Returns a customizable string representation of this <see cref="ColorSpace"/> instance.
		/// The returned string can be customized by providing a format string.
		/// </summary>
		/// <param name="formatString">
		/// Everything included in the <see cref="formatString"/> 
		/// will be included in the final return string,
		/// and can be modified using the following escape sequences:
		/// <code>``</code> (The ` character)
		/// <code>`N</code> (<see cref="ColorSpace"/> friendly name)
		/// <code>`#X[X](X)</code> (The 1st X represents the index of the ColorSpace property,
		/// the 2nd X represents the numeric format of the value (must include brackets),
		/// the 3rd X is a scalar by which the property value will be multiplied
		/// - this value is optional and must be specified inside parentheses if used)
		/// If the <see cref="formatString"/> is null or blank,
		/// the returned string will be a coded representation
		/// of the <see cref="ColorSpace"/> data (useful for serialization).
		/// </param>
		/// <param name="formatProvider">
		/// The <see cref="IFormatProvider"/> value used to format the string.
		/// If null, <see cref="CultureInfo.InvariantCulture"/> is assumed.
		/// </param>
		/// <returns>A customized string representation of this <see cref="ColorSpace"/> instance</returns>
		public string ToString(string formatString, IFormatProvider formatProvider = null)
		{
			var result = new StringBuilder();
			if (String.IsNullOrEmpty(formatString))
			{
				result.Append('@');
				result.Append(GetFriendlyName(this));
				for (var i = 0; i < GetPropertyCount(this); i++)
				{
					result.Append(':');
					result.Append(this[i].ToString("R"));
				}
			}
			else
			{
				int subStart = 0, i = 0;
				while (i < formatString.Length)
				{
					i = formatString.IndexOf('`', subStart);
					if (i == -1)
					{
						result.Append(formatString.Substring(subStart, formatString.Length - subStart));
						i = formatString.Length;
					}
					else
					{
						if (i > subStart)
							result.Append(formatString.Substring(subStart, i - subStart));
						switch (formatString[i + 1])
						{
							case '`':
								result.Append('`');
								subStart = i + 2;
								break;
							case 'N':
								var fn = GetFriendlyName(this);
								result.Append(fn);
								subStart = i + 2;
								break;
							case '#':
								var propertyIndex = Int32.Parse(formatString[i + 2].ToString(formatProvider ?? CultureInfo.InvariantCulture));
								var numericFormat = formatString.Substring(i + 4, formatString.IndexOf(']', i + 3) - (i + 4));
								var scalar        = 1.0;
								i += 4 + numericFormat.Length;
								if (formatString[i + 1] == '(')
								{
									var scalarStr = formatString.Substring(i + 2, formatString.IndexOf(')', i + 1) - (i + 2));
									scalar   = Double.Parse(scalarStr);
									subStart = i + 3 + scalarStr.Length;
								}
								else
								{
									subStart = i + 1;
								}

								result.Append((this[propertyIndex] * scalar).ToString(numericFormat));
								break;
							default:
								subStart = i + 2;
								break;
						}
					}
				}
			}

			return result.ToString();
		}
		#endregion

		#region Operator Overloads
		public static explicit operator System.Drawing.Color(ColorSpace value)
		{
			var rgb  = NormalizeRgb(value);
			var rgba = rgb as Rgba;
			return rgba != null
				? System.Drawing.Color.FromArgb((int)rgba.A, (int)rgba.R, (int)rgba.G, (int)rgba.B)
				: System.Drawing.Color.FromArgb((int)rgb.R,  (int)rgb.G,  (int)rgb.B);
		}

		public static explicit operator ColorSpace(System.Drawing.Color value)
		{
			return new Rgba(value.R, value.G, value.B, value.A);
		}
		#endregion
	}
}