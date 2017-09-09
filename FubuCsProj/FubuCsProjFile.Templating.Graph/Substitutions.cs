using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Fubu.CsProjFile.FubuCsProjFile.Templating.Runtime;
using FubuCore;
using FubuCore.Util;

namespace Fubu.CsProjFile.FubuCsProjFile.Templating.Graph
{
	public class Substitutions
	{
		public static readonly string ConfigFile = "fubu.templates.config";

		private readonly Cache<string, string> _values = new Cache<string, string>();

		public void SetIfNone(string key, string value)
		{
			this._values.Fill(key, value);
		}

		public void Set(string key, string value)
		{
			this._values.Fill(key, value);
		}

		public string ValueFor(string key)
		{
			return this._values[key];
		}

		public void ReadFrom(string file)
		{
			new FileSystem().ReadTextFile(file, delegate(string line)
			{
				if (FubuCore.StringExtensions.IsEmpty(line))
				{
					return;
				}
				string[] parts = line.Split(new char[]
				{
					'='
				});
				this.SetIfNone(parts.First<string>(), parts.Last<string>());
			});
		}

		public void WriteTo(string file)
		{
			FileSystemExtensions.WriteToFlatFile(new FileSystem(), file, delegate(IFlatFileWriter writer)
			{
				this._values.Each(delegate(string key, string value)
				{
					if (key != "%INSTRUCTIONS%")
					{
						writer.WriteProperty(key, value);
					}
				});
			});
		}

		public bool Has(string key)
		{
			return this._values.Has(key);
		}

		public string ApplySubstitutions(string rawText, Action<StringBuilder> moreAlteration = null)
		{
			StringBuilder builder = new StringBuilder(rawText);
			if (moreAlteration != null)
			{
				moreAlteration(builder);
			}
			this.ApplySubstitutions(builder);
			return builder.ToString();
		}

		public void ApplySubstitutions(StringBuilder builder)
		{
			this._values.Each(delegate(string key, string value)
			{
				builder.Replace(key, value);
			});
		}

		public void CopyTo(Substitutions substitutions2)
		{
			this._values.Each(new Action<string, string>(substitutions2.Set));
		}

		public void ReadInputs(IEnumerable<Input> inputs, Action<string> markMissing)
		{
			GenericEnumerableExtensions.Each<Input>(inputs, delegate(Input x)
			{
				if (!this._values.Has(x.Name) && FubuCore.StringExtensions.IsEmpty(x.Default))
				{
					markMissing(x.Name);
				}
				string resolved = this.ApplySubstitutions(x.Default ?? string.Empty, null);
				this.SetIfNone(x.Name, resolved);
			});
		}

		public void Trace(ITemplateLogger logger)
		{
			this._values.Each(delegate(string key, string value)
			{
				if (key != "%INSTRUCTIONS%")
				{
					logger.Trace("{0}={1}", new object[]
					{
						key,
						value
					});
				}
			});
		}
	}
}
