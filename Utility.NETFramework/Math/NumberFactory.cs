// ┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  FILE:       NumberFactory.cs
// ┃  PROJECT:    Utility.NET
// ┃  SOLUTION:   Utility.NET
// ┃  CREATED:    2015-12-23 @ 4:59 AM
// ┣━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  AUTHOR:     Jonathan Ruisi
// ┃  EMAIL:      JonathanRuisi@gmail.com
// ┣━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  GIT REPO:   https://github.com/jonathanruisi/Utility.NET
// ┃  LICENSE:    https://github.com/jonathanruisi/Utility.NET/blob/master/LICENSE
// ┗━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

using System;

namespace JLR.Utility.NETFramework.Math
{
	/// <summary>
	/// Contains static methods for easy creation of data sets
	/// </summary>
	public static class NumberFactory
	{
		/// <summary>
		/// Generates a signed integer array of a specified length containing random values within the specified range.
		/// </summary>
		/// <param name="length">The number of values to be generated.</param>
		/// <param name="minimum">The inclusive upper bound of the random numbers returned.</param>
		/// <param name="maximum">The exclusive upper bound of the random numbers returned.</param>
		/// <returns></returns>
		public static int[] CreateRandomIntegerArray(int length, int minimum, int maximum)
		{
			var rand        = new Random();
			var resultArray = new int[length];
			for (var i = 0; i < length; i++)
			{
				resultArray[i] = rand.Next(minimum, maximum);
			}

			return resultArray;
		}
	}
}