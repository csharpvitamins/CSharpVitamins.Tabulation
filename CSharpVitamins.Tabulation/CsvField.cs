using System;
using System.Collections.Generic;

namespace CSharpVitamins.Tabulation
{
	/// <summary>
	/// A class that holds field/column information
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class CsvField<T>
	{
		/// <summary>
		/// The key to use for this field - required, also doubles as a label, unless overwritten
		/// </summary>
		public string Key { get; private set; }

		/// <summary>
		/// The converter function used to produce a cell's string from the row object
		/// </summary>
		public Func<T, string> PickValue { get; set; }

		/// <summary>
		/// The column label to use. If null (default) uses the current `Key`. Can be an empty string to prevent a label from showing.
		/// </summary>
		public string Label { get; set; }

		/// <summary>
		/// An optional function used to determine if the field should be output or not. The fields key is passed in to the function
		/// </summary>
		public Func<string, bool> Include { get; set; }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="key"></param>
		public CsvField(string key)
		{
			this.Key = key;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="key"></param>
		/// <param name="picker"></param>
		public CsvField(string key, Func<T, string> picker)
			: this(key)
		{
			this.PickValue = picker;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="key"></param>
		/// <param name="label"></param>
		/// <param name="picker"></param>
		/// <param name="include"></param>
		public CsvField(string key, string label, Func<T, string> picker, Func<string, bool> include = null)
			: this(key, picker)
		{
			this.Label = label;
			this.Include = include;
		}

		/// <summary>
		/// If the field should be included in the output - see `.Include` for defineing func
		/// </summary>
		public bool ShouldInclude => null == Include || Include(this.Key);

		/// <summary>
		/// Mostly for backwards compatibility (previously was a KeyValuePair)
		/// </summary>
		/// <param name="field"></param>
		public static implicit operator KeyValuePair<string, Func<T, string>>(CsvField<T> field)
			=> new KeyValuePair<string, Func<T, string>>(field.Key, field.PickValue);

		/// <summary>
		/// Mostly for backwards compatibility (previously was a KeyValuePair)
		/// </summary>
		/// <param name="pair"></param>
		public static implicit operator CsvField<T>(KeyValuePair<string, Func<T, string>> pair)
			=> new CsvField<T>(key: pair.Key, picker: pair.Value);
	}
}
