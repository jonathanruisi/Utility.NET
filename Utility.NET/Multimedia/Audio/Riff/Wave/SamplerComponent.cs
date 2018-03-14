// ┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  FILE:       SamplerComponent.cs
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

using JLR.Utility.NET.Multimedia.Music;

namespace JLR.Utility.NET.Multimedia.Audio.Riff.Wave
{
	#region Enumerated Types
	public enum SmpteFormat
	{
		None      = 0,
		Fps24     = 24,
		Fps25     = 25,
		Fps30     = 29,
		Fps30Drop = 30
	}
	#endregion

	public sealed class SamplerComponent : RiffComponent
	{
		#region Fields
		private MusicNote _unityNote;
		private Timecode  _timecode;
		#endregion

		#region Properties
		public uint UnityNote
		{
			get { return (uint)_unityNote.NoteNumber; }
			set { _unityNote = MusicNote.FromNoteNumber((int)value); }
		}

		public SmpteFormat SmpteFormat
		{
			get
			{
				switch (_timecode.FrameRate)
				{
					case 24:
						return SmpteFormat.Fps24;
					case 25:
						return SmpteFormat.Fps25;
					case 30:
						return _timecode.CountingMode == CountingMode.Drop ? SmpteFormat.Fps30Drop : SmpteFormat.Fps30;
					default:
						return SmpteFormat.None;
				}
			}
			set
			{
				switch (value)
				{
					case SmpteFormat.Fps24:
						_timecode.FrameRate    = 24;
						_timecode.TimeBase     = TimeBase.Real;
						_timecode.CountingMode = CountingMode.NonDrop;
						break;
					case SmpteFormat.Fps25:
						_timecode.FrameRate    = 25;
						_timecode.TimeBase     = TimeBase.Real;
						_timecode.CountingMode = CountingMode.NonDrop;
						break;
					case SmpteFormat.Fps30:
						_timecode.FrameRate    = 30;
						_timecode.TimeBase     = TimeBase.Ntsc;
						_timecode.CountingMode = CountingMode.NonDrop;
						break;
					case SmpteFormat.Fps30Drop:
						_timecode.FrameRate    = 30;
						_timecode.TimeBase     = TimeBase.Ntsc;
						_timecode.CountingMode = CountingMode.Drop;
						break;
					default:
						_timecode.FrameRate    = 0;
						_timecode.TimeBase     = TimeBase.Real;
						_timecode.CountingMode = CountingMode.NonDrop;
						break;
				}
			}
		}

		public uint SmpteOffset
		{
			get { return _timecode.FrameRate == 0 ? 0 : _timecode.ToIntegerRepresentation(); }
			set { _timecode.AbsoluteTime = value == 0 ? 0 : (Timecode.FromIntegerRepresentation(value)).AbsoluteTime; }
		}

		public          uint Manufacturer  { get; set; }
		public          uint Product       { get; set; }
		public          uint SamplePeriod  { get; set; }
		public          uint PitchFraction { get; set; }
		public          uint LoopCount     { get; set; }
		public          uint ExtraDataSize { get; set; }
		public override long Size          { get { return 36; } }
		#endregion

		#region Constructors
		public SamplerComponent()
		{
			Manufacturer  = 0;
			Product       = 0;
			SamplePeriod  = 0;
			UnityNote     = 0;
			PitchFraction = 0;
			_timecode     = Timecode.Zero;
			LoopCount     = 0;
			ExtraDataSize = 0;
		}

		public SamplerComponent(BinaryReader reader)
		{
			Read(reader);
		}
		#endregion

		#region Internal Methods
		public override void Read(BinaryReader reader)
		{
			Manufacturer  = reader.ReadUInt32();
			Product       = reader.ReadUInt32();
			SamplePeriod  = reader.ReadUInt32();
			UnityNote     = reader.ReadUInt32();
			PitchFraction = reader.ReadUInt32();
			SmpteFormat   = (SmpteFormat)reader.ReadUInt32();
			SmpteOffset   = reader.ReadUInt32();
			LoopCount     = reader.ReadUInt32();
			ExtraDataSize = reader.ReadUInt32();
		}

		public override void Write(BinaryWriter writer)
		{
			writer.Write(Manufacturer);
			writer.Write(Product);
			writer.Write(SamplePeriod);
			writer.Write(UnityNote);
			writer.Write(PitchFraction);
			writer.Write((uint)SmpteFormat);
			writer.Write(SmpteOffset);
			writer.Write(LoopCount);
			writer.Write(ExtraDataSize);
		}
		#endregion

		#region Interface Implementation (IValidatable)
		public override ValidationResult Validate()
		{
			var result = new ValidationResult();

			// UnityNote
			if (_unityNote.NoteNumber > 127)
				result.EditValidity(Validity.Invalid, "\"UnityNote\" must be between 0 and 127");

			// Timecode
			if (SmpteFormat == SmpteFormat.None && SmpteOffset > 0)
				result.EditValidity(Validity.Invalid, "\"SmpteOffset\" must = 0 if \"SmpteFormat\" = 0 (None)");

			return result;
		}
		#endregion

		#region Method Overrides (System.Object)
		public override string ToString()
		{
			if (IsValid)
			{
				return String.Format(
					"Sampler ({0}, Period: {1}ns, {2} Loop{3})",
					_unityNote,
					SamplePeriod,
					LoopCount,
					LoopCount == 1 ? String.Empty : "s");
			}

			return Validate().ToString();
		}
		#endregion
	}
}