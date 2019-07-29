using System;
using System.Runtime.Serialization;

namespace CSharpVitamins.Tabulation
{
	/// <summary>
	/// An error thrown when the number of fields doesn't match the expected amount
	/// </summary>
	[Serializable]
	public class UnexpectedColumnCountException : Exception
	{
		/// <summary />
		public UnexpectedColumnCountException()
		{ }

		/// <summary />
		public UnexpectedColumnCountException(string message)
			: base(message)
		{ }

		/// <summary />
		public UnexpectedColumnCountException(string message, Exception inner)
			: base(message, inner)
		{ }

		/// <summary />
		protected UnexpectedColumnCountException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{ }

		/// <summary />
		public UnexpectedColumnCountException(int expected, int actual)
			: this($"Expected {expected:n0} columns, but {actual:n0} were given.")
		{ }
	}
}
