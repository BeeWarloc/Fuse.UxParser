namespace Fuse.UxParser.Syntax
{
	public class CommentStartToken : SyntaxToken
	{
		public CommentStartToken(TriviaSyntax leadingTrivia, TriviaSyntax trailingTrivia) : base(
			leadingTrivia,
			trailingTrivia) { }

		public override string Text => "<!--";

		public static bool TryParseCommentStartToken(Scanner scanner, out CommentStartToken token)
		{
			return SyntaxParser.TryParseString(
				scanner,
				out token,
				"<!--",
				(leadingWs, trailingWs) => new CommentStartToken(leadingWs, trailingWs),
				false);
		}
	}
}