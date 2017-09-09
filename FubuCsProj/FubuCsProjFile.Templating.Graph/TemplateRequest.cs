using System;
using System.Collections.Generic;
using System.Linq;

namespace Fubu.CsProjFile.FubuCsProjFile.Templating.Graph
{
	public class TemplateRequest
	{
		private readonly IList<string> _templates = new List<string>();

		private readonly IList<ProjectRequest> _projects = new List<ProjectRequest>();

		private readonly IList<ProjectRequest> _testingProjects = new List<ProjectRequest>();

		private readonly Substitutions _substitutions = new Substitutions();

		public Substitutions Substitutions
		{
			get
			{
				return this._substitutions;
			}
		}

		public string RootDirectory
		{
			get;
			set;
		}

		public IEnumerable<string> Templates
		{
			get
			{
				return this._templates;
			}
			set
			{
				this._templates.Clear();
				GenericEnumerableExtensions.AddRange<string>(this._templates, value);
			}
		}

		public string SolutionName
		{
			get;
			set;
		}

		public string Version
		{
			get;
			set;
		}

		public IEnumerable<ProjectRequest> Projects
		{
			get
			{
				return this._projects;
			}
			set
			{
				this._projects.Clear();
				GenericEnumerableExtensions.AddRange<ProjectRequest>(this._projects, value);
			}
		}

		public IEnumerable<ProjectRequest> TestingProjects
		{
			get
			{
				return this._testingProjects;
			}
			set
			{
				this._testingProjects.Clear();
				GenericEnumerableExtensions.AddRange<ProjectRequest>(this._testingProjects, value);
			}
		}

		public void AddTemplate(string template)
		{
			this._templates.Add(template);
		}

		public void AddProjectRequest(ProjectRequest request)
		{
			this._projects.Add(request);
		}

		public void AddProjectRequest(string name, string template, Action<ProjectRequest> configuration = null)
		{
			ProjectRequest request = new ProjectRequest(name, template);
			if (configuration != null)
			{
				configuration(request);
			}
			this._projects.Add(request);
		}

		public void AddTestingRequest(ProjectRequest request)
		{
			this._testingProjects.Add(request);
		}

		public IEnumerable<MissingTemplate> Validate(ITemplateLibrary templates)
		{
			IEnumerable<MissingTemplate> solutionErrors = templates.Validate(TemplateType.Solution, this._templates.ToArray<string>());
			IEnumerable<MissingTemplate> projectErrors = templates.Validate(TemplateType.Project, (from x in this._projects
			select x.Template).ToArray<string>());
			IEnumerable<MissingTemplate> alterationErrors = templates.Validate(TemplateType.Alteration, this._projects.SelectMany((ProjectRequest x) => x.Alterations).ToArray<string>());
			IEnumerable<MissingTemplate> testingErrors = templates.Validate(TemplateType.Project, (from x in this._testingProjects
			select x.Template).ToArray<string>());
			IEnumerable<MissingTemplate> testingAlterationErrors = templates.Validate(TemplateType.Alteration, this._testingProjects.SelectMany((ProjectRequest x) => x.Alterations).ToArray<string>());
			return solutionErrors.Union(projectErrors).Union(alterationErrors).Union(testingErrors).Union(testingAlterationErrors);
		}
	}
}
