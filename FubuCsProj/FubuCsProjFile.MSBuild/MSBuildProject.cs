using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Versioning;
using System.Text;
using System.Xml;
using Fubu.CsProjFile.FubuCsProjFile.Templating;
using FubuCore;

namespace Fubu.CsProjFile.FubuCsProjFile.MSBuild
{
	public class MSBuildProject
	{
		private class ProjectWriter : StringWriter
		{
			private readonly Encoding encoding;

			public ByteOrderMark ByteOrderMark
			{
				get;
				private set;
			}

			public override Encoding Encoding
			{
				get
				{
					return this.encoding ?? Encoding.UTF8;
				}
			}

			public ProjectWriter(ByteOrderMark bom)
			{
				this.encoding = ((bom != null) ? Encoding.GetEncoding(bom.Name) : null);
				this.ByteOrderMark = bom;
			}
		}

		public const string Schema = "http://schemas.microsoft.com/developer/msbuild/2003";

		private static XmlNamespaceManager manager;

		private readonly Dictionary<XmlElement, MSBuildObject> elemCache = new Dictionary<XmlElement, MSBuildObject>();

		private Dictionary<string, MSBuildItemGroup> bestGroups;

		private ByteOrderMark bom;

		public XmlDocument doc;

		private bool endsWithEmptyLine;

		private string newLine = Environment.NewLine;

		internal static XmlNamespaceManager XmlNamespaceManager
		{
			get
			{
				if (MSBuildProject.manager == null)
				{
					MSBuildProject.manager = new XmlNamespaceManager(new NameTable());
					MSBuildProject.manager.AddNamespace("tns", "http://schemas.microsoft.com/developer/msbuild/2003");
				}
				return MSBuildProject.manager;
			}
		}

		public string DefaultTargets
		{
			get
			{
				return this.doc.DocumentElement.GetAttribute("DefaultTargets");
			}
			set
			{
				this.doc.DocumentElement.SetAttribute("DefaultTargets", value);
			}
		}

		public Version ToolsVersion
		{
			get
			{
				return new Version(this.doc.DocumentElement.GetAttribute("ToolsVersion"));
			}
			set
			{
				if (value != null)
				{
					this.doc.DocumentElement.SetAttribute("ToolsVersion", value.ToString());
					return;
				}
				this.doc.DocumentElement.RemoveAttribute("ToolsVersion");
			}
		}

		public FrameworkName FrameworkName
		{
			get
			{
				return FrameworkNameDetector.Detect(this);
			}
		}

		public List<string> Imports
		{
			get
			{
				List<string> ims = new List<string>();
				foreach (XmlElement elem in this.doc.DocumentElement.SelectNodes("tns:Import", MSBuildProject.XmlNamespaceManager))
				{
					ims.Add(elem.GetAttribute("Project"));
				}
				return ims;
			}
		}

		public IEnumerable<MSBuildPropertyGroup> PropertyGroups
		{
			get
			{
				foreach (XmlElement elem in this.doc.DocumentElement.SelectNodes("tns:PropertyGroup", MSBuildProject.XmlNamespaceManager))
				{
					yield return this.GetGroup(elem);
				}
				yield break;
			}
		}

		public IEnumerable<MSBuildItemGroup> ItemGroups
		{
			get
			{
				foreach (XmlElement elem in this.doc.DocumentElement.SelectNodes("tns:ItemGroup", MSBuildProject.XmlNamespaceManager))
				{
					yield return this.GetItemGroup(elem);
				}
				yield break;
			}
		}

		public IEnumerable<MSBuildImport> ImportsItems
		{
			get
			{
				foreach (XmlElement elem in this.doc.DocumentElement.SelectNodes("tns:Import", MSBuildProject.XmlNamespaceManager))
				{
					yield return this.GetImport(elem);
				}
				yield break;
			}
		}

		public MSBuildProjectSettings Settings
		{
			get;
			set;
		}

		public IEnumerable<MSBuildTarget> Targets
		{
			get
			{
				foreach (XmlElement elem in this.doc.DocumentElement.SelectNodes("tns:Target", MSBuildProject.XmlNamespaceManager))
				{
					yield return this.GetTarget(elem);
				}
				yield break;
			}
		}

		public static MSBuildProject Create(string assemblyName)
		{
			string text = StreamExtensions.ReadAllText(Assembly.GetExecutingAssembly().GetManifestResourceStream(typeof(MSBuildProject), "Project.txt"));
			return MSBuildProject.create(assemblyName, text);
		}

