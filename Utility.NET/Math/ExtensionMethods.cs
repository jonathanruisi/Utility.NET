// ┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  FILE:       ExtensionMethods.cs
// ┃  PROJECT:    Utility.NET
// ┃  SOLUTION:   Utility.NET
// ┃  CREATED:    2015-12-23 @ 4:57 AM
// ┣━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  AUTHOR:     Jonathan Ruisi
// ┃  EMAIL:      JonathanRuisi@gmail.com
// ┣━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  GIT REPO:   https://github.com/jonathanruisi/Utility.NET
// ┃  LICENSE:    https://github.com/jonathanruisi/Utility.NET/blob/master/LICENSE
// ┗━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

using System;

namespace JLR.Utility.NET.Math
{
	public static class ExtensionMethods
	{
		/// <summary>
		/// Throws an exception if the current value does not fall within the specified range
		/// </summary>
		/// <typeparam name="T">Any type that implements <see cref="IComparable"/></typeparam>
		/// <param name="value">The value to be tested</param>
		/// <param name="minValue">Value indicating the inclusive lower-bound of the valid range</param>
		/// <param name="maxValue">Value indicating the inclusive upper-bound of the valid range</param>
		public static void ValidateRange<T>(this T value, T minValue, T maxValue) where T : IComparable<T>
		{
			if (minValue.CompareTo(maxValue) > 1)
				throw new ArgumentException("Minimum value must be less than or equal to maximum value");

			if (value.CompareTo(minValue) < 0 || value.CompareTo(maxValue) > 0)
				throw new ArgumentOutOfRangeException($"{value} must be between {minValue} and {maxValue} (inclusive)");
		}

		#region ApproximatelyEquals
		/// <summary>
		/// Returns a value indicating whether this value
		/// is approximately equal to another System.Single value.
		/// Any inaccuracies resulting from precision loss are ignored beyond a specified threshold.
		/// </summary>
		/// <param name="value1">This System.Single value.</param>
		/// <param name="value2">A System.Single value to compare to this value.</param>
		/// <param name="epsilon">
		/// Represents the largest possible difference between two System.Single
		/// values for which they are considered to be equal.
		/// </param>
		/// <returns>
		/// Returns a value indicating whether this value
		/// is approximately equal to another System.Single value.
		/// </returns>
		public static bool ApproximatelyEquals(this float value1, float value2, float epsilon = 0.00000001f)
		{
			// Check for equality
			if (value1.Equals(value2))
				return true;

			// Handle Infinity and NaN
			if (float.IsInfinity(value1) | float.IsNaN(value1))
				return value1.Equals(value2);
			if (float.IsInfinity(value2) | float.IsNaN(value2))
				return value1.Equals(value2);

			// Avoid division by zero
			var divisor = System.Math.Max(value1, value2);
			if (divisor.Equals(0))
				divisor = System.Math.Min(value1, value2);

			return System.Math.Abs(value1 - value2) / divisor <= epsilon;
		}

		/// <summary>
		/// Returns a value indicating whether this value
		/// is approximately equal to another System.Double value.
		/// Any inaccuracies resulting from precision loss are ignored beyond a specified threshold.
		/// </summary>
		/// <param name="value1">This System.Double value.</param>
		/// <param name="value2">A System.Double value to compare to this value.</param>
		/// <param name="epsilon">
		/// Represents the largest possible difference between two System.Double
		/// values for which they are considered to be equal.
		/// </param>
		/// <returns>
		/// Returns a value indicating whether this value
		/// is approximately equal to another System.Double value.
		/// </returns>
		public static bool ApproximatelyEquals(this double value1, double value2, double epsilon = 0.00000001)
		{
			// Check for equality
			if (value1.Equals(value2))
				return true;

			// Handle Infinity and NaN
			if (double.IsInfinity(value1) | double.IsNaN(value1))
				return value1.Equals(value2);
			if (double.IsInfinity(value2) | double.IsNaN(value2))
				return value1.Equals(value2);

			// Avoid division by zero
			var divisor = System.Math.Max(value1, value2);
			if (divisor.Equals(0))
				divisor = System.Math.Min(value1, value2);

			return System.Math.Abs(value1 - value2) / divisor <= epsilon;
		}
		#endregion

		#region IsPowerOfTwo
		public static bool IsPowerOfTwo(this sbyte value)
		{
			return (value > 0) && ((value & (value - 1)) == 0);
		}

		public static bool IsPowerOfTwo(this byte value)
		{
			return (value > 0) && ((value & (value - 1)) == 0);
		}

		public static bool IsPowerOfTwo(this short value)
		{
			return (value > 0) && ((value & (value - 1)) == 0);
		}

		public static bool IsPowerOfTwo(this ushort value)
		{
			return (value > 0) && ((value & (value - 1)) == 0);
		}

		public static bool IsPowerOfTwo(this int value)
		{
			return (value > 0) && ((value & (value - 1)) == 0);
		}

		public static bool IsPowerOfTwo(this uint value)
		{
			return (value > 0) && ((value & (value - 1)) == 0);
		}

		public static bool IsPowerOfTwo(this IntPtr value)
		{
			return value.ToInt64().IsPowerOfTwo();
		}

		public static bool IsPowerOfTwo(this UIntPtr value)
		{
			return value.ToUInt64().IsPowerOfTwo();
		}

		public static bool IsPowerOfTwo(this long value)
		{
			return (value > 0) && ((value & (value - 1)) == 0);
		}

		public static bool IsPowerOfTwo(this ulong value)
		{
			return (value > 0) && ((value & (value - 1)) == 0);
		}
		#endregion
	}
}