using System;
using System.Globalization;

using Windows.UI.Xaml.Data;

namespace JLR.Utility.UWP
{
	public class NullableNumberToStringConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			return value switch
			{
				sbyte sbyteValue     => sbyteValue.ToString(new CultureInfo(language)),
				byte byteValue       => byteValue.ToString(new CultureInfo(language)),
				short shortValue     => shortValue.ToString(new CultureInfo(language)),
				ushort ushortValue   => ushortValue.ToString(new CultureInfo(language)),
				int intValue         => intValue.ToString(new CultureInfo(language)),
				uint uintValue       => uintValue.ToString(new CultureInfo(language)),
				long longValue       => longValue.ToString(new CultureInfo(language)),
				ulong ulongValue     => ulongValue.ToString(new CultureInfo(language)),
				char charValue       => charValue.ToString(new CultureInfo(language)),
				float floatValue     => floatValue.ToString(new CultureInfo(language)),
				double doubleValue   => doubleValue.ToString(new CultureInfo(language)),
				decimal decimalValue => decimalValue.ToString(new CultureInfo(language)),
				null                 => "NULL",
				_                    => value.ToString()
			};
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			if (value is string str && !string.IsNullOrEmpty(str))
			{
				if (str.ToLower(CultureInfo.CurrentCulture) == "null")
					return null;

				if (targetType == typeof(sbyte)) return sbyte.Parse(str);
				if (targetType == typeof(byte)) return byte.Parse(str);
				if (targetType == typeof(short)) return short.Parse(str);
				if (targetType == typeof(ushort)) return ushort.Parse(str);
				if (targetType == typeof(int)) return int.Parse(str);
				if (targetType == typeof(uint)) return uint.Parse(str);
				if (targetType == typeof(long)) return long.Parse(str);
				if (targetType == typeof(ulong)) return ulong.Parse(str);
				if (targetType == typeof(char)) return char.Parse(str);
				if (targetType == typeof(float)) return float.Parse(str);
				if (targetType == typeof(double)) return double.Parse(str);
				if (targetType == typeof(decimal)) return decimal.Parse(str);
			}

			throw new ArgumentException("Unable to parse string");
		}
	}
}