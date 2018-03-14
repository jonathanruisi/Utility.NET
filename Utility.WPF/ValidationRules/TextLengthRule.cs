// ┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  FILE:       TextLengthRule.cs
// ┃  PROJECT:    Utility.WPF
// ┃  SOLUTION:   Utility.NET
// ┃  CREATED:    2015-12-25 @ 1:18 AM
// ┣━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  AUTHOR:     Jonathan Ruisi
// ┃  EMAIL:      JonathanRuisi@gmail.com
// ┣━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  GIT REPO:   https://github.com/jonathanruisi/Utility.NET
// ┃  LICENSE:    https://github.com/jonathanruisi/Utility.NET/blob/master/LICENSE
// ┗━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

using System;
using System.Globalization;
using System.Windows.Controls;

namespace JLR.Utility.WPF.ValidationRules
{
	public class TextLengthRule : ValidationRule
	{
		#region Fields
		private int _minimum, _maximum;
		#endregion

		#region Properties
		public int Minimum
		{
			get { return _minimum; }
			set
			{
				ValidateMinimum(value, _maximum);
				_minimum = value;
			}
		}

		public int Maximum
		{
			get { return _maximum; }
			set
			{
				ValidateMaximum(value, _minimum);
				_maximum = value;
			}
		}
		#endregion

		#region Constructor
		public TextLengthRule() : this(0, Int32.MaxValue) { }

		public TextLengthRule(int minimum, int maximum)
		{
			ValidateMinimum(minimum, maximum);
			ValidateMaximum(maximum, minimum);
			_minimum = minimum;
			_maximum = maximum;
		}
		#endregion

		#region Public Methods
		public override ValidationResult Validate(object value, CultureInfo cultureInfo)
		{
			var valueString = value as string;
			if (String.IsNullOrEmpty(valueString))
			{
				if (_minimum > 0)
					return new ValidationResult(false, "Must contain text");
			}
			else
			{
				if (_minimum == _maximum && valueString.Length != _minimum)
					return new ValidationResult(
						false,
						$"Text must be exactly {_minimum} character{(_minimum == 1 ? String.Empty : "s")} in length");
				if (valueString.Length < _minimum || valueString.Length > _maximum)
					return new ValidationResult(false, $"Text must be between {_minimum} and {_maximum} characters in length");
			}

			return new ValidationResult(true, null);
		}
		#endregion

		#region Private Methods
		private static void ValidateMinimum(int value, int maximum)
		{
			if (value < 0)
				throw new ArgumentOutOfRangeException(nameof(value), "Minimum value cannot be less than zero");
			if (value > maximum)
				throw new ArgumentOutOfRangeException(nameof(value), "Minimum value cannot be greater than Maximum value");
		}

		private static void ValidateMaximum(int value, int minimum)
		{
			if (value < 0)
				throw new ArgumentOutOfRangeException(nameof(value), "Maximum value cannot be less than zero");
			if (value < minimum)
				throw new ArgumentOutOfRangeException(nameof(value), "Maximum value cannot be less than Minimum value");
		}
		#endregion
	}
}