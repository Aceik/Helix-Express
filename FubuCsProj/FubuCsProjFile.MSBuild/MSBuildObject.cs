using System.Xml;

namespace Fubu.CsProjFile.FubuCsProjFile.MSBuild
{
	public class MSBuildObject
	{
		private readonly XmlElement elem;

		public XmlElement Element
		{
			get
			{
				return this.elem;
			}
		}

		public string Condition
		{
			get
			{
				return this.Element.GetAttribute("Condition");
			}
			set
			{
				if (string.IsNullOrEmpty(value))
				{
					this.Element.RemoveAttribute("Condition");
					return;
				}
				this.Element.SetAttribute("Condition", value);
			}
		}

		public MSBuildObject(XmlElement elem)
		{
			this.elem = elem;
		}

		protected XmlElement AddChildElement(string name)
		{
			XmlElement e = this.elem.OwnerDocument.CreateElement(null, name, "http://schemas.microsoft.com/developer/msbuild/2003");
			this.elem.AppendChild(e);
			return e;
		}
	}
}
