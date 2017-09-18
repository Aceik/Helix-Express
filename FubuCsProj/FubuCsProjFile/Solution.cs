using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Fubu.CsProjFile.FubuCsProjFile.MSBuild;
using FubuCore;
using FubuCore.Util;

namespace Fubu.CsProjFile.FubuCsProjFile
{
	public class Solution
	{
		public class SolutionReader
		{
			private readonly Solution _parent;

			private Action<string> _read;

			private GlobalSection _section;

			private ProjectSection _projectSection;

			private SolutionProject _solutionProject;

			private static HashSet<string> ignoredLibraryTypes = new HashSet<string>
			{
				Solution.SolutionFolderId.ToString("B"),
				CsProjFile.VisualStudioSetupLibraryType.ToString("B"),
				CsProjFile.WebSiteLibraryType.ToString("B")
			};

			public SolutionReader(Solution parent)
			{
				this._parent = parent;
				this._read = new Action<string>(this.normalRead);
			}

			private void lookForGlobalSection(string text)
			{
				text = text.Trim();
				if (text.Trim().StartsWith("GlobalSection"))
				{
					this._section = new GlobalSection(text);
					this._parent._sections.Add(this._section);
					this._read = new Action<string>(this.readSection);
				}
			}

			private void lookForProjectSection(string text)
			{
				text = text.Trim();
				if (text.Trim().StartsWith("ProjectSection"))
				{
					this._projectSection = (text.Trim().StartsWith("ProjectSection(ProjectDependencies)") ? new ProjectDependenciesSection(text) : new ProjectSection(text));
					this._solutionProject.ProjectSections.Add(this._projectSection);
					this._read = new Action<string>(this.readProjectSection);
				}
			}

			private void readSection(string text)
			{
				if (text.Trim() == "EndGlobalSection")
				{
					this._read = new Action<string>(this.lookForGlobalSection);
					return;
				}
				this._section.Read(text);
			}

			private void readProjectSection(string text)
			{
				if (text.Trim() == "EndProjectSection")
				{
					this._read = new Action<string>(this.readProject);
					return;
				}
				this._projectSection.Read(text);
			}

			private void readProject(string text)
			{
				if (text.Trim().StartsWith("EndProject"))
				{
					this._read = new Action<string>(this.normalRead);
					return;
				}
				if (text.Trim().StartsWith("ProjectSection"))
				{
					this.lookForProjectSection(text);
					return;
				}
				this._solutionProject.ReadLine(text);
			}

			private void normalRead(string text)
			{
				if (text.StartsWith("Global"))
				{
					this._read = new Action<string>(this.lookForGlobalSection);
					return;
				}
				if (text.StartsWith("ProjectSection"))
				{
					this._read = new Action<string>(this.lookForProjectSection);
					return;
				}
				if (Solution.SolutionReader.IncludeAsProject(text))
				{
					this._solutionProject = new SolutionProject(text, FubuCore.StringExtensions.ParentDirectory(this._parent._filename));
					this._solutionProject.Solution = this._parent;
					this._parent._projects.Add(this._solutionProject);
					this._read = new Action<string>(this.readProject);
					return;
				}
				this._parent._header.Add(text);
				if (FubuCore.StringExtensions.IsEmpty(this._parent.Version))
				{
					foreach (KeyValuePair<string, string[]> versionLine in Solution._versionLines.ToDictionary())
					{
						if (text.Trim() == versionLine.Value[1])
						{
							this._parent.Version = versionLine.Key;
						}
					}
				}
			}

			public static bool IncludeAsProject(string text)
			{
				return text.StartsWith("Project") && !Solution.SolutionReader.ignoredLibraryTypes.Any((string item) => text.Contains(item, StringComparison.InvariantCultureIgnoreCase));
			}

			public void Read(string text)
			{
				this._read(text);
			}
		}

		private const string Global = "Global";

		private const string EndGlobal = "EndGlobal";

		public const string EndGlobalSection = "EndGlobalSection";

		public const string EndProjectSection = "EndProjectSection";

		private const string SolutionConfigurationPlatforms = "SolutionConfigurationPlatforms";

