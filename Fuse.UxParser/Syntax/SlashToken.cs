namespace Fuse.UxParser.Syntax
{
	public class SlashToken : SyntaxToken
	{
		public SlashToken(TriviaSyntax leadingTrivia, TriviaSyntax trailingTrivia) : base(
			leadingTrivia,
			trailingTrivia) { }

		public static SlashToken Default { get; } = new SlashToken(TriviaSyntax.Empty, TriviaSyntax.Empty);

		public override string Text => "/";
	}
}