using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Fubu.CsProjFile.FubuCsProjFile.Templating.Graph;
using Fubu.CsProjFile.FubuCsProjFile.Templating.Runtime;
using FubuCore;

namespace Fubu.CsProjFile.FubuCsProjFile.Templating.Planning
{
	public class TemplatePlan
	{
		public const string SOLUTION_NAME = "%SOLUTION_NAME%";

		public const string SOLUTION_PATH = "%SOLUTION_PATH%";

		public const string INSTRUCTIONS = "%INSTRUCTIONS%";

		public static readonly string RippleImportFile = "ripple-install.txt";

		public static readonly string InstructionsFile = "instructions.txt";

		private readonly IFileSystem _fileSystem = new FileSystem();

		private readonly IList<string> _handled = new List<string>();

		private readonly StringWriter _instructions = new StringWriter();

		private readonly IList<string> _missingInputs = new List<string>();

		private readonly IList<ITemplateStep> _steps = new List<ITemplateStep>();

		private readonly Substitutions _substitutions = new Substitutions();

		private ProjectPlan _currentProject;

		private Solution _solution;

		public IList<string> MissingInputs
		{
			get
			{
				return this._missingInputs;
			}
		}

		public Substitutions Substitutions
		{
			get
			{
				return this._substitutions;
			}
		}

		public ITemplateLogger Logger
		{
			get;
			private set;
		}

		public string Root
		{
			get;
			set;
		}

		public string SourceName
		{
			get;
			set;
		}

		public string SourceDirectory
		{
			get
			{
				return FubuCore.StringExtensions.AppendPath(this.Root, new string[]
				{
					this.SourceName
				});
			}
		}

		public Solution Solution
		{
			get
			{
				return this._solution;
			}
			set
			{
				this._solution = value;
				this._substitutions.Set("%SOLUTION_NAME%", this._solution.Name);
				this._substitutions.Set("%SOLUTION_PATH%", FubuCore.StringExtensions.PathRelativeTo(this.Solution.Filename, this.Root).Replace("\\", "/"));
			}
		}

		public IFileSystem FileSystem
		{
			get
			{
				return this._fileSystem;
			}
		}

		public IEnumerable<ITemplateStep> Steps
		{
			get
			{
				return this._steps;
			}
		}

		public ProjectPlan CurrentProject
		{
			get
			{
				return this._currentProject ?? this._steps.OfType<ProjectPlan>().LastOrDefault<ProjectPlan>();
			}
		}

		public TemplatePlan(string rootDirectory)
		{
			this.Root = rootDirectory;
			this.SourceName = "src";
			this.Logger = new TemplateLogger();
		}

		public static TemplatePlan CreateClean(string directory)
		{
			FileSystem system = new FileSystem();
			system.CreateDirectory(directory);
			system.CleanDirectory(directory);
			return new TemplatePlan(directory);
		}

		public string ApplySubstitutions(string rawText)
		{
			return this._substitutions.ApplySubstitutions(rawText, delegate(StringBuilder builder)
			{
				if (this.CurrentProject != null)
				{
					this.CurrentProject.ApplySubstitutions(null, builder);
				}
			});
		}

		public void MarkHandled(string file)
		{
			this._handled.Add(file.CanonicalPath());
		}

		public void Add(ITemplateStep step)
		{
			this._steps.Add(step);
		}

		public void StartProject(ProjectPlan project)
		{
			this._currentProject = project;
		}

		public void Execute()
		{
			if (this._missingInputs.Any<string>())
			{
				this.Logger.Trace("Missing Inputs:", new object[0]);
				this.Logger.Trace("---------------", new object[0]);
				GenericEnumerableExtensions.Each<string>(this._missingInputs, delegate(string x)
				{
					Console.WriteLine(x);
				});
				throw new MissingInputException(this._missingInputs.ToArray<string>());
			}
			this.Logger.Starting(this._steps.Count);
			this._substitutions.Trace(this.Logger);
			this._substitutions.Set("%INSTRUCTIONS%", this.GetInstructions().Replace("\"", "'"));
			GenericEnumerableExtensions.Each<ITemplateStep>(this._steps, delegate(ITemplateStep x)
			{
				this.Logger.TraceStep(x);
				x.Alter(this);
			});
			if (this.Solution != null)
			{
				this.Logger.Trace("Saving solution to {0}", new object[]
				{
					this.Solution.Filename
				});
				this.Solution.Save(true);
			}
			this.Substitutions.WriteTo(FubuCore.StringExtensions.AppendPath(this.Root, new string[]
			{
				Substitutions.ConfigFile
			}));
			this.WriteNugetImports();
			this.Logger.Finish();
		}

