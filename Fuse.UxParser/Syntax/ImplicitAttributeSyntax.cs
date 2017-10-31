using System;
using System.IO;

namespace Fuse.UxParser.Syntax
{
	public class ImplicitAttributeSyntax : AttributeSyntaxBase
	{
		ImplicitAttributeSyntax(NameToken name) : base(name) { }

		public static ImplicitAttributeSyntax Create(NameToken name)
		{
			return new ImplicitAttributeSyntax(name);
		}

		public override TriviaSyntax LeadingTrivia => Name.LeadingTrivia;
		public override TriviaSyntax TrailingTrivia => Name.TrailingTrivia;

		public override int FullSpan => Name.FullSpan;

		public override string Value => Name.Text;

		public override void Write(TextWriter writer)
		{
			Name.Write(writer);
		}

		public override AttributeSyntaxBase With(string newName, string newValue)
		{
			throw new NotImplementedException();
		}
	}
}