using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace Fubu.CsProjFile.FubuCsProjFile.Templating.Graph
{
	public class TemplateGraph
	{
		public static readonly string FILE = "Templates.xml";

		private readonly IList<ProjectCategory> _categories = new List<ProjectCategory>();

		public static TemplateGraph Read(string file)
		{
			XmlDocument document = new XmlDocument();
			document.Load(file);
			TemplateGraph graph = new TemplateGraph();
			foreach (XmlElement element in document.DocumentElement.SelectNodes("category"))
			{
				ProjectCategory category = new ProjectCategory
				{
					Type = element.GetAttribute("type")
				};
				foreach (XmlElement projectElement in element.SelectNodes("project"))
				{
					category.Templates.Add(projectElement.BuildProjectTemplate());
				}
				graph._categories.Add(category);
			}
			return graph;
		}

		public void AddCategory(ProjectCategory category)
		{
			this._categories.Add(category);
		}

		public ProjectCategory FindCategory(string category)
		{
			return this._categories.FirstOrDefault((ProjectCategory x) => FubuCore.StringExtensions.EqualsIgnoreCase(x.Type, category));
		}

		public ProjectRequest BuildProjectRequest(TemplateChoices choices)
		{
			if (FubuCore.StringExtensions.IsEmpty(choices.Category))
			{
				throw new Exception("Category is required");
			}
			if (FubuCore.StringExtensions.IsEmpty(choices.ProjectName))
			{
				throw new Exception("ProjectName is required");
			}
			ProjectCategory category = this.FindCategory(choices.Category);
			if (category == null)
			{
				throw new Exception(FubuCore.StringExtensions.ToFormat("Category '{0}' is unknown", new object[]
				{
					choices.Category
				}));
			}
			ProjectTemplate project = category.FindTemplate(choices.ProjectType);
			if (project == null)
			{
				throw new Exception(FubuCore.StringExtensions.ToFormat("ProjectTemplate '{0}' for category {1} is unknown", new object[]
				{
					choices.ProjectType,
					choices.Category
				}));
			}
			return project.BuildProjectRequest(choices);
		}

		public ProjectCategory AddCategory(string categoryName)
		{
			ProjectCategory category = new ProjectCategory
			{
				Type = categoryName
			};
			this._categories.Add(category);
			return category;
		}
	}
}