		public void WritePreview()
		{
			this.Logger.Starting(this._steps.Count);
			this._substitutions.Trace(this.Logger);
			GenericEnumerableExtensions.Each<ITemplateStep>(this._steps, delegate(ITemplateStep x)
			{
				this.Logger.TraceStep(x);
				ProjectPlan project = x as ProjectPlan;
				if (project != null)
				{
					this.Logger.StartProject(project.Alterations.Count);
					project.Substitutions.Trace(this.Logger);
					GenericEnumerableExtensions.Each<IProjectAlteration>(project.Alterations, delegate(IProjectAlteration alteration)
					{
						this.Logger.TraceAlteration(this.ApplySubstitutions(alteration.ToString()));
					});
					this.Logger.EndProject();
				}
			});
			string[] projectsWithNugets = this.determineProjectsWithNugets();
			if (projectsWithNugets.Any<string>())
			{
				Console.WriteLine();
				Console.WriteLine("Nuget imports:");
				GenericEnumerableExtensions.Each<string>(projectsWithNugets, delegate(string x)
				{
					Console.WriteLine(x);
				});
			}
		}

		public void AlterFile(string relativeName, Action<List<string>> alter)
		{
			this._fileSystem.AlterFlatFile(FubuCore.StringExtensions.AppendPath(this.Root, new string[]
			{
				relativeName
			}), alter);
		}

		public bool FileIsUnhandled(string file)
		{
			if (Path.GetFileName(file).ToLowerInvariant() == TemplateLibrary.DescriptionFile)
			{
				return false;
			}
			if (Path.GetFileName(file).ToLowerInvariant() == Input.File)
			{
				return false;
			}
			if (Path.GetFileName(file).ToLowerInvariant() == TemplatePlan.InstructionsFile)
			{
				return false;
			}
			string path = file.CanonicalPath();
			return !this._handled.Contains(path);
		}

		public void CopyUnhandledFiles(string directory)
		{
			IEnumerable<string> unhandledFiles = this._fileSystem.FindFiles(directory, FileSet.Everything()).Where(new Func<string, bool>(this.FileIsUnhandled));
			if (this.CurrentProject == null)
			{
				GenericEnumerableExtensions.Each<string>(unhandledFiles, delegate(string file)
				{
					this.Add(new CopyFileToSolution(FubuCore.StringExtensions.PathRelativeTo(file, directory), file));
				});
				return;
			}
			GenericEnumerableExtensions.Each<string>(unhandledFiles, delegate(string file)
			{
				this.CurrentProject.Add(new CopyFileToProject(FubuCore.StringExtensions.PathRelativeTo(file, directory), file));
			});
		}

		public void WriteNugetImports()
		{
			string[] projectsWithNugets = this.determineProjectsWithNugets();
			if (projectsWithNugets.Any<string>())
			{
				this.Logger.Trace("Writing nuget imports:", new object[0]);
				GenericEnumerableExtensions.Each<string>(projectsWithNugets, delegate(string x)
				{
					this.Logger.Trace(x, new object[0]);
				});
				this.Logger.Trace("", new object[0]);
				TemplateLibrary.FileSystem.AlterFlatFile(FubuCore.StringExtensions.AppendPath(this.Root, new string[]
				{
					TemplatePlan.RippleImportFile
				}), delegate(List<string> list)
				{
					list.AddRange(projectsWithNugets);
				});
			}
		}

		private string[] determineProjectsWithNugets()
		{
			return (from x in this.Steps.OfType<ProjectPlan>()
			where x.NugetDeclarations.Any<string>()
			select x.ToNugetImportStatement()).ToArray<string>();
		}

		public ProjectPlan FindProjectPlan(string name)
		{
			return this._steps.OfType<ProjectPlan>().FirstOrDefault((ProjectPlan x) => x.ProjectName == name);
		}

		public void AddInstructions(string rawText)
		{
			this._instructions.WriteLine(rawText);
			this._instructions.WriteLine();
			this._instructions.WriteLine();
		}

		public void WriteInstructions()
		{
			if (FubuCore.StringExtensions.IsEmpty(this._instructions.ToString()))
			{
				return;
			}
			string instructionText = this.GetInstructions();
			string[] contents = instructionText.SplitOnNewLine();
			this.FileSystem.AlterFlatFile(FubuCore.StringExtensions.AppendPath(this.Root, new string[]
			{
				TemplatePlan.InstructionsFile
			}), delegate(List<string> list)
			{
				list.AddRange(contents);
			});
			Console.WriteLine();
			Console.WriteLine();
			Console.ForegroundColor = ConsoleColor.Cyan;
			GenericEnumerableExtensions.Each<string>(contents, delegate(string x)
			{
				Console.WriteLine(x);
			});
			Console.ResetColor();
		}

		public string GetInstructions()
		{
			return this.ApplySubstitutions(this._instructions.ToString());
		}
	}
}
