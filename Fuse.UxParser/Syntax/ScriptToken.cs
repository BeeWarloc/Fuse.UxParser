namespace Fuse.UxParser.Syntax
{
	public class ScriptToken : SyntaxToken
	{
		public ScriptToken(string text) : base(TriviaSyntax.Empty, TriviaSyntax.Empty)
		{
			Text = text;
		}

		public override string Text { get; }
	}
}