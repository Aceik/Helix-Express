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

        private readonly string _templates = $"D:\\Development\\Projects\\Helix-Express\\template\\";
        private readonly string _slnTemplate;
        private readonly string _newSlnFolder = $"D:\\Development\\Projects\\aceikhelix\\";
        private readonly string _newSlnLocation;
        private readonly string _referenceSlnLocation;

        private readonly string _foundationCsprojTemplateLocation;
        private readonly string _newFoundationCsprojeLocation;

        private Solution GrandSolution { get; set; }

        public SolutionCreator()
        {
            _foundationCsprojTemplateLocation = _templates + $"\\Sitecore.Foundation.csproj";
            _slnTemplate = _templates + $"\\HelixExpress.Template.sln";
            _referenceSlnLocation = _newSlnFolder + $"Habitat.sln";
            _newSlnLocation = _newSlnFolder + $"Aceik.HelixExpress.sln";
            _newFoundationCsprojeLocation = _newSlnFolder + $"Sitecore.Foundation.csproj";
        }

        public void LoadTemplateSlnFile()
        {
            //_slnFile = SolutionFile.FromFile();
            if(!File.Exists(_newSlnLocation))
                File.Copy(_slnTemplate, _newSlnLocation);

            this.GrandSolution = Solution.LoadFrom(_referenceSlnLocation);

            //ProcessProjects("feature");
            ProcessProjects("foundation", _foundationCsprojTemplateLocation);
            //ProcessProjects("projects");
        }

        private void ProcessProjects(string layer, string tempalteCsproj)
        {
            if (!File.Exists(_newFoundationCsprojeLocation))
                File.Copy(tempalteCsproj, _newFoundationCsprojeLocation);

            var newProject = CsProjFile.LoadFrom(_newFoundationCsprojeLocation);

            AttachCompilations(newProject, layer);

            var referencesUnique = CollectReferences(newProject, layer);
            var deDuped = RemoveDuplicates(referencesUnique);
            AttachReferences(newProject, deDuped);

            newProject.Save();
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
