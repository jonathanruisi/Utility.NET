// ┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  FILE:       CuePointComponent.cs
// ┃  PROJECT:    Utility.NET
// ┃  SOLUTION:   Utility.NET
// ┃  CREATED:    2018-03-13 @ 7:17 PM
// ┣━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  AUTHOR:     Jonathan Ruisi
// ┃  EMAIL:      JonathanRuisi@gmail.com
// ┣━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  GIT REPO:   https://github.com/jonathanruisi/Utility.NET
// ┃  LICENSE:    https://github.com/jonathanruisi/Utility.NET/blob/master/LICENSE
// ┗━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

using System;
using System.IO;

namespace JLR.Utility.NET.Multimedia.Audio.Riff.Wave
{
	public sealed class CuePointComponent : RiffComponent
	{
		#region Properties
		public          uint   CueId        { get; set; }
		public          uint   PlayOrder    { get; set; }
		public          FourCc ChunkId      { get; set; }
		public          uint   ChunkStart   { get; set; }
		public          uint   BlockStart   { get; set; }
		public          uint   SampleOffset { get; set; }
		public          string Name         { get; set; }
		public          string Comment      { get; set; }
		public override long   Size         => 24;
		#endregion

		#region Constructors
		public CuePointComponent()
		{
			CueId        = 0;
			PlayOrder    = 0;
			ChunkId      = FourCc.Zero;
			ChunkStart   = 0;
			BlockStart   = 0;
			SampleOffset = 0;
			Name         = String.Empty;
			Comment      = String.Empty;
		}

		public CuePointComponent(BinaryReader reader)
		{
			Read(reader);
		}
		#endregion

		#region Internal Methods
		public override void Read(BinaryReader reader)
		{
			CueId        = reader.ReadUInt32();
			PlayOrder    = reader.ReadUInt32();
			ChunkId      = reader.ReadFourCc();
			ChunkStart   = reader.ReadUInt32();
			BlockStart   = reader.ReadUInt32();
			SampleOffset = reader.ReadUInt32();
		}

		public override void Write(BinaryWriter writer)
		{
			writer.Write(CueId);
			writer.Write(PlayOrder);
			writer.Write(ChunkId.ToUInt32());
			writer.Write(ChunkStart);
			writer.Write(BlockStart);
			writer.Write(SampleOffset);
		}
		#endregion

		#region Interface Implementation (IValidatable)
		public override ValidationResult Validate()
		{
			var result = new ValidationResult();

			// ChunkId
			if (ChunkId == FourCc.Zero)
				result.EditValidity(Validity.Invalid, "\"ChunkId\" must be greater than zero");

			return result;
		}
		#endregion

		#region Method Overrides (System.Object)
		public override string ToString()
		{
			return IsValid ? $"[Cue Point] ID: {CueId}" : Validate().ToString();
		}
		#endregion
	}
}