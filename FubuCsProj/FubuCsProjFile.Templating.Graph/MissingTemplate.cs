using System.Collections.Generic;
using System.Linq;

namespace Fubu.CsProjFile.FubuCsProjFile.Templating.Graph
{
	public class MissingTemplate
	{
		public string Name
		{
			get;
			set;
		}

		public TemplateType TemplateType
		{
			get;
			set;
		}

		public string[] ValidChoices
		{
			get;
			set;
		}

		public override string ToString()
		{
			string validChoiceString = GenericEnumerableExtensions.Join(from x in this.ValidChoices
			select FubuCore.StringExtensions.ToFormat("'{0}'", new object[]
			{
				x
			}), ", ");
			return FubuCore.StringExtensions.ToFormat("Unknown {0} template '{1}', valid choices are {2}", new object[]
			{
				this.TemplateType.ToString(),
				this.Name,
				validChoiceString
			});
		}
	}
}
