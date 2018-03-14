// ┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  FILE:       BitArray64.cs
// ┃  PROJECT:    Utility.NET
// ┃  SOLUTION:   Utility.NET
// ┃  CREATED:    2015-12-23 @ 5:00 AM
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
using System.Globalization;
using System.Text;

namespace JLR.Utility.NET.Collections
{
	// TODO: Document!!!
	public class BitArray64 : IEnumerable<bool>, ICloneable, IFormattable
	{
		#region Fields
		private const int     BitsPerByte  = 8;
		private const int     BitsPerLong  = 64;
		private const int     BytesPerLong = 8;
		private       ulong[] _data;
		private       int     _length, _version;
		#endregion

		#region Properties
		public int Length
		{
			get { return _length; }
			set
			{
				if (value < 0)
					throw new ArgumentOutOfRangeException(nameof(value), "Array length cannot be negative");
				if (value == _length)
					return;

				var arrayLength = DetermineInternalArrayLength(value, 1);
				if (arrayLength != _data.Length)
				{
					var newData = new ulong[arrayLength];
					Array.Copy(_data, newData, arrayLength > _data.Length ? _data.Length : arrayLength);
					_data = newData;
				}

				if (value < _length)
					_data[_data.Length - 1] &= 0xFFFFFFFFFFFFFFFFUL >> (BitsPerLong - value % BitsPerLong);
				_length = value;
				_version++;
			}
		}

		public bool this[int index]
		{
			get
			{
				if (index < 0 || index >= _length)
					throw new ArgumentOutOfRangeException(nameof(index));
				return (_data[index / BitsPerLong] & 1UL << index % BitsPerLong) != 0;
			}
			set
			{
				if (index < 0 || index >= _length)
					throw new ArgumentOutOfRangeException(nameof(index));
				if (value)
					_data[index / BitsPerLong] |= 1UL << index % BitsPerLong;
				else
					_data[index / BitsPerLong] &= ~(1UL << index % BitsPerLong);
				_version++;
			}
		}
		#endregion

		#region Properties (Private)
		private int AlignmentOffset => _length % BitsPerLong;
		#endregion

		#region Constructors
		public BitArray64(BitArray64 bits)
		{
			if (bits == null)
				throw new ArgumentNullException(nameof(bits));

			var arrayLength = DetermineInternalArrayLength(bits._length, 1);
			_data   = new ulong[arrayLength];
			_length = bits._length;
			Array.Copy(bits._data, _data, arrayLength);
			_version = bits._version;
		}

		public BitArray64(int length, bool defaultValue = false)
		{
			if (length < 0)
				throw new ArgumentOutOfRangeException(nameof(length), "Specified length cannot be negative");

			_data   = new ulong[DetermineInternalArrayLength(length, 1)];
			_length = length;
			var num = defaultValue ? ulong.MaxValue : 0;
			for (var i = 0; i < _data.Length; i++)
			{
				_data[i] = num;
			}

			_version = 0;
		}

		public BitArray64(bool[] values)
		{
			if (values == null)
				throw new ArgumentNullException(nameof(values));

			_data   = new ulong[DetermineInternalArrayLength(values.Length, 1)];
			_length = values.Length;
			for (var i = 0; i < values.Length; i++)
			{
				if (values[i])
					_data[i / BitsPerLong] |= 1UL << (i % BitsPerLong);
			}

			_version = 0;
		}

