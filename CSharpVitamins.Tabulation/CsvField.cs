using System;
using System.Collections.Generic;

namespace CSharpVitamins.Tabulation
{
	/// <summary>
	/// A class that holds field/column information.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class CsvField<T>
	{
		/// <summary>
		/// Constructs a new field with the given key.
		/// </summary>
		/// <param name="key">The key to use for this field - required, also doubles as a label, unless overwritten.</param>
		public CsvField(string key)
		{
			this.Key = key;
		}

		/// <summary>
		/// Constructs a new field with the given key and picker function.
		/// </summary>
		/// <param name="key">The key to use for this field - required, also doubles as a label, unless overwritten.</param>
		/// <param name="picker">The converter function used to produce a cell's string from the row object.</param>
		public CsvField(string key, Func<T, string> picker)
			: this(key)
		{
			this.PickValue = picker;
		}

		/// <summary>
		/// Constructs a new field with the given key, label, picker and include functions.
		/// </summary>
		/// <param name="key">The key to use for this field - required.</param>
		/// <param name="label">
		///   The column label to use. If <c>null</c> (default) uses the current `Key` property.
		///   <para>An empty string to prevent a label from showing.</para>
		/// </param>
		/// <param name="picker">The converter function used to produce a cell's string from the row object.</param>
		/// <param name="include">
		///   An optional function used to determine if the field should be output or not.
		///   <para>The fields key is passed in to the function.</para>
		/// </param>
		public CsvField(string key, string label, Func<T, string> picker, Func<string, bool> include = null)
			: this(key, picker)
		{
			this.Label = label;
			this.Include = include;
		}

		/// <summary>
		/// Gets the key to use for this field - required, also doubles as a label, unless overwritten.
		/// </summary>
		public string Key { get; private set; }

		/// <summary>
		/// Gets or sets the converter function used to produce a cell's string from the row object.
		/// </summary>
		public Func<T, string> PickValue { get; set; }

		/// <summary>
		/// Gets or sets the column label to use. If <c>null</c> (default) uses the current `Key` property.
		/// <para>An empty string to prevent a label from showing.</para>
		/// </summary>
		public string Label { get; set; }

		/// <summary>
		/// Gets or sets an optional function used to determine if the field should be output or not.
		/// <para>The fields key is passed in to the function.</para>
		/// </summary>
		public Func<string, bool> Include { get; set; }

		/// <summary>
		/// If the field should be included in the output - see `.Include` for defining func.
		/// </summary>
		public bool ShouldInclude => null == Include || Include(this.Key);

		/// <summary>
		/// Mostly for backwards compatibility (previously was a KeyValuePair).
		/// </summary>
		/// <param name="field"></param>
		public static implicit operator KeyValuePair<string, Func<T, string>>(CsvField<T> field)
			=> new KeyValuePair<string, Func<T, string>>(field.Key, field.PickValue);

		/// <summary>
		/// Mostly for backwards compatibility (previously was a KeyValuePair).
		/// </summary>
		/// <param name="pair"></param>
		public static implicit operator CsvField<T>(KeyValuePair<string, Func<T, string>> pair)
			=> new CsvField<T>(key: pair.Key, picker: pair.Value);
	}
}
