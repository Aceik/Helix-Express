using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FubuCore;
using FubuCore.Util;

namespace Fubu.CsProjFile.FubuCsProjFile.Templating.Graph
{
	public class TemplateLibrary : ITemplateLibrary
	{
		public static readonly IFileSystem FileSystem = new FileSystem();

		public static readonly string Solution = "solution";

		public static readonly string Project = "project";

		public static readonly string Testing = "testing";

		public static readonly string Alteration = "alteration";

		private readonly Cache<TemplateType, string> _templateDirectories;

		public static readonly string DescriptionFile = "description.txt";

		private readonly string _templatesRoot;

		public TemplateGraph Graph = new TemplateGraph();

		public static TemplateLibrary BuildClean(string root)
		{
			TemplateLibrary.FileSystem.DeleteDirectory(root);
			TemplateLibrary.FileSystem.CreateDirectory(root);
			return new TemplateLibrary(root);
		}

		public TemplateBuilder StartTemplate(TemplateType type, string name)
		{
			string directory = FubuCore.StringExtensions.AppendPath(this._templateDirectories[type], new string[]
			{
				name
			});
			return new TemplateBuilder(directory);
		}

		public TemplateLibrary(string templatesRoot)
		{
			this._templatesRoot = templatesRoot;
			this._templateDirectories = new Cache<TemplateType, string>(delegate(TemplateType type)
			{
				string directory = FubuCore.StringExtensions.AppendPath(this._templatesRoot, new string[]
				{
					type.ToString().ToLowerInvariant()
				});
				TemplateLibrary.FileSystem.CreateDirectory(directory);
				return directory;
			});
			GenericEnumerableExtensions.Each<TemplateType>(Enum.GetValues(typeof(TemplateType)).OfType<TemplateType>(), delegate(TemplateType x)
			{
				this._templateDirectories.FillDefault(x);
			});
			string graphFile = FubuCore.StringExtensions.AppendPath(templatesRoot, new string[]
			{
				TemplateGraph.FILE
			});
			if (File.Exists(graphFile))
			{
				this.Graph = TemplateGraph.Read(graphFile);
			}
		}

		public IEnumerable<Template> All()
		{
			return this._templateDirectories.GetAllKeys().SelectMany(new Func<TemplateType, IEnumerable<Template>>(this.readTemplates));
		}

		private IEnumerable<Template> readTemplates(TemplateType templateType)
		{
			string path = this._templateDirectories[templateType];
			try
			{
				string[] directories = Directory.GetDirectories(path, "*", SearchOption.TopDirectoryOnly);
				for (int i = 0; i < directories.Length; i++)
				{
					string text = directories[i];
					Template template = new Template
					{
						Name = Path.GetFileName(text),
						Path = text,
						Type = templateType
					};
					string text2 = FubuCore.StringExtensions.AppendPath(text, new string[]
					{
						TemplateLibrary.DescriptionFile
					});
					if (TemplateLibrary.FileSystem.FileExists(text2))
					{
						template.Description = TemplateLibrary.FileSystem.ReadStringFromFile(text2);
					}
					yield return template;
				}
			}
			finally
			{
			}
			yield break;
		}

		public Template Find(TemplateType type, string name)
		{
			return this.readTemplates(type).FirstOrDefault((Template x) => FubuCore.StringExtensions.EqualsIgnoreCase(x.Name, name));
		}

		public IEnumerable<Template> Find(TemplateType type, IEnumerable<string> names)
		{
			return from x in names
			select this.Find(type, x);
		}

		public IEnumerable<MissingTemplate> Validate(TemplateType type, params string[] names)
		{
			IEnumerable<Template> source = this.readTemplates(type);
			try
			{
				for (int i = 0; i < names.Length; i++)
				{
					string name = names[i];
					if (!source.Any((Template x) => FubuCore.StringExtensions.EqualsIgnoreCase(x.Name, name)))
					{
						MissingTemplate missingTemplate = new MissingTemplate();
						missingTemplate.Name = name;
						missingTemplate.TemplateType = type;
						missingTemplate.ValidChoices = (from x in source
						select x.Name).ToArray<string>();
						yield return missingTemplate;
					}
				}
			}
			finally
			{
			}
			yield break;
		}
	}
}
