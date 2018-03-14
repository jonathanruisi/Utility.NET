// ┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  FILE:       ColorRef.cs
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
using System.Drawing;
using System.Runtime.InteropServices;

namespace JLR.Utility.WinApi
{
	[StructLayout(LayoutKind.Explicit, Size = 4)]
	public struct ColorRef : IEquatable<ColorRef>
	{
		#region Fields
		[FieldOffset(0)] private readonly byte _r;

		[FieldOffset(1)] private readonly byte _g;

		[FieldOffset(2)] private readonly byte _b;

		[FieldOffset(0)] private readonly uint _value;
		#endregion

		#region Properties
		public byte R     => _r;
		public byte G     => _g;
		public byte B     => _b;
		public uint Value => _value;
		#endregion

		#region Constructors
		public ColorRef(byte r, byte g, byte b)
		{
			_value = 0;
			_r     = r;
			_g     = g;
			_b     = b;
		}

		public ColorRef(uint value)
		{
			_r     = 0;
			_g     = 0;
			_b     = 0;
			_value = value & 0x00FFFFFF;
		}

		public ColorRef(Color value)
		{
			_value = 0;
			_r     = value.R;
			_g     = value.G;
			_b     = value.B;
		}
		#endregion

		#region Interface Implementation (IEquatable<ColorRef>)
		public bool Equals(ColorRef other)
		{
			return _value == other._value;
		}
		#endregion

		#region Method Overrides (System.ValueType)
		public override bool Equals(object obj)
		{
			if (obj == null)
			{
				return false;
			}

			if (obj is ColorRef)
				return Equals((ColorRef)obj);
			else
			{
				return false;
			}
		}

		public override int GetHashCode()
		{
			return _r * _g * _b;
		}

		public override string ToString()
		{
			return $"R={_r} G={_g} B={_b}";
		}
		#endregion

		#region Operator Overloads
		public static implicit operator int(ColorRef value)
		{
			return (int)value._value;
		}

		public static implicit operator uint(ColorRef value)
		{
			return value._value;
		}

		public static implicit operator ColorRef(int value)
		{
			return new ColorRef((uint)value);
		}

		public static implicit operator ColorRef(uint value)
		{
			return new ColorRef(value);
		}

		public static implicit operator Color(ColorRef value)
		{
			return Color.FromArgb(0, value._r, value._g, value._b);
		}

		public static implicit operator ColorRef(Color value)
		{
			return new ColorRef(value);
		}

		public static bool operator ==(ColorRef lhs, ColorRef rhs)
		{
			return lhs.Equals(rhs);
		}

		public static bool operator !=(ColorRef lhs, ColorRef rhs)
		{
			return !lhs.Equals(rhs);
		}
		#endregion
	}
}