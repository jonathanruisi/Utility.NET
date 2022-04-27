namespace JLR.Utility.NET.Math
{
	public static class Math
	{
		private static readonly double Rad5 = System.Math.Sqrt(5);

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
			return (int)((System.Math.Pow(1 + Rad5, n) - System.Math.Pow(1 - Rad5, n)) /
				(System.Math.Pow(2, n) * Rad5));
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
			return values.Aggregate((a, b) => a == 0 && b == 0
				? 0
				: (System.Math.Abs(a) / Gcd(a, b)) * System.Math.Abs(b));
		}
	}
}