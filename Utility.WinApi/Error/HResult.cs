// ┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  FILE:       HResult.cs
// ┃  PROJECT:    Utility.WinApi
// ┃  SOLUTION:   Utility.NET
// ┃  CREATED:    2016-01-14 @ 8:08 PM
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
	/// <summary>
	/// A .NET version of the COM <c>HRESULT</c> structure.
	/// These values translate seamlessly when performing COM interop,
	/// and provide informative translations of both success and failure codes.
	/// </summary>
	public struct HResult : IEquatable<HResult>
	{
		#region Fields
		private readonly int _value;
		#endregion

		#region Properties
		/// <summary>
		/// Gets the value of a success HResult. Returns null for all error HResults.
		/// </summary>
		public uint? SuccessValue
		{
			get
			{
				if (Succeeded(this))
				{
					return (uint)_value;
				}
				else
				{
					return null;
				}
			}
		}

		/// <summary>
		/// Gets the severity (error/success status) of an HResult.
		/// </summary>
		public Severity Severity => GetSeverity();

		/// <summary>
		/// Gets the facility code of an HResult.
		/// </summary>
		public Facility Facility => GetFacility();

		/// <summary>
		/// Gets the error/success code of an HResult.
		/// </summary>
		public ushort Code => GetCode();

		/// <summary>
		/// A read-only instance of the HResult structure whose value is 0.
		/// </summary>
		public static HResult Empty => new HResult(Severity.Success, Facility.NULL, 0);
		#endregion

		#region Constructors
		public HResult(int value)
		{
			_value = value;
		}

		public HResult(uint value)
		{
			_value = unchecked((int)value);
		}

		public HResult(Severity severity, Facility facility, ushort code)
		{
			_value = ((ushort)severity << 31) | ((ushort)facility << 16) | code;
		}
		#endregion

		#region Private Methods
		private Severity GetSeverity()
		{
			return (Severity)((_value >> 31) & 0x1U);
		}

		private Facility GetFacility()
		{
			return (Facility)((_value >> 16) & 0x1FFFU);
		}

		private ushort GetCode()
		{
			return (ushort)(_value & 0xFFFFU);
		}
		#endregion

		#region Static Methods
		public static HResult Create(Severity severity, Facility facility, ushort code)
		{
			return new HResult(severity, facility, code);
		}

		public static bool Succeeded(HResult result)
		{
			return result.Severity == Severity.Success;
		}

		public static bool Failed(HResult result)
		{
			return result.Severity == Severity.Error;
		}

		public static HResult Try(HResult result)
		{
			if (Succeeded(result))
				return result;
			throw new HResultException(result);
		}

		public static string Lookup(HResult hResult, Dictionary<HResult, string> hResultDictionary)
		{
			return hResultDictionary.ContainsKey(hResult) ? hResultDictionary[hResult] : null;
		}
		#endregion

		#region Interface Implementation (IEquatable<HResult>)
		public bool Equals(HResult other)
		{
			return _value == other._value;
		}
		#endregion

		#region Operator Overloads
		public static implicit operator int(HResult value)
		{
			return value._value;
		}

		public static implicit operator uint(HResult value)
		{
			return (uint)value._value;
		}

		public static implicit operator HResult(int value)
		{
			return new HResult(value);
		}

		public static implicit operator HResult(uint value)
		{
			return new HResult(value);
		}

		public static bool operator ==(HResult lhs, HResult rhs)
		{
			return lhs.Equals(rhs);
		}

		public static bool operator !=(HResult lhs, HResult rhs)
		{
			return !lhs.Equals(rhs);
		}
		#endregion

		#region Method Overrides (System.ValueType)
		public override bool Equals(object obj)
		{
			if (!((obj is HResult) || (obj is int)))
				return false;
			return Equals((HResult)obj);
		}

		public override int GetHashCode()
		{
			return _value;
		}

		public override string ToString()
		{
			return $"{Severity}: {Facility}({Code})";
		}
		#endregion
	}
}