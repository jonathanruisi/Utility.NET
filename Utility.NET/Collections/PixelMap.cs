// ┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  FILE:       PixelMap.cs
// ┃  PROJECT:    Utility.NET
// ┃  SOLUTION:   Utility.NET
// ┃  CREATED:    2016-01-04 @ 4:09 AM
// ┣━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  AUTHOR:     Jonathan Ruisi
// ┃  EMAIL:      JonathanRuisi@gmail.com
// ┣━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  GIT REPO:   https://github.com/jonathanruisi/Utility.NET
// ┃  LICENSE:    https://github.com/jonathanruisi/Utility.NET/blob/master/LICENSE
// ┗━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

using System;
using System.Drawing;
using System.Globalization;
using System.Text;
using System.Xml.Linq;

using JLR.Utility.NET.Math;
using JLR.Utility.NET.Xml;

namespace JLR.Utility.NET.Collections
{
	public class PixelMap : IEquatable<PixelMap>, IXNode<XElement>, ICloneable
	{
		#region Fields
		private int          _width, _height;
		private BitArray64[] _pixels;
		#endregion

		#region Properties
		public string NodeName => "PixelMap";
		public int    Width    => _width;
		public int    Height   => _height;
		public Size   Size     { get { return new Size(_width, _height); } set { Resize(value.Width, value.Height); } }

		public decimal SetPercentage
		{
			get
			{
				decimal result = 0;
				for (var r = 0; r < _height; r++)
				{
					for (var c = 0; c < _width; c++)
					{
						if (_pixels[r][c])
							result++;
					}
				}

				return result / (_width * _height);
			}
		}
		#endregion

		#region Indexers
		public ulong[] this[int row]
		{
			get { return _pixels[row].ToUInt64Array(); }
			set { _pixels[row] = new BitArray64(value); }
		}

		public bool this[int row, int column] { get { return _pixels[row][column]; } set { _pixels[row][column] = value; } }

		public PixelMap this[int row, int column, int width, int height]
		{
			get { return GetArea(row, column, width, height); }
			set { SetArea(value, row, column, width, height); }
		}
		#endregion

		#region Constructors
		public PixelMap(Size size) : this(size.Width, size.Height) { }

		public PixelMap(int width, int height)
		{
			if (width < 1 || height < 1)
				throw new ArgumentException("Width and height must each be greater than zero");

			_width  = width;
			_height = height;
			_pixels = new BitArray64[height];
			for (var i = 0; i < height; i++)
			{
				_pixels[i] = new BitArray64(width);
			}
		}

		private PixelMap(PixelMap pixelMap)
		{
			if (pixelMap == null)
				throw new ArgumentNullException(nameof(pixelMap));

			_width  = pixelMap.Width;
			_height = pixelMap.Height;
			_pixels = new BitArray64[pixelMap.Height];
			for (var i = 0; i < _pixels.Length; i++)
			{
				_pixels[i] = new BitArray64(pixelMap._pixels[i]);
			}
		}
		#endregion

		#region Public Methods (Row/Column)
		public BitArray64 GetRow(int row)
		{
			ValidateRow(row);
			return _pixels[row];
		}

		public BitArray64 GetColumn(int column)
		{
			ValidateColumn(column);
			var result = new BitArray64(_height);
			for (var r = 0; r < _height; r++)
			{
				result[r] = _pixels[r][column];
			}

			return result;
		}

		public void SetRow(BitArray64 source, int row)
		{
			ValidateRow(row);
			var width = MathHelper.Min(_width, source.Length);
			for (var c = 0; c < width; c++)
			{
				_pixels[row][c] = source[c];
			}
		}

		public void SetColumn(BitArray64 source, int column)
		{
			ValidateColumn(column);
			var height = MathHelper.Min(_height, source.Length);
			for (var r = 0; r < height; r++)
			{
				_pixels[r][column] = source[r];
			}
		}
		#endregion

		#region Public Methods (Area)
		public PixelMap GetArea(Rectangle area)
		{
			return GetArea(area.Y, area.X, area.Width, area.Height);
		}

		public PixelMap GetArea(Point location, Size size)
		{
			return GetArea(location.Y, location.X, size.Width, size.Height);
		}

		public PixelMap GetArea(int row, int column, int width, int height)
		{
			ValidateRow(row);
			ValidateColumn(column);
			ValidateRowSegment(column, width);
			ValidateColumnSegment(row, height);

			var result = new PixelMap(width, height);
			for (var r = 0; r < height; r++)
			{
				for (var c = 0; c < width; c++)
				{
					result[r, c] = _pixels[r][c];
				}
			}

			return result;
		}

