// ┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  FILE:       Timecode.cs
// ┃  PROJECT:    Utility.NET
// ┃  SOLUTION:   Utility.NET
// ┃  CREATED:    2015-12-23 @ 4:54 AM
// ┣━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  AUTHOR:     Jonathan Ruisi
// ┃  EMAIL:      JonathanRuisi@gmail.com
// ┣━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  GIT REPO:   https://github.com/jonathanruisi/Utility.NET
// ┃  LICENSE:    https://github.com/jonathanruisi/Utility.NET/blob/master/LICENSE
// ┗━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

using System;
using System.Text;

using JLR.Utility.NETFramework.Math;

namespace JLR.Utility.NETFramework.Multimedia
{
	#region Enumerated Types
	/// <summary>
	/// Specifies the time base.
	/// </summary>
	public enum TimeBase
	{
		/// <summary>
		/// Standard for most digital media and based on whole frames
		/// </summary>
		Real,

		/// <summary>
		/// National Television System Committee broadcast timing
		/// </summary>
		Ntsc
	}

	/// <summary>
	/// Specifies whether or not drop frame compensation is used
	/// in the context of an NTSC time base.
	/// </summary>
	public enum CountingMode
	{
		/// <summary>
		/// This mode accurately represents position with regard to frame count
		/// (every frame has a unique time code value),
		/// however temporal accuracy decreases over time.
		/// Depending on the media duration and specific application requirements,
		/// inaccuracies can often be neglected.
		/// </summary>
		NonDrop,

		/// <summary>
		/// This mode is temporally accurate, but does not represent an accurate frame count.
		/// Drop frame compensation can be thought of as the multimedia version of a leap year.
		/// Depending on specific application requirements, inaccuracies can often be neglected.
		/// </summary>
		Drop
	}

	/// <summary>
	/// Indicates the smallest unit by which a <see cref="Timecode"/> can be divided
	/// </summary>
	public enum UnitType
	{
		/// <summary>No further subdivision is possible beyond frames/milliseconds</summary>
		None,

		/// <summary>Samples per second</summary>
		Samples,
		Ticks
	}
	#endregion

	/// <summary>
	/// Represents an instant in time with respect to various multimedia formats.
	/// </summary>
	public struct Timecode : IEquatable<Timecode>, IComparable<Timecode>, IFormattable
	{
		#region Constants
		private const int          TicksPerSecond      = 10000000;
		private const int          DefaultFrameRate    = 30;
		private const int          DefaultUnitRate     = 48000;
		private const TimeBase     DefaultTimeBase     = TimeBase.Real;
		private const CountingMode DefaultCountingMode = CountingMode.NonDrop;
		private const UnitType     DefaultUnitType     = UnitType.None;
		#endregion

		#region Fields
		private static int          _roundingPrecision = 25;
		private        decimal      _absoluteTime;
		private        int          _frameRate, _unitRate;
		private        TimeBase     _timeBase;
		private        CountingMode _countingMode;
		#endregion

		#region Static Properties
		/// <summary>
		/// Gets or sets the current rounding precision (number of decimal places)
		/// </summary>
		public static int RoundingPrecision
		{
			get => _roundingPrecision;
			set
			{
				if (value < 0 || value > 28)
				{
					throw new ArgumentOutOfRangeException(
						nameof(value),
						"Rounding precision must be between 0 and 28 decimal places");
				}

				_roundingPrecision = value;
			}
		}

		/// <summary>
		/// Represents the <see cref="Timecode"/> zero value
		/// </summary>
		public static Timecode Zero => new Timecode(0M);
		#endregion

		#region Public Properties
		/// <summary>
		/// Gets or sets the minimum valid nonzero value in absolute time
		/// </summary>
		public decimal MinValue { get; private set; }

		/// <summary>
		/// Gets or sets the maximum valid value in absolute time
		/// </summary>
		public decimal MaxValue { get; private set; }

		public decimal AbsoluteTime
		{
			get => _absoluteTime;
			set
			{
				_absoluteTime = FrameRate == 0 ? 0 : value;
				ValidateAbsoluteTime();
			}
		}

