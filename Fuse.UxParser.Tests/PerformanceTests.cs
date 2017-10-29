using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Fuse.UxParser.Syntax;
using NUnit.Framework;

namespace Fuse.UxParser.Tests
{
	[TestFixture]
	public class PerformanceTests
	{
		[Explicit]
		[Test]
		public void Measure_performance_parsing_all_ux_files()
		{
			var allFiles = UxTestCases.ExampleDocsAndFuseSamples.Select(x => (string) x.Arguments[0]).ToList();
			const int iterationsPerFile = 100;
			Stopwatch sw = new Stopwatch();
			sw.Start();
			foreach (var f in allFiles)
				for (int i = 0; i < iterationsPerFile; i++)
					SyntaxParser.ParseDocument(f);
			sw.Stop();

			var totalSize = allFiles.Select(x => Encoding.UTF8.GetByteCount(x)).Sum();
			Console.WriteLine(
				"Parse speed {0}MiB/s",
				totalSize * iterationsPerFile / (double) (1 << 20) / sw.Elapsed.TotalSeconds);
		}
	}
}