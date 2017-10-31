using System.Collections.Immutable;

namespace Fuse.UxParser.Syntax
{
	public class NormalizeVisitor : SyntaxVisitor
	{
		AttributeSyntax NormalizeAttribute(AttributeSyntax syntax, bool isLast)
		{
			return AttributeSyntax.Create(
				syntax.Name.WithTrivia(TriviaSyntax.Space, TriviaSyntax.Empty),
				syntax.Eq.WithTrivia(TriviaSyntax.Empty, TriviaSyntax.Empty),
				syntax.Literal.With(literalKind: AttributeLiteralKind.DoubleQuoted)
					.WithTrivia(TriviaSyntax.Empty, TriviaSyntax.Empty));
		}

		protected override SyntaxBase VisitEmptyElement(EmptyElementSyntax syntax)
		{
			var attributes = VisitAttributes(syntax.Attributes);
			return EmptyElementSyntax.Create(
				LessThanToken.Create(TriviaSyntax.Empty, TriviaSyntax.Empty),
				syntax.Name.WithTrivia(TriviaSyntax.Empty, TriviaSyntax.Empty),
				attributes,
				SlashToken.Create(TriviaSyntax.Empty, TriviaSyntax.Empty),
				GreaterThanToken.Create(TriviaSyntax.Empty, TriviaSyntax.Empty));
		}

		protected override SyntaxBase VisitElementStartTag(ElementStartTagSyntax syntax)
		{
			var attributes = VisitAttributes(syntax.Attributes);
			return ElementStartTagSyntax.Create(
				LessThanToken.Create(TriviaSyntax.Empty, TriviaSyntax.Empty),
				syntax.Name.WithTrivia(TriviaSyntax.Empty, TriviaSyntax.Empty),
				attributes,
				GreaterThanToken.Create(TriviaSyntax.Empty, TriviaSyntax.Empty));
		}

		IImmutableList<AttributeSyntaxBase> VisitAttributes(IImmutableList<AttributeSyntaxBase> attributes)
		{
			var builder = ImmutableList.CreateBuilder<AttributeSyntaxBase>();
			for (var i = 0; i < attributes.Count; i++)
			{
				var attrBase = attributes[i];
				switch (attrBase)
				{
					case AttributeSyntax attr:
						builder.Add(NormalizeAttribute(attr, i == attributes.Count - 1));
						break;
					default:
						// TODO
						builder.Add(attrBase);
						break;
				}
			}
			return builder.ToImmutable();
		}
	}
}