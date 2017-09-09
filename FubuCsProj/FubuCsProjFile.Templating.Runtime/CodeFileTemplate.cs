using System;
using System.IO;
using System.Reflection;
using FubuCore;

namespace Fubu.CsProjFile.FubuCsProjFile.Templating.Runtime
{
	public class CodeFileTemplate : IProjectAlteration
	{
		public const string CLASS = "%CLASS%";

		private readonly string _relativePath;

		private readonly string _rawText;

		public string RelativePath
		{
			get
			{
				return this._relativePath;
			}
		}

		public string RawText
		{
			get
			{
				return this._rawText;
			}
		}

		public static CodeFileTemplate Class(string relativePath)
		{
			string @class = Path.GetFileNameWithoutExtension(relativePath);
			string rawText = StreamExtensions.ReadAllText(Assembly.GetExecutingAssembly().GetManifestResourceStream(typeof(CodeFileTemplate), "Class.txt")).Replace("%CLASS%", @class);
			if (Path.GetExtension(relativePath) != ".cs")
			{
				relativePath += ".cs";
			}
			return new CodeFileTemplate(relativePath, rawText);
		}

		public CodeFileTemplate(string relativePath, string rawText)
		{
			if (Path.GetExtension(relativePath) != ".cs")
			{
				throw new ArgumentOutOfRangeException("relativePath", "Relative Path must have the .cs extension");
			}
			this._relativePath = relativePath.Replace('\\', '/');
			this._rawText = rawText;
		}

		public void Alter(CsProjFile file, ProjectPlan plan)
		{
			string includePath = plan.ApplySubstitutionsRaw(this._relativePath, null);
			string filename = FubuCore.StringExtensions.AppendPath(FubuCore.StringExtensions.ParentDirectory(file.FileName), new string[]
			{
				includePath
			});
			if (!filename.EndsWith(".cs"))
			{
				filename += ".cs";
			}
			string text = plan.ApplySubstitutionsRaw(this._rawText, this._relativePath);
			new FileSystem().WriteStringToFile(filename, text);
			file.Add<CodeFile>(includePath);
		}

		public override string ToString()
		{
			return string.Format("Write and attach code file: {0}", this._relativePath);
		}
	}
}
