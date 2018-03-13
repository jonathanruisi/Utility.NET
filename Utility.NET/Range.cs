// ┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  FILE:       Range.cs
// ┃  PROJECT:    Utility.NET
// ┃  SOLUTION:   Utility.NET
// ┃  CREATED:    2015-12-23 @ 4:28 AM
// ┣━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  AUTHOR:     Jonathan Ruisi
// ┃  EMAIL:      JonathanRuisi@gmail.com
// ┣━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  GIT REPO:   https://github.com/jonathanruisi/Utility.NET
// ┃  LICENSE:    https://github.com/jonathanruisi/Utility.NET/blob/master/LICENSE
// ┗━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

using System;
using System.Collections;
using System.Collections.Generic;

namespace JLR.Utility.NET
{
	/// <summary>
	/// Represents a range of values bounded by a minimum and maximum value.
	/// The range of values can be made finite by specifying an optional increment.
	/// </summary>
	/// <typeparam name="T">
	/// Any value type that is conceptually comparable by value.
	/// (Does NOT need to implement <see cref="IComparable"/> or any other quantifying interface).
	/// </typeparam>
	public struct Range<T> : IEquatable<Range<T>>, IEnumerable<T> where T : struct
	{
		#region Properties
		/// <summary>Gets the lower bound of this range.</summary>
		public T Minimum { get; }

		/// <summary>Gets the upper bound of this range.</summary>
		public T Maximum { get; }

		/// <summary>Gets the lower bound of this range.</summary>
		public T Increment { get; }
		#endregion

		#region Constructors
		/// <summary>
		/// Initializes a new instance of the <see cref="Range{T}"/> structure.
		/// </summary>
		/// <param name="minimum">The lower bound of the range.</param>
		/// <param name="maximum">The upper bound of the range.</param>
		/// <param name="increment">
		/// By specifying an increment value, the range is made finite
		/// and can be treated as an array of values.
		/// </param>
		public Range(T minimum, T maximum, T increment = default(T))
		{
			Minimum   = minimum;
			Maximum   = maximum;
			Increment = increment;
		}
		#endregion

		#region Interface Implementation (IEquatable<T>)
		public bool Equals(Range<T> other)
		{
			return Minimum.Equals(other.Minimum) && Maximum.Equals(other.Maximum) && Increment.Equals(other.Increment);
		}
		#endregion

		#region Interface Implementation (IEnumerable<T>)
		public IEnumerator<T> GetEnumerator()
		{
			dynamic min = Minimum;
			dynamic max = Maximum;
			dynamic inc = Increment;

			if (inc == 0)
			{
				yield return min;
				yield return max;
			}
			else
			{
				for (var i = min; i <= max; i += inc)
				{
					yield return i;
				}
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
		#endregion

		#region Method Overrides (System.Object)
		public override bool Equals(object obj)
		{
			if (obj is Range<T>)
				return Equals((Range<T>)obj);
			return false;
		}

		public override int GetHashCode()
		{
			return Minimum.GetHashCode() ^ Maximum.GetHashCode() ^ Increment.GetHashCode();
		}

		public override string ToString()
		{
			return Increment.Equals(default(T))
				? $"{{{Minimum} ≤ X ≤ {Maximum}}}"
				: $"{{{Minimum} ≤ X ≤ {Maximum} (X₀+{Increment})}}";
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
}