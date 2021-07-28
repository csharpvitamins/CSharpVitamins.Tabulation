using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CSharpVitamins.Tabulation
{
	/// <summary>
	/// Creates a definition of fields to be used as a tabular output
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class CsvDefinition<T> : List<CsvField<T>>
	{
		/// <summary />
		public CsvDefinition()
			: base()
		{ }

		/// <summary />
		public CsvDefinition(int capacity)
			: base(capacity)
		{ }

		/// <summary />
		public CsvDefinition(IEnumerable<CsvField<T>> items)
			: base(items)
		{ }

		// backwards compat
		/// <summary />
		public CsvDefinition(IEnumerable<KeyValuePair<string, Func<T, string>>> items)
			: base(items.Cast<CsvField<T>>())
		{ }

		/// <summary>
		/// If the header has been output
		/// </summary>
		public bool HeaderWritten { get; private set; }

		/// <summary>
		/// Resets the state of the definition, like headers, so it can write a fresh output file.
		/// </summary>
		public void Reset()
		{
			HeaderWritten = false;
		}

		/// <summary>
		/// Determines if the field column/header is present in the current collection
		/// </summary>
		/// <param name="key">The name of the column/header</param>
		/// <param name="comparison">The string comparison to use - defaults to StringComparison.Ordinal</param>
		/// <returns>True if the key was found.</returns>
		public bool Contains(string key, StringComparison comparison = StringComparison.Ordinal)
		{
			return this.FindIndex(
				x => string.Equals(x.Key, key, comparison)
			) != -1;
		}

		/// <summary>
		/// Adds a Key/Value pair to the definition - used for easy object initialisation
		/// </summary>
		/// <param name="key">The name of the column/header</param>
		/// <param name="picker">The Func to convert T into a string for the column value</param>
		public void Add(string key, Func<T, string> picker)
		{
			Add(new CsvField<T>(key, picker));
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="key"></param>
		/// <param name="label"></param>
		/// <param name="picker"></param>
		public void Add(string key, string label, Func<T, string> picker)
		{
			Add(new CsvField<T>(key, label, picker));
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="key"></param>
		/// <param name="label"></param>
		/// <param name="picker"></param>
		/// <param name="include"></param>
		public void Add(string key, Func<T, string> picker, Func<string, bool> include)
		{
			Add(new CsvField<T>(key, label: null, picker: picker, include: include));
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="key"></param>
		/// <param name="label"></param>
		/// <param name="picker"></param>
		/// <param name="include"></param>
		public void Add(string key, string label, Func<T, string> picker, Func<string, bool> include)
		{
			Add(new CsvField<T>(key, label, picker, include));
		}

		/// <summary>
		/// Removes the field that matches the given column/header key. 
		/// </summary>
		/// <param name="key">The name of the column/header</param>
		/// <param name="comparison">The string comparison to use - defaults to StringComparison.Ordinal</param>
		/// <returns>True if the field was found and removed, otherwise false</returns>
		public bool Remove(string key, StringComparison comparison = StringComparison.Ordinal)
		{
			var index = this.FindIndex(
				x => string.Equals(x.Key, key, comparison)
			);

			if (index == -1)
				return false;

			this.RemoveAt(index);
			return true;
		}

		/// <summary>
		/// Writes the CSV, outputs a header line, then iterates over rows, converting into column values using the defined func
		/// </summary>
		/// <param name="writer">The TextWriter to write the lines to</param>
		/// <param name="rows">The enumerable of T that represents the lines</param>
		/// <param name="delimiter">The string to delimit column values with. A single character delimiter is also used to escape the value, multi-character strings are not escaped.</param>
		public void Write(TextWriter writer, IEnumerable<T> rows, string delimiter = ",")
		{
			if (null == writer)
				throw new ArgumentNullException(nameof(writer));

			if (null == rows)
				throw new ArgumentNullException(nameof(rows));

			if (null == delimiter)
				throw new ArgumentNullException(nameof(delimiter));

			char[] escChars = delimiter.Length == 1
				? new[] { delimiter[0], '\n', '\r', '"' }
				: new[] { '\n', '\r', '"' };

			var columns = this
				.Where(
					x => x.ShouldInclude
				)
				.ToArray();

			if (!HeaderWritten)
			{
				HeaderWritten = true;

				string header = string.Join(
					delimiter,
					columns.Select(
						x => Escape(x.Label ?? x.Key, escChars)
					)
				);
				writer.WriteLine(header);
			}

			foreach (T row in rows)
			{
				string line = string.Join(
					delimiter,
					columns.Select(
						x => Escape(x.PickValue(row), escChars)
					)
				);
				writer.WriteLine(line);
			}
		}

		/// <summary>
		/// Writes the CSV, outputs a header line, then iterates over rows, converting into column values using the defined func
		/// </summary>
		/// <param name="writer">The TextWriter to write the lines to</param>
		/// <param name="row">The single row of T that represents the line to write</param>
		/// <param name="delimiter">The string to delimit column values with. A single character delimiter is also used to escape the value, multi-character strings are not escaped.</param>
		public void Write(TextWriter writer, T row, string delimiter = ",")
		{
			this.Write(writer, new[] { row }, delimiter);
		}

		/// <summary>
		/// Creates a "PlainTextTable" instance with the fields in this definition, for fixed length column plain text output
		/// </summary>
		/// <param name="rows">The rows of data to tabulate</param>
		/// <param name="tab">Optional, if specified adds the tabulated data to the table. A new instance will be created and returned if this is null.</param>
		/// <returns>Either the PlainTextTable instance that was passed in, or a new instance of a PlainTextTable, populated with the data from the rows.</returns>
		public PlainTextTable Tabulate(IEnumerable<T> rows, PlainTextTable tab = null)
		{
			if (null == tab)
				tab = new PlainTextTable();

			var columns = this
				.Where(
					x => x.ShouldInclude
				)
				.ToArray();

			string[] line = columns
				.Select(
					x => x.Label ?? x.Key
				)
				.ToArray();
			tab.AddRow(line); // header

			foreach (T row in rows)
			{
				line = columns
					.Select(
						x => x.PickValue(row)
					)
					.ToArray();
				tab.AddRow(line);
			}

			return tab;
		}

		/// <summary>
		/// When found, escapes the entire string CSV style, by surrounding with quotes (embedded quotes are replaced with "")
		/// </summary>
		/// <param name="value">The string to escape</param>
		/// <param name="chars">The special chars that trigger the escape</param>
		/// <remarks>
		/// adapted from: http://www.asp.net/web-api/overview/formats-and-model-binding/media-formatters
		/// 
		/// This method should possibly quote strings the have leading or trailing whitespace.
		/// </remarks>
		/// <returns></returns>
		public static string Escape(string value, char[] chars)
		{
			if (value == null)
				return null;

			if (value.IndexOfAny(chars) != -1)
				return string.Concat("\"", value.Replace("\"", "\"\""), "\"");

			return value;
		}
	}
}
