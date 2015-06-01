namespace CSharpVitamins.Tabulation
{
	/// <summary>
	/// Represents an extra line in the tabular data for the creation of divisions (like a blank row between header and content)
	/// </summary>
	public class Divider
	{
		/// <summary>
		/// The char to repeat in the separator
		/// </summary>
		public char Char { get; set; }

		/// <summary>
		/// If true, the column separators are inserted at the correct intervals
		/// </summary>
		public bool UseColumnSeparator { get; set; }

		/// <summary>
		/// Index of where the seperator should occur. Use negative to work from the last item, backwards.
		/// </summary>
		public int Index { get; set; }
	}
}
