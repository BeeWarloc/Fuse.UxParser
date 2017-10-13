namespace Fuse.UxParser.Syntax
{
	public class CommentEndToken : SyntaxToken
	{
		public CommentEndToken(TriviaSyntax leadingTrivia, TriviaSyntax trailingTrivia) : base(
			leadingTrivia,
			trailingTrivia) { }

		public override string Text => "-->";

		public static bool TryParseCommentEndToken(Scanner scanner, out CommentEndToken token)
		{
			return SyntaxParser.TryParseString(
				scanner,
				out token,
				"-->",
				(leadingWs, trailingWs) => new CommentEndToken(leadingWs, trailingWs),
				false);
		}
	}
}