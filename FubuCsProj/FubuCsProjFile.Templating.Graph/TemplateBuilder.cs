using System;
using System.IO;

namespace Fubu.CsProjFile.FubuCsProjFile.Templating.Graph
{
	public class TemplateBuilder
	{
		private readonly string _directory;

		public TemplateBuilder(string directory)
		{
			this._directory = directory;
			TemplateLibrary.FileSystem.CreateDirectory(directory);
		}

		public void WriteContents(string relativePath, string contents)
		{
			TemplateLibrary.FileSystem.WriteStringToFile(FubuCore.StringExtensions.AppendPath(this._directory, new string[]
			{
				relativePath
			}), contents);
		}

		public void WriteContents(string file, Action<StringWriter> action)
		{
			StringWriter writer = new StringWriter();
			action(writer);
			this.WriteContents(file, writer.ToString());
		}

		public void WriteDescription(string text)
		{
			this.WriteContents(TemplateLibrary.DescriptionFile, text);
		}
	}
}