		private static MSBuildProject create(string assemblyName, string text)
		{
			text = text.Replace("FUBUPROJECTNAME", assemblyName);
			MSBuildProject project = new MSBuildProject();
			project.doc = new XmlDocument
			{
				PreserveWhitespace = false
			};
			project.doc.LoadXml(text);
			return project;
		}

		public static MSBuildProject CreateFromFile(string assemblyName, string file)
		{
			string text = TextFile.FileSystem.ReadStringFromFile(file);
			return MSBuildProject.create(assemblyName, text);
		}

		public static MSBuildProject Parse(string assemblyName, string text)
		{
			return MSBuildProject.create(assemblyName, text);
		}

		public MSBuildProject()
		{
			this.Settings = MSBuildProjectSettings.DefaultSettings;
			this.doc = new XmlDocument();
			this.doc.PreserveWhitespace = false;
			this.doc.AppendChild(this.doc.CreateElement(null, "Project", "http://schemas.microsoft.com/developer/msbuild/2003"));
		}

		public MSBuildItemGroup FindGroup(Func<MSBuildItem, bool> itemTest)
		{
			return this.ItemGroups.FirstOrDefault((MSBuildItemGroup x) => x.Items.Any(itemTest));
		}

		public MSBuildImport FindImport(Func<MSBuildImport, bool> itemTest)
		{
			return this.ImportsItems.FirstOrDefault(itemTest);
		}

		public void Load(string file)
		{
			using (FileStream fs = File.OpenRead(file))
			{
				byte[] buf = new byte[1024];
				int nread;
				if ((nread = fs.Read(buf, 0, buf.Length)) <= 0)
				{
					return;
				}
				int i;
				if (ByteOrderMark.TryParse(buf, nread, out this.bom))
				{
					i = this.bom.Length;
				}
				else
				{
					i = 0;
				}
				while (true)
				{
					if (i < nread)
					{
						if (buf[i] == 13)
						{
							this.newLine = "\r\n";
						}
						else
						{
							if (buf[i] != 10)
							{
								i++;
								continue;
							}
							this.newLine = "\n";
						}
					}
					if (this.newLine == null)
					{
						if ((nread = fs.Read(buf, 0, buf.Length)) <= 0)
						{
							break;
						}
						i = 0;
					}
					if (this.newLine != null)
					{
						goto IL_A7;
					}
				}
				this.newLine = "\n";
				IL_A7:
				this.endsWithEmptyLine = (fs.Seek(-1L, SeekOrigin.End) > 0L && fs.ReadByte() == 10);
			}
			this.doc = new XmlDocument();
			this.doc.PreserveWhitespace = false;
			string xml = File.ReadAllText(file);
			this.doc.LoadXml(xml);
		}

		public void Save(string fileName)
		{
			if (!this.Settings.MaintainOriginalItemOrder)
			{
				GenericEnumerableExtensions.Each<MSBuildItemGroup>(this.ItemGroups, delegate(MSBuildItemGroup group)
				{
					XmlElement[] elements = (from x in @group.Items
					select x.Element into x
					orderby x.GetAttribute("Include")
					select x).ToArray<XmlElement>();
					group.Element.RemoveAll();
					GenericEnumerableExtensions.Each<XmlElement>(elements, delegate(XmlElement x)
					{
						group.Element.AppendChild(x);
					});
				});
			}
			MSBuildProject.ProjectWriter sw = new MSBuildProject.ProjectWriter(this.bom);
			sw.NewLine = this.newLine;
			this.doc.Save(sw);
			string content = sw.ToString();
			if (this.endsWithEmptyLine && !content.EndsWith(this.newLine))
			{
				content += this.newLine;
			}
			bool shouldSave = !this.Settings.OnlySaveIfChanged || (File.Exists(fileName) && !File.ReadAllText(fileName).Equals(content));
			if (shouldSave)
			{
				new FileSystem().WriteStringToFile(fileName, content);
			}
		}

		public void AddNewImport(string name, string condition)
		{
			XmlElement elem = this.doc.CreateElement(null, "Import", "http://schemas.microsoft.com/developer/msbuild/2003");
			elem.SetAttribute("Project", name);
			XmlElement last = this.doc.DocumentElement.SelectSingleNode("tns:Import[last()]", MSBuildProject.XmlNamespaceManager) as XmlElement;
			if (last != null)
			{
				this.doc.DocumentElement.InsertAfter(elem, last);
				return;
			}
			this.doc.DocumentElement.AppendChild(elem);
		}

