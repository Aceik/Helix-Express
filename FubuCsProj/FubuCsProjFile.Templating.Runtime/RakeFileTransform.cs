using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Fubu.CsProjFile.FubuCsProjFile.Templating.Planning;
using FubuCore;

namespace Fubu.CsProjFile.FubuCsProjFile.Templating.Runtime
{
	public class RakeFileTransform : ITemplateStep
	{
		private readonly string _text;

		public static readonly string TargetFile = "rakefile";

		public static readonly string SourceFile = "rake.txt";

		public RakeFileTransform(string text)
		{
			this._text = text;
		}

		public static string FindFile(string directory)
		{
			if (File.Exists(FubuCore.StringExtensions.AppendPath(directory, new string[]
			{
				"rakefile.rb"
			})))
			{
				return FubuCore.StringExtensions.ToFullPath(FubuCore.StringExtensions.AppendPath(directory, new string[]
				{
					"rakefile.rb"
				}));
			}
			return FubuCore.StringExtensions.ToFullPath(FubuCore.StringExtensions.AppendPath(directory, new string[]
			{
				RakeFileTransform.TargetFile
			}));
		}

		public void Alter(TemplatePlan plan)
		{
			string[] lines = plan.ApplySubstitutions(this._text).SplitOnNewLine();
			string rakeFile = RakeFileTransform.FindFile(plan.Root);
			FileSystem fileSystem = new FileSystem();
			List<string> list = fileSystem.FileExists(rakeFile) ? FubuCore.StringExtensions.ReadLines(fileSystem.ReadStringFromFile(rakeFile)).ToList<string>() : new List<string>();
			if (list.ContainsSequence(lines))
			{
				return;
			}
			list.Add(string.Empty);
			list.AddRange(lines);
			fileSystem.WriteStringToFile(rakeFile, GenericEnumerableExtensions.Join(list, Environment.NewLine));
		}

		public override string ToString()
		{
			return "Add content to the rakefile:";
		}
	}
}
