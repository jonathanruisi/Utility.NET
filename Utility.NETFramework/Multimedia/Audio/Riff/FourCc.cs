// ┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  FILE:       FourCc.cs
// ┃  PROJECT:    Utility.NET
// ┃  SOLUTION:   Utility.NET
// ┃  CREATED:    2018-03-13 @ 7:09 PM
// ┣━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  AUTHOR:     Jonathan Ruisi
// ┃  EMAIL:      JonathanRuisi@gmail.com
// ┣━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  GIT REPO:   https://github.com/jonathanruisi/Utility.NET
// ┃  LICENSE:    https://github.com/jonathanruisi/Utility.NET/blob/master/LICENSE
// ┗━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

using System;
using System.Text;

namespace JLR.Utility.NET.Multimedia.Audio.Riff
{
	/// <summary>
	/// A 32-bit value representing a RIFF FourCC (Four Character Code)
	/// </summary>
	public struct FourCc : IEquatable<FourCc>
	{
		#region Fields
		private readonly uint _value;
		#endregion

		#region Properties
		public static FourCc Zero => new FourCc(0);
		#endregion

		#region Constructors
		public FourCc(uint value)
		{
			_value = value;
		}

		public FourCc(string value)
		{
			if (value.Length != 4)
			{
				throw new RiffException("Specified FourCC string must contain exactly 4 characters");
			}

			_value = BitConverter.ToUInt32(Encoding.ASCII.GetBytes(value), 0);
		}

		public FourCc(byte[] value)
		{
			if (value.Length != 4)
			{
				throw new RiffException("Specified byte[] must contain exactly 4 bytes");
			}

			_value = BitConverter.ToUInt32(value, 0);
		}

		public FourCc(char[] value)
		{
			if (value.Length != 4)
			{
				throw new RiffException("Specified char[] must contain exactly 4 characters");
			}

			_value = BitConverter.ToUInt32(Encoding.ASCII.GetBytes(value), 0);
		}
		#endregion

		#region Static Methods
		public static FourCc FromUInt32(uint value)
		{
			return new FourCc(value);
		}

		public static FourCc FromString(string value)
		{
			return new FourCc(value);
		}

		public static FourCc FromByteArray(byte[] value)
		{
			return new FourCc(value);
		}

		public static FourCc FromCharArray(char[] value)
		{
			return new FourCc(value);
		}
		#endregion

		#region Public Methods
		public uint ToUInt32()
		{
			return _value;
		}

		public int ToInt32()
		{
			return (int)_value;
		}

		public byte[] ToByteArray()
		{
			return BitConverter.GetBytes(_value);
		}

		public char[] ToCharArray()
		{
			return ToString().ToCharArray();
		}
		#endregion

		#region Interface Implementation (IEquatable<>)
		public bool Equals(FourCc other)
		{
			return _value == other._value;
		}
		#endregion

		#region Method Overrides (System.Object)
		public override bool Equals(object obj)
		{
			if (obj is FourCc) return Equals((FourCc)obj);
			return false;
		}

		public override int GetHashCode()
		{
			return ToInt32();
		}

		public override string ToString()
		{
			return Encoding.ASCII.GetString(BitConverter.GetBytes(_value));
		}
		#endregion

		#region Operator Overloads (Comparison)
		public static bool operator ==(FourCc lhs, FourCc rhs)
		{
			return lhs.Equals(rhs);
		}

		public static bool operator !=(FourCc lhs, FourCc rhs)
		{
			return !lhs.Equals(rhs);
		}
		#endregion

		#region Operator Overloads (Conversion)
		public static implicit operator FourCc(uint value)
		{
			return FromUInt32(value);
		}

		public static implicit operator FourCc(string value)
		{
			return FromString(value);
		}

		public static implicit operator FourCc(byte[] value)
		{
			return FromByteArray(value);
		}

		public static implicit operator FourCc(char[] value)
		{
			return FromCharArray(value);
		}

		public static implicit operator uint(FourCc value)
		{
			return value.ToUInt32();
		}

		public static implicit operator string(FourCc value)
		{
			return value.ToString();
		}

		public static implicit operator byte[](FourCc value)
		{
			return value.ToByteArray();
		}

		public static implicit operator char[](FourCc value)
		{
			return value.ToCharArray();
		}
		#endregion
	}
}