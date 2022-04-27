// ┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  FILE:       EnumeratedTypes.cs
// ┃  PROJECT:    Utility.NET
// ┃  SOLUTION:   Utility.NET
// ┃  CREATED:    2015-12-23 @ 4:52 AM
// ┣━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  AUTHOR:     Jonathan Ruisi
// ┃  EMAIL:      JonathanRuisi@gmail.com
// ┣━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  GIT REPO:   https://github.com/jonathanruisi/Utility.NET
// ┃  LICENSE:    https://github.com/jonathanruisi/Utility.NET/blob/master/LICENSE
// ┗━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

namespace JLR.Utility.NETFramework
{
	/// <summary>
	/// Defines terms for rows and columns
	/// </summary>
	public enum RowOrColumn
	{
		Row,
		Column
	}

	/// <summary>
	/// Defines generic terms for horizontal and vertical positioning
	/// </summary>
	public enum Position
	{
		Left,
		Center,
		Right,
		Top,
		Middle,
		Bottom
	}

	/// <summary>
	/// Defines generic inclusion terms
	/// </summary>
	public enum InclusionRule
	{
		Illegal,
		Optional,
		Required
	}

	/// <summary>
	/// Defines generic read/write access modes
	/// </summary>
	public enum AccessMode
	{
		None,
		ReadOnly,
		WriteOnly,
		ReadWrite
	}

	/// <summary>
	/// Defines generic acceptance terms
	/// </summary>
	public enum Acceptance
	{
		Accept,
		Reject
	}

	/// <summary>
	/// Defines generic filter actions
	/// </summary>
	public enum FilterAction
	{
		Keep,
		Discard
	}

	/// <summary>
	/// Defines basic logic operations
	/// </summary>
	public enum LogicOperation
	{
		Not,
		And,
		Or,
		XOr,
		XNor
	}

	/// <summary>
	/// Defines basic bitwise operations
	/// </summary>
	public enum BitwiseOperation
	{
		None,

		/// <summary>All bits are shifted left (sign bit not preserved)</summary>
		LogicalShiftLeft,

		/// <summary>All bits are shifted right (sign bit not preserved)</summary>
		LogicalShiftRight,

		/// <summary>All bits are shifted left (sign bit preserved)</summary>
		ArithmeticShiftLeft,

		/// <summary>All bits are shifted left (sign bit preserved)</summary>
		ArithmeticShiftRight,

		/// <summary>All bits are rotated through to the left (sign not preserved)</summary>
		RotateLeft,

		/// <summary>All bits are rotated through to the right (sign not preserved)</summary>
		RotateRight
	}

	/// <summary>
	/// Represents a number base
	/// </summary>
	public enum Radix
	{
		/// <summary>Base 2</summary>
		Binary,

		/// <summary>Base 8</summary>
		Octal,

		/// <summary>Base 10</summary>
		Decimal,

		/// <summary>Base 16</summary>
		Hexadecimal
	}

	/// <summary>
	/// Represents typical names used to indicate the size (in bits) of a data element
	/// </summary>
	public enum DataSize
	{
		/// <summary>1 Bit</summary>
		Bit = 1,

		/// <summary>4 Bits</summary>
		Nibble = 4,

		/// <summary>8 Bits</summary>
		Byte = 8,

		/// <summary>16 Bits</summary>
		Word = 16,

		/// <summary>32 Bits</summary>
		DoubleWord = 32,

		/// <summary>64 Bits</summary>
		QuadWord = 64
	}

	/// <summary>
	/// Represents the bit/byte position in relation to its potential value in a binary number.
	/// MSB has the highest potential value and LSB has the lowest potential value.
	/// </summary>
	public enum BitPosition
	{
		/// <summary>Least significant bit/byte</summary>
		Lsb,

		/// <summary>Most significant bit/byte</summary>
		Msb
	}

	/// <summary>
	/// Represents the way in which multi-byte values
	/// (or values that require more than one memory storage location)
	/// are stored in memory.
	/// </summary>
	public enum Endianness
	{
		/// <summary>Least significant byte is stored at the lowest memory address</summary>
		Little,

		/// <summary>Most significant byte is stored at the lowest memory address</summary>
		Big
	}

	/// <summary>
	/// Represents the current list of discrete MIME types registered with the IANA.
	/// MIME types are used to describe the general category into which a set of data falls.
	/// </summary>
	public enum MIMETypes
	{
		Unknown,
		Application,
		Audio,
		Example,
		Font,
		Image,
		Model,
		Text,
		Video
	}
}