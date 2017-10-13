using NUnit.Framework;

namespace Fuse.UxParser.Tests
{
	[TestFixture]
	public class UxNodeTests
	{
		[Test]
		[TestCaseSource(typeof(UxTestCases), nameof(UxTestCases.ExampleDocsAndFuseSamples))]
		public void SourceOffset_is_correct_for_all_nodes_in_example(string uxContent)
		{
			var doc = UxDocument.Parse(uxContent);
			foreach (var node in doc.DescendantNodes())
			{
				var expected = node.Syntax.ToString();
				var actual = uxContent.Substring(node.SourceOffset, node.Syntax.FullSpan);
				Assert.That(node.FullSpan, Is.EqualTo(expected.Length));
				Assert.That(actual, Is.EqualTo(expected));
			}
		}
	}
}