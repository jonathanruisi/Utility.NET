// ┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  FILE:       DebugHelper.cs
// ┃  PROJECT:    Utility.NET
// ┃  SOLUTION:   Utility.NET
// ┃  CREATED:    2016-01-04 @ 3:49 AM
// ┣━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  AUTHOR:     Jonathan Ruisi
// ┃  EMAIL:      JonathanRuisi@gmail.com
// ┣━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  GIT REPO:   https://github.com/jonathanruisi/Utility.NET
// ┃  LICENSE:    https://github.com/jonathanruisi/Utility.NET/blob/master/LICENSE
// ┗━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;

namespace JLR.Utility.NETFramework.Debug
{
	public static class DebugHelper
	{
		#region Properties
		public static int IndentLevel { get; set; }
		#endregion

		#region Constructor
		static DebugHelper()
		{
			IndentLevel = 0;
		}
		#endregion

		/// <summary>
		/// Methods that simplify printing to the debug console
		/// </summary>
		public static class Print
		{
			#region Public Methods
			[Conditional("DEBUG")]
			public static void Table<T>(bool includeRowNumbers, params PrintTableColumn<T>[] columns)
			{
				var line1             = new StringBuilder();
				var line2             = new StringBuilder();
				var line3             = new StringBuilder();
				var lines             = new StringCollection();
				var rowcount          = columns.Select(column => column.Items.Length).Max();
				var numberColumnWidth = includeRowNumbers ? rowcount.ToString(CultureInfo.InvariantCulture).Length : 0;

				// Determine the width of each column configured for automatic column width
				foreach (var column in columns)
				{
					if (column.Width == 0)
					{
						var maxWidth = column.Header.Length;
						foreach (var item in column.Items)
						{
							int itemWidth;
							var formattableItem = item as IFormattable;
							if (formattableItem != null && !String.IsNullOrEmpty(column.FormatString))
								itemWidth = formattableItem.ToString(column.FormatString, CultureInfo.InvariantCulture).Length;
							else
								itemWidth = item.ToString().Length;
							if (itemWidth > maxWidth)
								maxWidth = itemWidth;
						}

						column.Width = maxWidth;
					}
				}

				// ─ │ ┌ ┐ └ ┘ ├ ┤ ┬ ┴ ┼ ═ ║ ╒ ╓ ╔ ╕ ╖ ╗ ╘ ╙ ╚ ╛ ╜ ╝ ╞ ╟ ╠ ╡ ╢ ╣ ╤ ╥ ╦ ╧ ╨ ╩ ╪ ╫ ╬
				// ▀ ▄ ▌ ▐ █
				// ← ↑ → ↓ ↔ ↕ ▴ ▲ ▸ ► ▾ ▼ ◂ ◄
				// ▫ ▪ □ ■ ○ ●
				// ⁞ •

				// Build table header
				line1.Append('╔');
				line2.Append('║');
				line3.Append('╠');
				if (numberColumnWidth > 0)
				{
					line1.Append('═', numberColumnWidth);
					line1.Append('╦');
					line2.Append("#".PadAndCenter(numberColumnWidth));
					line2.Append('║');
					line3.Append('═', numberColumnWidth);
					line3.Append('╬');
				}

				for (var i = 0; i < columns.Length; i++)
				{
					line1.Append('═', columns[i].Width);
					line2.Append(columns[i].Header.PadAndCenter(columns[i].Width));
					line3.Append('═', columns[i].Width);
					if (i < columns.Length - 1)
					{
						line1.Append('╤');
						line2.Append('│');
						line3.Append('╪');
					}
				}

				line1.Append('╗');
				line2.Append('║');
				line3.Append('╣');
				lines.Add(line1.ToString());
				lines.Add(line2.ToString());
				lines.Add(line3.ToString());

				// Build table
				for (var i = 0; i < rowcount; i++)
				{
					line1 = new StringBuilder();

					// Number column
					if (numberColumnWidth > 0)
					{
						line1.Append('║');
						line1.Append(' ', numberColumnWidth - i.ToString(CultureInfo.InvariantCulture).Length);
						line1.Append(i);
						line1.Append('║');
					}
					else
					{
						line1.Append('║');
					}

					// Current row
					for (var j = 0; j < columns.Length; j++)
					{
						if (columns[j].Items[i] is IFormattable)
						{
							line1.Append(
								(columns[j].Items[i] as IFormattable).ToString(columns[j].FormatString, CultureInfo.InvariantCulture)
																	 .PadAndCenter(columns[j].Width));
						}
						else
						{
							line1.Append(columns[j].Items[i].ToString().PadAndCenter(columns[j].Width));
						}

						if (j < columns.Length - 1)
							line1.Append('│');
					}

					line1.Append('║');
					lines.Add(line1.ToString());
				}

				// Bottom border
				line1 = new StringBuilder();
				line1.Append('╚');
				if (numberColumnWidth > 0)
				{
					line1.Append('═', numberColumnWidth);
					line1.Append('╩');
				}

				for (var i = 0; i < columns.Length; i++)
				{
					line1.Append('═', columns[i].Width);
					if (i < columns.Length - 1)
						line1.Append('╧');
				}

				line1.Append('╝');
				lines.Add(line1.ToString());

				// Print table
				foreach (var line in lines)
				{
					System.Diagnostics.Debug.Print(line);
				}
			}
			#endregion
		}
	}
}