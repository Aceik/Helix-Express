using System;
using System.Diagnostics;
using FubuCore.CommandLine;

namespace Fubu.CsProjFile.FubuCsProjFile.Templating.Runtime
{
	public class TemplateLogger : ITemplateLogger
	{
		private int _indention;

		private int _stepCount;

		private readonly Stopwatch _stopwatch = new Stopwatch();

		private int _numberOfSteps;

		private int _numberOfAlterations;

		private int _alterationNumber;

		public void Starting(int numberOfSteps)
		{
			this._numberOfSteps = numberOfSteps;
			this._stopwatch.Start();
		}

		public void TraceStep(ITemplateStep step)
		{
			this._stepCount++;
			string text = string.Concat(new object[]
			{
				this._stepCount.ToString().PadLeft(3),
				"/",
				this._numberOfSteps,
				": ",
				step.ToString()
			});
			ConsoleWriter.WriteWithIndent(ConsoleColor.White, 0, text);
			this._indention = 4;
		}

		public void Trace(string contents, params object[] parameters)
		{
			ConsoleWriter.WriteWithIndent(ConsoleColor.White, this._indention, FubuCore.StringExtensions.ToFormat(contents, parameters));
		}

		public void StartProject(int numberOfAlterations)
		{
			this._alterationNumber = 0;
			this._numberOfAlterations = numberOfAlterations;
			this._indention = 4;
		}

		public void EndProject()
		{
			this._indention = 4;
		}

		public void TraceAlteration(string alteration)
		{
			this._alterationNumber++;
			string text = string.Concat(new object[]
			{
				this._alterationNumber.ToString().PadLeft(3),
				"/",
				this._numberOfAlterations,
				": ",
				alteration
			});
			ConsoleWriter.WriteWithIndent(ConsoleColor.Gray, this._indention, text);
		}

		public void Finish()
		{
			this._stopwatch.Stop();
			ConsoleWriter.Write(ConsoleColor.Green, FubuCore.StringExtensions.ToFormat("Templating successful in {0} ms", new object[]
			{
				this._stopwatch.ElapsedMilliseconds
			}));
		}

		public void WriteSuccess(string message)
		{
			ConsoleWriter.WriteWithIndent(ConsoleColor.Green, this._indention, message);
		}

		public void WriteWarning(string message)
		{
			ConsoleWriter.WriteWithIndent(ConsoleColor.Yellow, this._indention, message);
		}
	}
}
