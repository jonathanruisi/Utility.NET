// ┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  FILE:       WaveChunk.cs
// ┃  PROJECT:    Utility.NET
// ┃  SOLUTION:   Utility.NET
// ┃  CREATED:    2018-03-13 @ 7:20 PM
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

using JLR.Utility.NETFramework.Collections.Tree;
using JLR.Utility.NETFramework.IO;

namespace JLR.Utility.NETFramework.Multimedia.Audio.Riff.Wave
{
	#region Enumerated Types
	public enum WaveChunkId : uint
	{
		Format             = 0x20746D66, // "fmt "
		Data               = 0x61746164, // "data"
		Fact               = 0x74636166, // "fact"
		WaveList           = 0x6C766177, // "wavl"
		Silent             = 0x746E6C73, // "slnt"
		Cue                = 0x20657563, // "cue "
		Playlist           = 0x74736C70, // "plst"
		AssociatedDataList = 0x6C746461, // "adtl"
		Label              = 0x6C62616C, // "labl"
		Note               = 0x65746F6E, // "note"
		LabeledText        = 0x7478746C, // "ltxt"
		Sampler            = 0x6C706D73, // "smpl"
		Instrument         = 0x74736E69  // "inst"
	}
	#endregion

	public sealed class WaveChunk : ListChunk
	{
		#region Properties
		// Format
		public FormatComponent Format =>
			GetEnumerable(TraversalMode.BreadthFirst, TraversalDirection.TopDown).OfType<FormatComponent>().FirstOrDefault();

		// Data
		public Stream AudioData { get; private set; }

		// Fact
		public List<uint> Facts
		{
			get
			{
				var result = new List<uint>();
				var factChunk = GetEnumerable(TraversalMode.BreadthFirst, TraversalDirection.TopDown)
								.OfType<RiffChunk>()
								.FirstOrDefault(chunk => chunk.Id == (uint)WaveChunkId.Fact);
				if (factChunk != null)
				{
					result.AddRange(factChunk.Children.OfType<UInt32Component>().Select(component => component.Value));
				}

				return result;
			}
		}

		// Cue
		public List<CuePointComponent> CuePoints
		{
			get
			{
				var result = new List<CuePointComponent>();
				var cueChunk = GetEnumerable(TraversalMode.BreadthFirst, TraversalDirection.TopDown)
							   .OfType<RiffChunk>()
							   .FirstOrDefault(chunk => chunk.Id == (uint)WaveChunkId.Cue);
				if (cueChunk != null)
				{
					result.AddRange(cueChunk.Children.OfType<CuePointComponent>());
				}

				return result;
			}
		}

		// Playlist
		public List<PlaylistSegmentComponent> PlaylistSegments
		{
			get
			{
				var result = new List<PlaylistSegmentComponent>();
				var playlistChunk = GetEnumerable(TraversalMode.BreadthFirst, TraversalDirection.TopDown)
									.OfType<RiffChunk>()
									.FirstOrDefault(chunk => chunk.Id == (uint)WaveChunkId.Playlist);
				if (playlistChunk != null)
				{
					result.AddRange(playlistChunk.Children.OfType<PlaylistSegmentComponent>());
				}

				return result;
			}
		}

		// Sampler
		public SamplerComponent Sampler =>
			GetEnumerable(TraversalMode.BreadthFirst, TraversalDirection.TopDown).OfType<SamplerComponent>().FirstOrDefault();

		public List<SampleLoopComponent> SampleLoops
		{
			get
			{
				var result = new List<SampleLoopComponent>();
				var samplerChunk = GetEnumerable(TraversalMode.BreadthFirst, TraversalDirection.TopDown)
								   .OfType<RiffChunk>()
								   .FirstOrDefault(chunk => chunk.Id == (uint)WaveChunkId.Sampler);
				if (samplerChunk != null)
				{
					result.AddRange(samplerChunk.Children.OfType<SampleLoopComponent>());
				}

				return result;
			}
		}

		// Instrument
		public InstrumentComponent Instrument =>
			GetEnumerable(TraversalMode.BreadthFirst, TraversalDirection.TopDown).OfType<InstrumentComponent>().FirstOrDefault();

