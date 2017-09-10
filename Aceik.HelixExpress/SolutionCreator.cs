﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using Fubu.CsProjFile.FubuCsProjFile;
using Fubu.CsProjFile.FubuCsProjFile.MSBuild;
using Microsoft.Build.Construction;
using Sitecore.Diagnostics;

namespace Aceik.HelixExpress
{
    public interface ISolutionCreator
    {
    }

    public class SolutionCreator : ISolutionCreator
    {
        private SolutionFile _slnFile;

        private readonly string _templates = $"D:\\Development\\Projects\\Helix-Express\\template\\";
        private readonly string _slnTemplate;
        private readonly string _newRootFolder = $"D:\\Development\\Projects\\aceikhelix\\";
        private readonly string _newExpressFolder = $"D:\\Development\\Projects\\aceikhelix\\HelixExpress\\";
        private readonly string _newSlnLocation;
        private readonly string _referenceSlnLocation;

        private readonly string _foundationCsprojTemplateLocation;
        private readonly string _featureCsprojTemplateLocation;
        private readonly string _websiteCsprojTemplateLocation;

        private readonly string _newFoundationCsprojeLocation;
        private readonly string _newFeatureCsprojeLocation;
        private readonly string _newWebsiteCsprojeLocation;

        private Solution GrandSolution { get; set; }

        public SolutionCreator()
        {
            _foundationCsprojTemplateLocation = _templates + $"\\Sitecore.Foundation.csproj";
            _featureCsprojTemplateLocation = _templates + $"\\Sitecore.Feature.csproj";
            _websiteCsprojTemplateLocation = _templates + $"\\Sitecore.Website.csproj";
            _slnTemplate = _templates + $"\\HelixExpress.Template.sln";
            _referenceSlnLocation = _newRootFolder + $"Habitat.sln";
            _newSlnLocation = _newExpressFolder + $"Aceik.HelixExpress.sln";
            _newFoundationCsprojeLocation = _newExpressFolder + $"Sitecore.Foundation.csproj";
            _newFeatureCsprojeLocation = _newExpressFolder + $"Sitecore.Feature.csproj";
            _newWebsiteCsprojeLocation = _newExpressFolder + $"Sitecore.Website.csproj";
        }

        public void LoadTemplateSlnFile()
        {
            if (!Directory.Exists(_newExpressFolder))
                Directory.CreateDirectory(_newExpressFolder);

            if (!File.Exists(_newSlnLocation))
                File.Copy(_slnTemplate, _newSlnLocation);

            this.GrandSolution = Solution.LoadFrom(_newSlnLocation);

            var featureProjet = ProcessProjects("feature", _featureCsprojTemplateLocation, _newFeatureCsprojeLocation);
            this.GrandSolution.AddProject("Feature", featureProjet);

            var foundationProject = ProcessProjects("foundation", _foundationCsprojTemplateLocation, _newFoundationCsprojeLocation);
            this.GrandSolution.AddProject("Foundation", foundationProject);

            var websiteProject = ProcessProjects("website", _websiteCsprojTemplateLocation, _newWebsiteCsprojeLocation);
            this.GrandSolution.AddProject("Project", websiteProject);

            this.GrandSolution.Save();
        }

        private CsProjFile ProcessProjects(string layer, string tempalteCsproj, string newFile)
        {
            if (!File.Exists(newFile))
            {
                File.Copy(tempalteCsproj, newFile);
            }
            else if (File.Exists(newFile))
            {
                File.Delete(newFile);
                File.Copy(tempalteCsproj, newFile);
            }

            var newProject = CsProjFile.LoadFrom(newFile);

            AttachCompilations(newProject, layer);

            var referencesUnique = CollectReferences(newProject, layer);
            var deDuped = RemoveDuplicates(referencesUnique);
            AttachReferences(newProject, deDuped);

            AttachConfigs(newProject, layer);

            AttachOtherContent(newProject, layer);

            Attach(newProject, layer, "None", ".transform");

            Attach(newProject, layer, "Folder", "");

            newProject.Save();
            return newProject;
        }

        public Dictionary<string, MSBuildItem> CollectReferences(CsProjFile newProject, string layer)
        {
            Dictionary<string, MSBuildItem> referencesUnique = new Dictionary<string, MSBuildItem>();
            foreach (var project in GrandSolution.Projects.Where(x => x.ProjectName.ToLower().Contains(layer)))
            {
                foreach (var itemgroup in project.Project.BuildProject.ItemGroups)
                {
                    var references = itemgroup.Items.Where(x => x.Name == "Reference").ToList();
                    foreach (var referenceItem in references)
                    {
                        if (!referencesUnique.ContainsKey(referenceItem.Include))
                        {
                            referencesUnique.Add(referenceItem.Include, referenceItem);
                        }
                    }
                }
            }
            return referencesUnique;
        }

