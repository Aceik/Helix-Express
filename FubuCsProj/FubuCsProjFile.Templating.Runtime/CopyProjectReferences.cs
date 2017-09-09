using System.Collections.Generic;
using System.IO;
using System.Linq;
using Fubu.CsProjFile.FubuCsProjFile.Templating.Planning;

namespace Fubu.CsProjFile.FubuCsProjFile.Templating.Runtime
{
	public class CopyProjectReferences : ITemplateStep
	{
		private readonly string _originalProject;

		public string OriginalProject
		{
			get
			{
				return this._originalProject;
			}
		}

		public CopyProjectReferences(string originalProject)
		{
			this._originalProject = originalProject;
		}

		public void Alter(TemplatePlan plan)
		{
			CsProjFile original = plan.Solution.FindProject(this._originalProject).Project;
			ProjectPlan originalPlan = plan.FindProjectPlan(this._originalProject);
			ProjectPlan testPlan = plan.CurrentProject;
			CsProjFile testProject = plan.Solution.FindProject(testPlan.ProjectName).Project;
			CopyProjectReferences.copyNugetDeclarations(originalPlan, testPlan, original, testProject);
			this.findNugetsInOriginalRippleDeclarations(plan, testPlan);
			CopyProjectReferences.buildProjectReference(original, testProject);
		}

		private static void copyNugetDeclarations(ProjectPlan originalPlan, ProjectPlan testPlan, CsProjFile original, CsProjFile testProject)
		{
			GenericEnumerableExtensions.Each<string>(originalPlan.NugetDeclarations, delegate(string x)
			{
				GenericEnumerableExtensions.Fill<string>(testPlan.NugetDeclarations, x);
			});
			GenericEnumerableExtensions.Each<AssemblyReference>(from x in original.All<AssemblyReference>()
			where FubuCore.StringExtensions.IsEmpty(x.HintPath)
			select x, delegate(AssemblyReference x)
			{
				testProject.Add<AssemblyReference>(x.Include);
			});
		}

		private void findNugetsInOriginalRippleDeclarations(TemplatePlan plan, ProjectPlan testPlan)
		{
			string configFile = FubuCore.StringExtensions.AppendPath(FubuCore.StringExtensions.ParentDirectory(this._originalProject), new string[]
			{
				"ripple.dependencies.config"
			});
			plan.FileSystem.ReadTextFile(configFile, delegate(string line)
			{
				if (FubuCore.StringExtensions.IsNotEmpty(line))
				{
					GenericEnumerableExtensions.Fill<string>(testPlan.NugetDeclarations, line);
				}
			});
		}

		private static void buildProjectReference(CsProjFile original, CsProjFile testProject)
		{
			string relativePathToTheOriginal = FubuCore.StringExtensions.PathRelativeTo(original.FileName, testProject.FileName);
			if (FubuCore.StringExtensions.ParentDirectory(FubuCore.StringExtensions.ParentDirectory(original.FileName)) == FubuCore.StringExtensions.ParentDirectory(FubuCore.StringExtensions.ParentDirectory(testProject.FileName)))
			{
				relativePathToTheOriginal = Path.Combine("..", Path.GetFileName(FubuCore.StringExtensions.ParentDirectory(original.FileName)), Path.GetFileName(original.FileName));
			}
			ProjectReference reference = new ProjectReference(relativePathToTheOriginal)
			{
				ProjectGuid = original.ProjectGuid,
				ProjectName = original.ProjectName
			};
			testProject.Add<ProjectReference>(reference);
		}

		public override string ToString()
		{
			return string.Format("Copy all references from {0}", this._originalProject);
		}
	}
}
