// ┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  FILE:       WaveFormatExtensible.cs
// ┃  PROJECT:    Utility.NET
// ┃  SOLUTION:   Utility.NET
// ┃  CREATED:    2018-03-13 @ 7:04 PM
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
using System.Runtime.InteropServices;

namespace JLR.Utility.NET.Multimedia.Audio.Wave
{
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 2)]
	public sealed class WaveFormatExtensible : WaveFormat
	{
		#region Constants
		private static readonly Dictionary<Guid, string> subFormatDictionary =
			new Dictionary<Guid, string>() { { MediaSubtypeGuid.Pcm, "PCM" }, { MediaSubtypeGuid.IeeeFloat, "IEEE Float" } };
		#endregion

		#region Fields
		private short            _validBitsPerSample;
		private SpeakerPositions _channelMask;
		private Guid             _subFormat;
		#endregion

		#region Properties
		public short ValidBitsPerSample
		{
			get { return _validBitsPerSample; }
			internal set { _validBitsPerSample = value; }
		}

		public SpeakerPositions ChannelMask
		{
			get { return _channelMask; }
			internal set { _channelMask = value; }
		}

		public Guid SubFormat
		{
			get { return _subFormat; }
			internal set { _subFormat = value; }
		}
		#endregion

		#region Constructors
		public WaveFormatExtensible() : this(
			2,
			44100,
			16,
			16,
			SpeakerPositions.FrontLeft | SpeakerPositions.FrontRight,
			MediaSubtypeGuid.Pcm) { }

		public WaveFormatExtensible(int channels,
									int samplesPerSecond,
									int bitsPerSample,
									int validBitsPerSample,
									SpeakerPositions channelMask,
									Guid subFormat) : base(WaveFormatTag.Extensible, channels, samplesPerSecond, bitsPerSample)
		{
			extraDataSize       = (short)(StandardSizeExtensible - StandardSize);
			_validBitsPerSample = (short)validBitsPerSample;
			_channelMask        = channelMask;
			_subFormat          = subFormat;
		}
		#endregion

		#region Protected Methods
		internal override void Read(BinaryReader reader)
		{
			base.Read(reader);
			try
			{
				if (extraDataSize < StandardSizeExtensible - StandardSize && formatTag != WaveFormatTag.Extensible)
					return;
				_validBitsPerSample = reader.ReadInt16();
				_channelMask        = (SpeakerPositions)reader.ReadUInt32();
				_subFormat          = new Guid(reader.ReadBytes(16));
			}
			finally
			{
				reader = null;
			}
		}

		internal override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			try
			{
				writer.Write(_validBitsPerSample);
				writer.Write((uint)_channelMask);
				writer.Write(_subFormat.ToByteArray(), 0, 16);
			}
			finally
			{
				writer = null;
			}
		}
		#endregion

		#region Interface Implementation(IEquatable<>)
		public override bool Equals(WaveFormat other)
		{
			if (!(other is WaveFormatExtensible)) return false;
			var otherReference = (WaveFormatExtensible)other;
			return base.Equals(other) && _validBitsPerSample == otherReference._validBitsPerSample &&
				_channelMask == otherReference._channelMask && _subFormat.Equals(otherReference._subFormat);
		}
		#endregion

		#region Interface Implementation (IValidatable)
		public override ValidationResult Validate()
		{
			var result = base.Validate();

			// ValidBitsPerSample
			if (_validBitsPerSample <= 0)
				result.EditValidity(Validity.Invalid, "\"ValidBitsPerSample\" must be > 0");
			else if (_validBitsPerSample > bitsPerSample)
				result.EditValidity(Validity.Invalid, "\"ValidBitsPerSample\" must be = \"BitsPerSample\"");

			// ChannelMask
			if (_channelMask <= 0)
				result.EditValidity(Validity.Invalid, "\"ChannelMask\" must be > zero");

			// SubFormat
			if (!subFormatDictionary.ContainsKey(_subFormat))
			{
				result.EditValidity(
					Validity.Invalid,
					$"The specified \"SubFormat\" ({SubFormat.ToString()}) is not currently supported");
			}

			return result;
		}
		#endregion

		#region Method Overrides (System.Object)
		public override bool Equals(object obj)
		{
			return Equals(obj as WaveFormatExtensible);
		}

		public override int GetHashCode()
		{
			return base.GetHashCode() ^ ValidBitsPerSample ^ (int)ChannelMask ^ BitConverter.ToInt32(SubFormat.ToByteArray(), 0);
		}

		public override string ToString()
		{
			return IsValid
				? $"{subFormatDictionary[_subFormat]}: {bitsPerSample}({_validBitsPerSample})bits @ {samplesPerSecond}kHz X {channels}ch ({(_channelMask == SpeakerPositions.None ? "Channels not mapped to spatial locations" : _channelMask.ToString())})"
				: Validate().ToString();
		}
		#endregion
	}
}