		public BitArray64(byte[] values)
		{
			if (values == null)
				throw new ArgumentNullException(nameof(values));
			if (values.Length > 0xFFFFFFF)
				throw new ArgumentException("The specified array is too large");

			_data   = new ulong[DetermineInternalArrayLength(values.Length, BitsPerByte)];
			_length = values.Length * BitsPerByte;
			var index     = 0;
			var byteIndex = 0;
			while (values.Length - byteIndex >= BytesPerLong)
			{
				_data[index++] = (values[byteIndex] & 0xFFUL) | (values[byteIndex + 1] & 0xFFUL) << 8 |
					(values[byteIndex + 2] & 0xFFUL) << 16 | (values[byteIndex + 3] & 0xFFUL) << 24 |
					(values[byteIndex + 4] & 0xFFUL) << 32 | (values[byteIndex + 5] & 0xFFUL) << 40 |
					(values[byteIndex + 6] & 0xFFUL) << 48 | (values[byteIndex + 7] & 0xFFUL) << 56;
				byteIndex += BytesPerLong;
			}

			switch (values.Length - byteIndex)
			{
				case 7:
					_data[index] = (values[byteIndex] & 0xFFUL) | (values[byteIndex + 1] & 0xFFUL) << 8 |
						(values[byteIndex + 2] & 0xFFUL) << 16 | (values[byteIndex + 3] & 0xFFUL) << 24 |
						(values[byteIndex + 4] & 0xFFUL) << 32 | (values[byteIndex + 5] & 0xFFUL) << 40 |
						(values[byteIndex + 6] & 0xFFUL) << 48;
					break;
				case 6:
					_data[index] = (values[byteIndex] & 0xFFUL) | (values[byteIndex + 1] & 0xFFUL) << 8 |
						(values[byteIndex + 2] & 0xFFUL) << 16 | (values[byteIndex + 3] & 0xFFUL) << 24 |
						(values[byteIndex + 4] & 0xFFUL) << 32 | (values[byteIndex + 5] & 0xFFUL) << 40;
					break;
				case 5:
					_data[index] = (values[byteIndex] & 0xFFUL) | (values[byteIndex + 1] & 0xFFUL) << 8 |
						(values[byteIndex + 2] & 0xFFUL) << 16 | (values[byteIndex + 3] & 0xFFUL) << 24 |
						(values[byteIndex + 4] & 0xFFUL) << 32;
					break;
				case 4:
					_data[index] = (values[byteIndex] & 0xFFUL) | (values[byteIndex + 1] & 0xFFUL) << 8 |
						(values[byteIndex + 2] & 0xFFUL) << 16 | (values[byteIndex + 3] & 0xFFUL) << 24;
					break;
				case 3:
					_data[index] = (values[byteIndex] & 0xFFUL) | (values[byteIndex + 1] & 0xFFUL) << 8 |
						(values[byteIndex + 2] & 0xFFUL) << 16;
					break;
				case 2:
					_data[index] = (values[byteIndex] & 0xFFUL) | (values[byteIndex + 1] & 0xFFUL) << 8;
					break;
				case 1:
					_data[index] = values[byteIndex] & 0xFFUL;
					break;
			}

			_version = 0;
		}

		public BitArray64(ulong[] values)
		{
			if (values == null)
				throw new ArgumentNullException(nameof(values));
			if (values.Length > 0x3FFFFFF)
				throw new ArgumentException("The specified array is too large");

			_data   = new ulong[values.Length];
			_length = values.Length * BitsPerLong;
			Array.Copy(values, _data, values.Length);
			_version = 0;
		}

		public BitArray64(string value)
		{
			if (string.IsNullOrEmpty(value))
				throw new ArgumentNullException(nameof(value));

			_data   = new ulong[DetermineInternalArrayLength(value.Length, 1)];
			_length = value.Length;
			for (var i = 0; i < value.Length; i++)
			{
				if (value[i] != '0' && value[i] != '1')
					throw new ArgumentException("The specified array string may only contain 1s and 0s");
				if (value[i] == '1')
					_data[i / BitsPerLong] |= 1UL << (i % BitsPerLong);
			}

			_version = 0;
		}

		public static BitArray64 FromBitArray(BitArray bits)
		{
			if (bits == null)
				throw new ArgumentNullException(nameof(bits));

			var byteArray = new byte[DetermineArrayLength(bits.Length, BitsPerByte)];
			bits.CopyTo(byteArray, 0);
			return new BitArray64(byteArray);
		}

		public static BitArray64 Random(int length)
		{
			var result = new BitArray64(length);
			result.Randomize();
			return result;
		}
		#endregion

		#region Public Methods (Manipulation)
		public void SetAll(bool value)
		{
			var elementValue = value ? ulong.MaxValue : 0;
			for (var i = 0; i < _data.Length; i++)
				_data[i] = elementValue;
			_version++;
		}

		public void Reverse()
		{
			for (var i = 0; i < _data.Length / 2; i++)
			{
				var temp = _data[i];
				_data[i]                    = BitwiseReverse(_data[_data.Length - i - 1]);
				_data[_data.Length - i - 1] = BitwiseReverse(temp);
			}

			if (_data.Length / 2 % 2 == 1)
				_data[_data.Length / 2] = BitwiseReverse(_data[_data.Length / 2]);
			_version++;
		}

		public void Randomize()
		{
			var rand = new Random();
			for (var i = 0; i < _length; i++)
			{
				this[i] = rand.Next(99) >= 50;
			}

			_version++;
		}

