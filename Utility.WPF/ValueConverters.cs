// ┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  FILE:       ValueConverters.cs
// ┃  PROJECT:    Utility.WPF
// ┃  SOLUTION:   Utility.NET
// ┃  CREATED:    2015-12-25 @ 1:05 AM
// ┣━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  AUTHOR:     Jonathan Ruisi
// ┃  EMAIL:      JonathanRuisi@gmail.com
// ┣━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  GIT REPO:   https://github.com/jonathanruisi/Utility.NET
// ┃  LICENSE:    https://github.com/jonathanruisi/Utility.NET/blob/master/LICENSE
// ┗━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

using JLR.Utility.NET;
using JLR.Utility.NET.Color;

namespace JLR.Utility.WPF
{
	#region Boolean Converters
	public class NullToBoolConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return value != null;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new InvalidOperationException();
		}
	}

	public class PositiveIntegerToBoolConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return value is int && (int)value >= 1;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return value is bool && (bool)value ? 1 : 0;
		}
	}

	public class IntegerRangeToBoolConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (!(value is int))
				throw new ArgumentException("Value must be an integer", nameof(value));
			if (!(parameter is DiscreteRange<int>))
				throw new ArgumentException("Parameter must be a Range<int>", nameof(value));

			var intValue = (int)value;
			var intRange = (DiscreteRange<int>)parameter;
			return intValue >= intRange.Minimum && intValue <= intRange.Maximum;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new InvalidOperationException();
		}
	}

	public class EnumToBooleanConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return value.Equals(parameter);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return value.Equals(true) ? parameter : Binding.DoNothing;
		}
	}

	public class BoolNegationConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is bool)
				return !(bool)value;
			throw new ArgumentException("Value must be bool", nameof(value));
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is bool)
				return !(bool)value;
			throw new ArgumentException("Value must be bool", nameof(value));
		}
	}

	public class BooleanMultiConverter : IMultiValueConverter
	{
		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
		{
			return values.OfType<bool>().All(value => value);
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
		{
			throw new InvalidOperationException();
		}
	}
	#endregion

	#region Numeric Converters
	public class NumericInverseConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			dynamic n = value;
			return 1M / n;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			dynamic n = value;
			return 1M / n;
		}
	}
	#endregion

	#region Text Converters
	public class PercentageConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is double val)
				return val * 100.0;
			throw new ArgumentException("Expected a double precision floating point value");
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is string valueString)
			{
				if (valueString.Contains("%"))
					valueString = valueString.Remove(valueString.IndexOf('%'), 1);
				if (double.TryParse(valueString, out var result))
					return result / 100.0;
			}

			throw new ArgumentException("Expected a string containing a double precision floating point value");
		}
	}

	public class DegreeConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is double)
				return value;
			throw new ArgumentException("Expected a double precision floating point value");
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is string valueString)
			{
				if (valueString.Contains("°"))
					valueString = valueString.Remove(valueString.IndexOf('°'), 1);
				if (double.TryParse(valueString, out var result))
					return result;
			}

			throw new ArgumentException("Expected a string containing a double precision floating point value");
		}
	}
	#endregion

	#region Visibility Converters
	public class NullToVisibilityConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var visibility = parameter as Visibility? ?? Visibility.Collapsed;
			return value == null ? visibility : Visibility.Visible;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new InvalidOperationException();
		}
	}

	public class BoolToVisibilityConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var visibility = parameter as Visibility? ?? Visibility.Collapsed;
			return (bool)value ? Visibility.Visible : visibility;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new InvalidOperationException();
		}
	}

	public class BoolToVisibilityMultiConverter : IMultiValueConverter
	{
		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
		{
			var visibility = parameter as Visibility? ?? Visibility.Collapsed;
			return values.OfType<bool>().All(value => value) ? Visibility.Visible : visibility;
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
		{
			throw new InvalidOperationException();
		}
	}

	public class IntegerToVisibilityConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is int)
			{
				if (parameter is string)
				{
					var args = (parameter as string).Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
					if (args.Length != 3)
						throw new ArgumentException(
							"Invalid parameter string (must contain three comma separated arguments",
							nameof(parameter));
					var number = Int32.Parse(args[1]);
					if (args[2].ToUpper() != "HIDDEN" && args[2].ToUpper() != "COLLAPSED")
						throw new ArgumentException("Unrecognized visibility state", nameof(parameter));

					switch (args[0])
					{
						case "=":
							if ((int)value == number) return Visibility.Visible;
							break;
						case "!=":
							if ((int)value != number) return Visibility.Visible;
							break;
						case "<":
							if ((int)value < number) return Visibility.Visible;
							break;
						case ">":
							if ((int)value > number) return Visibility.Visible;
							break;
						case "<=":
							if ((int)value <= number) return Visibility.Visible;
							break;
						case ">=":
							if ((int)value >= number) return Visibility.Visible;
							break;
						default:
							throw new ArgumentException("Unrecognized comparison argument", nameof(parameter));
					}

					return args[2] == "HIDDEN" ? Visibility.Hidden : Visibility.Collapsed;
				}

				return (int)value > 0;
			}

			throw new ArgumentException("Value must be an integer", nameof(value));
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new InvalidOperationException();
		}
	}
	#endregion

	#region ColorSpace Converters
	public class ColorSpaceToVisibilityConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var colorSpaceString = value as string;
			if (colorSpaceString != null)
				return colorSpaceString == "CMYK" || colorSpaceString == "RGBA" ? Visibility.Visible : Visibility.Collapsed;
			throw new ArgumentException("Expected a string value");
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new InvalidOperationException();
		}
	}

	public class ColorSpaceToVisibilityMultiConverter : IMultiValueConverter
	{
		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
		{
			var colorSpaceString = values[0] as string;
			if (colorSpaceString != null && values[1] is bool)
			{
				var isChecked = (bool)values[1];
				return (colorSpaceString == "CMYK" || colorSpaceString == "RGBA") && isChecked
					? Visibility.Visible
					: Visibility.Collapsed;
			}

			throw new ArgumentException("Expected a string value and a bool value");
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
		{
			throw new InvalidOperationException();
		}
	}

	public class ColorSpaceToSolidColorBrushConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is ColorSpace colorSpace)
				return new SolidColorBrush(colorSpace.ToSystemWindowsMediaColor());
			if (value is string str)
				return new SolidColorBrush(ColorSpace.Parse(str).ToSystemWindowsMediaColor());
			throw new ArgumentException("Unable to convert specified object to SolidColorBrush");
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is SolidColorBrush brush)
			{
				var result = brush.Color.ToColorSpaceRgba();
				if (targetType == typeof(string))
					return result.ToString();
				return result;
			}

			throw new ArgumentException("Unable to convert specified object to ColorSpace");
		}
	}

	public class ColorSpaceToOverlaySolidColorBrushConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			ColorSpace colorSpace;
			switch (value)
			{
				case ColorSpace cs:
					colorSpace = cs;
					break;
				case string str:
					colorSpace = ColorSpace.Parse(str);
					break;
				default:
					throw new ArgumentException("Unable to convert specified object to SolidColorBrush");
			}

			return new SolidColorBrush(colorSpace.GetAutoDarkenOrLighten().ToSystemWindowsMediaColor());
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new InvalidOperationException();
		}
	}
	#endregion
}