using System.IO;

namespace Fuse.UxParser.Syntax
{
	public abstract class AttributeSyntax : AttributeSyntaxBase
	{
		AttributeSyntax(NameToken name, AttributeLiteralToken literal) : base(name)
		{
			Literal = literal;
		}

		public static AttributeSyntax Create(NameToken name, EqualsToken eq, AttributeLiteralToken literal)
		{
			if (eq.Equals(EqualsToken.Default))
				return new AttributeSyntaxWithDefaultEqToken(name, literal);
			return new AttributeSyntaxWithCustomEqToken(name, eq, literal);
		}

		[NodeChild(1)]
		public abstract EqualsToken Eq { get; }

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

		class AttributeSyntaxWithDefaultEqToken : AttributeSyntax
		{
			public AttributeSyntaxWithDefaultEqToken(NameToken name, AttributeLiteralToken literal) : base(name, literal) { }
			public override EqualsToken Eq => EqualsToken.Default;
		}

		class AttributeSyntaxWithCustomEqToken : AttributeSyntax
		{
			public AttributeSyntaxWithCustomEqToken(NameToken name, EqualsToken eq, AttributeLiteralToken literal) : base(
				name,
				literal)
			{
				Eq = eq;
			}

			public override EqualsToken Eq { get; }
		}

		public override AttributeSyntaxBase With(string newName = null, string newValue = null)
		{
			if ((newValue == null || Value == newValue) && (newName == null || Name.Text == newName))
				return this;
			return Create(Name.With(newName), Eq, Literal.With(newValue));
		}

		public AttributeSyntax WithTrivia(TriviaSyntax? leadingTrivia, TriviaSyntax? trailingTrivia)
		{
			var leading = leadingTrivia ?? LeadingTrivia;
			var trailing = trailingTrivia ?? TrailingTrivia;
			if (leading == LeadingTrivia && trailing == TrailingTrivia)
				return this;

			return Create(Name.WithTrivia(leadingTrivia), Eq, Literal.WithTrivia(trailingTrivia: trailing));
		}
	}
}