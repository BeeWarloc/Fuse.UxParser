using System.IO;

namespace Fuse.UxParser.Syntax
{
	public class AttributeSyntax : AttributeSyntaxBase
	{
		public AttributeSyntax(NameToken name, EqualsToken eq, AttributeLiteralToken literal) : base(name)
		{
			Eq = eq;
			Literal = literal;
		}

		[NodeChild(1)]
		public EqualsToken Eq { get; }

		[NodeChild(2)]
		public AttributeLiteralToken Literal { get; }

		public override string Value => Literal.UnescapedValue;

		public override TriviaSyntax LeadingTrivia => Name.LeadingTrivia;

		public override TriviaSyntax TrailingTrivia => Literal.TrailingTrivia;

		public override int FullSpan => Name.FullSpan + Eq.FullSpan + Literal.FullSpan;

		public override void Write(TextWriter writer)
		{
			Name.Write(writer);
			Eq.Write(writer);
			Literal.Write(writer);
		}


		public override AttributeSyntaxBase With(string newName = null, string newValue = null)
		{
			if ((newValue == null || Value == newValue) && (newName == null || Name.Text == newName))
				return this;
			return new AttributeSyntax(Name.With(newName), Eq, Literal.With(newValue));
		}

		public AttributeSyntax WithTrivia(TriviaSyntax? leadingTrivia, TriviaSyntax? trailingTrivia)
		{
			var leading = leadingTrivia ?? LeadingTrivia;
			var trailing = trailingTrivia ?? TrailingTrivia;
			if (leading == LeadingTrivia && trailing == TrailingTrivia)
				return this;

			return new AttributeSyntax(Name.WithTrivia(leadingTrivia), Eq, Literal.WithTrivia(trailingTrivia: trailing));
		}
	}
}