		private const string ProjectConfigurationPlatforms = "ProjectConfigurationPlatforms";

		public static readonly Guid SolutionFolderId;

		public static readonly string VS2010;

		public static readonly string VS2012;

		public static readonly string VS2013;

		public static readonly string DefaultVersion;

		private static readonly Cache<string, string[]> _versionLines;

		private readonly string _filename;

		private readonly IList<SolutionProject> _projects = new List<SolutionProject>();

		protected readonly IList<string> _header = new List<string>();

		private readonly IList<string> _globals = new List<string>();

		private readonly IList<GlobalSection> _sections = new List<GlobalSection>();

		public string Version
		{
			get;
			set;
		}

		public string Filename
		{
			get
			{
				return this._filename;
			}
		}

		public IList<GlobalSection> Sections
		{
			get
			{
				return this._sections;
			}
		}

		public IEnumerable<string> Globals
		{
			get
			{
				return this._globals;
			}
		}

		public IEnumerable<SolutionProject> Projects
		{
			get
			{
				return this._projects;
			}
		}

		public string ParentDirectory
		{
			get
			{
				return FubuCore.StringExtensions.ParentDirectory(this._filename);
			}
		}

		public string Name
		{
			get
			{
				return Path.GetFileNameWithoutExtension(this._filename);
			}
		}

		static Solution()
		{
			Solution.SolutionFolderId = new Guid("2150E333-8FDC-42A3-9474-1A3956D46DE8");
			Solution.VS2010 = "VS2010";
			Solution.VS2012 = "VS2012";
			Solution.VS2013 = "VS2013";
			Solution.DefaultVersion = Solution.VS2010;
			Solution._versionLines = new Cache<string, string[]>();
			Solution._versionLines.Fill(Solution.VS2010, new string[]
			{
				"Microsoft Visual Studio Solution File, Format Version 11.00",
				"# Visual Studio 2010"
			});
			Solution._versionLines.Fill(Solution.VS2012, new string[]
			{
				"Microsoft Visual Studio Solution File, Format Version 12.00",
				"# Visual Studio 2012"
			});
			Solution._versionLines.Fill(Solution.VS2013, new string[]
			{
				"Microsoft Visual Studio Solution File, Format Version 12.00",
				"# Visual Studio 2013",
				"VisualStudioVersion = 12.0.21005.1",
				"MinimumVisualStudioVersion = 10.0.40219.1"
			});
		}

		public static Solution CreateNew(string directory, string name)
		{
			string text = StreamExtensions.ReadAllText(Assembly.GetExecutingAssembly().GetManifestResourceStream(typeof(Solution), "Solution.txt"));
			string filename = FubuCore.StringExtensions.AppendPath(directory, new string[]
			{
				name
			});
			if (Path.GetExtension(filename) != ".sln")
			{
				filename += ".sln";
			}
			return new Solution(filename, text)
			{
				Version = Solution.DefaultVersion
			};
		}

		public static Solution LoadFrom(string filename)
		{
			string text = new FileSystem().ReadStringFromFile(filename);
			return new Solution(filename, text);
		}

		private Solution(string filename, string text)
		{
			this._filename = filename;
			string[] items = text.SplitOnNewLine();
			Solution.SolutionReader reader = new Solution.SolutionReader(this);
			GenericEnumerableExtensions.Each<string>(items, new Action<string>(reader.Read));
		}

		public IEnumerable<BuildConfiguration> Configurations()
		{
			GlobalSection section = this.FindSection("SolutionConfigurationPlatforms");
			if (section != null)
			{
				return from x in section.Properties
				select new BuildConfiguration(x);
			}
			return Enumerable.Empty<BuildConfiguration>();
		}

		public GlobalSection FindSection(string name)
		{
			return this._sections.FirstOrDefault((GlobalSection x) => x.SectionName == name);
		}

		public void Save(bool saveProjects = true)
		{
			this.Save(this._filename, saveProjects);
		}

