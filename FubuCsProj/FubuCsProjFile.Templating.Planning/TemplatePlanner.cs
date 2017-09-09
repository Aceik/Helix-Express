using System;
using System.Collections.Generic;
using Fubu.CsProjFile.FubuCsProjFile.Templating.Graph;
using Fubu.CsProjFile.FubuCsProjFile.Templating.Runtime;
using FubuCore;

namespace Fubu.CsProjFile.FubuCsProjFile.Templating.Planning
{
	public abstract class TemplatePlanner : ITemplatePlannerAction
	{
		private readonly IList<ITemplatePlanner> _planners = new List<ITemplatePlanner>();

		private FileSet _matching;

		public Action<TextFile, TemplatePlan> Do
		{
			set
			{
				FilesTemplatePlanner planner = new FilesTemplatePlanner(this._matching, value);
				this._planners.Add(planner);
			}
		}

		protected TemplatePlanner()
		{
			this.ShallowMatch(GemReference.File).Do = new Action<TextFile, TemplatePlan>(GemReference.ConfigurePlan);
			this.ShallowMatch(GitIgnoreStep.File).Do = new Action<TextFile, TemplatePlan>(GitIgnoreStep.ConfigurePlan);
			this.ShallowMatch(RakeFileTransform.SourceFile).Do = delegate(TextFile file, TemplatePlan plan)
			{
				plan.Add(new RakeFileTransform(file.ReadAll()));
			};
		}

		public void CreatePlan(Template template, TemplatePlan plan)
		{
			this.configurePlan(template.Path, plan);
			GenericEnumerableExtensions.Each<ITemplatePlanner>(this._planners, delegate(ITemplatePlanner x)
			{
				x.DetermineSteps(template.Path, plan);
			});
			plan.CopyUnhandledFiles(template.Path);
		}

		protected abstract void configurePlan(string directory, TemplatePlan plan);

		public void Add<T>() where T : ITemplatePlanner, new()
		{
			this._planners.Add((default(T) == null) ? Activator.CreateInstance<T>() : default(T));
		}

		public ITemplatePlannerAction Matching(FileSet matching)
		{
			this._matching = matching;
			return this;
		}

		public ITemplatePlannerAction DeepMatch(string pattern)
		{
			return this.Matching(FileSet.Deep(pattern, null));
		}

		public ITemplatePlannerAction ShallowMatch(string pattern)
		{
			return this.Matching(FileSet.Shallow(pattern, null));
		}
	}
}
