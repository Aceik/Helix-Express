using System.Collections.Generic;
using System.Linq;
using FubuCore.Descriptions;

namespace Fubu.CsProjFile.FubuCsProjFile.Templating.Graph
{
	public class OptionSelection : DescribesItself
	{
		public string Description;

		public string Name;

		public IList<Option> Options = new List<Option>();

		public void Configure(TemplateChoices choices, ProjectRequest request)
		{
			choices.Options = (choices.Options ?? ((IEnumerable<string>)new string[0]));
			Option option = choices.Selections.Has(this.Name) ? this.FindOption(choices.Selections[this.Name]) : (this.Options.FirstOrDefault((Option x) => choices.Options.Any((string o) => FubuCore.StringExtensions.EqualsIgnoreCase(o, x.Name))) ?? this.Options.First<Option>());
			GenericEnumerableExtensions.AddRange<string>(request.Alterations, option.Alterations);
		}

		public Option FindOption(string name)
		{
			return this.Options.FirstOrDefault((Option x) => FubuCore.StringExtensions.EqualsIgnoreCase(x.Name, name));
		}

		public void Describe(Description description)
		{
			description.Title = (this.Name);
			description.ShortDescription = (this.Description + FubuCore.StringExtensions.ToFormat(" (default is '{0}')", new object[]
			{
				this.Options.First<Option>().Name
			}));
			description.AddList("Options", this.Options);
		}
	}
}
