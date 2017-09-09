using System.Xml;

namespace Fubu.CsProjFile.FubuCsProjFile.MSBuild
{
	public class MSBuildProperty : MSBuildObject
	{
		public string Name
		{
			get
			{
				return base.Element.Name;
			}
		}

		public string Value
		{
			get
			{
				return base.Element.InnerXml;
			}
			set
			{
				base.Element.InnerXml = value;
			}
		}

		public MSBuildProperty(XmlElement elem) : base(elem)
		{
		}
	}
}
