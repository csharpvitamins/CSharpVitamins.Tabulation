using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CSharpVitamins.Tabulation
{
	public class CsvDefinitionFactory
	{
		/// <summary>
		/// The failover value converter. Simply calls a object.ToString() on non-nullable values
		/// </summary>
		public static readonly Func<PropertyInfo, object, string> FailoverValueConverter = (prop, value) => null == value ? null : value.ToString();

		/// <summary>
		/// The failover property name converter. Simply returns prop.Name.
		/// </summary>
		public static Func<PropertyInfo, string> FailoverNameConverter = (prop) => prop.Name;

		/// <summary>
		/// 
		/// </summary>
		public CsvDefinitionFactory()
		{
			ValueConverters = new Dictionary<Type, Func<PropertyInfo, object, string>>
			{
				{ typeof(DateTime), (prop, value) => ((DateTime)value).ToString("O") }
			};
		}

		/// <summary>
		/// Takes the details for a property and returns the name the column should be called
		/// </summary>
		public Func<PropertyInfo, string> NameConverter { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public IDictionary<Type, Func<PropertyInfo, object, string>> ValueConverters { get; set; }

		/// <summary>
		/// Resolves the value converter for the type
		/// </summary>
		/// <param name="type"></param>
		/// <param name="converters"></param>
		/// <returns></returns>
		static Func<PropertyInfo, object, string> resolve_converter(Type type, IDictionary<Type, Func<PropertyInfo, object, string>> converters)
		{
			if (null == converters)
				return FailoverValueConverter;

			Func<PropertyInfo, object, string> converter;
			converters.TryGetValue(type, out converter);
			return converter ?? FailoverValueConverter;
		}

		/// <summary>
		/// Creates a definition from the given model's properties (prop.Name & prop.Value.ToString())
		/// </summary>
		/// <typeparam name="T">The model to reflect on</typeparam>
		/// <returns>A TableDefinition instance that can render rows of data given the model T</returns>
		public CsvDefinition<T> CreateFromModel<T>()
		{
			var nameOf = NameConverter ?? FailoverNameConverter;

			return new CsvDefinition<T>(
				typeof(T)
					.GetProperties()
					.Select(prop => 
					{
						var converter = resolve_converter(prop.PropertyType, this.ValueConverters);

						return new KeyValuePair<string, Func<T, string>>(
							nameOf(prop),
							x =>
							{
								object value = prop.GetValue(x, null);
								return converter(prop, value);
							}
						);
					})
			);
		}
	}
}