		// Associated Data List
		public List<Tuple<LabeledTextComponent, string>> RegionLabels
		{
			get
			{
				var result = new List<Tuple<LabeledTextComponent, string>>();
				var listChunks = GetEnumerable(TraversalMode.BreadthFirst, TraversalDirection.TopDown)
								 .OfType<ListChunk>()
								 .Where(chunk => chunk.TypeId == (uint)WaveChunkId.AssociatedDataList);
				foreach (var listChunk in listChunks)
				{
					var labeledTexts = listChunk.GetEnumerable(TraversalMode.BreadthFirst, TraversalDirection.TopDown)
												.OfType<RiffChunk>()
												.Where(chunk => chunk.Id == (uint)WaveChunkId.LabeledText);
					result.AddRange(labeledTexts.Select(CreateLabelObject));
				}

				return result;
			}
		}

		// Wave List
		public bool HasWaveList
		{
			get
			{
				return GetEnumerable(TraversalMode.BreadthFirst, TraversalDirection.TopDown)
					   .OfType<ListChunk>()
					   .Any(chunk => chunk.TypeId == (uint)WaveChunkId.WaveList);
			}
		}
		#endregion

		#region Constructors
		public WaveChunk() : base((uint)FileFormatId.Riff, (uint)RiffChunkId.Wave, ReadChunk, WriteChunk, ValidateChunk)
		{
			var procedureSets = new List<ProcedureSet>
			{
				new ListChunkProcedureSet(
					(uint)RiffChunkId.List,
					(uint)WaveChunkId.AssociatedDataList,
					AssociatedDataListChunkReadProcedure,
					AssociatedDataListChunkWriteProcedure,
					AssociatedDataListChunkValidationProcedure),
				new ListChunkProcedureSet(
					(uint)RiffChunkId.List,
					(uint)WaveChunkId.WaveList,
					WaveListChunkReadProcedure,
					WaveListChunkWriteProcedure,
					WaveListChunkValidationProcedure),
				new ProcedureSet((uint)WaveChunkId.Cue, CueChunkReadProcedure, CueChunkWriteProcedure, CueChunkValidationProcedure),
				new ProcedureSet(
					(uint)WaveChunkId.Data,
					DataChunkReadProcedure,
					DataChunkWriteProcedure,
					DataChunkValidationProcedure),
				new ProcedureSet(
					(uint)WaveChunkId.Fact,
					FactChunkReadProcedure,
					FactChunkWriteProcedure,
					FactChunkValidationProcedure),
				new ProcedureSet(
					(uint)WaveChunkId.Format,
					FormatChunkReadProcedure,
					FormatChunkWriteProcedure,
					FormatChunkValidationProcedure),
				new ProcedureSet(
					(uint)WaveChunkId.Instrument,
					InstrumentChunkReadProcedure,
					InstrumentChunkWriteProcedure,
					InstrumentChunkValidationProcedure),
				new ProcedureSet(
					(uint)WaveChunkId.Playlist,
					PlaylistChunkReadProcedure,
					PlaylistChunkWriteProcedure,
					PlaylistChunkValidationProcedure),
				new ProcedureSet(
					(uint)WaveChunkId.Sampler,
					SamplerChunkReadProcedure,
					SamplerChunkWriteProcedure,
					SamplerChunkValidationProcedure)
			};
			AddSubchunkProcedures(this, procedureSets.ToArray());
		}

		public WaveChunk(BinaryReader reader) : this()
		{
			Read(reader);
			InterpretChunks();
		}
		#endregion

		#region Static Methods
		public static WaveChunk FromStream(Stream source, long startOffset = 0)
		{
			if (source == null)
				throw new ArgumentNullException(nameof(source));
			if (startOffset < 0)
				throw new ArgumentOutOfRangeException(nameof(startOffset), "Start offset must be greater than zero");
			if (startOffset > source.Length)
				throw new ArgumentOutOfRangeException(
					nameof(startOffset),
					"Start offset must be less than the length of the stream");
			source.ValidateCapabilities(StreamCapabilities.Seek | StreamCapabilities.Read);
			var reader = new BinaryReader(source);

			try
			{
				source.Seek(startOffset, SeekOrigin.Begin);
				if (reader.SeekNextFourCc((uint)FileFormatId.Riff))
				{
					long chunkSize = reader.PeekUInt32();
					if (reader.PeekFourCc(4) == (uint)RiffChunkId.Wave)
						return new WaveChunk(reader);
					else
						source.Seek(chunkSize, SeekOrigin.Current);
				}
			}
			catch (Exception ex)
			{
				throw new RiffException("Error occurred while searching for chunk", ex);
			}
			finally
			{
				reader.Close();
			}

			return null;
		}
		#endregion

