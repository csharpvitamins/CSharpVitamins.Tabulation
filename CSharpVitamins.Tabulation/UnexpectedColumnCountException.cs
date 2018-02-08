using System;
using System.Runtime.Serialization;

namespace CSharpVitamins.Tabulation
{
	[Serializable]
	public class UnexpectedColumnCountException : Exception
	{
		public UnexpectedColumnCountException()
		{ }

		public UnexpectedColumnCountException(string message)
			: base(message)
		{ }

		public UnexpectedColumnCountException(string message, Exception inner)
			: base(message, inner)
		{ }

		protected UnexpectedColumnCountException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{ }

		public UnexpectedColumnCountException(int expected, int actual)
			: this($"Expected {expected:n0} columns, but {actual:n0} were given.")
		{ }
	}
}
