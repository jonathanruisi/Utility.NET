// ┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  FILE:       TextComponent.cs
// ┃  PROJECT:    Utility.NET
// ┃  SOLUTION:   Utility.NET
// ┃  CREATED:    2018-03-13 @ 7:10 PM
// ┣━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  AUTHOR:     Jonathan Ruisi
// ┃  EMAIL:      JonathanRuisi@gmail.com
// ┣━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  GIT REPO:   https://github.com/jonathanruisi/Utility.NET
// ┃  LICENSE:    https://github.com/jonathanruisi/Utility.NET/blob/master/LICENSE
// ┗━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

using System.Collections.Generic;
using System.IO;
using System.Text;

namespace JLR.Utility.NET.Multimedia.Audio.Riff
{
	public sealed class TextComponent : RiffComponent
	{
		#region Fields
		private byte[] _text;
		#endregion

		#region Properties
		public          string Text { get { return GetString(); } set { SetString(value); } }
		public override long   Size => _text == null ? 0 : (uint)_text.Length;
		#endregion

		#region Constructors
		public TextComponent(string text)
		{
			SetString(text);
		}

		public TextComponent(BinaryReader reader)
		{
			Read(reader);
		}
		#endregion

		#region Public Methods
		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			try
			{
				var  tempString = new List<byte>();
				byte lastByte;
				do
				{
					lastByte = reader.ReadByte();
					tempString.Add(lastByte);
				} while (lastByte != 0);

				_text = tempString.ToArray();
			}
			finally
			{
				reader = null;
			}
		}

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			try
			{
				writer.Write(_text);
			}
			finally
			{
				writer = null;
			}
		}
		#endregion

		#region Private Methods
		private string GetString()
		{
			return Encoding.ASCII.GetString(_text, 0, _text.Length - 1);
		}

		private void SetString(string value)
		{
			_text = new byte[value.Length + 1];
			Encoding.ASCII.GetBytes(value, 0, value.Length, _text, 0);
		}
		#endregion

		#region Interface Implementation (IValidatable)
		public override ValidationResult Validate()
		{
			var result = new ValidationResult();

			// Text
			if (_text.Length == 0)
				result.EditValidity(Validity.Unknown, "\"Text\" has not been set");

			return result;
		}
		#endregion

		#region Method Overrides (System.Object)
		public override string ToString()
		{
			return Text;
		}
		#endregion
	}
}