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
	public class CsvDefinition<T> : List<KeyValuePair<string, Func<T, string>>>
	{
		public CsvDefinition()
			: base()
		{ }

		public CsvDefinition(int capacity)
			: base(capacity)
		{ }

		public CsvDefinition(IEnumerable<KeyValuePair<string, Func<T, string>>> items)
			: base(items)
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
			return this.FindIndex(x => string.Equals(x.Key, key, comparison)) != -1;
		}

		/// <summary>
		/// Adds a Key/Value pair to the definition - used for easy object initialisation
		/// </summary>
		/// <param name="key">The name of the column/header</param>
		/// <param name="value">The Func to convert T into a string for the column value</param>
		public void Add(string key, Func<T, string> value)
		{
			Add(new KeyValuePair<string, Func<T, string>>(key, value));
		}

		/// <summary>
		/// Removes the field that matches the given column/header key. 
		/// </summary>
		/// <param name="key">The name of the column/header</param>
		/// <param name="comparison">The string comparison to use - defaults to StringComparison.Ordinal</param>
		/// <returns>True if the field was found and removed, otherwise false</returns>
		public bool Remove(string key, StringComparison comparison = StringComparison.Ordinal)
		{
			var index = this.FindIndex(x => string.Equals(x.Key, key, comparison));
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
		/// <param name="delimiter">The string to delimit column values with. A single character delimiter isare also used to escape the value, multi-character strings are not escaped.</param>
		public void Write(TextWriter writer, IEnumerable<T> rows, string delimiter = ",")
		{
			if (writer == null)
				throw new ArgumentNullException(nameof(writer));

			if (rows == null)
				throw new ArgumentNullException(nameof(rows));

			if (delimiter == null)
				throw new ArgumentNullException(nameof(delimiter));

			char[] escChars = delimiter.Length == 1 ? new[] { delimiter[0], '\n', '\r', '"' } : new[] { '\n', '\r', '"' };

			if (!HeaderWritten)
			{
				HeaderWritten = true;

				string header = string.Join(delimiter, this.Select(x => Escape(x.Key, escChars)));
				writer.WriteLine(header);
			}

			foreach (T row in rows)
			{
				string line = string.Join(
					delimiter,
					this.Select(x => Escape(x.Value(row), escChars)) // x.Value == Func<T, string>
					);

				writer.WriteLine(line);
			}
		}

		/// <summary>
		/// Writes the CSV, outputs a header line, then iterates over rows, converting into column values using the defined func
		/// </summary>
		/// <param name="writer">The TextWriter to write the lines to</param>
		/// <param name="row">The single row of T that represents the line to write</param>
		/// <param name="delimiter">The string to delimit column values with. A single character delimiter isare also used to escape the value, multi-character strings are not escaped.</param>
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

			string[] line = this.Select(x => x.Key).ToArray();
			tab.AddRow(line); // header

			foreach (T row in rows)
			{
				line = this.Select(x => x.Value(row)).ToArray();
				tab.AddRow(line);
			}

			return tab;
		}

		/// <summary>
		/// When found, escapes the entire string CSV style, by surrounding with quotes (embedded quotes are replaced with "")
		/// </summary>
		/// <param name="value">The string to escape</param>
		/// <param name="chars">The sepcial chars that trigger the escape</param>
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
