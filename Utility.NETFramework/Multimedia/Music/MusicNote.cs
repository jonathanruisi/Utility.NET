// ┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  FILE:       MusicNote.cs
// ┃  PROJECT:    Utility.NET
// ┃  SOLUTION:   Utility.NET
// ┃  CREATED:    2018-03-13 @ 6:56 PM
// ┣━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  AUTHOR:     Jonathan Ruisi
// ┃  EMAIL:      JonathanRuisi@gmail.com
// ┣━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  GIT REPO:   https://github.com/jonathanruisi/Utility.NET
// ┃  LICENSE:    https://github.com/jonathanruisi/Utility.NET/blob/master/LICENSE
// ┗━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

using System;
using System.Text;

namespace JLR.Utility.NETFramework.Multimedia.Music
{
	#region Enumerated Types
	/// <summary>
	/// Represents common time signatures.  The following format can be used to describe any time signature:
	/// Time Signature = BBDD
	/// Where BB is the number of notes per measure (0-255),
	/// and DD is the duration of each note (fractions of a beat).
	/// This is represented as a 16-bit unsigned integer where MSB = BB and LSB = DD.
	/// For example, a 3/4 time signature would be represented as follows:
	/// 3/4 = BB/DD = 0xBBDD = 0x0304
	/// </summary>
	public enum TimeSignature : ushort
	{
		TwoTwo      = 0x0202,
		ThreeTwo    = 0x0302,
		FourTwo     = 0x0402,
		TwoFour     = 0x0204,
		ThreeFour   = 0x0304,
		FourFour    = 0x0404,
		ThreeEight  = 0x0308,
		SixEight    = 0x0608,
		NineEight   = 0x0908,
		TwelveEight = 0x0C08
	}

	public enum RhythmicUnit
	{
		Whole        = 0x01,
		Half         = 0x02,
		Quarter      = 0x04,
		Eighth       = 0x08,
		Sixteenth    = 0x10,
		ThirtySecond = 0x20,
		SixtyFourth  = 0x40
	}

	public enum Note
	{
		C      = 0x0,
		CSharp = 0x1,
		D      = 0x2,
		DSharp = 0x3,
		E      = 0x4,
		F      = 0x5,
		FSharp = 0x6,
		G      = 0x7,
		GSharp = 0x8,
		A      = 0x9,
		ASharp = 0xA,
		B      = 0xB
	}
	#endregion

	public struct MusicNote : IEquatable<MusicNote>, IComparable<MusicNote>, IValidatable
	{
		#region Constants
		private const double FrequencyA4  = 440;
		private const double SpeedOfSound = 343.42;
		#endregion

		#region Fields
		private Note         _note;
		private int          _octave;
		private RhythmicUnit _rhythmicUnit;
		private int          _dotCount;
		#endregion

		#region Properties
		public Note         Note         { get { return _note; }         set { _note         = value; } }
		public int          Octave       { get { return _octave; }       set { _octave       = value; } }
		public RhythmicUnit RhythmicUnit { get { return _rhythmicUnit; } set { _rhythmicUnit = value; } }
		public int          DotCount     { get { return _dotCount; }     set { _dotCount     = value; } }
		public int          NoteNumber   => ToNoteNumber();
		public double       Frequency    => ToFrequency();
		public double       Wavelength   => ToWavelength();
		public double       TotalBeats   => ToDuration();
		#endregion

		#region Static Properties
		public static MusicNote A4 => new MusicNote(Note.A);
		#endregion

		#region Constructors
		public MusicNote(Note note, int octave = 4, RhythmicUnit rhythmicUnit = RhythmicUnit.Whole, int dotCount = 0)
		{
			_note         = note;
			_octave       = octave;
			_rhythmicUnit = rhythmicUnit;
			_dotCount     = dotCount;
		}
		#endregion

		#region Public Methods
		public MusicNote Add(MusicNote other)
		{
			return FromNoteNumber(NoteNumber + other.NoteNumber);
		}

		public MusicNote Subtract(MusicNote other)
		{
			return FromNoteNumber(NoteNumber - other.NoteNumber);
		}

		public MusicNote Multiply(MusicNote other)
		{
			return FromNoteNumber(NoteNumber * other.NoteNumber);
		}

		public MusicNote Divide(MusicNote other)
		{
			return FromNoteNumber(NoteNumber / other.NoteNumber);
		}

		public MusicNote Increment()
		{
			return FromNoteNumber(NoteNumber + 1);
		}

		public MusicNote Decrement()
		{
			return FromNoteNumber(NoteNumber - 1);
		}
		#endregion

		#region Public Static Methods
		public static MusicNote FromNoteNumber(int noteNumber)
		{
			var result = new MusicNote { Octave = noteNumber / 12 };
			result.Note = (Note)(noteNumber - (result.Octave * 12));
			return result;
		}

		public static MusicNote FromFrequency(double frequencyInHertz)
		{
			if (frequencyInHertz < 0)
				throw new ArgumentOutOfRangeException(nameof(frequencyInHertz), "Specified frequency must be greater than zero");

			int distanceFromA4 = Convert.ToInt32(
				System.Math.Round(
					(12 * System.Math.Log(frequencyInHertz / FrequencyA4)) / System.Math.Log(2),
					0,
					MidpointRounding.AwayFromZero));
			return FromNoteNumber(CalculateNoteNumber(Note.A, 4) + distanceFromA4);
		}

