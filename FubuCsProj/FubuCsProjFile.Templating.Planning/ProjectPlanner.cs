using System;
using System.Collections.Generic;
using System.Linq;
using Fubu.CsProjFile.FubuCsProjFile.Templating.Graph;
using Fubu.CsProjFile.FubuCsProjFile.Templating.Runtime;
using FubuCore;

namespace Fubu.CsProjFile.FubuCsProjFile.Templating.Planning
{
	public class ProjectPlanner : TemplatePlanner
	{
		public static readonly string NugetFile = "nuget.txt";

		public ProjectPlanner()
		{
			base.ShallowMatch(Substitutions.ConfigFile).Do = delegate(TextFile file, TemplatePlan plan)
			{
				plan.CurrentProject.Substitutions.ReadFrom(file.Path);
			};
			base.ShallowMatch(Input.File).Do = delegate(TextFile file, TemplatePlan plan)
			{
				IEnumerable<Input> inputs = Input.ReadFromFile(file.Path);
				plan.CurrentProject.Substitutions.ReadInputs(inputs, new Action<string>(plan.MissingInputs.Add));
			};
			base.Matching(FileSet.Shallow(ProjectPlan.TemplateFile, null)).Do = delegate(TextFile file, TemplatePlan plan)
			{
				plan.CurrentProject.ProjectTemplateFile = file.Path;
			};
			base.Matching(FileSet.Shallow(ProjectPlanner.NugetFile, null)).Do = delegate(TextFile file, TemplatePlan plan)
			{
				GenericEnumerableExtensions.Each<string>(from x in file.ReadLines()
				where FubuCore.StringExtensions.IsNotEmpty(x)
				select x, delegate(string line)
				{
					plan.CurrentProject.NugetDeclarations.Add(line.Trim());
				});
			};
			base.Matching(FileSet.Shallow("assembly-info.txt", null)).Do = delegate(TextFile file, TemplatePlan plan)
			{
				string[] additions = (from x in file.ReadLines()
				where FubuCore.StringExtensions.IsNotEmpty(x)
				select x).ToArray<string>();
				plan.CurrentProject.Add(new AssemblyInfoAlteration(additions));
			};
			base.Matching(FileSet.Shallow("references.txt", null)).Do = delegate(TextFile file, TemplatePlan plan)
			{
				GenericEnumerableExtensions.Each<string>(from x in file.ReadLines()
				where FubuCore.StringExtensions.IsNotEmpty(x)
				select x, delegate(string assem)
				{
					plan.CurrentProject.Add(new SystemReference(assem));
				});
			};
			base.Matching(FileSet.Deep("*.cs", null)).Do = delegate(TextFile file, TemplatePlan plan)
			{
				CodeFileTemplate template = new CodeFileTemplate(file.RelativePath, file.ReadAll());
				plan.CurrentProject.Add(template);
			};
			base.ShallowMatch(TemplatePlan.InstructionsFile).Do = delegate(TextFile file, TemplatePlan plan)
			{
				string instructions = file.ReadAll();
				plan.AddInstructions(plan.ApplySubstitutions(instructions));
			};
		}

		protected override void configurePlan(string directory, TemplatePlan plan)
		{
			ProjectPlan current = plan.Steps.OfType<ProjectPlan>().LastOrDefault<ProjectPlan>();
			GenericEnumerableExtensions.Each<ProjectDirectory>(ProjectDirectory.PlanForDirectory(directory), delegate(ProjectDirectory x)
			{
				current.Add(x);
			});
		}
	}
}