		public int FrameRate
		{
			get => _frameRate;
			set
			{
				if (value == _frameRate) return;

				_frameRate = value;
				SetExtrema();
				ValidateAbsoluteTime();
			}
		}

		public TimeBase TimeBase
		{
			get => _timeBase;
			set
			{
				if (value == _timeBase) return;

				_timeBase = value;
				ValidateAndFixFormat(false);
				SetExtrema();
				ValidateAbsoluteTime();
			}
		}

		public CountingMode CountingMode
		{
			get => _countingMode;
			set
			{
				if (value == _countingMode) return;

				_countingMode = value;
				ValidateAndFixFormat(true);
				SetExtrema();
				ValidateAbsoluteTime();
			}
		}

		public int UnitRate
		{
			get => _unitRate;
			set
			{
				if (value == _unitRate) return;

				_unitRate = value;
				SetExtrema();
				ValidateAbsoluteTime();
			}
		}

		public UnitType UnitType { get; set; }

		public double TotalHours
		{
			get =>
				(double) decimal.Round(
					AbsoluteTimeToAbsoluteFrames(AbsoluteTime, FrameRate, TimeBase) / (FrameRate * 3600), 11);
			set => AbsoluteTime = AbsoluteFramesToAbsoluteTime((decimal) value * FrameRate * 3600, FrameRate, TimeBase);
		}

		public double TotalMinutes
		{
			get =>
				(double) decimal.Round(
					AbsoluteTimeToAbsoluteFrames(AbsoluteTime, FrameRate, TimeBase) / (FrameRate * 60), 11);
			set => AbsoluteTime = AbsoluteFramesToAbsoluteTime((decimal) value * FrameRate * 60, FrameRate, TimeBase);
		}

		public double TotalSeconds
		{
			get => (double) decimal.Round(AbsoluteTime, 11);
			set => AbsoluteTime = (decimal) value;
		}

		public double TotalMilliseconds
		{
			get => TotalSeconds * 1000D;
			set => AbsoluteTime = (decimal) value / 1000;
		}

		public double TotalFrames
		{
			get => (double) decimal.Round(AbsoluteTimeToAbsoluteFrames(AbsoluteTime, FrameRate, TimeBase), 11);
			set => AbsoluteTime = AbsoluteFramesToAbsoluteTime((decimal) value, FrameRate, TimeBase);
		}

		public double TotalUnits
		{
			get => (double) decimal.Round(AbsoluteTimeToAbsoluteUnits(AbsoluteTime, UnitRate), 11);
			set => AbsoluteTime = AbsoluteUnitsToAbsoluteTime((decimal) value, UnitRate);
		}

		public int Hours
		{
			get
			{
				AbsoluteTimeToTimecode(
					AbsoluteTime,
					FrameRate,
					TimeBase,
					CountingMode,
					out var hours,
					out _,
					out _,
					out _,
					out _);
				return hours;
			}
		}

		public int Minutes
		{
			get
			{
				AbsoluteTimeToTimecode(
					AbsoluteTime,
					FrameRate,
					TimeBase,
					CountingMode,
					out _,
					out var minutes,
					out _,
					out _,
					out _);
				return minutes;
			}
		}

		public int Seconds
		{
			get
			{
				AbsoluteTimeToTimecode(
					AbsoluteTime,
					FrameRate,
					TimeBase,
					CountingMode,
					out _,
					out _,
					out var seconds,
					out _,
					out _);
				return seconds;
			}
		}

		public int Milliseconds => (int) ((AbsoluteTime - (int) AbsoluteTime) * 1000);

		public int Frames
		{
			get
			{
				AbsoluteTimeToTimecode(
					AbsoluteTime,
					FrameRate,
					TimeBase,
					CountingMode,
					out _,
					out _,
					out _,
					out var frames,
					out _);
				return frames;
			}
		}

		public int Units
		{
			get
			{
				AbsoluteTimeToTimecodeWithUnits(
					AbsoluteTime,
					FrameRate,
					UnitRate,
					TimeBase,
					CountingMode,
					out _,
					out _,
					out _,
					out _,
					out var units,
					out _);
				return units;
			}
		}
		#endregion