		public static MusicNote FromWavelength(double wavelengthInCentimeters)
		{
			if (wavelengthInCentimeters < 0)
				throw new ArgumentOutOfRangeException(
					nameof(wavelengthInCentimeters),
					"Specified wavelength must be greater than zero");

			return FromFrequency((SpeedOfSound * 100) / wavelengthInCentimeters);
		}
		#endregion

		#region Private Methods
		private int ToNoteNumber()
		{
			return CalculateNoteNumber(_note, _octave);
		}

		private double ToFrequency()
		{
			return CalculateFrequencyInHertz(_note, _octave);
		}

		private double ToWavelength()
		{
			return CalculateWavelengthInCentimeters(ToFrequency());
		}

		private double ToDuration()
		{
			return CalculateDurationInBeats(_rhythmicUnit, _dotCount);
		}
		#endregion

		#region Private Static Methods
		private static double CalculateFrequencyInHertz(Note note, int octave)
		{
			int distanceFromA4 = CalculateNoteNumber(note, octave) - CalculateNoteNumber(Note.A, 4);
			return FrequencyA4 * System.Math.Pow(System.Math.Pow(2, 1 / 12D), distanceFromA4);
		}

		private static double CalculateWavelengthInCentimeters(double frequency)
		{
			return (SpeedOfSound / frequency) * 100;
		}

		private static double CalculateDurationInBeats(RhythmicUnit rhythmicUnit, int dotCount)
		{
			return (2 * (1 / (double)rhythmicUnit)) - ((1 / (double)rhythmicUnit) / System.Math.Pow(2, dotCount));
		}

		private static int CalculateNoteNumber(Note note, int octave)
		{
			return (octave * 12) + (int)note;
		}
		#endregion

		#region Interface Implementation (IEquatable<>)
		public bool Equals(MusicNote other)
		{
			return _note == other._note && _octave == other._octave && _rhythmicUnit == other._rhythmicUnit &&
				_dotCount == other._dotCount;
		}
		#endregion

		#region Interface Implementation (IComparable<>)
		public int CompareTo(MusicNote other)
		{
			if (CalculateNoteNumber(_note, _octave) < CalculateNoteNumber(other._note, other._octave)) return -1;
			if (CalculateNoteNumber(_note, _octave) > CalculateNoteNumber(other._note, other._octave)) return 1;
			return 0;
		}
		#endregion

		#region Interface Implementation (IValidatable)
		public bool IsValid => Validate();

		public ValidationResult Validate()
		{
			var result = new ValidationResult();

			// Note
			if (!Enum.IsDefined(typeof(Note), _note))
				result.EditValidity(Validity.Invalid, "The specified \"Note\" is not recognized");

			// RhythmicUnit
			if (_rhythmicUnit < 0)
				result.EditValidity(Validity.Invalid, "\"RhythmicUnit\" must be greater than zero");

			// DotCount
			if (_dotCount < 0)
				result.EditValidity(Validity.Invalid, "\"DotCount\" must be greater than zero");

			return result;
		}
		#endregion

		#region Method Overrides (System.ValueType)
		public override bool Equals(object obj)
		{
			if (obj is MusicNote)
				return Equals((MusicNote)obj);
			return false;
		}

		public override int GetHashCode()
		{
			return (int)Note ^ (int)RhythmicUnit ^ Octave ^ DotCount;
		}

		public override string ToString()
		{
			if (IsValid)
			{
				var    result     = new StringBuilder();
				string noteString = Enum.GetName(typeof(Note), _note);
				if (!String.IsNullOrEmpty(noteString))
				{
					result.Append(noteString[0]);
					if (noteString.Length > 1) result.Append('#');
					result.Append(_octave);
				}

				return result.ToString();
			}

			return Validate().ToString();
		}
		#endregion

		#region Operator Overloads (Comparison)
		public static bool operator ==(MusicNote lhs, MusicNote rhs)
		{
			return lhs.Equals(rhs);
		}

		public static bool operator !=(MusicNote lhs, MusicNote rhs)
		{
			return !lhs.Equals(rhs);
		}

		public static bool operator <(MusicNote lhs, MusicNote rhs)
		{
			return lhs.CompareTo(rhs) < 0;
		}

		public static bool operator >(MusicNote lhs, MusicNote rhs)
		{
			return lhs.CompareTo(rhs) > 0;
		}

		public static bool operator <=(MusicNote lhs, MusicNote rhs)
		{
			return lhs.CompareTo(rhs) <= 0;
		}

		public static bool operator >=(MusicNote lhs, MusicNote rhs)
		{
			return lhs.CompareTo(rhs) >= 0;
		}
		#endregion

		#region Operator Overloads (Unary)
		public static MusicNote operator ++(MusicNote value)
		{
			return value.Increment();
		}

		public static MusicNote operator --(MusicNote value)
		{
			return value.Decrement();
		}
		#endregion

		#region Operator Overloads (Binary)
		public static MusicNote operator +(MusicNote lhs, MusicNote rhs)
		{
			return lhs.Add(rhs);
		}

		public static MusicNote operator -(MusicNote lhs, MusicNote rhs)
		{
			return lhs.Subtract(rhs);
		}

		public static MusicNote operator *(MusicNote lhs, MusicNote rhs)
		{
			return lhs.Multiply(rhs);
		}

		public static MusicNote operator /(MusicNote lhs, MusicNote rhs)
		{
			return lhs.Divide(rhs);
		}
		#endregion
	}
}