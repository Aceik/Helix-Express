using Fubu.CsProjFile.FubuCsProjFile.Templating.Planning;

namespace Fubu.CsProjFile.FubuCsProjFile.Templating.Runtime
{
	public class ReadSolution : ITemplateStep
	{
		private readonly string _solutionFile;

		public string SolutionFile
		{
			get
			{
				return this._solutionFile;
			}
		}

		public ReadSolution(string solutionFile)
		{
			this._solutionFile = solutionFile;
		}

		public void Alter(TemplatePlan plan)
		{
			Solution solution = Solution.LoadFrom(this._solutionFile);
			plan.Solution = solution;
		}

		protected bool Equals(ReadSolution other)
		{
			return string.Equals(this._solutionFile, other._solutionFile);
		}

		public override bool Equals(object obj)
		{
			return !object.ReferenceEquals(null, obj) && (object.ReferenceEquals(this, obj) || (!(obj.GetType() != base.GetType()) && this.Equals((ReadSolution)obj)));
		}

		public override int GetHashCode()
		{
			if (this._solutionFile == null)
			{
				return 0;
			}
			return this._solutionFile.GetHashCode();
		}

		public override string ToString()
		{
			return string.Format("Read solution {0}", this._solutionFile);
		}
	}
}
