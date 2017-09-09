using System;
using System.Collections.Generic;

namespace Fubu.CsProjFile.FubuCsProjFile.Templating.Graph
{
	public class ProjectRequest
	{
		public readonly IList<string> Alterations = new List<string>();

		private readonly Substitutions _substitutions = new Substitutions();

		public string Version = DotNetVersion.V40;

		public string OriginalProject;

		public string Name
		{
			get;
			private set;
		}

		public string Template
		{
			get;
			private set;
		}

		public Substitutions Substitutions
		{
			get
			{
				return this._substitutions;
			}
		}

		public ProjectRequest(string name, string template)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			if (template == null)
			{
				throw new ArgumentNullException("template");
			}
			this.Name = name;
			this.Template = template;
		}

		public ProjectRequest(string name, string template, string originalProject) : this(name, template)
		{
			this.OriginalProject = originalProject;
		}
	}
}
