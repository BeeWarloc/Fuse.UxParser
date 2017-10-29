namespace Fuse.UxParser.Syntax
{
	public class GreaterThanToken : SyntaxToken
	{
		class MissingGreaterThanToken : GreaterThanToken
		{
			public MissingGreaterThanToken() : base(TriviaSyntax.Empty, TriviaSyntax.Empty) { }

			public override string Text => string.Empty;
			public override bool IsMissing => true;
		}

		GreaterThanToken(TriviaSyntax leadingTrivia, TriviaSyntax trailingTrivia) : base(
			leadingTrivia,
			trailingTrivia) { }

		public static GreaterThanToken Create(TriviaSyntax leadingTrivia, TriviaSyntax trailingTrivia)
		{
			if (leadingTrivia.IsEmpty && trailingTrivia.IsEmpty)
				return Default;
			return new GreaterThanToken(leadingTrivia, trailingTrivia);
		}

		public static GreaterThanToken Default { get; } = new GreaterThanToken(TriviaSyntax.Empty, TriviaSyntax.Empty);
		public static GreaterThanToken Missing { get; } = new MissingGreaterThanToken();

		public override string Text => ">";
	}
}