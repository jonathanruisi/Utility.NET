// ┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  FILE:       SampleLoopComponent.cs
// ┃  PROJECT:    Utility.NET
// ┃  SOLUTION:   Utility.NET
// ┃  CREATED:    2018-03-13 @ 7:19 PM
// ┣━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  AUTHOR:     Jonathan Ruisi
// ┃  EMAIL:      JonathanRuisi@gmail.com
// ┣━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  GIT REPO:   https://github.com/jonathanruisi/Utility.NET
// ┃  LICENSE:    https://github.com/jonathanruisi/Utility.NET/blob/master/LICENSE
// ┗━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

using System;
using System.IO;

namespace JLR.Utility.NETFramework.Multimedia.Audio.Riff.Wave
{
	#region Enumerated Types
	public enum LoopType
	{
		Forward     = 0,
		Alternating = 1,
		Backward    = 2
	}
	#endregion

	public sealed class SampleLoopComponent : RiffComponent
	{
		#region Properties
		public          uint              CueId       { get; set; }
		public          LoopType          LoopType    { get; set; }
		public          uint              StartOffset { get; set; }
		public          uint              EndOffset   { get; set; }
		public          uint              Fraction    { get; set; }
		public          uint              PlayCount   { get; set; }
		public          CuePointComponent CuePoint    { get; set; }
		public override long              Size        => 24;
		#endregion

		#region Constructors
		public SampleLoopComponent()
		{
			CueId       = 0;
			LoopType    = LoopType.Forward;
			StartOffset = 0;
			EndOffset   = 0;
			Fraction    = 0;
			PlayCount   = 0;
			CuePoint    = null;
		}

		public SampleLoopComponent(BinaryReader reader)
		{
			Read(reader);
		}
		#endregion

		#region Internal Methods
		public override void Read(BinaryReader reader)
		{
			CueId       = reader.ReadUInt32();
			LoopType    = (LoopType)reader.ReadUInt32();
			StartOffset = reader.ReadUInt32();
			EndOffset   = reader.ReadUInt32();
			Fraction    = reader.ReadUInt32();
			PlayCount   = reader.ReadUInt32();
		}

		public override void Write(BinaryWriter writer)
		{
			writer.Write(CueId);
			writer.Write((uint)LoopType);
			writer.Write(StartOffset);
			writer.Write(EndOffset);
			writer.Write(Fraction);
			writer.Write(PlayCount);
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
			var loopTypeString = String.Empty;
			if (Enum.IsDefined(typeof(LoopType), LoopType))
				loopTypeString = Enum.GetName(typeof(LoopType), LoopType);
			return
				$"[Sample Loop] Cue ID: {CueId} ({(String.IsNullOrEmpty(loopTypeString) ? String.Empty : loopTypeString)}{(String.IsNullOrEmpty(loopTypeString) ? String.Empty : " ")}{PlayCount}X)";
		}
		#endregion
	}
}