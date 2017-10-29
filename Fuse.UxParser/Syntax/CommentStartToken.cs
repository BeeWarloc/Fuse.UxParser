namespace Fuse.UxParser.Syntax
{
	public class CommentStartToken : SyntaxToken
	{
		public CommentStartToken(TriviaSyntax leadingTrivia, TriviaSyntax trailingTrivia) : base(
			leadingTrivia,
			trailingTrivia) { }

		public override string Text => "<!--";
	}
}