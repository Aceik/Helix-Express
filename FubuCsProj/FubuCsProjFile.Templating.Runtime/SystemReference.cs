namespace Fubu.CsProjFile.FubuCsProjFile.Templating.Runtime
{
	public class SystemReference : IProjectAlteration
	{
		public const string SourceFile = "references.txt";

		private readonly string _assemblyName;

		public string AssemblyName
		{
			get
			{
				return this._assemblyName;
			}
		}

		public SystemReference(string assemblyName)
		{
			this._assemblyName = assemblyName;
		}

		public void Alter(CsProjFile file, ProjectPlan plan)
		{
			file.Add<AssemblyReference>(this._assemblyName);
		}

		protected bool Equals(SystemReference other)
		{
			return string.Equals(this._assemblyName, other._assemblyName);
		}

		public override bool Equals(object obj)
		{
			return !object.ReferenceEquals(null, obj) && (object.ReferenceEquals(this, obj) || (!(obj.GetType() != base.GetType()) && this.Equals((SystemReference)obj)));
		}

		public override int GetHashCode()
		{
			if (this._assemblyName == null)
			{
				return 0;
			}
			return this._assemblyName.GetHashCode();
		}

		public override string ToString()
		{
			return string.Format("Add assembly reference to {0}", this._assemblyName);
		}
	}
}
