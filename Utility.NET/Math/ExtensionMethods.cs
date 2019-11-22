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
		[Obsolete]
		public static void ValidateRange<T>(this T value, T minValue, T maxValue) where T : IComparable<T>
		{
			if (minValue.CompareTo(maxValue) > 1)
				throw new ArgumentException("Minimum value must be less than or equal to maximum value");

			if (value.CompareTo(minValue) < 0 || value.CompareTo(maxValue) > 0)
				throw new ArgumentOutOfRangeException($"{value} must be between {minValue} and {maxValue} (inclusive)");
		}

		#region Equality
		/// <summary>
		/// Returns a value indicating whether or not this instance and a specified
		/// <see cref="T:System.Single"></see> object represent the same value.
		/// </summary>
		/// <param name="lhs">This instance</param>
		/// <param name="rhs">Value to compare to this instance</param>
		/// <param name="threshold">
		/// Specifies the value by which equality is determined.
		/// The meaning of this value differs based on the specified <paramref name="method"/>.
		/// </param>
		/// <param name="method">
		/// <see cref="EqualityMethods"/> value which specifies the meaning of <paramref name="threshold"/>.
		/// </param>
		/// <returns>
		/// Returns <c>true</c> if the values are considered equal
		/// despite differences due to precision, false otherwise.
		/// </returns>
		public static bool ReasonablyEquals(this float lhs,
		                                    float rhs,
		                                    int threshold,
		                                    EqualityMethods method = EqualityMethods.Precision)
		{
			if (float.IsNaN(lhs) || float.IsNaN(rhs))
				return false;

			if (float.IsInfinity(lhs) && float.IsInfinity(rhs))
				return lhs.Equals(rhs);

			switch (method)
			{
				case EqualityMethods.Precision:
				{
					var intLhs = BitConverter.ToUInt32(BitConverter.GetBytes(lhs), 0);
					var intRhs = BitConverter.ToUInt32(BitConverter.GetBytes(rhs), 0);

					// If the signs are different, return false except in the case of +/-0
					if (intLhs >> 31 != intRhs >> 31)
						return intLhs == intRhs;

					var diff = System.Math.Abs(intLhs - intRhs);
					return diff <= threshold;
				}

				case EqualityMethods.FractionalSignificance:
				{
					var divisor = System.Math.Max(lhs, rhs);
					if (divisor.Equals(0))
						divisor = System.Math.Min(lhs, rhs);

					return System.Math.Abs(lhs - rhs) / divisor <= System.Math.Pow(10.0, -threshold);
				}

				default:
					throw new ArgumentException(nameof(method));
			}
		}

		/// <summary>
		/// Returns a value indicating whether or not this instance and a specified
		/// <see cref="T:System.Double"></see> object represent the same value.
		/// </summary>
		/// <param name="lhs">This instance</param>
		/// <param name="rhs">Value to compare to this instance</param>
		/// <param name="threshold">
		/// Specifies the value by which equality is determined.
		/// The meaning of this value differs based on the specified <paramref name="method"/>.
		/// </param>
		/// <param name="method">
		/// <see cref="EqualityMethods"/> value which specifies the meaning of <paramref name="threshold"/>.
		/// </param>
		/// <returns>
		/// Returns <c>true</c> if the values are considered equal
		/// despite differences due to precision, false otherwise.
		/// </returns>
		public static bool ReasonablyEquals(this double lhs,
		                                    double rhs,
		                                    int threshold,
		                                    EqualityMethods method = EqualityMethods.Precision)
		{
			if (double.IsNaN(lhs) || double.IsNaN(rhs))
				return false;

			if (double.IsInfinity(lhs) && double.IsInfinity(rhs))
				return lhs.Equals(rhs);

			switch (method)
			{
				case EqualityMethods.Precision:
				{
					var intLhs = BitConverter.DoubleToInt64Bits(lhs);
					var intRhs = BitConverter.DoubleToInt64Bits(rhs);

					// If the signs are different, return false except in the case of +/-0
					if (intLhs >> 63 != intRhs >> 63)
						return intLhs == intRhs;

					var diff = System.Math.Abs(intLhs - intRhs);
					return diff <= threshold;
				}

				case EqualityMethods.FractionalSignificance:
				{
					var divisor = System.Math.Max(lhs, rhs);
					if (divisor.Equals(0))
						divisor = System.Math.Min(lhs, rhs);

					return System.Math.Abs(lhs - rhs) / divisor <= System.Math.Pow(10.0, -threshold);
				}

				default:
					throw new ArgumentException(nameof(method));
			}
		}

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
		[Obsolete]
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
		[Obsolete]
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

		#region Comparison
		/// <summary>
		/// Returns a value indicating whether or not this min/max value
		/// is an inclusive subset of a specified min/max.
		/// </summary>
		/// <param name="value">This instance</param>
		/// <param name="other">A tuple representing a minimum and maximum</param>
		/// <returns>
		/// <c>true</c> if this instance is an inclusive subset
		/// of the specified min/max, <c>false</c> otherwise.
		/// </returns>
		public static bool IsInclusiveSubsetOf(this (IComparable, IComparable) value, (IComparable, IComparable) other)
		{
			return value.Item1.CompareTo(value.Item2) < 1 &&
			       value.Item1.CompareTo(other.Item1) >= 0 &&
			       value.Item2.CompareTo(other.Item2) <= 0;
		}

		/// <summary>
		/// Returns a value indicating whether or not this min/max value
		/// is an exclusive subset of a specified min/max.
		/// </summary>
		/// <param name="value">This instance</param>
		/// <param name="other">A tuple representing a minimum and maximum</param>
		/// <returns>
		/// <c>true</c> if this instance is an exclusive subset
		/// of the specified min/max, <c>false</c> otherwise.
		/// </returns>
		public static bool IsExclusiveSubsetOf(this (IComparable, IComparable) value, (IComparable, IComparable) other)
		{
			return value.Item1.CompareTo(value.Item2) < 1 &&
			       value.Item1.CompareTo(other.Item1) > 0 &&
			       value.Item2.CompareTo(other.Item2) < 0;
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