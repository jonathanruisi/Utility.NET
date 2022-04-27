// ┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  FILE:       IntegerComponent.cs
// ┃  PROJECT:    Utility.NET
// ┃  SOLUTION:   Utility.NET
// ┃  CREATED:    2018-03-13 @ 7:09 PM
// ┣━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  AUTHOR:     Jonathan Ruisi
// ┃  EMAIL:      JonathanRuisi@gmail.com
// ┣━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  GIT REPO:   https://github.com/jonathanruisi/Utility.NET
// ┃  LICENSE:    https://github.com/jonathanruisi/Utility.NET/blob/master/LICENSE
// ┗━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

using System.IO;

namespace JLR.Utility.NETFramework.Multimedia.Audio.Riff
{
	public sealed class UInt8Component : IntegerComponent<byte>
	{
		public override long Size => 1;
		public UInt8Component(byte value) : base(value) { }

		public UInt8Component(BinaryReader reader)
		{
			Read(reader);
		}

		public override void Read(BinaryReader reader)
		{
			Value = reader.ReadByte();
		}

		public override void Write(BinaryWriter writer)
		{
			writer.Write(Value);
		}
	}

	public sealed class Int8Component : IntegerComponent<sbyte>
	{
		public override long Size => 1;
		public Int8Component(sbyte value) : base(value) { }

		public Int8Component(BinaryReader reader)
		{
			Read(reader);
		}

		public override void Read(BinaryReader reader)
		{
			Value = reader.ReadSByte();
		}

		public override void Write(BinaryWriter writer)
		{
			writer.Write(Value);
		}
	}

	public sealed class UInt16Component : IntegerComponent<ushort>
	{
		public override long Size => 2;
		public UInt16Component(ushort value) : base(value) { }

		public UInt16Component(BinaryReader reader)
		{
			Read(reader);
		}

		public override void Read(BinaryReader reader)
		{
			Value = reader.ReadUInt16();
		}

		public override void Write(BinaryWriter writer)
		{
			writer.Write(Value);
		}
	}

	public sealed class Int16Component : IntegerComponent<short>
	{
		public override long Size => 2;
		public Int16Component(short value) : base(value) { }

		public Int16Component(BinaryReader reader)
		{
			Read(reader);
		}

		public override void Read(BinaryReader reader)
		{
			Value = reader.ReadInt16();
		}

		public override void Write(BinaryWriter writer)
		{
			writer.Write(Value);
		}
	}

	public sealed class UInt32Component : IntegerComponent<uint>
	{
		public override long Size => 4;
		public UInt32Component(uint value) : base(value) { }

		public UInt32Component(BinaryReader reader)
		{
			Read(reader);
		}

		public override void Read(BinaryReader reader)
		{
			Value = reader.ReadUInt32();
		}

		public override void Write(BinaryWriter writer)
		{
			writer.Write(Value);
		}
	}

	public sealed class Int32Component : IntegerComponent<int>
	{
		public override long Size => 4;
		public Int32Component(int value) : base(value) { }

		public Int32Component(BinaryReader reader)
		{
			Read(reader);
		}

		public override void Read(BinaryReader reader)
		{
			Value = reader.ReadInt32();
		}

		public override void Write(BinaryWriter writer)
		{
			writer.Write(Value);
		}
	}

	public sealed class UInt64Component : IntegerComponent<ulong>
	{
		public override long Size => 8;
		public UInt64Component(ulong value) : base(value) { }

		public UInt64Component(BinaryReader reader)
		{
			Read(reader);
		}

		public override void Read(BinaryReader reader)
		{
			Value = reader.ReadUInt64();
		}

		public override void Write(BinaryWriter writer)
		{
			writer.Write(Value);
		}
	}

	public sealed class Int64Component : IntegerComponent<long>
	{
		public override long Size => 8;
		public Int64Component(long value) : base(value) { }

		public Int64Component(BinaryReader reader)
		{
			Read(reader);
		}

		public override void Read(BinaryReader reader)
		{
			Value = reader.ReadInt64();
		}

		public override void Write(BinaryWriter writer)
		{
			writer.Write(Value);
		}
	}

	public sealed class FourCcComponent : IntegerComponent<FourCc>
	{
		public override long Size => 8;
		public FourCcComponent(FourCc value) : base(value) { }

		public FourCcComponent(BinaryReader reader)
		{
			Read(reader);
		}

		public override void Read(BinaryReader reader)
		{
			Value = reader.ReadFourCc();
		}

		public override void Write(BinaryWriter writer)
		{
			writer.Write(Value.ToUInt32());
		}
	}

	public abstract class IntegerComponent<T> : RiffComponent where T : struct
	{
		#region Properties
		public T Value { get; set; }
		#endregion

		#region Constructors
		protected IntegerComponent(T value = default(T))
		{
			Value = value;
		}
		#endregion

		#region Interface Implementation (IValidatable)
		public override ValidationResult Validate()
		{
			return new ValidationResult();
		}
		#endregion

		#region Method Overrides (System.Object)
		public override string ToString()
		{
			return Value.ToString();
		}
		#endregion
	}
}