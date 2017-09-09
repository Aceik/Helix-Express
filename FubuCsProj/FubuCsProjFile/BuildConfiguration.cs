using System.Linq;

namespace Fubu.CsProjFile.FubuCsProjFile
{
	public class BuildConfiguration
	{
		public string Key
		{
			get;
			set;
		}

		public string Value
		{
			get;
			set;
		}

		public BuildConfiguration()
		{
		}

		public BuildConfiguration(string text)
		{
			string[] parts = FubuCore.StringExtensions.ToDelimitedArray(text.Trim(), '=');
			this.Key = parts.First<string>();
			this.Value = parts.Last<string>();
		}

		public void WriteProjectConfiguration(SolutionProject solutionProject, GlobalSection section)
		{
			section.Read(FubuCore.StringExtensions.ToFormat("\t\t{{{0}}}.{1}.ActiveCfg = {2}", new object[]
			{
				solutionProject.ProjectGuid.ToString().ToUpper(),
				this.Key,
				this.Value
			}));
			section.Read(FubuCore.StringExtensions.ToFormat("\t\t{{{0}}}.{1}.Build.0 = {2}", new object[]
			{
				solutionProject.ProjectGuid.ToString().ToUpper(),
				this.Key,
				this.Value
			}));
		}

		protected bool Equals(BuildConfiguration other)
		{
			return string.Equals(this.Key, other.Key) && string.Equals(this.Value, other.Value);
		}

		public override bool Equals(object obj)
		{
			return !object.ReferenceEquals(null, obj) && (object.ReferenceEquals(this, obj) || (!(obj.GetType() != base.GetType()) && this.Equals((BuildConfiguration)obj)));
		}

		public override int GetHashCode()
		{
			return ((this.Key != null) ? this.Key.GetHashCode() : 0) * 397 ^ ((this.Value != null) ? this.Value.GetHashCode() : 0);
		}

		public override string ToString()
		{
			return string.Format("Key: {0}, Value: {1}", this.Key, this.Value);
		}
	}
}
