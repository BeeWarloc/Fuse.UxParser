using System.Linq;
using Fuse.UxParser.Syntax;
using NUnit.Framework;

namespace Fuse.UxParser.Tests
{
	[TestFixture]
	public class UxElementTests
	{
		static UxElement ParseElement(string initial)
		{
			return (UxElement) UxNode.FromSyntax(SyntaxParser.ParseNode(initial));
		}

		static UxElement FindX(UxElement root)
		{
			return root.DescendantsAndSelf().Single(el => el.Name == "X");
		}

		[TestCase("<Root><Nana /><X/></Root>", "<FooBar />", "<Root><Nana /><X><FooBar /></X></Root>")]
		[TestCase("<Root><X/></Root>", "FooBar", "<Root><X>FooBar</X></Root>")]
		[TestCase("<Root><X></X></Root>", "<A /><B /><C />", "<Root><X><A /><B /><C /></X></Root>")]
		[TestCase("<X/>", "FooBar", "<X>FooBar</X>")]
		[Test]
		public void Add_nodes_to_element(string initial, string added, string expected)
		{
			var root = ParseElement(initial);
			var x = FindX(root);
			x.AddRawUx(added);
			Assert.That(root.ToString(), Is.EqualTo(expected));
		}

		[TestCase("<Root><Nana /><X/></Root>", "<Root><Nana /></Root>")]
		[Test]
		public void Remove_element_from_element(string initial, string expected)
		{
			var root = ParseElement(initial);
			var x = FindX(root);
			Assert.That(x.Remove(), Is.True);
			Assert.That(root.ToString(), Is.EqualTo(expected));
		}

		[Test]
		public void SetAttributeValue_to_null_when_attribute_exists_removes_attribute()
		{
			var root = ParseElement("<A Foo=\"What\"/>");
			root.SetAttributeValue("Foo", null);
			Assert.That(root.ToString(), Is.EqualTo("<A/>"));
		}

		[Test]
		public void SetAttributeValue_when_attribute_dont_exist_adds_attribute()
		{
			var root = ParseElement("<A/>");
			root.SetAttributeValue("Foo", "Bar");
			Assert.That(root.ToString(), Is.EqualTo("<A Foo=\"Bar\"/>"));
		}

		[Test]
		public void SetAttributeValue_when_attribute_exists_alters_value()
		{
			var root = ParseElement("<A Foo=\"QYDXS\"/>");
			root.SetAttributeValue("Foo", "Bar");
			Assert.That(root.ToString(), Is.EqualTo("<A Foo=\"Bar\"/>"));
		}

		[Test]
		public void SetAttributeValue_when_attribute_exists_with_single_quoted_value_alters_value_and_keeps_quotation()
		{
			var root = ParseElement("<A Foo='QYDXS'/>");
			root.SetAttributeValue("Foo", "Bar'\"");
			Assert.That(root.ToString(), Is.EqualTo("<A Foo='Bar&apos;\"'/>"));
		}


		[Test]
		public void SetAttributeValue_when_attribute_exists_with_unquoted_value_alters_value_and_keeps_it_unquoted()
		{
			var root = ParseElement("<A Foo=QYDXS/>");
			root.SetAttributeValue("Foo", "Bar");
			Assert.That(root.ToString(), Is.EqualTo("<A Foo=Bar/>"));
		}

		[Test]
		public void SetAttributeValue_when_existing_attribute_is_implicit_uses_double_quotes()
		{
			var root = ParseElement("<A Foo/>");
			root.SetAttributeValue("Foo", "Bar");
			Assert.That(root.ToString(), Is.EqualTo("<A Foo=\"Bar\"/>"));
		}
	}
}