		#region Constructors
		public Timecode(decimal absoluteTime,
						int frameRate = DefaultFrameRate,
						TimeBase timeBase = DefaultTimeBase,
						CountingMode countingMode = DefaultCountingMode,
						int unitRate = DefaultUnitRate,
						UnitType unitType = DefaultUnitType)
		{
			MinValue     = 0;
			MaxValue     = 0;
			_absoluteTime = absoluteTime;
			_frameRate    = frameRate;
			_timeBase     = timeBase;
			_countingMode = countingMode;
			_unitRate     = unitRate;
			UnitType     = unitType;
			ValidateFormat();
			SetExtrema();
			ValidateAbsoluteTime();
		}

		public Timecode(int hours,
						int minutes,
						int seconds,
						int frames,
						bool isNegative,
						int frameRate = DefaultFrameRate,
						TimeBase timeBase = DefaultTimeBase,
						CountingMode countingMode = DefaultCountingMode) : this(
			TimecodeToAbsoluteTime(frameRate, timeBase, countingMode, hours, minutes, seconds, frames, isNegative),
			frameRate,
			timeBase,
			countingMode)
		{
		}

		public Timecode(int hours,
						int minutes,
						int seconds,
						int frames,
						int units,
						bool isNegative,
						int frameRate = DefaultFrameRate,
						TimeBase timeBase = DefaultTimeBase,
						CountingMode countingMode = DefaultCountingMode,
						int unitRate = DefaultUnitRate,
						UnitType unitType = DefaultUnitType) : this(
			TimecodeWithUnitsToAbsoluteTime(
				frameRate,
				unitRate,
				timeBase,
				countingMode,
				hours,
				minutes,
				seconds,
				frames,
				units,
				isNegative),
			frameRate,
			timeBase,
			countingMode,
			unitRate,
			unitType)
		{
		}

		public Timecode(long totalUnits, int unitRate = DefaultUnitRate, UnitType unitType = DefaultUnitType) : this(
			AbsoluteUnitsToAbsoluteTime(totalUnits, unitRate),
			DefaultFrameRate,
			DefaultTimeBase,
			DefaultCountingMode,
			unitRate,
			unitType)
		{
		}

		public Timecode(TimeSpan duration) : this(
			AbsoluteUnitsToAbsoluteTime(duration.Ticks, TicksPerSecond),
			DefaultFrameRate,
			DefaultTimeBase,
			DefaultCountingMode,
			TicksPerSecond,
			UnitType.Ticks)
		{
		}
		#endregion

		#region Static Methods (Public)
		public static Timecode FromTimeSpan(TimeSpan value)
		{
			return new Timecode(value);
		}

		public static Timecode FromIntegerRepresentation(uint value)
		{
			var timecodeBytes = BitConverter.GetBytes(value);
			var isNegative    = timecodeBytes[3] >= 0x80;
			return new Timecode(
				isNegative ? 256 - timecodeBytes[3] : timecodeBytes[3],
				timecodeBytes[2],
				timecodeBytes[1],
				timecodeBytes[0],
				isNegative);
		}
		#endregion

		#region Methods (Public)
		public Timecode Add(Timecode other)
		{
			return new Timecode(AbsoluteTime + other.AbsoluteTime, FrameRate, TimeBase, CountingMode, UnitRate,
								UnitType);
		}

		public Timecode Subtract(Timecode other)
		{
			return new Timecode(AbsoluteTime - other.AbsoluteTime, FrameRate, TimeBase, CountingMode, UnitRate,
								UnitType);
		}

		public Timecode Multiply(Timecode other)
		{
			return new Timecode(AbsoluteTime * other.AbsoluteTime, FrameRate, TimeBase, CountingMode, UnitRate,
								UnitType);
		}

		public Timecode Divide(Timecode other)
		{
			return new Timecode(AbsoluteTime / other.AbsoluteTime, FrameRate, TimeBase, CountingMode, UnitRate,
								UnitType);
		}

