using System.Xml;

namespace Fubu.CsProjFile.FubuCsProjFile.MSBuild
{
	public class MSBuildTarget : MSBuildObject
	{
		public string Name
		{
			get
			{
				return base.Element.GetAttribute("Name");
			}
		}

		public MSBuildTarget(XmlElement elem) : base(elem)
		{
		}
	}
}
