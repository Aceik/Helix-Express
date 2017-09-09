using System;
using System.Collections.Generic;
using System.Linq;
using FubuCore.Descriptions;

namespace Fubu.CsProjFile.FubuCsProjFile.Templating.Graph
{
	public class ProjectTemplate : DescribesItself
	{
		public string Template;

		public IList<string> Alterations = new List<string>();

		public string Description;

		public string Name;

		public IList<Option> Options;

		public IList<OptionSelection> Selections;

		public string Url;

		public string DotNetVersion
		{
			get;
			set;
		}

		public ProjectTemplate()
		{
			this.Options = new List<Option>();
			this.Selections = new List<OptionSelection>();
		}

		public Option FindOption(string optionName)
		{
			return this.Options.FirstOrDefault((Option x) => FubuCore.StringExtensions.EqualsIgnoreCase(x.Name, optionName));
		}

		public ProjectRequest BuildProjectRequest(TemplateChoices choices)
		{
			ProjectRequest request = new ProjectRequest(choices.ProjectName, this.Template);
			GenericEnumerableExtensions.AddRange<string>(request.Alterations, this.Alterations);
			if (FubuCore.StringExtensions.IsNotEmpty(this.DotNetVersion))
			{
				request.Version = this.DotNetVersion;
			}
			if (choices.Options != null)
			{
				GenericEnumerableExtensions.Each<string>(choices.Options, delegate(string o)
				{
					Option opt = this.FindOption(o);
					if (opt == null)
					{
						if (!this.tryResolveSelection(o, choices) && opt == null)
						{
							throw new Exception(FubuCore.StringExtensions.ToFormat("Unknown option '{0}' for project type {1}", new object[]
							{
								o,
								this.Name
							}));
						}
					}
					else
					{
						GenericEnumerableExtensions.AddRange<string>(request.Alterations, opt.Alterations);
					}
				});
			}
			if (this.Selections != null)
			{
				GenericEnumerableExtensions.Each<OptionSelection>(this.Selections, delegate(OptionSelection selection)
				{
					selection.Configure(choices, request);
				});
			}
			choices.Inputs.Each(delegate(string key, string value)
			{
				request.Substitutions.Set(key, value);
			});
			return request;
		}

		private bool tryResolveSelection(string optionName, TemplateChoices choices)
		{
			OptionSelection selection = (this.Selections ?? ((IList<OptionSelection>)new OptionSelection[0])).FirstOrDefault((OptionSelection x) => x.FindOption(optionName) != null);
			if (selection == null)
			{
				return false;
			}
			choices.Selections.Fill(selection.Name, optionName);
			return true;
		}

		public void Describe(Description description)
		{
			description.Title = (this.Name);
			description.ShortDescription = (this.Description);
			if (FubuCore.StringExtensions.IsNotEmpty(this.Url))
			{
				description.Properties.Fill("Url", this.Url);
			}
			if (this.Selections != null && this.Selections.Any<OptionSelection>())
			{
				description.AddList("Selections", this.Selections);
			}
			if (this.Options != null && this.Options.Any<Option>())
			{
				description.AddList("Options", this.Options);
			}
		}
	}
}
