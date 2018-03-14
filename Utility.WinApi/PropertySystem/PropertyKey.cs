// ┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  FILE:       PropertyKey.cs
// ┃  PROJECT:    Utility.WinApi
// ┃  SOLUTION:   Utility.NET
// ┃  CREATED:    2016-01-14 @ 8:30 PM
// ┣━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  AUTHOR:     Jonathan Ruisi
// ┃  EMAIL:      JonathanRuisi@gmail.com
// ┣━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  GIT REPO:   https://github.com/jonathanruisi/Utility.NET
// ┃  LICENSE:    https://github.com/jonathanruisi/Utility.NET/blob/master/LICENSE
// ┗━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

using System;
using System.Runtime.InteropServices;

namespace JLR.Utility.WinApi.PropertySystem
{
	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	public struct PropertyKey : IEquatable<PropertyKey>
	{
		#region Properties
		public Guid FormatId   { get; }
		public int  PropertyId { get; }
		#endregion

		#region Constructors
		public PropertyKey(Guid formatId, int propertyId)
		{
			FormatId   = formatId;
			PropertyId = propertyId;
		}

		public PropertyKey(string formatId, int propertyId)
		{
			FormatId   = new Guid(formatId);
			PropertyId = propertyId;
		}
		#endregion

		#region Interface Implementation (IEquatable<PropertyKey>)
		public bool Equals(PropertyKey other)
		{
			return FormatId.Equals(other.FormatId) && (PropertyId == other.PropertyId);
		}
		#endregion

		#region Method Overrides (System.ValueType)
		public override bool Equals(object obj)
		{
			if (obj == null)
			{
				return false;
			}

			if (obj is PropertyKey)
			{
				var other = (PropertyKey)obj;
				return Equals(other);
			}
			else
			{
				return false;
			}
		}

		public override int GetHashCode()
		{
			return FormatId.GetHashCode() ^ PropertyId;
		}

		public override string ToString()
		{
			return $"{FormatId.ToString("B")}, {PropertyId}";
		}
		#endregion

		#region Operator Overloads
		public static bool operator ==(PropertyKey key1, PropertyKey key2)
		{
			return key1.Equals(key2);
		}

		public static bool operator !=(PropertyKey key1, PropertyKey key2)
		{
			return !key1.Equals(key2);
		}
		#endregion
	}
}