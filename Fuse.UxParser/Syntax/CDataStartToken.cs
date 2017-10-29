namespace Fuse.UxParser.Syntax
{
	public class CDataStartToken : SyntaxToken
	{
		public CDataStartToken(TriviaSyntax leadingTrivia, TriviaSyntax trailingTrivia) : base(
			leadingTrivia,
			trailingTrivia) { }

		public override string Text => "<![CDATA[";
	}
}