using System.Collections.Generic;
using System.IO;
using System.Linq;
using Fubu.CsProjFile.FubuCsProjFile.Templating.Planning;
using FubuCore;

namespace Fubu.CsProjFile.FubuCsProjFile.Templating.Runtime
{
	public class SolutionDirectory : ITemplateStep
	{
		private readonly string _relativePath;

		public string RelativePath
		{
			get
			{
				return this._relativePath;
			}
		}

		public static IEnumerable<SolutionDirectory> PlanForDirectory(string root)
		{
			return from dir in Directory.GetDirectories(root, "*", SearchOption.AllDirectories)
			select new SolutionDirectory(FubuCore.StringExtensions.PathRelativeTo(dir, root));
		}

		public SolutionDirectory(string relativePath)
		{
			this._relativePath = relativePath.Replace("\\", "/");
		}

		public void Alter(TemplatePlan plan)
		{
			FileSystemExtensions.CreateDirectory(new FileSystem(), new string[]
			{
				plan.Root,
				this._relativePath
			});
		}

		protected bool Equals(SolutionDirectory other)
		{
			return string.Equals(this._relativePath, other._relativePath);
		}

		public override bool Equals(object obj)
		{
			return !object.ReferenceEquals(null, obj) && (object.ReferenceEquals(this, obj) || (!(obj.GetType() != base.GetType()) && this.Equals((SolutionDirectory)obj)));
		}

		public override int GetHashCode()
		{
			if (this._relativePath == null)
			{
				return 0;
			}
			return this._relativePath.GetHashCode();
		}

		public override string ToString()
		{
			return string.Format("Create solution directory: {0}", this._relativePath);
		}
	}
}
