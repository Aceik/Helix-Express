using System.Collections.Generic;
using FubuCore.Util;

namespace Fubu.CsProjFile.FubuCsProjFile.Templating.Graph
{
	public class TemplateChoices
	{
		public string ProjectType;

		public string Category;

		public string ProjectName;

		public IEnumerable<string> Options;

		public Cache<string, string> Selections = new Cache<string, string>();

		public readonly Cache<string, string> Inputs = new Cache<string, string>();
	}
}
