namespace JLR.Utility.NET.Math
{
	public static class Prime
	{
		#region Constants
		private static readonly uint CacheSizeLimit = 1000000;
		#endregion

		#region Fields
		private static bool _isPrimeCacheEnabled;
		private static ulong _largestPrime;
		private static HashSet<ulong> _primes;
		#endregion

		#region Properties
		public static bool IsPrimeCacheEnabled
		{
			get => _isPrimeCacheEnabled;
			set
			{
				_isPrimeCacheEnabled = value;
				if (value)
				{
					if (_primes != null) return;
					_primes = new HashSet<ulong>() { 2, 3 };
					_largestPrime = 3;
				}
				else
				{
					_primes.Clear();
					_largestPrime = 0;
				}
			}
		}
		#endregion

		#region Constructor
		static Prime()
		{
			_primes = new HashSet<ulong>();
			IsPrimeCacheEnabled = false;
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Computes the prime factorization of a specified integer (using an optimized trial division algorithm)
		/// </summary>
		/// <param name="value">Value to factorize</param>
		/// <returns>A list of tuples containing the prime factor and its power</returns>
		/// <remarks>
		/// This method adds values to the prime number cache
		/// (if enabled via <code>MathHelper.IsPrimeCacheEnabled</code>)
		/// </remarks>
		public static List<(ulong factor, byte power)> PrimeFactors(ulong value)
		{
			var result = new List<(ulong factor, byte power)>();

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

			for (ulong i = 3; i * i <= value; i += 2)
			{
				power = 0;
				while (value % i == 0)
				{
					power++;
					value /= i;
				}

				if (power == 0)
					continue;

				result.Add((i, power));
				if (!IsPrimeCacheEnabled || i <= _largestPrime || _primes.Count >= CacheSizeLimit)
					continue;

				_primes.Add(i);
				_largestPrime = i;
			}

			if (value <= 2)
				return result;

			result.Add((value, 1));
			if (!IsPrimeCacheEnabled || _primes.Count >= CacheSizeLimit || _primes.Contains(value))
				return result;

			_primes.Add(value);
			_largestPrime = value;

			return result;
		}

		/// <summary>
		/// Determines if the specified integer is a prime number
		/// </summary>
		/// <param name="value">An integer value</param>
		/// <returns><code>true</code> if <paramref name="value"/> is prime, <code>false</code> otherwise</returns>
		public static bool IsPrime(ulong value)
		{
			if (value == 1)
				return true;

			if (IsPrimeCacheEnabled && value <= _largestPrime)
				return _primes.Contains(value);

			var primeFactors = PrimeFactors(value);
			return value > 2 && primeFactors[primeFactors.Count - 1].factor == value;
		}

		/// <summary>
		/// Computes all factors of a specified integer
		/// </summary>
		/// <param name="value">An integer value</param>
		/// <param name="isResultSorted">
		/// If <code>true</code>, the returned list of divisors will be sorted from smallest to largest.
		/// This optional parameter is <code>true</code> by default.
		/// </param>
		/// <returns>
		/// A list of <code>ulong</code> integers containing all divisors of <paramref name="value"/>,
		/// including 1 and itself.
		/// </returns>
		public static List<ulong> Divisors(ulong value, bool isResultSorted = true)
		{
			var result = new List<ulong>();
			if (value != 0)
				result.Add(1);

			var primeFactors = PrimeFactors(value);
			CalculateDivisors(1, 0);

			void CalculateDivisors(ulong n, ulong p)
			{
				for (var i = p; i < (ulong)primeFactors.Count; i++)
				{
					var x = primeFactors[(int)i].factor * n;
					for (var j = 0; j < primeFactors[(int)i].power; j++)
					{
						result.Add(x);
						CalculateDivisors(x, i + 1);
						x *= primeFactors[(int)i].factor;
					}
				}
			}

			if (isResultSorted)
				result.Sort();

			return result;
		}
		#endregion
	}
}