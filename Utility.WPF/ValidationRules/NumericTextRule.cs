// ┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  FILE:       NumericTextRule.cs
// ┃  PROJECT:    Utility.WPF
// ┃  SOLUTION:   Utility.NET
// ┃  CREATED:    2015-12-25 @ 1:20 AM
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
	public class NumericTextRule : ValidationRule
	{
		#region Fields
		private double _minimum, _maximum, _increment;
		#endregion

		#region Properties
		public NumberStyles NumberStyles { get; set; }

		public double MinimumNumber
		{
			get { return _minimum; }
			set
			{
				ValidateMinimum(value, _maximum, _increment);
				_minimum = value;
			}
		}

		public double MaximumNumber
		{
			get { return _maximum; }
			set
			{
				ValidateMaximum(value, _minimum, _increment);
				_maximum = value;
			}
		}

		public double NumberIncrement
		{
			get { return _increment; }
			set
			{
				ValidateIncrement(value, _minimum, _maximum);
				_increment = value;
			}
		}
		#endregion

		#region Constructor
		public NumericTextRule() : this(Double.MinValue, Double.MaxValue, 0.0) { }

		public NumericTextRule(double minimum, double maximum, double increment)
		{
			ValidateMinimum(minimum, maximum, increment);
			ValidateMaximum(maximum, minimum, increment);
			ValidateIncrement(increment, minimum, maximum);
			_minimum   = minimum;
			_maximum   = maximum;
			_increment = increment;
		}
		#endregion

		#region Public Methods
		public override ValidationResult Validate(object value, CultureInfo cultureInfo)
		{
			var valueString = value as string;
			if (String.IsNullOrEmpty(valueString))
				return new ValidationResult(false, "No text entered");

			double valueDouble;
			if (!Double.TryParse(valueString, NumberStyles, cultureInfo, out valueDouble))
				return new ValidationResult(false, "Contains illegal characters");

			if (valueDouble < _minimum || valueDouble > _maximum)
				return new ValidationResult(false, $"Value must be between {_minimum} and {_maximum}");

			if (!_increment.Equals(0.0) && !((valueDouble - _minimum) % _increment).Equals(0))
				return new ValidationResult(
					false,
					$"Value must be between {_minimum} and {_maximum} in increments of {_increment}");

			return new ValidationResult(true, null);
		}
		#endregion

		#region Private Methods
		private static void ValidateMinimum(double value, double maximum, double increment)
		{
			if (Double.IsNaN(value) || Double.IsInfinity(value))
				throw new ArgumentException("Minimum value must be a non-infinite, valid number", nameof(value));
			if (value > maximum)
				throw new ArgumentOutOfRangeException(nameof(value), "Minimum value cannot be greater than Maximum value");
			if (maximum - value < increment && increment > 0)
				throw new ArgumentException(
					"The maximum-minimum range cannot be smaller than the specified increment",
					nameof(value));
		}

		private static void ValidateMaximum(double value, double minimum, double increment)
		{
			if (Double.IsNaN(value) || Double.IsInfinity(value))
				throw new ArgumentException("Maximum value must be a non-infinite, valid number", nameof(value));
			if (value < minimum)
				throw new ArgumentOutOfRangeException(nameof(value), "Maximum value cannot be less than Minimum value");
			if (value - minimum < increment && increment > 0)
				throw new ArgumentException(
					"The maximum-minimum range cannot be smaller than the specified increment",
					nameof(value));
		}

		private static void ValidateIncrement(double value, double minimum, double maximum)
		{
			if (Double.IsNaN(value) || Double.IsInfinity(value))
				throw new ArgumentException("Increment value must be a non-infinite, valid number", nameof(value));
			if (value < 0)
				throw new ArgumentOutOfRangeException(nameof(value), "Increment cannot be less than zero");
			if (maximum - minimum < value && value > 0)
				throw new ArgumentException(
					"The specified increment cannot be larger than the maximum-minimum range",
					nameof(value));
		}
		#endregion
	}
}