namespace Fuse.UxParser.Syntax
{
	public class GreaterThanToken : SyntaxToken
	{
		public GreaterThanToken(TriviaSyntax leadingTrivia, TriviaSyntax trailingTrivia) : base(
			leadingTrivia,
			trailingTrivia) { }

		public static GreaterThanToken Default { get; } = new GreaterThanToken(TriviaSyntax.Empty, TriviaSyntax.Empty);

		public override string Text => ">";
	}
}