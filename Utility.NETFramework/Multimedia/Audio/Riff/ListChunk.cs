// ┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  FILE:       ListChunk.cs
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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace JLR.Utility.NET.Multimedia.Audio.Riff
{
	public class ListChunk : RiffChunk
	{
		#region Fields
		private readonly RiffChunkReadProcedure       _readProcedure;
		private readonly RiffChunkWriteProcedure      _writeProcedure;
		private readonly RiffChunkValidationProcedure _validationProcedure;
		private readonly List<ProcedureSet>           _subchunkProcedures;
		#endregion

		#region Properties
		public                 FourCc TypeId { get; private set; }
		public sealed override long   Size   => base.Size + 4;
		#endregion

		#region Constructors
		private ListChunk(FourCc id, FourCc typeId) : base(id, ReadChunk, WriteChunk, ValidateChunk)
		{
			TypeId               = typeId;
			_readProcedure       = null;
			_writeProcedure      = null;
			_validationProcedure = null;
			_subchunkProcedures  = new List<ProcedureSet>();
		}

		public ListChunk(FourCc id,
						 FourCc typeId,
						 RiffChunkReadProcedure readProcedure = null,
						 RiffChunkWriteProcedure writeProcedure = null,
						 RiffChunkValidationProcedure validationProcedure = null) : this(id, typeId)
		{
			_readProcedure       = readProcedure;
			_writeProcedure      = writeProcedure;
			_validationProcedure = validationProcedure;
		}

		public ListChunk(FourCc id,
						 FourCc typeId,
						 BinaryReader reader,
						 RiffChunkReadProcedure readProcedure = null,
						 RiffChunkWriteProcedure writeProcedure = null,
						 RiffChunkValidationProcedure validationProcedure = null) : this(
			id,
			typeId,
			readProcedure,
			writeProcedure,
			validationProcedure)
		{
			Read(reader);
		}

		public ListChunk(ListChunkProcedureSet chunkProcedures) : this(
			chunkProcedures.Id,
			chunkProcedures.TypeId,
			chunkProcedures.ReadProcedure,
			chunkProcedures.WriteProcedure,
			chunkProcedures.ValidationProcedure) { }

		public ListChunk(BinaryReader reader, ListChunkProcedureSet chunkProcedures) : this(chunkProcedures)
		{
			Read(reader);
		}
		#endregion

		#region Static Methods
		public static void AddSubchunkProcedures(ListChunk chunk, params ProcedureSet[] procedureSets)
		{
			chunk._subchunkProcedures.AddRange(procedureSets);
		}
		#endregion

		#region Private Methods
		private static void ReadChunk(RiffChunk chunk, BinaryReader reader)
		{
			var listChunk = chunk as ListChunk;
			if (listChunk == null)
				throw new FormatException("\"chunk\" must be a ListChunk");

			// Verify that the type ID read from the stream matches the expected type ID
			var typeIdRead     = reader.ReadFourCc();
			var typeIdExpected = listChunk.TypeId;
			if (typeIdRead != typeIdExpected)
			{
				listChunk.TypeId = typeIdRead;
				throw new RiffException(
					$"The LIST chunk TypeId read from the stream ({typeIdRead}) does not match the expected value ({typeIdExpected})");
			}

			// Run the read procedure
			listChunk._readProcedure?.Invoke(listChunk, reader);

			// Only predefined chunk IDs will be read into the LIST chunk
			// If the procedure list is empty, the entire contents of the LIST chunk will be ignored
			if (listChunk._subchunkProcedures.Count == 0)
				throw new RiffException("No subchunk IDs have been defined");

			// Add all defined subchunks
			var bytesRemaining = listChunk.StreamIndicatedSize - 4;
			while (bytesRemaining > 0 && (reader.BaseStream.Length - reader.BaseStream.Position > 2))
			{
				var chunkId = reader.ReadFourCc();
				if (listChunk._subchunkProcedures.Exists(set => set is ListChunkProcedureSet && set.Id == chunkId))
				{
					var chunkTypeId = reader.PeekFourCc(4);
					var listChunkProcedure = listChunk._subchunkProcedures.OfType<ListChunkProcedureSet>()
													  .First(set => set.Id == chunkId && set.TypeId == chunkTypeId);
					if (listChunkProcedure != null)
						listChunk.Children.Add(new ListChunk(reader, listChunkProcedure));
				}
				else if (listChunk._subchunkProcedures.Exists(set => set.Id == chunkId))
				{
					var chunkProcedure = listChunk._subchunkProcedures.First(set => set.Id == chunkId);
					if (chunkProcedure != null)
						listChunk.Children.Add(new RiffChunk(reader, chunkProcedure));
					bytesRemaining -= listChunk.Children.Last.ActualSize;
				}
				else
				{
					bytesRemaining -= SkipUnknownChunk(reader) + listChunk.HeaderSize;
				}
			}
		}

		private static void WriteChunk(RiffChunk chunk, BinaryWriter writer)
		{
			var listChunk = chunk as ListChunk;
			if (listChunk == null)
				throw new FormatException("\"chunk\" must be a ListChunk");

			writer.Write(listChunk.TypeId.ToUInt32());
			listChunk._writeProcedure?.Invoke(listChunk, writer);
		}

		private static void ValidateChunk(RiffChunk chunk, ref ValidationResult result)
		{
			var listChunk = chunk as ListChunk;
			if (listChunk == null)
				throw new FormatException("\"chunk\" must be a ListChunk");

			if (listChunk.TypeId == FourCc.Zero)
				result.EditValidity(Validity.Invalid, "\"TypeId\" cannot be equal to zero");

			listChunk._validationProcedure?.Invoke(listChunk, ref result);
		}
		#endregion

		#region Method Overrides (System.Object)
		public override string ToString()
		{
			if (!IsValid) return Validate().ToString();
			var idTally = from chunk in Children.OfType<RiffChunk>()
						  group chunk by chunk.Id
						  into ids
						  select new { id = ids.Key, count = ids.Count() };

			var contentString = new StringBuilder();
			var idDictionary  = idTally.ToDictionary(item => item.id, item => item.count);
			if (idDictionary.Count == 0)
				contentString.Append("empty");
			else
			{
				var i = 0;
				foreach (var id in idDictionary)
				{
					contentString.Append(id.Key.ToString());
					contentString.Append('(');
					contentString.Append(id.Value);
					contentString.Append(')');
					if (++i < idDictionary.Count)
						contentString.Append(", ");
				}
			}

			return $"LIST[{TypeId}] ({contentString})";
		}
		#endregion

		#region Classes
		public sealed class ListChunkProcedureSet : ProcedureSet
		{
			private readonly FourCc _typeId;
			public           FourCc TypeId => _typeId;

			public ListChunkProcedureSet(FourCc id,
										 FourCc typeId,
										 RiffChunkReadProcedure readProcedure = null,
										 RiffChunkWriteProcedure writeProcedure = null,
										 RiffChunkValidationProcedure validationProcedure = null) : base(
				id,
				readProcedure,
				writeProcedure,
				validationProcedure)
			{
				_typeId = typeId;
			}
		}
		#endregion
	}
}