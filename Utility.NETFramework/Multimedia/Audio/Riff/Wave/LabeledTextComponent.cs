// ┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  FILE:       LabeledTextComponent.cs
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
using System.IO;

namespace JLR.Utility.NETFramework.Multimedia.Audio.Riff.Wave
{
	#region Enumerated Types
	public enum CountryCode : ushort
	{
		None          = 0,
		Usa           = 1,
		Canada        = 2,
		LatinAmerica  = 3,
		Greece        = 30,
		Netherlands   = 31,
		Belgium       = 32,
		France        = 33,
		Spain         = 34,
		Italy         = 39,
		Switzerland   = 41,
		Austria       = 43,
		UnitedKingdom = 44,
		Denmark       = 45,
		Sweden        = 46,
		Norway        = 47,
		Germany       = 49,
		Mexico        = 52,
		Brazil        = 55,
		Australia     = 61,
		NewZealand    = 64,
		Japan         = 81,
		Korea         = 82,
		China         = 86,
		Taiwan        = 88,
		Turkey        = 90,
		Portugal      = 351,
		Luxembourg    = 352,
		Iceland       = 354,
		Finland       = 358
	}

	public enum LanguageCode : ushort
	{
		None          = 0,
		Arabic        = 1,
		Bulgarian     = 2,
		Catalan       = 3,
		Chinese       = 4,
		Czech         = 5,
		Danish        = 6,
		German        = 7,
		Greek         = 8,
		English       = 9,
		Spanish       = 10,
		Finnish       = 11,
		French        = 12,
		Hebrew        = 13,
		Hungarian     = 14,
		Icelandic     = 15,
		Italian       = 16,
		Japanese      = 17,
		Korean        = 18,
		Dutch         = 19,
		Norwegian     = 20,
		Polish        = 21,
		Portugese     = 22,
		RhaetoRomanic = 23,
		Romanian      = 24,
		Russian       = 25,
		SerboCroatian = 26,
		Slovak        = 27,
		Albanian      = 28,
		Swedish       = 29,
		Thai          = 30,
		Turkish       = 31,
		Urdu          = 32,
		Bahasa        = 33
	}
	#endregion

	public sealed class LabeledTextComponent : RiffComponent
	{
		#region Properties
		public          uint         CueId        { get; set; }
		public          uint         SampleLength { get; set; }
		public          FourCc       Purpose      { get; set; }
		public          CountryCode  Country      { get; set; }
		public          LanguageCode Language     { get; set; }
		public          ushort       Dialect      { get; set; }
		public          ushort       CodePage     { get; set; }
		public override long         Size         => 20;
		#endregion

		#region Constructors
		public LabeledTextComponent()
		{
			CueId        = 0;
			SampleLength = 0;
			Purpose      = FourCc.Zero;
			Country      = CountryCode.None;
			Language     = LanguageCode.None;
			Dialect      = 0;
			CodePage     = 0;
		}

		public LabeledTextComponent(BinaryReader reader)
		{
			Read(reader);
		}
		#endregion

		#region Internal Methods
		public override void Read(BinaryReader reader)
		{
			CueId        = reader.ReadUInt32();
			SampleLength = reader.ReadUInt32();
			Purpose      = reader.ReadFourCc();
			Country      = (CountryCode)reader.ReadUInt16();
			Language     = (LanguageCode)reader.ReadUInt16();
			Dialect      = reader.ReadUInt16();
			CodePage     = reader.ReadUInt16();
		}

		public override void Write(BinaryWriter writer)
		{
			writer.Write(CueId);
			writer.Write(SampleLength);
			writer.Write(Purpose.ToUInt32());
			writer.Write((ushort)Country);
			writer.Write((ushort)Language);
			writer.Write(Dialect);
			writer.Write(CodePage);
		}
		#endregion

		#region Interface Implementation (IValidatable)
		public override ValidationResult Validate()
		{
			var result = new ValidationResult();

			// Purpose
			if (Purpose == FourCc.Zero)
				result.EditValidity(Validity.Unknown, "\"Purpose\" has not been set");

			// Country
			/*if (Country == CountryCode.None)
				result.EditValidity(Validity.Unknown, "\"Country\" has not been set");*/
			if (!Enum.IsDefined(typeof(CountryCode), Country))
				result.EditValidity(Validity.Invalid, $"Unrecognized country code: {Country}");

			// Language
			/*if (Language == LanguageCode.None)
				result.EditValidity(Validity.Unknown, "\"Language\" has not been set");*/
			if (!Enum.IsDefined(typeof(LanguageCode), Language))
				result.EditValidity(Validity.Invalid, $"Unrecognized language code: {Language}");

			return result;
		}
		#endregion

		#region Method Overrides (System.Object)
		public override string ToString()
		{
			if (IsValid)
			{
				return $"[Labeled Text] {Purpose}{(Purpose != FourCc.Zero ? ", " : String.Empty)}Cue ID: {CueId}";
			}

			return Validate().ToString();
		}
		#endregion
	}
}