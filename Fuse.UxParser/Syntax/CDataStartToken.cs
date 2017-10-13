namespace Fuse.UxParser.Syntax
{
	public class CDataStartToken : SyntaxToken
	{
		public CDataStartToken(TriviaSyntax leadingTrivia, TriviaSyntax trailingTrivia) : base(
			leadingTrivia,
			trailingTrivia) { }

		public override string Text => "<![CDATA[";

		public static bool TryParseCDataStartToken(Scanner scanner, out CDataStartToken token)
		{
			return SyntaxParser.TryParseString(
				scanner,
				out token,
				"<![CDATA[",
				(leadingWs, trailingWs) => new CDataStartToken(leadingWs, trailingWs),
				false);
		}
	}
}