		public Timecode Increment()
		{
			return new Timecode(
				AbsoluteTime + AbsoluteFramesToAbsoluteTime(1, FrameRate, TimeBase),
				FrameRate,
				TimeBase,
				CountingMode,
				UnitRate,
				UnitType);
		}

		public Timecode Decrement()
		{
			return new Timecode(
				AbsoluteTime - AbsoluteFramesToAbsoluteTime(1, FrameRate, TimeBase),
				FrameRate,
				TimeBase,
				CountingMode,
				UnitRate,
				UnitType);
		}

		public Timecode Negate()
		{
			return new Timecode(decimal.Negate(AbsoluteTime), FrameRate, TimeBase, CountingMode, UnitRate, UnitType);
		}

		public TimeSpan ToTimeSpan()
		{
			return new TimeSpan((long) AbsoluteTimeToAbsoluteUnits(AbsoluteTime, TicksPerSecond));
		}

		public uint ToIntegerRepresentation()
		{
			AbsoluteTimeToTimecode(
				AbsoluteTime,
				FrameRate,
				TimeBase,
				CountingMode,
				out var hours,
				out var minutes,
				out var seconds,
				out var frames,
				out var isNegative);

			return BitConverter.ToUInt32(
				new[]
				{
					(byte) frames,
					(byte) seconds,
					(byte) minutes,
					(byte) (isNegative ? 256 - System.Math.Abs(hours) : hours)
				}, 0);
		}
		#endregion

		#region Static Methods (Private)
		private static decimal AbsoluteTimeToAbsoluteFrames(decimal absoluteTime, int frameRate, TimeBase timeBase)
		{
			var result = decimal.Round(
				System.Math.Abs(absoluteTime) * (timeBase == TimeBase.Ntsc ? frameRate / 1.001M : frameRate),
				RoundingPrecision);
			return absoluteTime < 0 ? decimal.Negate(result) : result;
		}

		private static decimal AbsoluteFramesToAbsoluteTime(decimal absoluteFrames, int frameRate, TimeBase timeBase)
		{
			var result = decimal.Round(
				System.Math.Abs(absoluteFrames) / (timeBase == TimeBase.Ntsc ? frameRate / 1.001M : frameRate),
				RoundingPrecision);
			return absoluteFrames < 0 ? decimal.Negate(result) : result;
		}

		private static decimal AbsoluteTimeToAbsoluteUnits(decimal absoluteTime, int unitRate)
		{
			return decimal.Round(absoluteTime * unitRate, RoundingPrecision);
		}

		private static decimal AbsoluteUnitsToAbsoluteTime(decimal absoluteUnits, int unitRate)
		{
			return decimal.Round(absoluteUnits / unitRate, RoundingPrecision);
		}

		private static long TimecodeToFrameCount(int frameRate,
												 CountingMode countingMode,
												 int hours,
												 int minutes,
												 int seconds,
												 int frames,
												 bool isNegative)
		{
			hours.ValidateRange(0, 23);
			minutes.ValidateRange(0, 59);
			seconds.ValidateRange(0, 59);
			frames.ValidateRange(0, frameRate - 1);

			var modeDependentFrameRate =
				countingMode == CountingMode.Drop ? decimal.Round(frameRate / 1.001M, 2) : frameRate;
			var dropFrameCompensation = countingMode == CountingMode.Drop ? 2 * (int) (minutes / 10D) : 0;

			long result = frames + (frameRate * seconds) + ((int) (modeDependentFrameRate * 60) * minutes) +
						  dropFrameCompensation +
						  ((int) (modeDependentFrameRate * 3600) * hours);

			return isNegative ? 0 - result : result;
		}

