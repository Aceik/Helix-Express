using Fubu.CsProjFile.FubuCsProjFile.MSBuild;

namespace Fubu.CsProjFile.FubuCsProjFile
{
	public class CodeFile : ProjectItem
	{
		private const string LinkAtt = "Link";

		public string Link
		{
			get;
			set;
		}

		public CodeFile(string relativePath) : base("Compile", relativePath)
		{
		}

		public CodeFile() : base("Compile")
		{
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
			this.Link = (item.HasMetadata("Link") ? item.GetMetadata("Link") : null);
		}

		internal override void Save()
		{
			base.Save();
			this.UpdateMetadata();
		}

		private void UpdateMetadata()
		{
			if (FubuCore.StringExtensions.IsNotEmpty(this.Link))
			{
				base.BuildItem.SetMetadata("Link", this.Link);
			}
		}
	}
}
