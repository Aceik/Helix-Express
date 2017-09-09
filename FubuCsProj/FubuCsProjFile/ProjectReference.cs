using System;
using System.IO;
using Fubu.CsProjFile.FubuCsProjFile.MSBuild;

namespace Fubu.CsProjFile.FubuCsProjFile
{
	public class ProjectReference : ProjectItem
	{
		public Guid ProjectGuid
		{
			get;
			set;
		}

		public string ProjectName
		{
			get;
			set;
		}

		public ProjectReference() : base("ProjectReference")
		{
		}

		public ProjectReference(string include) : base("ProjectReference", include)
		{
		}

		public ProjectReference(CsProjFile targetProject, CsProjFile reference) : base("ProjectReference")
		{
			base.Include = Path.Combine(FubuCore.StringExtensions.PathRelativeTo(reference.ProjectDirectory, targetProject.ProjectDirectory), Path.GetFileName(reference.FileName));
			this.ProjectGuid = reference.ProjectGuid;
			this.ProjectName = reference.ProjectName;
		}

		internal override MSBuildItem Configure(MSBuildItemGroup group)
		{
			MSBuildItem item = base.Configure(group);
			this.UpdateMetadata();
			return item;
		}

		internal override void Read(MSBuildItem item)
		{
			base.Read(item);
			this.ProjectName = item.GetMetadata("Name");
			string raw = item.GetMetadata("Project").TrimStart(new char[]
			{
				'{'
			}).TrimEnd(new char[]
			{
				'}'
			});
			this.ProjectGuid = Guid.Parse(raw);
		}

		internal override void Save()
		{
			base.Save();
			this.UpdateMetadata();
		}

		private void UpdateMetadata()
		{
			base.BuildItem.SetMetadata("Project", FubuCore.StringExtensions.ToFormat("{{{0}}}", new object[]
			{
				this.ProjectGuid
			}).ToUpper());
			if (this.ProjectName != null)
			{
				base.BuildItem.SetMetadata("Name", this.ProjectName);
			}
		}
	}
}
