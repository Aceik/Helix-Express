using System;
using System.Collections.Generic;
using System.Linq;
using FubuCore;

namespace Fubu.CsProjFile.FubuCsProjFile.Templating.Planning
{
	public class FilesTemplatePlanner : ITemplatePlanner
	{
		private readonly Action<TextFile, TemplatePlan> _action;

		private readonly FileSet _matching;

		public FilesTemplatePlanner(FileSet matching, Action<TextFile, TemplatePlan> action)
		{
			this._matching = matching;
			this._action = action;
		}

		public void DetermineSteps(string directory, TemplatePlan plan)
		{
			GenericEnumerableExtensions.Each<TextFile>(from x in TextFile.FileSystem.FindFiles(directory, this._matching)
			select new TextFile(x, FubuCore.StringExtensions.PathRelativeTo(x, directory)), delegate(TextFile file)
			{
				this._action(file, plan);
				plan.MarkHandled(file.Path);
			});
		}
	}
}
