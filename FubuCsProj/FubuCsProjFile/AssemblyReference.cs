using System.IO;
using Fubu.CsProjFile.FubuCsProjFile.MSBuild;

namespace Fubu.CsProjFile.FubuCsProjFile
{
	public class AssemblyReference : ProjectItem
	{
		private const string HintPathAtt = "HintPath";

		public string HintPath
		{
			get;
			set;
		}

		public string FusionName
		{
			get;
			set;
		}

		public string DisplayName
		{
			get;
			set;
		}

		public bool? SpecificVersion
		{
			get;
			set;
		}

		public bool? Private
		{
			get;
			set;
		}

		public string Aliases
		{
			get;
			set;
		}

		public string AssemblyName
		{
			get
			{
				if (!string.IsNullOrEmpty(this.HintPath))
				{
					return Path.GetFileName(this.HintPath) ?? string.Empty;
				}
				return string.Format("{0}.dll", base.Include.Split(new char[]
				{
					','
				})[0]);
			}
		}

		public AssemblyReference() : base("Reference")
		{
		}

		public AssemblyReference(string assemblyName) : base("Reference", assemblyName)
		{
		}

		public AssemblyReference(string assemblyName, string hintPath) : this(assemblyName)
		{
			this.HintPath = hintPath;
		}

		internal override MSBuildItem Configure(MSBuildItemGroup group)
		{
			MSBuildItem item = base.Configure(group);
			this.UpdateMetaData();
			return item;
		}

		internal override void Save()
		{
			base.Save();
			this.UpdateMetaData();
		}

		internal override void Read(MSBuildItem item)
		{
			base.Read(item);
			this.HintPath = (item.HasMetadata("HintPath") ? item.GetMetadata("HintPath") : null);
			this.FusionName = (item.HasMetadata("FusionName") ? item.GetMetadata("FusionName") : null);
			this.Aliases = (item.HasMetadata("Aliases") ? item.GetMetadata("Aliases") : null);
			this.DisplayName = (item.HasMetadata("Name") ? item.GetMetadata("Name") : null);
			if (item.HasMetadata("SpecificVersion"))
			{
				this.SpecificVersion = new bool?(bool.Parse(item.GetMetadata("SpecificVersion")));
			}
			if (item.HasMetadata("Private"))
			{
				this.Private = new bool?(bool.Parse(item.GetMetadata("Private")));
			}
		}

		private void UpdateMetaData()
		{
			if (FubuCore.StringExtensions.IsNotEmpty(this.HintPath))
			{
				base.BuildItem.SetMetadata("HintPath", this.HintPath);
			}
			if (FubuCore.StringExtensions.IsNotEmpty(this.FusionName))
			{
				base.BuildItem.SetMetadata("FusionName", this.FusionName);
			}
			if (FubuCore.StringExtensions.IsNotEmpty(this.Aliases))
			{
				base.BuildItem.SetMetadata("Aliases", this.Aliases);
			}
			if (FubuCore.StringExtensions.IsNotEmpty(this.DisplayName))
			{
				base.BuildItem.SetMetadata("Name", this.DisplayName);
			}
			if (this.SpecificVersion.HasValue)
			{
				base.BuildItem.SetMetadata("SpecificVersion", this.SpecificVersion.Value.ToString());
			}
			if (this.Private.HasValue)
			{
				base.BuildItem.SetMetadata("Private", this.Private.Value.ToString().ToLower());
			}
		}
	}
}