		public void SetArea(PixelMap source, int row, int column)
		{
			var width  = MathHelper.Min(_width - column, source._width);
			var height = MathHelper.Min(_height - row,   source._height);
			SetArea(source, row, column, width, height);
		}

		public void SetArea(PixelMap source, Rectangle area, Point sourceLocation)
		{
			SetArea(source, area.Y, area.X, area.Width, area.Height, sourceLocation.Y, sourceLocation.X);
		}

		public void SetArea(PixelMap source, Point location, Size size, Point sourceLocation)
		{
			SetArea(source, location.Y, location.X, size.Width, size.Height, sourceLocation.Y, sourceLocation.X);
		}

		public void SetArea(PixelMap source,
							int row,
							int column,
							int width,
							int height,
							int sourceRow = 0,
							int sourceColumn = 0)
		{
			if (source == null)
				throw new ArgumentNullException(nameof(source));

			ValidateRow(row);
			ValidateColumn(column);
			ValidateRowSegment(column, width);
			ValidateColumnSegment(row, height);
			source.ValidateRow(sourceRow);
			source.ValidateColumn(sourceColumn);
			source.ValidateRowSegment(sourceColumn, width);
			source.ValidateColumnSegment(sourceRow, height);

			for (var r = 0; r < height; r++)
			{
				for (var c = 0; c < width; c++)
				{
					_pixels[row + r][column + c] = source._pixels[sourceRow + r][sourceColumn + c];
				}
			}
		}

		public void SetArea(Rectangle area, bool? state)
		{
			SetArea(area.Y, area.X, area.Width, area.Height, state);
		}

		public void SetArea(Point origin, Size size, bool? state)
		{
			SetArea(origin.Y, origin.X, size.Width, size.Height, state);
		}

		public void SetArea(int row, int column, int width, int height, bool? state)
		{
			if (row == 0 && column == 0 && width == _width && height == _height)
			{
				if (state == null)
				{
					for (var i = 0; i < _height; i++)
					{
						_pixels[i].Not();
					}
				}
				else
				{
					for (var i = 0; i < _height; i++)
					{
						_pixels[i].SetAll((bool)state);
					}
				}
			}
			else
			{
				ValidateRow(row);
				ValidateColumn(column);
				ValidateRowSegment(column, width);
				ValidateColumnSegment(row, height);

				for (var r = 0; r < height; r++)
				{
					for (var c = 0; c < width; c++)
					{
						if (state == null)
							_pixels[r][c] = !_pixels[r][c];
						else
							_pixels[r][c] = (bool)state;
					}
				}
			}
		}
		#endregion

		#region Public Methods (Transformation)
		public void Resize(int newWidth, int newHeight)
		{
			Transform(Rectangle.FromLTRB(0, 0, newWidth, newHeight));
		}

		public void Transform(int top, int bottom, int left, int right)
		{
			Transform(Rectangle.FromLTRB(0 - left, 0 - top, _width + right, _height + bottom));
		}

		public void Transform(Rectangle relativeRectangle)
		{
			if (relativeRectangle.Width <= 0 || relativeRectangle.Height <= 0)
				throw new ArgumentException("The size of the specified transformation rectangle must be greater than zero");

			// Determine the size and location of the transformed grid
			var overlapRectangle = Rectangle.Intersect(Rectangle.FromLTRB(0, 0, _width, _height), relativeRectangle);

			//if (overlapRectangle == relativeRectangle)
			//return;

			// Initialize the new pixel matrix
			var newPixels = new BitArray64[relativeRectangle.Height];
			for (var i = 0; i < relativeRectangle.Height; i++)
			{
				newPixels[i] = new BitArray64(relativeRectangle.Width);
			}

			// If the transformation overlaps its original size and position,
			// copy the pixels in the overlapping area
			if (overlapRectangle.Size != Size.Empty)
			{
				for (var r = 0; r < overlapRectangle.Height; r++)
				{
					for (var c = 0; c < overlapRectangle.Width; c++)
					{
						newPixels[r][c] = _pixels[overlapRectangle.Y + r][overlapRectangle.X + c];
					}
				}
			}

			_width  = relativeRectangle.Width;
			_height = relativeRectangle.Height;
			_pixels = newPixels;
		}
		#endregion

		#region Public Methods (Whole-Map Operations)
		public void Clear(bool state)
		{
			SetArea(0, 0, _width, _height, state);
		}

		public void Invert()
		{
			SetArea(0, 0, _width, _height, null);
		}

		public void And(PixelMap other)
		{
			if (Size != other.Size)
				throw new ArgumentException("The specified pixel map must be of equal size");

			for (var i = 0; i < _height; i++)
			{
				_pixels[i].And(other._pixels[i]);
			}
		}

