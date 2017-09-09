using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace Fubu.CsProjFile.FubuCsProjFile.Templating.Graph
{
	public static class TemplateGraphExtensions
	{
		public static Option Find(this IList<Option> list, string name)
		{
			return list.FirstOrDefault((Option x) => FubuCore.StringExtensions.EqualsIgnoreCase(x.Name, name));
		}

		public static IList<Option> ReadOptions(this XmlElement parentElement)
		{
			List<Option> options = new List<Option>();
			foreach (XmlElement element in parentElement.SelectNodes("option"))
			{
				Option option = new Option
				{
					Name = element.GetAttribute("name"),
					Description = element.GetAttribute("description"),
					Alterations = FubuCore.StringExtensions.ToDelimitedArray(element.GetAttribute("alterations")).ToList<string>(),
					Url = element.GetAttribute("url")
				};
				options.Add(option);
			}
			return options;
		}

		public static IEnumerable<OptionSelection> BuildSelections(this XmlElement element)
		{
			foreach (XmlElement selectionElement in element.SelectNodes("selection"))
			{
				yield return selectionElement.BuildSelection();
			}
			yield break;
		}

		public static OptionSelection BuildSelection(this XmlElement selectionElement)
		{
			OptionSelection selection = new OptionSelection
			{
				Name = selectionElement.GetAttribute("name"),
				Description = selectionElement.GetAttribute("description")
			};
			selection.Options = selectionElement.ReadOptions();
			return selection;
		}

		public static ProjectTemplate BuildProjectTemplate(this XmlElement element)
		{
			ProjectTemplate projectTemplate = new ProjectTemplate
			{
				Name = element.GetAttribute("name"),
				Description = element.GetAttribute("description"),
				Template = element.GetAttribute("template"),
				DotNetVersion = element.GetAttribute("dotnet")
			};
			if (element.HasAttribute("alterations"))
			{
				GenericEnumerableExtensions.AddRange<string>(projectTemplate.Alterations, FubuCore.StringExtensions.ToDelimitedArray(element.GetAttribute("alterations")));
			}
			projectTemplate.Url = element.GetAttribute("url");
			projectTemplate.Options = element.ReadOptions();
			GenericEnumerableExtensions.AddRange<OptionSelection>(projectTemplate.Selections, element.BuildSelections());
			return projectTemplate;
		}
	}
}
