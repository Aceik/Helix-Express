using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Fubu.CsProjFile.FubuCsProjFile
{
	public class GlobalSection
	{
		private readonly string _declaration;

		private readonly IList<string> _properties = new List<string>();

		private readonly SolutionLoading _order;

		private readonly string _name;

		public string Declaration
		{
			get
			{
				return this._declaration;
			}
		}

		public string SectionName
		{
			get
			{
				return this._name;
			}
		}

		public IList<string> Properties
		{
			get
			{
				return this._properties;
			}
		}

		public SolutionLoading LoadingOrder
		{
			get
			{
				return this._order;
			}
		}

		public bool Empty
		{
			get
			{
				return this.Properties == null || this.Properties.Count == 0;
			}
		}

		public GlobalSection(string declaration)
		{
			this._declaration = declaration.Trim();
			this._order = FubuCore.StringExtensions.ToEnum<SolutionLoading>(declaration.Split(new char[]
			{
				'='
			}).Last<string>().Trim());
			int start = declaration.IndexOf('(');
			int end = declaration.IndexOf(')');
			this._name = declaration.Substring(start + 1, end - start - 1);
		}

		public void Read(string text)
		{
			this._properties.Add(text.Trim());
		}

		public void Write(StringWriter writer)
		{
			writer.WriteLine("\t" + this._declaration);
			GenericEnumerableExtensions.Each<string>(this._properties, delegate(string x)
			{
				writer.WriteLine("\t\t" + x);
			});
			writer.WriteLine("\tEndGlobalSection");
		}
	}
}