		public void Save(string filename, bool saveProjects = true)
		{
			this.CalculateProjectConfigurationPlatforms();
			StringWriter writer = new StringWriter();
			this.EnsureHeaders();
			GenericEnumerableExtensions.Each<string>(this._header, delegate(string x)
			{
				writer.WriteLine(x);
			});
			GenericEnumerableExtensions.Each<SolutionProject>(this._projects, delegate(SolutionProject x)
			{
				x.Write(writer);
			});
			writer.WriteLine("Global");
			GenericEnumerableExtensions.Each<GlobalSection>(this._sections, delegate(GlobalSection x)
			{
				x.Write(writer);
			});
			writer.WriteLine("EndGlobal");
			new FileSystem().WriteStringToFile(filename, writer.ToString());
			if (saveProjects)
			{
				GenericEnumerableExtensions.Each<SolutionProject>(this._projects, delegate(SolutionProject x)
				{
					x.Project.Save();
				});
			}
		}

		private void EnsureHeaders()
		{
			if (this._header.Count == 0)
			{
				this._header.Add(string.Empty);
				GenericEnumerableExtensions.Each<string>(Solution._versionLines.ToDictionary()[this.Version ?? Solution.DefaultVersion], new Action<string>(this._header.Add));
			}
		}

		private void CalculateProjectConfigurationPlatforms()
		{
			GlobalSection section = this.FindSection("ProjectConfigurationPlatforms");
			if (section == null)
			{
				section = new GlobalSection("GlobalSection(ProjectConfigurationPlatforms) = postSolution");
				this._sections.Add(section);
			}
			section.Properties.Clear();
			BuildConfiguration[] configurations = this.Configurations().ToArray<BuildConfiguration>();
			GenericEnumerableExtensions.Each<SolutionProject>(from x in this._projects
			where x.ProjectName != "Solution Items"
			select x, delegate(SolutionProject proj)
			{
				GenericEnumerableExtensions.Each<BuildConfiguration>(configurations, delegate(BuildConfiguration config)
				{
					config.WriteProjectConfiguration(proj, section);
				});
			});
			if (section.Empty)
			{
				this._sections.Remove(section);
			}
		}

		public SolutionProject AddProject(string projectName)
		{
			return this.AddProject(this.ParentDirectory, projectName);
		}

		public SolutionProject AddProject(string solutionFolder, string projectName)
		{
			SolutionProject existing = this.FindProject(projectName);
			if (existing != null)
			{
				return existing;
			}
			SolutionProject reference = SolutionProject.CreateNewAt(this.ParentDirectory, projectName);
			this._projects.Add(reference);
			return reference;
		}

		public void AddProject(CsProjFile project)
		{
			this.AddProject(this.ParentDirectory, project, String.Empty);
		}

		public void AddProject(string solutionDirectory, CsProjFile project, string relativeTo)
		{
			SolutionProject existing = this.FindProject(project.ProjectName);
			if (existing != null)
			{
				return;
			}
			SolutionProject reference = new SolutionProject(project, solutionDirectory, relativeTo);
			this._projects.Add(reference);
		}

		public SolutionProject AddProjectFromTemplate(string projectName, string templateFile)
		{
			SolutionProject existing = this.FindProject(projectName);
			if (existing != null)
			{
				throw new ArgumentOutOfRangeException("projectName", FubuCore.StringExtensions.ToFormat("Project with this name ({0}) already exists in the solution", new object[]
				{
					projectName
				}));
			}
			MSBuildProject project = MSBuildProject.CreateFromFile(projectName, templateFile);
			SolutionProject reference = new SolutionProject(new CsProjFile(FubuCore.StringExtensions.AppendPath(this.ParentDirectory, new string[]
			{
				projectName,
				projectName + ".csproj"
			}), project)
			{
				ProjectGuid = Guid.NewGuid()
			}, this.ParentDirectory);
			this._projects.Add(reference);
			return reference;
		}

		public void RemoveProject(CsProjFile project)
		{
			SolutionProject existing = this.FindProject(project.ProjectName);
			if (existing == null)
			{
				return;
			}
			this._projects.Remove(existing);
		}

		public SolutionProject FindProject(string projectName)
		{
			return this._projects.FirstOrDefault((SolutionProject x) => x.ProjectName == projectName);
		}

		public override string ToString()
		{
			return string.Format("{0}", this.Filename);
		}
	}
}
