using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Fubu.CsProjFile.FubuCsProjFile
{
	public static class StringExtensions
    {
		private static readonly string[] Splitters = new string[]
		{
			"\r\n",
			"\n"
		};

		public static string[] SplitOnNewLine(this string value)
		{
			return value.Split(StringExtensions.Splitters, StringSplitOptions.None);
		}

		public static string CanonicalPath(this string path)
		{
			return FubuCore.StringExtensions.ToFullPath(path).ToLower().Replace("\\", "/");
		}

		public static bool ContainsSequence(this List<string> list, IEnumerable<string> lines)
		{
			int index = list.IndexOf(lines.First<string>());
			if (index == -1)
			{
				return false;
			}
			if (lines.Count<string>() == 1)
			{
				return true;
			}
			int i = 0;
			foreach (string line in lines)
			{
				if (list.Count <= index + i)
				{
					bool result = false;
					return result;
				}
				if (list[index + i] != line)
				{
					bool result = false;
					return result;
				}
				i++;
			}
			return true;
		}

		public static string ExtractVersion(this string source)
		{
			StringBuilder result = new StringBuilder();
			for (int i = 0; i < source.Length; i++)
			{
				char value = source[i];
				if (!char.IsDigit(value) && value != '.' && result.Length > 0)
				{
					break;
				}
				if (char.IsDigit(value) || (value == '.' && result.Length > 0))
				{
					result.Append(value);
				}
			}
			return result.ToString().TrimEnd(new char[]
			{
				'.'
			});
		}

		public static bool Contains(this string source, string value, StringComparison comparison)
		{
			switch (comparison)
			{
			case StringComparison.CurrentCultureIgnoreCase:
			case StringComparison.InvariantCultureIgnoreCase:
			case StringComparison.OrdinalIgnoreCase:
				return source != null && value != null && source.ToLower().Contains(value.ToLower());
			}
			return source.Contains(value);
		}
	}
}
