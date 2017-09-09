using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace Fubu.CsProjFile.FubuCsProjFile
{
	public class ProjectSection
	{
		protected readonly List<string> _properties = new List<string>();

		private readonly string _declaration;

		public ReadOnlyCollection<string> Properties
		{
			get
			{
				return this._properties.AsReadOnly();
			}
		}

		public ProjectSection(string declaration)
		{
			this._declaration = declaration.Trim();
		}

		public void Read(string text)
		{
			this._properties.Add(text.Trim());
		}

		public void Write(StringWriter writer)
		{
			writer.WriteLine("\t" + this._declaration);
			GenericEnumerableExtensions.Each<string>(this.Properties, delegate(string x)
			{
				writer.WriteLine("\t\t" + x);
			});
			writer.WriteLine("\tEndProjectSection");
		}
	}
}
