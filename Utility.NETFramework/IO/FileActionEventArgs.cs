// ┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  FILE:       FileActionEventArgs.cs
// ┃  PROJECT:    Utility.NET
// ┃  SOLUTION:   Utility.NET
// ┃  CREATED:    2015-12-26 @ 12:15 AM
// ┣━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  AUTHOR:     Jonathan Ruisi
// ┃  EMAIL:      JonathanRuisi@gmail.com
// ┣━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  GIT REPO:   https://github.com/jonathanruisi/Utility.NET
// ┃  LICENSE:    https://github.com/jonathanruisi/Utility.NET/blob/master/LICENSE
// ┗━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

using System;

namespace JLR.Utility.NETFramework.IO
{
	#region Enumerated Types
	/// <summary>
	/// Represents a set of general file actions
	/// </summary>
	public enum FileAction
	{
		Load,
		Save,
		Delete
	}
	#endregion

	public class FileActionEventArgs : EventArgs
	{
		#region Properties
		public string     FileName   { get; set; }
		public FileAction FileAction { get; set; }
		#endregion

		#region Constructors
		public FileActionEventArgs(FileAction action, string fileName = null)
		{
			FileAction = action;
			FileName   = fileName ?? String.Empty;
		}
		#endregion

		#region Method Overrides (System.Object)
		public override string ToString()
		{
			return $"{Enum.GetName(typeof(FileAction), FileAction)}: {(String.IsNullOrEmpty(FileName) ? "UNKNOWN" : FileName)}";
		}
		#endregion
	}
}