		public void Or(PixelMap other)
		{
			if (Size != other.Size)
				throw new ArgumentException("The specified pixel map must be of equal size");

			for (var i = 0; i < _height; i++)
			{
				_pixels[i].Or(other._pixels[i]);
			}
		}

		public void XOr(PixelMap other)
		{
			if (Size != other.Size)
				throw new ArgumentException("The specified pixel map must be of equal size");

			for (var i = 0; i < _height; i++)
			{
				_pixels[i].Xor(other._pixels[i]);
			}
		}
		#endregion

		#region Public Methods (Conversion)
		public byte[,] ToArray8(RowOrColumn interpretBy, Endianness endianness)
		{
			// Calculate array dimensions
			var majorDimension = interpretBy == RowOrColumn.Row ? _height : _width;
			var minorDimension = ((interpretBy == RowOrColumn.Row ? _width : _height) - 1) / 8 + 1;
			var result         = new byte[majorDimension, minorDimension];

			// Generate array
			for (var i = 0; i < majorDimension; i++)
			{
				var bytes = interpretBy == RowOrColumn.Row ? GetRow(i).ToByteArray() : GetColumn(i).ToByteArray();
				for (var j = 0; j < minorDimension; j++)
				{
					result[i, j] = endianness == Endianness.Little ? bytes[j] : bytes[minorDimension - 1 - j];
				}
			}

			return result;
		}

		public ushort[,] ToArray16(RowOrColumn interpretBy, Endianness endianness)
		{
			// Calculate array dimensions
			var majorDimension = interpretBy == RowOrColumn.Row ? _height : _width;
			var minorDimension = ((interpretBy == RowOrColumn.Row ? _width : _height) - 1) / 16 + 1;
			var result         = new ushort[majorDimension, minorDimension];

			// Generate array
			for (var i = 0; i < majorDimension; i++)
			{
				var bytes = interpretBy == RowOrColumn.Row ? GetRow(i).ToByteArray() : GetColumn(i).ToByteArray();
				for (var j = 0; j < minorDimension; j++)
				{
					if (bytes.Length < 2)
						result[i, j] = endianness == Endianness.Little ? bytes[j] : bytes[minorDimension - 1 - j];
					else
						result[i, j] = endianness == Endianness.Little
							? BitConverter.ToUInt16(bytes, j * 2)
							: BitConverter.ToUInt16(bytes, (minorDimension - 1) * 2 - j * 2);
				}
			}

			return result;
		}

		public uint[,] ToArray32(RowOrColumn interpretBy, Endianness endianness)
		{
			// Calculate array dimensions
			var majorDimension = interpretBy == RowOrColumn.Row ? _height : _width;
			var minorDimension = ((interpretBy == RowOrColumn.Row ? _width : _height) - 1) / 32 + 1;
			var result         = new uint[majorDimension, minorDimension];

			// Generate array
			for (var i = 0; i < majorDimension; i++)
			{
				var bytes = interpretBy == RowOrColumn.Row ? GetRow(i).ToByteArray() : GetColumn(i).ToByteArray();
				for (var j = 0; j < minorDimension; j++)
				{
					if (bytes.Length < 4)
						result[i, j] = endianness == Endianness.Little ? bytes[j] : bytes[minorDimension - 1 - j];
					else
						result[i, j] = endianness == Endianness.Little
							? BitConverter.ToUInt16(bytes, j * 4)
							: BitConverter.ToUInt16(bytes, (minorDimension - 1) * 4 - j * 4);
				}
			}

			return result;
		}

		public ulong[,] ToArray64(RowOrColumn interpretBy, Endianness endianness)
		{
			// Calculate array dimensions
			var majorDimension = interpretBy == RowOrColumn.Row ? _height : _width;
			var minorDimension = ((interpretBy == RowOrColumn.Row ? _width : _height) - 1) / 64 + 1;
			var result         = new ulong[majorDimension, minorDimension];

			// Generate array
			for (var i = 0; i < majorDimension; i++)
			{
				var bytes = interpretBy == RowOrColumn.Row ? GetRow(i).ToByteArray() : GetColumn(i).ToByteArray();
				for (var j = 0; j < minorDimension; j++)
				{
					if (bytes.Length < 8)
						result[i, j] = endianness == Endianness.Little ? bytes[j] : bytes[minorDimension - 1 - j];
					else
						result[i, j] = endianness == Endianness.Little
							? BitConverter.ToUInt16(bytes, j * 8)
							: BitConverter.ToUInt16(bytes, (minorDimension - 1) * 8 - j * 8);
				}
			}

			return result;
		}
		#endregion

