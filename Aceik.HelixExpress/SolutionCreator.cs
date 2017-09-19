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

        private bool _includeTestProjects = false;

        private readonly string _templates = $"D:\\Development\\Projects\\Helix-Express\\template\\";
        private readonly string _companyPrefix = $"FLG";
        private readonly string _slnTemplate;
        private readonly string _newRootFolder = $"D:\\Development\\Projects\\ff-sitecore\\";
        private readonly string _newExpressFolder = $"D:\\Development\\Projects\\ff-sitecore\\";
        private readonly string _newSlnLocation;
        private readonly string _referenceSlnLocation;

        private readonly string _foundationCsprojTemplateLocation;
        private readonly string _featureCsprojTemplateLocation;
        private readonly string _websiteCsprojTemplateLocation;

        private readonly string _newFoundationCsprojeLocation;
        private readonly string _newFeatureCsprojeLocation;
        private readonly string _newWebsiteCsprojeLocation;

        private Solution ExpressSolution { get; set; }
        private Solution OriginalSolution { get; set; }

        public SolutionCreator()
        {
            _foundationCsprojTemplateLocation = _templates + $"Sitecore.Foundation.Express.csproj";
            _featureCsprojTemplateLocation = _templates + $"Sitecore.Feature.Express.csproj";
            _websiteCsprojTemplateLocation = _templates + $"Sitecore.Project.Express.csproj";
            _slnTemplate = _templates + $"HelixExpress.Template.sln";
            _referenceSlnLocation = _newRootFolder + $"FLG.FitnessFirst.Sitecore.sln";
            _newSlnLocation = _newExpressFolder + $"Aceik.HelixExpress.sln";
            _newFoundationCsprojeLocation = _newExpressFolder + $"Sitecore.Foundation.Express.csproj";
            _newFeatureCsprojeLocation = _newExpressFolder + $"Sitecore.Feature.Express.csproj";
            _newWebsiteCsprojeLocation = _newExpressFolder + $"Sitecore.Project.Express.csproj";
        }

        public void LoadTemplateSlnFile()
        {
            if (!Directory.Exists(_newExpressFolder))
                Directory.CreateDirectory(_newExpressFolder);

            if (!File.Exists(_newSlnLocation))
                File.Copy(_slnTemplate, _newSlnLocation);
            else
            {
                File.Delete(_newSlnLocation);
                File.Copy(_slnTemplate, _newSlnLocation);
            }

            this.ExpressSolution = Solution.LoadFrom(_newSlnLocation);
            this.OriginalSolution = Solution.LoadFrom(_referenceSlnLocation);

            var featureProjet = ProcessProjects("feature", _featureCsprojTemplateLocation, _newFeatureCsprojeLocation);
            this.ExpressSolution.AddProject("Feature", featureProjet, featureProjet.ProjectName + ".csproj");

            var foundationProject = ProcessProjects("foundation", _foundationCsprojTemplateLocation, _newFoundationCsprojeLocation);
            this.ExpressSolution.AddProject("Foundation", foundationProject, foundationProject.ProjectName + ".csproj");

            var websiteProject = ProcessProjects("project", _websiteCsprojTemplateLocation, _newWebsiteCsprojeLocation);
            this.ExpressSolution.AddProject("Project", websiteProject, websiteProject.ProjectName + ".csproj");

            this.ExpressSolution.Save();
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

            Attach(newProject, layer, "Compile", ".cs", "AssemblyInfo.cs");

            var referencesUnique = CollectReferences(newProject, layer);
            var deDuped = RemoveDuplicateIncludeKeys(referencesUnique);
            //deDuped = RemoveDuplicateHints(referencesUnique);
            AttachReferences(newProject, deDuped);

            Attach(newProject, layer, "Content", ".config");

            AttachOtherContent(newProject, layer);

            Attach(newProject, layer, "None", ".transform");

            Attach(newProject, layer, "Folder", "");

            newProject.Save();
            return newProject;
        }

        public Dictionary<string, MSBuildItem> CollectReferences(CsProjFile newProject, string layer)
        {
            Dictionary<string, MSBuildItem> referencesUnique = new Dictionary<string, MSBuildItem>();
            foreach (var project in OriginalSolution.Projects.Where(x => x.ProjectName.ToLower().Contains(layer)))
            {
                foreach (var itemgroup in project.Project.BuildProject.ItemGroups)
                {
                    var references = itemgroup.Items.Where(x => x.Name == "Reference").ToList();
                    foreach (var referenceItem in references)
                    {

                        // Don't need references within the same layer
                        bool sameLayer = referenceItem.Include.ToLower().StartsWith(($"{_companyPrefix}.{layer}").ToLower());

                        if (!referencesUnique.ContainsKey(referenceItem.Include) && !sameLayer)
                        {
                            referencesUnique.Add(referenceItem.Include, referenceItem);
                        }
                    }
                }
            }
            return referencesUnique;
        }

        public void AttachOtherContent(CsProjFile newProject, string layer)
        {
            var itemGroup = newProject.BuildProject.AddNewItemGroup();
            foreach (var project in OriginalSolution.Projects.Where(x => x.ProjectName.ToLower().Contains(layer)))
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

        public void Attach(CsProjFile newProject, string layer, string nodeName, string fileFilter, string nameFilter = "")
        {
            var itemGroup = newProject.BuildProject.AddNewItemGroup();
            foreach (var project in OriginalSolution.Projects.Where(x => x.ProjectName.ToLower().Contains(layer)))
            {
                if (!_includeTestProjects && project.ProjectName.ToLower().Contains("testing"))
                    continue;

                foreach (var itemgroup in project.Project.BuildProject.ItemGroups)
                {
                    var refs = itemgroup.Items.Where(x => x.Name == nodeName && x.Include.EndsWith(fileFilter)).ToList();
                    
                    if (!string.IsNullOrWhiteSpace(nameFilter))
                        refs = refs.Where(x => !x.Include.Contains(nameFilter)).ToList();

                    foreach (var compile in refs)
                    {
                        string relativeDirectory = project.RelativePath.Replace(project.ProjectName + ".csproj", "").Replace("/", "\\");

                        if (!_includeTestProjects && fileFilter == ".cs" && relativeDirectory.ToLower().Contains("tests"))
                            continue;

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
                var include = refenceItem.Value.Include;

                if (!_includeTestProjects && include.ToLower().Contains("xunit"))
                    continue;

                var refItem = itemGroup2.AddNewItem("Reference", include);
                if (refenceItem.Value.HasMetadata("HintPath"))
                {
                    var hintPath = refenceItem.Value.GetMetadata("HintPath").Replace("..\\..\\..\\..\\", ".\\");
                    hintPath = hintPath.Replace(".\\..\\packages\\", ".\\packages\\");
                        
                    refItem.SetMetadata("HintPath", hintPath);
                }
                    
                if (refenceItem.Value.HasMetadata("Private"))
                    refItem.SetMetadata("Private", refenceItem.Value.GetMetadata("Private"));
            }
        }

        public Dictionary<string, MSBuildItem> RemoveDuplicateIncludeKeys(Dictionary<string, MSBuildItem> referencesUnique)
        {
            Dictionary<string, MSBuildItem> newCopy = new Dictionary<string, MSBuildItem>();
            foreach (var reference in referencesUnique)
            {
                if (reference.Key.Contains(","))
                {
                    string uniqueName = reference.Key.Split(',')[0];
                    if (!newCopy.ContainsKey(uniqueName))
                    {
                        newCopy.Add(uniqueName, reference.Value);
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
                        newCopy.Add(uniqueName, reference.Value);
                    }
                }
            }
            return newCopy;
        }

        //public Dictionary<string, MSBuildItem> RemoveDuplicateHints(Dictionary<string, MSBuildItem> referencesUnique)
        //{
        //    Dictionary<string, MSBuildItem> newCopy = new Dictionary<string, MSBuildItem>();
        //    foreach (var reference in referencesUnique)
        //    {

        //        bool moreThanOne = referencesUnique.Count(x => x.Value.HasMetadata("HintPath") && x.Value.GetMetadata("HintPath") == reference.Value.GetMetadata("HintPath")) > 1;
        //        if (moreThanOne)
        //        {
                    
        //        }

        //        bool onehasAComma = referencesUnique.Count(x => x.Value.GetMetadata("HintPath") == reference.Value.GetMetadata("HintPath") && x.Value.Include.Contains(",")) >= 1;
        //        if (moreThanOne && onehasAComma)
        //        {
        //            if (reference.Value.Include.Contains(","))
        //            {
        //                string uniqueName = reference.Key.Split(',')[0];
        //                if (!newCopy.ContainsKey(uniqueName))
        //                {
        //                    newCopy.Add(uniqueName, reference.Value);
        //                    continue;
        //                }
        //            }
        //        }

        //        if (!newCopy.ContainsKey(reference.Key))
        //            newCopy.Add(reference.Key, reference.Value);
        //    }
        //    return newCopy;
        //}
    }
}