		private static void FrameCountToTimecode(long frameCount,
												 int frameRate,
												 CountingMode countingMode,
												 out int hours,
												 out int minutes,
												 out int seconds,
												 out int frames,
												 out bool isNegative)
		{
			isNegative = frameCount < 0;
			frameCount = System.Math.Abs(frameCount);
			var modeDependentFrameRate =
				countingMode == CountingMode.Drop ? decimal.Round(frameRate / 1.001M, 2) : frameRate;

			hours = (int) decimal.Round((frameCount / (modeDependentFrameRate * 3600)) % 24, RoundingPrecision);

			if (countingMode == CountingMode.NonDrop)
			{
				minutes = (int) decimal.Round((frameCount - (frameRate * hours * 3600M)) / (frameRate * 60M),
											  RoundingPrecision);
				seconds = (int) decimal.Round(
					(frameCount - (frameRate * minutes * 60M) - (frameRate * hours * 3600M)) / frameRate,
					RoundingPrecision);
				frames = (int) (frameCount - (frameRate * seconds) - (frameRate * minutes * 60) - (frameRate * hours * 3600));
			}
			else
			{
				minutes = (int) decimal.Round(
					((frameCount + (2 * (int) ((frameCount - (modeDependentFrameRate * hours * 3600)) / (frameRate * 60)))) -
					 (2 * (int) ((frameCount - (modeDependentFrameRate * hours * 3600)) / (frameRate * 3600))) -
					 (modeDependentFrameRate * hours * 3600)) / (frameRate * 60),
					RoundingPrecision);
				seconds = (int) decimal.Round(
					(frameCount - (modeDependentFrameRate * minutes * 60) - (2 * (minutes / 10)) -
					 (modeDependentFrameRate * hours * 3600)) /
					frameRate,
					RoundingPrecision);
				frames = (int) (frameCount - (frameRate * seconds) - (int) (modeDependentFrameRate * minutes * 60) -
								(2 * (minutes / 10)) - (int) (modeDependentFrameRate * hours * 3600));
			}
		}

		private static decimal TimecodeToAbsoluteTime(int frameRate,
													  TimeBase timeBase,
													  CountingMode countingMode,
													  int hours,
													  int minutes,
													  int seconds,
													  int frames,
													  bool isNegative)
		{
			var frameCount = TimecodeToFrameCount(frameRate, countingMode, hours, minutes, seconds, frames, isNegative);
			return AbsoluteFramesToAbsoluteTime(frameCount, frameRate, timeBase);
		}

		private static void AbsoluteTimeToTimecode(decimal absoluteTime,
												   int frameRate,
												   TimeBase timeBase,
												   CountingMode countingMode,
												   out int hours,
												   out int minutes,
												   out int seconds,
												   out int frames,
												   out bool isNegative)
		{
			var frameCount = (long) AbsoluteTimeToAbsoluteFrames(absoluteTime, frameRate, timeBase);
			FrameCountToTimecode(
				frameCount,
				frameRate,
				countingMode,
				out hours,
				out minutes,
				out seconds,
				out frames,
				out isNegative);
		}

		private static decimal TimecodeWithUnitsToAbsoluteTime(int frameRate,
															   int unitRate,
															   TimeBase timeBase,
															   CountingMode countingMode,
															   int hours,
															   int minutes,
															   int seconds,
															   int frames,
															   int units,
															   bool isNegative)
		{
			var frameCount = System.Math.Abs(
				TimecodeToFrameCount(frameRate, countingMode, hours, minutes, seconds, frames,
									 isNegative));
			var absoluteTime = System.Math.Abs(AbsoluteFramesToAbsoluteTime(frameCount, frameRate, timeBase));
			var result = decimal.Round(absoluteTime + AbsoluteUnitsToAbsoluteTime(units, unitRate),
									   RoundingPrecision);
			return isNegative ? decimal.Negate(result) : result;
		}

		private static void AbsoluteTimeToTimecodeWithUnits(decimal absoluteTime,
															int frameRate,
															int unitRate,
															TimeBase timeBase,
															CountingMode countingMode,
															out int hours,
															out int minutes,
															out int seconds,
															out int frames,
															out int units,
															out bool isNegative)
		{
			var frameCount =
				(long) AbsoluteTimeToAbsoluteFrames(System.Math.Abs(absoluteTime), frameRate, timeBase);
			var dependentFrameRate = timeBase == TimeBase.Ntsc ? frameRate / 1.001M : frameRate;
			units = (int) (unitRate * (System.Math.Abs(absoluteTime) - (frameCount / dependentFrameRate)));
			FrameCountToTimecode(
				frameCount,
				frameRate,
				countingMode,
				out hours,
				out minutes,
				out seconds,
				out frames,
				out isNegative);
			isNegative = absoluteTime < 0;
		}
		#endregion

