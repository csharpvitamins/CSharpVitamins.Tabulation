using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CSharpVitamins.Tabulation
{
	/// <summary>
	/// Takes arrays of data (representing lines) and outputs them as fixed length columns
	/// </summary>
	public class PlainTextTable
	{
		/// <summary>
		/// Default column separator is a single space
		/// </summary>
		public const string DefaultColumnSeparator = " ";

		/// <summary>
		/// Keeps track of the max value lengths of each column
		/// </summary>
		int[] maxColumnLengths;
		List<string[]> rows;

		public PlainTextTable()
		{
			Reset();

			ColumnSeparator = DefaultColumnSeparator;
			Alignments = new Dictionary<int, Alignment>();
			Dividers = new List<Divider>();
		}

		public PlainTextTable(IEnumerable<string[]> rows)
			: this()
		{
			ImportRows(rows);
		}

		public PlainTextTable(int columnsExpected)
			: this()
		{
			ColumnsExpected = columnsExpected;
			maxColumnLengths = new int[columnsExpected];
		}

		public PlainTextTable(int columnsExpected, IEnumerable<string[]> rows)
			: this(columnsExpected)
		{
			ImportRows(rows);
		}

		/// <summary>The column configuration for this object</summary>
		public IDictionary<int, Alignment> Alignments { get; private set; }

		/// <summary>The number of columns each row must contain - if this is not set, it will 
		/// infer the length from the first line added</summary>
		public int ColumnsExpected { get; private set; }

		/// <summary>The column separator, used to divide cells on the same row - defaults to 
		/// a single space</summary>
		public string ColumnSeparator { get; set; }

		/// <summary>
		/// Trims any trailing whitespace from the rightmost column
		/// </summary>
		public bool TrimTrailingWhitespace { get; set; }

		/// <summary>
		/// Gets/sets a list of dividers. To add a divider, set the index on the 
		/// divider instance for where it should be inserted when rendering the 
		/// results.
		/// </summary>
		public IList<Divider> Dividers { get; set; }

		/// <summary>
		/// The lines of data
		/// 
		/// Use this setter to reimport (and reset) the line data. 
		/// To add multiple lines, use the ImportLines method directly.
		/// </summary>
		public IEnumerable<string[]> Rows
		{
			get { return rows; }
			set
			{
				Reset();

				if (null != value)
					ImportRows(value);
			}
		}

		/// <summary>
		/// Resets the lines and number of columns expected, but not any other configuration
		/// </summary>
		/// <returns></returns>
		public PlainTextTable Reset()
		{
			rows = new List<string[]>();
			ColumnsExpected = -1;
			return this;
		}

		/// <summary>
		/// When true, trims any trailing whitespace from the rightmost column
		/// </summary>
		/// <param name="value">Defaults to true</param>
		/// <returns></returns>
		public PlainTextTable TrimTrailingSpace(bool value = true)
		{
			TrimTrailingWhitespace = value;
			return this;
		}

		/// <summary>
		/// The string that separates two cells
		/// </summary>
		/// <param name="value">The separator for cells</param>
		/// <returns></returns>
		public PlainTextTable SeparateBy(string value)
		{
			ColumnSeparator = value;
			return this;
		}

		/// <summary>
		/// Sets the alignment of the column
		/// </summary>
		/// <param name="index">The column index to set the alignment of</param>
		/// <param name="align">The alignment</param>
		/// <returns></returns>
		public PlainTextTable Align(int index, Alignment align)
		{
			Alignments[index] = align;
			return this;
		}

		/// <summary>
		/// Sets the alignment of a column
		/// </summary>
		/// <param name="index">The column index to set the alignment of</param>
		/// <param name="align">The alignment char - 'l' for left, 'r' for right, 'c' or 'm' for centre/middle</param>
		/// <returns></returns>
		public PlainTextTable Align(int index, char align)
		{
			Alignments[index] = parse_alignment(align);
			return this;
		}

		/// <summary>
		/// Sets multiple alignments, matching the arguments index to the columns index
		/// </summary>
		/// <param name="alignments">The array or alignment characters, in column index order - 'l' for left, 'r' for right, 'c' or 'm' for centre/middle</param>
		/// <returns></returns>
		public PlainTextTable Align(params char[] alignments)
		{
			for (int index = 0; index < alignments.Length; ++index)
				Alignments[index] = parse_alignment(alignments[index]);

			return this;
		}

		/// <summary>
		/// Sets multiple alignments, matching the arguments index to the columns index
		/// </summary>
		/// <param name="alignments">The array or alignment characters, in column index order - 'l' for left, 'r' for right, 'c' or 'm' for centre/middle</param>
		/// <returns></returns>
		public PlainTextTable Align(IEnumerable<char> alignments)
		{
			int index = -1;

			foreach (char letter in alignments)
				Alignments[++index] = parse_alignment(letter);

			return this;
		}

		/// <summary>
		/// Adds a new divider to the Dividers collection. 
		/// 
		/// Dividers are rendered before the index of the next row.
		/// </summary>
		/// <param name="index">
		///	The line index of where the divider should be inserted. 
		///	
		/// Use a negative value to work from the last item, backwards (for a footer, for instance).
		///	
		///	* An index of 1 will insert a divider after the first row
		///	* An index of -1 will insert a divider after the last row (an end-of-table divider)
		///	* An index of -2 will insert a divider before the last row (a summary divider)
		///	</param>
		/// <param name="repeatChar">The char to repeat in the separator</param>
		/// <param name="useColumnseparator">If true, the column separators are inserted at the correct intervals, otherwise the divider will span the entire length</param>
		/// <returns></returns>
		public PlainTextTable DivideAt(int index, char repeatChar, bool useColumnseparator = false)
		{
			Dividers.Add(new Divider
			{
				Index = index,
				Char = repeatChar,
				UseColumnSeparator = useColumnseparator,
			});

			return this;
		}

		/// <summary>
		/// Adds a divider at the current index.
		/// </summary>
		/// <param name="repeatChar">The char to repeat in the separator</param>
		/// <param name="useColumnseparator">If true, the column separators are inserted at the correct intervals, otherwise the divider will span the entire length</param>
		/// <example>
		/// tab.AddRow("Head1", "Head2");
		/// tab.Divide('-'); // after header
		/// tab.ImportRows(/*...*/);
		/// tab.Divide('-'); // after import of rows, before summary
		/// tab.AddRow("Foot1", "Foot2");
		/// </example>
		/// <returns></returns>
		public PlainTextTable Divide(char repeatChar, bool useColumnseparator = false)
		{
			Dividers.Add(new Divider
			{
				Index = rows.Count,
				Char = repeatChar,
				UseColumnSeparator = useColumnseparator,
			});

			return this;
		}

		/// <summary>
		/// Enumerates data calling AddLine for each item
		/// </summary>
		/// <param name="rows"></param>
		/// <returns></returns>
		public PlainTextTable ImportRows(IEnumerable<string[]> rows)
		{
			foreach (string[] row in rows)
				AddRow(row);

			return this;
		}

		/// <summary>
		/// Adds the array of columns to the set of data - if expected columns is not yet set, the first call of 
		/// this method will determine the number of expected columns. If subsequent lines do not have the same
		/// element lenght, an UnexpectedColumnCountException is thrown
		/// </summary>
		/// <param name="rowData"></param>
		/// <returns></returns>
		public PlainTextTable AddRow(params string[] rowData)
		{
			var count = rowData.Length;
			if (count != ColumnsExpected)
			{
				if (ColumnsExpected > 0)
					throw new UnexpectedColumnCountException(ColumnsExpected, count);

				// otherwise, this must be the first line we've encountered, so infer 
				// the expected columns from this line
				ColumnsExpected = count;
				maxColumnLengths = new int[count];
			}

			rows.Add(rowData);

			update_column_max_lengths(rowData);

			return this;
		}

		/// <summary>
		/// updates the max-length references for all elemnts of data
		/// </summary>
		/// <param name="rowData"></param>
		void update_column_max_lengths(string[] rowData)
		{
			for (int i = 0, l = rowData.Length; i < l; ++i)
			{
				if (null != rowData[i])
					maxColumnLengths[i] = Math.Max(maxColumnLengths[i], rowData[i].Length);
			}
		}

		/// <summary>
		/// Renders the tabbed data to string
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			using (var writer = new StringWriter())
			{
				Render(writer);
				return writer.ToString();
			}
		}

		/// <summary>
		/// Returns the column state information for the current object (used for rendering).
		/// Includes max width of column, alignment and index
		/// </summary>
		/// <returns></returns>
		public ColumnState[] GetColumnState()
		{
			if (ColumnsExpected < 1)
				return new ColumnState[0];

			var columns = new ColumnState[ColumnsExpected];
			for (var i = 0; i < ColumnsExpected; ++i)
			{
				if (!Alignments.TryGetValue(i, out Alignment align))
					align = Alignment.Left;

				columns[i] = new ColumnState
				{
					Index = i,
					Width = maxColumnLengths[i],
					Align = align,
				};
			}
			return columns;
		}

		/// <summary>
		/// Renders the set of data using the default alignment
		/// </summary>
		/// <param name="writer"></param>
		public void Render(TextWriter writer)
		{
			int length = rows.Count;
			if (length == 0)
				return;

			var columns = GetColumnState();
			var lookup = Dividers.ToLookup(
				x => x.Index >= 0 ? x.Index : (length + x.Index + 1)
				);
			for (int i = 0; i <= length; ++i)
			{
				var dividers = lookup[i];
				if (dividers.Any())
				{
					foreach (var divider in dividers)
						render_divider_line(writer, divider, columns, ColumnsExpected);
				}

				if (i < length)
				{
					string[] rowData = rows[i];
					render_data_line(writer, rowData, columns, ColumnsExpected);
				}
			}
		}

		/// <summary>
		/// Used to render each line the set of rows, performaing alignment and spacing
		/// </summary>
		/// <param name="writer"></param>
		/// <param name="cells"></param>
		/// <param name="columns"></param>
		/// <param name="count"></param>
		void render_data_line(TextWriter writer, string[] cells, ColumnState[] columns, int count)
		{
			for (int i = 0; i < count; ++i)
				render_cell_text(writer, i, count, cells[i] ?? string.Empty, columns[i]);

			writer.WriteLine();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="writer"></param>
		/// <param name="divider"></param>
		/// <param name="columns"></param>
		/// <param name="count"></param>
		void render_divider_line(TextWriter writer, Divider divider, ColumnState[] columns, int count)
		{
			for (int i = 0; i < count; ++i)
				render_cell_divider(writer, i, count, divider, columns[i]);

			writer.WriteLine();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="writer"></param>
		/// <param name="index"></param>
		/// <param name="count"></param>
		/// <param name="text"></param>
		/// <param name="column"></param>
		void render_cell_text(TextWriter writer, int index, int count, string text, ColumnState column)
		{
			if (index > 0)
				writer.Write(ColumnSeparator);

			bool mustHaveRightPadding = !TrimTrailingWhitespace || index < count - 1;
			string padded = Pad(text, column.Width, column.Align, mustHaveRightPadding);
			writer.Write(padded);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="writer"></param>
		/// <param name="index"></param>
		/// <param name="count"></param>
		/// <param name="divider"></param>
		/// <param name="column"></param>
		void render_cell_divider(TextWriter writer, int index, int count, Divider divider, ColumnState column)
		{
			int padding = 0;

			if (index > 0 && !string.IsNullOrEmpty(ColumnSeparator))
			{
				if (divider.UseColumnSeparator)
					writer.Write(ColumnSeparator);
				else
					padding = ColumnSeparator.Length;
			}

			writer.Write(new string(divider.Char, column.Width + padding));
		}

		/// <summary>
		/// Pads the string given the alignment
		/// </summary>
		/// <param name="text">The value to pad</param>
		/// <param name="longestLength">The length to pad the string to (longest length of a column)</param>
		/// <param name="align">The alignment of the column</param>
		/// <param name="padRight">If trailing whitespace _can_ be omitted, right padding will not be added.</param>
		/// <param name="paddingChar">The character to use to pad the string, defaults to a space</param>
		/// <returns></returns>
		public static string Pad(string text, int longestLength, Alignment align, bool padRight, char paddingChar = ' ')
		{
			if (null == text)
				throw new ArgumentException(nameof(text));

			if (text.Length == longestLength)
				return text;

			switch (align)
			{
				case Alignment.Left:
					return padRight ? text.PadRight(longestLength, paddingChar) : text;

				case Alignment.Right:
					return text.PadLeft(longestLength, paddingChar);

				case Alignment.Center:
					int remainder = longestLength - text.Length; // required padding
					int halfway = (int)Math.Floor(remainder / 2D);

					return padRight
						? string.Concat(new string(paddingChar, halfway), text, new string(paddingChar, remainder - halfway))
						: string.Concat(new string(paddingChar, halfway), text);
			}

			throw new NotImplementedException();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		static Alignment parse_alignment(string value)
		{
			if (null == value)
				throw new ArgumentNullException(nameof(value));

			if (value.Length == 0)
				return Alignment.Left;

			return parse_alignment(value[0]);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		static Alignment parse_alignment(char value)
		{
			switch (char.ToLowerInvariant(value))
			{
				case ' ':
				case 'l': return Alignment.Left;
				case 'm':
				case 'c': return Alignment.Center;
				case 'r': return Alignment.Right;
			}

			throw new NotSupportedException($"char({value}) is not a supported alignment.");
		}
	}
}
