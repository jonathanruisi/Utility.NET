// ┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  FILE:       PrintTableColumn.cs
// ┃  PROJECT:    Utility.NET
// ┃  SOLUTION:   Utility.NET
// ┃  CREATED:    2016-01-04 @ 3:48 AM
// ┣━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  AUTHOR:     Jonathan Ruisi
// ┃  EMAIL:      JonathanRuisi@gmail.com
// ┣━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  GIT REPO:   https://github.com/jonathanruisi/Utility.NET
// ┃  LICENSE:    https://github.com/jonathanruisi/Utility.NET/blob/master/LICENSE
// ┗━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace JLR.Utility.NETFramework.Debug
{
	public sealed class PrintTableColumn<T>
	{
		// Fields
		private int _width;

		// Properties
		public T[]    Items        { get; set; }
		public string Header       { get; set; }
		public string FormatString { get; set; }

		public int Width
		{
			get { return _width; }
			set
			{
				if (value < 0)
					throw new ArgumentException(
						"Width must be greater than zero for manual column width, or equal to zero for automatic column width",
						nameof(value));
				_width = value;
			}
		}

		// Constructors
		public PrintTableColumn(IEnumerable items, string header, string formatString = null, int width = 0)
		{
			if (String.IsNullOrEmpty(header))
				throw new ArgumentNullException(nameof(header), "A header string must be specified");
			Items        = items.Cast<T>().ToArray();
			Header       = header;
			FormatString = formatString;
			Width        = width;
		}

		public PrintTableColumn(IEnumerable<T> items, string header, string formatString = null, int width = 0)
		{
			if (String.IsNullOrEmpty(header))
				throw new ArgumentNullException(nameof(header), "A header string must be specified");
			Items        = items.ToArray();
			Header       = header;
			FormatString = formatString;
			Width        = width;
		}
	}
}