		#region Methods (Private)
		private void SetExtrema()
		{
			if (FrameRate == 0)
			{
				MinValue = 0;
				MaxValue = 0;
			}
			else
			{
				if (UnitType == UnitType.None)
				{
					MaxValue = TimecodeToAbsoluteTime(
						FrameRate,
						TimeBase,
						CountingMode,
						23,
						59,
						59,
						FrameRate - 1,
						false);
				}
				else
				{
					MaxValue = TimecodeWithUnitsToAbsoluteTime(
						FrameRate,
						UnitRate,
						TimeBase,
						CountingMode,
						23,
						59,
						59,
						FrameRate - 1,
						UnitRate - 1,
						false);
				}

				MinValue = decimal.Negate(MaxValue);
			}
		}

		private void ValidateAbsoluteTime()
		{
			if (AbsoluteTime < MinValue)
				throw new OverflowException($"Timecode < MinValue ({MinValue})");
			if (AbsoluteTime > MaxValue)
				throw new OverflowException($"Timecode > MaxValue ({MaxValue})");
		}

		private void ValidateAndFixFormat(bool prioritizeCountingMode)
		{
			if (CountingMode != CountingMode.Drop || TimeBase != TimeBase.Real)
				return;

			if (prioritizeCountingMode)
				TimeBase = TimeBase.Ntsc;
			else
				CountingMode = CountingMode.NonDrop;
		}

		private void ValidateFormat()
		{
			if (_countingMode == CountingMode.Drop && _timeBase == TimeBase.Real)
				throw new FormatException("Drop-frame counting is only valid for an NTSC time base");
		}
		#endregion

		#region Interface Implementation (IEquatable<>)
		public bool Equals(Timecode other)
		{
			return AbsoluteTime == other.AbsoluteTime;
		}
		#endregion

		#region Interface Implementation (IComparable<>)
		public int CompareTo(Timecode other)
		{
			if (AbsoluteTime < other.AbsoluteTime) return -1;
			if (AbsoluteTime > other.AbsoluteTime) return 1;

			return 0;
		}
		#endregion

