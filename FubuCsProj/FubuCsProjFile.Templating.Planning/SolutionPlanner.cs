using System;
using System.Collections.Generic;
using Fubu.CsProjFile.FubuCsProjFile.Templating.Graph;
using Fubu.CsProjFile.FubuCsProjFile.Templating.Runtime;

namespace Fubu.CsProjFile.FubuCsProjFile.Templating.Planning
{
	public class SolutionPlanner : TemplatePlanner
	{
		public SolutionPlanner()
		{
			base.ShallowMatch(Substitutions.ConfigFile).Do = delegate(TextFile file, TemplatePlan plan)
			{
				plan.Substitutions.ReadFrom(file.Path);
			};
			base.ShallowMatch(Input.File).Do = delegate(TextFile file, TemplatePlan plan)
			{
				IEnumerable<Input> inputs = Input.ReadFromFile(file.Path);
				plan.Substitutions.ReadInputs(inputs, new Action<string>(plan.MissingInputs.Add));
			};
			base.ShallowMatch(TemplatePlan.InstructionsFile).Do = delegate(TextFile file, TemplatePlan plan)
			{
				string instructions = file.ReadAll();
				plan.AddInstructions(instructions);
			};
		}

		protected override void configurePlan(string directory, TemplatePlan plan)
		{
			GenericEnumerableExtensions.Each<SolutionDirectory>(SolutionDirectory.PlanForDirectory(directory), new Action<SolutionDirectory>(plan.Add));
		}
	}
}
