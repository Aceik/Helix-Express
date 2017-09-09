using System;
using System.Collections.Generic;
using System.Linq;
using Fubu.CsProjFile.FubuCsProjFile.Templating.Planning;

namespace Fubu.CsProjFile.FubuCsProjFile.Templating.Runtime
{
	public class GemReference : ITemplateStep
	{
		public static readonly string DefaultFeed = "source 'http://rubygems.org'";

		public static readonly string File = "gems.txt";

		public string GemName
		{
			get;
			set;
		}

		public string Version
		{
			get;
			set;
		}

		public GemReference()
		{
		}

		public GemReference(string gemName, string version)
		{
			this.GemName = gemName;
			this.Version = version;
		}

		public void Alter(TemplatePlan plan)
		{
			plan.AlterFile("Gemfile", new Action<List<string>>(this.Alter));
		}

		private void Alter(List<string> list)
		{
			if (!list.Contains(GemReference.DefaultFeed))
			{
				list.Insert(0, GemReference.DefaultFeed);
				list.Insert(1, string.Empty);
			}
			string key = FubuCore.StringExtensions.ToFormat("\"{0}\"", new object[]
			{
				this.GemName
			});
			if (!list.Any((string x) => x.Contains(key)))
			{
				string line = FubuCore.StringExtensions.ToFormat("gem {0}, \"{1}\"", new object[]
				{
					key,
					this.Version
				});
				list.Add(line);
			}
		}

		protected bool Equals(GemReference other)
		{
			return string.Equals(this.GemName, other.GemName) && string.Equals(this.Version, other.Version);
		}

		public override bool Equals(object obj)
		{
			return !object.ReferenceEquals(null, obj) && (object.ReferenceEquals(this, obj) || (!(obj.GetType() != base.GetType()) && this.Equals((GemReference)obj)));
		}

		public override int GetHashCode()
		{
			return ((this.GemName != null) ? this.GemName.GetHashCode() : 0) * 397 ^ ((this.Version != null) ? this.Version.GetHashCode() : 0);
		}

		public override string ToString()
		{
			return string.Format("Add gem to Gemfile: {0}, {1}", this.GemName, this.Version);
		}

		public static void ConfigurePlan(TextFile textFile, TemplatePlan plan)
		{
			GenericEnumerableExtensions.Each<string>(textFile.ReadLines(), delegate(string line)
			{
				string[] parts = FubuCore.StringExtensions.ToDelimitedArray(line);
				plan.Add(new GemReference(parts.First<string>(), parts.Last<string>()));
			});
		}
	}
}
