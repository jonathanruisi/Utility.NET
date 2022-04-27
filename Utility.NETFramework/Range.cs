// ┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  FILE:       Range.cs
// ┃  PROJECT:    Utility.NET
// ┃  SOLUTION:   Utility.NET
// ┃  CREATED:    2018-03-27 @ 10:33 PM
// ┣━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  AUTHOR:     Jonathan Ruisi
// ┃  EMAIL:      JonathanRuisi@gmail.com
// ┣━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  GIT REPO:   https://github.com/jonathanruisi/Utility.NET
// ┃  LICENSE:    https://github.com/jonathanruisi/Utility.NET/blob/master/LICENSE
// ┗━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

using System;

namespace JLR.Utility.NET
{
	/// <summary>
	/// Represents a range of values bounded by a <see cref="Minimum"/> and <see cref="Maximum"/> value.
	/// </summary>
	/// <typeparam name="T">
	/// Any value type that can be tested for equality (Must implement <see cref="IEquatable{T}"/>).
	/// </typeparam>
	public struct Range<T> : IEquatable<Range<T>>
		where T : struct, IEquatable<T>
	{
		#region Properties
		/// <summary>Gets the lower bound of this <see cref="Range{T}"/></summary>
		public T Minimum { get; }

		/// <summary>Gets the upper bound of this <see cref="Range{T}"/></summary>
		public T Maximum { get; }
		#endregion

		#region Constructors
		/// <summary>
		/// Initializes a new instance of the <see cref="Range{T}"/> structure.
		/// </summary>
		/// <param name="minimum">The lower bound of the <see cref="Range{T}"/></param>
		/// <param name="maximum">The upper bound of the <see cref="Range{T}"/></param>
		public Range(T minimum, T maximum)
		{
			Minimum = minimum;
			Maximum = maximum;
		}
		#endregion

		#region Interface Implementation (IEquatable<T>)
		/// <inheritdoc />
		public bool Equals(Range<T> other)
		{
			return Minimum.Equals(other.Minimum) && Maximum.Equals(other.Maximum);
		}
		#endregion

		#region Method Overrides (System.ValueType)
		/// <inheritdoc />
		public override bool Equals(object obj)
		{
			if (obj is Range<T> range)
				return Equals(range);
			return false;
		}

		/// <inheritdoc />
		public override int GetHashCode()
		{
			return Minimum.GetHashCode() ^ Maximum.GetHashCode();
		}

		/// <inheritdoc />
		public override string ToString()
		{
			return $"{Minimum} ≤ X ≤ {Maximum}";
		}
		#endregion

		#region Operator Overloads
		public static bool operator ==(Range<T> lhs, Range<T> rhs)
		{
			return lhs.Equals(rhs);
		}

		public static bool operator !=(Range<T> lhs, Range<T> rhs)
		{
			return !lhs.Equals(rhs);
		}
		#endregion
	}

	#region Extension Methods
	public static class RangeExtensionMethods
	{
		/// <summary>
		/// Determines whether or not this <typeparamref name="T"/> is within a specified <see cref="Range{T}"/>.
		/// </summary>
		/// <typeparam name="T">
		/// A value type that implements <see cref="IEquatable{T}"/> and <see cref="IComparable{T}"/>.
		/// </typeparam>
		/// <param name="value">This value</param>
		/// <param name="range">The <see cref="Range{T}"/> to test</param>
		/// <param name="isLowerBoundInclusive">If <code>true</code>, <see cref="Range{T}.Minimum"/> is inclusive.</param>
		/// <param name="isUpperBoundInclusive">If <code>true</code>, <see cref="Range{T}.Maximum"/> is inclusive.</param>
		/// <returns>
		/// <code>true</code> if <paramref name="value"/> is within <paramref name="range"/>, <code>false</code> otherwise.
		/// </returns>
		public static bool IsBetween<T>(this T value,
										Range<T> range,
										bool isLowerBoundInclusive = true,
										bool isUpperBoundInclusive = true)
			where T : struct, IEquatable<T>, IComparable<T>
		{
			var lowerValid = isLowerBoundInclusive && value.CompareTo(range.Minimum) >= 0 ||
				!isLowerBoundInclusive && value.CompareTo(range.Minimum) > 0;
			var upperValid = isUpperBoundInclusive && value.CompareTo(range.Maximum) <= 0 ||
				!isUpperBoundInclusive && value.CompareTo(range.Maximum) < 0;
			return lowerValid && upperValid;
		}

