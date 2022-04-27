// ┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  FILE:       FormatComponent.cs
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

using JLR.Utility.NETFramework.Multimedia.Audio.Wave;

namespace JLR.Utility.NETFramework.Multimedia.Audio.Riff.Wave
{
	public sealed class FormatComponent : RiffComponent
	{
		#region Properties
		internal        WaveFormat    WaveFormat            { get; private set; }
		public override long          Size                  => WaveFormat.Size;
		public          bool          IsExtensible          => WaveFormat is WaveFormatExtensible;
		public          WaveFormatTag FormatTag             => WaveFormat.FormatTag;
		public          int           Channels              => WaveFormat.Channels;
		public          int           SamplesPerSecond      => WaveFormat.SamplesPerSecond;
		public          int           AverageBytesPerSecond => WaveFormat.AverageBytesPerSecond;
		public          int           BlockAlign            => WaveFormat.BlockAlign;
		public          int           BitsPerSample         => WaveFormat.BitsPerSample;

		public int? ValidBitsPerSample
		{
			get
			{
				var format = WaveFormat as WaveFormatExtensible;
				if (IsExtensible && format != null)
					return format.ValidBitsPerSample;
				return null;
			}
		}

		public SpeakerPositions? ChannelMask
		{
			get
			{
				var format = WaveFormat as WaveFormatExtensible;
				if (IsExtensible && format != null)
					return format.ChannelMask;
				return null;
			}
		}

		public Guid? SubFormat
		{
			get
			{
				var format = WaveFormat as WaveFormatExtensible;
				if (IsExtensible && format != null)
					return format.SubFormat;
				return null;
			}
		}
		#endregion

		#region Constructors
		public FormatComponent(WaveFormat format)
		{
			WaveFormat = format ?? new WaveFormat();
		}

		public FormatComponent(BinaryReader reader)
		{
			Read(reader);
		}
		#endregion

		#region Internal Methods
		public override void Read(BinaryReader reader)
		{
			WaveFormat = WaveFormat.FromStream(reader);
		}

		public override void Write(BinaryWriter writer)
		{
			WaveFormat.Write(writer);
		}
		#endregion

		#region Interface Implementation (IValidatable)
		public override ValidationResult Validate()
		{
			return WaveFormat.Validate();
		}
		#endregion

		#region Method Overrides (System.Object)
		public override string ToString()
		{
			return WaveFormat.ToString();
		}
		#endregion
	}
}