namespace Fuse.UxParser.Syntax
{
	public class CDataEndToken : SyntaxToken
	{
		public CDataEndToken(TriviaSyntax leadingTrivia, TriviaSyntax trailingTrivia) : base(
			leadingTrivia,
			trailingTrivia) { }

		public override string Text => "]]>";
	}
}