using System;
using System.Xml;

namespace Fubu.CsProjFile.FubuCsProjFile.MSBuild
{
	public class MSBuildItem : MSBuildObject
	{
		public string Include
		{
			get
			{
				return base.Element.GetAttribute("Include");
			}
			set
			{
				base.Element.SetAttribute("Include", value);
			}
		}

		public string Name
		{
			get
			{
				return base.Element.Name;
			}
		}

		public MSBuildItem(XmlElement elem) : base(elem)
		{
		}

		public bool HasMetadata(string name)
		{
			return base.Element[name, "http://schemas.microsoft.com/developer/msbuild/2003"] != null;
		}

		public void SetMetadata(string name, string value)
		{
			this.SetMetadata(name, value, true);
		}

		public void SetMetadata(string name, string value, bool isLiteral)
		{
			XmlElement elem = base.Element[name, "http://schemas.microsoft.com/developer/msbuild/2003"];
			if (elem == null)
			{
				elem = base.AddChildElement(name);
				base.Element.AppendChild(elem);
			}
			elem.InnerXml = value;
		}

		public void UnsetMetadata(string name)
		{
			XmlElement elem = base.Element[name, "http://schemas.microsoft.com/developer/msbuild/2003"];
			if (elem != null)
			{
				base.Element.RemoveChild(elem);
				if (!base.Element.HasChildNodes)
				{
					base.Element.IsEmpty = true;
				}
			}
		}

		public string GetMetadata(string name)
		{
			XmlElement elem = base.Element[name, "http://schemas.microsoft.com/developer/msbuild/2003"];
			if (elem != null)
			{
				return elem.InnerXml;
			}
			return null;
		}

		public bool GetMetadataIsFalse(string name)
		{
			return string.Compare(this.GetMetadata(name), "False", StringComparison.OrdinalIgnoreCase) == 0;
		}

		public void MergeFrom(MSBuildItem other)
		{
			foreach (XmlNode node in base.Element.ChildNodes)
			{
				if (node is XmlElement)
				{
					this.SetMetadata(node.LocalName, node.InnerXml);
				}
			}
		}

		public void Remove()
		{
			if (base.Element.ParentNode != null)
			{
				if (base.Element.ParentNode.ChildNodes.Count == 1 && base.Element.ParentNode.ParentNode != null)
				{
					base.Element.ParentNode.ParentNode.RemoveChild(base.Element.ParentNode);
					return;
				}
				base.Element.ParentNode.RemoveChild(base.Element);
			}
		}
	}
}
