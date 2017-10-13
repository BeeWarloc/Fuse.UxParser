namespace Fuse.UxParser.Syntax
{
	public class LessThanToken : SyntaxToken
	{
		public LessThanToken(TriviaSyntax leadingTrivia, TriviaSyntax trailingTrivia)
			: base(leadingTrivia, trailingTrivia) { }

		public static LessThanToken Default { get; } = new LessThanToken(TriviaSyntax.Empty, TriviaSyntax.Empty);

		public override string Text => "<";
	}
}