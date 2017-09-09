using System.Collections.Generic;
using System.IO;
using Fubu.CsProjFile.FubuCsProjFile.Templating.Graph;
using Fubu.CsProjFile.FubuCsProjFile.Templating.Runtime;

namespace Fubu.CsProjFile.FubuCsProjFile.Templating.Planning
{
	public class TemplatePlanBuilder
	{
		private readonly ITemplateLibrary _library;

		public TemplatePlanBuilder(ITemplateLibrary library)
		{
			this._library = library;
		}

		public TemplatePlan BuildPlan(TemplateRequest request)
		{
			TemplatePlan plan = new TemplatePlan(request.RootDirectory);
			if (FubuCore.StringExtensions.IsNotEmpty(request.SolutionName))
			{
				TemplatePlanBuilder.determineSolutionFileHandling(request, plan);
			}
			this.applySolutionTemplates(request, plan);
			request.Substitutions.CopyTo(plan.Substitutions);
			this.applyProjectTemplates(request, plan);
			this.applyTestingTemplates(request, plan);
			return plan;
		}

		private void applyTestingTemplates(TemplateRequest request, TemplatePlan plan)
		{
			GenericEnumerableExtensions.Each<ProjectRequest>(request.TestingProjects, delegate(ProjectRequest proj)
			{
				this.buildProjectPlan(plan, proj);
				plan.Add(new CopyProjectReferences(proj.OriginalProject));
			});
		}

		private void applyProjectTemplates(TemplateRequest request, TemplatePlan plan)
		{
			GenericEnumerableExtensions.Each<ProjectRequest>(request.Projects, delegate(ProjectRequest proj)
			{
				this.buildProjectPlan(plan, proj);
			});
		}

		private void buildProjectPlan(TemplatePlan plan, ProjectRequest proj)
		{
			ProjectPlan projectPlan = new ProjectPlan(proj.Name)
			{
				DotNetVersion = (proj.Version ?? DotNetVersion.V40)
			};
			plan.Add(projectPlan);
			proj.Substitutions.CopyTo(projectPlan.Substitutions);
			ProjectPlanner planner = new ProjectPlanner();
			if (FubuCore.StringExtensions.IsNotEmpty(proj.Template))
			{
				planner.CreatePlan(this._library.Find(TemplateType.Project, proj.Template), plan);
			}
			GenericEnumerableExtensions.Each<Template>(this._library.Find(TemplateType.Alteration, proj.Alterations), delegate(Template template)
			{
				planner.CreatePlan(template, plan);
			});
		}

		private void applySolutionTemplates(TemplateRequest request, TemplatePlan plan)
		{
			SolutionPlanner planner = new SolutionPlanner();
			GenericEnumerableExtensions.Each<Template>(this._library.Find(TemplateType.Solution, request.Templates), delegate(Template template)
			{
				planner.CreatePlan(template, plan);
			});
		}

		private static void determineSolutionFileHandling(TemplateRequest request, TemplatePlan plan)
		{
			string sourceDirectory = plan.SourceDirectory;
			string expectedFile = FubuCore.StringExtensions.AppendPath(sourceDirectory, new string[]
			{
				request.SolutionName
			});
			if (Path.GetExtension(expectedFile) != ".sln")
			{
				expectedFile += ".sln";
			}
			if (File.Exists(expectedFile))
			{
				plan.Add(new ReadSolution(expectedFile));
				return;
			}
			plan.Add(new CreateSolution(request.SolutionName)
			{
				Version = request.Version
			});
		}
	}
}
