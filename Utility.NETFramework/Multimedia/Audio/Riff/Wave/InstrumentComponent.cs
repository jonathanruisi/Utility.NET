// ┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  FILE:       InstrumentComponent.cs
// ┃  PROJECT:    Utility.NET
// ┃  SOLUTION:   Utility.NET
// ┃  CREATED:    2018-03-13 @ 7:18 PM
// ┣━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  AUTHOR:     Jonathan Ruisi
// ┃  EMAIL:      JonathanRuisi@gmail.com
// ┣━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  GIT REPO:   https://github.com/jonathanruisi/Utility.NET
// ┃  LICENSE:    https://github.com/jonathanruisi/Utility.NET/blob/master/LICENSE
// ┗━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

using System;
using System.Globalization;
using System.IO;

using JLR.Utility.NET.Multimedia.Music;

namespace JLR.Utility.NET.Multimedia.Audio.Riff.Wave
{
	public sealed class InstrumentComponent : RiffComponent
	{
		#region Fields
		private MusicNote _unshiftedNote;
		#endregion

		#region Properties
		public sbyte UnshiftedNote
		{
			get { return (sbyte)_unshiftedNote.NoteNumber; }
			set { _unshiftedNote = MusicNote.FromNoteNumber(value); }
		}

		public          sbyte FineTune     { get; set; }
		public          sbyte Gain         { get; set; }
		public          sbyte LowNote      { get; set; }
		public          sbyte HighNote     { get; set; }
		public          sbyte LowVelocity  { get; set; }
		public          sbyte HighVelocity { get; set; }
		public override long  Size         { get { return 7; } }
		#endregion

		#region Constructors
		public InstrumentComponent()
		{
			_unshiftedNote = MusicNote.FromNoteNumber(0);
			FineTune       = 0;
			Gain           = 0;
			LowNote        = 0;
			HighNote       = 127;
			LowVelocity    = 1;
			HighVelocity   = 127;
		}

		public InstrumentComponent(BinaryReader reader)
		{
			Read(reader);
		}
		#endregion

		#region Internal Methods
		public override void Read(BinaryReader reader)
		{
			_unshiftedNote = MusicNote.FromNoteNumber(reader.ReadSByte());
			FineTune       = reader.ReadSByte();
			Gain           = reader.ReadSByte();
			LowNote        = reader.ReadSByte();
			HighNote       = reader.ReadSByte();
			LowVelocity    = reader.ReadSByte();
			HighVelocity   = reader.ReadSByte();
		}

		public override void Write(BinaryWriter writer)
		{
			writer.Write((sbyte)_unshiftedNote.NoteNumber);
			writer.Write(FineTune);
			writer.Write(Gain);
			writer.Write(LowNote);
			writer.Write(HighNote);
			writer.Write(LowVelocity);
			writer.Write(HighVelocity);
		}
		#endregion

		#region Interface Implementation (IValidatable)
		public override ValidationResult Validate()
		{
			var result = new ValidationResult();

			// UnshiftedNote
			if (_unshiftedNote.NoteNumber < 0 || _unshiftedNote.NoteNumber > 127)
				result.EditValidity(Validity.Invalid, "\"UnshiftedNote\" must be between 0 and 127");

			// FineTune
			if (FineTune < -50 || FineTune > 50)
				result.EditValidity(Validity.Invalid, "\"FineTune\" must be between -50 and 50");

			// Gain
			if (Gain < -64 || Gain > 64)
				result.EditValidity(Validity.Invalid, "\"Gain\" must be between -64 and 64");

			// LowNote
			if (LowNote < 0 || LowNote > 127)
				result.EditValidity(Validity.Invalid, "\"LowNote\" must be between 0 and 127");

			// HighNote
			if (HighNote < 0 || HighNote > 127)
				result.EditValidity(Validity.Invalid, "\"HighNote\" must be between 0 and 127");

			// LowVelocity
			if (LowVelocity < 1 || LowVelocity > 127)
				result.EditValidity(Validity.Invalid, "\"LowVelocity\" must be between 1 and 127");

			// HighVelocity
			if (HighVelocity < 1 || HighVelocity > 127)
				result.EditValidity(Validity.Invalid, "\"HighVelocity\" must be between 1 and 127");

			return result;
		}
		#endregion

		#region Method Overrides (System.Object)
		public override string ToString()
		{
			if (IsValid)
			{
				return String.Format(
					"Instrument ({0} {1}{2})",
					_unshiftedNote,
					FineTune == 0 ? String.Empty : FineTune < 0 ? " -" : " +",
					FineTune == 0 ? String.Empty : System.Math.Abs(FineTune).ToString(CultureInfo.InvariantCulture));
			}

			return Validate().ToString();
		}
		#endregion
	}
}