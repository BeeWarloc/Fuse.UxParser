using System;
using Fuse.UxParser.Syntax;
using NUnit.Framework;

namespace Fuse.UxParser.Tests.Syntax
{
	[TestFixture]
	public class NormalizeVisitorTests
	{
		[Test]
		public void Visit_works()
		{
			// NOCOMMIT! DON'T KNOW IF THIS TEST WILL SURVIVE
			var normalizeVisitor = new NormalizeVisitor();
			var node = SyntaxParser.ParseDocument("<Funky    Foo = \"gagaga&x32;\"   Bar  =\t\t'\"'  />");
			Console.WriteLine("Before: {0}", node);
			Console.WriteLine("After: {0}", normalizeVisitor.Visit(node));

			Assert.Fail("Test NOT finished");
		}
	}
}