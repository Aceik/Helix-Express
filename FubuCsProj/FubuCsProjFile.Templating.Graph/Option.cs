using System.Collections.Generic;
using FubuCore.Descriptions;

namespace Fubu.CsProjFile.FubuCsProjFile.Templating.Graph
{
	public class Option : DescribesItself
	{
		public string Description;

		public string Name;

		public IList<string> Alterations = new List<string>();

		public string Url;

		public Option()
		{
		}

		public Option(string name, params string[] alterations)
		{
			this.Name = name;
			GenericEnumerableExtensions.AddRange<string>(this.Alterations, alterations);
		}

		public Option DescribedAs(string description)
		{
			this.Description = description;
			return this;
		}

		public void Describe(Description description)
		{
			description.Title = (this.Name);
			description.ShortDescription = (this.Description);
			if (FubuCore.StringExtensions.IsNotEmpty(this.Url))
			{
				description.Properties.Fill("Url", this.Url);
			}
		}
	}
}
