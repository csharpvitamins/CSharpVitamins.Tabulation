using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CSharpVitamins.Tabulation
{
	/// <summary>
	/// A factory class for working with models and property info.
	/// </summary>
	public class CsvDefinitionFactory
	{
		/// <summary>
		/// The failover value converter. Simply calls a <c>object.ToString()</c> on non-nullable values.
		/// </summary>
		public static readonly Func<PropertyInfo, object, string> FailoverValueConverter =
			(prop, value) => value?.ToString();

		/// <summary>
		/// The failover property name converter. Simply returns <c>prop.Name</c>.
		/// </summary>
		public static Func<PropertyInfo, string> FailoverNameConverter =
			prop => prop.Name;

		/// <summary />
		Func<PropertyInfo, bool> shouldInclude = prop => true;

		/// <summary>
		/// Takes the details for a property and returns the name the column should be called.
		/// </summary>
		public Func<PropertyInfo, string> NameConverter { get; set; }

		/// <summary>
		/// A dictionary of value converters, maps a type to the function that will serialise the value to a string.
		/// </summary>
		public IDictionary<Type, Func<PropertyInfo, object, string>> ValueConverters { get; set; } =
			new Dictionary<Type, Func<PropertyInfo, object, string>>
			{
				{ typeof(DateTime), (prop, value)       => ((DateTime)value).ToString("O") },
				{ typeof(DateTimeOffset), (prop, value) => ((DateTimeOffset)value).ToString("O") },
			};

		/// <summary>
		/// Takes the details for a property and returns if the property should be included as a column (cannot be null).
		/// </summary>
		public Func<PropertyInfo, bool> ShouldInclude
		{
			get => shouldInclude;
			set => shouldInclude = value ?? throw new NullReferenceException($"{nameof(ShouldInclude)} cannot be set to `null`");
		}

		/// <summary>
		/// The binding flags to use when getting the properties of a model when calling <see cref="CreateFromModel{Model}"/>.
		/// <para>Default: <c>BindingFlags.Instance | BindingFlags.Public</c></para>
		/// </summary>
		public BindingFlags PropertyBindingFlags { get; set; } =
			BindingFlags.Instance | BindingFlags.Public;

		/// <summary>
		/// Resolves the value converter for the type.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="converters"></param>
		/// <returns></returns>
		static Func<PropertyInfo, object, string> resolve_converter(Type type, IDictionary<Type, Func<PropertyInfo, object, string>> converters)
		{
			if (null == converters)
				return FailoverValueConverter;

			converters.TryGetValue(type, out Func<PropertyInfo, object, string> converter);
			return converter ?? FailoverValueConverter;
		}

		/// <summary>
		/// Creates a definition from the given model's properties (<c>prop.Name</c> &amp; <c>prop.Value.ToString()</c>).
		/// </summary>
		/// <typeparam name="Model">The model to reflect on</typeparam>
		/// <returns>A TableDefinition instance that can render rows of data given the model T.</returns>
		public CsvDefinition<Model> CreateFromModel<Model>()
		{
			var nameOf = NameConverter ?? FailoverNameConverter;

			return new CsvDefinition<Model>(
				typeof(Model)
					.GetProperties(PropertyBindingFlags)
					.Where(ShouldInclude)
					.Select(prop =>
					{
						var converter = resolve_converter(prop.PropertyType, this.ValueConverters);

						return new CsvField<Model>(
							nameOf(prop),
							model =>
							{
								object value = prop.GetValue(model, null);
								return converter(prop, value);
							}
						);
					})
			);
		}
	}
}
