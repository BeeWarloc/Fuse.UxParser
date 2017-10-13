namespace Fuse.UxParser.Syntax
{
	public class EqualsToken : SyntaxToken
	{
		public EqualsToken(TriviaSyntax leadingTrivia, TriviaSyntax trailingTrivia)
			: base(leadingTrivia, trailingTrivia) { }

		public static EqualsToken Default { get; } = new EqualsToken(TriviaSyntax.Empty, TriviaSyntax.Empty);

		public override string Text => "=";

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