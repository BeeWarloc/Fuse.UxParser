namespace Fuse.UxParser.Syntax
{
	public class LessThanToken : SyntaxToken
	{
		class MissingLessThanToken : LessThanToken
		{
			public MissingLessThanToken() : base(TriviaSyntax.Empty, TriviaSyntax.Empty) { }

			public override string Text => string.Empty;

			public override bool IsMissing => true;
		}

		LessThanToken(TriviaSyntax leadingTrivia, TriviaSyntax trailingTrivia)
			: base(leadingTrivia, trailingTrivia) { }

		public static LessThanToken Create(TriviaSyntax leadingTrivia, TriviaSyntax trailingTrivia)
		{
			if (leadingTrivia.IsEmpty && trailingTrivia.IsEmpty)
				return Default;
			return new LessThanToken(leadingTrivia, trailingTrivia);
		}

		public static LessThanToken Default { get; } = new LessThanToken(TriviaSyntax.Empty, TriviaSyntax.Empty);
		public static LessThanToken Missing { get; } = new MissingLessThanToken();

		public override string Text => "<";
	}
}