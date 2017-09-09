using System.Collections.Generic;
using System.Xml;

namespace Fubu.CsProjFile.FubuCsProjFile.MSBuild
{
	public class MSBuildItemGroup : MSBuildObject
	{
		private readonly MSBuildProject parent;

		public IEnumerable<MSBuildItem> Items
		{
			get
			{
				foreach (XmlNode xmlNode in base.Element.ChildNodes)
				{
					XmlElement xmlElement = xmlNode as XmlElement;
					if (xmlElement != null)
					{
						yield return this.parent.GetItem(xmlElement);
					}
				}
				yield break;
			}
		}

		internal MSBuildItemGroup(MSBuildProject parent, XmlElement elem) : base(elem)
		{
			this.parent = parent;
		}

		public MSBuildItem AddNewItem(string name, string include)
		{
			XmlElement elem = base.AddChildElement(name);
			MSBuildItem it = this.parent.GetItem(elem);
			it.Include = include;
			return it;
		}
	}
}
