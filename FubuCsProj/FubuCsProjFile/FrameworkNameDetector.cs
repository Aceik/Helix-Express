using System;
using System.Linq;
using System.Runtime.Versioning;
using Fubu.CsProjFile.FubuCsProjFile.MSBuild;

namespace Fubu.CsProjFile.FubuCsProjFile
{
	public class FrameworkNameDetector
	{
		public const string DefaultIdentifier = ".NETFramework";

		public const string DefaultFrameworkVersion = "v4.0";

		public static FrameworkName Detect(MSBuildProject project)
		{
			MSBuildPropertyGroup group = project.PropertyGroups.FirstOrDefault((MSBuildPropertyGroup x) => x.Properties.Any((MSBuildProperty p) => p.Name.Contains("TargetFramework")));
			string identifier = ".NETFramework";
			string versionString = "v4.0";
			string profile = null;
			if (group != null)
			{
				identifier = (group.GetPropertyValue("TargetFrameworkIdentifier") ?? ".NETFramework");
				versionString = (group.GetPropertyValue("TargetFrameworkVersion") ?? "v4.0");
				profile = group.GetPropertyValue("TargetFrameworkProfile");
			}
			Version version = Version.Parse(versionString.Replace("v", "").Replace("V", ""));
			return new FrameworkName(identifier, version, profile);
		}
	}
}
