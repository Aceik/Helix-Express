using System;

namespace Fubu.CsProjFile.FubuCsProjFile.Templating.Planning
{
	public interface ITemplatePlannerAction
	{
		Action<TextFile, TemplatePlan> Do
		{
			set;
		}
	}
}
