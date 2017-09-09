using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using Fubu.CsProjFile.FubuCsProjFile.MSBuild;
using FubuCore;

namespace Fubu.CsProjFile.FubuCsProjFile
{
	public class CsProjFile
	{
		private const string PROJECTGUID = "ProjectGuid";

		private const string ROOT_NAMESPACE = "RootNamespace";

		private const string ASSEMBLY_NAME = "AssemblyName";

		private readonly string _fileName;

		private readonly MSBuildProject _project;

		private readonly Dictionary<string, ProjectItem> _projectItemCache = new Dictionary<string, ProjectItem>();

		public static readonly Guid ClassLibraryType = Guid.Parse("FAE04EC0-301F-11D3-BF4B-00C04F79EFBC");

		public static readonly Guid WebSiteLibraryType = new Guid("E24C65DC-7377-472B-9ABA-BC803B73C61A");

		public static readonly Guid VisualStudioSetupLibraryType = new Guid("54435603-DBB4-11D2-8724-00A0C9A8B90C");

		private AssemblyInfo assemblyInfo;

		public Guid ProjectGuid
		{
			get
			{
				string raw = (from x in this._project.PropertyGroups
				select x.GetPropertyValue("ProjectGuid")).FirstOrDefault((string x) => FubuCore.StringExtensions.IsNotEmpty(x));
				if (!FubuCore.StringExtensions.IsEmpty(raw))
				{
					return Guid.Parse(raw.TrimStart(new char[]
					{
						'{'
					}).TrimEnd(new char[]
					{
						'}'
					}));
				}
				return Guid.Empty;
			}
			internal set
			{
				MSBuildPropertyGroup arg_51_0;
				if ((arg_51_0 = this._project.PropertyGroups.FirstOrDefault((MSBuildPropertyGroup x) => x.Properties.Any((MSBuildProperty p) => p.Name == "ProjectGuid"))) == null)
				{
					arg_51_0 = (this._project.PropertyGroups.FirstOrDefault<MSBuildPropertyGroup>() ?? this._project.AddNewPropertyGroup(true));
				}
				MSBuildPropertyGroup group = arg_51_0;
				group.SetPropertyValue("ProjectGuid", value.ToString().ToUpper(), true);
			}
		}

		public string AssemblyName
		{
			get
			{
				MSBuildPropertyGroup arg_51_0;
				if ((arg_51_0 = this._project.PropertyGroups.FirstOrDefault((MSBuildPropertyGroup x) => x.Properties.Any((MSBuildProperty p) => p.Name == "AssemblyName"))) == null)
				{
					arg_51_0 = (this._project.PropertyGroups.FirstOrDefault<MSBuildPropertyGroup>() ?? this._project.AddNewPropertyGroup(true));
				}
				MSBuildPropertyGroup group = arg_51_0;
				return group.GetPropertyValue("AssemblyName");
			}
			set
			{
				MSBuildPropertyGroup arg_51_0;
				if ((arg_51_0 = this._project.PropertyGroups.FirstOrDefault((MSBuildPropertyGroup x) => x.Properties.Any((MSBuildProperty p) => p.Name == "AssemblyName"))) == null)
				{
					arg_51_0 = (this._project.PropertyGroups.FirstOrDefault<MSBuildPropertyGroup>() ?? this._project.AddNewPropertyGroup(true));
				}
				MSBuildPropertyGroup group = arg_51_0;
				group.SetPropertyValue("AssemblyName", value, true);
			}
		}

		public string RootNamespace
		{
			get
			{
				MSBuildPropertyGroup arg_51_0;
				if ((arg_51_0 = this._project.PropertyGroups.FirstOrDefault((MSBuildPropertyGroup x) => x.Properties.Any((MSBuildProperty p) => p.Name == "RootNamespace"))) == null)
				{
					arg_51_0 = (this._project.PropertyGroups.FirstOrDefault<MSBuildPropertyGroup>() ?? this._project.AddNewPropertyGroup(true));
				}
				MSBuildPropertyGroup group = arg_51_0;
				return group.GetPropertyValue("RootNamespace");
			}
			set
			{
				MSBuildPropertyGroup arg_51_0;
				if ((arg_51_0 = this._project.PropertyGroups.FirstOrDefault((MSBuildPropertyGroup x) => x.Properties.Any((MSBuildProperty p) => p.Name == "RootNamespace"))) == null)
				{
					arg_51_0 = (this._project.PropertyGroups.FirstOrDefault<MSBuildPropertyGroup>() ?? this._project.AddNewPropertyGroup(true));
				}
				MSBuildPropertyGroup group = arg_51_0;
				group.SetPropertyValue("RootNamespace", value, true);
			}
		}

