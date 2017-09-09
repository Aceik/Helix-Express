using System.Xml;

namespace Fubu.CsProjFile.FubuCsProjFile.MSBuild
{
	public class MSBuildImport : MSBuildObject
	{
		public string Project
		{
			get
			{
				return base.Element.GetAttribute("Project");
			}
		}

		public string Name
		{
			get
			{
				return base.Element.Name;
			}
		}

		public MSBuildImport(XmlElement elem) : base(elem)
		{
		}
	}
}
