namespace Fuse.UxParser.Syntax
{
	public class EncodedTextToken : SyntaxToken
	{
		public EncodedTextToken(string text) : base(TriviaSyntax.Empty, TriviaSyntax.Empty)
		{
			Text = text;
		}

		public override string Text { get; }
	}
}