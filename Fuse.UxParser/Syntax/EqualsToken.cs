namespace Fuse.UxParser.Syntax
{
	public class EqualsToken : SyntaxToken
	{
		EqualsToken(TriviaSyntax leadingTrivia, TriviaSyntax trailingTrivia)
			: base(leadingTrivia, trailingTrivia) { }

		public static EqualsToken Default { get; } = new EqualsToken(TriviaSyntax.Empty, TriviaSyntax.Empty);

		public override string Text => "=";

		public static EqualsToken Create(TriviaSyntax leadingTrivia, TriviaSyntax trailingTrivia)
		{
			if (leadingTrivia.IsEmpty && trailingTrivia.IsEmpty)
				return Default;
			return new EqualsToken(leadingTrivia, trailingTrivia);
		}


		public EqualsToken WithTrivia(TriviaSyntax? leadingTrivia, TriviaSyntax? trailingTrivia)
		{
			var leading = leadingTrivia ?? LeadingTrivia;
			var trailing = trailingTrivia ?? TrailingTrivia;

			if (leading == LeadingTrivia && trailing == TrailingTrivia)
				return this;

			return new EqualsToken(leading, trailing);
		}
	}
}