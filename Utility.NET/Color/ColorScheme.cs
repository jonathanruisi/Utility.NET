// ┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  FILE:       ColorScheme.cs
// ┃  PROJECT:    Utility.NET
// ┃  SOLUTION:   Utility.NET
// ┃  CREATED:    2016-01-04 @ 7:12 AM
// ┣━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  AUTHOR:     Jonathan Ruisi
// ┃  EMAIL:      JonathanRuisi@gmail.com
// ┣━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  GIT REPO:   https://github.com/jonathanruisi/Utility.NET
// ┃  LICENSE:    https://github.com/jonathanruisi/Utility.NET/blob/master/LICENSE
// ┗━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace JLR.Utility.NET.Color
{
	public sealed class ColorScheme : ICollection<ColorSpace>
	{
		#region Fields
		private readonly Dictionary<ColorSpace, int> _colorDictionary;
		private          int                         _maxUsageLevel;
		#endregion

		#region Properties
		public IEnumerable<ColorSpace> AvailableColors =>
			from color in _colorDictionary where color.Value < _maxUsageLevel select color.Key;

		public IEnumerable<ColorSpace> UsedColors =>
			from color in _colorDictionary where color.Value == _maxUsageLevel select color.Key;
		#endregion

		#region Static Properties
		public static ColorScheme BlackOnlyScheme => new ColorScheme(ColorSpace.OpaqueBlack);

		public static ColorScheme BasicRgbScheme =>
			new ColorScheme(new Rgb(255, 0, 0), new Rgb(0, 255, 0), new Rgb(0, 0, 255));
		#endregion

		#region Constructors
		public ColorScheme(params ColorSpace[] colors) : this()
		{
			if (colors == null || colors.Length == 0)
				throw new ArgumentException("At least one color must be specified", nameof(colors));
			foreach (var color in colors)
			{
				_colorDictionary.Add(color, 0);
			}
		}

		public ColorScheme(StringCollection colorStrings) : this()
		{
			if (colorStrings == null || colorStrings.Count == 0)
				throw new ArgumentException("At least one color must be specified", nameof(colorStrings));
			foreach (var colorString in colorStrings)
			{
				_colorDictionary.Add(ColorSpace.Parse(colorString), 0);
			}
		}

		private ColorScheme()
		{
			_colorDictionary = new Dictionary<ColorSpace, int>();
			_maxUsageLevel   = 1;
		}
		#endregion

		#region Public Methods
		public ColorSpace GetNextAvailableColor()
		{
			var result = AvailableColors.FirstOrDefault();
			if (result == null)
			{
				_maxUsageLevel++;
				return GetNextAvailableColor();
			}

			_colorDictionary[result] = _maxUsageLevel;
			return result;
		}

		public void ReturnColor(ColorSpace color)
		{
			if (_colorDictionary.ContainsKey(color))
				_colorDictionary[color] = _maxUsageLevel - 1;

			if (_maxUsageLevel > 0 && _colorDictionary.Values.Min() < _maxUsageLevel)
				_maxUsageLevel--;
		}

		public StringCollection ToStringCollection()
		{
			var result = new StringCollection();
			foreach (var color in _colorDictionary.Keys)
			{
				result.Add(color.ToString());
			}

			return result;
		}
		#endregion

		#region Interface Implementation (ICollection<T>)
		public bool IsReadOnly => false;
		public int  Count      => _colorDictionary.Count;

		public void Add(ColorSpace color)
		{
			if (!_colorDictionary.ContainsKey(color))
				_colorDictionary.Add(color, _maxUsageLevel - 1);
		}

		public bool Remove(ColorSpace color)
		{
			return _colorDictionary.Remove(color);
		}

		public void Clear()
		{
			_colorDictionary.Clear();
		}

		public bool Contains(ColorSpace color)
		{
			return _colorDictionary.ContainsKey(color);
		}

		public void CopyTo(ColorSpace[] array, int arrayIndex)
		{
			if (array.Length < Count)
				throw new ArgumentException(
					"The destination array is not large enough to copy the contents of this collection",
					nameof(array));
			if (array.Length - arrayIndex > Count)
				throw new ArgumentOutOfRangeException(
					nameof(arrayIndex),
					$"There is not enough room in the destination array if copying begins at index {arrayIndex}");

			var colors = _colorDictionary.Keys.ToList();
			for (var i = 0; i < Count; i++)
			{
				array[arrayIndex + i] = colors[i];
			}
		}

		public IEnumerator<ColorSpace> GetEnumerator()
		{
			return _colorDictionary.Keys.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
		#endregion
	}
}