using System.Linq;
using NUnit.Framework;

namespace Fuse.UxParser.Tests
{
	[TestFixture]
	public class UxNodePathTests
	{
		static UxDocument ParseDoc(string ux)
		{
			var doc = UxDocument.Parse(ux);
			return doc;
		}

		[TestCase("<X/>", "/0")]
		[TestCase("<A><X/></A>", "/0/0")]
		[TestCase("<A><B/><X/></A>", "/0/1")]
		[TestCase("<A><!--   --><B/><X/></A>", "/0/2")]
		[Test]
		public void From_element(string xml, string expected)
		{
			var node = ParseDoc(xml).DescendantNodes().OfType<UxElement>().Single(x => x.Name == "X");
			Assert.That(node.NodePath.ToString(), Is.EqualTo(expected));
		}

		[TestCase("/0")]
		[TestCase("/1/7/4")]
		[Test]
		public void Parse_root_returns_correct_path(string path)
		{
			Assert.That(UxNodePath.Parse(path).ToString(), Is.EqualTo(path));
		}
	}
}