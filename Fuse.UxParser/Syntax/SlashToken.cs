namespace Fuse.UxParser.Syntax
{
	public class SlashToken : SyntaxToken
	{
		class MissingSlashToken : SlashToken
		{
			public MissingSlashToken() : base(TriviaSyntax.Empty, TriviaSyntax.Empty) { }

			public override string Text => string.Empty;
			public override bool IsMissing => true;
		}

		SlashToken(TriviaSyntax leadingTrivia, TriviaSyntax trailingTrivia) : base(
			leadingTrivia,
			trailingTrivia) { }

		public static SlashToken Create(TriviaSyntax leadingTrivia, TriviaSyntax trailingTrivia)
		{
			if (leadingTrivia.IsEmpty && trailingTrivia.IsEmpty)
				return Default;
			return new SlashToken(leadingTrivia, trailingTrivia);
		}

		public static SlashToken Default { get; } = new SlashToken(TriviaSyntax.Empty, TriviaSyntax.Empty);
		public static SlashToken Missing { get; } = new MissingSlashToken();

		public override string Text => "/";
	}
}