using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Fuse.UxParser.Syntax;
using NUnit.Framework;

namespace Fuse.UxParser.Tests
{
	// Not really tests I guess..

	[TestFixture]
	public class MemoryOverheadTests
	{
		// Easier to debug when not iterating many times
		static int MeasureIterations { get; } = Debugger.IsAttached ? 1 : 8;

		[Test]
		public void Measure_ux_tree_converted_to_xobject_overhead()
		{
			MeasureTree(x => SyntaxParser.ParseDocument(x).ToXml(), 10);
		}

		[Test]
		public void Measure_in_memory_size_of_syntax_tree()
		{
			MeasureTree(SyntaxParser.ParseDocument, 20);
		}

		[Test]
		public void Measure_in_memory_size_of_fully_expanded_UxDocument_tree()
		{
			MeasureTree(ParseAndExpandUxDocument, 24);
		}

		void MeasureTree<T>(Func<string, T> treeConstructor, double maximumAcceptedRatio)
		{
			var ratios = new List<double>();
			int totalFileSize = 0;
			var totalFileCount = 0;
			foreach (var tc in UxTestCases.ExampleDocsAndFuseSamples)
			{
				var input = (string) tc.Arguments[0];
				var size = MemBenchmark(() => input, treeConstructor, MeasureIterations);
				var fileSize = Encoding.UTF8.GetByteCount(input);
				totalFileSize += fileSize;
				var ratio = size / fileSize;
				ratios.Add(ratio);
				totalFileCount++;
			}
			ratios.Sort();
			var ratioMean = ratios.Count % 2 ==
				0
					? (ratios[ratios.Count / 2] + ratios[ratios.Count / 2 + 1]) / 2.0
					: ratios[ratios.Count / 2];
			var ratiosAvg = ratios.Average();
			Console.WriteLine("(tree size)/(file size) ratio is {0} avg {1} median", ratiosAvg, ratioMean);
			const double mib = (double) (1 << 20);
			Console.WriteLine(
				"The total size of all {0} example UX docs is {1:0.####}MiB, taking up {2:0.####}MiB in memory",
				totalFileCount,
				totalFileSize / mib,
				totalFileSize * ratioMean / mib);
			Assert.That(
				ratiosAvg,
				Is.LessThan(maximumAcceptedRatio),
				"Memory overhead ratio has grown to be higher than " + maximumAcceptedRatio);
		}

		static UxDocument ParseAndExpandUxDocument(string uxText)
		{
			var doc = UxDocument.Parse(uxText);
			// ReSharper disable once UnusedVariable
			foreach (var node in doc.Root.DescendantNodesAndSelf())
			{
				// Do nothing here
			}
			return doc;
		}

		double MemBenchmark<TInit, T>(Func<TInit> initFunc, Func<TInit, T> func, int iterations = 1024)
		{
			var instances = new List<T>(iterations + 1) { func(initFunc()) };
			var initials = Enumerable.Range(0, iterations).Select(_ => initFunc()).ToList();
			var preMemory = GC.GetTotalMemory(true);
			for (int i = 0; i < iterations; i++)
				instances.Add(func(initials[i]));
			var memIncrease = (GC.GetTotalMemory(true) - preMemory) / (double) iterations;
			//Console.WriteLine("Size of each instance is around " + memIncrease);
			Assert.That(instances, Is.Not.Null); // Just to pin instances until measure is complete
			return memIncrease;
		}
	}
}