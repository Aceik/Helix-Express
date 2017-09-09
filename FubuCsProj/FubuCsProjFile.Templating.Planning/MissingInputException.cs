using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Fubu.CsProjFile.FubuCsProjFile.Templating.Planning
{
	[Serializable]
	public class MissingInputException : Exception
	{
		private readonly IEnumerable<string> _inputNames;

		public IEnumerable<string> InputNames
		{
			get
			{
				return this._inputNames;
			}
		}

		public MissingInputException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		public MissingInputException(IEnumerable<string> inputNames) : base(FubuCore.StringExtensions.ToFormat("Required inputs {0} are missing", new object[]
		{
			GenericEnumerableExtensions.Join(inputNames, ", ")
		}))
		{
			this._inputNames = inputNames;
		}
	}
}
