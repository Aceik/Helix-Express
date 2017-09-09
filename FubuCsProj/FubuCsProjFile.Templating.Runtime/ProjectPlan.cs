using System.Collections.Generic;
using System.Linq;
using System.Text;
using Fubu.CsProjFile.FubuCsProjFile.Templating.Graph;
using Fubu.CsProjFile.FubuCsProjFile.Templating.Planning;

namespace Fubu.CsProjFile.FubuCsProjFile.Templating.Runtime
{
	public class ProjectPlan : ITemplateStep
	{
		public const string NAMESPACE = "%NAMESPACE%";

		public const string ASSEMBLY_NAME = "%ASSEMBLY_NAME%";

		public const string SHORT_NAME = "%SHORT_NAME%";

		public const string PROJECT_PATH = "%PROJECT_PATH%";

		public const string PROJECT_FOLDER = "%PROJECT_FOLDER%";

		public const string RAKE_TASK_PREFIX = "%RAKE_TASK_PREFIX%";

		public static readonly string TemplateFile = "csproj.xml";

		private readonly string _projectName;

		private readonly IList<IProjectAlteration> _alterations = new List<IProjectAlteration>();

		private readonly IList<string> _nugetDeclarations = new List<string>();

		private string _relativePath;

		private readonly Substitutions _substitutions = new Substitutions();

		public Substitutions Substitutions
		{
			get
			{
				return this._substitutions;
			}
		}

		public IList<string> NugetDeclarations
		{
			get
			{
				return this._nugetDeclarations;
			}
		}

		public string ProjectTemplateFile
		{
			get;
			set;
		}

		public string ProjectName
		{
			get
			{
				return this._projectName;
			}
		}

		public IList<IProjectAlteration> Alterations
		{
			get
			{
				return this._alterations;
			}
		}

		public string DotNetVersion
		{
			get;
			set;
		}

		public ProjectPlan(string projectName)
		{
			this._projectName = projectName;
			this._substitutions.Set("%ASSEMBLY_NAME%", projectName);
			string shortName = projectName.Split(new char[]
			{
				'.'
			}).Last<string>();
			this._substitutions.Set("%SHORT_NAME%", shortName);
			this._substitutions.Set("%RAKE_TASK_PREFIX%", shortName.ToLower());
			this.DotNetVersion = FubuCsProjFile.DotNetVersion.V40;
		}

		public void Alter(TemplatePlan plan)
		{
			this._substitutions.Set("%INSTRUCTIONS%", plan.GetInstructions());
			plan.Logger.StartProject(this._alterations.Count);
			plan.StartProject(this);
			this._substitutions.Trace(plan.Logger);
			SolutionProject reference = plan.Solution.FindProject(this._projectName);
			if (reference == null)
			{
				if (FubuCore.StringExtensions.IsEmpty(this.ProjectTemplateFile))
				{
					plan.Logger.Trace("Creating project {0} from the default template", new object[]
					{
						this._projectName
					});
					reference = plan.Solution.AddProject(this._projectName);
				}
				else
				{
					plan.Logger.Trace("Creating project {0} from template at {1}", new object[]
					{
						this._projectName,
						this.ProjectTemplateFile
					});
					reference = plan.Solution.AddProjectFromTemplate(this._projectName, this.ProjectTemplateFile);
				}
				reference.Project.AssemblyName = (reference.Project.RootNamespace = this.ProjectName);
				if (this.DotNetVersion != null)
				{
					reference.Project.DotNetVersion = this.DotNetVersion;
				}
			}
			string projectDirectory = reference.Project.ProjectDirectory;
			plan.FileSystem.CreateDirectory(projectDirectory);
			this._relativePath = FubuCore.StringExtensions.PathRelativeTo(reference.Project.FileName, plan.Root).Replace("\\", "/");
			this._substitutions.Set("%PROJECT_PATH%", this._relativePath);
			this._substitutions.Set("%PROJECT_FOLDER%", GenericEnumerableExtensions.Join(this._relativePath.Split(new char[]
			{
				'/'
			}).Reverse<string>().Skip(1).Reverse<string>(), "/"));
			GenericEnumerableExtensions.Each<IProjectAlteration>(this._alterations, delegate(IProjectAlteration x)
			{
				plan.Logger.TraceAlteration(this.ApplySubstitutionsRaw(x.ToString(), null));
				x.Alter(reference.Project, this);
			});
			this.Substitutions.WriteTo(FubuCore.StringExtensions.AppendPath(projectDirectory, new string[]
			{
				Substitutions.ConfigFile
			}));
			plan.Logger.EndProject();
		}

		public void Add(IProjectAlteration alteration)
		{
			this._alterations.Add(alteration);
		}

		public string ToNugetImportStatement()
		{
			string arg_4B_0 = "{0}: {1}";
			object[] array = new object[2];
			array[0] = this.ProjectName;
			array[1] = GenericEnumerableExtensions.Join(from x in this._nugetDeclarations
			orderby x
			select x, ", ");
			return FubuCore.StringExtensions.ToFormat(arg_4B_0, array);
		}

		public string ApplySubstitutionsRaw(string rawText, string relativePath = null)
		{
			return this._substitutions.ApplySubstitutions(rawText, delegate(StringBuilder builder)
			{
				this.writeNamespace(relativePath, builder);
			});
		}

		internal void ApplySubstitutions(string relativePath, StringBuilder builder)
		{
			this._substitutions.ApplySubstitutions(builder);
			this.writeNamespace(relativePath, builder);
		}

		private void writeNamespace(string relativePath, StringBuilder builder)
		{
			if (FubuCore.StringExtensions.IsNotEmpty(relativePath))
			{
				string @namespace = ProjectPlan.GetNamespace(relativePath, this.ProjectName);
				builder.Replace("%NAMESPACE%", @namespace);
			}
		}

		public static string GetNamespace(string relativePath, string projectName)
		{
			return GenericEnumerableExtensions.Join(relativePath.Split(new char[]
			{
				'/'
			}).Reverse<string>().Skip(1).Union(new string[]
			{
				projectName
			}).Reverse<string>(), ".");
		}

		public override string ToString()
		{
			return FubuCore.StringExtensions.ToFormat("Create or load project '{0}'", new object[]
			{
				this._projectName
			});
		}
	}
}
