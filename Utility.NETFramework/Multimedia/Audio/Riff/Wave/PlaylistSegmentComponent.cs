// ┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  FILE:       PlaylistSegmentComponent.cs
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

namespace JLR.Utility.NET.Multimedia.Audio.Riff.Wave
{
	public sealed class PlaylistSegmentComponent : RiffComponent
	{
		#region Properties
		public          uint              CueId     { get; set; }
		public          uint              Length    { get; set; }
		public          uint              LoopCount { get; set; }
		public          CuePointComponent CuePoint  { get; set; }
		public override long              Size      => 12;
		#endregion

		#region Constructors
		public PlaylistSegmentComponent()
		{
			CueId     = 0;
			Length    = 0;
			LoopCount = 0;
			CuePoint  = null;
		}

		public PlaylistSegmentComponent(BinaryReader reader)
		{
			Read(reader);
		}
		#endregion

		#region Internal Methods
		public override void Read(BinaryReader reader)
		{
			CueId     = reader.ReadUInt32();
			Length    = reader.ReadUInt32();
			LoopCount = reader.ReadUInt32();
		}

		public override void Write(BinaryWriter writer)
		{
			writer.Write(CueId);
			writer.Write(Length);
			writer.Write(LoopCount);
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
			return
				$"[Playlist Segment] Cue ID: {CueId} ({Length} samples, {LoopCount} loop{(LoopCount == 1 ? String.Empty : "s")})";
		}
		#endregion
	}
}