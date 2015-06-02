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
	public class CsvDefinitionFacts
	{
		ITestOutputHelper output;

		public CsvDefinitionFacts(ITestOutputHelper output)
		{
			this.output = output;
		}

		class DemoModel
		{
			public string FieldA { get; set; }
			public string FieldB { get; set; }
			public string FieldC { get; set; }
		}

		IList<DemoModel> create_data()
		{
			return new[]
			{
				new DemoModel { FieldA = "Row 1 A", FieldB = "B1", FieldC = "C1" },
				new DemoModel { FieldA = "Row 2 A", FieldB = "B2", FieldC = "C2" },
				new DemoModel { FieldA = "Row 3 A" }
			};
		}

		CsvDefinition<DemoModel> create_definition()
		{
			return new CsvDefinition<DemoModel>
			{
				{ "Field A", item => item.FieldA },
				{ "Field B", item => item.FieldB },
				{ "Field C", item => item.FieldC },
			};
		}

		[Fact]
		void demoFields_should_haveThreeElements()
		{
			var fields = create_definition();
			Assert.Equal(3, fields.Count);
		}

		[Fact]
		void demoFields_should_orderInSequenceAdded()
		{
			var fields = create_definition();

			Assert.Equal("Field A", fields[0].Key);
			Assert.Equal("Field B", fields[1].Key);
			Assert.Equal("Field C", fields[2].Key);
		}

		[Fact]
		void fieldRemoval_should_removeSecondElement()
		{
			var fields = create_definition();

			fields.RemoveAt(1);

			Assert.Equal(2, fields.Count);

			Assert.Equal("Field A", fields[0].Key);
			// B is gone
			Assert.Equal("Field C", fields[1].Key);
		}

		[Fact]
		void fieldAddition_should_appendToEnd()
		{
			var fields = create_definition();

			fields.Add("Extra Field", model => null);

			Assert.Equal(4, fields.Count);

			Assert.Equal("Field C", fields[2].Key);
			Assert.Equal("Extra Field", fields[3].Key);
		}

		[Fact]
		void propHeaders_should_startAsFalse()
		{
			var def = create_definition();

			Assert.Equal(false, def.HeaderWritten);
		}

		[Fact]
		void propHeaders_should_setToTrueAfterWrite()
		{
			var fields = create_definition();
			var data = create_data();

			using (var writer = new StringWriter())
				fields.Write(writer, data);

			Assert.Equal(true, fields.HeaderWritten);
		}

		[Fact]
		void propHeaders_should_resetToFalse()
		{
			var fields = create_definition();
			var data = create_data();

			using (var writer = new StringWriter())
				fields.Write(writer, data);

			fields.Reset();

			Assert.Equal(false, fields.HeaderWritten);
		}

		static char[] escapeChars = new char[] { '\n', '\r', '"', ',' };

		[Theory]
		[InlineData((string)null, (string)null)]
		[InlineData("Plain Data", "Plain Data")]
		[InlineData("With, a comma", "\"With, a comma\"")]
		[InlineData("With\r\nLine Breaks", "\"With\r\nLine Breaks\"")]
		[InlineData("With \"Quoted Text\"", "\"With \"\"Quoted Text\"\"\"")]
		void cellStrings_should_escapeSpecialChars(string input, string expected)
		{
			var result = CsvDefinition<DemoModel>.Escape(input, escapeChars);

			Assert.Equal(expected, result);
		}

		[Fact]
		void cellDelimiter_should_writeComma()
		{
			var fields = create_definition();
			var data = create_data();

			string result;
			using (var writer = new StringWriter())
			{
				fields.Write(writer, data);
				result = writer.ToString();
			}

			string expected = "Field A,Field B,Field C\r\nRow 1 A,B1,C1\r\nRow 2 A,B2,C2\r\nRow 3 A,,\r\n";

			Assert.Equal(expected, result);
		}

		[Fact]
		void cellDelimiter_should_writeSpacePipeSpace()
		{
			var fields = create_definition();
			var data = create_data();

			string result;
			using (var writer = new StringWriter())
			{
				fields.Write(writer, data, " | ");
				result = writer.ToString();
			}

			string expected = "Field A | Field B | Field C\r\nRow 1 A | B1 | C1\r\nRow 2 A | B2 | C2\r\nRow 3 A |  | \r\n";

			Assert.Equal(expected, result);
		}
	}
}