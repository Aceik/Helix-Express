using System;
using System.IO;
using System.Linq;
using System.Text;
using FubuCore;

namespace Fubu.CsProjFile.FubuCsProjFile
{
	public class AssemblyInfo : CodeFile
	{
		private readonly CodeFile _codeFile;

		private readonly CsProjFile _projFile;

		private readonly FileSystem _fileSystem;

		private string FullPath
		{
			get
			{
				return this._fileSystem.GetFullPath(Path.Combine(this._projFile.ProjectDirectory, this._codeFile.Include));
			}
		}

		private string[] Lines
		{
			get;
			set;
		}

		public Version AssemblyVersion
		{
			get;
			set;
		}

		public Version AssemblyFileVersion
		{
			get;
			set;
		}

		public string AssemblyTitle
		{
			get;
			set;
		}

		public string AssemblyDescription
		{
			get;
			set;
		}

		public string AssemblyConfiguration
		{
			get;
			set;
		}

		public string AssemblyCompany
		{
			get;
			set;
		}

		public string AssemblyProduct
		{
			get;
			set;
		}

		public string AssemblyCopyright
		{
			get;
			set;
		}

		public string AssemblyInformationalVersion
		{
			get;
			set;
		}

		public AssemblyInfo(CodeFile codeFile, CsProjFile projFile)
		{
			this._codeFile = codeFile;
			this._projFile = projFile;
			this._fileSystem = new FileSystem();
			this.Initialize();
		}

		internal override void Save()
		{
			if (this._fileSystem.FileExists(this.FullPath))
			{
				StringBuilder result = new StringBuilder();
				if (this.AssemblyVersion != null)
				{
					this.UpdateLine(this.Lines, "AssemblyVersion", this.AssemblyVersion.ToString());
				}
				if (this.AssemblyFileVersion != null)
				{
					this.UpdateLine(this.Lines, "AssemblyFileVersion", this.AssemblyFileVersion.ToString());
				}
				this.UpdateLine(this.Lines, "AssemblyTitle", this.AssemblyTitle);
				this.UpdateLine(this.Lines, "AssemblyDescription", this.AssemblyDescription);
				this.UpdateLine(this.Lines, "AssemblyConfiguration", this.AssemblyConfiguration);
				this.UpdateLine(this.Lines, "AssemblyCompany", this.AssemblyCompany);
				this.UpdateLine(this.Lines, "AssemblyProduct", this.AssemblyProduct);
				this.UpdateLine(this.Lines, "AssemblyCopyright", this.AssemblyCopyright);
				this.UpdateLine(this.Lines, "AssemblyInformationalVersion", this.AssemblyInformationalVersion);
				Array.ForEach<string>(this.Lines, delegate(string s)
				{
					result.AppendLine(s);
				});
				this._fileSystem.WriteStringToFile(this.FullPath, result.ToString().TrimEnd(Environment.NewLine.ToCharArray()));
			}
		}

		private void Initialize()
		{
			if (this._fileSystem.FileExists(this.FullPath))
			{
				this.Lines = this._fileSystem.ReadStringFromFile(this.FullPath).SplitOnNewLine();
				this.Parse("AssemblyVersion", delegate(string value)
				{
					this.AssemblyVersion = new Version(value.ExtractVersion());
				}, this.Lines);
				this.Parse("AssemblyFileVersion", delegate(string value)
				{
					this.AssemblyFileVersion = new Version(value.ExtractVersion());
				}, this.Lines);
				this.Parse("AssemblyTitle", delegate(string value)
				{
					this.AssemblyTitle = this.GetValueBetweenQuotes(value);
				}, this.Lines);
				this.Parse("AssemblyDescription", delegate(string value)
				{
					this.AssemblyDescription = this.GetValueBetweenQuotes(value);
				}, this.Lines);
				this.Parse("AssemblyConfiguration", delegate(string value)
				{
					this.AssemblyConfiguration = this.GetValueBetweenQuotes(value);
				}, this.Lines);
				this.Parse("AssemblyCompany", delegate(string value)
				{
					this.AssemblyCompany = this.GetValueBetweenQuotes(value);
				}, this.Lines);
				this.Parse("AssemblyProduct", delegate(string value)
				{
					this.AssemblyProduct = this.GetValueBetweenQuotes(value);
				}, this.Lines);
				this.Parse("AssemblyCopyright", delegate(string value)
				{
					this.AssemblyCopyright = this.GetValueBetweenQuotes(value);
				}, this.Lines);
				this.Parse("AssemblyInformationalVersion", delegate(string value)
				{
					this.AssemblyInformationalVersion = this.GetValueBetweenQuotes(value);
				}, this.Lines);
			}
		}

		private void UpdateLine(string[] lines, string property, string value)
		{
			for (int i = 0; i < lines.Length; i++)
			{
				if (AssemblyInfo.Match(property, lines[i]))
				{
					lines[i] = this.UpdateValueBetweenQuotes(lines[i], value);
					return;
				}
			}
		}

		private void Parse(string property, Action<string> action, string[] lines)
		{
			string rawValue = lines.FirstOrDefault((string line) => AssemblyInfo.Match(property, line));
			if (!string.IsNullOrWhiteSpace(rawValue))
			{
				action(rawValue);
			}
		}

		private static bool Match(string property, string line)
		{
			return !line.Trim().StartsWith("//") && line.IndexOf(property, StringComparison.InvariantCultureIgnoreCase) > -1;
		}

		private string GetValueBetweenQuotes(string value)
		{
			int start = value.IndexOf('"') + 1;
			int end = value.IndexOf('"', start);
			return value.Substring(start, end - start);
		}

		private string UpdateValueBetweenQuotes(string line, string value)
		{
			int start = line.IndexOf('"') + 1;
			int end = line.IndexOf('"', start);
			return line.Substring(0, start) + value + line.Substring(end);
		}
	}
}
