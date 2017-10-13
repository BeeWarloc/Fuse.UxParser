using System.IO;
using System.Reflection;
using System.Text;
using Fuse.UxParser.Syntax;
using NUnit.Framework;

namespace Fuse.UxParser.Tests.Syntax
{
	[TestFixture]
	public class SyntaxParserTests
	{
		[TestCase("<Foo />", "<Boo />", false)]
		[TestCase("<Foo></Foo>", "<Boo></Boo>", false)]
		public void Equals_test(string aux, string bux, bool expected)
		{
			var a = SyntaxParser.ParseNode(aux);
			var b = SyntaxParser.ParseNode(bux);
			Assert.That(a.Equals(b), Is.EqualTo(expected));
			Assert.That(b.Equals(a), Is.EqualTo(expected));
			Assert.That(a.GetHashCode() == b.GetHashCode(), Is.EqualTo(expected));
		}

		[TestCase("  &amp;", "  &")]
		[TestCase("  &quot;", "  \"")]
		[TestCase("  &gt;", "  >")]
		[TestCase("", "")]
		[TestCase("abc", "abc")]
		[TestCase("  &amp;&amp;&amp;", "  &&&")]
		[TestCase("  &amp;&amp;&amp;", "  &&&")]
		[TestCase("abc&x3B4;xyz", "abcδxyz")]
		[TestCase("abc&#948;xyz", "abcδxyz")]
		[Test]
		public void Decode_text(string input, string expected)
		{
			Assert.That(UxTextEncoding.Unescape(input), Is.EqualTo(expected));
		}

		[TestCase("<Donkey />")]
		[TestCase("<Root>Some text</Root>")]
		[TestCase("<Root SingleQuote=\"'\" DoubleQuote='\"' />")]
		[TestCase("<Root><!-- A comment with some ignored <tag> inside --></Root>")]
		[TestCase("<Root><![CDATA[  <Random stuff s/s ad/,ksd ;.sd>,>>>S /s/S?D>A?>/ ]]></Root>")]
		[TestCase("<Boom Mama='Monkey'  Bassss=\"skjsklj\" >      <im-so:Empty/>   <Foo  ></Foo ></Boom>")]
		[Test]
		public void Parse_roundtrip(string input)
		{
			Assert.That(SyntaxParser.TryParseDocumentSyntax(new Scanner(input), out var syntax), Is.True);
			Assert.That(syntax.ToString(), Is.EqualTo(input));
			Assert.That(syntax.FullSpan, Is.EqualTo(input.Length));
			Assert.That(string.Concat(syntax.AllTokens), Is.EqualTo(input));

			// Check that equals works
			SyntaxParser.TryParseDocumentSyntax(new Scanner(input), out var syntaxReparsed);
			Assert.That(syntax, Is.EqualTo(syntaxReparsed));
		}


		[Test]
		[TestCaseSource(typeof(UxTestCases), nameof(UxTestCases.ExampleDocsAndFuseSamples))]
		public void Parse_roundtrip_from_all_ux_files_in_directory(string input)
		{
			Assert.That(
				SyntaxParser.TryParseDocumentSyntax(new Scanner(input), out var syntax),
				Is.True,
				"Unable to parse:\r\n" + input);
			Assert.That(syntax.ToString(), Is.EqualTo(input));
			Assert.That(syntax.FullSpan, Is.EqualTo(input.Length));
			Assert.That(string.Concat(syntax.AllTokens), Is.EqualTo(input));
		}

		[TestCase("Examples.Ex1.ux")]
		[Test]
		public void Parse_roundtrip_from_file(string resourceName)
		{
			string input;
			using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(GetType(), resourceName))
			using (var reader = new StreamReader(stream, Encoding.UTF8))
			{
				input = reader.ReadToEnd();
			}
			Assert.That(SyntaxParser.TryParseElementSyntax(new Scanner(input), out var syntax), Is.True);
			Assert.That(syntax.ToString(), Is.EqualTo(input));
			Assert.That(syntax.FullSpan, Is.EqualTo(input.Length));
			Assert.That(string.Concat(syntax.AllTokens), Is.EqualTo(input));
		}
	}
}