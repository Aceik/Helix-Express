using System.Collections.Generic;
using FubuCore;

namespace Fubu.CsProjFile.FubuCsProjFile
{
	[MarkedForTermination]
	public class Sln
	{
		private readonly IList<CsProjFile> _projects = new List<CsProjFile>();

		private readonly IList<string> _postSolution = new List<string>();

		public string FileName
		{
			get;
			private set;
		}

		public IEnumerable<CsProjFile> Projects
		{
			get
			{
				return this._projects;
			}
		}

		public IEnumerable<string> PostSolutionConfiguration
		{
			get
			{
				return this._postSolution;
			}
		}

		public Sln(string fileName)
		{
			this.FileName = fileName;
		}

		public void AddProject(CsProjFile project)
		{
			GenericEnumerableExtensions.Fill<CsProjFile>(this._projects, project);
		}

		public void RegisterPostSolutionConfiguration(string projectGuid, string config)
		{
			string id = "{" + projectGuid + "}";
			GenericEnumerableExtensions.Fill<string>(this._postSolution, FubuCore.StringExtensions.ToFormat("\t\t{0}.{1}", new object[]
			{
				id,
				config
			}));
		}

		public void RegisterPostSolutionConfigurations(string projectGuid, params string[] configs)
		{
			GenericEnumerableExtensions.Each<string>(configs, delegate(string config)
			{
				this.RegisterPostSolutionConfiguration(projectGuid, config);
			});
		}
	}
}
