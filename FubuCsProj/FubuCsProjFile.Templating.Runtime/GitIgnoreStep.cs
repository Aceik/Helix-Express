using System;
using System.Collections.Generic;
using System.Linq;
using Fubu.CsProjFile.FubuCsProjFile.Templating.Planning;

namespace Fubu.CsProjFile.FubuCsProjFile.Templating.Runtime
{
	public class GitIgnoreStep : ITemplateStep
	{
		public static readonly string File = "ignore.txt";

		private readonly string[] _entries;

		public string[] Entries
		{
			get
			{
				return this._entries;
			}
		}

		public GitIgnoreStep(params string[] entries)
		{
			this._entries = entries;
		}

		public void Alter(TemplatePlan plan)
		{
			plan.AlterFile(".gitignore", delegate(List<string> list)
			{
				GenericEnumerableExtensions.Each<string>(this._entries, new Action<string>(list.Fill<string>));
			});
		}

		public static void ConfigurePlan(TextFile textFile, TemplatePlan plan)
		{
			string[] ignores = (from x in textFile.ReadLines()
			where FubuCore.StringExtensions.IsNotEmpty(x)
			select x).ToArray<string>();
			GitIgnoreStep step = new GitIgnoreStep(ignores);
			plan.Add(step);
		}

		public override string ToString()
		{
			return string.Format("Adding to .gitignore: {0}", GenericEnumerableExtensions.Join(this._entries, ", "));
		}
	}
}
