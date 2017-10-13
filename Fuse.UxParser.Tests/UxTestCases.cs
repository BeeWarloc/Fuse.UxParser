using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace Fuse.UxParser.Tests
{
	public static class UxTestCases
	{
		public static IEnumerable<TestCaseData> ExampleDocsAndFuseSamples =>
			ExampleDocs().Concat(FuseSamples()).OrderBy(x => (x.Arguments?[0] as string)?.Length);

		static string SolutionDirectory
		{
			get
			{
				var dir = Path.GetDirectoryName(new Uri(typeof(UxTestCases).Assembly.CodeBase, UriKind.Absolute).AbsolutePath);
				while (!Directory.Exists(Path.Combine(dir, ".git")))
					dir = Path.GetDirectoryName(dir);
				return dir;
			}
		}

		public static IEnumerable<TestCaseData> ExampleDocs()
		{
			return GetTestCaseDataForAllUxDocsRecursively(Path.Combine(SolutionDirectory, "..", "example-docs"));
		}

		public static IEnumerable<TestCaseData> FuseSamples()
		{
			return GetTestCaseDataForAllUxDocsRecursively(@"c:\src\fuse-samples");
		}

		static IEnumerable<TestCaseData> GetTestCaseDataForAllUxDocsRecursively(string directory)
		{
			directory = Path.GetFullPath(directory);
			if (!Directory.Exists(directory))
				throw new DirectoryNotFoundException($"Unable to locate ux example dir {directory}");

			foreach (var fn in Directory.GetFiles(directory, "*.ux", SearchOption.AllDirectories))
			{
				var str = File.ReadAllText(fn);
				yield return new TestCaseData(str) { TestName = $"len{str.Length:D5}{fn.Substring(directory.Length)}" };
			}
		}
	}
}