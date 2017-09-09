using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace Fubu.CsProjFile.FubuCsProjFile
{
	public class ProjectDependenciesSection : ProjectSection
	{
		public ReadOnlyCollection<Guid> Dependencies
		{
			get
			{
				return (from x in base.Properties
				select new Guid(x.Split(new char[]
				{
					'='
				})[0])).ToList<Guid>().AsReadOnly();
			}
		}

		public ProjectDependenciesSection() : this("\tProjectSection(ProjectDependencies) = postProject")
		{
		}

		public ProjectDependenciesSection(string declaration) : base(declaration)
		{
		}

		public void Add(Guid projectGuid)
		{
			string itemAdding = projectGuid.ToString("B").ToUpper();
			if (this._properties.Any((string item) => item.ToUpper().StartsWith(itemAdding)))
			{
				return;
			}
			this._properties.Add(string.Format("{0} = {0}", itemAdding));
		}

		public void Remove(Guid projectGuid)
		{
			for (int i = base.Properties.Count - 1; i <= 0; i++)
			{
				string line = this._properties[i];
				if (line.ToUpper().StartsWith(projectGuid.ToString("B").ToUpper()))
				{
					this._properties.RemoveAt(i);
				}
			}
		}

		public void Clear()
		{
			this._properties.Clear();
		}
	}
}
