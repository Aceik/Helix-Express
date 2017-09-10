using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Fubu.CsProjFile.FubuCsProjFile
{
	public class SolutionProject
	{
		public static readonly string ProjectLineTemplate = "Project(\"{{{0}}}\") = \"{1}\", \"{2}\", \"{{{3}}}\"";

		private readonly Guid _projectType;

		private readonly Guid _projectGuid;

		private readonly string _projectName;

		private readonly string _relativePath;

		private readonly IList<string> _directives = new List<string>();

		private readonly IList<ProjectSection> _projectSections = new List<ProjectSection>();

		private readonly Lazy<CsProjFile> _project;

		public Guid ProjectGuid
		{
			get
			{
				return this._projectGuid;
			}
		}

		public Guid ProjectType
		{
			get
			{
				return this._projectType;
			}
		}

		public string ProjectName
		{
			get
			{
				return this._projectName;
			}
		}

		public string RelativePath
		{
			get
			{
				return this._relativePath;
			}
		}

		public CsProjFile Project
		{
			get
			{
				return this._project.Value;
			}
		}

		public Solution Solution
		{
			get;
			set;
		}

		public IList<ProjectSection> ProjectSections
		{
			get
			{
				return this._projectSections;
			}
		}

		public ProjectDependenciesSection ProjectDependenciesSection
		{
			get
			{
				return this.ProjectSections.OfType<ProjectDependenciesSection>().FirstOrDefault<ProjectDependenciesSection>();
			}
		}

		public static SolutionProject CreateNewAt(string solutionDirectory, string projectName)
		{
			CsProjFile csProjFile = CsProjFile.CreateAtSolutionDirectory(projectName, solutionDirectory);
			return new SolutionProject(csProjFile, solutionDirectory);
		}

		public SolutionProject(CsProjFile csProjFile, string solutionDirectory, string relativePath = "")
		{
			this._project = new Lazy<CsProjFile>(() => csProjFile);
			this._projectName = csProjFile.ProjectName;

		    string fileName = csProjFile.FileName;
		    if (!string.IsNullOrWhiteSpace(relativePath))
		    {
		        fileName = relativePath;
		    }   
            this._relativePath = fileName;
			this._projectType = csProjFile.ProjectTypes().LastOrDefault<Guid>();
			this._projectGuid = csProjFile.ProjectGuid;
		}

		public SolutionProject(string text, string solutionDirectory)
		{
			SolutionProject solution = this;
			string[] parts = FubuCore.StringExtensions.ToDelimitedArray(text, '=');
			this._projectType = Guid.Parse(parts.First<string>().TextBetweenSquiggles());
			this._projectGuid = Guid.Parse(parts.Last<string>().TextBetweenSquiggles());
			string[] secondParts = FubuCore.StringExtensions.ToDelimitedArray(parts.Last<string>());
			this._projectName = secondParts.First<string>().TextBetweenQuotes();
			this._relativePath = secondParts.ElementAt(1).TextBetweenQuotes().Replace("\\", "/");
			this._project = new Lazy<CsProjFile>(delegate
			{
				string filename = FubuCore.StringExtensions.AppendPath(solutionDirectory, new string[]
				{
				    solution._relativePath
				});
				if (File.Exists(filename))
				{
					CsProjFile projFile = CsProjFile.LoadFrom(filename);
				    solution.InitializeFromSolution(projFile, solution.Solution);
					return projFile;
				}
				CsProjFile project = CsProjFile.CreateAtLocation(filename, solution._projectName);
				project.ProjectGuid = solution._projectGuid;
				return project;
			});
		}

		private void InitializeFromSolution(CsProjFile projFile, Solution solution)
		{
			GlobalSection tfsSourceControl = solution.Sections.FirstOrDefault((GlobalSection section) => section.SectionName.Equals("TeamFoundationVersionControl"));
			if (tfsSourceControl != null)
			{
				this.InitializeTfsSourceControlSettings(projFile, solution, tfsSourceControl);
			}
		}

		private void InitializeTfsSourceControlSettings(CsProjFile projFile, Solution solution, GlobalSection tfsSourceControl)
		{
			string projUnique = tfsSourceControl.Properties.FirstOrDefault((string item) => item.EndsWith(Path.GetFileName(projFile.FileName)));
			if (projUnique == null)
			{
				return;
			}
			int index = Convert.ToInt32(projUnique.Substring("SccProjectUniqueName".Length, projUnique.IndexOf('=') - "SccProjectUniqueName".Length).Trim());
			projFile.SourceControlInformation = new SourceControlInformation(tfsSourceControl.Properties.First((string item) => item.StartsWith("SccProjectUniqueName" + index)).Split(new char[]
			{
				'='
			})[1].Trim(), tfsSourceControl.Properties.First((string item) => item.StartsWith("SccProjectName" + index)).Split(new char[]
			{
				'='
			})[1].Trim(), tfsSourceControl.Properties.First((string item) => item.StartsWith("SccLocalPath" + index)).Split(new char[]
			{
				'='
			})[1].Trim());
		}

		public void Write(StringWriter writer)
		{
			writer.WriteLine(SolutionProject.ProjectLineTemplate, new object[]
			{
				this._projectType.ToString().ToUpper(),
				this._projectName,
				this._relativePath.Replace('/', Path.DirectorySeparatorChar),
				this._projectGuid.ToString().ToUpper()
			});
			GenericEnumerableExtensions.Each<string>(this._directives, delegate(string x)
			{
				writer.WriteLine(x);
			});
			GenericEnumerableExtensions.Each<ProjectSection>(this._projectSections, delegate(ProjectSection x)
			{
				x.Write(writer);
			});
			writer.WriteLine("EndProject");
		}

		public void ReadLine(string text)
		{
			this._directives.Add(text);
		}

		public void AddProjectDependency(Guid projectGuid)
		{
			if (this.ProjectDependenciesSection == null)
			{
				this.ProjectSections.Add(new ProjectDependenciesSection());
			}
			this.ProjectDependenciesSection.Add(projectGuid);
		}

		public void RemoveProjectDependency(Guid guid)
		{
			if (this.ProjectDependenciesSection == null)
			{
				return;
			}
			this.ProjectDependenciesSection.Remove(guid);
			if (this.ProjectDependenciesSection.Dependencies.Count == 0)
			{
				this.ProjectSections.Remove(this.ProjectDependenciesSection);
			}
		}

		public void RemoveAllProjectDependencies()
		{
			if (this.ProjectDependenciesSection == null)
			{
				return;
			}
			this.ProjectDependenciesSection.Clear();
			this.ProjectSections.Remove(this.ProjectDependenciesSection);
		}

		public override string ToString()
		{
			return string.Format("{0} : {1}", this.ProjectName, this.ProjectGuid.ToString("B").ToUpper());
		}
	}
}