		#region Interface Implementation (IFormattable)
		public string ToString(string format, IFormatProvider formatProvider)
		{
			var result         = new StringBuilder();
			var termSizeString = new StringBuilder();
			var componentSize  = 0;
			var isEscape       = false;
			var isLiteral      = false;

			var isNegative                  = false;
			int hours                       = 0, minutes = 0, seconds = 0, frames = 0, units = 0;
			var standardComponentsRetrieved = false;
			var allComponentsRetrieved      = false;

			foreach (var ch in format)
			{
				if (char.IsDigit(ch) && !isEscape && !isLiteral)
				{
					termSizeString.Append(ch);
					continue;
				}

				if ((ch == '\\' && !isEscape && !isLiteral) || (ch == '\\' && isLiteral))
				{
					isEscape = true;
					continue;
				}

				if (ch == '\'' && !isEscape)
				{
					isLiteral = !isLiteral;
					continue;
				}

				if (isEscape)
				{
					result.Append(ch);
					isEscape = false;
					continue;
				}

				if (isLiteral)
				{
					result.Append(ch);
					continue;
				}

				if (char.IsLetter(ch) && termSizeString.Length > 0)
				{
					if (!int.TryParse(termSizeString.ToString(), out componentSize))
					{
						throw new FormatException("Format string contains an invalid precision specification");
					}
				}

				switch (ch)
				{
					case 'h':
					{
						if (!standardComponentsRetrieved && !allComponentsRetrieved)
						{
							AbsoluteTimeToTimecode(
								AbsoluteTime,
								FrameRate,
								TimeBase,
								CountingMode,
								out hours,
								out minutes,
								out seconds,
								out frames,
								out isNegative);
							standardComponentsRetrieved = true;
						}

						if (componentSize > 0)
							result.Append(hours.ToString($"D{componentSize}"));
						else result.Append(hours);
						break;
					}

					case 'm':
					{
						if (!standardComponentsRetrieved && !allComponentsRetrieved)
						{
							AbsoluteTimeToTimecode(
								AbsoluteTime,
								FrameRate,
								TimeBase,
								CountingMode,
								out hours,
								out minutes,
								out seconds,
								out frames,
								out isNegative);
							standardComponentsRetrieved = true;
						}

						if (componentSize > 0)
							result.Append(minutes.ToString($"D{componentSize}"));
						else result.Append(minutes);
						break;
					}

					case 's':
					{
						if (!standardComponentsRetrieved && !allComponentsRetrieved)
						{
							AbsoluteTimeToTimecode(
								AbsoluteTime,
								FrameRate,
								TimeBase,
								CountingMode,
								out hours,
								out minutes,
								out seconds,
								out frames,
								out isNegative);
							standardComponentsRetrieved = true;
						}

						if (componentSize > 0)
							result.Append(seconds.ToString($"D{componentSize}"));
						else result.Append(seconds);
						break;
					}

					case 'q':
					{
						if (componentSize > 0)
							result.Append(Milliseconds.ToString($"D{componentSize}"));
						else result.Append(Milliseconds);
						break;
					}

					case 'f':
					{
						if (!standardComponentsRetrieved && !allComponentsRetrieved)
						{
							AbsoluteTimeToTimecode(
								AbsoluteTime,
								FrameRate,
								TimeBase,
								CountingMode,
								out hours,
								out minutes,
								out seconds,
								out frames,
								out isNegative);
							standardComponentsRetrieved = true;
						}

						if (componentSize > 0)
							result.Append(frames.ToString($"D{componentSize}"));
						else result.Append(frames);
						break;
					}

					case 'u':
					{
						if (!allComponentsRetrieved)
						{
							AbsoluteTimeToTimecodeWithUnits(
								AbsoluteTime,
								FrameRate,
								UnitRate,
								TimeBase,
								CountingMode,
								out hours,
								out minutes,
								out seconds,
								out frames,
								out units,
								out isNegative);
							allComponentsRetrieved = true;
						}

						if (componentSize > 0)
							result.Append(units.ToString($"D{componentSize}"));
						else result.Append(units);
						break;
					}

					case 'H':
					{
						if (componentSize > 0)
							result.Append(TotalHours.ToString($"F{componentSize}"));
						else result.Append(TotalHours);
						break;
					}

					case 'M':
					{
						if (componentSize > 0)
							result.Append(TotalMinutes.ToString($"F{componentSize}"));
						else result.Append(TotalMinutes);
						break;
					}

					case 'S':
					{
						if (componentSize > 0)
							result.Append(TotalSeconds.ToString($"F{componentSize}"));
						else result.Append(TotalSeconds);
						break;
					}

					case 'Q':
					{
						if (componentSize > 0)
							result.Append(TotalMilliseconds.ToString($"F{componentSize}"));
						else result.Append(TotalMilliseconds);
						break;
					}

					case 'F':
					{
						if (componentSize > 0)
							result.Append(TotalFrames.ToString($"F{componentSize}"));
						else result.Append(TotalFrames);
						break;
					}

					case 'U':
					{
						if (componentSize > 0)
							result.Append(TotalUnits.ToString($"F{componentSize}"));
						else result.Append(TotalUnits);
						break;
					}

					case 'R':
					{
						result.Append(TimeBase == TimeBase.Ntsc ? decimal.Round(FrameRate / 1.001M, 2) : FrameRate);
						break;
					}

					case 'r':
					{
						result.Append(UnitRate);
						break;
					}

					case 'D':
					{
						result.Append("fps");
						break;
					}

					case 'd':
					{
						if (UnitType == UnitType.Samples)
							result.Append("Hz");
						else if (UnitType == UnitType.Ticks)
							result.Append("ticks/sec");
						else
							result.Append("units/sec");
						break;
					}

					case 'N':
					{
						if (!standardComponentsRetrieved && !allComponentsRetrieved)
						{
							AbsoluteTimeToTimecode(
								AbsoluteTime,
								FrameRate,
								TimeBase,
								CountingMode,
								out hours,
								out minutes,
								out seconds,
								out frames,
								out isNegative);
							standardComponentsRetrieved = true;
						}

						if (AbsoluteTime != 0) result.Append(isNegative ? '-' : '+');
						break;
					}

					case 'n':
					{
						if (!standardComponentsRetrieved && !allComponentsRetrieved)
						{
							AbsoluteTimeToTimecode(
								AbsoluteTime,
								FrameRate,
								TimeBase,
								CountingMode,
								out hours,
								out minutes,
								out seconds,
								out frames,
								out isNegative);
							standardComponentsRetrieved = true;
						}

						if (isNegative) result.Append('-');
						break;
					}

					case '|':
					{
						result.Append(CountingMode == CountingMode.Drop ? ';' : ':');
						break;
					}

					default:
					{
						result.Append(ch);
						break;
					}
				}

				componentSize = 0;
				termSizeString.Clear();
			}

			return result.ToString();
		}
		#endregion

