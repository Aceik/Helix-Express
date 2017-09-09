using Fubu.CsProjFile.FubuCsProjFile.Templating.Planning;

namespace Fubu.CsProjFile.FubuCsProjFile.Templating.Runtime
{
	public class CopyFileToSolution : ITemplateStep
	{
		private readonly string _relativePath;

		private readonly string _source;

		public CopyFileToSolution(string relativePath, string source)
		{
			this._relativePath = relativePath.Replace("\\", "/");
			this._source = source;
		}

		public void Alter(TemplatePlan plan)
		{
			string expectedFile = FubuCore.StringExtensions.AppendPath(plan.Root, new string[]
			{
				this._relativePath
			});
			string contents = plan.FileSystem.ReadStringFromFile(this._source);
			string transformedContents = plan.ApplySubstitutions(contents);
			plan.FileSystem.WriteStringToFile(expectedFile, transformedContents);
		}

		protected bool Equals(CopyFileToSolution other)
		{
			return string.Equals(this._relativePath, other._relativePath) && string.Equals(this._source.CanonicalPath(), other._source.CanonicalPath());
		}

		public override bool Equals(object obj)
		{
			return !object.ReferenceEquals(null, obj) && (object.ReferenceEquals(this, obj) || (!(obj.GetType() != base.GetType()) && this.Equals((CopyFileToSolution)obj)));
		}

		public override int GetHashCode()
		{
			return ((this._relativePath != null) ? this._relativePath.GetHashCode() : 0) * 397 ^ ((this._source != null) ? this._source.GetHashCode() : 0);
		}

		public override string ToString()
		{
			return string.Format("Copy {1} to {0}", this._relativePath, this._source);
		}
	}
}