		public string ProjectName
		{
			get
			{
				return Path.GetFileNameWithoutExtension(this._fileName);
			}
		}

		public MSBuildProject BuildProject
		{
			get
			{
				return this._project;
			}
		}

		public string FileName
		{
			get
			{
				return this._fileName;
			}
		}

		public string ProjectDirectory
		{
			get
			{
				return FubuCore.StringExtensions.ParentDirectory(this._fileName);
			}
		}

		public FrameworkName FrameworkName
		{
			get
			{
				return this._project.FrameworkName;
			}
		}

		public string DotNetVersion
		{
			get
			{
				return (from x in this._project.PropertyGroups
				select x.GetPropertyValue("TargetFrameworkVersion")).FirstOrDefault((string x) => FubuCore.StringExtensions.IsNotEmpty(x));
			}
			set
			{
				MSBuildPropertyGroup arg_51_0;
				if ((arg_51_0 = this._project.PropertyGroups.FirstOrDefault((MSBuildPropertyGroup x) => x.Properties.Any((MSBuildProperty p) => p.Name == "TargetFrameworkVersion"))) == null)
				{
					arg_51_0 = (this._project.PropertyGroups.FirstOrDefault<MSBuildPropertyGroup>() ?? this._project.AddNewPropertyGroup(true));
				}
				MSBuildPropertyGroup group = arg_51_0;
				group.SetPropertyValue("TargetFrameworkVersion", value, true);
			}
		}

		public SourceControlInformation SourceControlInformation
		{
			get;
			set;
		}

		public AssemblyInfo AssemblyInfo
		{
			get
			{
				if (this.assemblyInfo == null)
				{
					CodeFile codeFile = this.All<CodeFile>().FirstOrDefault((CodeFile item) => item.Include.EndsWith("AssemblyInfo.cs"));
					if (codeFile != null)
					{
						this.assemblyInfo = new AssemblyInfo(codeFile, this);
						this._projectItemCache.Add("AssemblyInfo+" + codeFile.Include, this.assemblyInfo);
					}
				}
				return this.assemblyInfo;
			}
		}

		public TargetFrameworkVersion TargetFrameworkVersion
		{
			get
			{
				return this.BuildProject.GetGlobalPropertyGroup().GetPropertyValue("TargetFrameworkVersion");
			}
			set
			{
				this.BuildProject.GetGlobalPropertyGroup().SetPropertyValue("TargetFrameworkVersion", value, false);
			}
		}

		public string Platform
		{
			get
			{
				return this.BuildProject.GetGlobalPropertyGroup().GetPropertyValue("Platform");
			}
			set
			{
				this.BuildProject.GetGlobalPropertyGroup().SetPropertyValue("Platform", value, false);
			}
		}

		public CsProjFile(string fileName) : this(fileName, MSBuildProject.LoadFrom(fileName))
		{
		}

		public CsProjFile(string fileName, MSBuildProject project)
		{
			this._fileName = fileName;
			this._project = project;
		}

		public void Add<T>(T item) where T : ProjectItem
		{
			MSBuildItemGroup arg_58_0;
			if ((arg_58_0 = this._project.FindGroup(new Func<MSBuildItem, bool>(item.Matches))) == null)
			{
				arg_58_0 = (this._project.FindGroup((MSBuildItem x) => x.Name == item.Name) ?? this._project.AddNewItemGroup());
			}
			MSBuildItemGroup group = arg_58_0;
			item.Configure(group);
		}

		public T Add<T>(string include) where T : ProjectItem, new()
		{
			T t = Activator.CreateInstance<T>();
			t.Include = include;
			T item = t;
			this._projectItemCache.Remove(item.Include);
			this._projectItemCache.Add(include, item);
			this.Add<T>(item);
			return item;
		}

		public IEnumerable<T> All<T>() where T : ProjectItem, new()
		{
			T t = Activator.CreateInstance<T>();
			string name = t.Name;
			return (from x in this._project.GetAllItems(new string[]
			{
				name
			})
			orderby x.Include
			select x).Select(delegate(MSBuildItem item)
			{
				T projectItem;
				if (this._projectItemCache.ContainsKey(item.Include))
				{
					projectItem = (T)((object)this._projectItemCache[item.Include]);
				}
				else
				{
					projectItem = Activator.CreateInstance<T>();
					projectItem.Read(item);
					this._projectItemCache.Add(item.Include, projectItem);
				}
				return projectItem;
			});
		}

