using System.Collections.Generic;
using FubuCore;

namespace Fubu.CsProjFile.FubuCsProjFile.Templating
{
	public class TextFile
	{
		public static readonly IFileSystem FileSystem = new FileSystem();

		private readonly string _path;

		private readonly string _relativePath;

		public string RelativePath
		{
			get
			{
				return this._relativePath;
			}
		}

		public string Path
		{
			get
			{
				return this._path;
			}
		}

		public TextFile(string path, string relativePath)
		{
			this._path = path;
			this._relativePath = relativePath.Replace('\\', '/');
		}

		public string ReadAll()
		{
			return TextFile.FileSystem.ReadStringFromFile(this._path);
		}

		public IEnumerable<string> ReadLines()
		{
			return this.ReadAll().Trim().SplitOnNewLine();
		}
	}
}