		#region Private Methods
		private void InterpretChunks()
		{
			// Set the AudioData stream
			if (HasWaveList)
			{
				throw new NotSupportedException("Wave lists are not currently supported");
			}
			else
			{
				var dataChunks = GetEnumerable(TraversalMode.BreadthFirst, TraversalDirection.TopDown)
								 .OfType<RiffChunk>()
								 .Where(chunk => chunk.Id == (uint)WaveChunkId.Data);

				foreach (var chunk in dataChunks)
				{
					if (chunk.Parent is RiffChunk && (chunk.Parent as RiffChunk).Id == (uint)WaveChunkId.WaveList)
						continue;
					var dataComponent = chunk.Children.OfType<RawDataComponent>().FirstOrDefault();
					if (dataComponent != null)
						AudioData = dataComponent.DataStream;
				}
			}

			// Associate text with cue points
			var labelChunks = new List<RiffChunk>();
			var noteChunks  = new List<RiffChunk>();
			var listChunks = GetEnumerable(TraversalMode.BreadthFirst, TraversalDirection.TopDown)
							 .OfType<ListChunk>()
							 .Where(chunk => chunk.TypeId == (uint)WaveChunkId.AssociatedDataList);

			foreach (var listChunk in listChunks)
			{
				labelChunks.AddRange(
					listChunk.GetEnumerable(TraversalMode.BreadthFirst, TraversalDirection.TopDown)
							 .OfType<RiffChunk>()
							 .Where(chunk => chunk.Id == (uint)WaveChunkId.Label));
				noteChunks.AddRange(
					listChunk.GetEnumerable(TraversalMode.BreadthFirst, TraversalDirection.TopDown)
							 .OfType<RiffChunk>()
							 .Where(chunk => chunk.Id == (uint)WaveChunkId.Note));
			}

			foreach (var cuePoint in CuePoints)
			{
				// Set cue point name
				var associatedTextItem = labelChunks.Find(chunk => ((UInt32Component)chunk.Children.First).Value == cuePoint.CueId);
				if (associatedTextItem?.Children.Last is TextComponent)
					cuePoint.Name = (associatedTextItem.Children.Last as TextComponent).Text;

				// Set cue point comment
				associatedTextItem = noteChunks.Find(chunk => ((UInt32Component)chunk.Children.First).Value == cuePoint.CueId);
				if (associatedTextItem?.Children.Last is TextComponent)
					cuePoint.Comment = (associatedTextItem.Children.Last as TextComponent).Text;
			}

			// Associate cue points with playlist segments
			foreach (var playlistSegment in PlaylistSegments)
			{
				var associatedCuePoint = CuePoints.Find(cuePoint => cuePoint.CueId == playlistSegment.CueId);
				if (associatedCuePoint != null)
					playlistSegment.CuePoint = associatedCuePoint;
			}

			// Associate cue points with sample loops
			foreach (var sampleLoop in SampleLoops)
			{
				var associatedCuePoint = CuePoints.Find(cuePoint => cuePoint.CueId == sampleLoop.CueId);
				if (associatedCuePoint != null)
					sampleLoop.CuePoint = associatedCuePoint;
			}
		}

		private Tuple<LabeledTextComponent, string> CreateLabelObject(RiffChunk chunk)
		{
			var labeledTextComponent = chunk.Children.OfType<LabeledTextComponent>().FirstOrDefault();
			var textComponent        = chunk.Children.OfType<TextComponent>().FirstOrDefault();
			return textComponent != null
				? new Tuple<LabeledTextComponent, string>(labeledTextComponent, textComponent.Text)
				: new Tuple<LabeledTextComponent, string>(labeledTextComponent, null);
		}
		#endregion

		#region WaveChunk Procedures
		private static void ReadChunk(RiffChunk chunk, BinaryReader reader) { }
		private static void WriteChunk(RiffChunk chunk, BinaryWriter writer) { }
		private static void ValidateChunk(RiffChunk chunk, ref ValidationResult result) { }
		#endregion

