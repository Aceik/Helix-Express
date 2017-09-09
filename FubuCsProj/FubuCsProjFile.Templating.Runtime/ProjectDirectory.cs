using System.Collections.Generic;
using System.IO;
using System.Linq;
using Fubu.CsProjFile.FubuCsProjFile.Templating.Graph;

namespace Fubu.CsProjFile.FubuCsProjFile.Templating.Runtime
{
	public class ProjectDirectory : IProjectAlteration
	{
		private readonly string _relativePath;

		public string RelativePath
		{
			get
			{
				return this._relativePath;
			}
		}

		public ProjectDirectory(string relativePath)
		{
			this._relativePath = relativePath.Replace("\\", "/");
		}

		public void Alter(CsProjFile file, ProjectPlan plan)
		{
			TemplateLibrary.FileSystem.CreateDirectory(FubuCore.StringExtensions.AppendPath(file.ProjectDirectory, new string[]
			{
				this._relativePath
			}));
		}

		protected bool Equals(ProjectDirectory other)
		{
			return string.Equals(this._relativePath, other._relativePath);
		}

		public override bool Equals(object obj)
		{
			return !object.ReferenceEquals(null, obj) && (object.ReferenceEquals(this, obj) || (!(obj.GetType() != base.GetType()) && this.Equals((ProjectDirectory)obj)));
		}

		public override int GetHashCode()
		{
			if (this._relativePath == null)
			{
				return 0;
			}
			return this._relativePath.GetHashCode();
		}

		public static IEnumerable<ProjectDirectory> PlanForDirectory(string root)
		{
			return from dir in Directory.GetDirectories(root, "*", SearchOption.AllDirectories)
			select new ProjectDirectory(FubuCore.StringExtensions.PathRelativeTo(dir, root));
		}

		public override string ToString()
		{
			return string.Format("Create folder {0}", this._relativePath);
		}
	}
}