		public void ShiftLeft(int amount)
		{
			if (amount < 0)
				throw new ArgumentOutOfRangeException(nameof(amount), "Shift amount cannot be negative");
			if (amount == 0)
				return;
			if (amount >= _length)
			{
				SetAll(false);
				return;
			}

			ShiftLeftCore(_data, amount);
			CorrectUnalignedLeftShift(_data, AlignmentOffset);
			_version++;
		}

		public void ShiftRight(int amount)
		{
			if (amount < 0)
				throw new ArgumentOutOfRangeException(nameof(amount), "Shift amount cannot be negative");
			if (amount == 0)
				return;
			if (amount >= _length)
			{
				SetAll(false);
				return;
			}

			ShiftRightCore(_data, amount);
			_version++;
		}

		public void RotateLeft(int amount)
		{
			// Validate and/or adjust rotation amount and direction
			if (amount < 0)
				throw new ArgumentOutOfRangeException(nameof(amount), "Rotation amount cannot be negative");
			if (amount % _length == 0)
				return;
			if (amount / _length >= 1)
				amount %= _length;
			if (amount > _length / 2)
				RotateRight(amount - _length / 2);

			// Perform rotation using the method most appropriate for the array length and rotation amount
			if (_length <= BitsPerLong)
			{
				var mask       = (0xFFFFFFFFFFFFFFFFUL << BitsPerLong - amount) >> BitsPerLong - _length;
				var carryValue = (_data[0] & mask) >> _length - amount;
				_data[0] <<= amount;
				_data[0] ^=  carryValue;
				CorrectUnalignedLeftShift(_data, _length);
			}
			else if (amount <= BitsPerLong - AlignmentOffset && AlignmentOffset != 0)
			{
				ShiftLeftCore(_data, amount);
				var carryValue = _data[_data.Length - 1];
				CorrectUnalignedLeftShift(_data, AlignmentOffset);
				carryValue >>= AlignmentOffset;
				_data[0]   ^=  carryValue;
			}
			else
			{
				var majorAmount               = amount / BitsPerLong;
				var minorAmount               = amount % BitsPerLong;
				var alignmentOffsetRotational = System.Math.Abs(AlignmentOffset - minorAmount);
				var postRotationSpace         = majorAmount + (minorAmount > 0 ? 1 : 0);
				var preRotationalSpace        = postRotationSpace + (AlignmentOffset < minorAmount ? 1 : 0);

				var carryArray = new ulong[preRotationalSpace];
				Array.Copy(_data, _data.Length - preRotationalSpace, carryArray, 0, preRotationalSpace);
				ShiftLeftCore(_data, amount);
				CorrectUnalignedLeftShift(_data, AlignmentOffset);

				if (AlignmentOffset > minorAmount)
					ShiftRightCore(carryArray, alignmentOffsetRotational);
				else if (AlignmentOffset < minorAmount)
					ShiftLeftCore(carryArray, alignmentOffsetRotational);

				Array.Copy(carryArray, preRotationalSpace - postRotationSpace, _data, 0, majorAmount);
				_data[majorAmount] ^= carryArray[carryArray.Length - 1];
			}
		}

