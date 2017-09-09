using Fubu.CsProjFile.FubuCsProjFile.Templating.Planning;

namespace Fubu.CsProjFile.FubuCsProjFile.Templating.Runtime
{
	public interface ITemplateStep
	{
		void Alter(TemplatePlan plan);
	}
}
