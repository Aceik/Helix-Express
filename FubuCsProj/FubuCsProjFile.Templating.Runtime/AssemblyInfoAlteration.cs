using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FubuCore;

namespace Fubu.CsProjFile.FubuCsProjFile.Templating.Runtime
{
	public class AssemblyInfoAlteration : IProjectAlteration
	{
		public const string SourceFile = "assembly-info.txt";

		public readonly string[] AssemblyInfoPath = new string[]
		{
			"Properties",
			"AssemblyInfo.cs"
		};

		private readonly IEnumerable<string> _additions;

		public AssemblyInfoAlteration(params string[] additions)
		{
			this._additions = additions;
		}

		public void Alter(CsProjFile file, ProjectPlan plan)
		{
			string assemblyInfoPath = Path.Combine(this.AssemblyInfoPath);
			CodeFile codeFile = file.Find<CodeFile>(assemblyInfoPath) ?? file.Add<CodeFile>(assemblyInfoPath);
			string path = file.PathTo(codeFile);
			string parentDirectory = FubuCore.StringExtensions.ParentDirectory(path);
			if (!Directory.Exists(parentDirectory))
			{
				Directory.CreateDirectory(parentDirectory);
			}
			new FileSystem().AlterFlatFile(path, delegate(List<string> contents)
			{
				this.Alter(contents, plan);
			});
		}

		public void Alter(List<string> contents, ProjectPlan plan)
		{
		    GenericEnumerableExtensions.Each<string>(from x in this._additions
		        select plan.ApplySubstitutionsRaw(x, null) into x
		        where !contents.Contains(x)
		        select x, new Action<string>(contents.Add));
        }

		public override string ToString()
		{
			return string.Format("AssemblyInfo content:  {0}", GenericEnumerableExtensions.Join(from x in this._additions
			select "'{0}'", "; "));
		}
	}
}
