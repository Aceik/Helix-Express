using System.Collections.Generic;
using System.Linq;
using FubuCore;

namespace Fubu.CsProjFile.FubuCsProjFile.Templating.Graph
{
	public class Input
	{
		public static readonly string File = "inputs.txt";

		public string Name
		{
			get;
			set;
		}

		public string Default
		{
			get;
			set;
		}

		public string Description
		{
			get;
			set;
		}

		public Input()
		{
		}

		public Input(string text)
		{
			string[] parts = FubuCore.StringExtensions.ToDelimitedArray(text);
			if (parts.First<string>().Contains("="))
			{
				string[] nameParts = parts.First<string>().Split(new char[]
				{
					'='
				});
				this.Name = nameParts.First<string>();
				this.Default = nameParts.Last<string>();
			}
			else
			{
				this.Name = parts.First<string>();
			}
			this.Description = parts.Last<string>();
		}

		public static IEnumerable<Input> ReadFrom(string directory)
		{
			FileSystem fileSystem = new FileSystem();
			string file = FubuCore.StringExtensions.AppendPath(directory, new string[]
			{
				Input.File
			});
			if (!fileSystem.FileExists(file))
			{
				return Enumerable.Empty<Input>();
			}
			return Input.ReadFromFile(file);
		}

		public static IEnumerable<Input> ReadFromFile(string file)
		{
			return (from x in FubuCore.StringExtensions.ReadLines(new FileSystem().ReadStringFromFile(file))
			where FubuCore.StringExtensions.IsNotEmpty(x)
			select new Input(x)).ToArray<Input>();
		}
	}
}
