using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Fubu.CsProjFile.FubuCsProjFile;
using Fubu.CsProjFile.FubuCsProjFile.MSBuild;
using Microsoft.Build.Construction;
using net.r_eg.MvsSln;
using net.r_eg.MvsSln.Core;
using net.r_eg.MvsSln.Projects;
using Sln = net.r_eg.MvsSln.Sln;


namespace Aceik.HelixExpress.Main
{
    public interface ISolutionCreator
    {
    }

    public class SolutionCreator : ISolutionCreator
    {
        private SolutionFile _slnFile;

        private readonly string templates = $"D:\\Development\\Projects\\Helix-Express\\template\\";
        private readonly string slnTemplate;
        private readonly string newSlnFolder = $"D:\\Development\\Projects\\aceikhelix\\";
        private readonly string newSlnLocation;
        private readonly string referenceSlnLocation;

        private readonly string foundationCsprojTemplateLocation;
        private readonly string newFoundationCsprojeLocation;

        public SolutionCreator()
        {
            foundationCsprojTemplateLocation = templates + $"\\Sitecore.Foundation.csproj";
            slnTemplate = templates + $"\\HelixExpress.Template.sln";
            referenceSlnLocation = newSlnFolder + $"Habitat.sln";
            newSlnLocation = newSlnFolder + $"Aceik.HelixExpress.sln";
            newFoundationCsprojeLocation = newSlnFolder + $"Sitecore.Foundation.csproj";
        }

        public void LoadTemplateSlnFile()
        {
            //_slnFile = SolutionFile.FromFile();
            if(!File.Exists(newSlnLocation))
                File.Copy(slnTemplate, newSlnLocation);

            //ProcessProjects("feature");
            ProcessProjects("foundation", foundationCsprojTemplateLocation);
            //ProcessProjects("projects");
        }

        private void ProcessProjects(string layer, string tempalteCsproj)
        {
            if (!File.Exists(newFoundationCsprojeLocation))
                File.Copy(tempalteCsproj, newFoundationCsprojeLocation);

            var newProject = CsProjFile.LoadFrom(newFoundationCsprojeLocation);
            var itemGroup = newProject.BuildProject.AddNewItemGroup();

            //List<MSBuildItem> compilableItem = new List<MSBuildItem>();
            //using (var sln = new Sln(referenceSlnLocation, SlnItems.All))
            //{
                
            //    foreach (IXProject xp in sln.Result.Env.Projects.Where(x => x.ProjectName.ToLower().Contains(layer)))
            //    {
            //        var itemsa = xp.GetItems();
            //        compilableItem.AddRange(itemsa.Where(x => x.type == "Compile"));
            //    }
            //}
            var solution = Solution.LoadFrom(referenceSlnLocation);
            foreach (var project in solution.Projects.Where(x => x.ProjectName.ToLower().Contains(layer)))
            {
                foreach (var itemgroup in project.Project.BuildProject.ItemGroups)
                {
                    var compilableItems = itemgroup.Items.Where(x => x.Name == "Compile").ToList();
                    //compilableItem.AddRange(compilableItems);
                    foreach (var compile in compilableItems)
                    {
                        string relativeDirectory = project.RelativePath.Replace(project.ProjectName + ".csproj", "").Replace("/", "\\");
                        itemGroup.AddNewItem("Compile", $"{relativeDirectory}{compile.Include}");
                    }
                }
            }

            //foreach (var varItems in compilableItem)
            //{                                    
            //    itemGroup.AddNewItem("Compile", varItems.Include);
            //}

            newProject.Save();
        }
    }
}
