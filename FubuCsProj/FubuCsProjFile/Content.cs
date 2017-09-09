using Fubu.CsProjFile.FubuCsProjFile.MSBuild;

namespace Fubu.CsProjFile.FubuCsProjFile
{
	public class Content : ProjectItem
	{
		private const string LinkAtt = "Link";

		public static readonly string CopyToOutputDirectoryAtt = "CopyToOutputDirectory";

		public ContentCopy CopyToOutputDirectory
		{
			get;
			set;
		}

		public string Link
		{
			get;
			set;
		}

		public Content() : base("Content")
		{
			this.CopyToOutputDirectory = ContentCopy.Never;
		}

		public Content(string include) : base("Content", include)
		{
			this.CopyToOutputDirectory = ContentCopy.Never;
		}

		protected Content(string buildAction, string include) : base(buildAction, include)
		{
			this.CopyToOutputDirectory = ContentCopy.Never;
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
			string copyString = item.HasMetadata(Content.CopyToOutputDirectoryAtt) ? item.GetMetadata(Content.CopyToOutputDirectoryAtt) : null;
			string a;
			if ((a = copyString) != null)
			{
				if (!(a == "Always"))
				{
					if (a == "PreserveNewest")
					{
						this.CopyToOutputDirectory = ContentCopy.IfNewer;
					}
				}
				else
				{
					this.CopyToOutputDirectory = ContentCopy.Always;
				}
			}
			else
			{
				this.CopyToOutputDirectory = ContentCopy.Never;
			}
			this.Link = (item.HasMetadata("Link") ? item.GetMetadata("Link") : null);
		}

		internal override void Save()
		{
			base.Save();
			this.UpdateMetadata();
		}

		private void UpdateMetadata()
		{
			switch (this.CopyToOutputDirectory)
			{
			case ContentCopy.Always:
				base.BuildItem.SetMetadata(Content.CopyToOutputDirectoryAtt, "Always");
				break;
			case ContentCopy.IfNewer:
				base.BuildItem.SetMetadata(Content.CopyToOutputDirectoryAtt, "PreserveNewest");
				break;
			}
			if (FubuCore.StringExtensions.IsNotEmpty(this.Link))
			{
				base.BuildItem.SetMetadata("Link", this.Link);
			}
		}
	}
}