		public void RemoveImport(string name)
		{
			XmlElement elem = (XmlElement)this.doc.DocumentElement.SelectSingleNode("tns:Import[@Project='" + name + "']", MSBuildProject.XmlNamespaceManager);
			if (elem != null)
			{
				elem.ParentNode.RemoveChild(elem);
				return;
			}
			Console.WriteLine("ppnf:");
		}

		public MSBuildPropertySet GetGlobalPropertyGroup()
		{
			MSBuildPropertyGroupMerged res = new MSBuildPropertyGroupMerged();
			foreach (MSBuildPropertyGroup grp in this.PropertyGroups)
			{
				if (grp.Condition.Length == 0)
				{
					res.Add(grp);
				}
			}
			if (res.GroupCount <= 0)
			{
				return null;
			}
			return res;
		}

		public MSBuildPropertyGroup GetDebugPropertyGroup(string platform = null)
		{
			if (platform == null)
			{
				platform = this.GetGlobalPropertyGroup().GetPropertyValue("Platform");
			}
			return this.PropertyGroups.First((MSBuildPropertyGroup item) => item.Condition.Contains(string.Format("{0}|{1}", "Debug", platform), StringComparison.InvariantCultureIgnoreCase));
		}

		public MSBuildPropertyGroup GetReleasePropertyGroup(string platform = null)
		{
			if (platform == null)
			{
				platform = this.GetGlobalPropertyGroup().GetPropertyValue("Platform");
			}
			return this.PropertyGroups.First((MSBuildPropertyGroup item) => item.Condition.Contains(string.Format("{0}|{1}", "Release", platform), StringComparison.InvariantCultureIgnoreCase));
		}

		public MSBuildPropertyGroup AddNewPropertyGroup(bool insertAtEnd)
		{
			XmlElement elem = this.doc.CreateElement(null, "PropertyGroup", "http://schemas.microsoft.com/developer/msbuild/2003");
			if (insertAtEnd)
			{
				XmlElement last = this.doc.DocumentElement.SelectSingleNode("tns:PropertyGroup[last()]", MSBuildProject.XmlNamespaceManager) as XmlElement;
				if (last != null)
				{
					this.doc.DocumentElement.InsertAfter(elem, last);
				}
			}
			else
			{
				XmlElement first = this.doc.DocumentElement.SelectSingleNode("tns:PropertyGroup", MSBuildProject.XmlNamespaceManager) as XmlElement;
				if (first != null)
				{
					this.doc.DocumentElement.InsertBefore(elem, first);
				}
			}
			if (elem.ParentNode == null)
			{
				XmlElement first2 = this.doc.DocumentElement.SelectSingleNode("tns:ItemGroup", MSBuildProject.XmlNamespaceManager) as XmlElement;
				if (first2 != null)
				{
					this.doc.DocumentElement.InsertBefore(elem, first2);
				}
				else
				{
					this.doc.DocumentElement.AppendChild(elem);
				}
			}
			return this.GetGroup(elem);
		}

		public MSBuildPropertyGroup AddNewPropertyGroup(MSBuildObject insertAfter)
		{
			XmlElement elem = this.doc.CreateElement(null, "PropertyGroup", "http://schemas.microsoft.com/developer/msbuild/2003");
			this.doc.DocumentElement.InsertAfter(elem, insertAfter.Element);
			return this.GetGroup(elem);
		}

		public IEnumerable<MSBuildItem> GetAllItems()
		{
			foreach (XmlElement elem in this.doc.DocumentElement.SelectNodes("tns:ItemGroup/*", MSBuildProject.XmlNamespaceManager))
			{
				yield return this.GetItem(elem);
			}
			yield break;
		}

		public IEnumerable<MSBuildItem> GetAllItems(params string[] names)
		{
			string str = string.Join("|tns:ItemGroup/tns:", names);
			foreach (XmlElement elem in this.doc.DocumentElement.SelectNodes("tns:ItemGroup/tns:" + str, MSBuildProject.XmlNamespaceManager))
			{
				yield return this.GetItem(elem);
			}
			yield break;
		}

		public MSBuildItemGroup AddNewItemGroup()
		{
			XmlElement elem = this.doc.CreateElement(null, "ItemGroup", "http://schemas.microsoft.com/developer/msbuild/2003");
			this.doc.DocumentElement.AppendChild(elem);
			return this.GetItemGroup(elem);
		}

		public MSBuildItem AddNewItem(string name, string include)
		{
			MSBuildItemGroup grp = this.FindBestGroupForItem(name);
			return grp.AddNewItem(name, include);
		}