		#region Subchunk Read Procedures
		private static void AssociatedDataListChunkReadProcedure(RiffChunk chunk, BinaryReader reader)
		{
			var listChunk = chunk as ListChunk;
			if (listChunk == null)
				throw new FormatException("\"chunk\" must be a ListChunk");

			var procedureSets = new List<ProcedureSet>
			{
				new ProcedureSet(
					(uint)WaveChunkId.Label,
					LabelChunkReadProcedure,
					LabelChunkWriteProcedure,
					LabelChunkValidationProcedure),
				new ProcedureSet(
					(uint)WaveChunkId.Note,
					NoteChunkReadProcedure,
					NoteChunkWriteProcedure,
					NoteChunkValidationProcedure),
				new ProcedureSet(
					(uint)WaveChunkId.LabeledText,
					LabeledTextChunkReadProcedure,
					LabeledTextChunkWriteProcedure,
					LabeledTextChunkValidationProcedure)
			};
			AddSubchunkProcedures(listChunk, procedureSets.ToArray());
		}

		private static void WaveListChunkReadProcedure(RiffChunk chunk, BinaryReader reader)
		{
			var listChunk = chunk as ListChunk;
			if (listChunk == null)
				throw new FormatException("\"chunk\" must be a ListChunk");

			var procedureSets = new List<ProcedureSet>
			{
				new ProcedureSet(
					(uint)WaveChunkId.Data,
					DataChunkReadProcedure,
					DataChunkWriteProcedure,
					DataChunkValidationProcedure),
				new ProcedureSet(
					(uint)WaveChunkId.Silent,
					SilentChunkReadProcedure,
					SilentChunkWriteProcedure,
					SilentChunkValidationProcedure)
			};
			AddSubchunkProcedures(listChunk, procedureSets.ToArray());
		}

		private static void CueChunkReadProcedure(RiffChunk chunk, BinaryReader reader)
		{
			var cuePointCount = reader.ReadUInt32();
			chunk.Children.Add(new UInt32Component(cuePointCount));
			for (var i = 0; i < cuePointCount; i++)
			{
				chunk.Children.Add(new CuePointComponent(reader));
			}
		}

		private static void DataChunkReadProcedure(RiffChunk chunk, BinaryReader reader)
		{
			chunk.Children.Add(new RawDataComponent(reader, (int)chunk.StreamIndicatedSize));
		}

		private static void FactChunkReadProcedure(RiffChunk chunk, BinaryReader reader)
		{
			var factCount = (chunk.StreamIndicatedSize / 4);
			for (var i = 0; i < factCount; i++)
			{
				chunk.Children.Add(new UInt32Component(reader));
			}

			// Skip any extra data that may exist (indicates existence of fact data that are not 4B in size)
			var extraBytes = chunk.StreamIndicatedSize % 4;
			if (extraBytes != 0)
				reader.BaseStream.Seek(extraBytes % 4, SeekOrigin.Current);
		}

		private static void FormatChunkReadProcedure(RiffChunk chunk, BinaryReader reader)
		{
			chunk.Children.Add(new FormatComponent(reader));
		}

		private static void InstrumentChunkReadProcedure(RiffChunk chunk, BinaryReader reader)
		{
			chunk.Children.Add(new InstrumentComponent(reader));
		}

		private static void LabelChunkReadProcedure(RiffChunk chunk, BinaryReader reader)
		{
			var cueId = reader.ReadUInt32();
			chunk.Children.Add(new UInt32Component(cueId));
			chunk.Children.Add(new TextComponent(reader));
		}

		private static void LabeledTextChunkReadProcedure(RiffChunk chunk, BinaryReader reader)
		{
			chunk.Children.Add(new LabeledTextComponent(reader));
			if (chunk.StreamIndicatedSize > 20)
				chunk.Children.Add(new TextComponent(reader));
		}

		private static void NoteChunkReadProcedure(RiffChunk chunk, BinaryReader reader)
		{
			var cueId = reader.ReadUInt32();
			chunk.Children.Add(new UInt32Component(cueId));
			chunk.Children.Add(new TextComponent(reader));
		}

		private static void PlaylistChunkReadProcedure(RiffChunk chunk, BinaryReader reader)
		{
			var playlistSegmentCount = reader.ReadUInt32();
			chunk.Children.Add(new UInt32Component(playlistSegmentCount));
			for (var i = 0; i < playlistSegmentCount; i++)
			{
				chunk.Children.Add(new PlaylistSegmentComponent(reader));
			}
		}

