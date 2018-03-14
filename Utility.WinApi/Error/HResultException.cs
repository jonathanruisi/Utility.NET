// ┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  FILE:       HResultException.cs
// ┃  PROJECT:    Utility.WinApi
// ┃  SOLUTION:   Utility.NET
// ┃  CREATED:    2016-01-14 @ 8:15 PM
// ┣━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  AUTHOR:     Jonathan Ruisi
// ┃  EMAIL:      JonathanRuisi@gmail.com
// ┣━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  GIT REPO:   https://github.com/jonathanruisi/Utility.NET
// ┃  LICENSE:    https://github.com/jonathanruisi/Utility.NET/blob/master/LICENSE
// ┗━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

using System;
using System.Collections.Generic;

namespace JLR.Utility.WinApi.Error
{
	public class HResultException : Exception
	{
		#region Properties
		public string Details { get; private set; }
		#endregion

		#region Constructors
		public HResultException() : base() { }
		public HResultException(string message) : base(message) { }
		public HResultException(string message, Exception innerException) : base(message, innerException) { }

		public HResultException(HResult hResult) : base()
		{
			HResult = hResult;
		}

		public HResultException(HResult hResult, string message) : base(message)
		{
			HResult = hResult;
		}

		public HResultException(HResult hResult, string message, Exception innerException) : base(message, innerException)
		{
			HResult = hResult;
		}
		#endregion

		#region Public Methods
		public bool LookupDetails(Dictionary<HResult, string> hResultDictionary)
		{
			if (hResultDictionary.ContainsKey(HResult))
			{
				Details = hResultDictionary[HResult];
				return true;
			}

			Details = String.Empty;
			return false;
		}
		#endregion
	}
}