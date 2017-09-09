using System.Collections.Generic;

namespace Fubu.CsProjFile.FubuCsProjFile.Templating.Graph
{
	public interface ITemplateLibrary
	{
		IEnumerable<Template> All();

		Template Find(TemplateType type, string name);

		IEnumerable<Template> Find(TemplateType type, IEnumerable<string> names);

		IEnumerable<MissingTemplate> Validate(TemplateType type, params string[] names);
	}
}