		/// <summary>
		/// Determines whether or not this <see cref="Range{T}"/> contains a specified <typeparamref name="T"/>.
		/// </summary>
		/// <typeparam name="T">
		/// A value type that implements <see cref="IEquatable{T}"/> and <see cref="IComparable{T}"/>.
		/// </typeparam>
		/// <param name="range">This <see cref="Range{T}"/></param>
		/// <param name="value">The <typeparamref name="T"/> to test</param>
		/// <param name="isLowerBoundInclusive">If <code>true</code>, <see cref="Range{T}.Minimum"/> is inclusive.</param>
		/// <param name="isUpperBoundInclusive">If <code>true</code>, <see cref="Range{T}.Maximum"/> is inclusive.</param>
		/// <returns>
		/// <code>true</code> if <paramref name="range"/> contains <paramref name="value"/>, <code>false</code> otherwise.
		/// </returns>
		public static bool Contains<T>(this Range<T> range,
									   T value,
									   bool isLowerBoundInclusive = true,
									   bool isUpperBoundInclusive = true)
			where T : struct, IEquatable<T>, IComparable<T>
		{
			var lowerValid = isLowerBoundInclusive && value.CompareTo(range.Minimum) >= 0 ||
				!isLowerBoundInclusive && value.CompareTo(range.Minimum) > 0;
			var upperValid = isUpperBoundInclusive && value.CompareTo(range.Maximum) <= 0 ||
				!isUpperBoundInclusive && value.CompareTo(range.Maximum) < 0;
			return lowerValid && upperValid;
		}

		/// <summary>
		/// Determines whether or not this <see cref="Range{T}"/> contains a specified <see cref="Range{T}"/>.
		/// </summary>
		/// <typeparam name="T">
		/// A value type that implements <see cref="IEquatable{T}"/> and <see cref="IComparable{T}"/>.
		/// </typeparam>
		/// <param name="range">This <see cref="Range{T}"/></param>
		/// <param name="value">The <see cref="Range{T}"/> to test</param>
		/// <param name="isLowerBoundInclusive">If <code>true</code>, <see cref="Range{T}.Minimum"/> is inclusive.</param>
		/// <param name="isUpperBoundInclusive">If <code>true</code>, <see cref="Range{T}.Maximum"/> is inclusive.</param>
		/// <returns>
		/// <code>true</code> if <paramref name="range"/> contains <paramref name="value"/>, <code>false</code> otherwise.
		/// </returns>
		public static bool Contains<T>(this Range<T> range,
									   Range<T> value,
									   bool isLowerBoundInclusive = true,
									   bool isUpperBoundInclusive = true)
			where T : struct, IEquatable<T>, IComparable<T>
		{
			var lowerValid = isLowerBoundInclusive && value.Minimum.CompareTo(range.Minimum) >= 0 ||
				!isLowerBoundInclusive && value.Minimum.CompareTo(range.Minimum) > 0;
			var upperValid = isUpperBoundInclusive && value.Maximum.CompareTo(range.Maximum) <= 0 ||
				!isUpperBoundInclusive && value.Maximum.CompareTo(range.Maximum) < 0;
			return lowerValid && upperValid;
		}

		/// <summary>
		/// Gets the magnitude (distance between <see cref="Range{T}.Maximum"/>
		/// and <see cref="Range{T}.Minimum"/>) of this <see cref="Range{T}"/>.
		/// </summary>
		/// <param name="range">This <see cref="Range{T}"/></param>
		/// <returns>The magnitude of <paramref name="range"/></returns>
		public static byte Magnitude(this Range<byte> range)
		{
			return range.Maximum > range.Minimum ? (byte)(range.Maximum - range.Minimum) : (byte)(range.Minimum - range.Maximum);
		}

		/// <summary>
		/// Gets the magnitude (distance between <see cref="Range{T}.Maximum"/>
		/// and <see cref="Range{T}.Minimum"/>) of this <see cref="Range{T}"/>.
		/// </summary>
		/// <param name="range">This <see cref="Range{T}"/></param>
		/// <returns>The magnitude of <paramref name="range"/></returns>
		public static ushort Magnitude(this Range<ushort> range)
		{
			return range.Maximum > range.Minimum
				? (ushort)(range.Maximum - range.Minimum)
				: (ushort)(range.Minimum - range.Maximum);
		}

		/// <summary>
		/// Gets the magnitude (distance between <see cref="Range{T}.Maximum"/>
		/// and <see cref="Range{T}.Minimum"/>) of this <see cref="Range{T}"/>.
		/// </summary>
		/// <param name="range">This <see cref="Range{T}"/></param>
		/// <returns>The magnitude of <paramref name="range"/></returns>
		public static uint Magnitude(this Range<uint> range)
		{
			return range.Maximum > range.Minimum ? range.Maximum - range.Minimum : range.Minimum - range.Maximum;
		}

		/// <summary>
		/// Gets the magnitude (distance between <see cref="Range{T}.Maximum"/>
		/// and <see cref="Range{T}.Minimum"/>) of this <see cref="Range{T}"/>.
		/// </summary>
		/// <param name="range">This <see cref="Range{T}"/></param>
		/// <returns>The magnitude of <paramref name="range"/></returns>
		public static ulong Magnitude(this Range<ulong> range)
		{
			return range.Maximum > range.Minimum ? range.Maximum - range.Minimum : range.Minimum - range.Maximum;
		}

