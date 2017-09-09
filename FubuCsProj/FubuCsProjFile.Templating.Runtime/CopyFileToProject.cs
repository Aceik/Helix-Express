using FubuCore;

namespace Fubu.CsProjFile.FubuCsProjFile.Templating.Runtime
{
	public class CopyFileToProject : IProjectAlteration
	{
		private readonly string _relativePath;

		private readonly string _source;

		public CopyFileToProject(string relativePath, string source)
		{
			this._relativePath = relativePath.Replace('\\', '/');
			this._source = source;
		}

		public void Alter(CsProjFile file, ProjectPlan plan)
		{
			FileSystem fileSystem = new FileSystem();
			string rawText = fileSystem.ReadStringFromFile(this._source);
			string templatedText = plan.ApplySubstitutionsRaw(rawText, this._relativePath);
			string expectedPath = FubuCore.StringExtensions.AppendPath(file.ProjectDirectory, new string[]
			{
				this._relativePath
			});
			fileSystem.WriteStringToFile(expectedPath, templatedText);
			file.Add<Content>(new Content(this._relativePath));
		}

		public override string ToString()
		{
			return string.Format("Copy {0} to {1}", this._source, this._relativePath);
		}
	}
}
