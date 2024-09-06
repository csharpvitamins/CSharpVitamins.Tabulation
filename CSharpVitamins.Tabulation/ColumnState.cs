namespace CSharpVitamins.Tabulation
{
	/// <summary>
	/// Shows the derived state of a column in the plain text table.
	/// </summary>
	public class ColumnState
	{
		/// <summary>
		/// The index of the column.
		/// </summary>
		public int Index { get; internal set; }

		/// <summary>
		/// The horizontal alignment of the columns cells.
		/// </summary>
		public Alignment Align { get; internal set; }

		/// <summary>
		/// The length of the longest word in the column.
		/// </summary>
		public int Width { get; internal set; }
	}
}
