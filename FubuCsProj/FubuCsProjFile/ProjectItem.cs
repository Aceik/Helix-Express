using System;
using System.Linq;
using Fubu.CsProjFile.FubuCsProjFile.MSBuild;

namespace Fubu.CsProjFile.FubuCsProjFile
{
	public abstract class ProjectItem
	{
		private string _name;

		private string _include;

		public string Name
		{
			get
			{
				return this._name;
			}
			protected set
			{
				this._name = value;
			}
		}

		public string Include
		{
			get
			{
				return this._include;
			}
			set
			{
				this._include = value.Replace('/', '\\');
			}
		}

		protected MSBuildItem BuildItem
		{
			get;
			set;
		}

		protected ProjectItem(string name)
		{
			this._name = name;
		}

		protected ProjectItem(string name, string include)
		{
			this._name = name;
			this.Include = include;
		}

		internal bool Matches(MSBuildItem item)
		{
			return item.Name == this.Name && item.Include == this.Include;
		}

		internal virtual MSBuildItem Configure(MSBuildItemGroup group)
		{
			MSBuildItem item = group.Items.FirstOrDefault(new Func<MSBuildItem, bool>(this.Matches)) ?? group.AddNewItem(this.Name, this.Include);
			this.BuildItem = item;
			return item;
		}

		internal virtual void Read(MSBuildItem item)
		{
			this.BuildItem = item;
			this.Include = item.Include;
		}

		internal virtual void Save()
		{
			this.BuildItem.Include = this.Include;
		}

		protected bool Equals(ProjectItem other)
		{
			return string.Equals(this._name, other._name) && string.Equals(this.Include, other.Include);
		}

		public override bool Equals(object obj)
		{
			return !object.ReferenceEquals(null, obj) && (object.ReferenceEquals(this, obj) || (!(obj.GetType() != base.GetType()) && this.Equals((ProjectItem)obj)));
		}

		public override int GetHashCode()
		{
			return ((this._name != null) ? this._name.GetHashCode() : 0) * 397 ^ ((this.Include != null) ? this.Include.GetHashCode() : 0);
		}

		public override string ToString()
		{
			return string.Format("Item {0}: {1}", this.Name, this.Include);
		}
	}
}
