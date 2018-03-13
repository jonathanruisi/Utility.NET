// ┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  FILE:       ExtensionMethods.cs
// ┃  PROJECT:    Utility.NET
// ┃  SOLUTION:   Utility.NET
// ┃  CREATED:    2015-12-23 @ 4:51 AM
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

namespace JLR.Utility.NET
{
	public static class ExtensionMethods
	{
		#region System.Enum
		/// <summary>
		/// Provides an enumeration of the names of all <c>true</c> bit field flags.
		/// </summary>
		/// <param name="value">An <c>enum</c> (bit field)</param>
		/// <returns>A <see cref="System.String"/> enumerable</returns>
		public static IEnumerable<string> GetSetFlagNames(this Enum value)
		{
			var values = Enum.GetValues(value.GetType());
			return (from object obj in values where value.HasFlag((Enum)obj) select Enum.GetName(value.GetType(), obj)).ToList();
		}
		#endregion

		#region System.String
		/// <summary>
		/// A quick-and-dirty way to center-justify strings based on a specified width.
		/// For example, the string "Hello" and a total width of 20 would yield:
		/// "_______Hello!_______" (Underscored provided for clarity).
		/// When used for output purposes, this method is only effective on monospaced fonts.
		/// </summary>
		/// <param name="value">The <see cref="System.String"/> to pad and center</param>
		/// <param name="totalWidth">The total column width of the formatted space.</param>
		/// <returns>
		/// A <see cref="System.String"/> containing the original text center-justified
		/// padded with spaces on either side.
		/// </returns>
		public static String PadAndCenter(this string value, int totalWidth)
		{
			if (totalWidth < value.Length)
				throw new ArgumentOutOfRangeException(
					nameof(value),
					$"The overall length must be at least {totalWidth} (the length of the specified string)");

			var padCount = (totalWidth - value.Length) / 2;
			var result   = new StringBuilder();
			result.Append(' ', padCount);
			result.Append(value);
			result.Append(' ', (totalWidth - value.Length) % 2 == 0 ? padCount : padCount + 1);
			return result.ToString();
		}

		/// <summary>
		/// Parses a <see cref="System.String"/> for specified characters.
		/// Matching characters are either kept or removed based on a specified rule.
		/// </summary>
		/// <param name="value">The source <see cref="System.String"/></param>
		/// <param name="matchRule">Specifies whether matching characters are kept or removed.</param>
		/// <param name="characters">A list of characters to match.</param>
		/// <returns>A <see cref="System.String"/> containing the results.</returns>
		public static string Filter(this string value, FilterAction matchRule, params char[] characters)
		{
			return new string(
				(from c in value where matchRule == FilterAction.Keep ? characters.Contains(c) : !characters.Contains(c) select c)
				.ToArray());
		}

		public static string Filter(this string value, FilterAction matchRule, UnicodeCategory categories)
		{
			return new string(
				(from c in value
				 where matchRule == FilterAction.Keep
					 ? categories.HasFlag(Char.GetUnicodeCategory(c))
					 : !categories.HasFlag(Char.GetUnicodeCategory(c))
				 select c).ToArray());
		}
		#endregion
	}
}