		public void RotateRight(int amount)
		{
			// Validate and/or adjust rotation amount and direction
			if (amount < 0)
				throw new ArgumentOutOfRangeException(nameof(amount), "Rotation amount cannot be negative");
			if (amount % _length == 0)
				return;
			if (amount / _length >= 1)
				amount %= _length;
			if (amount > _length / 2)
				RotateLeft(amount - _length / 2);

			// Perform rotation using the method most appropriate for the array length and rotation amount
			if (_length < BitsPerLong)
			{
				var mask       = 0xFFFFFFFFFFFFFFFFUL >> BitsPerLong - amount;
				var carryValue = (_data[0] & mask) << _length - amount;
				_data[0] >>= amount;
				_data[0] ^=  carryValue;
			}
			else if (amount <= AlignmentOffset)
			{
				var mask       = 0xFFFFFFFFFFFFFFFFUL >> BitsPerLong - amount;
				var carryValue = (_data[0] & mask) << AlignmentOffset - amount;
				ShiftRightCore(_data, amount);
				_data[_data.Length - 1] ^= carryValue;
			}
			else
			{
				var majorAmount               = amount / BitsPerLong;
				var minorAmount               = amount % BitsPerLong;
				var alignmentOffsetPrime      = BitsPerLong - AlignmentOffset;
				var alignmentOffsetRotational = System.Math.Abs(AlignmentOffset - minorAmount);
				var preRotationSpace          = majorAmount + (minorAmount > 0 ? 1 : 0);
				var postRotationSpace         = preRotationSpace + (AlignmentOffset < minorAmount ? 1 : 0);
				var mask1                     = 0xFFFFFFFFFFFFFFFFUL >> alignmentOffsetPrime;
				var mask2 = 0xFFFFFFFFFFFFFFFFUL << (AlignmentOffset < minorAmount
					? BitsPerLong - alignmentOffsetRotational
					: alignmentOffsetRotational);

				var carryArray = new ulong[postRotationSpace];
				Array.Copy(_data, 0, carryArray, carryArray.Length - preRotationSpace, preRotationSpace);
				ShiftRightCore(_data, amount);

				if (AlignmentOffset > minorAmount)
					ShiftLeftCore(carryArray, alignmentOffsetRotational);
				else if (AlignmentOffset < minorAmount)
					ShiftRightCore(carryArray, alignmentOffsetRotational);

				if (postRotationSpace > 2)
					Array.Copy(carryArray, 1, _data, _data.Length - postRotationSpace + 1, postRotationSpace - 2);

				_data[_data.Length - postRotationSpace] ^= mask2 & carryArray[0];
				_data[_data.Length - 1]                 ^= mask1 & carryArray[carryArray.Length - 1];
			}
		}

		public void TrimLeadingZeros()
		{
			var amount = GetLeadingZeroCount();
			Length -= amount;
		}

		public void TrimTrailingZeros()
		{
			var amount = GetTrailingZeroCount();
			ShiftRight(amount);
			Length -= amount;
		}

		public void TrimLeadingAndTrailingZeros()
		{
			ShiftRight(GetTrailingZeroCount());
			Length -= GetLeadingZeroCount();
		}
		#endregion

		#region Public Methods (Logic)
		public void And(BitArray64 value)
		{
			if (value == null)
				throw new ArgumentNullException(nameof(value));
			if (_length != value._length)
				throw new ArgumentException("The specified BitArray64 has a different length");

			for (var i = 0; i < _data.Length; i++)
			{
				_data[i] &= value._data[i];
			}

			_version++;
		}

		public void Or(BitArray64 value)
		{
			if (value == null)
				throw new ArgumentNullException(nameof(value));
			if (_length != value._length)
				throw new ArgumentException("The specified BitArray64 has a different length");

			for (var i = 0; i < _data.Length; i++)
			{
				_data[i] |= value._data[i];
			}

			_version++;
		}

		public void Xor(BitArray64 value)
		{
			if (value == null)
				throw new ArgumentNullException(nameof(value));
			if (_length != value._length)
				throw new ArgumentException("The specified BitArray64 has a different length");

			for (var i = 0; i < _data.Length; i++)
			{
				_data[i] ^= value._data[i];
			}

			_version++;
		}

		public void Not()
		{
			for (var i = 0; i < _data.Length; i++)
			{
				_data[i] = ~_data[i];
			}

			_version++;
		}
		#endregion

		#region Public Methods (Conversion)
		public ulong[] ToUInt64Array()
		{
			var result = new ulong[DetermineArrayLength(_length, BitsPerLong)];
			Array.Copy(_data, 0, result, 0, result.Length);
			return result;
		}

		public uint[] ToUInt32Array()
		{
			var result = new uint[DetermineArrayLength(_length, 32)];
			for (var i = 0; i < result.Length; i++)
			{
				result[i] = (uint)((_data[i / 2] >> (i % 2 * 32)) & 0xFFFFFFFFUL);
			}

			return result;
		}

		public ushort[] ToUInt16Array()
		{
			var result = new ushort[DetermineArrayLength(_length, 16)];
			for (var i = 0; i < result.Length; i++)
			{
				result[i] = (ushort)((_data[i / 4] >> (i % 4 * 16)) & 0xFFFFUL);
			}

			return result;
		}

		public byte[] ToByteArray()
		{
			var result = new byte[DetermineArrayLength(_length, BitsPerByte)];
			for (var i = 0; i < result.Length; i++)
			{
				result[i] = (byte)((_data[i / BytesPerLong] >> (i % BytesPerLong * BitsPerByte)) & 0xFFUL);
			}

			return result;
		}

		public bool[] ToBoolArray()
		{
			var result = new bool[_length];
			for (var i = 0; i < _length; i++)
			{
				result[i] = ((_data[i / BitsPerLong] >> (i % BitsPerLong)) & 0x01UL) != 0;
			}

			return result;
		}
		#endregion