        public void AttachCompilations(CsProjFile newProject, string layer)
        {
            var itemGroup = newProject.BuildProject.AddNewItemGroup();
            foreach (var project in GrandSolution.Projects.Where(x => x.ProjectName.ToLower().Contains(layer)))
            {
                foreach (var itemgroup in project.Project.BuildProject.ItemGroups)
                {
                    var compilableItems = itemgroup.Items.Where(x => x.Name == "Compile").ToList();
                    foreach (var compile in compilableItems)
                    {
                        string relativeDirectory = project.RelativePath.Replace(project.ProjectName + ".csproj", "").Replace("/", "\\");
                        itemGroup.AddNewItem("Compile", $"{relativeDirectory}{compile.Include}");
                    }
                }
            }
        }

        public void AttachConfigs(CsProjFile newProject, string layer)
        {
            var itemGroup = newProject.BuildProject.AddNewItemGroup();
            foreach (var project in GrandSolution.Projects.Where(x => x.ProjectName.ToLower().Contains(layer)))
            {
                foreach (var itemgroup in project.Project.BuildProject.ItemGroups)
                {
                    var refs = itemgroup.Items.Where(x => x.Name == "Content" && x.Include.EndsWith(".config")).ToList();
                    foreach (var compile in refs)
                    {
                        string relativeDirectory = project.RelativePath.Replace(project.ProjectName + ".csproj", "").Replace("/", "\\");
                        itemGroup.AddNewItem("Content", $"{relativeDirectory}{compile.Include}");
                    }
                }
            }
        }

        public void AttachOtherContent(CsProjFile newProject, string layer)
        {
            var itemGroup = newProject.BuildProject.AddNewItemGroup();
            foreach (var project in GrandSolution.Projects.Where(x => x.ProjectName.ToLower().Contains(layer)))
            {
                foreach (var itemgroup in project.Project.BuildProject.ItemGroups)
                {
                    var refs = itemgroup.Items.Where(x => x.Name == "Content" && !x.Include.EndsWith(".config") && !x.Include.EndsWith(".cs")).ToList();
                    foreach (var compile in refs)
                    {
                        string relativeDirectory = project.RelativePath.Replace(project.ProjectName + ".csproj", "").Replace("/", "\\");
                        itemGroup.AddNewItem("Content", $"{relativeDirectory}{compile.Include}");
                    }
                }
            }
        }

        public void Attach(CsProjFile newProject, string layer, string nodeName, string fileFilter)
        {
            var itemGroup = newProject.BuildProject.AddNewItemGroup();
            foreach (var project in GrandSolution.Projects.Where(x => x.ProjectName.ToLower().Contains(layer)))
            {
                foreach (var itemgroup in project.Project.BuildProject.ItemGroups)
                {
                    var refs = itemgroup.Items.Where(x => x.Name == nodeName && x.Include.EndsWith(fileFilter)).ToList();
                    foreach (var compile in refs)
                    {
                        string relativeDirectory = project.RelativePath.Replace(project.ProjectName + ".csproj", "").Replace("/", "\\");
                        itemGroup.AddNewItem(nodeName, $"{relativeDirectory}{compile.Include}");
                    }
                }
            }
        }

        public void AttachReferences(CsProjFile newProject, Dictionary<string, MSBuildItem> referencesUnique)
        {
            var itemGroup2 = newProject.BuildProject.AddNewItemGroup();
            foreach (var refenceItem in referencesUnique)
            {
                var refItem = itemGroup2.AddNewItem("Reference", refenceItem.Value.Include);
                if (refenceItem.Value.HasMetadata("HintPath"))
                    refItem.SetMetadata("HintPath", refenceItem.Value.GetMetadata("HintPath"));
                if (refenceItem.Value.HasMetadata("Private"))
                    refItem.SetMetadata("Private", refenceItem.Value.GetMetadata("Private"));
            }
        }

        public Dictionary<string, MSBuildItem> RemoveDuplicates(Dictionary<string, MSBuildItem> referencesUnique)
        {
            Dictionary<string, MSBuildItem> newCopy = new Dictionary<string, MSBuildItem>();
            foreach (var reference in referencesUnique)
            {
                if (reference.Key == "Sitecore.Kernel")
                {
                    
                }

                if (reference.Key.Contains(","))
                {
                    string uniqueName = reference.Key.Split(',')[0];
                    if (!newCopy.ContainsKey(uniqueName))
                    {
                        newCopy.Add(reference.Key, reference.Value);
                    }
                    else
                    {
                        //Log.Debug($"Found duplicate {reference.Key}");  
                    }
                }
                else   // No comma means versioning information is not present
                {   
                    string uniqueName = reference.Key;

                    // exists in the original dictionary and is not the same entry
                    if (referencesUnique.Keys.Any(x => x.Contains(uniqueName+",")) && referencesUnique.Keys.Any(x => x.Equals(uniqueName)))
                    {
                        //Log.Debug($"Found duplicate {reference.Key}");
                    }
                    else
                    {
                        newCopy.Add(reference.Key, reference.Value);
                    }
                }
            }
            return newCopy;
        }
    }
}