		#region Public Methods (Static)
		public static PixelMap Checkerboard(int width, int height, bool firstPixelState)
		{
			if (width <= 0 || height <= 0)
				throw new ArgumentException("Both width and height must be greater than zero");

			var result = new PixelMap(width, height);
			for (var r = 0; r < height; r++)
			{
				for (var c = 0; c < width; c++)
				{
					if ((r % 2 == (firstPixelState ? 0 : 1) && c % 2 == 0) || (r % 2 == (firstPixelState ? 1 : 0) && c % 2 == 1))
						result[r, c] = true;
				}
			}

			return result;
		}
		#endregion

		#region Private Methods
		private void ValidateRow(int row)
		{
			if (row < 0 || row >= _height)
				throw new ArgumentOutOfRangeException(nameof(row), $"The specified row must be between 0 and {_height - 1}");
		}

		private void ValidateRowSegment(int startColumn, int width)
		{
			ValidateColumn(startColumn);
			if (width <= 0 || startColumn + width > _width)
				throw new ArgumentException(
					$"The specified segment width must be between 1 and {_width - startColumn} based on a start column of {startColumn}");
		}

		private void ValidateColumn(int column)
		{
			if (column < 0 || column >= _width)
				throw new ArgumentOutOfRangeException(nameof(column), $"The specified column must be between 0 and {_width - 1}");
		}

		private void ValidateColumnSegment(int startRow, int height)
		{
			ValidateRow(startRow);
			if (height <= 0 || startRow + height > _height)
				throw new ArgumentException(
					$"The specified segment height must be between 1 and {_height - startRow} based on a start row of {startRow}");
		}
		#endregion

		#region Interface Implementation (IXNode<XElement>)
		public XElement ToXNode()
		{
			var element = new XElement("PixelMap");
			element.Add(new XAttribute("Width",  _width));
			element.Add(new XAttribute("Height", _height));

			// Generate value string
			var valueString = new StringBuilder();
			for (var i = 0; i < _height; i++)
			{
				var row = _pixels[i].ToUInt32Array();
				for (var j = 0; j < row.Length; j++)
				{
					valueString.Append(row[j].ToString(CultureInfo.InvariantCulture));
					if (j < row.Length - 1)
						valueString.Append(',');
				}

				if (i < _height - 1)
					valueString.Append('|');
			}

			element.Add(valueString.ToString());
			return element;
		}

		public void FromXNode(XElement element)
		{
			_width  = Int32.Parse(element.Attribute("Width").Value);
			_height = Int32.Parse(element.Attribute("Height").Value);
			_pixels = new BitArray64[_height];

			// Parse value string
			var rowStrings = element.Value.Split('|');
			for (var i = 0; i < _height; i++)
			{
				var rowIntString = rowStrings[i].Split(',');
				var rowValue     = new ulong[rowIntString.Length];
				for (var j = 0; j < rowIntString.Length; j++)
				{
					rowValue[j] = UInt64.Parse(rowIntString[j]);
				}

				_pixels[i] = new BitArray64(rowValue);
			}
		}
		#endregion

		#region Interface Implementation (IEquatable<PixelMapEx>)
		public bool Equals(PixelMap other)
		{
			if (other == null || Size != other.Size)
				return false;

			for (var r = 0; r < _height; r++)
			{
				for (var c = 0; c < _width; c++)
				{
					if (_pixels[r][c] != other._pixels[r][c])
						return false;
				}
			}

			return true;
		}
		#endregion

		#region Interface Implementation (ICloneable)
		public object Clone()
		{
			return new PixelMap(this);
		}
		#endregion

		#region Method Overrides (System.Object)
		public override string ToString()
		{
			var result = new StringBuilder();
			for (var r = 0; r < _height; r++)
			{
				for (var c = 0; c < _width; c++)
				{
					result.Append(_pixels[r][c] ? '▓' : '░');
				}

				result.AppendLine();
			}

			return result.ToString();
		}
		#endregion

		#region Operator Overloads
		public static PixelMap operator ~(PixelMap value)
		{
			var result = new PixelMap(value);
			result.Invert();
			return result;
		}

		public static PixelMap operator &(PixelMap lhs, PixelMap rhs)
		{
			var result = new PixelMap(lhs);
			result.And(rhs);
			return result;
		}

		public static PixelMap operator |(PixelMap lhs, PixelMap rhs)
		{
			var result = new PixelMap(lhs);
			result.Or(rhs);
			return result;
		}

		public static PixelMap operator ^(PixelMap lhs, PixelMap rhs)
		{
			var result = new PixelMap(lhs);
			result.XOr(rhs);
			return result;
		}
		#endregion
	}
}