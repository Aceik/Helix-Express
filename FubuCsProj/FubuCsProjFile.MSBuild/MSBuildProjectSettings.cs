namespace Fubu.CsProjFile.FubuCsProjFile.MSBuild
{
	public class MSBuildProjectSettings
	{
		public bool MaintainOriginalItemOrder
		{
			get;
			set;
		}

		public bool OnlySaveIfChanged
		{
			get;
			set;
		}

		public static MSBuildProjectSettings DefaultSettings
		{
			get
			{
				return new MSBuildProjectSettings
				{
					MaintainOriginalItemOrder = false
				};
			}
		}

		public static MSBuildProjectSettings MinimizeChanges
		{
			get
			{
				return new MSBuildProjectSettings
				{
					MaintainOriginalItemOrder = true,
					OnlySaveIfChanged = true
				};
			}
		}
	}
}