		private MSBuildItemGroup FindBestGroupForItem(string itemName)
		{
			MSBuildItemGroup group;
			if (this.bestGroups == null)
			{
				this.bestGroups = new Dictionary<string, MSBuildItemGroup>();
			}
			else if (this.bestGroups.TryGetValue(itemName, out group))
			{
				return group;
			}
			foreach (MSBuildItemGroup grp in this.ItemGroups)
			{
				foreach (MSBuildItem it in grp.Items)
				{
					if (it.Name == itemName)
					{
						this.bestGroups[itemName] = grp;
						return grp;
					}
				}
			}
			group = this.AddNewItemGroup();
			this.bestGroups[itemName] = group;
			return group;
		}

		public string GetProjectExtensions(string section)
		{
			XmlElement elem = this.doc.DocumentElement.SelectSingleNode("tns:ProjectExtensions/tns:" + section, MSBuildProject.XmlNamespaceManager) as XmlElement;
			if (elem != null)
			{
				return elem.InnerXml;
			}
			return string.Empty;
		}

		public void SetProjectExtensions(string section, string value)
		{
			XmlElement elem = this.doc.DocumentElement["ProjectExtensions", "http://schemas.microsoft.com/developer/msbuild/2003"];
			if (elem == null)
			{
				elem = this.doc.CreateElement(null, "ProjectExtensions", "http://schemas.microsoft.com/developer/msbuild/2003");
				this.doc.DocumentElement.AppendChild(elem);
			}
			XmlElement sec = elem[section];
			if (sec == null)
			{
				sec = this.doc.CreateElement(null, section, "http://schemas.microsoft.com/developer/msbuild/2003");
				elem.AppendChild(sec);
			}
			sec.InnerXml = value;
		}

		public void RemoveProjectExtensions(string section)
		{
			XmlElement elem = this.doc.DocumentElement.SelectSingleNode("tns:ProjectExtensions/tns:" + section, MSBuildProject.XmlNamespaceManager) as XmlElement;
			if (elem != null)
			{
				XmlElement parent = (XmlElement)elem.ParentNode;
				parent.RemoveChild(elem);
				if (!parent.HasChildNodes)
				{
					parent.ParentNode.RemoveChild(parent);
				}
			}
		}

		public void RemoveItem(MSBuildItem item)
		{
			this.elemCache.Remove(item.Element);
			XmlElement parent = (XmlElement)item.Element.ParentNode;
			item.Element.ParentNode.RemoveChild(item.Element);
			if (parent.ChildNodes.Count == 0)
			{
				this.elemCache.Remove(parent);
				parent.ParentNode.RemoveChild(parent);
				this.bestGroups = null;
			}
		}

		internal MSBuildItem GetItem(XmlElement elem)
		{
			MSBuildObject ob;
			if (this.elemCache.TryGetValue(elem, out ob))
			{
				return (MSBuildItem)ob;
			}
			MSBuildItem it = new MSBuildItem(elem);
			this.elemCache[elem] = it;
			return it;
		}

		private MSBuildPropertyGroup GetGroup(XmlElement elem)
		{
			MSBuildObject ob;
			if (this.elemCache.TryGetValue(elem, out ob))
			{
				return (MSBuildPropertyGroup)ob;
			}
			MSBuildPropertyGroup it = new MSBuildPropertyGroup(this, elem);
			this.elemCache[elem] = it;
			return it;
		}

		private MSBuildItemGroup GetItemGroup(XmlElement elem)
		{
			MSBuildObject ob;
			if (this.elemCache.TryGetValue(elem, out ob))
			{
				return (MSBuildItemGroup)ob;
			}
			MSBuildItemGroup it = new MSBuildItemGroup(this, elem);
			this.elemCache[elem] = it;
			return it;
		}

		private MSBuildImport GetImport(XmlElement elem)
		{
			MSBuildObject ob;
			if (this.elemCache.TryGetValue(elem, out ob))
			{
				return (MSBuildImport)ob;
			}
			MSBuildImport it = new MSBuildImport(elem);
			this.elemCache[elem] = it;
			return it;
		}

		private MSBuildTarget GetTarget(XmlElement elem)
		{
			MSBuildObject ob;
			if (this.elemCache.TryGetValue(elem, out ob))
			{
				return (MSBuildTarget)ob;
			}
			MSBuildTarget it = new MSBuildTarget(elem);
			this.elemCache[elem] = it;
			return it;
		}

		public void RemoveGroup(MSBuildPropertyGroup grp)
		{
			this.elemCache.Remove(grp.Element);
			grp.Element.ParentNode.RemoveChild(grp.Element);
		}

		public static MSBuildProject LoadFrom(string fileName)
		{
			MSBuildProject project = new MSBuildProject();
			project.Load(fileName);
			return project;
		}
	}
}