		/// <summary>
		/// Gets the magnitude (distance between <see cref="Range{T}.Maximum"/>
		/// and <see cref="Range{T}.Minimum"/>) of this <see cref="Range{T}"/>.
		/// </summary>
		/// <param name="range">This <see cref="Range{T}"/></param>
		/// <returns>The magnitude of <paramref name="range"/></returns>
		public static sbyte Magnitude(this Range<sbyte> range)
		{
			return (sbyte)System.Math.Abs(range.Maximum - range.Minimum);
		}

		/// <summary>
		/// Gets the magnitude (distance between <see cref="Range{T}.Maximum"/>
		/// and <see cref="Range{T}.Minimum"/>) of this <see cref="Range{T}"/>.
		/// </summary>
		/// <param name="range">This <see cref="Range{T}"/></param>
		/// <returns>The magnitude of <paramref name="range"/></returns>
		public static short Magnitude(this Range<short> range)
		{
			return (short)System.Math.Abs(range.Maximum - range.Minimum);
		}

		/// <summary>
		/// Gets the magnitude (distance between <see cref="Range{T}.Maximum"/>
		/// and <see cref="Range{T}.Minimum"/>) of this <see cref="Range{T}"/>.
		/// </summary>
		/// <param name="range">This <see cref="Range{T}"/></param>
		/// <returns>The magnitude of <paramref name="range"/></returns>
		public static int Magnitude(this Range<int> range)
		{
			return System.Math.Abs(range.Maximum - range.Minimum);
		}

		/// <summary>
		/// Gets the magnitude (distance between <see cref="Range{T}.Maximum"/>
		/// and <see cref="Range{T}.Minimum"/>) of this <see cref="Range{T}"/>.
		/// </summary>
		/// <param name="range">This <see cref="Range{T}"/></param>
		/// <returns>The magnitude of <paramref name="range"/></returns>
		public static long Magnitude(this Range<long> range)
		{
			return System.Math.Abs(range.Maximum - range.Minimum);
		}

		/// <summary>
		/// Gets the magnitude (distance between <see cref="Range{T}.Maximum"/>
		/// and <see cref="Range{T}.Minimum"/>) of this <see cref="Range{T}"/>.
		/// </summary>
		/// <param name="range">This <see cref="Range{T}"/></param>
		/// <returns>The magnitude of <paramref name="range"/></returns>
		public static float Magnitude(this Range<float> range)
		{
			return System.Math.Abs(range.Maximum - range.Minimum);
		}

		/// <summary>
		/// Gets the magnitude (distance between <see cref="Range{T}.Maximum"/>
		/// and <see cref="Range{T}.Minimum"/>) of this <see cref="Range{T}"/>.
		/// </summary>
		/// <param name="range">This <see cref="Range{T}"/></param>
		/// <returns>The magnitude of <paramref name="range"/></returns>
		public static double Magnitude(this Range<double> range)
		{
			return System.Math.Abs(range.Maximum - range.Minimum);
		}

		/// <summary>
		/// Gets the magnitude (distance between <see cref="Range{T}.Maximum"/>
		/// and <see cref="Range{T}.Minimum"/>) of this <see cref="Range{T}"/>.
		/// </summary>
		/// <param name="range">This <see cref="Range{T}"/></param>
		/// <returns>The magnitude of <paramref name="range"/></returns>
		public static decimal Magnitude(this Range<decimal> range)
		{
			return System.Math.Abs(range.Maximum - range.Minimum);
		}

		/// <summary>
		/// Gets the magnitude (distance between <see cref="Range{T}.Maximum"/>
		/// and <see cref="Range{T}.Minimum"/>) of this <see cref="Range{T}"/>.
		/// </summary>
		/// <param name="range">This <see cref="Range{T}"/></param>
		/// <returns>The magnitude of <paramref name="range"/></returns>
		public static TimeSpan Magnitude(this Range<DateTime> range)
		{
			return range.Maximum > range.Minimum ? range.Maximum - range.Minimum : range.Minimum - range.Maximum;
		}

		/// <summary>
		/// Gets the magnitude (distance between <see cref="Range{T}.Maximum"/>
		/// and <see cref="Range{T}.Minimum"/>) of this <see cref="Range{T}"/>.
		/// </summary>
		/// <param name="range">This <see cref="Range{Timespan}"/></param>
		/// <returns>The magnitude of <paramref name="range"/></returns>
		public static TimeSpan Magnitude(this Range<TimeSpan> range)
		{
			return range.Maximum > range.Minimum ? range.Maximum - range.Minimum : range.Minimum - range.Maximum;
		}
	}
	#endregion
}