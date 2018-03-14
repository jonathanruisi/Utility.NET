// ┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  FILE:       ByteArrayComponent.cs
// ┃  PROJECT:    Utility.NET
// ┃  SOLUTION:   Utility.NET
// ┃  CREATED:    2018-03-13 @ 7:08 PM
// ┣━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  AUTHOR:     Jonathan Ruisi
// ┃  EMAIL:      JonathanRuisi@gmail.com
// ┣━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  GIT REPO:   https://github.com/jonathanruisi/Utility.NET
// ┃  LICENSE:    https://github.com/jonathanruisi/Utility.NET/blob/master/LICENSE
// ┗━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

using System.IO;

namespace JLR.Utility.NET.Multimedia.Audio.Riff
{
	public sealed class ByteArrayComponent : RiffComponent
	{
		#region Properties
		public          byte[] Data { get; set; }
		public override long   Size => Data.Length;
		#endregion

		#region Constructors
		public ByteArrayComponent(params byte[] data)
		{
			Data = data;
		}

		public ByteArrayComponent(BinaryReader reader, int length)
		{
			Data = new byte[length];
			Read(reader);
		}
		#endregion

		#region Internal Methods
		public override void Read(BinaryReader reader)
		{
			Data = reader.ReadBytes(Data.Length);
		}

		public override void Write(BinaryWriter writer)
		{
			writer.Write(Data);
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
			return $"[Byte Array] Size: {Data.Length}B";
		}
		#endregion
	}
}