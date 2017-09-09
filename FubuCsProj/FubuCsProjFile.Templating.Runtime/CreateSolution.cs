using Fubu.CsProjFile.FubuCsProjFile.Templating.Planning;

namespace Fubu.CsProjFile.FubuCsProjFile.Templating.Runtime
{
	public class CreateSolution : ITemplateStep
	{
		private readonly string _solutionName;

		public string SolutionName
		{
			get
			{
				return this._solutionName;
			}
		}

		public string Version
		{
			get;
			set;
		}

		public CreateSolution(string solutionName)
		{
			this._solutionName = solutionName;
			this.Version = Solution.VS2012;
		}

		public void Alter(TemplatePlan plan)
		{
			Solution solution = Solution.CreateNew(plan.SourceDirectory, this._solutionName);
			solution.Version = this.Version;
			plan.Solution = solution;
		}

		public override string ToString()
		{
			return string.Format("Create solution '{0}'", this._solutionName);
		}
	}
}
