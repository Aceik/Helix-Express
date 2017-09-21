using System;
using System.Collections.Generic;
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

        private string _templates = $"D:\\Development\\Projects\\Helix-Express\\template\\";
        private string _companyPrefix = $"FLG";
        private readonly string _slnTemplate;
        private string _newRootFolder = $"D:\\Development\\Projects\\ff-sitecore\\";
        private string _newExpressFolder = $"D:\\Development\\Projects\\ff-sitecore\\";
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

        public SolutionCreator(string companyPrefix, string solutionFolderPath, string helixExpressTemplatesFolder)
        {
            if(!string.IsNullOrWhiteSpace(companyPrefix))
                _companyPrefix = companyPrefix;
            if (!string.IsNullOrWhiteSpace(solutionFolderPath))
                _newRootFolder = _newExpressFolder = solutionFolderPath;
            if (!string.IsNullOrWhiteSpace(helixExpressTemplatesFolder))
                _templates = helixExpressTemplatesFolder;
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
            Console.Out.WriteLine("Beginning Conversion");

            if (!Directory.Exists(_newExpressFolder))
            {
                Directory.CreateDirectory(_newExpressFolder);
                Console.Out.WriteLine($"Creating Directory {_newExpressFolder}");
            }

            if (!File.Exists(_newSlnLocation))
            {
                File.Copy(_slnTemplate, _newSlnLocation);
                Console.Out.WriteLine($"Creating new solution file: {_newSlnLocation}");
            }
            else
            {
                Console.Out.WriteLine($"Clearing solution file: {_newSlnLocation}");
                File.Delete(_newSlnLocation);
                Console.Out.WriteLine($"Recreating solution file: {_newSlnLocation}");
                File.Copy(_slnTemplate, _newSlnLocation);
            }

            this.ExpressSolution = Solution.LoadFrom(_newSlnLocation);
            Console.Out.WriteLine($"Solution loaded: {_newSlnLocation}");
            this.OriginalSolution = Solution.LoadFrom(_referenceSlnLocation);
            Console.Out.WriteLine($"Original Solution loaded: {_referenceSlnLocation}");

            Console.Out.WriteLine($"Processing Feature projects");
            var featureProjet = ProcessProjects("feature", _featureCsprojTemplateLocation, _newFeatureCsprojeLocation);
            Console.Out.WriteLine($"Finished processing Feature projects");
            this.ExpressSolution.AddProject("Feature", featureProjet, featureProjet.ProjectName + ".csproj");

            Console.Out.WriteLine($"Processing foundation projects");
            var foundationProject = ProcessProjects("foundation", _foundationCsprojTemplateLocation, _newFoundationCsprojeLocation);
            Console.Out.WriteLine($"Finished processing Foundation projects");
            this.ExpressSolution.AddProject("Foundation", foundationProject, foundationProject.ProjectName + ".csproj");

            Console.Out.WriteLine($"Processing project layer projects");
            var websiteProject = ProcessProjects("project", _websiteCsprojTemplateLocation, _newWebsiteCsprojeLocation);
            Console.Out.WriteLine($"Finished processing project layer projects");
            this.ExpressSolution.AddProject("Project", websiteProject, websiteProject.ProjectName + ".csproj");

            this.ExpressSolution.Save();
            Console.Out.WriteLine($"New solution is up to date.");
            Console.Out.WriteLine($"*****************************");
            Console.Out.WriteLine($"Open the new SLN file and attempt to build in Visual Studio");
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

            Console.Out.WriteLine($"Processing Project: {newProject.ProjectName}");

            Attach(newProject, layer, "Compile", ".cs", "AssemblyInfo.cs");

            var referencesUnique = CollectReferences(newProject, layer);
            var deDuped = RemoveDuplicateIncludeKeys(referencesUnique);
            //deDuped = RemoveDuplicateHints(referencesUnique);

            Console.Out.WriteLine($"{newProject.ProjectName}: attaching references");
            AttachReferences(newProject, deDuped);
            Console.Out.WriteLine($"{newProject.ProjectName}: FINISHED -- attaching references");

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
                Console.Out.WriteLine($"Processing Project other content: {project.ProjectName}");

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

                Console.Out.WriteLine($"Processing project: {project.ProjectName} layer: {layer}, files: *{fileFilter}");

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
                        Console.Out.WriteLine($"Found duplicate reference {reference.Key}");
                    }
                }
                else   // No comma means versioning information is not present
                {   
                    string uniqueName = reference.Key;

                    // exists in the original dictionary and is not the same entry
                    if (referencesUnique.Keys.Any(x => x.Contains(uniqueName+",")) && referencesUnique.Keys.Any(x => x.Equals(uniqueName)))
                    {
                        Console.Out.WriteLine($"Found duplicate reference {reference.Key}");
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