		private static void SamplerChunkReadProcedure(RiffChunk chunk, BinaryReader reader)
		{
			var samplerComponent = new SamplerComponent(reader);
			chunk.Children.Add(samplerComponent);
			for (var i = 0; i < samplerComponent.LoopCount; i++)
			{
				chunk.Children.Add(new SampleLoopComponent(reader));
			}

			if (samplerComponent.ExtraDataSize > 0)
				chunk.Children.Add(new ByteArrayComponent(reader, (int)samplerComponent.ExtraDataSize));
		}

		private static void SilentChunkReadProcedure(RiffChunk chunk, BinaryReader reader)
		{
			var silentSampleCount = reader.ReadUInt32();
			chunk.Children.Add(new UInt32Component(silentSampleCount));
		}
		#endregion

		#region Subchunk Write Procedures
		private static void AssociatedDataListChunkWriteProcedure(RiffChunk chunk, BinaryWriter writer)
		{
			var listChunk = chunk as ListChunk;
			if (listChunk == null)
				throw new FormatException("\"chunk\" must be a ListChunk");
		}

		private static void WaveListChunkWriteProcedure(RiffChunk chunk, BinaryWriter writer)
		{
			var listChunk = chunk as ListChunk;
			if (listChunk == null)
				throw new FormatException("\"chunk\" must be a ListChunk");
		}

		private static void CueChunkWriteProcedure(RiffChunk chunk, BinaryWriter writer) { }

		private static void DataChunkWriteProcedure(RiffChunk chunk, BinaryWriter writer) { }

		private static void FactChunkWriteProcedure(RiffChunk chunk, BinaryWriter writer) { }

		private static void FormatChunkWriteProcedure(RiffChunk chunk, BinaryWriter writer) { }

		private static void InstrumentChunkWriteProcedure(RiffChunk chunk, BinaryWriter writer) { }

		private static void LabelChunkWriteProcedure(RiffChunk chunk, BinaryWriter writer) { }

		private static void LabeledTextChunkWriteProcedure(RiffChunk chunk, BinaryWriter writer) { }

		private static void NoteChunkWriteProcedure(RiffChunk chunk, BinaryWriter writer) { }

		private static void PlaylistChunkWriteProcedure(RiffChunk chunk, BinaryWriter writer) { }

		private static void SamplerChunkWriteProcedure(RiffChunk chunk, BinaryWriter writer) { }

		private static void SilentChunkWriteProcedure(RiffChunk chunk, BinaryWriter writer) { }
		#endregion

		#region Subchunk Validation Procedures
		private static void AssociatedDataListChunkValidationProcedure(RiffChunk chunk, ref ValidationResult result)
		{
			var listChunk = chunk as ListChunk;
			if (listChunk == null)
				throw new FormatException("\"chunk\" must be a ListChunk");
		}

		private static void WaveListChunkValidationProcedure(RiffChunk chunk, ref ValidationResult result)
		{
			var listChunk = chunk as ListChunk;
			if (listChunk == null)
				throw new FormatException("\"chunk\" must be a ListChunk");
		}

		private static void CueChunkValidationProcedure(RiffChunk chunk, ref ValidationResult result) { }

		private static void DataChunkValidationProcedure(RiffChunk chunk, ref ValidationResult result) { }

		private static void FactChunkValidationProcedure(RiffChunk chunk, ref ValidationResult result) { }

		private static void FormatChunkValidationProcedure(RiffChunk chunk, ref ValidationResult result) { }

		private static void InstrumentChunkValidationProcedure(RiffChunk chunk, ref ValidationResult result) { }

		private static void LabelChunkValidationProcedure(RiffChunk chunk, ref ValidationResult result) { }

		private static void LabeledTextChunkValidationProcedure(RiffChunk chunk, ref ValidationResult result) { }

		private static void NoteChunkValidationProcedure(RiffChunk chunk, ref ValidationResult result) { }

		private static void PlaylistChunkValidationProcedure(RiffChunk chunk, ref ValidationResult result) { }

		private static void SamplerChunkValidationProcedure(RiffChunk chunk, ref ValidationResult result) { }

		private static void SilentChunkValidationProcedure(RiffChunk chunk, ref ValidationResult result) { }
		#endregion

		#region Method Overrides (System.Object)
		public override string ToString()
		{
			return $"{Id}:{TypeId}";
		}
		#endregion

		#region Method Overrides (Disposable)
		protected override void Dispose(bool disposing)
		{
			if (!IsDisposed)
			{
				if (disposing)
				{
					AudioData.Dispose();
					AudioData = null;
				}
			}

			IsDisposed = true;
			base.Dispose(disposing);
		}
		#endregion
	}
}