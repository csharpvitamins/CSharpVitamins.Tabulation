namespace CSharpVitamins.Tabulation
{
	class ColumnState
	{
		/// <summary>
		/// The index of the column
		/// </summary>
		public int Index { get; set; }

		/// <summary>
		/// The horizontal alignment of the columns cells
		/// </summary>
		public Alignment Align { get; set; }

		/// <summary>
		/// The length of the longest word in the column
		/// </summary>
		public int Length { get; set; }
	}
}