		#region Public Methods (Analysis)
		/// <summary>
		/// Calculates the population of the current instance.
		/// Also known as "Hamming weight" or "sideways addition",
		/// this method efficiently counts the number of bits currently set.
		/// </summary>
		/// <returns>Number of bits currently set</returns>
		public int GetPopulation()
		{
			const ulong mask   = 0x0101010101010101;
			const ulong mask1  = ~0UL / 3 << 1;
			const ulong mask2  = ~0UL / 5;
			const ulong mask3  = ~0UL / 17;
			ulong       result = 0;

			foreach (var v in _data)
			{
				var value = v;
				value  -= (mask1 & value) >> 1;
				value  =  (value & mask2) + ((value >> 2) & mask2);
				value  += value >> 4;
				value  &= mask3;
				result += (value * mask) >> 56;
			}

			return (int)result;
		}

		public int GetLeadingZeroCount()
		{
			var count = 0;
			for (var i = _data.Length - 1; i >= 0; i--)
			{
				var value = _data[i];
				if (value == 0)
				{
					count += i == _data.Length - 1 ? AlignmentOffset : BitsPerLong;
					continue;
				}

				if (i == _data.Length - 1)
					value <<= BitsPerLong - AlignmentOffset;

				if (value <= 0x00000000FFFFFFFFUL)
				{
					count +=  32;
					value <<= 32;
				}

				if (value <= 0x0000FFFFFFFFFFFFUL)
				{
					count +=  16;
					value <<= 16;
				}

				if (value <= 0x00FFFFFFFFFFFFFFUL)
				{
					count +=  8;
					value <<= 8;
				}

				if (value <= 0x0FFFFFFFFFFFFFFFUL)
				{
					count +=  4;
					value <<= 4;
				}

				if (value <= 0x3FFFFFFFFFFFFFFFUL)
				{
					count +=  2;
					value <<= 2;
				}

				if (value <= 0x7FFFFFFFFFFFFFFFUL)
					count += 1;
				break;
			}

			return count;
		}

		public int GetTrailingZeroCount()
		{
			var count = 1;
			for (var i = 0; i < _data.Length; i++)
			{
				var value = _data[i];
				if (value == 0)
				{
					count += BitsPerLong;
					continue;
				}

				if ((value & 0x00000000FFFFFFFFUL) == 0)
				{
					count +=  32;
					value >>= 32;
				}

				if ((value & 0x000000000000FFFFUL) == 0)
				{
					count +=  16;
					value >>= 16;
				}

				if ((value & 0x00000000000000FFUL) == 0)
				{
					count +=  8;
					value >>= 8;
				}

				if ((value & 0x000000000000000FUL) == 0)
				{
					count +=  4;
					value >>= 4;
				}

				if ((value & 0x0000000000000003UL) == 0)
				{
					count +=  2;
					value >>= 2;
				}

				count -= (int)(value & 0x1UL);
				break;
			}

			return count;
		}
		#endregion

		#region Private Methods
		private static int DetermineArrayLength(int numberOfValues, int valueSize)
		{
			return numberOfValues <= 0 ? 0 : (numberOfValues - 1) / valueSize + 1;
		}

		private static int DetermineInternalArrayLength(int numberOfValues, int valueSize)
		{
			return numberOfValues <= 0 ? 0 : (numberOfValues - 1) / (BitsPerLong / valueSize) + 1;
		}

		private static ulong BitwiseReverse(ulong value)
		{
			if (value == 0 || value == ulong.MaxValue)
				return value;

			value = (value & 0x5555555555555555UL) << 1 | (value & 0xAAAAAAAAAAAAAAAAUL) >> 1;
			value = (value & 0x3333333333333333UL) << 2 | (value & 0xCCCCCCCCCCCCCCCCUL) >> 2;
			value = (value & 0x0F0F0F0F0F0F0F0FUL) << 4 | (value & 0xF0F0F0F0F0F0F0F0UL) >> 4;
			value = (value & 0x00FF00FF00FF00FFUL) << 8 | (value & 0xFF00FF00FF00FF00UL) >> 8;
			value = (value & 0x0000FFFF0000FFFFUL) << 16 | (value & 0xFFFF0000FFFF0000UL) >> 16;
			value = (value & 0x00000000FFFFFFFFUL) << 32 | (value & 0xFFFFFFFF00000000UL) >> 32;
			return value;
		}

