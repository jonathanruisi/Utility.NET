// ┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  FILE:       Tick.cs
// ┃  PROJECT:    Utility.WPF
// ┃  SOLUTION:   Utility.NET
// ┃  CREATED:    2018-04-08 @ 11:31 AM
// ┣━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  AUTHOR:     Jonathan Ruisi
// ┃  EMAIL:      JonathanRuisi@gmail.com
// ┣━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  GIT REPO:   https://github.com/jonathanruisi/Utility.NET
// ┃  LICENSE:    https://github.com/jonathanruisi/Utility.NET/blob/master/LICENSE
// ┗━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Text;
using System.Windows.Data;

namespace JLR.Utility.WPF.Elements
{
	#region Enumerated Types
	[Flags]
	public enum TickTypes
	{
		Origin = 1,
		Major = 2,
		Minor = 4
	};
	#endregion

	#region Type Converters
	public class TickHashSetConverter : TypeConverter, IValueConverter
	{
		/// <inheritdoc />
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
		{
			return sourceType == typeof(string);
		}

		/// <inheritdoc />
		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
		{
			return destinationType == typeof(string);
		}

		/// <inheritdoc />
		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			if (!(value is string strValue))
				return null;

			var result      = new Dictionary<decimal, Tick>();
			var tickStrings = strValue.Split(',', ' ');
			foreach (var tickString in tickStrings)
			{
				var paramStrings = tickString.Split(':');
				if (!decimal.TryParse(paramStrings[0], out var number))
					continue;

				TickTypes tickType = 0;
				if (paramStrings.Length >= 2)
				{
					foreach (var ch in paramStrings[1])
					{
						switch (ch)
						{
							case 'O':
								tickType |= TickTypes.Origin;
								break;
							case 'M':
								tickType |= TickTypes.Major;
								break;
							case 'm':
								tickType |= TickTypes.Minor;
								break;
						}
					}
				}

				result.Add(number, new Tick(tickType == 0 ? TickTypes.Major : tickType, 0));
			}

			return result;
		}

		/// <inheritdoc />
		public override object ConvertTo(ITypeDescriptorContext context,
										 CultureInfo culture,
										 object value,
										 Type destinationType)
		{
			if (!(value is Dictionary<decimal, Tick> ticks))
				return null;

			var str = new StringBuilder();
			foreach (var tick in ticks)
			{
				str.Append(tick.Key);
				str.Append(':');
				if (tick.Value.TickType.HasFlag(TickTypes.Origin))
					str.Append('O');
				if (tick.Value.TickType.HasFlag(TickTypes.Major))
					str.Append('M');
				if (tick.Value.TickType.HasFlag(TickTypes.Minor))
					str.Append('m');
				str.Append(", ");
			}

			if (ticks.Count > 1)
				str.Remove(str.Length - 2, 2);
			return str.ToString();
		}

		#region IValueConverter Implementation
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return ConvertTo(null, culture, value, targetType);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return ConvertFrom(value);
		}
		#endregion
	}
	#endregion

	public struct Tick : IEquatable<Tick>
	{
		#region Properties
		public TickTypes TickType { get; }
		public double Coordinate { get; }
		#endregion

		#region Static Properties
		public static Tick Zero => new Tick(TickTypes.Origin, 0);
		#endregion

		#region Constructors
		public Tick(TickTypes tickType, double coordinate)
		{
			TickType   = tickType;
			Coordinate = coordinate;
		}
		#endregion

		#region Interface Implementation (IEquatable<T>)
		/// <inheritdoc />
		public bool Equals(Tick other)
		{
			return TickType == other.TickType && Math.Abs(Coordinate - other.Coordinate) < double.Epsilon;
		}
		#endregion

		#region Method Overrides (System.ValueType)
		/// <inheritdoc />
		public override bool Equals(object obj)
		{
			if (obj is Tick tick)
				return Equals(tick);
			return false;
		}

		/// <inheritdoc />
		public override int GetHashCode()
		{
			return TickType.GetHashCode() ^ Coordinate.GetHashCode();
		}

		/// <inheritdoc />
		public override string ToString()
		{
			return $"{Coordinate:0.###} ({(TickType.HasFlag(TickTypes.Origin) ? "O" : string.Empty)}" +
				$"{(TickType.HasFlag(TickTypes.Major) ? "M" : string.Empty)}" +
				$"{(TickType.HasFlag(TickTypes.Minor) ? "m" : string.Empty)})";
		}
		#endregion

		#region Operator Overloads
		public static bool operator ==(Tick lhs, Tick rhs)
		{
			return lhs.Equals(rhs);
		}

		public static bool operator !=(Tick lhs, Tick rhs)
		{
			return !lhs.Equals(rhs);
		}

		public static explicit operator double(Tick tick)
		{
			return tick.Coordinate;
		}
		#endregion
	}
}