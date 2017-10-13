using System.Linq;
using System.Xml.Linq;
using NUnit.Framework;
using XmlExperimentation;

namespace XmlPatchStreams.Tests
{
	[TestFixture]
	public class XmlNodePathTests
	{
		[TestCase("<X/>", "/")]
		[TestCase("<A><X/></A>", "/0")]
		[TestCase("<A><B/><X/></A>", "/1")]
		[TestCase("<A><!--   --><B/><X/></A>", "/2")]
		[Test]
		public void From_element(string xml, string expected)
		{
			var node = ParseDoc(xml).DescendantNodes().OfType<XElement>().Single(x => x.Name == "X");
			Assert.That(node.NodePath().ToString(), Is.EqualTo(expected));
		}

		[TestCase("<A/>", "/0", "<X/>", "<A><X/></A>")]
		[TestCase("<A><B/></A>", "/0/0", "<X/>", "<A><B><X/></B></A>")]
		[TestCase("<A>  <B/></A>", "/1/0", "<X/>", "<A>  <B><X/></B></A>")]
		//          / (0) (1) 2/(0) ↓1
		[TestCase("<A><B/>   <B><D/><E/></B></A>", "/2/1", "<X/>", "<A><B/>   <B><D/><X/><E/></B></A>")]
		[TestCase("<A/>", "/0", "some text", "<A>some text</A>")]
		[Test]
		public void TryInsertAt_is_successful(string before, string nodePath, string insertedFragment, string expectedAfter)
		{
			var beforeDoc = ParseDoc(before);
			var expectedAfterDoc = ParseDoc(expectedAfter);
			var path = XmlNodePath.Parse(nodePath);
			var inserted = ParseDoc(string.Format("<r>{0}</r>", insertedFragment)).Root.Nodes().Single();
			inserted.Remove();
			Assert.That(beforeDoc.TryInsert(path, inserted), Is.True);
			Assert.That(beforeDoc.DeepEquals(expectedAfterDoc), Is.True);
		}

		[TestCase("<A/>", "/0", "<X/>", "<A><X/></A>")]
		[TestCase("<A><B/></A>", "/0/0", "<X/>", "<A><B><X/></B></A>")]
		[TestCase("<A>  <B/></A>", "/1/0", "<X/>", "<A>  <B><X/></B></A>")]
		//          / (0) (1) 2/(0) ↓1
		[TestCase("<A><B/>   <B><D/><E/></B></A>", "/2/1", "<X/>", "<A><B/>   <B><D/><X/><E/></B></A>")]
		[TestCase("<A/>", "/0", "some text", "<A>some text</A>")]
		[Test]
		public void TryRemoveAt_is_successful(string expectedAfter, string nodePath, string removedFragment, string before)
		{
			var beforeDoc = ParseDoc(before);
			var expectedAfterDoc = ParseDoc(expectedAfter);
			var path = XmlNodePath.Parse(nodePath);
			var removedCompareNode = ParseDoc(string.Format("<r>{0}</r>", removedFragment)).Root.Nodes().Single();
			removedCompareNode.Remove();
			Assert.That(beforeDoc.TryRemove(path, removedCompareNode), Is.True);
			Assert.That(beforeDoc.DeepEquals(expectedAfterDoc), Is.True);
		}

		static XDocument ParseDoc(string xml)
		{
			var doc = XDocument.Parse(xml, LoadOptions.PreserveWhitespace | LoadOptions.SetLineInfo);
			return doc;
		}
	}
}