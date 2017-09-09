using System.Collections.Generic;
using System.Linq;
using FubuCore.Descriptions;

namespace Fubu.CsProjFile.FubuCsProjFile.Templating.Graph
{
	public class ProjectCategory : DescribesItself
	{
		public string Type;

		public readonly IList<ProjectTemplate> Templates;

		public ProjectCategory()
		{
			this.Templates = new List<ProjectTemplate>();
		}

		public ProjectTemplate FindTemplate(string name)
		{
			return this.Templates.FirstOrDefault((ProjectTemplate x) => FubuCore.StringExtensions.EqualsIgnoreCase(x.Name, name));
		}

		public void Describe(Description description)
		{
			description.Title = (this.Type + " projects");
			description.ShortDescription = ("Project templating options");
			description.AddList("Project Types", this.Templates);
		}
	}
}
