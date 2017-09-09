using System;
using System.Collections.Generic;
using System.Linq;

namespace Fubu.CsProjFile.FubuCsProjFile.MSBuild
{
	internal class MSBuildPropertyGroupMerged : MSBuildPropertySet
	{
		private readonly List<MSBuildPropertyGroup> groups = new List<MSBuildPropertyGroup>();

		public int GroupCount
		{
			get
			{
				return this.groups.Count;
			}
		}

		public IEnumerable<MSBuildProperty> Properties
		{
			get
			{
				foreach (MSBuildPropertyGroup current in this.groups)
				{
					foreach (MSBuildProperty current2 in current.Properties)
					{
						yield return current2;
					}
				}
				yield break;
			}
		}

		public MSBuildProperty GetProperty(string name)
		{
			for (int i = this.groups.Count - 1; i >= 0; i--)
			{
				MSBuildPropertyGroup g = this.groups[i];
				MSBuildProperty p = g.GetProperty(name);
				if (p != null)
				{
					return p;
				}
			}
			return null;
		}

		public MSBuildProperty SetPropertyValue(string name, string value, bool preserveExistingCase)
		{
			MSBuildProperty p = this.GetProperty(name);
			if (p != null)
			{
				if (!preserveExistingCase || !string.Equals(value, p.Value, StringComparison.OrdinalIgnoreCase))
				{
					p.Value = value;
				}
				return p;
			}
			return this.groups[0].SetPropertyValue(name, value, preserveExistingCase);
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
			bool found = false;
			foreach (MSBuildPropertyGroup g in this.groups)
			{
				if (g.RemoveProperty(name))
				{
					this.Prune(g);
					found = true;
				}
			}
			return found;
		}

		public void RemoveAllProperties()
		{
			foreach (MSBuildPropertyGroup g in this.groups)
			{
				g.RemoveAllProperties();
				this.Prune(g);
			}
		}

		public void UnMerge(MSBuildPropertySet baseGrp, ISet<string> propertiesToExclude)
		{
			foreach (MSBuildPropertyGroup g in this.groups)
			{
				g.UnMerge(baseGrp, propertiesToExclude);
			}
		}

		public void Add(MSBuildPropertyGroup g)
		{
			this.groups.Add(g);
		}

		private void Prune(MSBuildPropertyGroup g)
		{
			if (g != this.groups[0] && !g.Properties.Any<MSBuildProperty>())
			{
				g.Parent.RemoveGroup(g);
			}
		}
	}
}