		public static CsProjFile CreateAtSolutionDirectory(string assemblyName, string directory)
		{
			string fileName = FubuCore.StringExtensions.AppendPath(FubuCore.StringExtensions.AppendPath(directory, new string[]
			{
				assemblyName
			}), new string[]
			{
				assemblyName
			}) + ".csproj";
			MSBuildProject project = MSBuildProject.Create(assemblyName);
			return CsProjFile.CreateCore(project, fileName);
		}

		public static CsProjFile CreateAtLocation(string fileName, string assemblyName)
		{
			return CsProjFile.CreateCore(MSBuildProject.Create(assemblyName), fileName);
		}

		private static CsProjFile CreateCore(MSBuildProject project, string fileName)
		{
			MSBuildPropertyGroup arg_42_0;
			if ((arg_42_0 = project.PropertyGroups.FirstOrDefault((MSBuildPropertyGroup x) => x.Properties.Any((MSBuildProperty p) => p.Name == "ProjectGuid"))) == null)
			{
				arg_42_0 = (project.PropertyGroups.FirstOrDefault<MSBuildPropertyGroup>() ?? project.AddNewPropertyGroup(true));
			}
			MSBuildPropertyGroup group = arg_42_0;
			group.SetPropertyValue("ProjectGuid", Guid.NewGuid().ToString().ToUpper(), true);
			CsProjFile file = new CsProjFile(fileName, project);
			file.AssemblyName = (file.RootNamespace = file.ProjectName);
			return file;
		}

		public static CsProjFile LoadFrom(string filename)
		{
			MSBuildProject project = MSBuildProject.LoadFrom(filename);
			return new CsProjFile(filename, project);
		}

		public void Save()
		{
			this.Save(this._fileName);
		}

		public void Save(string file)
		{
			foreach (KeyValuePair<string, ProjectItem> item in this._projectItemCache)
			{
				item.Value.Save();
			}
			this._project.Save(file);
		}

		public IEnumerable<Guid> ProjectTypes()
		{
			IEnumerable<string> enumerable = from x in this._project.PropertyGroups
			select x.GetPropertyValue("ProjectTypeGuids") into x
			where FubuCore.StringExtensions.IsNotEmpty(x)
			select x;
			if (enumerable.Any<string>())
			{
				foreach (string current in enumerable)
				{
					try
					{
						string[] array = current.Split(new char[]
						{
							';'
						});
						for (int i = 0; i < array.Length; i++)
						{
							string text = array[i];
							yield return Guid.Parse(text.TrimStart(new char[]
							{
								'{'
							}).TrimEnd(new char[]
							{
								'}'
							}));
						}
					}
					finally
					{
					}
				}
			}
			else
			{
				yield return CsProjFile.ClassLibraryType;
			}
			yield break;
		}

		public void CopyFileTo(string source, string relativePath)
		{
			string target = FubuCore.StringExtensions.AppendPath(FubuCore.StringExtensions.ParentDirectory(this._fileName), new string[]
			{
				relativePath
			});
			new FileSystem().Copy(source, target);
		}

		public T Find<T>(string include) where T : ProjectItem, new()
		{
			return this.All<T>().FirstOrDefault((T x) => x.Include == include);
		}

		public string PathTo(CodeFile codeFile)
		{
			string path = codeFile.Include;
			if (FubuCore.Platform.IsUnix())
			{
				path = path.Replace('\\', Path.DirectorySeparatorChar);
			}
			return FubuCore.StringExtensions.AppendPath(FubuCore.StringExtensions.ParentDirectory(this._fileName), new string[]
			{
				path
			});
		}

		public void Remove<T>(string include) where T : ProjectItem, new()
		{
			T t = Activator.CreateInstance<T>();
			string name = t.Name;
			this._projectItemCache.Remove(include);
			MSBuildItem element = this._project.GetAllItems(new string[]
			{
				name
			}).FirstOrDefault((MSBuildItem x) => x.Include == include);
			if (element != null)
			{
				element.Remove();
			}
		}

		public void Remove<T>(T item) where T : ProjectItem, new()
		{
			this._projectItemCache.Remove(item.Include);
			MSBuildItem element = this._project.GetAllItems(new string[]
			{
				item.Name
			}).FirstOrDefault((MSBuildItem x) => x.Include == item.Include);
			if (element != null)
			{
				element.Remove();
			}
		}

		public override string ToString()
		{
			return string.Format("{0}", this.FileName);
		}
	}
}