		#region Method Overrides (System.ValueType)
		public override bool Equals(object obj)
		{
			if (obj is Timecode time)
				return Equals(time);

			return false;
		}

		public override int GetHashCode()
		{
			return (int) AbsoluteTime ^ FrameRate ^ UnitRate;
		}

		public override string ToString()
		{
			var formatString = UnitType == UnitType.None ? "n2h:2m:2s|2f" : "n2h:2m:2s|2f:u";
			return ToString(formatString, null);
		}
		#endregion

		#region Operator Overloads (Comparison)
		public static bool operator ==(Timecode lhs, Timecode rhs)
		{
			return lhs.Equals(rhs);
		}

		public static bool operator !=(Timecode lhs, Timecode rhs)
		{
			return !lhs.Equals(rhs);
		}

		public static bool operator <(Timecode lhs, Timecode rhs)
		{
			return lhs.CompareTo(rhs) < 0;
		}

		public static bool operator >(Timecode lhs, Timecode rhs)
		{
			return lhs.CompareTo(rhs) > 0;
		}

		public static bool operator <=(Timecode lhs, Timecode rhs)
		{
			return lhs.CompareTo(rhs) <= 0;
		}

		public static bool operator >=(Timecode lhs, Timecode rhs)
		{
			return lhs.CompareTo(rhs) >= 0;
		}
		#endregion

		#region Operator Overloads (Unary)
		public static Timecode operator +(Timecode value)
		{
			return value;
		}

		public static Timecode operator -(Timecode value)
		{
			return value.Negate();
		}

		public static Timecode operator ++(Timecode value)
		{
			return value.Increment();
		}

		public static Timecode operator --(Timecode value)
		{
			return value.Decrement();
		}
		#endregion

		#region Operator Overloads (Binary)
		public static Timecode operator +(Timecode lhs, Timecode rhs)
		{
			return lhs.Add(rhs);
		}

		public static Timecode operator -(Timecode lhs, Timecode rhs)
		{
			return lhs.Subtract(rhs);
		}

		public static Timecode operator *(Timecode lhs, Timecode rhs)
		{
			return lhs.Multiply(rhs);
		}

		public static Timecode operator /(Timecode lhs, Timecode rhs)
		{
			return lhs.Divide(rhs);
		}
		#endregion

		#region Operator Overloads (Implicit)
		public static implicit operator Timecode(decimal value)
		{
			return new Timecode(value);
		}

		public static implicit operator decimal(Timecode value)
		{
			return value.AbsoluteTime;
		}

		public static implicit operator Timecode(TimeSpan value)
		{
			return FromTimeSpan(value);
		}

		public static implicit operator TimeSpan(Timecode value)
		{
			return value.ToTimeSpan();
		}
		#endregion
	}
}