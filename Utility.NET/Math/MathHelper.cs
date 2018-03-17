// ┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  FILE:       MathHelper.cs
// ┃  PROJECT:    Utility.NET
// ┃  SOLUTION:   Utility.NET
// ┃  CREATED:    2015-12-23 @ 4:58 AM
// ┣━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  AUTHOR:     Jonathan Ruisi
// ┃  EMAIL:      JonathanRuisi@gmail.com
// ┣━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  GIT REPO:   https://github.com/jonathanruisi/Utility.NET
// ┃  LICENSE:    https://github.com/jonathanruisi/Utility.NET/blob/master/LICENSE
// ┗━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace JLR.Utility.NET.Math
{
	public static class MathHelper
	{
		#region Constants
		private static readonly double Rad5 = System.Math.Sqrt(5);
		#endregion

		#region Fields
		private static bool _isPrimeCacheEnabled;
		private static long _largestPrime;
		public static HashSet<long> _primes;
		#endregion

		#region Properties
		public static bool IsPrimeCacheEnabled
		{
			get => _isPrimeCacheEnabled;
			set
			{
				_isPrimeCacheEnabled = value;
				if (value && _primes == null)
				{
					_primes = new HashSet<long>() { 2, 3 };
					_largestPrime = 3;
				}
				else
				{
					_primes = null;
					_largestPrime = 0;
				}
			}
		}
		#endregion

		#region Constructor
		static MathHelper()
		{
			IsPrimeCacheEnabled = true;
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Returns the largest of the specified values
		/// </summary>
		/// <typeparam name="T">Any type that implements <see cref="IComparable"/></typeparam>
		/// <param name="values">A set of values to compare</param>
		/// <returns>The largest of the specified values</returns>
		public static T Max<T>(params T[] values) where T : IComparable<T>
		{
			var result = values[0];
			for (var i = 1; i < values.Length; i++)
			{
				if (values[i].CompareTo(result) == 1)
					result = values[i];
			}

			return result;
		}

		/// <summary>
		/// Returns the smallest of the specified values
		/// </summary>
		/// <typeparam name="T">Any type that implements <see cref="IComparable{T}"/></typeparam>
		/// <param name="values">A set of values to compare</param>
		/// <returns>The smallest of the specified values</returns>
		public static T Min<T>(params T[] values) where T : IComparable<T>
		{
			var result = values[0];
			for (var i = 1; i < values.Length; i++)
			{
				if (values[i].CompareTo(result) == -1)
					result = values[i];
			}

			return result;
		}

		/// <summary>
		/// Computes the factorial of the specified value.
		/// A factorial is the product of all positive integers
		/// less than or equal to a specified positive integer.
		/// </summary>
		/// <param name="value">The value</param>
		/// <returns>The factorial of <paramref name="value"/></returns>
		public static ulong Factorial(ulong value)
		{
			for (var i = value; i > 1; i--)
			{
				value = checked(value * (i - 1));
			}

			return value == 0 ? 1 : value;
		}

		/// <summary>
		/// Computes the permutation of the specified values (nPr).
		/// A permutation is the choice of <paramref name="r"/> objects
		/// from a set of <paramref name="n"/> objects
		/// without replacement and where order matters.
		/// </summary>
		/// <param name="n">The "n" value (total number of objects)</param>
		/// <param name="r">The "r" value (number of choices)</param>
		/// <returns>The permutation nPr</returns>
		public static ulong Permutation(ulong n, ulong r)
		{
			if (r > n) return 0;
			if (r == 0) return 1;
			if (r == 1) return n;
			if (n == r) return Factorial(n);
			return Factorial(n) / Factorial(n - r);
		}

		/// <summary>
		/// Computes the combination of the specified values (n choose r).
		/// A combination is the choice of <paramref name="r"/> objects
		/// from a set of <paramref name="n"/> objects
		/// without replacement and where order does not matter.
		/// </summary>
		/// <param name="n">The "n" value (total number of objects)</param>
		/// <param name="r">The "r" value (number of choices)</param>
		/// <returns>The combination nCr (n choose r)</returns>
		public static ulong Combination(ulong n, ulong r)
		{
			if (r > n) return 0;
			if (r == 0 || n == r) return 1;
			if (r == 1 || r == n - 1) return n;
			return Permutation(n, r) / Factorial(r);
		}

		/// <summary>
		/// Computes a specified term of the Fibonacci sequence.
		/// </summary>
		/// <param name="n">The nth term to compute</param>
		/// <returns>The specified term of the Fibonacci sequence</returns>
		public static int Fibonacci(int n)
		{
			return (int)((System.Math.Pow(1 + Rad5, n) - System.Math.Pow(1 - Rad5, n)) / (System.Math.Pow(2, n) * Rad5));
		}

		/// <summary>
		/// Computes the greatest common divisor of a set of integers
		/// </summary>
		/// <param name="values">Set of integer values</param>
		/// <returns>The greatest common divisor of the specified values</returns>
		public static long Gcd(params long[] values)
		{
			return values.Aggregate(Gcd);
		}

		/// <summary>
		/// Computes the greatest common divisor of two integers
		/// </summary>
		/// <param name="a">An integer value</param>
		/// <param name="b">An integer value</param>
		/// <returns>The greatest common divisor of the specified values</returns>
		public static long Gcd(long a, long b)
		{
			if (a < 0) a = System.Math.Abs(a);
			if (b < 0) b = System.Math.Abs(b);

			while (a != 0 && b != 0)
			{
				if (a > b)
					a %= b;
				else
					b %= a;
			}

			return a == 0 ? b : a;
		}

		/// <summary>
		/// Computes the least common multiple of a set of integers
		/// </summary>
		/// <param name="values">Set of integer values</param>
		/// <returns>The least common multiple of the specified values</returns>
		public static long Lcm(params long[] values)
		{
			return values.Aggregate((a, b) => a == 0 && b == 0 ? 0 : (System.Math.Abs(a) / Gcd(a, b)) * System.Math.Abs(b));
		}

		public static List<(long factor, byte power)> PrimeFactors(long value)
		{
			var result = new List<(long, byte)>();
			if (value < 2)
				return result;

			byte power = 0;
			while (value % 2 == 0)
			{
				power++;
				value >>= 1;
			}

			if (power > 0)
				result.Add((2, power));

			for (long i = 3; i * i <= value; i += 2)
			{
				power = 0;
				while (value % i == 0)
				{
					power++;
					value /= i;
				}

				if (power <= 0) continue;
				result.Add((i, power));
				if (!IsPrimeCacheEnabled || i <= _largestPrime) continue;
				_primes.Add(i);
				_largestPrime = i;
			}

			if (value <= 2) return result;
			result.Add((value, 1));
			if (!IsPrimeCacheEnabled || _primes.Contains(value)) return result;
			_primes.Add(value);
			_largestPrime = value;

			return result;
		}
		#endregion

		#region Private Methods
		
		#endregion
	}
}