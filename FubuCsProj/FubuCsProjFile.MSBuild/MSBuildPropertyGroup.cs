using System;
using System.Collections.Generic;
using System.Xml;

namespace Fubu.CsProjFile.FubuCsProjFile.MSBuild
{
	public class MSBuildPropertyGroup : MSBuildObject, MSBuildPropertySet
	{
		private readonly MSBuildProject parent;

		private readonly Dictionary<string, MSBuildProperty> properties = new Dictionary<string, MSBuildProperty>();

		public MSBuildProject Parent
		{
			get
			{
				return this.parent;
			}
		}

		public IEnumerable<MSBuildProperty> Properties
		{
			get
			{
				foreach (XmlNode xmlNode in base.Element.ChildNodes)
				{
					XmlElement xmlElement = xmlNode as XmlElement;
					if (xmlElement != null)
					{
						MSBuildProperty mSBuildProperty;
						if (this.properties.TryGetValue(xmlElement.Name, out mSBuildProperty))
						{
							yield return mSBuildProperty;
						}
						else
						{
							mSBuildProperty = new MSBuildProperty(xmlElement);
							this.properties[xmlElement.Name] = mSBuildProperty;
							yield return mSBuildProperty;
						}
					}
				}
				yield break;
			}
		}

		public MSBuildPropertyGroup(MSBuildProject parent, XmlElement elem) : base(elem)
		{
			this.parent = parent;
		}

		public MSBuildProperty GetProperty(string name)
		{
			MSBuildProperty prop;
			if (this.properties.TryGetValue(name, out prop))
			{
				return prop;
			}
			XmlElement propElem = base.Element[name, "http://schemas.microsoft.com/developer/msbuild/2003"];
			if (propElem != null)
			{
				prop = new MSBuildProperty(propElem);
				this.properties[name] = prop;
				return prop;
			}
			return null;
		}

		public MSBuildProperty SetPropertyValue(string name, string value, bool preserveExistingCase)
		{
			MSBuildProperty prop = this.GetProperty(name);
			if (prop == null)
			{
				XmlElement pelem = base.AddChildElement(name);
				prop = new MSBuildProperty(pelem);
				this.properties[name] = prop;
				prop.Value = value;
			}
			else if (!preserveExistingCase || !string.Equals(value, prop.Value, StringComparison.OrdinalIgnoreCase))
			{
				prop.Value = value;
			}
			return prop;
		}

		public string GetPropertyValue(string name)
		{
			MSBuildProperty prop = this.GetProperty(name);
			if (prop == null)
			{
				return null;
			}
			return prop.Value;
		}

		public bool RemoveProperty(string name)
		{
			MSBuildProperty prop = this.GetProperty(name);
			if (prop != null)
			{
				this.properties.Remove(name);
				base.Element.RemoveChild(prop.Element);
				return true;
			}
			return false;
		}

		public void RemoveAllProperties()
		{
			List<XmlNode> toDelete = new List<XmlNode>();
			foreach (XmlNode node in base.Element.ChildNodes)
			{
				if (node is XmlElement)
				{
					toDelete.Add(node);
				}
			}
			foreach (XmlNode node2 in toDelete)
			{
				base.Element.RemoveChild(node2);
			}
			this.properties.Clear();
		}

		public void UnMerge(MSBuildPropertySet baseGrp, ISet<string> propsToExclude)
		{
			foreach (MSBuildProperty prop in baseGrp.Properties)
			{
				if (propsToExclude == null || !propsToExclude.Contains(prop.Name))
				{
					MSBuildProperty thisProp = this.GetProperty(prop.Name);
					if (thisProp != null && prop.Value.Equals(thisProp.Value, StringComparison.CurrentCultureIgnoreCase))
					{
						this.RemoveProperty(prop.Name);
					}
				}
			}
		}

		public override string ToString()
		{
			string s = "[MSBuildPropertyGroup:";
			foreach (MSBuildProperty prop in this.Properties)
			{
				string text = s;
				s = string.Concat(new string[]
				{
					text,
					" ",
					prop.Name,
					"=",
					prop.Value
				});
			}
			return s + "]";
		}
	}
}
