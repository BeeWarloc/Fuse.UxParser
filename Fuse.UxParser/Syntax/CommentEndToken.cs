namespace Fuse.UxParser.Syntax
{
	public class CommentEndToken : SyntaxToken
	{
		public CommentEndToken(TriviaSyntax leadingTrivia, TriviaSyntax trailingTrivia) : base(
			leadingTrivia,
			trailingTrivia) { }

		public override string Text => "-->";
	}
}