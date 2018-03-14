// ┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  FILE:       ExtensionMethods.cs
// ┃  PROJECT:    Utility.WinApi
// ┃  SOLUTION:   Utility.NET
// ┃  CREATED:    2016-01-14 @ 7:32 PM
// ┣━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  AUTHOR:     Jonathan Ruisi
// ┃  EMAIL:      JonathanRuisi@gmail.com
// ┣━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  GIT REPO:   https://github.com/jonathanruisi/Utility.NET
// ┃  LICENSE:    https://github.com/jonathanruisi/Utility.NET/blob/master/LICENSE
// ┗━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

using System;

namespace JLR.Utility.WinApi
{
	public static class ExtensionMethods
	{
		#region System.DateTime
		/// <summary>
		/// Converts a <c>DateTime</c> value to a COM <c>FILETIME</c> structure.
		/// </summary>
		/// <param name="value">The <c>DateTime</c> value to convert.</param>
		/// <returns>Returns a <c>System.Runtime.InteropServices.ComTypes.FILETIME</c> structure.</returns>
		public static System.Runtime.InteropServices.ComTypes.FILETIME ToFileTimeCom(this DateTime value)
		{
			var filetime = value.ToFileTime();
			var ft = new System.Runtime.InteropServices.ComTypes.FILETIME
			{
				dwLowDateTime  = (int)(filetime & 0xFFFFFFFF),
				dwHighDateTime = (int)(filetime >> 32)
			};
			return ft;
		}
		#endregion

		#region System.Runtime.InteropServices.ComTypes.FILETIME
		/// <summary>
		/// Converts a COM <c>FILETIME</c> structure to a <c>DateTime</c> value (<c>Int64</c> format).
		/// </summary>
		/// <param name="value">The <c>System.Runtime.InteropServices.ComTypes.FILETIME</c> structure to convert.</param>
		/// <returns>Returns a <c>DateTime</c> value in the form of a <c>Int64</c> value.</returns>
		public static long ToInt64(this System.Runtime.InteropServices.ComTypes.FILETIME value)
		{
			return ((long)value.dwHighDateTime << 32) + value.dwLowDateTime;
		}
		#endregion
	}
}