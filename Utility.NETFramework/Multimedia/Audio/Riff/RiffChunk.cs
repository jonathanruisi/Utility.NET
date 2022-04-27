// ┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  FILE:       RiffChunk.cs
// ┃  PROJECT:    Utility.NET
// ┃  SOLUTION:   Utility.NET
// ┃  CREATED:    2018-03-13 @ 7:10 PM
// ┣━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  AUTHOR:     Jonathan Ruisi
// ┃  EMAIL:      JonathanRuisi@gmail.com
// ┣━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  GIT REPO:   https://github.com/jonathanruisi/Utility.NET
// ┃  LICENSE:    https://github.com/jonathanruisi/Utility.NET/blob/master/LICENSE
// ┗━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

using System;
using System.IO;

namespace JLR.Utility.NET.Multimedia.Audio.Riff
{
	#region Enumerated Types
	public enum FileFormatId : uint
	{
		/// <summary>RIFF</summary>
		Riff = 0x46464952,

		/// <summary>RF64</summary>
		Rf64 = 0x34364652
	}

	public enum RiffChunkId : uint
	{
		Wave = 0x45564157,

		/// <summary>JUNK</summary>
		Junk = 0x4B4E554A,

		/// <summary>LIST</summary>
		List = 0x5453494C
	}

	public enum Rf64ChunkId : uint
	{
		/// <summary>ds64</summary>
		Ds64 = 0x34367364
	}
	#endregion

	#region Delegates
	public delegate void RiffChunkReadProcedure(RiffChunk chunk, BinaryReader reader);

	public delegate void RiffChunkWriteProcedure(RiffChunk chunk, BinaryWriter writer);

	public delegate void RiffChunkValidationProcedure(RiffChunk chunk, ref ValidationResult result);
	#endregion

	public class RiffChunk : RiffComponent
	{
		#region Fields
		internal         long                         StreamIndicatedSize;
		internal         long                         StreamIndicatedOffset;
		private readonly RiffChunkReadProcedure       _readProcedure;
		private readonly RiffChunkWriteProcedure      _writeProcedure;
		private readonly RiffChunkValidationProcedure _validationProcedure;
		#endregion

		#region Properties
		protected sealed override int    HeaderSize => 8;
		protected sealed override bool   IsPadded   => Size % 2 != 0;
		public                    FourCc Id         { get; }
		#endregion

		#region Constructors
		private RiffChunk(FourCc id)
		{
			Id                   = id;
			_readProcedure       = null;
			_writeProcedure      = null;
			_validationProcedure = null;
		}

		public RiffChunk(FourCc id,
						 RiffChunkReadProcedure readProcedure = null,
						 RiffChunkWriteProcedure writeProcedure = null,
						 RiffChunkValidationProcedure validationProcedure = null) : this(id)
		{
			_readProcedure       = readProcedure;
			_writeProcedure      = writeProcedure;
			_validationProcedure = validationProcedure;
		}

		public RiffChunk(FourCc id,
						 BinaryReader reader,
						 RiffChunkReadProcedure readProcedure = null,
						 RiffChunkWriteProcedure writeProcedure = null,
						 RiffChunkValidationProcedure validationProcedure = null) : this(
			id,
			readProcedure,
			writeProcedure,
			validationProcedure)
		{
			Read(reader);
		}

		public RiffChunk(ProcedureSet chunkProcedures) : this(
			chunkProcedures.Id,
			chunkProcedures.ReadProcedure,
			chunkProcedures.WriteProcedure,
			chunkProcedures.ValidationProcedure) { }

		public RiffChunk(BinaryReader reader, ProcedureSet chunkProcedures) : this(chunkProcedures)
		{
			Read(reader);
		}
		#endregion

		#region Internal, Protected, and Private Methods
		public sealed override void Read(BinaryReader reader)
		{
			base.Read(reader);
			try
			{
				// Read header
				StreamIndicatedSize   = reader.ReadUInt32();
				StreamIndicatedOffset = reader.BaseStream.Position;

				// Read data
				_readProcedure?.Invoke(this, reader);

				// If current stream position is not word-aligned, adjust position for padding byte
				if (IsPadded)
					ReadPadByte(reader);
			}
			catch (Exception ex)
			{
				throw new RiffException("Failed to read chunk data from stream", ex);
			}
		}

		public sealed override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			try
			{
				// Write header
				writer.BaseStream.Position = ActualOffset;
				writer.Write(Id.ToUInt32());
				writer.Write((uint)Size);

				// Write data
				_writeProcedure?.Invoke(this, writer);
				foreach (var child in Children)
				{
					child.Write(writer);
				}

				// Write padding byte if stream position is not word-aligned
				if (IsPadded)
					writer.Write((byte)0);
			}
			catch (Exception ex)
			{
				throw new RiffException("Failed to write chunk data to stream", ex);
			}
		}

		protected static long SkipUnknownChunk(BinaryReader reader)
		{
			var skipSize = reader.ReadUInt32();
			reader.BaseStream.Seek(skipSize, SeekOrigin.Current);
			if (skipSize % 2 != 0)
			{
				ReadPadByte(reader);
				skipSize++;
			}

			return skipSize;
		}

		private static void ReadPadByte(BinaryReader reader)
		{
			var padByte = reader.ReadByte();
			if (padByte != 0)
				throw new RiffException("Encountered a non-zero pad byte");
		}
		#endregion

		#region Interface Implementation (IValidatable)
		public sealed override ValidationResult Validate()
		{
			var result = new ValidationResult();
			if (Id == FourCc.Zero)
				result.EditValidity(Validity.Invalid, "\"Id\" cannot be equal to zero");

			_validationProcedure?.Invoke(this, ref result);

			foreach (var component in Children)
			{
				result.Merge(component.Validate());
			}

			return result;
		}
		#endregion

		#region Method Overrides (System.Object)
		public override string ToString()
		{
			return Id.ToString();
		}
		#endregion

		#region Nested Types
		public class ProcedureSet
		{
			#region Properties
			public FourCc                       Id                  { get; }
			public RiffChunkReadProcedure       ReadProcedure       { get; }
			public RiffChunkWriteProcedure      WriteProcedure      { get; }
			public RiffChunkValidationProcedure ValidationProcedure { get; }
			#endregion

			#region Constructor
			public ProcedureSet(FourCc id,
								RiffChunkReadProcedure readProcedure = null,
								RiffChunkWriteProcedure writeProcedure = null,
								RiffChunkValidationProcedure validationProcedure = null)
			{
				Id                  = id;
				ReadProcedure       = readProcedure;
				WriteProcedure      = writeProcedure;
				ValidationProcedure = validationProcedure;
			}
			#endregion
		}
		#endregion
	}
}