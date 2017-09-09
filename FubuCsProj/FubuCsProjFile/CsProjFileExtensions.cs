namespace Fubu.CsProjFile.FubuCsProjFile
{
	public static class CsProjFileExtensions
	{
		public static string TextBetweenSquiggles(this string text)
		{
			int start = text.IndexOf("{");
			int end = text.IndexOf("}");
			return text.Substring(start + 1, end - start - 1);
		}

		public static string TextBetweenQuotes(this string text)
		{
			return text.Trim().TrimStart(new char[]
			{
				'"'
			}).TrimEnd(new char[]
			{
				'"'
			});
		}
	}
}