		private static void CorrectUnalignedLeftShift(ulong[] data, int alignmentOffset)
		{
			/*if (alignmentOffset == 0)
				data[data.Length - 1] = 0;
			else
				data[data.Length - 1] &= 0xFFFFFFFFFFFFFFFFUL >> BitsPerLong - alignmentOffset;*/

			data[data.Length - 1] &= 0xFFFFFFFFFFFFFFFFUL >> BitsPerLong - alignmentOffset;
		}

		private static void ShiftLeftCore(ulong[] data, int amount)
		{
			var majorAmount = amount / BitsPerLong;
			var minorAmount = amount % BitsPerLong;
			var mask        = 0xFFFFFFFFFFFFFFFFUL << (BitsPerLong - minorAmount);

			var value1 = 0UL;
			for (var i = 0; i < majorAmount; i++)
			{
				for (var j = data.Length - 1; j > 0; j--)
				{
					data[j] = data[j - 1];
				}

				data[i] = 0UL;
			}

			for (var i = majorAmount; i < data.Length && minorAmount > 0; i++)
			{
				var value2 = data[i] & mask;
				data[i] <<= minorAmount;
				data[i] ^=  value1;
				value1  =   value2 >> (BitsPerLong - minorAmount);
			}
		}

		private static void ShiftRightCore(ulong[] data, int amount)
		{
			var majorAmount = amount / BitsPerLong;
			var minorAmount = amount % BitsPerLong;
			var mask        = 0xFFFFFFFFFFFFFFFFUL >> (BitsPerLong - minorAmount);

			var value1 = 0UL;
			for (var i = 0; i < majorAmount; i++)
			{
				for (var j = 0; j < data.Length - i - 1; j++)
				{
					data[j] = data[j + 1];
				}

				data[data.Length - i - 1] = 0UL;
			}

			for (var i = data.Length - majorAmount - 1; i >= 0 && minorAmount > 0; i--)
			{
				var value2 = data[i] & mask;
				data[i] >>= minorAmount;
				data[i] ^=  value1;
				value1  =   value2 << (BitsPerLong - minorAmount);
			}
		}
		#endregion

		#region Interface Implementation (IEnumerable<bool>)
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public IEnumerator<bool> GetEnumerator()
		{
			return GetEnumerable(Endianness.Little).GetEnumerator();
		}

		public IEnumerable<bool> GetEnumerable(Endianness direction)
		{
			if (direction == Endianness.Little)
			{
				var compareVersion = _version;
				for (var i = 0; i < _length; i++)
				{
					if (_version != compareVersion)
						throw new InvalidOperationException("Enumeration version check failed");
					yield return this[i];
				}
			}
			else
			{
				var compareVersion = _version;
				for (var i = _length - 1; i >= 0; i--)
				{
					if (_version != compareVersion)
						throw new InvalidOperationException("Enumeration version check failed");
					yield return this[i];
				}
			}
		}
		#endregion

		#region Interface Implementation (ICloneable)
		public object Clone()
		{
			return new BitArray64(this);
		}
		#endregion

		#region Interface Implementation (IFormattable)
		public string ToString(string format)
		{
			return ToString(format, CultureInfo.CurrentCulture);
		}

		public string ToString(string format, IFormatProvider formatProvider)
		{
			var result = new StringBuilder();
			switch (format)
			{
				case "G":
				case "B":
					result.Append("0b");
					foreach (var bit in GetEnumerable(Endianness.Big))
					{
						result.Append(bit ? '1' : '0');
					}

					break;
				case "X":
					result.Append("0x");
					for (var i = _data.Length - 1; i >= 0; i--)
					{
						if (_data.Length == 1 || i == _data.Length - 1)
						{
							var hexDigitsNeeded = AlignmentOffset / 4 + (AlignmentOffset % 4 == 0 ? 0 : 1);
							result.Append(_data[i].ToString($"X{hexDigitsNeeded}", formatProvider));
						}
						else
						{
							result.Append(_data[i].ToString("X16", formatProvider));
						}
					}

					break;
				case "D":
					throw new NotImplementedException("This format has not yet been implemented...");
				default:
					throw new ArgumentException("Unrecognized format string", nameof(format));
			}

			return result.ToString();
		}
		#endregion

		#region Method Overrides (System.Object)
		public override string ToString()
		{
			return ToString("X");
		}
		#endregion

		#region Operator Overloads
		// TODO: Implement
		#endregion
	}
}