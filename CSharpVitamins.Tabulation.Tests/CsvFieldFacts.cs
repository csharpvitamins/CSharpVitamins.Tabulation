using CSharpVitamins.Tabulation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using Xunit.Extensions;

//namespace CSharpVitamins.Tabulation.Tests
namespace Tests
{
	public class CsvFieldFacts
	{
		ITestOutputHelper output;

		public CsvFieldFacts(ITestOutputHelper output)
		{
			this.output = output;
		}

		class DemoModel
		{
			public string FieldA { get; set; }
			public string FieldB { get; set; }
			public string FieldC { get; set; }
			public string FieldD { get; set; }
		}

		IList<DemoModel> create_data()
		{
			return new[]
			{
				new DemoModel { FieldA = "Row 1 A", FieldB = "B1", FieldC = "C1", FieldD = "D1" },
				new DemoModel { FieldA = "Row 2 A", FieldB = "B2", FieldC = "C2" },
				new DemoModel { FieldA = "Row 3 A" }
			};
		}

		CsvDefinition<DemoModel> create_definition()
		{
			return new CsvDefinition<DemoModel>
			{
				{ "Field A", "Field A Label", item => item.FieldA },
				{ "Field B", item => item.FieldB },
				{ "Field C", item => item.FieldC, x => false },
				{ "Field D", "D Field", item => item.FieldD, x => true },
				{ "Field B", "", item => item.FieldB },
				{ "Field C", "Field C Again", item => item.FieldC },
			};
		}

		[Fact]
		void observes_includedFields_and_labels()
		{
			var fields = create_definition();
			var data = create_data();

			string result;
			using (var writer = new StringWriter())
			{
				fields.Write(writer, data, " | ");
				result = writer.ToString();
			}

			const string expected = "Field A Label | Field B | D Field |  | Field C Again"
				+ "\r\nRow 1 A | B1 | D1 | B1 | C1"
				+ "\r\nRow 2 A | B2 |  | B2 | C2"
				+ "\r\nRow 3 A |  |  |  | "
				+ "\r\n";

			Assert.Equal(expected, result);
		}
	}
}
